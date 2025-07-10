using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusInBondContainer.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5ArrivalContainersEquipmentGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(Business.NctsArrivalHeaderContainerCollection), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { BC_SequenceNumber, BC_UnloadedState, BC_Mode, BC_ContainerNum, "TotalSealCount" },
				containerEquipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestAdditionalSealsGrid()
		{
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", containerEquipmentGrid);
				AssertEquals("BindTo", ".", containerEquipmentGrid.BindTo);
			});
		}

		public void TestContainersEquipmentTabControl()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Tabs", 1, userControl.ContainersEquipmentTabControl.TabPages.Count);
				AssertEquals("Containers/Equipment", userControl.ContainersEquipmentTabPage.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5ArrivalContainersEquipmentGridUserControl();
			containerEquipmentGrid = userControl.ContainersEquipmentGrid;
		}
		Phase5ArrivalContainersEquipmentGridUserControl userControl;
		ZGrid containerEquipmentGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
