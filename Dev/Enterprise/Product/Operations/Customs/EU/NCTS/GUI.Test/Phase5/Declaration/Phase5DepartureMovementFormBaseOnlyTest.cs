using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5DepartureMovementForm))]
	sealed class Phase5DepartureMovementFormBaseOnlyTest : Phase5DepartureMovementFormAbstractTest<NctsHeader>
	{
		public void TestMessagesUserControlType()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectedTab = form.MessagesTabPage;
				var messagesTabDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("MessagesTabDynamicUserControl");
				AssertType<MessagesTabUserControl>(messagesTabDynamicUserControl.HostedControl);
				AssertEquals("MovementHeader.MessagesForDisplay", messagesTabDynamicUserControl.BindingSource.DataMember);
			}
		}

		public void TestDocDataPlugIn_Supported()
		{
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfiguration(Factory, true))
			using (var form = new Phase5DepartureMovementForm(header))
			{
				AssertNotNull("DocData PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
				AssertNotNull("DocumentVisualizer PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer));
			}
		}

		[RequiresSTA]
		public void TestDocDataPlugIn_Unsupported()
		{
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfiguration(Factory, false))
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				AssertNull("DocData PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
				AssertNull("DocumentVisualizer PlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer));

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertNull("No DocDataTabPage", mainTabControl.AllTabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == "DocDataTabPage"));
			}
		}

		[RequiresSTA]
		public void TestMinimumSize()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true), form.MinimumSize);
			}
		}

		public void TestFormCaption()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertContains("NCTS Transit Movement", form.Text);
			}
		}

		[RequiresSTA]
		public void TestMainTabPage()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = mainTabControl.GetTabPage("MainTabPage");

				CombineAssertions(() =>
				{
					AssertEquals("MainTabPage.AutoScroll", true, mainTabPage.AutoScroll);

					var declarationDetailsTabUserControl = form.FindSingle<Phase5DeclarationDetailsTabUserControl>("DeclarationDetailsTabUserControl", -1);
					AssertEquals("DeclarationDetailsTabUserControl is within MainTabPage", true, mainTabPage.Contains(declarationDetailsTabUserControl));
					AssertEquals("DeclarationDetailsTabUserControl.Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true), declarationDetailsTabUserControl.Size);
				});
			}
		}

		public void TestMovements()
		{
			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory, "GetIsMultipleMovementsEnabled", true))
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var movementsTabPage = mainTabControl.GetTabPage(nameof(Phase5DepartureMovementForm.MovementsTabPage));
				mainTabControl.SelectedTab = movementsTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("MovementsTabPage.Caption", "Movements", movementsTabPage.CaptionResourceString.Caption);

					var departureMovementsTabUserControl = form.DepartureMovementsTabUserControl;
					AssertEquals("DepartureMovementsTabUserControl is within MainTabPage", true, movementsTabPage.Contains(departureMovementsTabUserControl));
					AssertEquals("DepartureMovementsTabUserControl is docked to fill", DockStyle.Fill, departureMovementsTabUserControl.Dock);
					AssertEquals("DepartureMovementsTabUserControl DataSource", typeof(INctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>), departureMovementsTabUserControl.BindingSource.DataSourceType);
				});
			}
		}

		[RequiresSTA]
		public void TestTransportAndPackagingTabPage()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectedTab = form.TransportAndPackagingTabPage;
				CombineAssertions(() =>
				{
					AssertEquals("TransportAndPackagingTabPage.Caption", "Transport && Containers", form.TransportAndPackagingTabPage.CaptionResourceString.Caption);

					var transportAndPackagingTabUserControl = form.FindSingle<Phase5TransportAndPackagingTabUserControl>();
					AssertEquals("TransportAndPackagingTabUserControl is within TransportAndPackagingTabPage", true, form.TransportAndPackagingTabPage.Contains(transportAndPackagingTabUserControl));
					AssertEquals("TransportAndPackagingTabUserControl.Dock", DockStyle.Fill, transportAndPackagingTabUserControl.Dock);
					AssertEquals("TransportAndPackagingTabUserControl.DataSourceType", typeof(NctsHeader), transportAndPackagingTabUserControl.BindingSource.DataSourceType);
				});
			}
		}

		public void TestServicesTabPage() => TestUsingForm(header, (form) =>
		{
			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			mainTabControl.SelectedTab = form.ServicesTabPage;

			var servicesTabPage = form.ServicesTabPage;
			AssertEquals("Caption", "Services", servicesTabPage.CaptionResourceString.Caption);
		});

		public void TestServicesTabUserControl() => TestUsingForm(header, (form) =>
		{
			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			mainTabControl.SelectedTab = form.ServicesTabPage;
			var servicesTabUserControl = form.ServicesTabUserControl;
			AssertEquals("ServicesTabUserControl is within ServicesTabPage", true, form.ServicesTabPage.Contains(servicesTabUserControl));
			AssertType<Phase5DeclarationServicesTabUserControl>("Type", servicesTabUserControl);
			AssertEquals("BindingMember", ".", servicesTabUserControl.GetBindingMember());
			AssertEquals("Dock", DockStyle.Fill, servicesTabUserControl.Dock);
		});

		public void TestMessagesTabPage()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				AssertEquals("Caption", "Messages", form.MessagesTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestHouseConsignmentsTabPage()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectedTab = form.HouseConsignmentsTabPage;
				CombineAssertions(() =>
				{
					AssertEquals("HouseConsignmentsTabPage.Caption", "House Consignments", form.HouseConsignmentsTabPage.CaptionResourceString.Caption);

					var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
					AssertEquals("HouseConsignmentsTabUserControl is within HouseConsignmentsTabPage", true, form.HouseConsignmentsTabPage.Contains(houseConsignmentsTabUserControl));
					AssertEquals("HouseConsignmentsTabUserControl.Dock", DockStyle.Fill, houseConsignmentsTabUserControl.Dock);
					AssertEquals("HouseConsignmentsTabUserControl.DataMember", nameof(NctsHeader.Bills), houseConsignmentsTabUserControl.BindingSource.DataMember);
				});
			}
		}

		public void TestDeclarationDetailsTabUserControl()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				var declarationDetailsTabUserControl = form.FindSingle<Phase5DeclarationDetailsTabUserControl>("DeclarationDetailsTabUserControl");
				AssertEquals(DockStyle.Fill, declarationDetailsTabUserControl.Dock);
			}
		}

		[RequiresSTA]
		public void TestTabPagesOrderWithDocDataPluginSupport()
		{
			var expectedTabPagesInOrder = new string[]
			{
				"MainTabPage", "ServicesTabPage", "TransportAndPackagingTabPage", "HouseConsignmentsTabPage", "MessagesTabPage", "CustomFieldsTabPage", "WorkflowTabPage",
				"BillingTabPage", "DocDataTabPage", "eDocsTabPage", "NotesTabPage", "LogsTabPage"
			};

			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfiguration(Factory, docDataPlugInSupportForDepartureMovement: true))
			{
				AssertTabPagesOrder(expectedTabPagesInOrder);
			}
		}

		public void TestTabPagesOrderWithMiscTabPageSupport()
		{
			var expectedTabPagesInOrder = new string[]
			{
				"MainTabPage", "ServicesTabPage", "TransportAndPackagingTabPage", "HouseConsignmentsTabPage", "MiscTabPage", "MessagesTabPage", "CustomFieldsTabPage", "WorkflowTabPage",
				"BillingTabPage", "eDocsTabPage", "NotesTabPage", "LogsTabPage"
			};

			using (TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(miscTabPageSupport: true))
			{
				AssertTabPagesOrder(expectedTabPagesInOrder);
			}
		}

		[RequiresSTA]
		public void TestTabPagesOrderWithMovements()
		{
			var expectedTabPagesInOrder = new string[]
			{
				"MainTabPage", "ServicesTabPage", "MovementsTabPage", "TransportAndPackagingTabPage", "HouseConsignmentsTabPage", "MessagesTabPage", "CustomFieldsTabPage", "WorkflowTabPage",
				"BillingTabPage", "eDocsTabPage", "NotesTabPage", "LogsTabPage"
			};

			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory, "GetIsMultipleMovementsEnabled", true))
			{
				AssertTabPagesOrder(expectedTabPagesInOrder);
			}
		}

		void AssertTabPagesOrder(string[] expectedTabPagesInOrder)
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertSequencesEqual("TabPageNames", expectedTabPagesInOrder, mainTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name));
			}
		}

		public void TestMenuItems()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				var menuItems = form.Menu.MenuItems;
				AssertSequencesEqual(new[] { "&File", "&Edit", "View", "Actio&ns", "&NCTS", "&Job Invoicing", "&Help" }, menuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestMiscOptionsTabPage()
		{
			using (TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(miscTabPageSupport: true))
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("MiscTabPage.Caption", "Misc.", form.MiscTabPage.CaptionResourceString.Caption);

					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					mainTabControl.SelectedTab = form.MiscTabPage;
					AssertEquals("MiscTabUserControl is within MiscTabPage", true, form.MiscTabPage.Contains(form.MiscTabUserControl));
					AssertEquals("MiscTabUserControl.Dock", DockStyle.Fill, form.MiscTabUserControl.Dock);
					AssertEquals("MiscTabUserControl.DataSourceType", typeof(NctsHeader), form.MiscTabUserControl.BindingSource.DataSourceType);
				});
			}
		}

		public void TestMiscOptionsTabPageVisibilityWhenMiscTabPageIsNotSupported()
		{
			AssertMiscTabPageVisibility(miscTabPageSupport: false);
		}

		[RequiresSTA]
		public void TestMiscOptionsTabPageVisibilityWhenMiscTabPageIsSupported()
		{
			AssertMiscTabPageVisibility(miscTabPageSupport: true);
		}

		void AssertMiscTabPageVisibility(bool miscTabPageSupport)
		{
			using (TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(miscTabPageSupport))
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				AssertEquals("MiscTabPage Visible", miscTabPageSupport, form.MiscTabPage.TabVisible);
			}
		}

		IDisposable TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(bool miscTabPageSupport)
		{
			return NctsConfigurationTestHelper.TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(Factory, miscTabPageSupport);
		}

		public void TestShowPreSaveDialogs_ConditionR0520()
		{
			const string message = "One or more fields of the declaration that have been amended are not allowed to change when the IE015 has already been accepted by customs. Do you wish to continue saving despite this rule?";
			var header = Factory.New<Business.Testing.NctsHeaderForTest>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (IShowPreSaveDialog form = new Phase5DepartureMovementForm(header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				header.IsConditionR0520_UserShouldNotSaveAmendmentsForTest = false;
				var result = form.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertNotEquals("No warning of amendments", message, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				header.IsConditionR0520_UserShouldNotSaveAmendmentsForTest = true;
				result = form.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals("Should warn user of amendments", message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowPreSaveDialogs_ConditionR0520_Guarantees()
		{
			const string message = "Either 'Guarantee' can be amended or 'Transit Operation's fields', both can't be amended simultaneously. Do you wish to continue saving despite this rule?";
			var header = Factory.New<Business.Testing.NctsHeaderForTest>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (IShowPreSaveDialog form = new Phase5DepartureMovementForm(header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				header.IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesForTest = false;
				var result = form.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertNotEquals("No warning of amendments", message, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				header.IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesForTest = true;
				result = form.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals("Should warn user of amendments", message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestTabPages_ShouldBeRegisteredForLock()
		{
			var configs = new DeclarationLockConfigCollection(null, Factory);
			var config = configs.AddNew();
			config.DeclarationType = "Any";
			var tabLockInfo = config.TabInfos.AddNew();
			tabLockInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.All;

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configs))
			{
				using (var form = new Phase5DepartureMovementForm(header))
				{
					var customsControlLockManager = form.LockManager;

					var mainTabPage = GUITestHelper.FindControl<ZTabPage>(form.Controls, "MainTabPage");
					customsControlLockManager.AssertIsRegisteredForLock(mainTabPage, Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureHeader,
						ignoreControlNames: nameof(Phase5DeclarationDetailsTabUserControl.DeclarationDetailsGroupBox));

					customsControlLockManager.AssertIsRegisteredForLock(form.TransportAndPackagingTabPage, Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureTransportAndEquipment);

					customsControlLockManager.AssertIsRegisteredForLock(form.HouseConsignmentsTabPage, Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureHouse,
						ignoreControlNames: nameof(HouseConsignmentsTabUserControl.GoodsItemsTabPage));

					customsControlLockManager.AssertIsRegisteredForLock(form.HouseConsignmentsTabPage, Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureGoodsItem, ignoreControlNames:
						new[]
							{
								nameof(HouseConsignmentsGridUserControl.HouseConsignmentsGrid),
								nameof(HouseConsignmentsTabUserControl.HouseConsignmentDetailsTabPage),
								nameof(HouseConsignmentsTabUserControl.HouseConsignmentSupportingDocumentsTabPage),
								nameof(HouseConsignmentsTabUserControl.HouseConsignmentAdditionalDocumentsTabPage),
								nameof(HouseConsignmentsTabUserControl.HouseConsignmentPreviousDocumentsTabPage),
								nameof(HouseConsignmentsTabUserControl.HouseConsignmentSupplyChainActorsTabPage)
							});
				}
			}
		}

		[RequiresSTA]
		public void TestCanDisplayMessageBuilderMappingPath()
		{
			using var form = new Phase5DepartureMovementForm(header);
			var devToolMessageBuilderPathConfigurator = form as IDevToolMessageBuilderMappingPathConfigurator;

			AssertNotNull("Phase5DepartureMovementForm as Message Builder Mapping Path Configurator", devToolMessageBuilderPathConfigurator);
			AssertEquals("CanDisplayMessageBuilderMappingPath", expected: true, devToolMessageBuilderPathConfigurator.CanDisplayMessageBuilderMappingPath);
		}

		public void TestSave_ShouldNotChangeHeaderAndSetFormToBrowseMode()
		{
			var invoicingJob = new JobHeader.Loader(header).TryLoadOrCreate();
			invoicingJob.JH_GE = Factory.New<GlbDepartment>().PK;

			using var form = new Phase5DepartureMovementForm(header);

			form.Show();
			var result = form.FireSaveButton();

			CombineAssertions(() =>
			{
				AssertEquals("Header HasChanges?", expected: false, header.HasChanges);
				AssertEquals("Form DisplayMode", expected: ODisplayMode.Browse, form.DisplayMode);
			});
		}

		public void TestPlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel_Visibility()
		{
			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory, "GetIsMultipleMovementsEnabled", true))
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");

				var placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel = form.FindSingle<ZPanel>("PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel");
				AssertEquals(true, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				mainTabControl.SelectedTab = form.MessagesTabPage;
				AssertEquals(false, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				mainTabControl.SelectedTab = form.HouseConsignmentsTabPage;
				AssertEquals(true, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				mainTabControl.SelectedTab = form.TransportAndPackagingTabPage;
				AssertEquals(true, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				mainTabControl.SelectedTab = form.MovementsTabPage;
				AssertEquals(true, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				mainTabControl.SelectedTab = form.ServicesTabPage;
				AssertEquals(false, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);
			}
		}

		[RequiresSTA]
		public void TestSynchronizeMonetaryValue_ShouldBeCalledInConstructor()
		{
			// Arrange
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate = NCTSTestHelper.CreateExchangeRate(Factory, 1.5, Core.Constants.CurrencyCodes.UnitedKingdom);
			Factory.Save();

			goodsItem.BY_LinePrice = 15;
			AssertEquals("Precondition", 10m, goodsItem.BY_MonetaryValue);

			exchangeRate.RE_SellRate = 1;

			// Act
			using var form = new Phase5DepartureMovementForm(header);

			//Assert
			AssertEquals("SynchronizeMonetaryValue is called", 15m, goodsItem.BY_MonetaryValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		}

		static void TestUsingForm(NctsHeader header, Action<Phase5DepartureMovementForm> test)
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();
				CombineAssertions(() => test?.Invoke(form));
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		[DeveloperOnlyTest]
		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}

		NctsHeader header;
	}
}
