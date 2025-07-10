using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : H7ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestGetManifestLayout()
		{
			var header = CreateNewManifest();
			var manifestLayout = ASYCUDA.GUI.ApplicationGUIProvider.GetApplicationGuiProvider(header).GetManifestLayout();
			AssertType<H7ManifestLayout>(manifestLayout);
		}

		public void TestBillAdditionalTabPages_GridColumns()
		{
			var expectedControlGridColumns = new Dictionary<string, string[]>
			{
				{ "AdditionalInfoUserControl", new[] { "CSI_Code", "CSI_Description" } },
				{ "SupportingDocumentsUserControl", new[] { "CSI_Code", "CSI_Description", "CSI_ReferenceNumber", "CSI_Actions", "CSI_Availability", "CSI_DateOfIssue", "CSI_ReferenceNumber2", "CSI_DateOfExpiry", "CSI_SubType" } },
				{ "H7PreviousDocumentsUserControl", new[] { "CSI_Code", "CSI_ReferenceNumber", "DocumentDescription" } }
			};

			var manifest = CreateNewManifest();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				foreach (var controlAndGridColumns in expectedControlGridColumns)
				{
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == $"billsAndPacksTabControl_TabPage_{controlAndGridColumns.Key}");
					billsAndPacksTabControl.SelectedTab = itemTabpage;

					var grid = itemTabpage.FindSingle<ZGrid>();

					CombineAssertions(controlAndGridColumns.Key, () =>
					{
						AssertContainsExactElementsInAnyOrder("number of columns is the same", controlAndGridColumns.Value, grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName));
						EUH7GUITestHelper.AssertGridLayout(grid, controlAndGridColumns.Value);
					});
				}
			}
		}

		public void TestTariffDataGroupingForNorthIrelandPortOfDischarge()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			var refCountryStates = Factory.New<RefCountryStates>();
			refCountryStates.RW_RN_NKCountryCode = "UK";
			refCountryStates.RW_Code = "XXX";
			refCountryStates.RW_RegionName = "NORTHERN IRELAND";
			belfast.RL_RW = refCountryStates.PK;

			var manifest = CreateNewManifest();
			manifest.AMA_RL_NKPortOfDischarge = "GBBEL";
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>("billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>("billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var itemsGrid = itemTabpage.FindSingle<ZGrid>("ItemsGrid");
				var tariffColumnStyle = itemsGrid.ColumnStyles.ToArray().FirstOrDefault(x => x is Universal.GUI.TariffColumnStyleInfo) as Universal.GUI.TariffColumnStyleInfo;
				var tariffFindBox = billsAndPacksTabPage.FindSingle<Universal.GUI.TariffFindBox>("TariffFindBox");

				CombineAssertions(() =>
				{
					AssertEquals("Tariff Column DataGrouping", Universal.Constants.DataGrouping.EuropeanUnion, tariffColumnStyle.GetDataGrouping());
					AssertEquals("TariffFindBox DataGrouping", Universal.Constants.DataGrouping.EuropeanUnion, tariffFindBox.GetDataGrouping());
				});
				
				manifest.AMA_RL_NKPortOfDischarge = "GBLON";

				CombineAssertions(() =>
				{
					AssertEquals("Tariff Column DataGrouping", Constants.CountryCodes.UnitedKingdom, tariffColumnStyle.GetDataGrouping());
					AssertEquals("TariffFindBox DataGrouping", Constants.CountryCodes.UnitedKingdom, tariffFindBox.GetDataGrouping());
				});
			}
		}

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(H7BillLayout);

		protected override Type ExpectedBillPartiesLayoutType => typeof(H7BillPartiesLayout);

		protected override Type ExpectedH7ItemDetailsLayoutsType => typeof(H7ItemDetailsLayout);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[]
		{
			typeof(H7PackTabUserControl),
			typeof(EUH7ItemUserControl),
			typeof(AdditionalInfoUserControl),
			typeof(SupportingDocumentsUserControl),
			typeof(H7PreviousDocumentsUserControl),
		};

		protected override IReadOnlyList<Type> ExpectedItemAdditionalTabPageUserControls => new[]
		{
			typeof(EUH7ItemPacksUserControl),
			typeof(AdditionalInfoUserControl),
			typeof(H7SupportingDocumentsUserControl),
			typeof(H7PreviousDocumentsUserControl),
		};

		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => Enumerable.Empty<Type>();

		protected override IReadOnlyList<(string, Type)> ExpectedItemsGridColumnInfos => new[]
		{
			(AsycudaPackedItem.Schema.API_FormattedTariff, typeof(Universal.GUI.TariffColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GoodsValue, typeof(ZCalcEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency, typeof(ZCodeFindBoxColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GoodsDescription, typeof(ZTextBoxColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin, typeof(ZCodeFindBoxColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_CustomsQty2, typeof(ZCalcEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_CustomsUQ2, typeof(ZDropEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GrossWeight, typeof(ZCalcEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GrossWeightUQ, typeof(ZDropEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_NetWeight, typeof(ZCalcEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_NetWeightUQ, typeof(ZDropEditColumnStyleInfo))
		};

		protected override IList<KeyValuePair<string, Type>> ExpectedControlsOnItemDetailsTab => new[]
		{
			new KeyValuePair<string, Type>("TariffFindBox", typeof(Universal.GUI.TariffFindBox)),
			new KeyValuePair<string, Type>("IntrinsicValueConvertToLocalCurrencyControl", typeof(ConvertToLocalCurrencyControl)),
			new KeyValuePair<string, Type>("SupplementaryDropEdit", typeof(ZCalcDropEdit)),
			new KeyValuePair<string, Type>("GrossWeightCalcDropEdit", typeof(ZCalcDropEdit)),
			new KeyValuePair<string, Type>("NetWeightCalcDropEdit", typeof(ZCalcDropEdit)),
			new KeyValuePair<string, Type>("GoodsOriginCodeFindBox", typeof(ZCodeFindBox)),
			new KeyValuePair<string, Type>("GoodsDescriptionTextBox", typeof(ZTextBox)),
			new KeyValuePair<string, Type>("CustomEntriesSeparatorUserControl", typeof(SeparatorUserControl)),
			new KeyValuePair<string, Type>("CustomEntriesGrid", typeof(ZGrid)),
			new KeyValuePair<string, Type>("ColumnSeparator0", typeof(ZPanel)),
		};

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			var shipmentTypeColumnStyleInfo = columnInfos.First(i => i.ColumnName == AsycudaBill.Schema.ABL_ShipmentType);
			Assert(shipmentTypeColumnStyleInfo.IsVisible);
		}
	}
}
