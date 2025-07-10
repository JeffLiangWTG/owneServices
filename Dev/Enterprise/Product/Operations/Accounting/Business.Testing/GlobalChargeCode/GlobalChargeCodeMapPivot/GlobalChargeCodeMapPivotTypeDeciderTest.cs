using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGlobalChargeCodeMapIntercompanyType()
		{
			GlobalChargeCodeMapIntercompany globalChargeCodeIntercompany = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			ZGuid pivotPK = globalChargeCodeIntercompany.PivotCollection.AddNew().PK;
			GlobalChargeCodeMapPivot globalChargeCodePivot = Factory.Load<GlobalChargeCodeMapPivot>(pivotPK);
			AssertType(typeof(GlobalChargeCodeMapPivotIntercompany), globalChargeCodePivot);
		}

		public void TestGlobalChargeCodeMapOrganizationType()
		{
			GlobalChargeCodeMapOrganization globalChargeCodeOrganization = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCodeOrganization.YG_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			ZGuid pivotPK = globalChargeCodeOrganization.PivotCollection.AddNew().PK;
			GlobalChargeCodeMapPivot globalChargeCodePivot = Factory.Load<GlobalChargeCodeMapPivot>(pivotPK);
			AssertType(typeof(GlobalChargeCodeMapPivotOrganization), globalChargeCodePivot);
		}
	}
}