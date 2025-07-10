using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DBLoaderExtesionTest : TestCaseWithFactory
	{
		public void TestFetchStaffExternalPassword()
		{
			var (_, query) = Factory.FetchStaffExternalPassword("YMS");
			AssertEquals("GP_PasswordType = @CWO1_ and GP_CertificateAuthority <> @CWO2_ and GP_UserID <> @CWO3_ and GP_CurrentPassword <> @CWO4_ and (GP_GS IN (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_IsActive = @CWO5_ and GS_Code = @CWO6_))", query.FilterString);
		}

		public void TestFetchStaffExternalPasswordForManager()
		{
			var (_, query) = Factory.FetchStaffExternalPasswordForManager("YMS");
			AssertEquals("GP_PasswordType = @CWO1_ and GP_CertificateAuthority <> @CWO2_ and GP_UserID <> @CWO3_ and GP_CurrentPassword <> @CWO4_ and (GP_GS IN (SELECT GSM_GS_Manager FROM dbo.GlbStaffManager WHERE (GSM_ManagerType = @CWO5_ and GSM_IsApproved = @CWO6_ and GSM_EffectiveDate <= @CWO7_ and (GSM_EndDate is NULL or GSM_EndDate > @CWO8_)) and (GSM_GS_Staff IN (SELECT GSM_GS_Staff FROM dbo.GlbStaff WHERE GS_IsActive = @CWO9_ and GS_Code = @CWO10_)) and (GSM_GS_Staff IN (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_IsActive = @CWO9_ and GS_Code = @CWO10_))))", query.FilterString);
		}
	}
}
