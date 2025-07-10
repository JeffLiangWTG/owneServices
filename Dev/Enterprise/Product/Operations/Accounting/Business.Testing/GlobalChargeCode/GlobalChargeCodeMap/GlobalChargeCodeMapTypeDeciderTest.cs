using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGlobalChargeCodeMapIntercompanyType()
		{
			GlobalChargeCodeMapIntercompany globalChargeCodeIntercompany = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMap globalChargeCode = Factory.Load<GlobalChargeCodeMap>(globalChargeCodeIntercompany.PK);
			AssertType(typeof(GlobalChargeCodeMapIntercompany), globalChargeCode);
		}

		public void TestGlobalChargeCodeMapOrganizationType()
		{
			GlobalChargeCodeMapOrganization globalChargeCodeOrganization = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCodeOrganization.YG_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			GlobalChargeCodeMap globalChargeCode = Factory.Load<GlobalChargeCodeMap>(globalChargeCodeOrganization.PK);
			AssertType(typeof(GlobalChargeCodeMapOrganization), globalChargeCode);
		}
	}
}