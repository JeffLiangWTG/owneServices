using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class NctsContainerGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new NctsContainerGridUserControl())
			{
				AssertEquals(typeof(NctsPackage), control.BindingSource.DataSourceType);
			}
		}

		public void TestSelectedContainersGrid()
		{
			using (var control = new NctsContainerGridUserControl())
			{
				var containersGrid = control.SelectedContainersGrid;
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", nameof(NctsPackage.ContainersPivotsForBindingOnly), containersGrid.BindTo);
					AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, containersGrid.Dock);

					var containerNumberColumnInfo = containersGrid.GetColumnStyle(nameof(NonPersistentDepartureContainerPivot.ContainerNumber));
					AssertType<ZTextBoxColumnStyleInfo>("ContainerNumber column type", containerNumberColumnInfo);
					AssertEquals("ContainerNumber column width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), containerNumberColumnInfo.Width);
					AssertEquals("ContainerNumber character casing", System.Windows.Forms.CharacterCasing.Normal, containerNumberColumnInfo.CharacterCasing);

					var containerSelectedColumnInfo = containersGrid.GetColumnStyle(nameof(NonPersistentDepartureContainerPivot.ContainerSelected));
					AssertType<ZCheckBoxColumnStyleInfo>("ContainerSelected column type", containerSelectedColumnInfo);
					AssertEquals("ContainerSelected column width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60), containerSelectedColumnInfo.Width);
				});
			}
		}
	}
}
