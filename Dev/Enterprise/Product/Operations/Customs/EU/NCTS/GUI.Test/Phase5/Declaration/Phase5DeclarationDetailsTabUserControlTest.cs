using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5DeclarationDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestTabPagesOrder()
		{
			AssertSequencesEqual("TabPageNames", ExpectedTabPageNamesInOrder, userControl.DeclarationDetailsTabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name));
		}

		public static IEnumerable<string> ExpectedTabPageNamesInOrder = new[]
		{
				"AuthorizationsTabPage",
				"SupportingDocumentsTabPage",
				"AdditionalDocumentsTabPage",
				"PreviousDocumentsTabPage",
				"CountryOfRoutingTabPage",
				"SupplyChainActorTabPage",
		};

		public void TestBindingSource()
		{
			AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestTraderDetailsGroupBox()
		{
			var traderDetailsGroupBox = userControl.TraderDetailsGroupBox;
			var dynamicTraderDetailsPanel = userControl.DynamicTraderDetailsPanel;
			CombineAssertions(() =>
			{
				AssertEquals("TraderDetailsGroupBox Caption", "Trader Details", traderDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("TraderDetailsGroupBox Dock", DockStyle.Left, traderDetailsGroupBox.Dock);
				AssertEquals("TraderDetailsGroupBox TabIndex", 0, traderDetailsGroupBox.TabIndex);
				AssertEquals("DynamicTraderDetailsPanel is within TraderDetailsGroupBox", true, traderDetailsGroupBox.Controls.Contains(dynamicTraderDetailsPanel));
				AssertEquals("DynamicTraderDetailsPanel Dock", DockStyle.Fill, dynamicTraderDetailsPanel.Dock);
			});
		}

		public void TestDynamicDeclarationDetailsPanelProperties()
		{
			var dynamicDeclarationDetailsPanel = userControl.DynamicDeclarationDetailsPanel;
			AssertEquals("AutoScroll", true, dynamicDeclarationDetailsPanel.AutoScroll);
		}

		public void TestDepartureDetailsGroupBox()
		{
			var departureDetailsGroupBox = userControl.DepartureDetailsGroupBox;
			var dynamicDepartureDetailsPanel = userControl.DynamicDepartureDetailsPanel;
			CombineAssertions(() =>
			{
				AssertEquals("DepartureDetailsGroupBox Caption", "Departure Details", departureDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("DepartureDetailsGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left, departureDetailsGroupBox.Anchor);
				AssertEquals("DepartureDetailsGroupBox TabIndex", 1, departureDetailsGroupBox.TabIndex);
				AssertEquals("DynamicDepartureDetailsPanel is within DepartureDetailsGroupBox", true, departureDetailsGroupBox.Controls.Contains(dynamicDepartureDetailsPanel));
				AssertEquals("DynamicDepartureDetailsPanel Dock", DockStyle.Fill, dynamicDepartureDetailsPanel.Dock);
				AssertGreaterThan("DepartureDetailsGroupBox height", departureDetailsGroupBox.Height, 340);
			});
		}

		public void TestDeclarationDetailsGroupBox()
		{
			var declarationDetailsGroupBox = userControl.DeclarationDetailsGroupBox;
			var dynamicDeclarationDetailsPanel = userControl.DynamicDeclarationDetailsPanel;
			CombineAssertions(() =>
			{
				AssertEquals("DeclarationDetailsGroupBox Caption", "Declaration Details", declarationDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("DeclarationDetailsGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, declarationDetailsGroupBox.Anchor);
				AssertEquals("DeclarationDetailsGroupBox TabIndex", 3, declarationDetailsGroupBox.TabIndex);
				AssertEquals("DynamicDeclarationDetailsPanel is within DeclarationDetailsGroupBox", true, declarationDetailsGroupBox.Controls.Contains(dynamicDeclarationDetailsPanel));
				AssertEquals("DynamicDeclarationDetailsPanel Dock", DockStyle.Fill, dynamicDeclarationDetailsPanel.Dock);
			});
		}

		public void TestCustomsOfficesGroupBox()
		{
			var customsOfficesGroupBox = userControl.CustomsOfficesGroupBox;
			var dynamicCustomsOfficesUserControl = userControl.DynamicCustomsOfficesUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsOfficeGroupBox Caption", "Customs Offices", customsOfficesGroupBox.CaptionResourceString.Caption);
				AssertEquals("CustomsOfficeGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, customsOfficesGroupBox.Anchor);
				AssertEquals("CustomsOfficeGroupBox TabIndex", 4, customsOfficesGroupBox.TabIndex);
				AssertEquals("DynamicCustomsOfficesUserControl is within CustomsOfficeGroupBox", true, customsOfficesGroupBox.Controls.Contains(dynamicCustomsOfficesUserControl));
				AssertEquals("DynamicCustomsOfficesUserControl Dock", DockStyle.Fill, dynamicCustomsOfficesUserControl.Dock);
			});
		}

		public void TestGuaranteesGroupBox()
		{
			var guaranteesGroupBox = userControl.GuaranteesGroupBox;
			var dynamicGuaranteesUserControl = userControl.DynamicGuaranteesUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("GuaranteesGroupBox Caption", "Guarantees", guaranteesGroupBox.CaptionResourceString.Caption);
				AssertEquals("GuaranteesGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, guaranteesGroupBox.Anchor);
				AssertEquals("GuaranteesGroupBox TabIndex", 5, guaranteesGroupBox.TabIndex);
				AssertEquals("DynamicGuaranteesUserControl is within GuaranteesGroupBox", true, guaranteesGroupBox.Controls.Contains(dynamicGuaranteesUserControl));
				AssertEquals("DynamicGuaranteesUserControl Dock", DockStyle.Fill, dynamicGuaranteesUserControl.Dock);
			});
		}

		public void TestSecurityAtDepartureGroupBox()
		{
			var securityAtDepartureGroupBox = userControl.SecurityAtDepartureGroupBox;
			var securityAtDepartureDynamicLayoutPanel = userControl.SecurityAtDepartureDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("SecurityAtDepartureGroupBox Caption", "Security", securityAtDepartureGroupBox.CaptionResourceString.Caption);
				AssertEquals("SecurityAtDepartureGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left, securityAtDepartureGroupBox.Anchor);
				AssertEquals("SecurityAtDepartureGroupBox TabIndex", 2, securityAtDepartureGroupBox.TabIndex);
				AssertEquals("SecurityAtDepartureDynamicLayoutPanel is within SecurityAtDepartureGroupBox", true, securityAtDepartureGroupBox.Controls.Contains(securityAtDepartureDynamicLayoutPanel));
				AssertEquals("SecurityAtDepartureDynamicLayoutPanel Dock", DockStyle.Fill, securityAtDepartureDynamicLayoutPanel.Dock);
			});
		}

		public void TestTraderDetailsPanelLayout()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			var dynamicTraderDetailsPanel = userControl.DynamicTraderDetailsPanel;
			DynamicLayoutPanelTest.AssertControlsOrder(dynamicTraderDetailsPanel, nameof(TraderDetailsControlBag.PrincipalDocAddressControl),
				nameof(TraderDetailsControlBag.ConsignorDocAddressControl),
				nameof(TraderDetailsControlBag.ConsigneeDocAddressControl),
				nameof(TraderDetailsControlBag.RepresentativeDocAddressControl),
				nameof(TraderDetailsControlBag.FromWarehouseGroupBox));
		}

		public void TestDepartureDetailsPanelLayout()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			var dynamicDepartureDetailsPanel = userControl.DynamicDepartureDetailsPanel;
			DynamicLayoutPanelTest.AssertControlsOrder(dynamicDepartureDetailsPanel,
				nameof(DepartureDetailsControlBag.CustomerReferenceNumberTextBox),
				nameof(DepartureDetailsControlBag.DeclarationTypeDropEdit),
				nameof(DepartureDetailsControlBag.SecurityDropEdit),
				nameof(DepartureDetailsControlBag.CountryOfDispatchDropEdit),
				nameof(DepartureDetailsControlBag.CountryOfDestinationDropEdit),
				nameof(DepartureDetailsControlBag.GrossWeightCalcDropEdit),
				nameof(DepartureDetailsControlBag.LocationOfGoodsUserControl),
				nameof(DepartureDetailsControlBag.CommercialReferenceNumberTextBox));
		}

		public void TestDeclarationDetailsPanelLayout()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			var dynamicDeclarationDetailsPanel = userControl.DynamicDeclarationDetailsPanel;
			DynamicLayoutPanelTest.AssertControlsOrder(dynamicDeclarationDetailsPanel,
				nameof(DeclarationDetailsControlBag.MrnTextBox),
				nameof(DeclarationDetailsControlBag.ReleaseDateEdit),
				nameof(DeclarationDetailsControlBag.DepartureStatusDropEdit),
				nameof(DeclarationDetailsControlBag.AcceptanceDateEdit),
				nameof(DeclarationDetailsControlBag.PhaseStatusDropEdit),
				nameof(DeclarationDetailsControlBag.MessageStatusDropEdit));
		}

		[RequiresSTA]
		public void TestDynamicCustomsOfficesUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			AssertEquals(typeof(Phase5CustomsOfficesUserControl), userControl.DynamicCustomsOfficesUserControl.UserControlType);
		}

		public void TestDynamicGuaranteesUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			AssertEquals(typeof(Phase5GuaranteesUserControl), userControl.DynamicGuaranteesUserControl.UserControlType);
		}

		public void TestSecurityAtDepartureDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			userControl.SetDataBinding(nctsHeader, "");
			var securityAtDepartureDynamicLayoutPanel = userControl.SecurityAtDepartureDynamicLayoutPanel;
			DynamicLayoutPanelTest.AssertControlsOrder(
				securityAtDepartureDynamicLayoutPanel,
				nameof(SecurityAtDepartureControlBag.PlaceOfLoadingUserControl),
				nameof(SecurityAtDepartureControlBag.PlaceOfUnloadingUserControl),
				nameof(SecurityAtDepartureControlBag.SpecificCircumstanceIndicatorDropEdit));
		}

		public void TestDeclarationDetailsTabControl()
		{
			var declarationDetailsTabControl = userControl.DeclarationDetailsTabControl;
			AssertEquals("TabIndex", 6, declarationDetailsTabControl.TabIndex);
		}

		public void TestSupportingDocumentsTabPage()
		{
			var supportingDocumentsTabPage = userControl.SupportingDocumentsTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Supporting Documents", supportingDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("Within DeclarationDetailsTabControl", true, userControl.DeclarationDetailsTabControl.Contains(supportingDocumentsTabPage));
				AssertEquals("Contains DynamicSupportingDocumentsTabUserControl", true, supportingDocumentsTabPage.Contains(userControl.DynamicSupportingDocumentsTabUserControl));
			});
		}

		public void TestSupportingDocumentsTabUserControl()
		{
			userControl.SetDataBinding(Factory.New<NctsHeader>(), "");
			var dynamicSupportingDocumentsTabUserControl = userControl.DynamicSupportingDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(DeclarationSupportingDocumentsGridUserControl), dynamicSupportingDocumentsTabUserControl.UserControlType);
				AssertType<ZDynamicControlCreationUserControl>("Dynamic", dynamicSupportingDocumentsTabUserControl);
				AssertEquals("BindingMember", ".", dynamicSupportingDocumentsTabUserControl.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, dynamicSupportingDocumentsTabUserControl.Dock);
			});
		}

		public void TestPreviousDocumentsTabPage()
		{
			var previousDocumentsTabPage = userControl.PreviousDocumentsTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Previous Documents", previousDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("Within DeclarationDetailsTabControl", true, userControl.DeclarationDetailsTabControl.Contains(previousDocumentsTabPage));
				AssertEquals("Contains DynamicPreviousDocumentsTabUserControl", true, previousDocumentsTabPage.Contains(userControl.DynamicPreviousDocumentsTabUserControl));
			});
		}

		[RequiresSTA]
		public void TestPreviousDocumentsTabUserControl()
		{
			userControl.SetDataBinding(Factory.New<NctsHeader>(), "");
			var dynamicPreviousDocumentsTabUserControl = userControl.DynamicPreviousDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(DeclarationPreviousDocumentsGridUserControl), dynamicPreviousDocumentsTabUserControl.UserControlType);
				AssertType<ZDynamicControlCreationUserControl>("Dynamic", dynamicPreviousDocumentsTabUserControl);
				AssertEquals("BindingMember", ".", dynamicPreviousDocumentsTabUserControl.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, dynamicPreviousDocumentsTabUserControl.Dock);
			});
		}

		public void TestAdditionalDocumentsTabPage()
		{
			var additionalDocumentsTabPage = userControl.AdditionalDocumentsTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Additional Documents", additionalDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("Within DeclarationDetailsTabControl", true, userControl.DeclarationDetailsTabControl.Contains(additionalDocumentsTabPage));
				AssertEquals("Contains DynamicAdditionalDocumentsTabUserControl", true, additionalDocumentsTabPage.Contains(userControl.DynamicAdditionalDocumentsTabUserControl));
			});
		}

		public void TestAdditionalDocumentsTabUserControl()
		{
			userControl.SetDataBinding(Factory.New<NctsHeader>(), "");
			var dynamicAdditionalDocumentsTabUserControl = userControl.DynamicAdditionalDocumentsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(DeclarationAdditionalDocumentsGridUserControl), dynamicAdditionalDocumentsTabUserControl.UserControlType);
				AssertType<ZDynamicControlCreationUserControl>("Dynamic", dynamicAdditionalDocumentsTabUserControl);
				AssertEquals("BindingMember", ".", dynamicAdditionalDocumentsTabUserControl.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, dynamicAdditionalDocumentsTabUserControl.Dock);
			});
		}

		public void TestCountryOfRoutingTabPage()
		{
			var countryOfRoutingTabPage = userControl.CountryOfRoutingTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Country/Region of Routing", countryOfRoutingTabPage.CaptionResourceString.Caption);
				AssertEquals("Within DeclarationDetailsTabControl", true, userControl.DeclarationDetailsTabControl.Contains(countryOfRoutingTabPage));
				AssertEquals("Contains CountryOfRoutingTabUserControl", true, countryOfRoutingTabPage.Contains(userControl.CountryOfRoutingTabUserControl));
			});
		}

		public void TestCountryOfRoutingTabUserControl()
		{
			var countryOfRoutingTabUserControl = userControl.CountryOfRoutingTabUserControl;
			CombineAssertions(() =>
			{
				AssertType<Phase5DeclarationCountryOfRoutingTabUserControl>("Type", countryOfRoutingTabUserControl);
				AssertEquals("BindingMember", nameof(NctsHeader.CountriesOfRouting), countryOfRoutingTabUserControl.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, countryOfRoutingTabUserControl.Dock);
			});
		}

		public void TestGroupBoxAlignments()
		{
			var departureDetailsGroupBox = userControl.DepartureDetailsGroupBox;
			var securityGroupBox = userControl.SecurityAtDepartureGroupBox;
			var declarationDetailsGroupBox = userControl.DeclarationDetailsGroupBox;
			var customsOfficesGroupBox = userControl.CustomsOfficesGroupBox;
			var guarantiesGroupBox = userControl.GuaranteesGroupBox;

			var declarationDetailsGroupBoxLocationX = declarationDetailsGroupBox.Location.X;
			var guarantiesGroupBoxLocationY = guarantiesGroupBox.Location.Y;
			var declarationDetailsGroupBoxWidth = declarationDetailsGroupBox.Width;
			var departureDetailsGroupBoxLocationY = departureDetailsGroupBox.Location.Y;
			var securityGroupBoxLocationY = securityGroupBox.Location.Y;
			CombineAssertions(() =>
			{
				AssertEquals("Left side aligned: DepartureDetails, Security", departureDetailsGroupBox.Location.X, securityGroupBox.Location.X);
				AssertEquals("Same width: DepartureDetails, Security", departureDetailsGroupBox.Width, securityGroupBox.Width);
				AssertEquals("Left side aligned: DeclarationDetails, CustomsOffices and Guarantees", true, declarationDetailsGroupBoxLocationX == customsOfficesGroupBox.Location.X && declarationDetailsGroupBoxLocationX == guarantiesGroupBox.Location.X);
				AssertEquals("Same width: DeclarationDetails, CustomsOffices and Guarantees", true, declarationDetailsGroupBoxWidth == customsOfficesGroupBox.Width && declarationDetailsGroupBoxWidth == guarantiesGroupBox.Width);
				AssertEquals("Same location-Y: DepartureDetails, DeclarationDetails", departureDetailsGroupBoxLocationY, declarationDetailsGroupBox.Location.Y);
				AssertEquals("Bottom Alignment: Security, Gurantees", securityGroupBoxLocationY + securityGroupBox.Height, guarantiesGroupBoxLocationY + guarantiesGroupBox.Height);
			});
		}

		public void TestSupplyChainActorsTabPage()
		{
			var supplyChainActorTabPage = userControl.SupplyChainActorTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Supply Chain Actors", supplyChainActorTabPage.CaptionResourceString.Caption);
				AssertEquals("Within DeclarationDetailsTabControl", true,
					userControl.DeclarationDetailsTabControl.Contains(supplyChainActorTabPage));
				AssertEquals("Contains DynamicSupplyChainActorTabUserControl", true,
					supplyChainActorTabPage.Contains(userControl.DynamicSupplyChainActorTabUserControl));
			});
		}

		public void TestSupplyChainActorsTabUserControl()
		{
			userControl.SetDataBinding(Factory.New<NctsHeader>(), "");
			var dynamicSupplyChainActorTabUserControl = userControl.DynamicSupplyChainActorTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(Phase5DeclarationSupplyChainActorsTabUserControl),
					dynamicSupplyChainActorTabUserControl.UserControlType);
				AssertType<ZDynamicControlCreationUserControl>("Dynamic", dynamicSupplyChainActorTabUserControl);
				AssertEquals("BindingMember", nameof(NctsHeader.MovementHeader), dynamicSupplyChainActorTabUserControl.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, dynamicSupplyChainActorTabUserControl.Dock);
			});
		}

		public void TestAuthorizationsTabPage()
		{
			var authorizationsTabPage = userControl.AuthorizationsTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Authorizations", authorizationsTabPage.CaptionResourceString.Caption);
				AssertEquals("Within DeclarationDetailsTabControl", true, userControl.DeclarationDetailsTabControl.Contains(authorizationsTabPage));
				AssertEquals("Contains DynamicAuthorizationsTabUserControl", true, authorizationsTabPage.Contains(userControl.DynamicAuthorizationsTabUserControl));
			});
		}

		public void TestAuthorizationsTabUserControl()
		{
			userControl.SetDataBinding(Factory.New<NctsHeader>(), "");
			var dynamicAuthorizationsTabUserControl = userControl.DynamicAuthorizationsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(Phase5DeclarationAuthorizationsTabUserControl), dynamicAuthorizationsTabUserControl.UserControlType);
				AssertType<ZDynamicControlCreationUserControl>("Dynamic", dynamicAuthorizationsTabUserControl);
				AssertEquals("BindingMember", ".", dynamicAuthorizationsTabUserControl.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, dynamicAuthorizationsTabUserControl.Dock);
			});
		}

		public void TestAdditionalTabPages()
		{
			var mock = new Mock<INctsPhase5LayoutProvider>();
			mock.Setup(m => m.AdditionalDeclarationDetailTabPages).Returns(new List<ITabPage> { new AdditionalTabPageForTest1(), new AdditionalTabPageForTest2(), new AdditionalTabPageForTest3() });

			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(mock.Object) }
			};

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			{
				userControl.SetDataBinding(Factory.New<NctsHeader>(), "");

				CombineAssertions(() =>
				{
					AssertEquals("There are 3 additional tabs", 3, userControl.DeclarationDetailsTabControl.AllTabPages.Count(t => t.Name.StartsWith("AdditionalTabPageForTest")));

					AssertTabPage<AdditionalTabPageForTest1UserControl>(userControl, nameof(AdditionalTabPageForTest1), 1);
					AssertTabPage<AdditionalTabPageForTest2UserControl>(userControl, nameof(AdditionalTabPageForTest2), 2);
					AssertTabPage<AdditionalTabPageForTest3UserControl>(userControl, nameof(AdditionalTabPageForTest3), 3);
				});
			}
		}

		public void TestReorderTabPages()
		{
			var mock = new Mock<INctsPhase5LayoutProvider>();
			mock.Setup(m => m.AdditionalDeclarationDetailTabPages).Returns(new List<ITabPage> { new AdditionalTabPageForTest1(), new AdditionalTabPageForTest2(), new AdditionalTabPageForTest3() });
			mock.Setup(m => m.ReorderDeclarationDetailTabPagesNames(defaultTabPageNames)).Returns(reorderedTabPageNames);

			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(mock.Object) }
			};

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			{
				CombineAssertions(() =>
				{
					var declarationDetailsTabControl = userControl.DeclarationDetailsTabControl;
					AssertNotNull("Before binding, has AdditionalDocumentsTabPage", declarationDetailsTabControl.GetTabPage(nameof(Phase5DeclarationDetailsTabUserControl.AdditionalDocumentsTabPage)));
					AssertNotNull("Before binding, has AuthorizationsTabPage", declarationDetailsTabControl.GetTabPage(nameof(Phase5DeclarationDetailsTabUserControl.AuthorizationsTabPage)));

					userControl.SetDataBinding(Factory.New<NctsHeader>(), "");

					AssertNotNull("After binding, AdditionalDocumentsTabPage not removed", declarationDetailsTabControl.GetTabPage(nameof(Phase5DeclarationDetailsTabUserControl.AdditionalDocumentsTabPage)));
					AssertNull("After binding, AuthorizationsTabPage removed", declarationDetailsTabControl.GetTabPage(nameof(Phase5DeclarationDetailsTabUserControl.AuthorizationsTabPage)));
					AssertSequencesEqual("TabPageNames", reorderedTabPageNames, userControl.DeclarationDetailsTabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name));
				});
			}
		}

		public void TestUpdateTotalGrossWeightPopup_OK()
		{
			var departureMovementHeader = HeaderForUpdateTotalGrossWeightTest();
			departureMovementHeader.BM_GrossWeight = 11.0m;
			departureMovementHeader.BM_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(departureMovementHeader.Header, string.Empty);

				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				departureMovementHeader.Factory.Save();

				AssertEquals("Dialog pops up during save", "Total Gross Weight is lower than sum of Gross Weights in House Consignments. Update Total Gross Weight with Calculated value?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("BM_GrossWeight is updated from the Total weight of Bills.",12.0m, departureMovementHeader.BM_GrossWeight);
			}
		}

		public void TestUpdateTotalGrossWeightPopup_Cancel()
		{
			var departureMovementHeader = HeaderForUpdateTotalGrossWeightTest();
			departureMovementHeader.BM_GrossWeight = 11.0m;
			departureMovementHeader.BM_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(departureMovementHeader.Header, string.Empty);

				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				departureMovementHeader.Factory.Save();

				AssertEquals("Dialog pops up during save", "Total Gross Weight is lower than sum of Gross Weights in House Consignments. Update Total Gross Weight with Calculated value?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("BM_GrossWeight is not updated.",11.0m, departureMovementHeader.BM_GrossWeight);
			}
		}

		NctsDepartureMovementHeader HeaderForUpdateTotalGrossWeightTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var departureMovementHeader = nctsHeader.MovementHeader;

			var bill = nctsHeader.Bills.AddNew();
			bill.B0_Weight = 12.0m;

			return departureMovementHeader;
		}

		void AssertTabPage<T>(Phase5DeclarationDetailsTabUserControl userControl, string additionalGoodsItemTabPageName, int index)
			where T : ZUserControl
		{
			var additionalTab = userControl.DeclarationDetailsTabControl.GetTabPage(additionalGoodsItemTabPageName);
			AssertEquals($"Tab{index} Caption", $"Additional Tab Page For Test #{index}", additionalTab.CaptionResourceString.Caption);
			AssertEquals($"Tab{index} Dock style", DockStyle.Fill, additionalTab.Dock);

			var additionalTabUserControl = additionalTab.Controls.Cast<Control>().FirstOrDefault(x => x is T);
			AssertEquals($"Tab{index}UserControl BindingMember", $"Property{index}", additionalTabUserControl.GetBindingMember());
			AssertEquals($"Tab{index}UserControl Dock style", DockStyle.Fill, additionalTabUserControl.Dock);
		}

		readonly string[] defaultTabPageNames = new[] { "AuthorizationsTabPage", "SupportingDocumentsTabPage", "AdditionalDocumentsTabPage", "PreviousDocumentsTabPage", "CountryOfRoutingTabPage", "SupplyChainActorTabPage", "AdditionalTabPageForTest1", "AdditionalTabPageForTest2", "AdditionalTabPageForTest3" };
		readonly string[] reorderedTabPageNames = new[] { "AdditionalTabPageForTest1", "SupportingDocumentsTabPage", "AdditionalTabPageForTest2", "AdditionalDocumentsTabPage", "PreviousDocumentsTabPage", "CountryOfRoutingTabPage", "SupplyChainActorTabPage", "AdditionalTabPageForTest3" };

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5DeclarationDetailsTabUserControl();
		}
		Phase5DeclarationDetailsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
