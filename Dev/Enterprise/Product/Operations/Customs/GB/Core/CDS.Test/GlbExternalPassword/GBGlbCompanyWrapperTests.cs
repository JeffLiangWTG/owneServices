using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.GB;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(GBGlbCompanyWrapper))]
	public class GBGlbCompanyWrapperTests : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GBGlbCompanyWrapper>
	{
		public void TestIGlbCompanyWrapperMembers()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var wrapper = GetWrapper(company);
			IGBGlbCompanyWrapper iWrapper = wrapper;
			AssertEquals(wrapper.GBBPasswordCollection, iWrapper.GBBPasswordCollection);
		}

		public void TestGGBPasswordCollection()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CK1";

			var password1 = Factory.NewWithValidTestData<GlbExternalPassword_GB>();
			password1.GP_GC = company.PK;
			password1.GP_UserID = "A.1";

			var password2 = Factory.NewWithValidTestData<GlbExternalPassword_GB>();
			password2.GP_GC = company.PK;
			password2.GP_UserID = "A.2";

			var password3 = Factory.NewWithValidTestData<GlbExternalPassword_GB>();
			password3.GP_GC = company.PK;
			password3.GP_UserID = "A.3";
			password3.GP_GC = company.PK;

			var wrapper = GetWrapper(company);
			AssertEquals(3, wrapper.GBBPasswordCollection.Count);
			AssertCollectionContains(password1, wrapper.GBBPasswordCollection);
			AssertCollectionContains(password2, wrapper.GBBPasswordCollection);
			AssertCollectionContains(password3, wrapper.GBBPasswordCollection);
		}
	}
}
