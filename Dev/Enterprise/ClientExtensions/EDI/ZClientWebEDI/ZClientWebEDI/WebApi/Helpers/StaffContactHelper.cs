using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class StaffContactHelper
	{
		public StaffContactHelper(string secureQueryStringContent)
		{
			SecureQueryString queryString;
			try
			{
				queryString = new SecureQueryString(secureQueryStringContent);
				this.licenceCode = queryString[StaffContactValueObjectHelper.QueryStringKeys.LicenceCode];
				this.databaseNumberAsText = queryString[StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber];
				this.contactData = queryString[StaffContactValueObjectHelper.QueryStringKeys.ContactData];
				this.staffCode = queryString[StaffContactValueObjectHelper.QueryStringKeys.StaffCode];
				this.branchCode = queryString[StaffContactValueObjectHelper.QueryStringKeys.HomeBranchCode];
			}
			catch (QueryStringException)
			{
			}

			Init();
		}

		public StaffContactHelper(string licenceCode, string databaseNumberAsText, string contactData)
		{
			this.licenceCode = licenceCode;
			this.databaseNumberAsText = databaseNumberAsText;
			this.contactData = contactData;
			Init();
		}

		readonly string licenceCode;
		readonly string databaseNumberAsText;
		readonly string contactData;
		readonly string staffCode;
		readonly string branchCode;
		LicenceDatabase database;
		string enterpriseCode;
		string companyCode;
		string serverCode;

		public LicenceDatabase Database => database;
		public ZString LicenceCode => licenceCode;
		public ZString BranchCode => branchCode;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory() { NameForDebugging = nameof(StaffContactHelper), RefreshEnabled = false }); }
		}
		BusinessObjectFactory factory;

		void Init()
		{
			if (licenceCode != null && licenceCode.Length == 9)
			{
				enterpriseCode = licenceCode.Substring(0, 3);
				companyCode = licenceCode.Substring(3, 3);
				serverCode = licenceCode.Substring(6, 3);
			}

			if (databaseNumberAsText != null && int.TryParse(databaseNumberAsText, out int databaseNumber))
			{
				database = Factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, databaseNumber));
			}
			else if (enterpriseCode != null && serverCode != null)
			{
				database = LicenceDatabase.LoadFromEnterpriseAndServerCode(Factory, enterpriseCode, serverCode);
			}
		}

		public ContactImportResult FindOrCreateContact()
		{
			if (database == null)
			{
				return new ContactImportResult(null, null, Res.GetString("ca88dce7-00b5-45e8-b0fb-45d2f0ca7543", "No Database could be found"));
			}

			WebRequestContactImporter importer = null;
			if (string.IsNullOrEmpty(staffCode) && !string.IsNullOrEmpty(companyCode))
			{
				var companyQuery = new ZDBOnlySubQuery(typeof(ClientCompany), ClientCompanySchema.LCC_OH);
				companyQuery.AddToFilter(ClientCompanySchema.LCC_Code, companyCode);
				companyQuery.AddToFilter(ClientCompanySchema.LCC_LD, database.PK);
				var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				orgQuery.AddSubQuery(companyQuery, JoinCondition.And);
				var org = Factory.LoadTop1<OrgHeader>(orgQuery);

				if (org != null)
				{
					importer = new WebRequestContactImporter(Factory, database, org);
				}
			}

			if (importer == null)
			{
				importer = new WebRequestContactImporter(Factory, database);
			}

			var xsdContact = ValueObjectEncoder.Deserialize<Xsd.OrgContact>(contactData);
			var result = importer.ImportFromContactXsd(xsdContact, staffCode, branchCode);
			var contact = result.Contact;

			if (contact != null)
			{
				var isNewCreatedContact = !contact.IsInDatabase;
				var isEnableWebAccess = contact.OC_WebAccessEnabled && contact.OC_WebAccessEnabledInfo.HasChanges;
				if (isNewCreatedContact || contact.HasChanges)
				{
					importer.SaveIfNeeded();
					if (isNewCreatedContact || isEnableWebAccess)
					{
						importer.MergeContactWebSecurity(contact);
					}
				}

				if (!string.IsNullOrEmpty(staffCode))
				{
					var ediContact = contact as EDIOrgContact;
					if (ediContact != null)
					{
						ediContact.LogMyAccountDisclaimerAcknowledgementIfRequired(database);
						ediContact.Factory.Save();
					}
				}
			}
			else
			{
				var userAccount = result.UserAccount;
				if (userAccount != null && (!userAccount.IsInDatabase || userAccount.HasChanges))
				{
					importer.SaveIfNeeded();
				}
			}

			return result;
		}
	}
}
