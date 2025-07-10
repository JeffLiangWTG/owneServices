using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.CNS.ServiceTasks.AirCourier;
using Enterprise.Customs.GB.CNS.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.GB.GUI.Testing
{
	public class EDIMenuTest : TestCaseWithFactory
	{
		public void TestPerformanceViaSendToChiefViaCsp()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MawbTestHelper.MakeBadge("AGI", "CCSUK", true);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			consol1.FillWithValidTestData();
			consol2.FillWithValidTestData();
			var dec = DeclarationChosererTester.CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			dec.JE_OH_Importer = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_OH_Supplier = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_RL_NKPortOfLoading = "GBTST";
			dec.JE_OverrideFreightDefaults = true;
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_JS = shipment.PK;
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			var entry2 = dec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl");

			var controllerId = ZArchitecture.Modules.ZControllerFactory.Instance.GetRegisteredIdentifierByName("JobDeclarationPluggedIntoShipment");
			var controller = ZArchitecture.Modules.ZControllerFactory.Create(controllerId);
			using (var form = controller.ShowEditForm(dec) as ZForm)
			{
				form.Show();
				var allControls = GetAllControls(form);
				var sendToChiefMenu = form.Menu.MenuItems.FindByText("Brokerage").MenuItems.FindByText("CHIEF").MenuItems.FindByText(ChiefEDIMenu.TransferToCustomsCaption);
				AssertNoExceptionThrown(() =>
				{
					using (ZGrid.TrackUpdateGridNotificationType())
					{
						sendToChiefMenu.PerformClick();
					}
				});
			}
		}

		public void TestPerformanceViaSendToCDS()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MawbTestHelper.MakeBadge("AGI", "CCSUK", true);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			consol1.FillWithValidTestData();
			consol2.FillWithValidTestData();
			var dec = DeclarationChosererTester.CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			dec.JE_OH_Importer = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_OH_Supplier = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_RL_NKPortOfLoading = "GBTST";
			dec.JE_OverrideFreightDefaults = true;
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_JS = shipment.PK;
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			var entry2 = dec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl");

			var controllerId = ZArchitecture.Modules.ZControllerFactory.Instance.GetRegisteredIdentifierByName("JobDeclarationPluggedIntoShipment");
			var controller = ZArchitecture.Modules.ZControllerFactory.Create(controllerId);
			using (var form = controller.ShowEditForm(dec) as ZForm)
			{
				form.Show();
				var allControls = GetAllControls(form);
				var sendToCDSMenu = form.Menu.MenuItems.FindByText("Brokerage").MenuItems.FindByText("CDS").MenuItems.FindByText(CDSEDIMenu.SendToCDS);
				AssertNoExceptionThrown(() =>
				{
					using (ZGrid.TrackUpdateGridNotificationType())
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						sendToCDSMenu.PerformClick();
					}
				});
			}
		}

		public void TestWizardMenu()
		{
			var dec = Factory.New<JobDeclaration>();
			var menu = new EDIMenu
			{
				Declaration = dec
			};
			menu.OnPopup(EventArgs.Empty);
			var cdsFsdWizardMenu = menu.MenuItems.FindByText(EDIMenu.CDSFsdWizardCaption, true);
			Assert(cdsFsdWizardMenu.Visible);
		}

		[TestDate(2011, 12, 13)]
		public void TestSendToCDSWithNoEntries()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DeclarationApplicationCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

				var menu = new EDIMenu();
				menu.Declaration = declaration;
				menu.OnPopup(EventArgs.Empty);
				Factory.Save();

				var cdsMenu = menu.MenuItems.FindByText(CDSEDIMenu.CDSMenuCaption);
				var sendMenuItem = cdsMenu.MenuItems.FindByText(CDSEDIMenu.SendToCDS);
				AssertNotNull("Menu Item was found", sendMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();
				AssertContains("There are no entries. Would you like to merge (generate entries) now, save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenuItem.PerformClick();
				AssertContains(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Error when merging {0}", declaration.JobNumber), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2019, 11, 24)]
		public void TestSendToCDSWith_CheckDeniedParty_MenuItemClick()
		{
			Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Registry.Business.DPSFreightMovementRestrictionsOptions.Codes.All);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DeclarationApplicationCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

				var menu = new EDIMenu();
				menu.Declaration = declaration;
				menu.OnPopup(EventArgs.Empty);
				Factory.Save();

				var cdsMenu = menu.MenuItems.FindByText(CDSEDIMenu.CDSMenuCaption);
				var sendMenuItem = cdsMenu.MenuItems.FindByText(CDSEDIMenu.SendToCDS);
				AssertNotNull("Menu Item was found", sendMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				declaration.JE_MessageType = "IMP";
				declaration.JE_ScreeningStatus = "UNK";
				declaration.JE_CustomsProfile = "FEY";
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();

				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				var mergedLine1 = entryHeader1.MergedLines.AddNew();
				invoiceLine1.JI_CL = mergedLine1.PK;
				invoiceLine1.JI_CEI = entryInstruction1.PK;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				var mergedLine2 = entryHeader2.MergedLines.AddNew();
				invoiceLine2.JI_CL = mergedLine2.PK;
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				sendMenuItem.PerformClick();
				Factory.Save();

				var decWrapper = new GB.CDS.JobDeclarationMessageSendingObjectParent(declaration);
				AssertEquals("Sending objects count", 2, decWrapper.SendingObjectsCollection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendMenuItem.PerformClick();

				AssertContains("Unable to submit message due to Denied Party Screening cancellation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(DocumentLoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.JE_ScreeningStatus = "CLR";
				Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				sendMenuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertContains("", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					AssertEquals("Declaration.HasChanges", false, declaration.HasChanges);
				});
			}
		}

		public void TestCDSMenuVisible()
		{
			CombineAssertions(() =>
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var menu = new EDIMenu
				{
					Declaration = dec
				};
				menu.OnPopup(EventArgs.Empty);
				var cdsMenu = menu.MenuItems.FindByText(CDSEDIMenu.CDSMenuCaption);
				AssertEquals("CDS", true, cdsMenu.Visible);

				dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("CHF", false, cdsMenu.Visible);

				dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("ITF", false, cdsMenu.Visible);
			});
		}

		public void TestDeclarationWizardMenuVisibleForCDS()
		{
			CombineAssertions(() =>
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var menu = new EDIMenu
				{
					Declaration = dec
				};
				menu.OnPopup(EventArgs.Empty);
				var cdsMenu = menu.MenuItems.FindByText(CDSEDIMenu.CDSMenuCaption);
				var decWizardMenu = cdsMenu.MenuItems.FindByText(CDSEDIMenu.JobDeclarationWizardCaption);
				AssertNotNull(decWizardMenu);

				var wizardMenu = menu.MenuItems.FindByText("Wizards");
				decWizardMenu = wizardMenu.MenuItems.FindByText(CDSEDIMenu.JobDeclarationWizardCaption);
				AssertEquals("CDS", true, decWizardMenu.Visible);

				dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				menu.OnPopup(EventArgs.Empty);
				wizardMenu = menu.MenuItems.FindByText("Wizards");
				decWizardMenu = wizardMenu.MenuItems.FindByText(CDSEDIMenu.JobDeclarationWizardCaption);
				AssertEquals("CHF", false, decWizardMenu.Visible);

				dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("ITF", false, decWizardMenu.Visible);
			});
		}

		public void TestChiefMenuVisible()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var dec = Factory.New<JobDeclaration>();
					dec.JE_ApplicationCode = "CDS";
					var menu = new EDIMenu
					{
						Declaration = dec
					};
					menu.OnPopup(EventArgs.Empty);
					var chiefMenu = menu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
					AssertEquals("CDS", false, chiefMenu.Visible);

					dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("CHF", true, chiefMenu.Visible);

					dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("ITF", false, chiefMenu.Visible);
				}
			});
		}

		public void TestSupplementaryMenuItemsVisibility()
		{
			using var menu = new EDIMenu();
			var declaration = Factory.New<JobDeclaration>();
			menu.Declaration = declaration;
			var supplementaryEntryMenuItem = menu.MenuItems.FindByText(EU.GUI.EDIMenuCaptions.SupplementaryEntry);
			AssertNotNull("Pre-requisite: SupplementaryEntryMenuItem exists", supplementaryEntryMenuItem);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			menu.RefreshMenu();
			AssertEquals($"{declaration.JE_ApplicationCode}", expected: true, supplementaryEntryMenuItem.Visible);
		}

		public void TestDemAndLemOptionsWarnForNonExports()
		{
			var menu = new EDIMenu();
			menu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			menu.Declaration.JE_CustomsProfile = "FEY";
			menu.Declaration.JE_MessageType = "IMP";
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "GREY");
			Factory.Save();

			var chief = menu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var requestsMenu = chief.MenuItems.FindByText(ChiefEDIMenu.ReportsAndRequestsChief);
			var demMenu = requestsMenu.MenuItems.FindByText(new Interrogate_Dem().MenuCaption);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			demMenu.PerformClick();
			AssertContains("It is only for declarations of type Export", UnitTestUserNotification.Instance.LastMessage.Text);

			var lemMenu = requestsMenu.MenuItems.FindByText(new Interrogate_Lem().MenuCaption);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			lemMenu.PerformClick();
			AssertContains("It is only for declarations of type Export", UnitTestUserNotification.Instance.LastMessage.Text);

			menu.Declaration.JE_MessageType = "EXP";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			demMenu.PerformClick();
			AssertNotContains("It is only for declarations of type Export", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			lemMenu.PerformClick();
			AssertNotContains("It is only for declarations of type Export", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDemRequestCapturesMovementNumber()
		{
			MakeBadgeAndCred(GatewayList.Codes.CCSUKviaNTMsgGW);
			var menu = new EDIMenu();
			menu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			menu.Declaration.JE_CustomsProfile = "FEY";
			menu.Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "GREY");
			Factory.Save();

			var chief = menu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var requestsMenu = chief.MenuItems.FindByText(ChiefEDIMenu.ReportsAndRequestsChief);
			var demMenu = requestsMenu.MenuItems.FindByText(new Interrogate_Dem().MenuCaption);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddUserResponse("12345678");
			demMenu.PerformClick();
			AssertContains("RFF+AES:12345678", menu.Declaration.CustomsEntryHeaders[0].Messages[0].EM_MessageText);
		}

		public void TestSendToChiefClick()
		{
			var menu = new EDIMenu();
			menu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			menu.Declaration.JE_CustomsProfile = "FEY";

			var chief = menu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var exportMenuItem = chief.MenuItems.FindByText(ChiefEDIMenu.TransferToCustomsCaption);
			AssertNotNull("Menu Item was not found", exportMenuItem);

			exportMenuItem.PerformClick();
			AssertContains("Please save first", UnitTestUserNotification.Instance.LastMessage.Text);

			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			exportMenuItem.PerformClick();
			AssertNotContains("Please save first", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCancelDeclarationButtonOnlyDoesStuffIfDeclarationSent()
		{
			var ediMenu = new EDIMenu();

			string expectedMessage = "There are no entries with an entry number, so there is nothing to cancel. No cancellation message will be sent.";

			JobDeclaration dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_CustomsProfile = "FEY";
			ediMenu.Declaration = dec;
			Factory.Save();

			// Assert we CANNOT see the menu:
			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var requestsMenu = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.ReportsAndRequestsChief);
			var xtcMenu = requestsMenu.MenuItems.FindByText(ChiefEDIMenu.CancelDeclarationCaption);
			AssertNotNull("Cancel menu visible even before we have transmitted the cusdec", xtcMenu);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			xtcMenu.PerformClick();
			AssertEquals("Check that clicking the XTC button before the Dec is sent will show a warning", UnitTestUserNotification.Instance.LastMessage.Text, expectedMessage);

			// Only when we have an entry should we see the cancellation menu
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "ABC123";
			Assert(dec.CustomsEntryHeaders.Count > 0);
			ediMenu.Declaration = dec;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			xtcMenu.PerformClick();
			AssertEquals("Check that clicking the XTC button AFTER the Dec is sent will not show a warning",
							UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage),
							false
							);
		}

		public void TestTransferToCustomsNotVisible()
		{
			var ediMenu = new EDIMenu();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			ediMenu.Declaration = dec;
			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var transferToCustomsCaptionMenu = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.TransferToCustomsCaption);
			AssertEquals("Transfer to Customs menu should not be visible", false, transferToCustomsCaptionMenu.Visible);
		}

		public void TestReportsAndRequestsChiefNotVisible()
		{
			var ediMenu = new EDIMenu();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			ediMenu.Declaration = dec;
			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var reportsAndRequestsChiefMenu = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.ReportsAndRequestsChief);
			AssertEquals("Reports and Requests menu should not be visible", false, reportsAndRequestsChiefMenu.Visible);
		}

		public void TestExportMenuShownOnlyIfDeclarationTypeIsExport()
		{
			var ediMenu = new EDIMenu();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			ediMenu.Declaration = dec;
			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var exportMasterFunctionsMenu = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.InventoryManagementCaption);
			AssertEquals("Export Master Functions menu should be visible, Export, CCSUK", true, exportMasterFunctionsMenu.Visible);
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			ediMenu.RefreshMenu();
			AssertEquals("Export Master Functions menu should not be visible, Import", false, exportMasterFunctionsMenu.Visible);
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			ediMenu.RefreshMenu();
			AssertEquals("Export Master Functions menu should be visible, Export, CCSUK", true, exportMasterFunctionsMenu.Visible);
			dec.ZG_Gateway = GatewayList.Codes.CDS;
			ediMenu.RefreshMenu();
			AssertEquals("Export Master Functions menu should not be visible, Export, CDS", false, exportMasterFunctionsMenu.Visible);
		}

		public void TestArriveDepartMenuForDEPAndMaritimeLoader()
		{
			MawbTestHelper.MakeBadge("AAA", "CCSUK", true);
			MawbTestHelper.MakeDepBadge("XBB", "CCSUK", true);

			MawbTestHelper.MakeBadge("CCC", GatewayList.Codes.CNS_CUSDECOnly, "", company: "ABC", makeCredentialToo: true, isMartimeLoader: false);
			MawbTestHelper.MakeBadge("DDD", GatewayList.Codes.CNS_CUSDECOnly, "", company: "ABC", makeCredentialToo: true, isMartimeLoader: true);

			var ediMenu = new EDIMenu();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			ediMenu.Declaration = dec;
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var arriveDepartMenu = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.ArriveAndDepartCaption);
			var edl = arriveDepartMenu.MenuItems.FindByText(new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration).FunctionHuman);
			var eal = arriveDepartMenu.MenuItems.FindByText(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration).FunctionHuman);
			var eaa = arriveDepartMenu.MenuItems.FindByText(new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration).FunctionHuman);
			dec.JE_CustomsProfile = "AAA";
			ediMenu.RefreshMenu();
			AssertEquals(false, edl.Enabled);
			AssertEquals(false, eaa.Enabled);
			AssertEquals(false, eal.Enabled);
			dec.JE_CustomsProfile = "XBB";
			ediMenu.RefreshMenu();
			AssertEquals(true, edl.Enabled);
			AssertEquals(true, eaa.Enabled);
			AssertEquals(true, eal.Enabled);
			dec.JE_CustomsProfile = "CCC";
			ediMenu.RefreshMenu();
			AssertEquals(false, edl.Enabled);
			AssertEquals(false, eaa.Enabled);
			AssertEquals(false, eal.Enabled);
			dec.JE_CustomsProfile = "DDD";
			ediMenu.RefreshMenu();
			AssertEquals(true, edl.Enabled);
			AssertEquals(true, eaa.Enabled);
			AssertEquals(true, eal.Enabled);
		}

		public void TestClickingMucrFunctionButtonActuallyKicksOffTheMessageSendingProcess()
		{
			var ediMenu = new EDIMenu();
			MakeBadgeAndCred();
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateImportAirDeclaration();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "sdsdsd";
			entry.CH_CustomsMessageRemarks = "Sod this";
			declaration.JE_RL_NKPortOfArrival = "GBLHR";
			declaration.JE_CustomsProfile = "FEY";
			declaration.JE_MasterUCR = "Do me baby";
			Factory.Save();
			ediMenu.Declaration = declaration;
			GB.Registry.GBCustomsDataRegistry.Instance.McpDestin8Url = "https://CatsLikePlainCrisps.com";

			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var mucrFunctions = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.InventoryManagementCaption);
			var closeMenuItem = mucrFunctions.MenuItems.FindByText(ChiefEDIMenu.MucrManagementClose);
			int messageCounter = declaration.CustomsEntryHeaders[0].Messages.Count;
			closeMenuItem.PerformClick();
			AssertEquals("Close. Checking that something has happened when we click the button. If this fails, it might be because our validation has become more restrictive and we're actually not 'OK to send' due to incorrect or insufficient data in your test declaration.", 1 + messageCounter, declaration.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals(GbCusDecMessageFunctionsList.Descriptions.Close, declaration.CustomsEntryHeaders[0].Logs.MostRecentLog.SL_Reference);
			var assMenuItem = mucrFunctions.MenuItems.FindByText(ChiefEDIMenu.MucrManagementAssociate);
			messageCounter = declaration.CustomsEntryHeaders[0].Messages.Count;
			assMenuItem.PerformClick();
			AssertEquals("Associate. Checking that something has happened when we click the button. If this fails, it might be because our validation has become more restrictive and we're actually not 'OK to send' due to incorrect or insufficient data in your test declaration.", 1 + messageCounter, declaration.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals(GbCusDecMessageFunctionsList.Descriptions.Associate, declaration.CustomsEntryHeaders[0].Logs.MostRecentLog.SL_Reference);
			var disMenuItem = mucrFunctions.MenuItems.FindByText(ChiefEDIMenu.MucrManagementDisassociate);
			messageCounter = declaration.CustomsEntryHeaders[0].Messages.Count;
			disMenuItem.PerformClick();
			AssertEquals("DisAssociate. Checking that something has happened when we click the button. If this fails, it might be because our validation has become more restrictive and we're actually not 'OK to send' due to incorrect or insufficient data in your test declaration.", 1 + messageCounter, declaration.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals(GbCusDecMessageFunctionsList.Descriptions.Disassociate, declaration.CustomsEntryHeaders[0].Logs.MostRecentLog.SL_Reference);
		}

		public void TestClickingCancelDeclarationButtonActuallyKicksOffTheMessageSendingProcess()
		{
			var ediMenu = new EDIMenu();
			MakeBadgeAndCred();
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateImportAirDeclaration();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "sdsdsd";
			entry.CH_CustomsMessageRemarks = "Sod this";
			declaration.JE_RL_NKPortOfArrival = "GBLHR";
			declaration.JE_CustomsProfile = "FEY";
			declaration.JE_MasterUCR = "Do me baby";
			Factory.Save();
			ediMenu.Declaration = declaration;
			GB.Registry.GBCustomsDataRegistry.Instance.McpDestin8Url = "https://CatsLikePlainCrisps.com";

			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var requestsAndReports = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.ReportsAndRequestsChief);
			var xtcMenu = requestsAndReports.MenuItems.FindByText(ChiefEDIMenu.CancelDeclarationCaption);
			int messageCounter = declaration.CustomsEntryHeaders[0].Messages.Count;
			xtcMenu.PerformClick();
			AssertEquals("Checking that something has happened when we click the button. If this fails, it might be because validation has become more restrictive and we're actually not 'OK to send' due to incorrect or insufficient data in your test declaration.", 1 + messageCounter, declaration.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestAccessingWhenDeclarationIsNotSet()
		{
			var menu = new EDIMenu();
			AssertNoExceptionThrown("Acessing menu when declaration is not set like from Shipment should work", () => menu.RefreshMenu());
		}

		public void TestEntryLevelChiefMenuSendsMessageOnlyForSelectedEntryAndNotAllEntriesAndThatExportMucrFunctionsAreAvailable()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MawbTestHelper.MakeBadge("AGI", "CCSUK", true);
			var dec = DeclarationChosererTester.CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			dec.JE_OH_Importer = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_OH_Supplier = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_RL_NKPortOfLoading = "GBTST";
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			var entry2 = dec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl");
			using (var form = new JobDeclarationForm(dec))
			{
				// Everything inide this USING is pug fugly, but it very accurately mimics a true user experience (instead of the usual fake crap that looks nothing like what really happens). // Awww
				form.Show();
				var allControls = GetAllControls(form);
				var entryTab = allControls.OfType<TabPage>().First(x => x.Text == "Entries");
				entryTab.Show();
				var entryTabControl = (TabControl)entryTab.Parent;
				entryTabControl.Show();
				entryTabControl.SelectTab(entryTab);
				var entriesGrid = GetAllControls(entryTab).OfType<ZGrid>().First(x => x.Name == "EntriesBoundGrid");
				entriesGrid.Show();
				entriesGrid.Select(1);  // Select entry row
				entriesGrid.ContextMenu.OnPopup_ForTest();
				var chiefMenu = (ChiefEDIMenu)entriesGrid.ContextMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
				AssertEquals(true, chiefMenu.Visible);
				chiefMenu.RefreshMenu();
				var sendToChiefMenu = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.TransferToCustomsCaption);
				var gbMessageUserControl = (MessageUserControl)form.Controls.Find("MessageUserControl", true).First();
				gbMessageUserControl.SetMenusEntryOnPopup(null, null); // to simulate ContextMenu.Show() by user right-clicking the grid - cannot use Show() because it blocks until the menu disappears when focus is lost
				sendToChiefMenu.PerformClick();
				AssertEquals(1, entry2.Messages.Count);
				AssertEquals(0, entry1.Messages.Count);
				AssertEquals(false, entry1.DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries);
				AssertEquals(false, entry2.DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries);

				chiefMenu.RefreshMenu();
				AssertNotNull(chiefMenu.Declaration);
				var exportMucr = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.InventoryManagementCaption);
				AssertNotNull(exportMucr);
			}
		}

		public void TestCnsCourierClick()
		{
			MakeBadgeAndCred(GatewayList.Codes.CNS_CUSDECOnly);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_TransportMode = "AIR";
			dec.JE_CustomsProfile = "FEY";
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			Factory.Save();

			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				var ediMenu = new GbCnsAirCourierMenuForTest(form);
				ediMenu.Declaration = dec;
				ediMenu.MenuItems[0].PerformClick();
				AssertEquals(2, dec.Messages.Count);
				AssertContains("Courier manifest message was created", "AirImportManifest", dec.Messages[0].EM_MessageText);
			}
		}

		public void TestCnsCourierShow()
		{
			var ediMenu = new EDIMenu();
			MakeBadgeAndCred(GatewayList.Codes.CNS_CUSDECOnly);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_TransportMode = "AIR";
			dec.JE_CustomsProfile = "FEY";
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			ediMenu.Declaration = dec;
			var chiefMenu = ediMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
			var cnsMenu = chiefMenu.MenuItems.FindByText(GbCnsAirCourierMenu.CnsCourierCaption);
			ediMenu.RefreshMenu();
			AssertEquals(true, cnsMenu.Enabled);
		}

		public void TestMCPClaimUCNMenuItemsVisible_ClaimUCN_Interface()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				declaration.JE_MessageType = "IMP";
				declaration.JE_TransportMode = "SEA";
				declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
				declaration.CusContainers.AddNew();
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				ediMenu.RefreshMenu();
				var claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
				AssertEquals(false, claimUCNMenu.Visible);
			}
		}

		public void TestMCPClaimUCNMenuItemsVisible_ClaimUCNAmalgamate_Interface()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				declaration.JE_MessageType = "IMP";
				declaration.JE_TransportMode = "SEA";
				declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
				declaration.CusContainers.AddNew();
				declaration.CusContainers.AddNew();
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				ediMenu.RefreshMenu();
				var claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
				AssertEquals(false, claimUCNAmalgamateMenu.Visible);
			}
		}

		public void TestMCPClaimUCNMenuItemsVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var ediMenu = new EDIMenu();
			var claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
			var claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
			AssertNotNull("Functionality valid", claimUCNMenu);
			AssertNull("Functionality valid", claimUCNAmalgamateMenu);

			ediMenu = new EDIMenu();
			dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CHF";
			dec.JE_MessageType = "IMP";
			dec.JE_TransportMode = "SEA";
			dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			ediMenu.Declaration = dec;
			claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
			claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
			ediMenu.RefreshMenu();
			AssertEquals("MCP - Import - 0 Containers", false, claimUCNMenu?.Visible ?? false);
			AssertEquals("MCP - Import - 0 Containers", false, claimUCNAmalgamateMenu?.Visible ?? false);

			dec.CusContainers.AddNew();
			ediMenu.RefreshMenu();
			claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
			claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
			AssertEquals("MCP - Import - 1 Container", true, claimUCNMenu?.Visible ?? false);
			AssertEquals("MCP - Import - 1 Container", false, claimUCNAmalgamateMenu?.Visible ?? false);

			dec.CusContainers.AddNew();
			ediMenu.RefreshMenu();
			claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
			claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
			AssertEquals("MCP - Import - 2 Containers", false, claimUCNMenu?.Visible ?? false);
			AssertEquals("MCP - Import - 2 Containers", true, claimUCNAmalgamateMenu?.Visible ?? false);

			dec.ZG_Gateway = GatewayList.Codes.Pentant;
			ediMenu.RefreshMenu();
			claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
			claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
			AssertEquals("PNT - Import - 2 Containers", false, claimUCNMenu?.Visible ?? false);
			AssertEquals("PNT - Import - 2 Containers", false, claimUCNAmalgamateMenu?.Visible ?? false);

			dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			ediMenu.RefreshMenu();
			claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
			claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
			AssertEquals("MCP - Import - 2 Containers", false, claimUCNMenu?.Visible ?? false);
			AssertEquals("MCP - Import - 2 Containers", true, claimUCNAmalgamateMenu?.Visible ?? false);

			dec.JE_MessageType = "EXP";
			ediMenu.RefreshMenu();
			claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
			claimUCNAmalgamateMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNAmalgamateCaption);
			AssertEquals("MCP - Export - 2 Containers", false, claimUCNMenu?.Visible ?? false);
			AssertEquals("MCP - Export - 2 Containers", false, claimUCNAmalgamateMenu?.Visible ?? false);
		}

		public void TestMCPClaimUCN()
		{
			using (SetTestBranchCredential())
			{
				var dec = Factory.New<JobDeclaration>();
				using (var form = new JobDeclarationForm(dec))
				{
					form.Show();
					var ediMenu = new EDIMenuForTest(form);
					var claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
					AssertNotNull($"Pre-requisite: menu item default caption is {EDIMenu.ClaimUCNCaption}", claimUCNMenu);

					dec.JE_MessageType = "IMP";
					dec.JE_TransportMode = "SEA";
					dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
					dec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
					dec.JE_CustomsProfile = "CAW";
					ediMenu.Declaration = dec;

					dec.CusContainers.AddNew();
					ediMenu.RefreshMenu();
					claimUCNMenu.PerformClick();
					AssertEquals(UCNMessageCreationSuccess, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("EDIMessage added to declaration", 1, dec.Messages.Count);
				}
			}
		}

		public void TestMCPClaimUCNWithNotFclAndNotLcl()
		{
			using (SetTestBranchCredential())
			{
				var dec = Factory.New<JobDeclaration>();
				using (var form = new JobDeclarationForm(dec))
				{
					form.Show();
					var ediMenu = new EDIMenuForTest(form);
					var claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
					AssertNotNull($"Pre-requisite: menu item default caption is {EDIMenu.ClaimUCNCaption}", claimUCNMenu);

					dec.JE_MessageType = "IMP";
					dec.JE_TransportMode = "SEA";
					dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
					dec.JE_ContainerMode = Core.Constants.ContainerModes.Containerised; //CNT
					dec.JE_CustomsProfile = "CAW";
					ediMenu.Declaration = dec;

					dec.CusContainers.AddNew();
					ediMenu.RefreshMenu();
					claimUCNMenu.PerformClick();
					AssertEquals(UCNFunctionNotSupportedError, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("No EDIMessage added to declaration", 0, dec.Messages.Count);
				}
			}
		}

		public void TestMCPClaimUCNWithNotFclAndNotLclButOneFclContainer()
		{
			using (SetTestBranchCredential())
			{
				var dec = Factory.New<JobDeclaration>();
				using (var form = new JobDeclarationForm(dec))
				{
					form.Show();
					var ediMenu = new EDIMenuForTest(form);
					var claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
					AssertNotNull($"Pre-requisite: menu item default caption is {EDIMenu.ClaimUCNCaption}", claimUCNMenu);

					dec.JE_MessageType = "IMP";
					dec.JE_TransportMode = "SEA";
					dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
					dec.JE_ContainerMode = Core.Constants.ContainerModes.Containerised; //CNT
					dec.JE_CustomsProfile = "CAW";
					ediMenu.Declaration = dec;

					var cnt = dec.CusContainers.AddNew();
					cnt.CO_FCL_LCL_AIR = "FCL";
					ediMenu.RefreshMenu();
					claimUCNMenu.PerformClick();
					AssertEquals(UCNMessageCreationSuccess, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("EDIMessage added to declaration", 1, dec.Messages.Count);
				}
			}
		}

		public void TestMCPClaimUCNWithNoCredentials()
		{
			var dec = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				var ediMenu = new EDIMenuForTest(form);
				var claimUCNMenu = ediMenu.MenuItems.FindByText(EDIMenu.ClaimUCNCaption);
				AssertNotNull($"Pre-requisite: menu item default caption is {EDIMenu.ClaimUCNCaption}", claimUCNMenu);

				var testBadge = "CAW";
				dec.JE_MessageType = "IMP";
				dec.JE_TransportMode = "SEA";
				dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
				dec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				dec.JE_CustomsProfile = testBadge;
				ediMenu.Declaration = dec;

				dec.CusContainers.AddNew();
				ediMenu.RefreshMenu();
				claimUCNMenu.PerformClick();
				var expectedMessage = ZString.Format(CredentialsNotFoundError, testBadge);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No EDIMessage added to declaration", 0, dec.Messages.Count);
			}
		}

		public void TestPentantRegenerateACA()
		{
			var badge = new BadgeCodeSetting();
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badge.BadgeCode = "DJC";
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;
			credential.PIMA = "CUKFFW98000DAN";
			credential.Company = "ABC";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credentials = new CredentialsSettingCollection();
			credentials.Add(credential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_TransportMode = "AIR";
			dec.JE_CustomsProfile = "DJC";
			dec.ZG_Gateway = GatewayList.Codes.Pentant;
			Factory.Save();
			AssertEquals("ABC00001M", dec.JE_ACAReference);

			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				var ediMenu = new GbViaPentantMenuForTest(form);
				ediMenu.Declaration = dec;
				ediMenu.MenuItems[0].PerformClick();
				AssertEquals("ABC00002M", dec.JE_ACAReference);
			}
		}

		public void TestPentantAmendCargoReport()
		{
			MakeBadgeAndCred(GatewayList.Codes.Pentant);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_TransportMode = "AIR";
			dec.JE_CustomsProfile = "FEY";
			dec.ZG_Gateway = GatewayList.Codes.Pentant;
			var entry = dec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("FEY00001M", dec.JE_ACAReference);

			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				var ediMenu = new GbViaPentantMenuForTest(form);
				ediMenu.Declaration = dec;
				ediMenu.MenuItems[2].PerformClick();
				entry.Messages.Load();
				AssertEquals(1, entry.Messages.Count);
				AssertStartsWith("Cargo report amend message was created", "BEGINMESSAGE~", entry.Messages[0].EM_MessageText);
			}
		}

		public void TestPentantMenuOnlyShownForPentantJobs()
		{
			MakeBadgeAndCred(GatewayList.Codes.Pentant);

			var menu = new EDIMenu();
			menu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			menu.Declaration.JE_CustomsProfile = "FEY";
			menu.Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			menu.Declaration.ZG_Gateway = GatewayList.Codes.Pentant;
			var entry = menu.Declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			menu.RefreshMenu();
			var pentantMenu = menu.MenuItems.FindByText("Pentant");
			Assert(pentantMenu.Visible);

			MakeBadgeAndCred(GatewayList.Codes.MCP_CUSDECOnly, "ABC");

			menu = new EDIMenu();
			menu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			menu.Declaration.JE_CustomsProfile = "ABC";
			menu.Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			menu.Declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			entry = menu.Declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			menu.RefreshMenu();

			pentantMenu = menu.MenuItems.FindByText("Pentant");
			Assert(!pentantMenu.Visible);

			menu.Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			menu.OnPopup(EventArgs.Empty);
			AssertEquals("ITF", false, pentantMenu.Visible);
		}

		public void TestPentantRequestCargoReport()
		{
			MakeBadgeAndCred(GatewayList.Codes.Pentant);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_TransportMode = "AIR";
			dec.JE_CustomsProfile = "FEY";
			dec.ZG_Gateway = GatewayList.Codes.Pentant;
			var entry = dec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("FEY00001M", dec.JE_ACAReference);

			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				var ediMenu = new GbViaPentantMenuForTest(form);
				ediMenu.Declaration = dec;
				ediMenu.MenuItems[1].PerformClick();
				entry.Messages.Load();
				AssertEquals(1, entry.Messages.Count);
				AssertStartsWith("Cargo message was created", "BEGINMESSAGE~", entry.Messages[0].EM_MessageText);
			}
		}

		public void TestEntryLevelChiefMenuSendsMessageOnlyForSelectedEntryAndNotAllEntries_Cancel()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			MawbTestHelper.MakeBadge("AGI", "CCSUK", true);
			var dec = DeclarationChosererTester.CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			dec.JE_OH_Importer = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_OH_Supplier = GlbCompany.CurrentCompany.OrgProxy.PK;
			dec.JE_RL_NKPortOfLoading = "GBTST";
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			var entry2 = dec.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "071-123456A";
			entry2.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
			entry1.CH_CustomsMessageRemarks = "please cancel";
			entry2.CH_CustomsMessageRemarks = "please cancel";
			Factory.Save();
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl");
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				var allControls = GetAllControls(form);
				var entryTab = allControls.OfType<TabPage>().FirstOrDefault(x => x.Text == "Entries");
				entryTab.Show();
				var entryTabControl = (TabControl)entryTab.Parent;
				entryTabControl.Show();
				entryTabControl.SelectTab(entryTab);
				var entriesGrid = GetAllControls(entryTab).OfType<ZGrid>().FirstOrDefault(x => x.Name == "EntriesBoundGrid");
				entriesGrid.Show();

				entriesGrid.Select(1);  // select CANCELLABLE entry
				var chiefMenu = (ChiefEDIMenu)entriesGrid.ContextMenu.MenuItems.FindByText(ChiefEDIMenu.ChiefMenuCaption);
				chiefMenu.RefreshMenu();
				var requestsMenu = chiefMenu.MenuItems.FindByText(ChiefEDIMenu.ReportsAndRequestsChief);
				var xtcMenu = requestsMenu.MenuItems.FindByText(ChiefEDIMenu.CancelDeclarationCaption);
				var gbMessageUserControl = (MessageUserControl)form.Controls.Find("MessageUserControl", true).First();
				gbMessageUserControl.SetMenusEntryOnPopup(null, null);
				xtcMenu.PerformClick();
				AssertEquals(1, entry2.Messages.Count);
				AssertEquals(0, entry1.Messages.Count);

				entriesGrid.Select(0);  // select NON-CANCELLABLE entry
				gbMessageUserControl.SetMenusEntryOnPopup(null, null);
				xtcMenu.PerformClick();
				AssertEquals(1, entry2.Messages.Count);
				AssertEquals(0, entry1.Messages.Count);
				AssertContains("there is nothing to cancel", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCDSQueryMenu()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var menu = new EDIMenu
			{
				Declaration = dec
			};
			menu.OnPopup(EventArgs.Empty);
			var cdsMenu = menu.MenuItems.FindByText(CDSEDIMenu.CDSMenuCaption);

			var queryMenu = cdsMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryMenuCaption);
			AssertNotNull("CDS Query Menu not found", queryMenu);
			AssertEquals("Query Menu not Visible", true, queryMenu.Visible);
			AssertEquals("Should be disabled without CEI", false, queryMenu.Enabled);

			var cei1 = dec.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "H1";

			var inv1 = dec.Invoices.AddNew();
			var line1 = inv1.InvoiceLines.AddNew();
			line1.JI_CEI = cei1.PK;

			var doc1 = dec.PreviousDocuments.AddNew();
			doc1.CSI_Code = "DCR";
			doc1.CSI_ReferenceNumber = "UNITTEST/00001";

			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			dec.DoMerge();
			Factory.Save();

			menu.OnPopup(EventArgs.Empty);

			var menuMRN = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryMRNMenuCaption);
			var menuMRNSummary = menuMRN?.MenuItems.FindByText(CDSEDIMenu.CDSQueryMRNMenuSummaryCaption);
			var menuMRNSnapshot = menuMRN?.MenuItems.FindByText(CDSEDIMenu.CDSQueryMRNMenuSnapshotCaption);
			var menuDUCR = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryDUCRMenuCaption);
			var menuInventory = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryInventoryMenuCaption);
			var menuUCR = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryUCRMenuCaption);

			CombineAssertions(() =>
			{
				AssertNotNull("MRN Menu not found", menuMRN);
				AssertNotNull("MRN-Summary Menu not found", menuMRNSummary);
				AssertNotNull("MRN-Snapshot Menu not found", menuMRNSnapshot);
				AssertNotNull("DUCR Menu not found", menuDUCR);
				AssertNotNull("Inventory Menu not found", menuInventory);
				AssertNotNull("UCR Menu not found", menuUCR);
			});

			CombineAssertions(() =>
			{
				AssertEquals("MRN Menu not Visible", expected: true, menuMRN.Visible);
				AssertEquals("MRN Menu should not have sub-menus", 2, menuMRN.MenuItems.Count);
				AssertEquals("MRN Menu Caption", "By movement reference (MRN)", menuMRN.Text);

				AssertEquals("DUCR Menu not Visible", expected: true, menuDUCR.Visible);
				AssertEquals("DUCR Menu should not have sub-menus", 0, menuDUCR.MenuItems.Count);
				AssertEquals("DUCR Menu Caption", "By previous document type DCR (DUCR)", menuDUCR.Text);

				AssertEquals("Inventory Menu not Visible", expected: true, menuInventory.Visible);
				AssertEquals("Inventory Menu should not have sub-menus", 0, menuInventory.MenuItems.Count);
				AssertEquals("Inventory Menu Caption", "By inventory reference (MUCR)", menuInventory.Text);

				AssertEquals("UCR Menu not Visible", expected: true, menuUCR.Visible);
				AssertEquals("UCR Menu should not have sub-menus", 0, menuUCR.MenuItems.Count);
				AssertEquals("UCR Menu Caption", "By entry reference (data element 2/4 - UCR)", menuUCR.Text);

				AssertEquals("MRN-Summary Menu not Visible", expected: true, menuMRNSummary.Visible);
				AssertEquals("MRN-Summary Menu should not have sub-menus", 0, menuMRNSummary.MenuItems.Count);
				AssertEquals("MRN-Summary Menu Caption", "Summary with Status Update", menuMRNSummary.Text);

				AssertEquals("MRN-Snapshot Menu not Visible", expected: true, menuMRNSnapshot.Visible);
				AssertEquals("MRN-Snapshot Menu should not have sub-menus", 0, menuMRNSnapshot.MenuItems.Count);
				AssertEquals("MRN-Snapshot Menu Caption", "Snapshot", menuMRNSnapshot.Text);
			});

			var cei2 = dec.CustomsEntryInstructions.AddNew();
			cei2.CEI_Style = "H2";

			var inv2 = dec.Invoices.AddNew();
			var line2 = inv2.InvoiceLines.AddNew();
			line2.JI_CEI = cei2.PK;

			dec.DoMerge();
			Factory.Save();

			AssertEquals("Should have 2 entry headers", 2, dec.ActiveEntryHeaders.Count);
			var entry1 = (Business.Declaration.CusEntryHeader)dec.ActiveEntryHeaders[0];
			var entry2 = (Business.Declaration.CusEntryHeader)dec.ActiveEntryHeaders[1];
			entry1.MovementReferenceNumberSetter("MRN123456789", ZDateTime.Now);

			menu.OnPopup(EventArgs.Empty);

			menuMRN = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryMRNMenuCaption);
			menuMRNSummary = menuMRN?.MenuItems.FindByText(CDSEDIMenu.CDSQueryMRNMenuSummaryCaption);
			menuMRNSnapshot = menuMRN?.MenuItems.FindByText(CDSEDIMenu.CDSQueryMRNMenuSnapshotCaption);
			menuDUCR = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryDUCRMenuCaption);
			menuInventory = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryInventoryMenuCaption);
			menuUCR = queryMenu.MenuItems.FindByText(CDSEDIMenu.CDSQueryUCRMenuCaption);

			var ducrMenu1Caption = entry1.EntryTypeFriendlyName + " UNITTEST/00001";
			var ucrMenu1Caption = entry1.EntryTypeFriendlyName + " " + entry1.CH_BGMReference;

			CombineAssertions(() =>
			{
				AssertEquals("MRN Menu not Visible", expected: true, menuMRN.Visible);
				AssertEquals("MRN Menu should have sub-menus", 2, menuMRN.MenuItems.Count);

				AssertEquals("MRN-Summary Menu not Visible", expected: true, menuMRNSummary.Visible);
				AssertEquals("MRN-Summary Menu should have sub-menus", 2, menuMRNSummary.MenuItems.Count);
				AssertEquals("MRN-Summary/MRN 1", entry1.EntryTypeFriendlyName + " MRN123456789", menuMRNSummary.MenuItems[0].Text);
				AssertEquals("MRN-Summary/MRN 2", entry2.EntryTypeFriendlyName + " (No MRN)", menuMRNSummary.MenuItems[1].Text);

				AssertEquals("MRN-Snapshot Menu not Visible", expected: true, menuMRNSnapshot.Visible);
				AssertEquals("MRN-Snapshot Menu should have sub-menus", 2, menuMRNSnapshot.MenuItems.Count);
				AssertEquals("MRN-Snapshot/MRN 1", entry1.EntryTypeFriendlyName + " MRN123456789", menuMRNSnapshot.MenuItems[0].Text);
				AssertEquals("MRN-Snapshot/MRN 2", entry2.EntryTypeFriendlyName + " (No MRN)", menuMRNSnapshot.MenuItems[1].Text);

				AssertEquals("DUCR Menu not Visible", expected: true, menuDUCR.Visible);
				AssertEquals("DUCR Menu should have sub-menus", 2, menuDUCR.MenuItems.Count);
				AssertEquals("DUCR1 Caption", ducrMenu1Caption, menuDUCR.MenuItems[0].Text);

				AssertEquals("Inventory Menu not Visible", expected: true, menuInventory.Visible);
				AssertEquals("Inventory Menu should have sub-menus", 2, menuInventory.MenuItems.Count);

				AssertEquals("UCR Menu not Visible", expected: true, menuUCR.Visible);
				AssertEquals("UCR Menu should have sub-menus", 2, menuUCR.MenuItems.Count);
				AssertEquals("UCR1 Caption", ucrMenu1Caption, menuUCR.MenuItems[0].Text);
			});
		}

		class EDIMenuForTest : EDIMenu
		{
			public EDIMenuForTest(ZForm form)
			{
				this.form = form;
			}

			protected override ZForm FormInternal
			{
				get { return form ?? base.Form; }
			}
			readonly ZForm form;
		}

		class GbViaPentantMenuForTest : GBViaPentantMenu
		{
			public GbViaPentantMenuForTest(ZForm form)
			{
				this.form = form;
			}

			protected override ZForm FormInternal
			{
				get { return form ?? base.Form; }
			}
			readonly ZForm form;
		}

		class GbCnsAirCourierMenuForTest : GbCnsAirCourierMenu
		{
			public GbCnsAirCourierMenuForTest(ZForm form)
			{
				this.form = form;
			}

			protected override AirCourierFromDeclaration GetNewGenerator(AirCourierFromDeclaration.AirCourierMessageTypes how)
			{
				return new AirCourierFromDeclarationForTestDontReallyConnect(Declaration, how, AirCourierFromDeclarationForTestDontReallyConnect.ResponseTypesForTest.Success0000);
			}

			protected override ZForm FormInternal
			{
				get { return form ?? base.Form; }
			}
			readonly ZForm form;
		}

		IEnumerable<Control> GetAllControls(Control container)
		{
			List<Control> controlList = new List<Control>();
			foreach (Control c in container.Controls)
			{
				controlList.AddRange(GetAllControls(c));
				controlList.Add(c);
			}
			return controlList;
		}

		protected override void SetUp()
		{
			base.SetUp();

			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "FEY";
			badgeCodeSetting.RL_PortCode = "GBLHR";
			badgeCodeSetting.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
		}

		public static void MakeBadgeAndCred(string csp = GatewayList.Codes.MCP_CUSDECOnly, string badge = "FEY")
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = badge;
			badgeCodeSetting.RL_PortCode = "GBLHR";
			badgeCodeSetting.CSPCode = csp;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var cred = credentials.AddNew();
			cred.BadgeCode = badge;
			cred.Company = badge;
			cred.Printer = "X";
			cred.Username = "Y";
			cred.Password = "Z";
			cred.SenderID = "SSS";
			cred.ReceiverID = "RRR";
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);
		}

		IDisposable SetTestBranchCredential(string badge = "CAW", string device = "CAW3")
		{
			var creds = new McpIslCredentialsSetting()
			{
				McpIslCompanyCode = badge,
				McpIslDevice = device
			};
			return GBCustomsDataRegistry.Instance.McpIslWebServiceCredentialsSet.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new McpIslCredentialsSettingCollection() { creds });
		}

		const string CredentialsNotFoundError = "No MCP ISL credential was found for company {0} for this declaration’s branch. Please ensure a credential record is supplied in the registry using the correct branch and company (badge) code.";
		const string UCNMessageCreationSuccess = "CSN message created successfully";
		const string UCNFunctionNotSupportedError = "This function is only supported for FCL and LCL modes";
	}

	public class EDIMenuZZCustomsFunctionalityTest : ZZCustomsFunctionalityEffectiveDateTests
	{
		[TestDate(2011, 12, 13)]
		public void TestSendToCDS_BetweenEffectiveDates()
		{
			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var isDevOrSupport = SetupCurrentUserAsDeveloper(Factory, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DeclarationApplicationCodeExports, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertIsCDSFunctionalityEnabled(Factory, true, "20111213", isDevOrSupport, true);
			}
		}

		[TestDate(2011, 12, 16)]
		public void TestSendToCDS_DeveloperOrSupport()
		{
			var isDevOrSupport = SetupCurrentUserAsDeveloper(Factory, true);
			AssertIsCDSFunctionalityEnabled(Factory, true, "20111216", isDevOrSupport, false);
		}

		public static bool SetupCurrentUserAsDeveloper(BusinessObjectFactory factory, bool isDevOrSupport = true)
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = isDevOrSupport;
			factory.Save();
			return isDevOrSupport;
		}

		public static void AssertIsCDSFunctionalityEnabled(BusinessObjectFactory testFactory, ZBool assertCDSIsEnabledAndNoMessage, ZString expectedDateToday, bool isExpectedDevOrSupportUserEnabled, bool isExpectedCDSFunctionalityEnabled)
		{
			var declaration = testFactory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var menu = new EDIMenu();
			menu.Declaration = declaration;
			menu.OnPopup(EventArgs.Empty);
			testFactory.Save();

			var cdsMenu = menu.MenuItems.FindByText(CDSEDIMenu.CDSMenuCaption);
			var sendMenuItem = cdsMenu.MenuItems.FindByText(CDSEDIMenu.SendToCDS);
			AssertNotNull("Menu Item was found", cdsMenu);
			AssertEquals("Pre-req: date should be", expectedDateToday, ZDateTime.Now.ToString("yyyyMMdd"));
			AssertEquals("Pre-req: Current User's is dev or support status should be:", isExpectedDevOrSupportUserEnabled, GlbStaff.CurrentUser.GS_IsDeveloper);
			AssertEquals("Pre-req: CDS IsFunctionalityValid value should be", isExpectedCDSFunctionalityEnabled, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.DeclarationApplicationCodeExports, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, GlbCompany.CurrentCompany.LicenceKeyIdentifier));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			sendMenuItem.PerformClick();

			if (assertCDSIsEnabledAndNoMessage)
			{
				AssertNotContains("CDS is not yet enabled for your company.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertContains("CDS is not yet enabled for your company.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
