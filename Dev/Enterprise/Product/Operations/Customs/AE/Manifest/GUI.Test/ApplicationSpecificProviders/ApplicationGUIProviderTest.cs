using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.AE.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(ApplicationGUIProvider))]
sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
{
	public void TestManifestLayoutType()
	{
		var provider = GetProvider();
		AssertType<ManifestLayouts>("ManifestLayouts Type", provider.GetManifestLayout());
	}

	protected override void AssertColumnsToRemoveInBillsGrid(string[] columns)
	{
		AssertContainsExactElementsInExactOrder([AE.Manifest.Business.AsycudaBill.Schema.ABL_MarksAndNumbers, AE.Manifest.Business.AsycudaBill.Schema.ABL_Remarks, AE.Manifest.Business.AsycudaBill.Schema.ABL_GoodsDescription], columns);
	}

	protected override int MaxColumnsOfManifestLayout => 3;

	protected override Type ExpectedBillLayoutType => typeof(BillDetailsLayout);

	protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

	protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

	protected override Type ExpectedAsycudaItemSelectionDialogType => typeof(BillsSelectionDialog);

	protected override AsycudaManifestHeader CreateNewManifest()
	{
		var manifest = base.CreateNewManifest();
		manifest.AMA_ManifestType = "ACI";
		return manifest;
	}

	protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
	{
		AssertContainsExactElementsInAnyOrder(new[] { AsycudaContainer.Schema.ACN_SetPointTemperature, AsycudaContainer.Schema.ACN_SetPointTemperatureUnit }, columnInfos.Select(x => x.ColumnName));

		var temperatureGroup = columnInfos.Single(x => x.ColumnName == AsycudaContainer.Schema.ACN_SetPointTemperature).GroupName;
		var temperatureUQGroup = columnInfos.Single(x => x.ColumnName == AsycudaContainer.Schema.ACN_SetPointTemperatureUnit).GroupName;

		AssertEquals("Temperature and Temperature UQ are in the same group", temperatureGroup, temperatureUQGroup);
	}

	protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
	{
		var provider = GetProvider();
		var packs = provider.GetPacksGridColumnsOrder();
		var columns = new[]
		{
			AsycudaPack.Schema.ContainerPK,
			AsycudaPack.Schema.APA_PackQty,
			AsycudaPack.Schema.APA_PackUQ,
			"PackedItem+API_FormattedTariff",
			AsycudaPack.Schema.APA_GoodsDescription,
			AsycudaPack.Schema.APA_MarksAndNumbers,
			AsycudaPack.Schema.APA_Weight,
			AsycudaPack.Schema.APA_WeightUQ,
			AsycudaPack.Schema.APA_Volume,
			AsycudaPack.Schema.APA_VolumeUQ,
		};

		AssertContainsExactElementsInExactOrder("PacksGridColumnsOrder", columns, packs);
	}

	protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
	{
		var negotiableColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AE.Manifest.Business.AsycudaBill.Schema.Negotiable);
		var payerColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AE.Manifest.Business.AsycudaBill.Schema.Payer);
		var billStatusColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AE.Manifest.Business.AsycudaBill.Schema.ABL_BillStatus);
		var billStatusDescriptionColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AE.Manifest.Business.AsycudaBill.Schema.ABL_BillStatusDescription);
		var messageStatusColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AE.Manifest.Business.AsycudaBill.Schema.ABL_MessageStatus);

		CombineAssertions(() =>
		{
			AssertNotNull("negotiableColumnStyleInfo", negotiableColumnStyleInfo);
			Assert(negotiableColumnStyleInfo.IsVisible);
			AssertType(typeof(ZDropEditColumnStyleInfo), negotiableColumnStyleInfo);

			AssertNotNull("payerColumnStyleInfo", payerColumnStyleInfo);
			Assert(payerColumnStyleInfo.IsVisible);
			AssertType(typeof(ZTextBoxColumnStyleInfo), payerColumnStyleInfo);

			AssertNotNull("billStatusColumnStyleInfo", billStatusColumnStyleInfo);
			AssertEquals("ABL_BillStatus column is not visible by default", false, billStatusColumnStyleInfo.IsVisible);
			AssertEquals("ABL_BillStatus character casing should be upper", System.Windows.Forms.CharacterCasing.Upper, billStatusColumnStyleInfo.CharacterCasing);
			AssertEquals("ABL_BillStatus column is available", false, billStatusColumnStyleInfo.IsUnavailable);
			AssertEquals("ABL_BillStatus should be grouped under Bill Status", "Bill Status", billStatusColumnStyleInfo.GroupName.Caption);
			AssertType(typeof(ZDropEditColumnStyleInfo), billStatusColumnStyleInfo);

			AssertNotNull("billStatusDescriptionColumnStyleInfo", billStatusDescriptionColumnStyleInfo);
			AssertEquals("ABL_BillStatusDescription column is not visible by default", false, billStatusDescriptionColumnStyleInfo.IsVisible);
			AssertEquals("ABL_BillStatusDescription should be grouped under Bill Status", "Bill Status", billStatusDescriptionColumnStyleInfo.GroupName.Caption);
			AssertType(typeof(ZTextBoxColumnStyleInfo), billStatusDescriptionColumnStyleInfo);

			AssertNotNull("messageStatusColumnStyleInfo", messageStatusColumnStyleInfo);
			AssertEquals("ABL_MessageStatus column is not visible by default", false, messageStatusColumnStyleInfo.IsVisible);
			AssertEquals("ABL_MessageStatus character casing should be upper", System.Windows.Forms.CharacterCasing.Upper, messageStatusColumnStyleInfo.CharacterCasing);
			AssertEquals("ABL_MessageStatus is available", false, messageStatusColumnStyleInfo.IsUnavailable);
			AssertType(typeof(ZTextBoxColumnStyleInfo), messageStatusColumnStyleInfo);
		});
	}

	protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
	{
		Assert(columnInfos.First(i => i.ColumnName == "PackedItem+API_FormattedTariff").IsVisible);
	}

	protected override void AssertGetPacksGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility) => CombineAssertions(() =>
	{
		var pair = columnVisibility.Single();
		AssertEquals("Key", false, pair.Key);
		AssertEquals("Columns", AsycudaPack.Schema.APA_CommodityCode, pair.Value.Single());
	});

	ASYCUDA.GUI.ApplicationGUIProvider GetProvider() => ASYCUDA.GUI.ApplicationGUIProvider.GetApplicationGuiProvider(CreateNewManifest());

	public void TestAddEDocsMenuItems()
	{
		var header = CreateNewManifest();
		var main = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).New<StorageMain>();
		main.SM_ParentFK = header.PK;
		main.SM_Type = Core.Constants.DocManagerCodes.JobDeclaration;
		var document = main.Documents.AddNew();
		document.SC_FileName = "QRPTest";
		document.SC_DataType = Core.Constants.FileFormats.PDF;
		document.SC_DocType = Core.Constants.RefDocTypes.QuarantineRemotePrint;
		using var form = new ManifestForm(header);
		CombineAssertions(() =>
		{
			form.Show();
			var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl", 1);
			var eDocTabPage = (ZTabPage)mainTabControl.TabPages["eDocsTabPage"];
			mainTabControl.SelectedTab = eDocTabPage;
			var plugIn = (eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
			var userControl = (eDocsUserControl)plugIn.UserControl;
			var grid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");
			grid.SelectAll();
			AssertNotNull("menuItem exists", GetMenuItem());
			AssertEquals("menuItem is visible and enabled", true, GetMenuItem().Enabled);
			userControl.Visible = false;
			grid.RebuildContextMenu();
			AssertNull("MenuItem does not exist", GetMenuItem());
			userControl.Visible = true;
			AssertNotNull("userControl is visible, and one document is selected, menuItem exists", GetMenuItem());
			AssertEquals("The grid is visible, menuItem is abled", true, GetMenuItem().Enabled);

			MenuItem GetMenuItem() => grid.ContextMenu.MenuItems.FindByText("Send this document to the NAIC");
		});
	}
}
