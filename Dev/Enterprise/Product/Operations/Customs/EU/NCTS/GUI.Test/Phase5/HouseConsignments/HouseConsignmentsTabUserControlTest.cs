using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestHouseConsignmentsSplitContainer()
		{
			CombineAssertions(() =>
			{
				var houseConsignmentsSplitContainer = userControl.HouseConsignmentsSplitContainer;
				AssertEquals("HouseConsignmentsSplitContainer is within HouseConsignmentsTabUserControl", true, userControl.Contains(houseConsignmentsSplitContainer));
				AssertEquals("HouseConsignmentsSplitContainer.Orientation", Orientation.Horizontal, houseConsignmentsSplitContainer.Orientation);
				AssertEquals("HouseConsignmentsSplitContainer.SplitterDistance", 75, houseConsignmentsSplitContainer.SplitterDistance);
				AssertEquals("Dock", DockStyle.Fill, houseConsignmentsSplitContainer.Dock);
				AssertEquals("Grid min size", 75, houseConsignmentsSplitContainer.Panel1MinSize);
				AssertEquals("Details min size", 350, houseConsignmentsSplitContainer.Panel2MinSize);
				AssertEquals("Default Grid min size", 75, houseConsignmentsSplitContainer.Panel1.Height);
			});
		}

		public void TestHouseConsignmentsGridUserControl()
		{
			var header = Factory.New<NctsHeader>();
			userControl.SetDataBinding(header, "");
			var houseConsignmentsGridUserControl = userControl.HouseConsignmentsSplitContainer.Panel1.Controls.OfType<HouseConsignmentsGridUserControl>().First();
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, houseConsignmentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", houseConsignmentsGridUserControl.GetBindingMember());
			});
		}

		public void TestHouseConsignmentTabControl()
		{
			var houseConsignmentTabControl = userControl.HouseConsignmentTabControl;
			CombineAssertions(() =>
			{
				AssertEquals("HouseConsignmentTabControl is within HouseConsignmentsSplitContainer.Panel2", true, userControl.HouseConsignmentsSplitContainer.Panel2.Contains(houseConsignmentTabControl));
				AssertEquals("Dock", DockStyle.Fill, houseConsignmentTabControl.Dock);
			});
		}

		[RequiresSTA]
		public void TestHouseConsignmentDetailsTabPage()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;

			using (var form = new Phase5DepartureMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
				var houseConsignmentDetailsTabPage = houseConsignmentsTabUserControl.HouseConsignmentDetailsTabPage;
				houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(houseConsignmentDetailsTabPage);

				var houseConsignmentDetailsDynamicLayoutPanel = houseConsignmentsTabUserControl.HouseConsignmentDetailsDynamicLayoutPanel;
				var transportDepartureGroupBox = houseConsignmentsTabUserControl.TransportDepartureGroupBox;
				var transportDepartureDynamicLayoutPanel = houseConsignmentsTabUserControl.TransportDepartureDynamicLayoutPanel;
				CombineAssertions(() =>
				{
					AssertEquals("HouseConsignmentDetailsTabPage.Caption", "Details", houseConsignmentDetailsTabPage.CaptionResourceString.Caption);

					AssertEquals("HouseConsignmentDetailsDynamicLayoutPanel.Top", DockStyle.Top, houseConsignmentDetailsDynamicLayoutPanel.Dock);
					DynamicLayoutPanelTest.AssertControlsOrder(houseConsignmentDetailsDynamicLayoutPanel,
						nameof(HouseConsignmentDetailsControlBag.CountryOfDispatchDropEdit),
						nameof(HouseConsignmentDetailsControlBag.ConsignorDocAddressControl),
						nameof(HouseConsignmentDetailsControlBag.ConsigneeDocAddressControl),
						nameof(HouseConsignmentDetailsControlBag.CountryOfDestinationDropEdit),
						nameof(HouseConsignmentDetailsControlBag.GrossWeightCalcDropEdit),
						nameof(HouseConsignmentDetailsControlBag.ReferenceNumberUCRTextBox),
						nameof(HouseConsignmentDetailsControlBag.TransportMoPDropEdit),
						nameof(HouseConsignmentDetailsControlBag.LinePriceCurrencyDropEdit));

					AssertEquals("TransportDepartureGroupBox caption", "Transport Departure", transportDepartureGroupBox.CaptionResourceString.Caption);
					AssertEquals("TransportDepartureDynamicLayoutPanel is within TransportDepartureGroupBox", true, transportDepartureGroupBox.Contains(transportDepartureDynamicLayoutPanel));
					DynamicLayoutPanelTest.AssertControlsOrder(transportDepartureDynamicLayoutPanel,
						nameof(TransportDepartureControlBag.InlandTransportModeDropEdit),
						nameof(TransportDepartureControlBag.PlaceHolderLabel),
						nameof(TransportDepartureControlBag.TransportAtDepartureTypeDropEdit),
						nameof(TransportDepartureControlBag.PlaceHolder2Label),
						nameof(TransportDepartureControlBag.VesselCodeFindBox),
						nameof(TransportDepartureControlBag.VesselCountryCodeFindBox));
				});
			}
		}

		public void TestGoodsItemsTabPage()
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
				CombineAssertions(() =>
				{
					AssertEquals("GoodsItemsTabPage.Caption", "Goods Items", goodsItemsTabPage.CaptionResourceString.Caption);
					AssertEquals("GoodsItemsTabPage is within HouseConsignmentTabControl", true, houseConsignmentsTabUserControl.HouseConsignmentTabControl.TabPages.Contains(goodsItemsTabPage));
					AssertEquals("GoodsItemsTabUserControl is within GoodsItemsTabPage", true, goodsItemsTabPage.Controls.Contains(phase5GoodsItemsTabUserControl));
					AssertEquals("GoodsItemsTabUserControl.Dock", DockStyle.Fill, phase5GoodsItemsTabUserControl.Dock);
					AssertEquals("GoodsItemsTabUserControl.BindingMember", nameof(NctsBill.GoodsItems), phase5GoodsItemsTabUserControl.GetBindingMember());
				});
			}
		}

		public void TestHouseConsignmentPreviousDocumentsTabPage()
		{
			var houseConsignmentPreviousDocumentsTabPage = userControl.HouseConsignmentPreviousDocumentsTabPage;
			var houseConsignmentsPreviousDocumentsTabUserControl = userControl.HouseConsignmentPreviousDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("HouseConsignmentPreviousDocumentsTabPage Caption", "Previous Documents", houseConsignmentPreviousDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("HouseConsignmentPreviousDocumentsTabUserControl is within HouseConsignmentPreviousDocumentsTabPage", true, houseConsignmentPreviousDocumentsTabPage.Controls.Contains(houseConsignmentsPreviousDocumentsTabUserControl));
				AssertEquals("HouseConsignmentPreviousDocumentsTabUserControl Dock", DockStyle.Fill, houseConsignmentsPreviousDocumentsTabUserControl.Dock);
				AssertEquals("HouseConsignmentPreviousDocumentsTabUserControl BindingMember", nameof(NctsBill.PreviousDocuments), houseConsignmentsPreviousDocumentsTabUserControl.GetBindingMember());
			});
		}

		public void TestHouseConsignmentSupportingDocumentsTabPage()
		{
			var supportingDocumentsTabPage = userControl.HouseConsignmentSupportingDocumentsTabPage;
			var supportingDocumentsTabUserControl = userControl.HouseConsignmentSupportingDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("HouseConsignmentSupportingDocumentsTabPage Caption", "Supporting Documents", supportingDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("HouseConsignmentSupportingDocumentsTabPage is within HouseConsignmentTabControl", true, userControl.HouseConsignmentTabControl.Contains(supportingDocumentsTabPage));

				AssertEquals("HouseConsignmentSupportingDocumentsTabUserControl is within HouseConsignmentSupportingDocumentsTabPage", true, supportingDocumentsTabPage.Controls.Contains(supportingDocumentsTabUserControl));
				AssertEquals("HouseConsignmentSupportingDocumentsTabUserControl Dock", DockStyle.Fill, supportingDocumentsTabUserControl.Dock);
				AssertEquals("HouseConsignmentSupportingDocumentsTabUserControl.BindingMember", nameof(NctsBill.SupportingDocuments), supportingDocumentsTabUserControl.GetBindingMember());
			});
		}

		public void TestHouseConsignmentSupplyChainActorsTabPage()
		{
			var houseConsignmentSupplyChainActorsTabPage = userControl.HouseConsignmentSupplyChainActorsTabPage;
			var houseConsignmentSupplyChainActorsTabUserControl = userControl.HouseConsignmentSupplyChainActorsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("HouseConsignmentSupplyChainActorsTabPage Caption", "Supply Chain Actors", houseConsignmentSupplyChainActorsTabPage.CaptionResourceString.Caption);
				AssertEquals("HouseConsignmentSupplyChainActorsTabUserControl is within HouseConsignmentSupplyChainActorsTabPage", true, houseConsignmentSupplyChainActorsTabPage.Controls.Contains(houseConsignmentSupplyChainActorsTabUserControl));
				AssertEquals("HouseConsignmentSupplyChainActorsTabUserControl Dock", DockStyle.Fill, houseConsignmentSupplyChainActorsTabUserControl.Dock);
				AssertEquals("HouseConsignmentSupplyChainActorsTabUserControl BindingMember", nameof(NctsBill.CusSupplyChainActorReferences), houseConsignmentSupplyChainActorsTabUserControl.GetBindingMember());
			});
		}

		public void TestHouseConsignmentAdditionalDocumentsTabPage()
		{
			var houseConsignmentAdditionalDocumentsTabPage = userControl.HouseConsignmentAdditionalDocumentsTabPage;
			var houseConsignmentAdditionalDocumentsTabUserControl = userControl.HouseConsignmentAdditionalDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("HouseConsignmentAdditionalDocumentsTabPage Caption", "Additional Documents", houseConsignmentAdditionalDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("HouseConsignmentAdditionalDocumentsTabUserControl is within HouseConsignmentAdditionalDocumentsTabPage", true, houseConsignmentAdditionalDocumentsTabPage.Controls.Contains(houseConsignmentAdditionalDocumentsTabUserControl));
				AssertEquals("HouseConsignmentAdditionalDocumentsTabUserControl Dock", DockStyle.Fill, houseConsignmentAdditionalDocumentsTabUserControl.Dock);
				AssertEquals("HouseConsignmentAdditionalDocumentsTabUserControl BindingMember", nameof(NctsBill.AdditionalDocuments), houseConsignmentAdditionalDocumentsTabUserControl.GetBindingMember());
			});
		}

		public void TestTransportDepartureGroupVisibility() => CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var nctsPhase5LayoutProviderMock = new Mock<INctsPhase5LayoutProvider>();
			var nctsPhase5LayoutProviderDefault = new NctsPhase5LayoutProvider();
			nctsPhase5LayoutProviderMock.Setup(x => x.DeclarationDetailsTabUserControlType).Returns(nctsPhase5LayoutProviderDefault.DeclarationDetailsTabUserControlType);
			nctsPhase5LayoutProviderMock.Setup(x => x.HouseConsignmentsTabUserControlType).Returns(nctsPhase5LayoutProviderDefault.HouseConsignmentsTabUserControlType);
			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
				{
					{ "Default", new TestObjectHandle(nctsPhase5LayoutProviderMock.Object) }
				};
			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			{
				nctsPhase5LayoutProviderMock.Setup(x => x.HouseConsignmentTransportDeparturePanelLayout).Returns(new HouseConsignmentTransportDepartureLayout());
				nctsPhase5LayoutProviderMock.Setup(x => x.HouseConsignmentDetailsPanelLayoutWithGrid).Returns(new HouseConsignmentDetailsLayoutWithGrid());
				nctsPhase5LayoutProviderMock.Setup(x => x.GetHouseConsignmentDetailsGridColumnLayout()).Returns(new HouseConsignmentDetailsGridColumnLayout());
				AssertTransportDepartureGroupBoxVisibility("With HouseConsignmentTransportDeparturePanelLayout", expectedVisibility: true);

				nctsPhase5LayoutProviderMock.Setup(x => x.HouseConsignmentTransportDeparturePanelLayout).Returns((IPanelLayoutProvider)null);
				AssertTransportDepartureGroupBoxVisibility("With no HouseConsignmentTransportDeparturePanelLayout", expectedVisibility: false);
			}

			void AssertTransportDepartureGroupBoxVisibility(string assertionMessage, bool expectedVisibility)
			{
				using (var form = new Phase5DepartureMovementForm(nctsHeader))
				{
					form.Show();

					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

					var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
					AssertEquals(assertionMessage, expectedVisibility, houseConsignmentsTabUserControl.TransportDepartureGroupBox.Visible);
				}
			}
		});

		public void TestTabPagesOrder()
		{
			AssertSequencesEqual("TabPageNames",
			new[]
			{
				"HouseConsignmentDetailsTabPage",
				"GoodsItemsTabPage",
				"HouseConsignmentSupportingDocumentsTabPage",
				"HouseConsignmentAdditionalDocumentsTabPage",
				"HouseConsignmentPreviousDocumentsTabPage",
				"HouseConsignmentSupplyChainActorsTabPage"
			}, userControl.HouseConsignmentTabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentsTabUserControl();
		}
		HouseConsignmentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
