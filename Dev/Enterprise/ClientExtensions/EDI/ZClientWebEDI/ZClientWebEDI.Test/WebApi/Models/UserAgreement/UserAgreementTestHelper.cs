using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class UserAgreementTestHelper
	{
		public EdiUserAgreement SetupAgreement(BusinessObjectFactory factory, string type, string countryCode, ZDateTime effectiveTimeUtc)
		{
			var agreement = factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = type;
			agreement.ERA_Title = "Enterprise User Agreement";
			agreement.ERA_Content = "Enterprise User Agreement Content";
			agreement.ERA_RN_NKCountryCode = countryCode;
			agreement.ERA_EffectiveTimeUtc = effectiveTimeUtc;
			return agreement;
		}

		public void SetupCustomerUserAccount(BusinessObjectFactory factory)
		{
			var org = factory.New<OrgHeader>();
			org.OH_FullName = "some org";
			org.OH_Code = "code";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "batman@gotham.city";

			var applicant = factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "batman@gotham.city";
			applicant.HA_FullName = "batman";

			factory.Save();

			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = "name";
			staff.GS_Code = "BAT";
			staff.GS_PER = contact.OC_PER;
			staff.GS_LoginName = "batman";

			LicenceEnterprise = factory.New<LicenceEnterprise>();
			LicenceEnterprise.LE_OH = org.PK;
			LicenceEnterprise.LE_EnterpriseCode = "BLA";
			LicenceDatabase = factory.New<LicenceDatabase>();
			LicenceDatabase.LD_LE = LicenceEnterprise.PK;
			LicenceDatabase.LD_DatabaseNumber = 0;
			LicenceDatabase.LD_ServerCode = "123";
			LicenceDatabase.LD_Product = "CW1";
			LicenceDatabase.LD_TenantID = "12345";
			UserAccount = factory.New<EdiCustomerUserAccount>();
			UserAccount.EUA_OC_WebAccessContact = contact.PK;
			UserAccount.EUA_LD = LicenceDatabase.PK;
			UserAccount.EUA_UserID = staff.GS_Code;
			factory.Save();
		}

		public void DisableEffectiveDateTriggers(DbConnection connection)
		{
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		public void EnableEffectiveDateTriggers(DbConnection connection)
		{
			connection.ExecuteNonQuery("ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; ENABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		public LicenceEnterprise LicenceEnterprise;
		public LicenceDatabase LicenceDatabase;
		public EdiCustomerUserAccount UserAccount;
	}
}
