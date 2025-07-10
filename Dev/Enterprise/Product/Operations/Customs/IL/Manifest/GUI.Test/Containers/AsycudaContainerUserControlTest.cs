using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	sealed class AsycudaContainerUserControlTest : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var containersTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "containersTabPage");
				mainTabControl.SelectedTab = containersTabPage;
				var asycudaContainerUserControl = containersTabPage.FindSingle<AsycudaContainerUserControl>(c => c.Name == "asycudaContainerUserControl");
				var containerDataSplitContainer = asycudaContainerUserControl.FindSingle<CargoWise.Windows.UI.KSplitContainer>(c => c.Name == "containerDataSplitContainer");
				var containersGrid = containerDataSplitContainer.Panel1.FindSingle<ZGrid>(c => c.Name == "containersGrid");
				CombineAssertions(() =>
				{
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal1UnloadingState, false, false, true, CharacterCasing.Upper, "Seal 1 State", 120);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal2UnloadingState, false, false, true, CharacterCasing.Upper, "Seal 2 State", 120);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal3UnloadingState, false, false, true, CharacterCasing.Upper, "Seal 3 State", 120);
				});
			}
		}

		void AssertGridColumnInfo(ZGrid containersGrid, string columnName, bool isUnavailable, bool isMandatory, bool isVisible, CharacterCasing characterCasing, string headerText, int width)
		{
			var gridColumnInfo = containersGrid.GetColumnStyle(columnName);

			AssertNotNull(columnName + " column template", gridColumnInfo);

			AssertEquals(columnName + " IsUnavailable", isUnavailable, gridColumnInfo.IsUnavailable);
			AssertEquals(columnName + " IsMandatory", isMandatory, gridColumnInfo.IsMandatory);
			AssertEquals(columnName + " IsVisible", isVisible, gridColumnInfo.IsVisible);
			AssertEquals(columnName + " CharacterCasing", characterCasing, gridColumnInfo.CharacterCasing);
			AssertEquals(columnName + " CaptionResourceString.Caption", null, gridColumnInfo.CaptionResourceString.Caption);
			AssertEquals(columnName + " GroupName.Caption", null, gridColumnInfo.GroupName.Caption);
			AssertEquals(columnName + " Width", width, gridColumnInfo.Width);

			var columnInstance = containersGrid.Columns.OfType<ZGridColumn>().SingleOrDefault(x => x.ColumnName == columnName);
			if (isUnavailable)
			{
				AssertNull(columnName + " column instance", columnInstance);
			}
			else
			{
				AssertNotNull(columnName + " column instance", columnInstance);

				if (headerText != null)
				{
					AssertEquals(columnName + " Column.Caption", headerText, columnInstance.ColumnStyle.HeaderText);
				}
			}
		}
	}
}
