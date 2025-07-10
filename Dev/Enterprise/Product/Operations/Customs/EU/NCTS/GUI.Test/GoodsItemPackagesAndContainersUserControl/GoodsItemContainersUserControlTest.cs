using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class GoodsItemContainersUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestContainersGroupBox()
		{
			var containersGroupBox = control.ContainersGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Containers", containersGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, containersGroupBox.Dock);
			});
		}

		public void TestSelectedContainersGrid()
		{
			var containersGrid = control.SelectedContainersGrid;
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.ContainersPivots), containersGrid.BindTo);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, containersGrid.Dock);

				var containerNumberColumnInfo = containersGrid.GetColumnStyle(nameof(NonPersistentDepartureContainerPivot.ContainerNumber));
				AssertType<ZTextBoxColumnStyleInfo>("ContainerNumber column type", containerNumberColumnInfo);
				AssertEquals("ContainerNumber column width", 80, containerNumberColumnInfo.Width);
				AssertEquals("ContainerNumber character casing", System.Windows.Forms.CharacterCasing.Normal, containerNumberColumnInfo.CharacterCasing);

				var containerSelectedColumnInfo = containersGrid.GetColumnStyle(nameof(NonPersistentDepartureContainerPivot.ContainerSelected));
				AssertType<ZCheckBoxColumnStyleInfo>("ContainerSelected column type", containerSelectedColumnInfo);
				AssertEquals("ContainerSelected column width", 60, containerSelectedColumnInfo.Width);

				AssertEquals("SelectedContainersGrid is within ContainersGroupBox", true, control.ContainersGroupBox.Controls.Contains(containersGrid));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GoodsItemContainersUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		GoodsItemContainersUserControl control;
	}
}
