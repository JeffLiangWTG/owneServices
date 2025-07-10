using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForFactorySave()
		{
			AUCustomsDataRegistry.Instance.AttachOrphanedCARSTsWhenCusSCAPivotCreated.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertEquals(0, Factory.ActiveFetchHintsForTable(EDIMessageSchema.Constants.TableName));
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OC";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN001";
			var house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "HB";
			var pivot = house.Pivot.AddNew();
			container.Pivots.Add(pivot);
			var strategy = new CusSCAPivotFetchStrategy(pivot);
			strategy.FetchForFactorySave();
			AssertEquals("CMRCARSTMessage", 1, Factory.ActiveFetchHintsForTable(EDIMessageSchema.Constants.TableName));
		}
	}
}
