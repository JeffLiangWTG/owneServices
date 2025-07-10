using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class AUCustomsHouseAirCargoFilterControlTest : TestCaseWithFactory
	{
		public void TestInitializeColumnsAvailibility()
		{
			var mawb = Factory.New<CusMAWB>();
			var cusHAWBCollection = new CusHAWBCollection(mawb, Factory);
			var filterBO = new AUCustomsHouseAirCargoFilterBusinessObject();
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			using (ZForm form = new ZForm())
			using (var filterControl = new AUCustomsHouseAirCargoFilterControl(cusHAWBCollection, filterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNull(grid.Columns[CusHAWB.Schema.CS_fPartShipConsignmentReference]);
				AssertNull(grid.Columns[CusHAWB.Schema.InBondStore]);
			}

			Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (ZForm form = new ZForm())
			using (var filterControl = new AUCustomsHouseAirCargoFilterControl(cusHAWBCollection, filterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertNotNull(grid.Columns[CusHAWB.Schema.CS_fPartShipConsignmentReference]);
			}
		}
	}
}
