using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Accounting.ElectronicMessaging.Romania;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;
using Logger = Enterprise.Accounting.ElectronicMessaging.Common.Logger;

[assembly: HostedService(
	RomaniaRefreshAccessTokenServiceTask.ServiceTaskCode,
	"Romania E-Invoice Refresh Access Token",
	"ACC",
	typeof(RomaniaRefreshAccessTokenServiceTask),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Romania,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "1hour",
	MaximumPeriod = "48hours",
	DefaultScheduleRunEvery = "12hours")
]
namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("RomaniaRefreshAccessTokenServiceTask is under implementation.")]
	public class RomaniaRefreshAccessTokenServiceTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "RRO";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Romania E-Invoice Refresh Access Token started.");

			var romaniaComplianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCodes.Romania) as RomaniaComplianceInfo;
			var eInvoicingCredentialsRegistry = romaniaComplianceInfo.GetEInvoicingCredentialsRegistryItem();
			var credentials = eInvoicingCredentialsRegistry.Value;
			if (credentials.ClientId.IsEmpty || credentials.ClientSecret.IsEmpty)
			{
				ServiceLogger.Log(LogType.Error, $"Client Id or Client Secret is missing. Please check registry: {eInvoicingCredentialsRegistry.Location()}. Romania E-Invoice Refresh Access Token terminated.");
				return;
			}

			var activeRomanianCompanies = GlbCompany.GetActiveCompanies(CountryCodes.Romania);
			var firstActiveRomanianBranch = activeRomanianCompanies.FirstOrDefault()?.FirstActiveBranch;
			if (firstActiveRomanianBranch == null)
			{
				ServiceLogger.Log(LogType.Error, "There is no branch that belongs to a Romanian company and is in active status. Romania E-Invoice Refresh Access Token terminated.");
				return;
			}

			var licenseCodes = GetAvailableLicenseCodes(activeRomanianCompanies);
			if (!licenseCodes.Any())
			{
				ServiceLogger.Log(LogType.Information, "There is no Romanian Company that needs to refresh E-Invoicing Credentials. Romania E-Invoice Refresh Access Token terminated.");
				return;
			}

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Will generate Romania E-Invoice Refresh Access Token message for {0} Company(s): {1}", licenseCodes.Count(), string.Join(",", licenseCodes)));

			EncryptClientIdAndSecret(credentials, out string encryptedClientId, out string encryptedClientSecret);
			var xmlMessage = GenerateRefreshMessage(encryptedClientId, encryptedClientSecret, licenseCodes);

			using (DisposableEnvironment.ForBranch(firstActiveRomanianBranch.PK.ToGuid()))
			using (var disposeManager = new DisposableManager())
			using (var messageStream = (SubStreamableStream)new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(xmlMessage)))
			{
				var factory = new BusinessObjectFactory();
				factory.AddDisposableService(disposeManager);

				var deliver = new EHubDelivery();
				var result = deliver.Deliver(GetDeliveryContext(factory), GetMode(), new DeliveryStreamWrapperUXML(messageStream));

				factory.Save();
			}

			ServiceLogger.Log(LogType.Information, "Romania E-Invoice Refresh Access Token completed");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element name")]
		string GenerateRefreshMessage(string encryptedClientId, string encryptedClientSecret, IEnumerable<string> licenseCodes)
		{
			XNamespace ns = "http://www.wisetechglobal.com/Schemas/Configuration";
			var root = new XElement(ns + "Configuration",
				new XAttribute("Name", "RomaniaEInvoicingTokenConfiguration"),
				new XAttribute("Version", "1.0")
			);

			var group = new XElement(ns + "Group", new XAttribute("Type", "System"),
				new XElement(ns + "Item", new XAttribute("Name", "MessageType"), RomaniaEInvoiceAPICommandList.Codes.RomaniaInvoiceRefreshAccessToken),
				new XElement(ns + "Item", new XAttribute("Name", "EncryptedCilentId"), encryptedClientId),
				new XElement(ns + "Item", new XAttribute("Name", "EncryptedCilentSecret"), encryptedClientSecret)
			);

			foreach (var code in licenseCodes)
			{
				group.Add(new XElement(ns + "Item", new XAttribute("Name", "LicenseCode"), code));
			}

			root.Add(group);

			return new XDocument(root).ToString();
		}

		IEnumerable<string> GetAvailableLicenseCodes(GlbCompany[] activeRomanianCompanies)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var licenseCodes = FilterCompaniesWithCredential(activeRomanianCompanies)
				.Select(company => registrationKey.EnterpriseCode + company.GC_Code + registrationKey.ServerCode)
				.ToList();

			return licenseCodes;
		}

		IEnumerable<GlbCompany> FilterCompaniesWithCredential(GlbCompany[] companies)
		{
			var companyIds = companies.Select(company => company.PK);
			var query = new ZDBOnlyQuery(typeof(GlbCompany));
			query.AddToFilter(GlbCompanySchema.PK, companyIds);

			var subQuery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GC);
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var companiesWithCredential = companies[0].Factory.Load<GlbCompany>(query);

			return companiesWithCredential;
		}

		void EncryptClientIdAndSecret(EInvoicingCredentials credential, out string encryptedClientId, out string encryptedClientSecret)
		{
			var aesEncryptionKey = ObjectFactory.Get<IAccounting>().RSADecrypt(AccountingMasterFilesRegistry.Instance.RomaniaEncryptionKey.Value);
			var aesCrypto = new AESCrypto(HashAlgorithmName.SHA256);
			encryptedClientId = aesCrypto.EncryptStringAES(credential.ClientId, aesEncryptionKey);
			encryptedClientSecret = aesCrypto.EncryptStringAES(credential.ClientSecret, aesEncryptionKey);
		}

		DeliveryContext GetDeliveryContext(BusinessObjectFactory factory)
		{
			return new DeliveryContext(factory)
			{
				PurposeCode = Purpose,
				ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice,
				MessageTypeCode = RomaniaEInvoiceAPICommandList.Codes.RomaniaInvoiceRefreshAccessToken,
				MessageSubTypeCode = RomaniaEInvoiceAPICommandList.Codes.RomaniaInvoiceRefreshAccessToken,
				Notifications = new Logger()
			};
		}

		IEDICommunicationsMode GetMode()
		{
			return new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				EK_Destination = EInvoicingServicePoint,
				EK_MessagePurpose = Purpose
			};
		}

		const string Purpose = "";
		const string EInvoicingServicePoint = "RO_JWT_RefreshToken";
	}
}
