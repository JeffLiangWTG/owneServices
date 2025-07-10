using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaContainerUserControlTest : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var containersTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "containersTabPage");
				mainTabControl.SelectedTab = containersTabPage;
				var asycudaContainerUserControl = containersTabPage.FindSingle<AsycudaContainerUserControl>(c => c.Name == "asycudaContainerUserControl");
				var containerDataSplitContainer = asycudaContainerUserControl.FindSingle<CargoWise.Windows.UI.KSplitContainer>(c => c.Name == "containerDataSplitContainer");
				AssertEquals("containerDataSplitContainer.Panel1Collapsed", false, containerDataSplitContainer.Panel1Collapsed);
				AssertEquals("containerDataSplitContainer.Panel2Collapsed", true, containerDataSplitContainer.Panel2Collapsed);
				var containersGrid = containerDataSplitContainer.Panel1.FindSingle<ZGrid>(c => c.Name == "containersGrid");
				CombineAssertions(() =>
				{
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_ContainerNumber, false, true, true, CharacterCasing.Upper, "Container Number", 110);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_EmptyFullIndicator, false, true, true, CharacterCasing.Upper, "Empty/Full", 74);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_RC_ContainerType, false, true, true, CharacterCasing.Upper, "Cont. Type", 75);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal1, false, false, true, CharacterCasing.Upper, "Seal 1 Number", 109);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealType1, false, false, true, CharacterCasing.Upper, "Seal 1 Type", 79);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyType, false, false, true, CharacterCasing.Upper, "Seal 1 Party Type", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyName, false, false, true, CharacterCasing.Upper, "Seal 1 Party Name", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal2, false, false, false, CharacterCasing.Upper, "Seal 2 Number", 109);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealType2, false, false, false, CharacterCasing.Upper, "Seal 2 Type", 79);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyType2, false, false, false, CharacterCasing.Upper, "Seal 2 Party Type", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal3, false, false, false, CharacterCasing.Upper, "Seal 3 Number", 109);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealType3, false, false, false, CharacterCasing.Upper, "Seal 3 Type", 79);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyType3, false, false, false, CharacterCasing.Upper, "Seal 3 Party Type", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_NumberOfPackages, false, false, true, CharacterCasing.Upper, "Packages", 70);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_CommodityCode, false, false, true, CharacterCasing.Upper, "Commodity Code", 105);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_GoodsWeight, false, false, true, CharacterCasing.Normal, "Gross Weight", 87);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_GoodsWeightUQ, false, false, true, CharacterCasing.Upper, "Weight UQ", 75);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_StowageLocation, false, false, true, CharacterCasing.Upper, "Stow Location", 90);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal1UnloadingState, false, false, false, CharacterCasing.Upper, "Seal 1 State", 120);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal2UnloadingState, false, false, false, CharacterCasing.Upper, "Seal 2 State", 120);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal3UnloadingState, false, false, false, CharacterCasing.Upper, "Seal 3 State", 120);
				});
			}

			var gridColumnAvailability = new Dictionary<bool, string[]>();
			gridColumnAvailability.Add(true, new[] { AsycudaContainer.Schema.ACN_ClusterKey });
			gridColumnAvailability.Add(false, new[] { AsycudaContainer.Schema.ACN_StowageLocation });
			var gridColumnsOrder = new List<string>(new[]
			{
				AsycudaContainer.Schema.ACN_ClusterKey,
				AsycudaContainer.Schema.ACN_CommodityCode
			});
			var clusterKeyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			clusterKeyCalcEditColumnStyleInfo.IsVisible = false;
			clusterKeyCalcEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			clusterKeyCalcEditColumnStyleInfo.ColumnName = AsycudaContainer.Schema.ACN_ClusterKey;
			clusterKeyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			var gridExtraColumnInfos = new List<ZGridColumnInfo>(new[] { clusterKeyCalcEditColumnStyleInfo });
			var gridColumnWidth = new Dictionary<string, int>();
			gridColumnWidth.Add(AsycudaContainer.Schema.ACN_ContainerNumber, 130);
			gridColumnWidth.Add(AsycudaContainer.Schema.ACN_EmptyFullIndicator, 150);
			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(manifest);
			applicationGUIProvider.GetContainerCountrySpecificUserControlForTesting = () => new ContainerCountrySpecificUserControlForTesting();
			applicationGUIProvider.GetContainersGridColumnAvailabilityForTesting = () => gridColumnAvailability;
			applicationGUIProvider.GetContainersGridColumnsOrderForTesting = () => gridColumnsOrder;
			applicationGUIProvider.GetContainersGridExtraColumnInfosForTesting = () => gridExtraColumnInfos;
			applicationGUIProvider.GetContainersGridColumnsWidthForTesting = () => gridColumnWidth;
			using (applicationGUIProvider)
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var containersTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "containersTabPage");
				mainTabControl.SelectedTab = containersTabPage;
				var asycudaContainerUserControl = containersTabPage.FindSingle<AsycudaContainerUserControl>(c => c.Name == "asycudaContainerUserControl");
				var containerDataSplitContainer = asycudaContainerUserControl.FindSingle<CargoWise.Windows.UI.KSplitContainer>(c => c.Name == "containerDataSplitContainer");
				AssertEquals("containerDataSplitContainer.Panel1Collapsed", false, containerDataSplitContainer.Panel1Collapsed);
				AssertEquals("containerDataSplitContainer.Panel2Collapsed", false, containerDataSplitContainer.Panel2Collapsed);
				var containersGrid = containerDataSplitContainer.Panel1.FindSingle<ZGrid>(c => c.Name == "containersGrid");
				CombineAssertions(() =>
				{
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_ClusterKey, false, false, false, CharacterCasing.Lower, null, 86);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_CommodityCode, false, false, true, CharacterCasing.Upper, "Commodity Code", 105);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_ContainerNumber, false, true, true, CharacterCasing.Upper, "Container Number", 130);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_EmptyFullIndicator, false, true, true, CharacterCasing.Upper, "Empty/Full Indicator", 150);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_RC_ContainerType, false, true, true, CharacterCasing.Upper, "Cont. Type", 75);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal1, false, false, true, CharacterCasing.Upper, "Seal 1 Number", 109);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealType1, false, false, true, CharacterCasing.Upper, "Seal 1 Type", 79);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyType, false, false, true, CharacterCasing.Upper, "Seal 1 Party Type", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyName, false, false, true, CharacterCasing.Upper, "Seal 1 Party Name", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal2, false, false, false, CharacterCasing.Upper, "Seal 2 Number", 109);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealType2, false, false, false, CharacterCasing.Upper, "Seal 2 Type", 79);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyType2, false, false, false, CharacterCasing.Upper, "Seal 2 Party Type", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_Seal3, false, false, false, CharacterCasing.Upper, "Seal 3 Number", 109);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealType3, false, false, false, CharacterCasing.Upper, "Seal 3 Type", 79);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_SealingPartyType3, false, false, false, CharacterCasing.Upper, "Seal 3 Party Type", 115);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_NumberOfPackages, false, false, true, CharacterCasing.Upper, "Packages", 70);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_GoodsWeight, false, false, true, CharacterCasing.Normal, "Gross Weight", 87);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_GoodsWeightUQ, false, false, true, CharacterCasing.Upper, "Weight UQ", 75);
					AssertGridColumnInfo(containersGrid, AsycudaContainer.Schema.ACN_StowageLocation, true, false, true, CharacterCasing.Upper, "Stow Location", 90);
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

		sealed class ContainerCountrySpecificUserControlForTesting : ContainerCountrySpecificUserControl { }
	}
}
