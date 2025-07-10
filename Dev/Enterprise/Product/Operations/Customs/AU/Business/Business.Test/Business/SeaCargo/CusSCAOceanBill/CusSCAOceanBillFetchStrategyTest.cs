using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForFactorySave_Messages()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var houseBill1 = oceanBill.HouseBills.AddNew();
			var pivot1 = houseBill1.Pivot.AddNew();
			pivot1.CV_CN = container.PK;
			var houseBill2 = oceanBill.HouseBills.AddNew();
			var pivot2 = houseBill2.Pivot.AddNew();
			pivot2.CV_CN = container.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var oceanBillInNewFactory = newFactory.Load<CusSCAOceanBill>(oceanBill.PK);
			oceanBillInNewFactory.HasChanges = true;
			var pivot1InNewFactory = newFactory.Load<CusSCAPivot>(pivot1.PK);
			pivot1InNewFactory.HasChanges = true;
			var pivot2InNewFactory = newFactory.Load<CusSCAPivot>(pivot2.PK);
			pivot2InNewFactory.HasChanges = true;
			newFactory.Save();

			AssertEquals(1, newFactory.GetTableHitCount(CusSCAHouseSchema.Constants.TableName));
		}
	}
}
