using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	sealed class ContainerSpecificUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new ContainerSpecificUserControl())
			{
				var tabControl = control.FindSingle<ZTabControl>("SealTabControl");
				AssertNotNull(tabControl);

				AssertEquals("Tab Pages count", 1, tabControl.TabCount);

				AssertContainsExactElementsInExactOrder("Tab Pages names",
					new[]
					{
						"AdditionalSealsTabPage"
					},
					tabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
			}
		}

		public void TestGridColumns()
		{
			using (var control = new ContainerSpecificUserControl())
			{
				var additionalSealsGrid = (ZGrid)control.Controls.Find("AdditionalSealsGrid", true).First();
				var columns = additionalSealsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("AdditionalSealsGrid Column Names",
					new[]
					{
						"BK_SealNumber",
						"BK_SealType",
						"BK_UnloadingState",
						"BK_SealingPartyType"
					},
					columns.Select(x => x.ColumnName));

				AssertGridColumnInfo(additionalSealsGrid, "BK_SealNumber", false, false, true, CharacterCasing.Normal, 150);
				AssertGridColumnInfo(additionalSealsGrid, "BK_SealType", false, false, true, CharacterCasing.Normal, 150);
				AssertGridColumnInfo(additionalSealsGrid, "BK_UnloadingState", false, false, true, CharacterCasing.Normal, 150);
			}
		}

		void AssertGridColumnInfo(ZGrid containersGrid, string columnName, bool isUnavailable, bool isMandatory, bool isVisible, CharacterCasing characterCasing, int width)
		{
			var gridColumnInfo = containersGrid.GetColumnStyle(columnName);

			CombineAssertions(() =>
			{
				AssertNotNull(columnName + " column template", gridColumnInfo);
				AssertEquals(columnName + " IsUnavailable", isUnavailable, gridColumnInfo.IsUnavailable);
				AssertEquals(columnName + " IsMandatory", isMandatory, gridColumnInfo.IsMandatory);
				AssertEquals(columnName + " IsVisible", isVisible, gridColumnInfo.IsVisible);
				AssertEquals(columnName + " CharacterCasing", characterCasing, gridColumnInfo.CharacterCasing);
				AssertEquals(columnName + " Width", width, gridColumnInfo.Width);
			});
		}
	}
}
