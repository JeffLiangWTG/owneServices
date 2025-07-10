using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using UniversalReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestRefreshMenu_NoException_AsDeclarationIsNotCreated()
		{
			using (var menu = new EDIMenu())
			{
				AssertNoExceptionThrown(() =>
				{
					menu.RefreshMenu();
				});
			}
		}

		public void TestSendToCustomsMenuVisibility_Export()
		{
			PrepareExportDeclaration();
			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();

				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
				CombineAssertions(() =>
				{
					Assert("Menu is invisible", !sendToCustomsMenu.Visible);

					expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					Assert("Menu is visible", sendToCustomsMenu.Visible);
				});
			}
		}

		public void TestSendToCustomsMenuEnabled_Import()
		{
			PrepareImportDeclaration();
			using (var form = new JobDeclarationForm(impDeclaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				impDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();

				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
				CombineAssertions(() =>
				{
					Assert("Menu is enabled", sendToCustomsMenu.Enabled);

					Env.Security.ImportMessaging.IsAllowed = false;
					menu.RefreshMenu();
					Assert("Menu is disabled", !sendToCustomsMenu.Enabled);
				});
			}
		}

		public void TestSendToCustomsMenuEnabled_Export()
		{
			PrepareExportDeclaration();
			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();

				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
				CombineAssertions(() =>
				{
					Assert("Menu is enabled", sendToCustomsMenu.Enabled);

					Env.Security.ExportMessaging.IsAllowed = false;
					menu.RefreshMenu();
					Assert("Menu is disabled", !sendToCustomsMenu.Enabled);
				});
			}
		}

		public void TestSendToCustomsMenuEnabled_IsWarehouseAdjustment()
		{
			var whsDeclaration = Factory.New<JobDeclaration>();
			whsDeclaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;

			using (var form = new JobDeclarationForm(whsDeclaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				whsDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();

				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
				CombineAssertions(() =>
				{
					Assert("Menu is enabled", sendToCustomsMenu.Enabled);

					Env.Security.ImportMessaging.IsAllowed = false;
					menu.RefreshMenu();
					Assert("Menu is disabled", !sendToCustomsMenu.Enabled);
				});
			}
		}

		public void TestSendToCustomsForExportMenuClickCausesMerge()
		{
			PrepareExportDeclaration();
			Factory.Save();
			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();
				var exportDeclarationMenu = FindSendToCustomsMenu(form);

				exportDeclarationMenu.PerformClick();
			}
			Assert(expDeclaration.IsMergeDone);
		}

		public void TestSendToCustomsForExportMenuClickCausesSave()
		{
			PrepareExportDeclaration();
			Factory.Save();

			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();

				expDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
				Assert(expDeclaration.HasChanges);

				var exportDeclarationMenu = FindSendToCustomsMenu(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				exportDeclarationMenu.PerformClick();
			}
			CombineAssertions(() =>
			{
				Assert("declaration no longer has changes", !expDeclaration.HasChanges);
				Assert("entries are saved", expDeclaration.CustomsEntryHeaders[0].IsInDatabase);
			});
		}

		public void TestSendToCustomsForExportMenuClickCausesMergeAndSaveWithoutDialog()
		{
			PrepareExportDeclaration();
			Factory.Save();

			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();

				var exportDeclarationMenu = FindSendToCustomsMenu(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				exportDeclarationMenu.PerformClick();
			}
			CombineAssertions(() =>
			{
				Assert("Should be no message box", UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert("Merge is done", expDeclaration.IsMergeDone);
				Assert("declaration has no changes (has been saved)", !expDeclaration.HasChanges);
				Assert("entries are saved", expDeclaration.CustomsEntryHeaders[0].IsInDatabase);
			});
		}

		public void TestSendToCustomsForExportMenuClickShouldDisplaySaveDialogIfWasNotSaved()
		{
			PrepareExportDeclaration();
			Factory.Save();
			expDeclaration.HasChanges = true;

			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();

				var exportDeclarationMenu = FindSendToCustomsMenu(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				exportDeclarationMenu.PerformClick();
			}
			CombineAssertions(() =>
			{
				Assert("Merge is not done", !expDeclaration.IsMergeDone);
				Assert("declaration still has changes", expDeclaration.HasChanges);
				AssertEquals("no entries exist", 0, expDeclaration.CustomsEntryHeaders.Count);
			});
		}

		public void TestSendToCustomsForAVABRMenuClickCausesMerge()
		{
			PrepareAVABRDeclaration();
			Factory.Save();
			using (var form = new JobDeclarationForm(avabrDeclaration))
			{
				form.Show();
				var finalizeAvabrMenu = FindSendToCustomsMenu(form);

				finalizeAvabrMenu.PerformClick();
			}
			Assert(avabrDeclaration.IsMergeDone);
		}

		public void TestSendToCustomsForAVABRMenuClickCausesSave()
		{
			PrepareAVABRDeclaration();
			Factory.Save();

			using (var form = new JobDeclarationForm(avabrDeclaration))
			{
				form.Show();

				avabrDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
				Assert(avabrDeclaration.HasChanges);

				var finalizeAvabrMenu = FindSendToCustomsMenu(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				finalizeAvabrMenu.PerformClick();
			}
			CombineAssertions(() =>
			{
				Assert("declaration no longer has changes", !avabrDeclaration.HasChanges);
				Assert("entries are saved", avabrDeclaration.CustomsEntryHeaders[0].IsInDatabase);
			});
		}

		public void TestSendToCustomsForAVABRMenuClickShouldDisplaySaveDialogIfWasNotSaved()
		{
			PrepareAVABRDeclaration();
			Factory.Save();
			avabrDeclaration.HasChanges = true;

			using (var form = new JobDeclarationForm(avabrDeclaration))
			{
				form.Show();

				var finalizeAvabrMenu = FindSendToCustomsMenu(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				finalizeAvabrMenu.PerformClick();
			}
			CombineAssertions(() =>
			{
				Assert("Merge is not done", !avabrDeclaration.IsMergeDone);
				Assert("declaration still has changes", avabrDeclaration.HasChanges);
				AssertEquals("no entries exist", 0, avabrDeclaration.CustomsEntryHeaders.Count);
			});
		}

		public void TestExportBottomSectionUserControlIsAddedForExportMessageMenu()
		{
			PrepareExportDeclaration();
			expDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();

			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();

				var exportDeclarationMenu = FindSendToCustomsMenu(form);
				exportDeclarationMenu.PerformClick();

				var messageSendingForm = ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(messageSendingForm.Controls.Find("ExportBottomSectionUserControl", true));
			}
		}

		public void TestCopyValuesBacktoDeclarationForExportMessageMenu()
		{
			PrepareExportDeclaration();
			expDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();

			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();

				var exportDeclarationMenu = FindSendToCustomsMenu(form);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm<ExportDeclarationMessageSendingActionParent>)obj;
					var action = (ExportEntryMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
					action.ShouldSend = true;
					action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
					action.ExitDate = new ZDateTime(2019, 10, 25);
				});
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				exportDeclarationMenu.PerformClick();
				AssertEquals("Cancelled. It should not have been copied", ZDateTime.Empty, expInstruction.ZG_ExitDate);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				exportDeclarationMenu.PerformClick();
				AssertEquals("users agree to proceed and It should have been copied", new ZDateTime(2019, 10, 25), expInstruction.ZG_ExitDate);
			}
		}

		public void TestSendSupplementaryExportDeclaration()
		{
			TestSendExportDeclaration(ExportEntryTypeList.Codes.SupplementaryExportDeclaration);
		}

		public void TestSendCancellationRequest()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				TestSendExportDeclaration(ExportEntryTypeList.Codes.CancellationRequest, additionalActionSetter: action => action.ExitDate = new ZDateTime(2019, 10, 25));
			}
		}

		public void TestSendExportAmendment()
		{
			TestSendExportDeclaration(ExportEntryTypeList.Codes.ExportAmendment);
		}

		public void TestSendExportExit()
		{
			TestSendExportDeclaration(ExportEntryTypeList.Codes.ExitToExport);
		}

		public void TestSendExportData()
		{
			TestSendExportDeclaration(ExportEntryTypeList.Codes.ExportDeclaration, setMrn: false);
		}

		public void TestBondedWarehouseMenuItemsNoBondedWarehousingEntry()
		{
			PrepareImportDeclaration();
			using (var menu = new EDIMenu())
			{
				menu.Declaration = impDeclaration;
				menu.RefreshMenu();
				var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
				CombineAssertions(() =>
				{
					AssertNull("invoiceLine.CusProcedure should be null", impInvoiceLine.CusProcedure);
					AssertEquals("entry.IsInwardBondedWarehousingEnabled", false, impEntry.IsInwardBondedWarehousingEnabled);
					AssertEquals("bondedWarehouseMenuItem.Visible", false, bondedWarehouseMenuItem.Visible);
				});
			}
		}

		public void TestBondedWarehouseMenuItemsInwardBondedWarehousingEnabled()
		{
			PrepareImportDeclaration();
			using (var menu = new EDIMenu())
			{
				menu.Declaration = impDeclaration;
				SetupEnableInwardBondedWarehousing();
				menu.RefreshMenu();

				var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
				CombineAssertions(() =>
				{
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);

					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(bondedWarehouseMenuItem, impEntry.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem, "Select Inventory", false);

					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertMenuItem(entryMenuItem1, "Disable Integration", true);
				});
			}
		}

		public void TestBondedWarehouseMenuItemsAutomationIsDisabled()
		{
			PrepareImportDeclaration();
			using (var menu = new EDIMenu())
			{
				menu.Declaration = impDeclaration;
				SetupEnableInwardBondedWarehousing();
				menu.RefreshMenu();

				impEntry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
				bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
				var entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
				entryMenuItem1.OnPopup(EventArgs.Empty);
				AssertMenuItem(entryMenuItem1, "Disable Integration", false);
			}
		}

		public void TestSendToCustomsForImportMenuClickCausesMergeAndSaveSaveWithoutDialog()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var sendToCustomsMenu = FindSendToCustomsMenu(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				sendToCustomsMenu.PerformClick();
			}
			CombineAssertions(() =>
			{
				Assert("Should be no message box", UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert("Merge is done", declaration.IsMergeDone);
				Assert("declaration has changes due to merge", declaration.HasChanges);
				Assert("entries are not saved", !declaration.CustomsEntryHeaders[0].IsInDatabase);
			});
		}

		public void TestSendToCustomsMenuVisibility_Import()
		{
			//test menu item is dependent on secutity right "Import Messaging"
			PrepareExportDeclaration();
			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				expDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				menu.RefreshMenu();

				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
				CombineAssertions(() =>
				{
					Assert("Menu is invisible", !sendToCustomsMenu.Visible);

					expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					Assert("Menu is visible", sendToCustomsMenu.Visible);
				});
			}
		}

		public void TestSendToCustomsMenuVisibility_WarehouseAdjustment()
		{
			PrepareExportDeclaration();
			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				expDeclaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
				menu.RefreshMenu();

				var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
				CombineAssertions(() =>
				{
					Assert("Menu is invisible", !sendToCustomsMenu.Visible);

					expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					menu.RefreshMenu();
					Assert("Menu is visible", sendToCustomsMenu.Visible);
				});
			}
		}

		public void TestSendImportDeclarationConfirmation()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.GCCONJ), isCUSCON: true);
		}

		public void TestSendSingleDeclarationFreeCirculationSender()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.FCFCDF), declarationType: ImportEntryTypeList.Codes.SingleDeclarationFreeCirculation);
		}

		public void TestSendSimplifiedDeclarationBondedWarehouseSender_VZL()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.LSCWRL), declarationType: ImportEntryTypeList.Codes.SimplifiedDeclarationBondedWarehouse);
		}

		public void TestSendSimplifiedDeclarationBondedWarehouseSender_AZL()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.LSCWRL), declarationType: ImportEntryTypeList.Codes.EntryInDeclarantsRecordsBondedWarehouse);
		}

		public void TestSendSingleDeclarationBondedWarehouseSender()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.LSCWDL), declarationType: ImportEntryTypeList.Codes.SingleDeclarationBondedWarehouse);
		}

		public void TestSendSingleDeclarationInwardProcessingSender()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.VSCIDC), declarationType: ImportEntryTypeList.Codes.SingleDeclarationInwardProcessing);
		}

		public void TestSendSimplifiedDeclarationInwardProcessingSender_VAV()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.VSCIRJ), declarationType: ImportEntryTypeList.Codes.SimplifiedDeclarationInwardProcessing);
		}

		public void TestSendSimplifiedDeclarationInwardProcessingSender_AAV()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.VSCIRJ), declarationType: ImportEntryTypeList.Codes.EntryInDeclarantsRecordsInwardProcessing);
		}

		public void TestSendSimplifiedDeclarationFreeCirculationSender_VZA()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.FCFCRF), declarationType: ImportEntryTypeList.Codes.SimplifiedDeclarationFreeCirculation);
		}

		public void TestSendSimplifiedDeclarationFreeCirculationSender_AZ()
		{
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.FCFCRF), declarationType: ImportEntryTypeList.Codes.EntryInDeclarantsRecordsFreeCirculation);
		}

		public void TestSendStockTransferBondedWarehouseSender()
		{
			PrepareImportDeclaration();
			impInstruction.CEI_Style = ImportEntryTypeList.Codes.StockTransferBondedWarehouse;
			impDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
			var entry = impDeclaration.CustomsEntryHeaders[0];
			entry.AllEntryLines.ForEach(e =>
			{
				var previousDocument = entry.EntryInstruction.PreviousDocuments.AddNew();
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._T1;
				previousDocument.CSI_ReferenceNumber = "REF1";
				previousDocument.CSI_ItemNumber = e.RandomLine.JI_LineNo;
			});
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.LCUSWK), declarationType: ImportEntryTypeList.Codes.StockTransferBondedWarehouse);
		}

		public void TestSendCollectiveClearanceBondedWarehouseSender()
		{
			PrepareImportDeclaration();
			impDeclaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			TestSendImportDeclaration(ATLASVersionNumberList.Codes._101, nameof(ATLASVersion10_1.LECWCG), declarationType: ImportEntryTypeList.Codes.CollectiveClearanceCustomsWarehouse);
		}

		public void TestSendToCustoms_ShouldCheckCredit_Import()
		{
			PrepareImportDeclaration();
			impInstruction.CEI_Style = ImportEntryTypeList.Codes.SingleDeclarationFreeCirculation;
			impDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();
			AssertCreditCheck(impDeclaration);
		}

		public void TestSendToCustoms_ShouldCheckCredit_Export()
		{
			PrepareExportDeclaration();
			expDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();
			AssertCreditCheck(expDeclaration);
		}

		void AssertCreditCheck(JobDeclaration declaration)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingFormWithValidationDetails)obj;
				var action = (MessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				action.ShouldSend = true;
			});
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			const string notSentMessage = "Submit message with credit restriction canceled.";
			const string sentMessage = "The message has been sent.";

			var testCases = new[]
			{
				(jobDeclaration: declaration, isCheckPass: false, expectedMessageCount: 0, expectedMessageText: notSentMessage),
				(jobDeclaration: declaration, isCheckPass: true, expectedMessageCount: 1, expectedMessageText: sentMessage),
			};

			foreach (var (jobDeclaration, isCheckPass, expectedMessageCount, expectedMessageText) in testCases)
			{
				var entryHeader = jobDeclaration.CustomsEntryHeaders[0];

				using (var form = new JobDeclarationForm(jobDeclaration))
				{
					var sendToCustomsMenu = FindSendToCustomsMenu(form);

					using (CheckCreditTestHelper.WithCreditCheck(Factory, jobDeclaration, isCheckPass))
					{
						AssertEquals($"MessageType: {jobDeclaration.JE_MessageType}, Credit check passed: {isCheckPass}, Precondition", 0, entryHeader.Messages.Count);

						sendToCustomsMenu.PerformClick();

						CombineAssertions($"Credit check passed: {isCheckPass}", () =>
						{
							AssertEquals("Last message after sending", expectedMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals("Messages count after sending", expectedMessageCount, entryHeader.Messages.Count);
						});
					}
				}
			}
		}

		void TestSendExportDeclaration(ZString entryType, bool setMrn = true, Action<ExportEntryMessageSendingAction> additionalActionSetter = null)
		{
			PrepareExportDeclaration();
			const string Mrn = "19DE586600822952E1";
			expDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();
			AssertEquals("EntryHeader count", 1, expDeclaration.CustomsEntryHeaders.Count);
			var entryHeader = expDeclaration.CustomsEntryHeaders[0];
			AssertEquals("Messages count before sending", 0, entryHeader.Messages.Count);
			using (var form = new JobDeclarationForm(expDeclaration))
			{
				form.Show();

				var sendToCustomsMenu = FindSendToCustomsMenu(form);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm<ExportDeclarationMessageSendingActionParent>)obj;
					var action = (ExportEntryMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
					action.ShouldSend = true;
					action.EntryType = entryType;
					if (setMrn)
					{
						action.MovementReferenceNumber = Mrn;
					}
					additionalActionSetter?.Invoke(action);
				});

				CombineAssertions(() =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					sendToCustomsMenu.PerformClick();
					AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Messages count after sending", 1, entryHeader.Messages.Count);
					if (setMrn)
					{
						AssertNotNull($"MRN CusEntryNum persistent stored", CusEntryNumber.Load<CusEntryNumber>(new ReadOnlyBusinessObjectFactory(), CusEntryNumberTypes.Standard.MovementReferenceNumber, Mrn, Core.Constants.CountryCodes.Germany).SingleOrDefault());
					}
				});
			}
		}

		void TestSendImportDeclaration(string currentAtlasVersion, string expectedApplicationReference, string declarationType = "", bool isCUSCON = false)
		{
			PrepareImportDeclaration();
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = currentAtlasVersion } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				impInstruction.CEI_Style = !string.IsNullOrEmpty(declarationType) ? (ZString)declarationType : ZString.Empty;
				impDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("EntryHeaders count", 1, impDeclaration.CustomsEntryHeaders.Count);
					var entryHeader = impDeclaration.CustomsEntryHeaders[0];
					AssertEquals("EntryHeader's messages count", 0, entryHeader.Messages.Count);

					using (var form = new JobDeclarationForm(impDeclaration))
					{
						form.Show();
						var sendToCustomsMenu = FindSendToCustomsMenu(form);

						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
						{
							var dialog = (MessageSendingForm<ImportDeclarationMessageSendingActionParent>)obj;
							var action = (ImportEntryMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
							action.ShouldSend = true;
							action.CusCon = isCUSCON;
						});
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						sendToCustomsMenu.PerformClick();
						AssertEquals("Message has been sent notification appears", "The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("EntryHeader's message count after send button click", 1, entryHeader.Messages.Count);
						AssertEquals("Message EM_ApplicationReference", expectedApplicationReference, ((EDIMessage)entryHeader.Messages.FirstOrDefault())?.EM_ApplicationReference);
					}
				});
			}
		}

		public void TestFinalizeAVABRDeclaration()
		{
			PrepareAVABRDeclaration();
			avabrDeclaration.DoMerge(new SendsMessagesToCustomsGUI());
			Factory.Save();

			AssertEquals("EntryHeaders count", 1, avabrDeclaration.CustomsEntryHeaders.Count);
			var entryHeader = avabrDeclaration.CustomsEntryHeaders[0];
			AssertEquals("Precondition", ZString.Empty, entryHeader.CH_EntryStatus);

			using (var form = new JobDeclarationForm(avabrDeclaration))
			{
				form.Show();
				var sendToCustomsMenu = FindSendToCustomsMenu(form);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm<FinalizeAVABRMessageSendingActionParent>)obj;
					var action = (FinalizeAVABREntryMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
					action.ShouldSend = true;
				});
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendToCustomsMenu.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Message has been sent notification appears", "Inward Processing declaration has been finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Entry Header status set", UniversalReferenceConstants.EntryStatus.TX8, entryHeader.CH_EntryStatus);
				});
			}
		}

		static MenuItem FindSendToCustomsMenu(JobDeclarationForm form)
		{
			var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
			var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
			return sendToCustomsMenu;
		}

		void AssertMenuItem(MenuItem parentMenuItem, ZString text, bool visible)
		{
			var menuItem = parentMenuItem.MenuItems.FindByText(text);
			AssertNotNull(text + " Not Null", menuItem);
			AssertEquals(text + " Visible", visible, menuItem?.Visible ?? false);
		}

		void SetupEnableInwardBondedWarehousing()
		{
			impInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			impInstruction.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
			impInvoiceLine.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;

			AssertNotNull("invoiceLine.CusProcedure should not null", impInvoiceLine.CusProcedure);
			AssertEquals("entry.IsInwardBondedWarehousingEnabled", true, impEntry.IsInwardBondedWarehousingEnabled);
		}

		JobDeclaration expDeclaration;
		JobDeclaration impDeclaration;
		JobDeclaration avabrDeclaration;
		Business.Declaration.CusEntryInstruction expInstruction;
		Business.Declaration.CusEntryInstruction impInstruction;
		Business.Declaration.CusEntryInstruction avabrInstruction;
		WhsDataTestHelper helper;
		Business.Declaration.CusEntryHeader impEntry;
		JobComInvoiceLine impInvoiceLine;

		void PrepareImportDeclaration()
		{
			if (impDeclaration == null)
			{
				impDeclaration = Factory.New<JobDeclaration>();
				impDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				impDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				impInstruction = impDeclaration.CustomsEntryInstructions.AddNew();
				helper = new WhsDataTestHelper(Factory);
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;

				impDeclaration.JE_OH_Importer = helper.Importer.PK;

				impEntry = impDeclaration.CustomsEntryHeaders.AddNew();
				impEntry.CH_CEI_Instruction = impInstruction.PK;
				var impEntryLine = impEntry.MergedLines.AddNew();

				var impInvoice = impDeclaration.Invoices.AddNew();
				impInvoiceLine = impInvoice.InvoiceLines.AddNew();
				impInvoiceLine.JI_CEI = impInstruction.PK;
				impInvoiceLine.JI_CL = impEntryLine.PK;
			}
		}

		void PrepareExportDeclaration()
		{
			expDeclaration = Factory.New<JobDeclaration>();
			expDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			expDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			expInstruction = expDeclaration.CustomsEntryInstructions.AddNew();
			var invoice = expDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = expInstruction.PK;
		}

		void PrepareAVABRDeclaration()
		{
			avabrDeclaration = Factory.New<JobDeclaration>();
			avabrDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			avabrDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			avabrInstruction = avabrDeclaration.CustomsEntryInstructions.AddNew();
			avabrInstruction.CEI_Style = "AVABR";
			var invoiceLine = avabrDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = avabrInstruction.PK;
		}
	}
}
