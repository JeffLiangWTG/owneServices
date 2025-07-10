using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemsTabUserControlTest : TestCaseWithFactory
	{
		public void TestInitializeAdditionalTabs()
		{
			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new NctsPhase5LayoutProviderForAdditionalTabsTest()) }
			};

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			using (var userControl = new Phase5GoodsItemsTabUserControl())
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				userControl.SetDataBinding(nctsHeader, "");

				CombineAssertions(() =>
				{
					AssertEquals("There are 2 additional tabs", 2, userControl.GoodsItemTabControl.AllTabPages.Count(t => t.Name.StartsWith("AdditionalTabPageForTest")));

					Phase5LayoutTestHelper.AssertAdditionalTabPage<AdditionalTabPageForTest1UserControl>(userControl.GoodsItemTabControl, nameof(AdditionalTabPageForTest1), 1);
					Phase5LayoutTestHelper.AssertAdditionalTabPage<AdditionalTabPageForTest2UserControl>(userControl.GoodsItemTabControl, nameof(AdditionalTabPageForTest2), 2);
				});
			}
		}

		[RequiresSTA]
		public void TestInitializeReorderTabs()
		{
			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new NctsPhase5LayoutProviderForReorderTabsTest()) }
			};

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			using (var userControl = new Phase5GoodsItemsTabUserControl())
			{
				CombineAssertions(() =>
				{
					var goodsItemTabControl = userControl.GoodsItemTabControl;
					AssertNotNull("Before binding, has GoodsItemSupportingDocumentsTabPage", goodsItemTabControl.GetTabPage(nameof(Phase5GoodsItemsTabUserControl.GoodsItemSupportingDocumentsTabPage)));
					AssertNotNull("Before binding, has GoodsItemPreviousDocumentsTabPage", goodsItemTabControl.GetTabPage(nameof(Phase5GoodsItemsTabUserControl.GoodsItemPreviousDocumentsTabPage)));

					var nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					userControl.SetDataBinding(nctsHeader, "");

					AssertNotNull("After binding, GoodsItemSupportingDocumentsTabPage not removed", goodsItemTabControl.GetTabPage(nameof(Phase5GoodsItemsTabUserControl.GoodsItemSupportingDocumentsTabPage)));
					AssertNull("After binding, GoodsItemPreviousDocumentsTabPage removed", goodsItemTabControl.GetTabPage(nameof(Phase5GoodsItemsTabUserControl.GoodsItemPreviousDocumentsTabPage)));

					AssertSequencesEqual("TabPageNames",
					new[]
					{
						"AdditionalTabPageForTest1", "GoodsItemDetailsTabPage", "GoodsItemPackagesAndContainersTabPage", "GoodsItemSupportingDocumentsTabPage", "GoodsItemAdditionalDocumentsTabPage", "GoodsItemSupplyChainActorsTabPage", "AdditionalTabPageForTest2"
					}, userControl.GoodsItemTabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name));
				});
			}
		}

		public void TestGoodsItemsSplitContainer()
		{
			CombineAssertions(() =>
			{
				var goodsItemsSplitContainer = userControl.GoodsItemsSplitContainer;
				AssertEquals("GoodsItemsSplitContainer is within Phase5GoodsItemsTabUserControl", true, userControl.Contains(goodsItemsSplitContainer));
				AssertEquals("GoodsItemsSplitContainer.Orientation", Orientation.Horizontal, goodsItemsSplitContainer.Orientation);
				AssertEquals("GoodsItemsSplitContainer.SplitterDistance", ControlDpiScalingHelper.ScaleToCurrentDpiY(159), goodsItemsSplitContainer.SplitterDistance);
				AssertEquals("GoodsItemsSplitContainer.Panel2MinSize", ControlDpiScalingHelper.ScaleToCurrentDpiY(310), goodsItemsSplitContainer.Panel2MinSize);
				AssertEquals("GoodsItemsSplitContainer.Panel1MinSize", ControlDpiScalingHelper.ScaleToCurrentDpiY(10), goodsItemsSplitContainer.Panel1MinSize);
				AssertEquals("Dock", DockStyle.Fill, goodsItemsSplitContainer.Dock);
			});
		}

		public void TestGoodsItemTabControl()
		{
			var goodsItemTabControl = userControl.GoodsItemTabControl;
			CombineAssertions(() =>
			{
				AssertEquals("GoodsItemTabControl is within GoodsItemsSplitContainer.Panel2", true, userControl.GoodsItemsSplitContainer.Panel2.Contains(goodsItemTabControl));
				AssertEquals("Dock", DockStyle.Fill, goodsItemTabControl.Dock);
			});
		}

		[RequiresSTA]
		public void TestGoodsItemDetailsTabPage()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var form = new Phase5DepartureMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
				var goodsItemsTabPage = houseConsignmentsTabUserControl.GoodsItemsTabPage;
				houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.GoodsItemsTabPage.Controls[0];
				var goodsItemDetailsTabPage = phase5GoodsItemsTabUserControl.GoodsItemDetailsTabPage;
				phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(goodsItemDetailsTabPage);

				var dynamicGoodsItemDetailsPanel = phase5GoodsItemsTabUserControl.DynamicGoodsItemDetailsPanel;
				CombineAssertions(() =>
				{
					AssertEquals("GoodsItemDetailsTabPage.Caption", "Item Details", goodsItemDetailsTabPage.CaptionResourceString.Caption);

					AssertEquals("DynamicGoodsItemDetailsPanel.AutoScroll", true, dynamicGoodsItemDetailsPanel.AutoScroll);
					AssertEquals("DynamicGoodsItemDetailsPanel.Dock", DockStyle.Fill, dynamicGoodsItemDetailsPanel.Dock);
					DynamicLayoutPanelTest.AssertControlsOrder(dynamicGoodsItemDetailsPanel,
						nameof(GoodsItemDetailsControlBag.ItemNumberTextBox),
						nameof(GoodsItemDetailsControlBag.CountryOfOriginDropEdit),
						nameof(GoodsItemDetailsControlBag.FeesUserControl),
						nameof(GoodsItemDetailsControlBag.DeclarationGoodsItemNumberTextBox),
						nameof(GoodsItemDetailsControlBag.CommercialReferenceNumberTextBox),
						nameof(GoodsItemDetailsControlBag.DescriptionOfGoodsTextBox),
						nameof(GoodsItemDetailsControlBag.TransportChargesMethodOfPaymentDropEdit),
						nameof(GoodsItemDetailsControlBag.GrossWeightCalcDropEdit),
						nameof(GoodsItemDetailsControlBag.UNDangerousGoodsUserControl),
						nameof(GoodsItemDetailsControlBag.NetWeightCalcDropEdit),
						nameof(GoodsItemDetailsControlBag.CusC4NumberCodeFindBox),
						nameof(GoodsItemDetailsControlBag.CommodityCodeTariffFindBox),
						nameof(GoodsItemDetailsControlBag.CustomsQuantityDropEdit),
						nameof(GoodsItemDetailsControlBag.ConsigneeDocAddressControl),
						nameof(GoodsItemDetailsControlBag.SupplementaryUnitsCalcDropEdit),
						nameof(GoodsItemDetailsControlBag.CustomsThirdQuantityDropEdit),
						nameof(GoodsItemDetailsControlBag.DeclarationTypeDropEdit),
						nameof(GoodsItemDetailsControlBag.CustomsFourthQuantityDropEdit),
						nameof(GoodsItemDetailsControlBag.CountryOfDispatchDropEdit),
						nameof(GoodsItemDetailsControlBag.LinePriceCalcDropEdit),
						nameof(GoodsItemDetailsControlBag.CountryOfDestinationDropEdit),
						nameof(GoodsItemDetailsControlBag.CustomsValueCalcDropEdit),
						nameof(GoodsItemDetailsControlBag.TaxOrFeeDropEdit),
						nameof(GoodsItemDetailsControlBag.AdditionalSupplementaryCodesUserControl));

					AssertEquals("BindToOrganisations corrected", "Bills.GoodsItems.Lookups.ConsigneeList",
						((MasterFiles.GUI.ZDocAddressControl)phase5GoodsItemsTabUserControl.Controls.Find(nameof(GoodsItemDetailsControlBag.ConsigneeDocAddressControl), true)[0]).BindToOrganisations);
				});
			}
		}

		[RequiresSTA]
		public void TestLayoutProvider()
		{
			foreach (var countryCode in new[] { Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Spain })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

					using (var form = new Phase5DepartureMovementForm(nctsHeader))
					{
						form.Show();

						var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
						mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

						var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
						var goodsItemsTabPage = houseConsignmentsTabUserControl.GoodsItemsTabPage;
						houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(goodsItemsTabPage);

						var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.GoodsItemsTabPage.Controls[0];
						phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(phase5GoodsItemsTabUserControl.GoodsItemDetailsTabPage);

						var layoutProvider = typeof(Phase5GoodsItemsTabUserControl).GetProperty("LayoutProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(phase5GoodsItemsTabUserControl);
						AssertEquals($"Enterprise.Customs.{countryCode}.NCTS.GUI.NctsPhase5LayoutProvider", layoutProvider.GetType().FullName);
					}
				}
			}
		}

		public void TestGoodsItemPackagesAndContainersTabPage()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var form = new Phase5DepartureMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
				var goodsItemsTabPage = houseConsignmentsTabUserControl.GoodsItemsTabPage;
				houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.GoodsItemsTabPage.Controls[0];
				var itemPackagesAndContainersTabPage = phase5GoodsItemsTabUserControl.GoodsItemPackagesAndContainersTabPage;
				phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(itemPackagesAndContainersTabPage);

				var goodsItemPackagesAndContainersDynamicLayoutPanel = phase5GoodsItemsTabUserControl.GoodsItemPackagesAndContainersDynamicCreationUserControl.HostedControl;
				CombineAssertions(() =>
				{
					AssertEquals("GoodsItemPackagesAndContainersTabPage Caption", "Packages && Containers", itemPackagesAndContainersTabPage.CaptionResourceString.Caption);

					DynamicLayoutPanelTest.AssertControlsOrder(goodsItemPackagesAndContainersDynamicLayoutPanel,
						nameof(Phase5GoodsItemPackagesAndContainersUserControl.DynamicPackagesUserControl),
						nameof(Phase5GoodsItemPackagesAndContainersUserControl.ContainersUserControl));
				});
			}
		}

		public void TestGoodsItemSupportingDocumentsTabPage()
		{
			var supportingDocumentsTabPage = userControl.GoodsItemSupportingDocumentsTabPage;
			var supportingDocumentsTabUserControl = userControl.GoodsItemSupportingDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("GoodsItemSupportingDocumentsTabPage Caption", "Supporting Documents", supportingDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("GoodsItemSupportingDocumentsTabPage is within GoodsItemTabControl", true, userControl.GoodsItemTabControl.Contains(supportingDocumentsTabPage));

				AssertEquals("GoodsItemSupportingDocumentsTabUserControl is within GoodsItemSupportingDocumentsTabPage", true, supportingDocumentsTabPage.Controls.Contains(supportingDocumentsTabUserControl));
				AssertEquals("GoodsItemSupportingDocumentsTabUserControl Dock", DockStyle.Fill, supportingDocumentsTabUserControl.Dock);
				AssertEquals("GoodsItemSupportingDocumentsTabUserControl.BindingMember", nameof(NctsDepartureCargoDesc.SupportingDocuments), supportingDocumentsTabUserControl.GetBindingMember());
			});
		}

		public void TestGoodsItemPreviousDocumentsTabPage()
		{
			var previousDocumentsTabPage = userControl.GoodsItemPreviousDocumentsTabPage;
			var previousDocumentsTabUserControl = userControl.GoodsItemPreviousDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("GoodsItemPreviousDocumentsTabPage Caption", "Previous Documents", previousDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("GoodsItemPreviousDocumentsTabPage is within GoodsItemTabControl", true, userControl.GoodsItemTabControl.Contains(previousDocumentsTabPage));

				AssertEquals("GoodsItemPreviousDocumentsTabUserControl is within GoodsItemPreviousDocumentsTabPage", true, previousDocumentsTabPage.Controls.Contains(previousDocumentsTabUserControl));
				AssertEquals("GoodsItemPreviousDocumentsTabUserControl Dock", DockStyle.Fill, previousDocumentsTabUserControl.Dock);
				AssertEquals("GoodsItemPreviousDocumentsTabUserControl.BindingMember", nameof(NctsDepartureCargoDesc.PreviousDocuments), previousDocumentsTabUserControl.GetBindingMember());
			});
		}

		public void TestGoodsItemSupplyChainActorsTabPage()
		{
			var supplyChainActorsTabPage = userControl.GoodsItemSupplyChainActorsTabPage;
			var supplyChainActorsTabUserControl = userControl.GoodsItemSupplyChainActorsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("GoodsItemSupplyChainActorsTabPage.Caption", "Supply Chain Actors", supplyChainActorsTabPage.CaptionResourceString.Caption);
				AssertEquals("GoodsItemSupplyChainActorsTabPage is within GoodsItemTabControl", true, userControl.GoodsItemTabControl.TabPages.Contains(supplyChainActorsTabPage));

				AssertEquals("GoodsItemSupplyChainActorsTabUserControl is within GoodsItemSupplyChainActorsTabPage", true, supplyChainActorsTabPage.Controls.Contains(supplyChainActorsTabUserControl));
				AssertEquals("GoodsItemSupplyChainActorsTabUserControl.Dock", DockStyle.Fill, supplyChainActorsTabUserControl.Dock);
				AssertEquals("GoodsItemSupplyChainActorsTabUserControl.BindingMember", nameof(NctsDepartureCargoDesc.CusSupplyChainActorReferences), supplyChainActorsTabUserControl.GetBindingMember());
			});
		}

		[RequiresSTA]
		public void TestGoodsItemAdditionalDocumentsTabPage()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var form = new Phase5DepartureMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
				var goodsItemsTabPage = houseConsignmentsTabUserControl.GoodsItemsTabPage;
				houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.GoodsItemsTabPage.Controls[0];
				var goodsItemDetailsTabPage = phase5GoodsItemsTabUserControl.GoodsItemDetailsTabPage;
				phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(goodsItemDetailsTabPage);

				var goodsItemAdditionalDocumentsTabPage = phase5GoodsItemsTabUserControl.GoodsItemAdditionalDocumentsTabPage;
				var goodsItemAdditionalDocumentsControl = goodsItemAdditionalDocumentsTabPage.Controls[0];
				var additionalDocumentsTabPage = userControl.GoodsItemAdditionalDocumentsTabPage;
				CombineAssertions(() =>
				{
					AssertEquals("GoodsItemAdditionalDocumentsTabPage.Caption", "Additional Documents", goodsItemAdditionalDocumentsTabPage.CaptionResourceString.Caption);
					AssertEquals("GoodsItemAdditionalDocumentsTabPage is within GoodsItemTabControl", true, userControl.GoodsItemTabControl.TabPages.Contains(additionalDocumentsTabPage));

					AssertEquals("DynamicGoodsItemAdditionalDocumentsTabUserControl.Dock", DockStyle.Fill, goodsItemAdditionalDocumentsControl.Dock);
					AssertEquals("DynamicGoodsItemAdditionalDocumentsTabUserControl.Name", "GoodsItemAdditionalDocumentsTabUserControl", goodsItemAdditionalDocumentsControl.Name);
					AssertEquals("DynamicGoodsItemAdditionalDocumentsTabUserControl.BindingMember", "AdditionalInfos", goodsItemAdditionalDocumentsControl.GetBindingMember());
				});
			}
		}

		public void TestTabPagesOrder()
		{
			AssertSequencesEqual("TabPageNames", ExpectedTabPageNamesInOrder, userControl.GoodsItemTabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name));
		}

		[RequiresSTA]
		public void TestInitializeUNDGDataItemFormManager()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			using (var form = new Phase5DepartureMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
				var goodsItemsTabPage = houseConsignmentsTabUserControl.GoodsItemsTabPage;
				houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.GoodsItemsTabPage.Controls[0];
				var goodsItemDetailsTabPage = phase5GoodsItemsTabUserControl.GoodsItemDetailsTabPage;
				phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(goodsItemDetailsTabPage);

				var unDangerousGoodsUserControl = goodsItemDetailsTabPage.Controls[0].Controls.Find("UNDangerousGoodsUserControl", false).First();
				var unDangerousGoodsTextBox = unDangerousGoodsUserControl.Controls.Find("UNDangerousGoodsTextBox", false).First();

				CombineAssertions(() =>
				{
					var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
					var undg1 = goodsItem.UNDGs.AddNew();
					var undgPivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
					undgPivot1.DP_IsDefault = true;
					undgPivot1.DP_Standard = "IMO";
					undgPivot1.DP_UNNO = "1111";
					Factory.Save();

					Assert(unDangerousGoodsTextBox.Visible);
					AssertEquals("text of 1 item", "1111", unDangerousGoodsTextBox.Text);

					var undg2 = goodsItem.UNDGs.AddNew();
					var undgPivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
					undgPivot2.DP_IsDefault = true;
					undgPivot2.DP_Standard = "IMO";
					undgPivot2.DP_UNNO = "1112";
					Factory.Save();

					Assert(unDangerousGoodsTextBox.Visible);
					AssertEquals("text of 2 items", "1111,1112", unDangerousGoodsTextBox.Text);
				});
			}
		}

		public static IEnumerable<string> ExpectedTabPageNamesInOrder = new[]
		{
			"GoodsItemDetailsTabPage",
			"GoodsItemPackagesAndContainersTabPage",
			"GoodsItemSupportingDocumentsTabPage",
			"GoodsItemAdditionalDocumentsTabPage",
			"GoodsItemPreviousDocumentsTabPage",
			"GoodsItemSupplyChainActorsTabPage",
		};

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemsTabUserControl();
		}
		Phase5GoodsItemsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}

	sealed class NctsPhase5LayoutProviderForReorderTabsTest : NctsPhase5LayoutProvider, INctsPhase5LayoutProvider
	{
		IEnumerable<ITabPage> INctsPhase5LayoutProvider.AdditionalGoodsItemTabPages
		{
			get
			{
				yield return new AdditionalTabPageForTest1();
				yield return new AdditionalTabPageForTest2();
			}
		}

		IEnumerable<string> INctsPhase5LayoutProvider.ReorderGoodsItemTabPageNames(IEnumerable<string> defaultTabPageNames)
		{
			return new[] { "AdditionalTabPageForTest1" }.Union(defaultTabPageNames.Except(new[] { "GoodsItemPreviousDocumentsTabPage" }));
		}
	}

	sealed class NctsPhase5LayoutProviderForAdditionalTabsTest : NctsPhase5LayoutProvider, INctsPhase5LayoutProvider
	{
		IEnumerable<ITabPage> INctsPhase5LayoutProvider.AdditionalGoodsItemTabPages
		{
			get
			{
				yield return new AdditionalTabPageForTest1();
				yield return new AdditionalTabPageForTest2();
			}
		}
	}
}
