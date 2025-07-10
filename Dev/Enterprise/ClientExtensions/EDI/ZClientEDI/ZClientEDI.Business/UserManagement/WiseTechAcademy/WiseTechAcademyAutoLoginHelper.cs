using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public static partial class WiseTechAcademyAutoLoginHelper
	{
		public static (EdiCustomerUserAccount userAccount, EDIOrgHeader billingOrg) GetOrCreateCustomerUserAccount(OrgHeader loggedInOrganisation, OrgContact loggedInOrgContact)
		{
			if (loggedInOrganisation == null || loggedInOrgContact == null)
			{
				return (null, null);
			}

			var factory = new BusinessObjectFactory();
			var masterOrg = factory.Load<EDIOrgHeader>(loggedInOrganisation.PK);

			var company = masterOrg.LicCompany;
			if (company == null)
			{
				masterOrg.CreateAndLoadLicenceForOrg();
				company = masterOrg.LicCompany;
			}

			var dbs = company.ActiveOrAllLicDatabases.OfType<LicenceDatabase>();
			var ld = dbs.FirstOrDefault(x => x.LD_Product == ProductTypes.Codes.WiseTechAcademy);

			if (ld == null)
			{
				var currentEnterpriseDatabases = company.LicEnterprise.Databases.OfType<LicenceDatabase>();
				ld = currentEnterpriseDatabases.FirstOrDefault(x => x.LD_Product == ProductTypes.Codes.WiseTechAcademy);
				if (ld != null)
				{
					if (!ld.LD_IsActive)
					{
						ld.LD_IsActive = true;
					}

					var currentLicenceHeader = ld.LicHeadersForAllCompanies.OfType<LicenceHeader>().FirstOrDefault(x => x.LA_LC == company.PK && x.LA_LD == ld.PK);
					if (currentLicenceHeader != null)
					{
						if (!currentLicenceHeader.LA_IsActive)
						{
							currentLicenceHeader.LA_IsActive = true;
						}
					}
					else
					{
						var licenceHeader = ld.ActiveLicHeadersForAllCompanies.AddNew();
						licenceHeader.LA_LD = ld.PK;
						licenceHeader.LA_LC = company.PK;
						licenceHeader.LA_IsActive = true;
					}

					factory.Save();
				}
				else
				{
					var serverCode = LicenceHelper.GenerateServerCode(currentEnterpriseDatabases, ProductTypes.Codes.WiseTechAcademy);
					ld = company.ActiveOrAllLicDatabases.AddNew();
					ld.LD_Product = ProductTypes.Codes.WiseTechAcademy;
					ld.LD_AllowAutoLogin = true;
					ld.LD_LicenceType = DatabaseTypes.Codes.Production;
					ld.LD_Status = DatabaseStatusList.Codes.REG;
					ld.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
					ld.LD_ServerCode = serverCode;
					ld.LD_IsActive = true;
					ld.LD_OH_WebAccessOrg = masterOrg.PK;

					factory.Save();
					ld.LD_TenantID = ld.LD_DatabaseNumber.ToString();
					factory.Save();
				}
			}

			var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, ld.PK);
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, loggedInOrgContact.PK);

			var userAccount = factory.LoadTop1<EdiCustomerUserAccount>(query);
			if (userAccount == null)
			{
				userAccount = factory.New<EdiCustomerUserAccount>();
				userAccount.EUA_LD = ld.PK;
				userAccount.EUA_UserID = userAccount.PK.ToString();
				userAccount.EUA_FullName = loggedInOrgContact.OC_ContactName;
				userAccount.EUA_Email = loggedInOrgContact.OC_Email;
				userAccount.EUA_IsActive = true;
				userAccount.EUA_OC_WebAccessContact = loggedInOrgContact.PK;
				userAccount.EUA_IsContactRelationshipActive = true;
				userAccount.EUA_SystemVerifiedDateUtc = ZDateTime.UtcNow;
				userAccount.EUA_UserVerifiedDateUtc = ZDateTime.UtcNow;
				userAccount.EUA_IsEmailVerificationRequired = false;
				userAccount.EUA_RN_NKCountry = GetWorkingAddressCountryCode(loggedInOrgContact);
				userAccount.EUA_IsEmailOverridden = false;
				factory.Save();
			}

			var delivery = company.InvoiceDeliveries.FindByServerAndSystem(ld.LD_ServerCode, BillingConstants.BillingSystem.All);
			var billingOrg = delivery?.InvoiceTo ?? masterOrg;

			return (userAccount, billingOrg);
		}

		public static string GetWorkingAddressCountryCode(OrgContact loggedInOrgContact)
		{
			var workingAddressPK = loggedInOrgContact.WorkingAddressPK;
			OrgAddress workingAddress;
			string workingAddressCountryCode;
			if (!workingAddressPK.IsEmpty
				&& (workingAddress = loggedInOrgContact.Factory.Load<OrgAddress>(workingAddressPK)) != null
				&& workingAddress.Country != null)
			{
				workingAddressCountryCode = workingAddress.Country.Code;
			}
			else
			{
				workingAddressCountryCode = loggedInOrgContact.Header.CountryCode;
			}

			return workingAddressCountryCode;
		}
	}
}
