namespace Enterprise.Client.EDI.Registry.Business.Test
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;

	public class DocMyAccountAmbiguousLoginTest : TestCaseWithFactory
	{
		public void TestCompanyList()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var contact3 = Factory.NewWithValidTestData<OrgContact>();

			contact1.Header.OH_Code = "APPLE";
			contact2.Header.OH_Code = "BANANA";
			contact3.Header.OH_Code = "ORANGE";
			contact1.Header.MainAddress.OA_CompanyNameOverride = "is a fruit";
			contact2.Header.MainAddress.OA_CompanyNameOverride = "is yellow";
			contact3.Header.MainAddress.OA_CompanyNameOverride = "cures scurvy";

			var loginBizO = new MyAccountAmbiguousLogin(new[] { contact1, contact2, contact3 });

			var docWrapper = new DocMyAccountAmbiguousLogin(loginBizO, Factory);
			AssertEquals("APPLE - is a fruit<br>BANANA - is yellow<br>ORANGE - cures scurvy", docWrapper.CompanyList);
		}
	}
}
