using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaPackedItemsUserControlTest : TestCaseWithFactory
	{
		public void TestSetVisibility()
		{
			var manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_TransportMode = "AIR";
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItems = pack.PackedItems.AddNewPackedItem();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var asycudaPackedItemsUserControl = asycudaPackUserControl.FindSingle<AsycudaPackedItemsUserControl>(c => c.Name == "AsycudaPackedItemsUserControl");
				var packedItemsGrid = asycudaPackedItemsUserControl.FindSingle<ZGrid>(c => c.Name == "PackedItemsGrid");

				var tariffColumn = packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_FormattedTariff) as Universal.GUI.TariffColumnStyleInfo;
				AssertEquals("API_FormattedTariff Visible", true, tariffColumn.IsVisible);
				AssertEquals("GetCountryCode()", "TR", tariffColumn.GetCountryCode());
				AssertEquals("GetDataGrouping()", "TR", tariffColumn.GetDataGrouping());
				AssertEquals("API_GoodsDescription Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_GoodsDescription).IsVisible);
				AssertEquals("API_CustomsQty Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_CustomsQty).IsVisible);
				AssertEquals("API_CustomsUQ Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_CustomsUQ).IsVisible);
				AssertEquals("API_GrossWeight Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_GrossWeight).IsVisible);
				AssertEquals("API_GrossWeightUQ Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_GrossWeightUQ).IsVisible);
				AssertEquals("API_NetWeight Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_NetWeight).IsVisible);
				AssertEquals("API_NetWeightUQ Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_NetWeightUQ).IsVisible);
				AssertEquals("API_GoodsValue Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_GoodsValue).IsVisible);
				AssertEquals("API_RX_NKGoodsValueCurrency Visible", true, packedItemsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency).IsVisible);
				AssertEquals("UNDGSubstanceManagerValue Visible", true, packedItemsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue).IsVisible);
				AssertEquals("UNDGClassManagerValue Visible", true, packedItemsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue).IsVisible);
			}

			var previousValue = Env.Registry.ExternalBorderComplianceTool;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			using (new DisposableAction(() => Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None, () => Env.Registry.ExternalBorderComplianceTool = previousValue))
			{
				var helper = new ZZDataTestHelper(Factory);
				Factory.Save();

				var manifest2 = Factory.New<AsycudaManifestHeader>();
				manifest2.FillWithValidTestData();
				manifest2.AMA_TransportMode = TransportTypeList.Codes.Air;

				var bill2 = manifest2.Bills.AddNew();
				var pack2 = bill2.Packs.AddNew();

				using (var form = new ManifestForm(manifest2))
				{
					form.Show();
					Application.DoEvents();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;

					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
					billsAndPacksTabControl.SelectedTab = packsTabPage;

					var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
					var tariffFindBox = asycudaPackUserControl.FindSingle<Universal.GUI.TariffFindBox>(nameof(CommonPackedItemDetailsUserControl.TariffFindBox));

					AssertEquals("tariffFindBox.Visible", true, tariffFindBox.Visible);
					AssertEquals("PackedItem.API_FormattedTariff", tariffFindBox.BindTo);
					AssertEquals("tariffFindBox.ShouldResize", false, tariffFindBox.ShouldResize);
					AssertEquals("tariffFindBox.ShowDescriptionBox", false, tariffFindBox.ShowDescriptionBox);
					var goodsTypeDropEdit = asycudaPackUserControl.FindSingle<ZDropEdit>(c => c.Name == "GoodsTypeDropEdit");
					AssertEquals("goodsTypeDropEdit.Visible", true, goodsTypeDropEdit.Visible);
					AssertEquals("PackedItem.GoodsType", goodsTypeDropEdit.BindTo);
				}
			}
		}

		public void TestPackAdditionalTabPageVisibility()
		{
			var mockProvider = new Mock<ApplicationGUIProvider>();
			mockProvider.CallBase = true;
			mockProvider
				.Protected()
				.Setup<IEnumerable<IAdditionalTabPage>>("GetPackAdditionalTabPageUserControlCore")
				.Returns(new List<AdditionalTabPageForTest>() { new AdditionalTabPageForTest() });
			mockProvider
				.Protected()
				.Setup<IEnumerable<IAdditionalTabPage>>("GetBillAdditionalTabPageUserControlCore")
				.Returns(new List<AsycudaPackUserControl>() { new AsycudaPackUserControl() });

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var applicationProviderKey = manifest.GetApplicationProviderKey();
			var cacheKey = string.Format("ApplicationGuiProvider_{0}_{1}_{2}", applicationProviderKey.CountryOrGrouping, applicationProviderKey.ManfestTypeCode, applicationProviderKey.ApplicationCode);
			Factory.GetCachedValue(cacheKey, () =>
			{
				return mockProvider.Object;
			});

			var bill = manifest.Bills.AddNew();
			bill.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			var pack = bill.Packs.AddNew();
			var packedItems = pack.PackedItems.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var asycudaPackedItemsUserControl = asycudaPackUserControl.FindSingle<AsycudaPackedItemsUserControl>(c => c.Name == "AsycudaPackedItemsUserControl");

				var additionalTabPage = (ZTabPage)asycudaPackedItemsUserControl.Controls.Find("PackedItemDeatilsTabControl_TabPage_AdditionalTabPageForTest", true).FirstOrDefault();
				AssertEquals(true, additionalTabPage.TabVisible);
			}

			mockProvider.VerifyAll();
		}

		sealed class AdditionalTabPageForTest : ZUserControl, IAdditionalTabPage
		{
			public ZUserControl AdditionalTabPageUserControl => this;

			public ResourceStringData AdditionalTabPageCaption => NoResourceStringData.GetData("TestAdditionalTabPageCaption", "Test");

			public AdditionalTabPageVisibility AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

			public int TabPageSequence => 1;
		}
	}
}
