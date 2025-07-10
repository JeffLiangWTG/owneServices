using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5DepartureMovementsTabGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(INctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>), control.BindingSource.DataSourceType);
		}

		public void TestMovementsGrid()
		{
			control.AssertContainsControl<ZGrid>(nameof(Phase5DepartureMovementsTabGridUserControl.MovementsGrid), x => x
				.WithBindTo(".")
			);
		}

		public void TestMenuItem()
		{
			var grid = control.MovementsGrid;

			AssertEquals(true, grid.ContextMenu.MenuItems.Cast<MenuItem>().Any(x => x.Text == "Reset Movement for Retransmission"));
		}

		public void TestContextMenu_ResetMovementMenuItem_Visible()
		{
			NctsConfigurationTestHelper.TemporarilySetIsDepartureRetransmissionSupportedCore(Factory, true);

			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();

			control.SetDataBinding(header.MovementHeader, "");
			var movementsGrid = control.MovementsGrid;
			movementsGrid.DataSource = header.MovementHeader;

			movementsGrid.Select(0);
			movementsGrid.ContextMenu.ShowPopupMenu();
			var resetMovementMenuItem = movementsGrid.ContextMenu.MenuItems.FindByText("Reset Movement for Retransmission");
			AssertEquals("resetMovementMenuItem.Visible", false, resetMovementMenuItem.Visible);

			header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;
			Factory.Save();

			movementsGrid.Select(0);
			movementsGrid.ContextMenu.ShowPopupMenu();
			resetMovementMenuItem = movementsGrid.ContextMenu.MenuItems.FindByText("Reset Movement for Retransmission");
			AssertEquals("resetMovementMenuItem.Visible", true, resetMovementMenuItem.Visible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5DepartureMovementsTabGridUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		Phase5DepartureMovementsTabGridUserControl control;
	}
}
