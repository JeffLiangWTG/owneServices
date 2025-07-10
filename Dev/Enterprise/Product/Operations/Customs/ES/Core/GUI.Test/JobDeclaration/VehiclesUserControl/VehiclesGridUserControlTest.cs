using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class VehiclesGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ICusVehicleCollection<CusVehicle, JobComInvoiceLine>), control.BindingSource.DataSourceType);
		}

		public void TestVehiclesGridBinding()
		{
			using (var control = new VehiclesGridUserControl())
			{
				AssertEquals("VehiclesGrid.BindTo", ".", control.VehiclesGrid.BindTo);
			}
		}

		public void TestVehiclesGridColumns()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			control.SetDataBinding(invoiceLine.Vehicles, "");
			var columnNames = control.VehiclesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			var layoutProvider = Activator.CreateInstance<VehiclesGridColumnLayout>() as IGridColumnLayoutProvider;
			var expectedColumnNames = layoutProvider.Layout.Columns.Select(x => x.ColumnName);
			AssertContainsExactElementsInExactOrder(expectedColumnNames, columnNames);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new VehiclesGridUserControl();
		}
		VehiclesGridUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
