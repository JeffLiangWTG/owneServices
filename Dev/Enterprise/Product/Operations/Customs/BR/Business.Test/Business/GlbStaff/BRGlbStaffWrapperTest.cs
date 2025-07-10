using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(BRGlbStaffWrapper))]
	public class BRGlbStaffWrapperTest : GlbStaffWrapperTest<BRGlbStaffWrapper>
	{
		protected override BRGlbStaffWrapper CreateNewWrapper(GlbStaff staff)
		{
			return BRGlbStaffWrapper.Get(staff);
		}

		public void TestIGlbStaffWrapperMembers()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = BRGlbStaffWrapper.Get(staff);
			BRGlbStaffWrapper iWrapper = wrapper;
			AssertEquals(wrapper.EventSubscriptions, iWrapper.EventSubscriptions);
		}

		public void TestBRSPasswordCollection()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "DK@";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "DK1";
			var wrapper = BRGlbStaffWrapper.Get(staff1);
			var password1 = wrapper.EventSubscriptions.AddNew();
			password1.GP_GS = staff1.PK;
			password1.GP_UserID = "1";
			password1.GP_GC = company1.PK;
			password1.GP_StatusReason = "Reason1";
			password1.GP_MailBoxID = "Mail1";
			var password2 = wrapper.EventSubscriptions.AddNew();
			password2.GP_GS = staff1.PK;
			password2.GP_UserID = "2";
			password2.GP_GC = company1.PK;
			password2.GP_StatusReason = "Reason2";
			password2.GP_MailBoxID = "Mail2";
			var password3 = wrapper.EventSubscriptions.AddNew();
			password3.GP_GS = staff1.PK;
			password3.GP_UserID = "3";
			password3.GP_GC = GlbCompany.CurrentCompany.PK;
			password3.GP_StatusReason = "Reason3";
			password3.GP_MailBoxID = "Mail3";
			Factory.Save();
			AssertEquals(3, wrapper.EventSubscriptions.Count);
			AssertCollectionContains(password1, wrapper.EventSubscriptions);
			AssertCollectionContains(password2, wrapper.EventSubscriptions);
			AssertCollectionContains(password3, wrapper.EventSubscriptions);
		}

		public void TestGetCCTPassword()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "ABC";
			var password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.GP_PasswordType = PasswordTypesList.Codes.CCT;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			Factory.Save();

			var cctPassword = BRGlbStaffWrapper.Get(newStaff).GetCCTPassword();

			AssertEquals("GP_PK should be", password.PK, cctPassword.PK);
			AssertEquals("GP_PasswordType should be", PasswordTypesList.Codes.CCT, cctPassword.GP_PasswordType);
			AssertEquals("GP_ExpiryDate should be", true, cctPassword.GP_ExpiryDate.IsInTheFuture());
			AssertEquals("GP_PasswordStatus should be", BRPasswordStatusList.Codes.Valid, cctPassword.GP_PasswordStatus);
		}
	}
}
