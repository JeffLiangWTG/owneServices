using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	[TestedType(typeof(RomaniaRefreshAccessTokenServiceTask))]
	public class RomaniaRefreshAccessTokenServiceTaskTest : ServiceTaskTestCase<RomaniaRefreshAccessTokenServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestFailedCreationOfEDIInterchange_NoClientCredentials()
		{
			SetUpIProductRegistration();
			using (AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingTestCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EInvoicingCredentials()))
			{
				var serviceTask = new RomaniaRefreshAccessTokenServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				var errorMessage = "Error|Client Id or Client Secret is missing. Please check registry: Accounting -> E-Reporting and E-Invoicing Configurations -> Romania (RO) -> Test E-Invoicing Credentials (CargoWiseOne Support Only). Romania E-Invoice Refresh Access Token terminated.";
				TestFailedCreationOfEDIInterchange(errorMessage);
			}
		}

		public void TestFailedCreationOfEDIInterchange_NoCompany()
		{
			foreach (var company in GlbCompany.GetActiveCompanies())
			{
				company.GC_IsActive = false;
				company.Factory.Save();
			}

			var errorMessage = "There is no branch that belongs to a Romanian company and is in active status. Romania E-Invoice Refresh Access Token terminated.";
			TestFailedCreationOfEDIInterchange(errorMessage);
		}

		public void TestFailedCreationOfEDIInterchange_SingleCompanyWithoutCredential()
		{
			CreateRomanianCompanyWithoutCredential("RO1", "B01");
			Factory.Save();

			var errorMessage = "Information|Romania E-Invoice Refresh Access Token started.\r\nInformation|There is no Romanian Company that needs to refresh E-Invoicing Credentials. Romania E-Invoice Refresh Access Token terminated.\r\n";
			TestFailedCreationOfEDIInterchange(errorMessage);
		}

		public void TestSuccessfulCreationOfEDIInterchange_SingleCompany()
		{
			CreateRomanianCompanyWithCredential("RO1", "B01");
			Factory.Save();

			TestSuccessfulCreationOfEDIInterchange(new List<string> { "RRORO1AAA" });
		}

		public void TestSuccessfulCreationOfEDIInterchange_MultiCompanies()
		{
			CreateRomanianCompanyWithCredential("RO1", "B01");
			CreateRomanianCompanyWithCredential("RO2", "B02");
			Factory.Save();

			TestSuccessfulCreationOfEDIInterchange(new List<string> { "RRORO1AAA", "RRORO2AAA" });
		}

		public void TestSuccessfulCreationOfEDIInterchange_OneCompanyWithoutCredential()
		{
			CreateRomanianCompanyWithCredential("RO1", "B01");
			CreateRomanianCompanyWithoutCredential("RO2", "B02");
			CreateRomanianCompanyWithCredential("RO3", "B03");
			Factory.Save();

			TestSuccessfulCreationOfEDIInterchange(new List<string> { "RRORO1AAA", "RRORO3AAA" });
		}

		public void TestSuccessfulCreationOfEDIInterchange_TwoCompaniesWithoutCredential()
		{
			CreateRomanianCompanyWithoutCredential("RO1", "B01");
			CreateRomanianCompanyWithoutCredential("RO2", "B02");
			CreateRomanianCompanyWithCredential("RO3", "B03");
			Factory.Save();

			TestSuccessfulCreationOfEDIInterchange(new List<string> { "RRORO3AAA" });
		}

		public void TestSuccessfulCreationOfEDIInterchange_OneCompanyWithOtherPasswordType()
		{
			CreateRomanianCompanyWithCredential("RO1", "B01", PasswordTypesList.Codes.EBD);
			CreateRomanianCompanyWithCredential("RO2", "B02");
			Factory.Save();

			TestSuccessfulCreationOfEDIInterchange(new List<string> { "RRORO2AAA" });
		}

		public void TestHostedServiceTimeProperties()
		{
			var attributes = typeof(RomaniaRefreshAccessTokenServiceTask).Assembly.GetCustomAttributes<HostedServiceAttribute>();
			var attributeForRO = attributes.First(x => x.Code == RomaniaEInvoiceAPICommandList.Codes.RomaniaInvoiceRefreshAccessToken);
			AssertEquals("12hours", attributeForRO.DefaultScheduleRunEvery);
			AssertEquals("1hour", attributeForRO.MinimumPeriod);
			AssertEquals("48hours", attributeForRO.MaximumPeriod);
		}

		void TestFailedCreationOfEDIInterchange(string loggerMessage)
		{
			var serviceTask = new RomaniaRefreshAccessTokenServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertContains(loggerMessage, logger.ToString());

			var ediInterchangeCollection = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			var ediMessageCollection = Factory.Load<IXmlEDIMessage>(new ZQuery());
			AssertEquals("There should be no edi interchange added", 0, ediInterchangeCollection.Length);
			AssertEquals("There should be no edi message added", 0, ediMessageCollection.Length);
		}

		void TestSuccessfulCreationOfEDIInterchange(List<string> expectedLicenseCodes)
		{
			var testCredentials = new EInvoicingCredentials
			{
				ClientId = "0819a6689265e8db236e6e37c6237e8a7e3ee71dc75c6765",
				ClientSecret = "9c5ae51d64866e513c873c1b7d5d085fa66d49ea9f407e8a7e3ee71dc75c6765"
			};

			SetUpIProductRegistration();
			var firstActiveRomanianBranch = GlbBranch.GetFirstActiveBranch(CountryCodes.Romania);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, firstActiveRomanianBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.RomaniaEInvoicingTestCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCredentials))
			{
				var serviceTask = new RomaniaRefreshAccessTokenServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				var info = logger.ToString();

				AssertEquals($"expect log counts: 3", 3, logger.Count);

				var ediInterchange = Factory.LoadTop1<IXmlEDIInterchange>(new ZQuery());
				EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchange, expectedMessageType: "RRO", expectedTo: "RO_JWT_RefreshToken");

				var ediMessages = ediInterchange.LoadMessages();
				AssertEquals("expect 1 messages created", 1, ediMessages.Length);

				AssertEDIMessage(ediMessages[0]);

				AssertEDIMessageText(ediMessages[0].EM_MessageText, testCredentials, expectedLicenseCodes);
			}
		}

		void AssertEDIMessage(XmlEDIMessage message)
		{
			AssertEquals("EM_IsActive", ZBool.True, message.EM_IsActive);
			AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
			AssertEquals("EM_ApplicationCode", "GEI", message.EM_ApplicationCode);
			AssertEquals("EM_MessageType","RRO", message.EM_MessageType);
			AssertEquals("EM_MessageSubType", "RRO", message.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "SNT", message.EM_Status);
			AssertEquals("EM_GE", GlbDepartment.CurrentDepartment.PK, message.EM_GE);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
		}

		void AssertEDIMessageText(string messageText, EInvoicingCredentials expectedCredentials, List<string> expectedLicenseCodes)
		{
			var doc = XDocument.Parse(messageText);
			XNamespace ns = "http://www.wisetechglobal.com/Schemas/Configuration";

			var messageType = doc.Descendants(ns + "Item")
				.FirstOrDefault(item => item.Attribute("Name")?.Value == "MessageType")?.Value ?? string.Empty;

			var encrytedClientId = doc.Descendants(ns + "Item")
				.FirstOrDefault(item => item.Attribute("Name")?.Value == "EncryptedCilentId")?.Value ?? string.Empty;

			var encrytedClientSecret = doc.Descendants(ns + "Item")
				.FirstOrDefault(item => item.Attribute("Name")?.Value == "EncryptedCilentSecret")?.Value ?? string.Empty;

			var licenseCodes = doc.Descendants(ns + "Item")
				.Where(item => item.Attribute("Name")?.Value == "LicenseCode")
				.Select(item => item.Value)
				.ToList();

			var aesEncryptionKey = ObjectFactory.Get<IAccounting>().RSADecrypt(AccountingMasterFilesRegistry.Instance.RomaniaEncryptionKey.Value);
			var aesCrypto = new AESCrypto(HashAlgorithmName.SHA256);
			var clientId = aesCrypto.DecryptStringAES(encrytedClientId, aesEncryptionKey);
			var clientSecret = aesCrypto.DecryptStringAES(encrytedClientSecret, aesEncryptionKey);

			AssertEquals("MessageType should be RRO.", "RRO", messageType);
			AssertEquals("Client Id should be equal.", expectedCredentials.ClientId, clientId);
			AssertEquals("Client Secret should be equal.", expectedCredentials.ClientSecret, clientSecret);
			AssertContainsExactElementsInAnyOrder("License codes should be the identical.", expectedLicenseCodes, licenseCodes);
		}

		void SetUpIProductRegistration()
		{
			var productRegistrationKey = new Mock<IProductRegistrationKey>();
			productRegistrationKey
				.Setup(x => x.EnterpriseCode)
				.Returns("RRO");
			productRegistrationKey
				.Setup(x => x.ServerCode)
				.Returns("AAA");

			var productRegistration = new Mock<IProductRegistration>();
			productRegistration
				.Setup(x => x.Key)
				.Returns(productRegistrationKey.Object);
			productRegistration
				.Setup(x => x.IsWiseTechGlobalInternalUATSystem())
				.Returns(true);

			ObjectFactory.Substitute(productRegistration.Object);
		}

		GlbCompany CreateRomanianCompanyWithCredential(string companyCode, string branchCode, string passwordType = PasswordTypesList.Codes.EIM)
		{
			var company = TestObjectCreator.CreateNewCompany(companyCode, CountryCodes.Romania);
			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(passwordType);
			TestObjectCreator.CreateNewBranch(company, branchCode);

			return company;
		}

		GlbCompany CreateRomanianCompanyWithoutCredential(string companyCode, string branchCode)
		{
			var company = TestObjectCreator.CreateNewCompany(companyCode, CountryCodes.Romania);
			TestObjectCreator.CreateNewBranch(company, branchCode);

			return company;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory, true));
		TestObjectCreator testObjectCreator;
	}
}
