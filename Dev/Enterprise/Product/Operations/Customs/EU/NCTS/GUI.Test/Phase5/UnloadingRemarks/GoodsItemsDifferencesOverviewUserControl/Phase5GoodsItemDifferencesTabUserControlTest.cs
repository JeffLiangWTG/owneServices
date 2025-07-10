using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemDifferencesTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("Phase5GoodsItemDifferencesTabUserControl BindingSource DataSourceType", typeof(INctsArrivalCargoDescCollection<NctsArrivalCargoDesc>), userControl.BindingSource.DataSourceType);
		}

		public void TestGoodsItemDifferencesGridUserControl()
		{
			var goodsItemDifferencesGridUserControl = userControl.GoodsItemDifferencesGridUserControl;
			CombineAssertions(() =>
			{
				AssertType<Phase5GoodsItemDifferencesGridUserControl>("Type", goodsItemDifferencesGridUserControl);
				AssertEquals("Within GoodsItemDifferencesSplitContainer.Panel1", true, userControl.GoodsItemDifferencesSplitContainer.Panel1.Controls.Contains(goodsItemDifferencesGridUserControl));
				AssertEquals("BindTo", ".", goodsItemDifferencesGridUserControl.GetBindingMember());
			});
		}

		public void TestGoodsItemDifferencesTabControl()
		{
			CombineAssertions(() =>
			{
				var tabControl = userControl.GoodsItemDifferencesTabControl;
				AssertEquals("Within GoodsItemDifferencesSplitContainer.Panel2", true, userControl.GoodsItemDifferencesSplitContainer.Panel2.Controls.Contains(tabControl));

				var tabPages = tabControl.TabPages;
				AssertArrayEqualsByElements(new[] { "ItemDetailsTabPage", "ItemPackagesTabPage", "ItemSupportingDocumentsTabPage", "ItemAdditionalDocumentsTabPage", "ItemPreviousDocumentsTabPage", "LiabilityCalculationTabPage" },
					tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
			});
		}

		public void TestItemPackagesTabPage()
		{
			var tabPage = userControl.GoodsItemDifferencesTabControl.FindSingle<ZTabPage>("ItemPackagesTabPage");
			CombineAssertions(() =>
			{
				AssertEquals("ItemPackagesTabPage visible", true, tabPage.TabVisible);
				AssertEquals("Caption", "Packages && Containers", tabPage.CaptionResourceString.Caption);
			});
		}

		public void TestGoodItemsAdditionalDocuments()
		{
			var phase5GoodItemsAdditionalDocumentPanelUserControl = userControl.Phase5GoodItemsAdditionalDocumentPanelUserControl;
			AssertType<Phase5GoodItemsAdditionalDocumentPanelUserControl>("Type", phase5GoodItemsAdditionalDocumentPanelUserControl);
		}

		public void TestContainersGroupBox()
		{
			var containersGroupBox = userControl.ContainersGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Containers", containersGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", DockStyle.Right, containersGroupBox.Dock);
			});
		}

		public void TestPackagesGroupBox()
		{
			var packagesGroupBox = userControl.PackagesGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Packages", packagesGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", DockStyle.Fill, packagesGroupBox.Dock);
			});
		}

		public void TestNctsContainerGridUserControl()
		{
			var nctsContainerGridUserControl = userControl.NctsContainerGridUserControl;
			CombineAssertions(() =>
			{
				AssertType<NctsContainerGridUserControl>("Type", nctsContainerGridUserControl);
				AssertEquals("Within ContainersGroupBox", true, userControl.ContainersGroupBox.Controls.Contains(nctsContainerGridUserControl));
				AssertEquals("BindTo", "Packages", nctsContainerGridUserControl.GetBindingMember());
			});
		}

		public void TestGoodsItemDifferencesSplitContainer()
		{
			AssertEquals("Minimum size of panel 2", 300, userControl.GoodsItemDifferencesSplitContainer.Panel2MinSize);
		}

		public void TestGoodsItemDifferencesDetailsDynamicLayoutPanel()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			userControl.SetDataBinding(header.Bills, "ArrivalGoodsItems");

			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();

				var goodsItemDifferencesDetailsDynamicLayoutPanel = userControl.GoodsItemDifferencesDetailsDynamicLayoutPanel;

				CombineAssertions(() =>
				{
					AssertEquals("BindingMember", ".", goodsItemDifferencesDetailsDynamicLayoutPanel.GetBindingMember());

					DynamicLayoutPanelTest.AssertControlsOrder(goodsItemDifferencesDetailsDynamicLayoutPanel,
						nameof(Phase5GoodsItemDifferencesDetailsControlBag.SequenceNumberTextBox),
						nameof(Phase5GoodsItemDifferencesDetailsControlBag.ItemNumberTextBox));
				});
			}
		}

		[RequiresSTA]
		public void TestGoodsItemDifferencesDetailsColumnDynamicLayoutPanel()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			bill.ArrivalGoodsItems.AddNew();
			userControl.SetDataBinding(header.Bills, "ArrivalGoodsItems");

			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();

				var goodsItemDifferencesDetailsColumnDynamicLayoutPanel = userControl.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel;

				CombineAssertions(() =>
				{
					AssertEquals("BindingMember", ".", goodsItemDifferencesDetailsColumnDynamicLayoutPanel.GetBindingMember());

					DynamicLayoutPanelTest.AssertControlsOrder(goodsItemDifferencesDetailsColumnDynamicLayoutPanel,
						nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredValueLabel),
						nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredCommodityCodeCodeFindBox),
						nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredCusCodeCodeFindBox),
						nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredDescriptionTextBox),
						nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredGrossWeightDropEdit),
						nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredNetWeightDropEdit));
				});
			}
		}

		public void TestItemPreviousDocumentsTabPage()
		{
			var tabPage = userControl.GoodsItemDifferencesTabControl.FindSingle<ZTabPage>("ItemPreviousDocumentsTabPage");
			CombineAssertions(() =>
			{
				AssertEquals("ItemPreviousDocumentsTabPage visible", true, tabPage.TabVisible);
				AssertEquals("Caption", "Previous Documents", tabPage.CaptionResourceString.Caption);
			});
		}

		public void TestGoodsItemPreviousDocuments()
		{
				var phase5GoodItemsAdditionalDocumentPanelUserControl = userControl.GoodsItemPreviousDocumentsPanelUserControl;
				AssertType<HouseConsignmentPreviousDocumentsPanelUserControl>("Type", phase5GoodItemsAdditionalDocumentPanelUserControl);
		}

		public void TestLiabilityCalculationTabPage()
		{
			var tabPage = userControl.GoodsItemDifferencesTabControl.FindSingle<ZTabPage>("LiabilityCalculationTabPage");
			AssertEquals("Caption", "Liability Calculation", tabPage.CaptionResourceString.Caption);
		}

		public void TestLiabilityCalculationTabPageVisibility()
		{
			CombineAssertions(() =>
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, arrivalHeader);
				var userControl = new Phase5GoodsItemDifferencesTabUserControl();
				userControl.SetDataBinding(arrivalHeader.Bills, "ArrivalGoodsItems");
				using var form = new ZForm(arrivalHeader);

				form.Controls.Add(userControl);
				form.Show();

				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, false))
				{
					var tabPage = userControl.GoodsItemDifferencesTabControl.FindSingleOrDefault<ZTabPage>("LiabilityCalculationTabPage");
					AssertNull("LiabilityCalculationTabPage is null when IsLiabilityCalculationForArrivalSupported is false", tabPage);
				}

				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, true))
				{
					var tabPage = userControl.GoodsItemDifferencesTabControl.FindSingleOrDefault<ZTabPage>("LiabilityCalculationTabPage");
					AssertNull("LiabilityCalculationTabPage is still null until State is change", tabPage);

					arrivalGoodsItem.BY_UnloadedState = "DIF";
					tabPage = userControl.GoodsItemDifferencesTabControl.FindSingleOrDefault<ZTabPage>("LiabilityCalculationTabPage");
					AssertNotNull("LiabilityCalculationTabPage is not null when IsLiabilityCalculationForArrivalSupported is true", tabPage);
					AssertEquals("LiabilityCalculationTabPage is visible when IsLiabilityCalculationForArrivalSupported is true", true, tabPage.TabVisible);
				}
			});
		}

		[RequiresSTA]
		public void TestLiabilityDynamicLayoutPanel()
		{
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, arrivalHeader);
				arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = "UAP";
				using (var form = new Phase5ArrivalMovementForm(arrivalHeader))
				{
					form.Show();

					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					mainTabControl.SelectTab(form.UnloadingRemarksTabPage);

					var unloadingRemarksTabUserControl = form.FindSingle<Phase5UnloadingRemarksTabUserControl>();
					var houseConsignmentDifferencesTabPage = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage;
					unloadingRemarksTabUserControl.UnloadingRemarksTabControl.SelectTab(houseConsignmentDifferencesTabPage);

					var houseConsignmentDifferencesTabUserControl = (HouseConsignmentDifferencesTabUserControl)unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage.Controls[0];
					var goodsItemTabPage = houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage;
					houseConsignmentDifferencesTabUserControl.HouseConsignmentDifferencesTabControl.SelectTab(goodsItemTabPage);

					var phase5GoodsItemDifferencesTabUserControl = (Phase5GoodsItemDifferencesTabUserControl)houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage.Controls[0];
					var liabilityCalculationTabPage = phase5GoodsItemDifferencesTabUserControl.LiabilityCalculationTabPage;
					phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesTabControl.SelectTab(liabilityCalculationTabPage);

					var liabilityCalculationDynamicLayoutPanel = phase5GoodsItemDifferencesTabUserControl.LiabilityCalculationDynamicLayoutPanel;

					CombineAssertions(() =>
					{
						AssertEquals("BindingMember", ".", liabilityCalculationDynamicLayoutPanel.GetBindingMember());

						DynamicLayoutPanelTest.AssertControlsOrder(liabilityCalculationDynamicLayoutPanel,
							nameof(LiabilityDetailsControlBag.CountryOfOriginDropEdit),
							nameof(LiabilityDetailsControlBag.FeesUserControl),
							nameof(LiabilityDetailsControlBag.CommodityCodeTariffFindBox),
							nameof(LiabilityDetailsControlBag.SupplementaryUnitsCalcDropEdit),
							nameof(LiabilityDetailsControlBag.CustomsThirdQuantityDropEdit),
							nameof(LiabilityDetailsControlBag.CustomsFourthQuantityDropEdit),
							nameof(LiabilityDetailsControlBag.CustomsValueCalcDropEdit),
							nameof(LiabilityDetailsControlBag.AdditionalSupplementaryCodesUserControl));
					});
				}
			}
		}

		[RequiresSTA]
		public void TestLiabilityCalculationTabPageVisibilityWhenValueChange()
		{
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, arrivalHeader);
				arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = "UAP";
				arrivalGoodsItem.BY_UnloadedState = "MIS";
				using (var form = new Phase5ArrivalMovementForm(arrivalHeader))
				{
					form.Show();

					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					mainTabControl.SelectTab(form.UnloadingRemarksTabPage);

					var unloadingRemarksTabUserControl = form.FindSingle<Phase5UnloadingRemarksTabUserControl>();
					var houseConsignmentDifferencesTabPage = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage;
					unloadingRemarksTabUserControl.UnloadingRemarksTabControl.SelectTab(houseConsignmentDifferencesTabPage);

					var houseConsignmentDifferencesTabUserControl = (HouseConsignmentDifferencesTabUserControl)unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage.Controls[0];
					var goodsItemTabPage = houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage;
					houseConsignmentDifferencesTabUserControl.HouseConsignmentDifferencesTabControl.SelectTab(goodsItemTabPage);

					var phase5GoodsItemDifferencesTabUserControl = (Phase5GoodsItemDifferencesTabUserControl)houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage.Controls[0];
					var liabilityCalculationTabPage = phase5GoodsItemDifferencesTabUserControl.LiabilityCalculationTabPage;

					CombineAssertions(() =>
					{
						AssertEquals("LiabilityCalculationTabPage is not visible when IsLiabilityCalculationForArrivalSupported is false", false, liabilityCalculationTabPage.TabVisible);

						arrivalGoodsItem.BY_UnloadedState = "DIF";
						AssertEquals("LiabilityCalculationTabPage is visible when IsLiabilityCalculationForArrivalSupported is true", true, liabilityCalculationTabPage.TabVisible);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemDifferencesTabUserControl();

			arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalGoodsItem = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			arrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		}
		Phase5GoodsItemDifferencesTabUserControl userControl;
		NctsHeader arrivalHeader;
		NctsArrivalCargoDesc arrivalGoodsItem;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
