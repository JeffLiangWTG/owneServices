using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5DeclarationCountryOfRoutingTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CountryOfRoutingCollection<CountryOfRouting>), userControl.BindingSource.DataSourceType);
		}

		public void TestGridDockStyle()
		{
			AssertEquals("Dock", DockStyle.Fill, userControl.CountryOfRoutingsGrid.Dock);
		}

		public void TestGridBindingMember()
		{
			AssertEquals("BindingMember", ".", userControl.CountryOfRoutingsGrid.GetBindingMember());
		}

		public void TestCY_Order()
		{
			AssertColumn<ZCalcEditColumnStyleInfo>(CountryOfRouting.Schema.CY_Order, 0, 73);
		}

		public void TestCY_Data()
		{
			AssertColumn<ZCodeFindBoxColumnStyleInfo>(CountryOfRouting.Schema.CY_Data, 1, 61);
		}

		public void TestDescription()
		{
			AssertColumn<ZTextBoxColumnStyleInfo>(CountryOfRouting.Schema.Description, 2, 204);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5DeclarationCountryOfRoutingTabUserControl();
		}
		Phase5DeclarationCountryOfRoutingTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		void AssertColumn<T>(string name, int index, int width) where T : ZGridColumnInfo
		{
			var grid = userControl.CountryOfRoutingsGrid;
			var columnStyle = grid.GetColumnStyle(name);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Index", index, grid.ColumnStyles.IndexOf(columnStyle));
				AssertEquals("Width", width, columnStyle.Width);
				AssertEquals("Visible", true, columnStyle.IsVisible);
			});
		}
	}
}
