using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestsSubclassesOf(typeof(H7ApplicationGUIProvider))]
	public abstract class H7ApplicationGUIProviderAbstractTest<T, THeader> : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<T, THeader>
		where T : H7ApplicationGUIProvider
		where THeader : AsycudaManifestHeader
	{
		protected override Form GetFormToBashCore()
		{
			var result = (ManifestForm)base.GetFormToBashCore();
			result.ControllerID = ControllerIDs.Customs.EU.EUH7;
			return result;
		}

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(EUH7BillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(EUH7BillPartiesLayouts);

		protected virtual Type ExpectedH7ItemDetailsLayoutsType => typeof(EUH7ItemDetailsLayouts);

		protected virtual Type ExpectedH7PackDetailsLayoutsType => typeof(EUH7PackDetailsLayouts);

		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => new[]
		{
			typeof(AdditionalDocumentsUserControl),
			typeof(SupportingDocumentsUserControl),
			typeof(PreviousDocumentsUserControl),
		};

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[]
		{
			typeof(EUH7PackUserControl),
			typeof(EUH7ItemUserControl),
			typeof(AdditionalDocumentsUserControl),
			typeof(SupportingDocumentsUserControl),
			typeof(PreviousDocumentsUserControl),
		};

		protected virtual IReadOnlyList<Type> ExpectedItemAdditionalTabPageUserControls => new[]
		{
			typeof(EUH7ItemPacksUserControl),
			typeof(AdditionalDocumentsUserControl),
			typeof(SupportingDocumentsUserControl),
			typeof(PreviousDocumentsUserControl),
		};

		protected override IEnumerable<ControllerID> ExpectedBillPluginsControllerIDs => new[] { ControllerIDs.eDocsPlugIn };

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override string ExpectedMenuCaptionEnglishText => "&Messaging";

		protected override bool ExpectedShouldPositionMessagesTabAccordingToMessageLevel => true;

		protected override string ExpectedGetBillFormCaption => "Bill TESTBILL1";

		protected override Type ExpectedBillMessagesUserControl => typeof(EUH7MessagesUserControl);

		public void TestGetActionsExtraMenuItemsCore()
		{
			var header = CreateNewManifest();
			var provider = (H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var menuItems = provider.GetActionsExtraMenuItems();
			var standAloneDeclarationItem = menuItems.FindByText("Convert to Stand Alone Declaration");
			AssertNotNull("Convert to Stand Alone Declaration", standAloneDeclarationItem);
		}

		[RequiresSTA]
		public void TestStandAloneDeclarationActionMenuItem()
		{
			var header = CreateNewManifest();
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var actionMenuItem = GetActionMenuItem(form);
				var standAloneDeclarationItem = actionMenuItem.MenuItems.FindByText("Convert to Stand Alone Declaration");
				AssertNotNull("Convert to Stand Alone Declaration Menu Item exists", standAloneDeclarationItem);
				standAloneDeclarationItem.PerformClick();

				Assert(ZFormModaliser.LastFormShownDialogForTest is EmbeddedModulePopup);

				var lastShownModuleID = ((EmbeddedModulePopup)ZFormModaliser.LastFormShownDialogForTest).CurrentModule.ModuleID;
				AssertEquals(ModuleIDs.Customs.EU.EUH7Bill, lastShownModuleID);
			}
		}

		MenuItem GetActionMenuItem(ManifestForm form)
		{
			var menuItems = form.Menu.MenuItems.Cast<MenuItem>();

			var menuItem = menuItems.FirstOrDefault(item => item.Text == "Actio&ns");
			
			return menuItem;
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			var messageReferenceNumberColumnStyleInfo = columnInfos.First(i => i.ColumnName == AsycudaBill.Schema.MovementReferenceNumber);
			Assert(messageReferenceNumberColumnStyleInfo.IsVisible);

			var goodsValueColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_GoodsValue);
			Assert(goodsValueColumnStyleInfo.IsVisible);

			var goodsValueCurrencyColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency);
			Assert(goodsValueCurrencyColumnStyleInfo.IsVisible);

			var incotermColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_Incoterm);
			Assert(!incotermColumnStyleInfo.IsVisible);

			var procedureColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_Procedure);
			Assert(procedureColumnStyleInfo.IsVisible);

			var standAloneDeclarationReferenceColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == "EntrySummaryReferenceNumber");
			Assert(standAloneDeclarationReferenceColumnStyleInfo.IsVisible);

			var containerNumberColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == "ContainerNumber");
			Assert(!containerNumberColumnStyleInfo.IsVisible);

			var containerModeColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == "ABL_ContainerMode");
			Assert(!containerModeColumnStyleInfo.IsVisible);

			var messageStatusColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_MessageStatus);
			Assert(messageStatusColumnStyleInfo.IsReadOnly);

			var customStatusColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_BillStatus);
			Assert(customStatusColumnStyleInfo.IsReadOnly);
		}

		public void TestGetItemsGridColumnAvailability()
		{
			var header = CreateNewManifest();
			var columnAvailability = ((H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(header)).GetItemsGridColumnAvailability(header);

			if (!columnAvailability.Any())
			{
				Assert(true);
			}

			foreach (var availabiltyInfo in columnAvailability)
			{
				AssertContainsExactElementsInAnyOrder(
					string.Format("Columns expected to be {0}", availabiltyInfo.Key ? "available" : "unavailable"),
					ExpectedItemsGridAvailability[availabiltyInfo.Key],
					availabiltyInfo.Value);
			}
		}

		protected virtual IDictionary<bool, string[]> ExpectedItemsGridAvailability => new Dictionary<bool, string[]>();

		public void TestItemAdditionalTabPageUserControls()
		{
			var header = CreateNewManifest();
			var additionalTabPageTypes = new List<Type>();
			foreach (var tabPageUserControl in ((H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(header)).GetItemAdditionalTabPageUserControl().OrderBy(t => t.TabPageSequence))
			{
				using (tabPageUserControl)
				{
					additionalTabPageTypes.Add(tabPageUserControl?.GetType());
				}
			}

			AssertSequencesEqual(additionalTabPageTypes, ExpectedItemAdditionalTabPageUserControls);
		}

		public void TestGetH7ItemDetailsLayout()
		{
			var header = CreateNewManifest();
			var provider = (H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var layout = provider.GetH7ItemDetailsLayout();
			AssertType(ExpectedH7ItemDetailsLayoutsType, layout);
		}

		public void TestGetH7PackDetailsLayout()
		{
			var header = CreateNewManifest();
			var provider = (H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var layout = provider.GetH7PackDetailsLayout();
			AssertType(ExpectedH7PackDetailsLayoutsType, layout);
		}

		[RequiresSTA]
		public void TestItemDetailsDynamicPanelControls()
		{
			var header = CreateNewManifest();
			var bill = header.Bills.AddNew();
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var itemTabControl = itemTabpage.FindSingle<EUH7ItemTabControl>();
				var itemDetailsUserControl = itemTabControl.FindSingle<EUH7ItemDetailsUserControl>();
				var dynamicPackedItemDetailsPanel = itemDetailsUserControl.FindSingle<DynamicLayoutPanel>();

				var allControls = dynamicPackedItemDetailsPanel.Controls
					.Cast<Control>()
					.Where(c => c.Visible)
					.Select(c => new KeyValuePair<string, Type>(c.Name, c.GetType()));

				AssertContainsExactElementsInAnyOrder("Controls in Packed Item Details tab page should be visible as expected.", ExpectedControlsOnItemDetailsTab, allControls);
			}

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var billsAndPacksTabControl = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var itemTabControl = itemTabpage.FindSingle<EUH7ItemTabControl>();
				var itemDetailsUserControl = itemTabControl.FindSingle<EUH7ItemDetailsUserControl>();
				var dynamicPackedItemDetailsPanel = itemDetailsUserControl.FindSingle<DynamicLayoutPanel>();

				var allControls = dynamicPackedItemDetailsPanel.Controls
					.Cast<Control>()
					.Where(c => c.Visible)
					.Select(c => new KeyValuePair<string, Type>(c.Name, c.GetType()));

				AssertContainsExactElementsInAnyOrder("Controls in Packed Item Details tab page should be visible as expected.", ExpectedControlsOnItemDetailsTab, allControls);
			}
		}

		protected virtual IList<KeyValuePair<string, Type>> ExpectedControlsOnItemDetailsTab => new[]
		{
			new KeyValuePair<string, Type>("TariffFindBox", typeof(Universal.GUI.TariffFindBox)),
			new KeyValuePair<string, Type>("IntrinsicValueConvertToLocalCurrencyControl", typeof(ConvertToLocalCurrencyControl)),
			new KeyValuePair<string, Type>("SupplementaryDropEdit", typeof(ZCalcDropEdit)),
			new KeyValuePair<string, Type>("GrossWeightCalcDropEdit", typeof(ZCalcDropEdit)),
			new KeyValuePair<string, Type>("GoodsOriginCodeFindBox", typeof(ZCodeFindBox)),
			new KeyValuePair<string, Type>("GoodsDescriptionTextBox", typeof(ZTextBox)),
			new KeyValuePair<string, Type>("CustomEntriesSeparatorUserControl", typeof(SeparatorUserControl)),
			new KeyValuePair<string, Type>("CustomEntriesGrid", typeof(ZGrid)),
			new KeyValuePair<string, Type>("ColumnSeparator0", typeof(ZPanel)),
		};

		public void TestItemGridColumnVisibility()
		{
			var header = CreateNewManifest();
			var bill = header.Bills.AddNew();
			bill.Packs.AddNew();
			bill.PackedItems.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var grid = itemTabpage.FindSingle<ZGrid>("ItemsGrid");

				EUH7GUITestHelper.AssertGridLayout(grid, ExpectedItemsGridColumnInfos);
			}

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var billsAndPacksTabControl = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var grid = itemTabpage.FindSingle<ZGrid>("ItemsGrid");

				EUH7GUITestHelper.AssertGridLayout(grid, ExpectedItemsGridColumnInfos);
			}
		}

		protected virtual IReadOnlyList<(string, Type)> ExpectedItemsGridColumnInfos => new[]
		{
			(AsycudaPackedItem.Schema.API_FormattedTariff, typeof(Universal.GUI.TariffColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GoodsValue, typeof(ZCalcEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency, typeof(ZCodeFindBoxColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin, typeof(ZCodeFindBoxColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GoodsDescription, typeof(ZTextBoxColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_CustomsQty2, typeof(ZCalcEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_CustomsUQ2, typeof(ZDropEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GrossWeight, typeof(ZCalcEditColumnStyleInfo)),
			(AsycudaPackedItem.Schema.API_GrossWeightUQ, typeof(ZDropEditColumnStyleInfo)),
		};
	}

	[TestedType(typeof(H7ApplicationGUIProvider))]
	sealed class H7ApplicationGUIProviderTest : H7ApplicationGUIProviderAbstractTest<H7ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestType()
		{
			AssertType<H7ApplicationGUIProvider>(CreateNewGuiProvider());
		}

		public void TestCustomizeBillsGridCore_OpenBillFormWhenDoubleClickOnBillGrid()
		{
			var header = CreateNewManifest();
			header.Bills.AddNew();
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				Assert("Precondition: no opened bill form", !Application.OpenForms.OfType<AsycudaBillForm>().Any());

				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.PerformMouseDoubleClickForTest(0);

				using (var billForm = Application.OpenForms.OfType<AsycudaBillForm>().SingleOrDefault())
				{
					AssertNotNull("Asycuda bill form opened", billForm);
				}
			}
		}

		public void TestCustomizeBillGridCore_SeeConvertToStandAloneDeclarationMenuWhenRightClickOnBillGrid()
		{
			var header = CreateNewManifest();
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.OnPopup_CallForTesting();

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				AssertNotNull("Convert To Stand Alone action menu should be added in the menu item list", convertMenu);
			}
		}

		public void TestFreightValueAvailability()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					Assert("ABL_FreightValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_FreightValue).IsUnavailable);
					Assert("ABL_RX_NKFreightValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsUnavailable);
				});
			}
		}

		[RequiresSTA]
		public void TestTransportValueCaptions()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Transport Value", billsGrid.Columns[AsycudaBill.Schema.ABL_TransportValue].ColumnStyle.HeaderText);
					AssertEquals("Transport Value", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_TransportValue).GroupName.Caption);
					AssertEquals("Transport Value", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency).GroupName.Caption);
				});
			}
		}

		[TestDate(2025, 6, 6)]
		public void TestCustomizeItemsGridCoreSetsTariffProperties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var manifest = CreateNewManifest();
				manifest.Bills.AddNew();

				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;

					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
					billsAndPacksTabControl.SelectedTab = itemTabpage;

					var itemsGrid = itemTabpage.FindSingle<ZGrid>("ItemsGrid");
					var tariffColumnStyle = itemsGrid.ColumnStyles.ToArray().FirstOrDefault(x => x is Universal.GUI.TariffColumnStyleInfo) as Universal.GUI.TariffColumnStyleInfo;

					CombineAssertions(() =>
					{
						AssertEquals("Tariff Column DataGrouping", Core.Constants.CountryCodes.Spain, tariffColumnStyle.GetDataGrouping());
						AssertEquals("Tariff Column Effective Date", new ZDateTime(2025, 6, 6), tariffColumnStyle.GetEffectiveDate());
						AssertEquals("Tariff Column Tariff Type", UniversalReferenceConstants.CusTariffTypes.ImportTariff, tariffColumnStyle.GetTariffType());
					});
				}
			}
		}

		[TestDate(2025, 6, 6)]
		public void TestCustomizeTariffFindBoxCore()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var manifest = CreateNewManifest();
				manifest.Bills.AddNew();

				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;

					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
					billsAndPacksTabControl.SelectedTab = itemTabpage;

					var tariffFindBox = billsAndPacksTabControl.FindSingle<Universal.GUI.TariffFindBox>("TariffFindBox");

					CombineAssertions(() =>
					{
						AssertEquals("TariffFindBox DataGrouping", Core.Constants.CountryCodes.Spain, tariffFindBox.GetDataGrouping());
						AssertEquals("TariffFindBox Effective Date", new ZDateTime(2025, 6, 6), tariffFindBox.GetEffectiveDate());
						AssertEquals("TariffFindBox Tariff Type", UniversalReferenceConstants.CusTariffTypes.ImportTariff, tariffFindBox.GetTariffType());
					});
				}
			}
		}
	}
}
