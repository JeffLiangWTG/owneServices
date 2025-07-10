using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.OperationalActions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class MessageUserControlTest : TestCaseWithFactory
	{
		public void TestCreditCodeMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "Number";
				entryHeader.MovementReferenceNumberSetter("MRN0001", ZDateTime.Today);
				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.EntryNumber = "Number2";
				entryHeader2.MovementReferenceNumberSetter("MRN0002", ZDateTime.Today);

				var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");

				var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Credit COD");
				entriesGrid.ContextMenu.DoPopup();
				AssertNotNull(menuItem);
				Assert("Credit COD menu item should not show when no entry is selected.", !menuItem.Visible);

				entriesGrid.Select(0);
				menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Credit COD");
				entriesGrid.ContextMenu.DoPopup();
				AssertNotNull(menuItem);
				Assert("Credit COD menu item should show as soon as one entry is selected.", menuItem.Visible);

				entriesGrid.SelectAllElements();
				menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Credit COD");
				entriesGrid.ContextMenu.DoPopup();
				AssertNotNull(menuItem);
				Assert("Credit COD menu item should show when more than one entries are selected.", menuItem.Visible);

				menuItem.PerformClick();
				AssertEquals(typeof(OperationalActionRunnerForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestEntryFeesTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				var entryFeesTabPage = messageUserControl.FindSingle<ZTabPage>("EntryFeesTabPage");
				entryFeesTabPage.Show();

				CombineAssertions(() =>
				{
					AssertEquals("EntryFeesTabPage visible", true, entryFeesTabPage.TabVisible);
					AssertEquals("Caption", "Entry Fees", entryFeesTabPage.CaptionResourceString.Caption);

					AssertEntryFeeGrid(messageUserControl, "EntryFeesGrid", "Calculated");
					AssertEntryFeeGrid(messageUserControl, "ConfirmedFeesGrid", "Confirmed");
				});
			}
		}

		public void TestFeesGridAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					var entriesGrid = (ZGrid)messageUserControl.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
					AssertNotNull("User control should have a Total Paid column.", entriesGrid.GetColumnStyle(CusEntryHeader.Schema.CH_TotalPaid));
					AssertNotNull("User control should have a ECS Status column.", entriesGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus));
				});
			}
		}

		public void TestSetupEntryLineColumns()
		{
			var testDec = Factory.New<JobDeclaration>();
			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
					var columns = entryLineGrid.ColumnStyles.Cast<ZGridColumnInfo>();

					var customsStaticalSTAColumn = columns.Single(x => x.ColumnName == nameof(CusEntryLine.CL_Calc_StatisticalBasisExcludingSTACharge));
					AssertEquals("ZG_CustomsStatus Column Caption", "Statistical Basis", customsStaticalSTAColumn.CaptionResourceString.Caption);

					var invoiceAmountColumn = columns.Single(x => x.ColumnName == nameof(CusEntryLine.CL_InvoiceAmount));
					AssertEquals("invoiceAmountColumn Caption", "Invoiced Price", invoiceAmountColumn.CaptionResourceString.Caption);

					var invoiceAmountCurrencyColumn = columns.Single(x => x.ColumnName == nameof(CusEntryLine.CL_RX_NKInvoiceAmountCurrency));
					AssertEquals("invoiceAmountCurrencyColumn Caption", "Inv. Price UQ", invoiceAmountCurrencyColumn.CaptionResourceString.Caption);

					var confirmedStatisticalValueColumn = columns.Single(x => x.ColumnName == nameof(CusEntryLine.CL_ConfirmedStatisticalValue));
					AssertEquals("confirmedStatisticalValueColumn Caption", "Confirmed Statistical Value", confirmedStatisticalValueColumn.CaptionResourceString.Caption);

					var confirmedCustomsValue = columns.Single(x => x.ColumnName == nameof(CusEntryLine.CL_ConfirmedCustomsValue));
					AssertEquals("confirmedCustomsValue Caption", "Confirmed Customs Value", confirmedCustomsValue.CaptionResourceString.Caption);

					var confirmedValueForVATColumn = columns.Single(x => x.ColumnName == nameof(CusEntryLine.CL_ConfirmedValueForVAT));
					AssertEquals("confirmedValueForVATColumn Caption", "Confirmed VAT-able Value", confirmedValueForVATColumn.CaptionResourceString.Caption);
				});
			}
		}

		public void TestAddAlternateProofOfExitMenuItem_Visibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "Number";
				entryHeader.MovementReferenceNumberSetter("MRN0001", ZDateTime.Today);
				var docManagerInfo = (entryHeader as IDocManagerSupport).DocManagerInfo;
				var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Add Alternate Proof of Exit");

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				entriesGrid.Select(0);
				entriesGrid.ContextMenu.DoPopup();
				AssertEquals("Add Alternative Proof of Exit menu item should not be visible for Import declaration", false, menuItem.Visible);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				entriesGrid.Select(0);
				entriesGrid.ContextMenu.DoPopup();
				AssertEquals("Add Alternative Proof of Exit menu item should be visible for Export declaration", true, menuItem.Visible);

				entryHeader.MovementReferenceNumberSetter("", ZDateTime.Today);
				entriesGrid.Select(0);
				entriesGrid.ContextMenu.DoPopup();
				AssertEquals("Add Alternative Proof of Exit menu item should not be visible if no MRN。", false, menuItem.Visible);
			}
		}

		public void TestRemoveAlternateProofOfExitMenuItem_Visibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "Number";
				entryHeader.MovementReferenceNumberSetter("MRN0001", ZDateTime.Today);
				var docManagerInfo = (entryHeader as IDocManagerSupport).DocManagerInfo;
				var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Remove Alternate Proof of Exit");

				using (FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					entriesGrid.Select(0);
					entriesGrid.ContextMenu.DoPopup();
					AssertEquals("Remove Alternative Proof of Exit menu item should not be visible for Import declaration", false, menuItem.Visible);
				}

				using (FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TYP"))
				{
					entriesGrid.Select(0);
					entriesGrid.ContextMenu.DoPopup();
					AssertEquals("Remove Alternative Proof of Exit menu item should be not visible for entries without Alternate Proof of Exit documents", false, menuItem.Visible);

					entriesGrid.Select(0);
					docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("contents"), "filename", "TYP");
					entriesGrid.ContextMenu.DoPopup();
					AssertEquals("Remove Alternative Proof of Exit menu item should be visible for entries without Alternate Proof of Exit documents", true, menuItem.Visible);

					entriesGrid.Select(0);
					entryHeader.MovementReferenceNumberSetter("", ZDateTime.Today);
					entriesGrid.ContextMenu.DoPopup();
					AssertEquals("Add Alternative Proof of Exit menu item should not be visible if no MRN。", false, menuItem.Visible);
				}
			}
		}

		public void TestAddAlternateProofOfExitMenuItem_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControlWithOverridenGetFileContentsAndName())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "Number";
				entryHeader.CH_ExitedStatus = ZString.Empty;
				var docManagerInfo = (entryHeader as IDocManagerSupport).DocManagerInfo;
				var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				entriesGrid.Select(0);
				var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Add Alternate Proof of Exit");
				var docTypeForAPE = "TYP";
				var successMessage = "ECS status has been set to APE, and a document has has been added.";
				var eDocsRefreshHint = "You may need to click the \"Refresh\" button in eDocs tab to see the latest documents.";
				var isFileAdded = docManagerInfo.AllEDocs.Cast<IeDoc>().Any(d => d.FileName == "filename");

				using (FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					entriesGrid.ContextMenu.DoPopup();
					menuItem.PerformClick();

					AssertEquals("No document type is specified as Alternate Proof of Exit, please configure it in registry Customs > France > Exit Control System > Document Type as Alternate Proof of Exit.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotEquals(ExportControlStatusList.Codes.APE, entryHeader.CH_ExitedStatus);
				}

				using (FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, docTypeForAPE))
				{
					entriesGrid.ContextMenu.DoPopup();
					menuItem.PerformClick();
					isFileAdded = docManagerInfo.AllEDocs.Cast<IeDoc>().Any(d => d.FileName == "filename");
					AssertEquals("A new file should be added", true, isFileAdded);
					AssertEquals(successMessage + "\r\n" + eDocsRefreshHint, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ExportControlStatusList.Codes.APE, entryHeader.CH_ExitedStatus);

					menuItem.PerformClick();
					var isDuplicatedFileAdded = docManagerInfo.AllEDocs.Cast<IeDoc>().Count(d => d.FileName == "filename") > 1;
					AssertEquals("A duplicated file should not be added", false, isDuplicatedFileAdded);
					AssertEquals("A document with the selected filename already exists. No action will be performed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ExportControlStatusList.Codes.APE, entryHeader.CH_ExitedStatus);
				}
			}
		}

		public void TestRemoveAlternateProofOfExitMenuItem_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControlWithOverridenGetFileContentsAndName())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "Number";
				Factory.Save();
				var previousECSStatusToAPE = entryHeader.CH_ExitedStatus = "INT";
				Factory.Save();
				var currentECSStatus = entryHeader.CH_ExitedStatus = ExportControlStatusList.Codes.APE;
				Factory.Save();
				var docManagerInfo = (entryHeader as IDocManagerSupport).DocManagerInfo;
				var entriesGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				entriesGrid.Select(0);
				var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Remove Alternate Proof of Exit");
				var docTypeForAPE = "TYP";

				using (FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					entriesGrid.ContextMenu.DoPopup();
					menuItem.PerformClick();

					AssertEquals("No document type is specified as Alternate Proof of Exit, please configure it in registry Customs > France > Exit Control System > Document Type as Alternate Proof of Exit.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ExportControlStatusList.Codes.APE, entryHeader.CH_ExitedStatus);
				}

				using (FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, docTypeForAPE))
				{
					docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("contents"), "filename1", docTypeForAPE);
					docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("contents"), "filename2", docTypeForAPE);
					docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("contents"), "filename3", "FIL");

					entriesGrid.ContextMenu.DoPopup();
					menuItem.PerformClick();

					var revertedMessage = "ECS status has been reverted to INT.";
					var deletedMessage = "Document(s) have been deleted permanently:\r\n\r\n\tfilename1\r\n\tfilename2";
					var eDocsRefreshHint = "You may need to click the \"Refresh\" button in eDocs tab to see the latest documents.";
					AssertEquals(revertedMessage + "\r\n" + deletedMessage + "\r\n\r\n" + eDocsRefreshHint, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Only one document should survive the deletion", 1, docManagerInfo.AllEDocs.Count);
					AssertEquals("The existing documents should not contain any of APE type", false, docManagerInfo.AllEDocs.Cast<IeDoc>().Any(doc => doc.DocType == docTypeForAPE));
					AssertEquals(entryHeader.CH_ExitedStatus, previousECSStatusToAPE);
				}
			}
		}

		public void TestCH_CalcTotalInvoicedAmountInLocalCurrencyColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					var entriesGrid = (ZGrid)messageUserControl.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
					AssertNotNull("User control should have a CH_CalcTotalInvoicedAmountInLocalCurrency column.", entriesGrid.GetColumnStyle(CusEntryHeader.Schema.CH_CalcTotalInvoicedAmountInLocalCurrency));
					AssertEquals(true, entriesGrid.GetColumnStyle(CusEntryHeader.Schema.CH_CalcTotalInvoicedAmountInLocalCurrency).IsReadOnly);
				});
			}
		}

		public void TestTriggeringPointForValidationColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					var entriesGrid = (ZGrid)messageUserControl.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
					AssertNotNull("User control should have a CH_TriggeringPointForValidation column.", entriesGrid.GetColumnStyle(CusEntryHeader.Schema.CH_TriggeringPointForValidation));
					AssertType<ZDropEditColumnStyleInfo>("CH_TriggeringPointForValidation column should have a list.", entriesGrid.GetColumnStyle(CusEntryHeader.Schema.CH_TriggeringPointForValidation));
					AssertEquals(false, entriesGrid.GetColumnStyle(CusEntryHeader.Schema.CH_TriggeringPointForValidation).IsReadOnly);
				});
			}
		}

		public void TestCH_ConfirmedGuaranteeAmountColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					var entriesGrid = (ZGrid)messageUserControl.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
					AssertNotNull("User control should have a CH_ConfirmedGuaranteeAmount column.", entriesGrid.GetColumnStyle("CH_ConfirmedGuaranteeAmount"));
					AssertType<ZCalcEditColumnStyleInfo>("CH_ConfirmedGuaranteeAmount column Type.", entriesGrid.GetColumnStyle("CH_ConfirmedGuaranteeAmount"));
					AssertEquals(true, entriesGrid.GetColumnStyle("CH_ConfirmedGuaranteeAmount").IsReadOnly);
				});
			}
		}

		public void TestCH_Calc_GuaranteeAmountColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					var entriesGrid = (ZGrid)messageUserControl.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
					AssertNotNull("User control should have a CH_Calc_GuaranteeAmount column.", entriesGrid.GetColumnStyle("CH_Calc_GuaranteeAmount"));
					AssertType<ZCalcEditColumnStyleInfo>("CH_Calc_GuaranteeAmount column Type.", entriesGrid.GetColumnStyle("CH_Calc_GuaranteeAmount"));
					AssertEquals(true, entriesGrid.GetColumnStyle("CH_Calc_GuaranteeAmount").IsReadOnly);
				});
			}
		}

		public void TestSetEntryAsAmendmentMenuOption_AmendEntryOperation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;
				form.Controls.Add(messageUserControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var entriesBoundGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				AssertNotNull(entriesBoundGrid);

				var setEntryAsAmendmentMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Set Entry As Amendment");
				entriesBoundGrid.Select(0);
				entriesBoundGrid.ContextMenu.DoPopup();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				setEntryAsAmendmentMenuItem.PerformClick();

				AssertEquals("After Entry has been set to Amending, feedback message is shown to the user", "One Entry was set to Amending", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("A log entry related to Entry status AMG must be found", entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry status set to AMG, original status: REG")));
				Assert("A log entry related to Entry message status reset must be found", entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry status set to AMG, original status")));
				AssertEquals("Entry status is set to AMG", DeltaIEImportCusEntryStatusList.Codes.Amending, entryHeader.CH_EntryStatus);
				AssertEquals("Entry message status is reset", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("Single entry present, declaration Entry status is same as Entry header status", DeltaIEImportCusEntryStatusList.Codes.Amending, declaration.JE_EntryStatus);
			}
		}

		public void TestSetEntryAsAmendmentMenuOption_CancelOperation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;
				form.Controls.Add(messageUserControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var entriesBoundGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				AssertNotNull(entriesBoundGrid);

				var setEntryAsAmendmentMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Set Entry As Amendment");
				entriesBoundGrid.Select(0);
				entriesBoundGrid.ContextMenu.DoPopup();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				setEntryAsAmendmentMenuItem.PerformClick();

				AssertEquals("If user chooses Cancel, no feedback message is shown", "Are you sure you want to set this Entry as AMENDING?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("A log entry related to Entry status AMG must not found", !entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry status set to AMG, original status: REG")));
				Assert("A log entry related to Entry message status must not be found", !entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry message status reset")));
				AssertEquals("Entry status is remains the same", DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered, entryHeader.CH_EntryStatus);
				AssertEquals("Entry message status remains the same", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("Single entry present, declaration Entry status is same as Entry header status", DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered, declaration.JE_EntryStatus);
			}
		}

		public void TestSetEntryAsAmendmentMenuOption_Visibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				form.Controls.Add(messageUserControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var entriesBoundGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				AssertNotNull(entriesBoundGrid);

				var setEntryAsAmendmentMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Set Entry As Amendment");
				entriesBoundGrid.Select(0);
				entriesBoundGrid.ContextMenu.DoPopup();

				CombineAssertions(() =>
				{
					AssertNotNull("Set Entry as Amendment menu item must be Available", setEntryAsAmendmentMenuItem);
					AssertEquals("DeltaG Export Declaration, Set Entry As Amenedment is NOT Visible", false, setEntryAsAmendmentMenuItem.Visible);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					entriesBoundGrid.Select(0);
					entriesBoundGrid.ContextMenu.DoPopup();
					AssertEquals("DeltaG Import Declaration, Set Entry As Amenedment is NOT Visible", false, setEntryAsAmendmentMenuItem.Visible);

					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
					entriesBoundGrid.Select(0);
					entriesBoundGrid.ContextMenu.DoPopup();
					AssertEquals("DeltaIE Import Declaration, Set Entry As Amenedment is Visible", true, setEntryAsAmendmentMenuItem.Visible);
				});
			}
		}

		public void TestSetEntryAsAmendmentMenuOption_WithDeltaIEImportCusEntryStatusList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				form.Controls.Add(messageUserControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var entriesBoundGrid = messageUserControl.FindSingle<ZGrid>("EntriesBoundGrid");
				AssertNotNull(entriesBoundGrid);

				var setEntryAsAmendmentMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Set Entry As Amendment");
				entriesBoundGrid.Select(0);
				entriesBoundGrid.ContextMenu.DoPopup();

				AssertNotNull("Set Entry as Amendment menu item must be available", setEntryAsAmendmentMenuItem);
				AssertEquals("Set Entry as Amendment menu item must be visible", true, setEntryAsAmendmentMenuItem.Visible);

				CombineAssertions(() =>
				{
					var amendmentSuccessMessage = "One Entry was set to Amending";
					var amendmentNotAllowedErrorMessage = "Entry status doesn't allow Amendment.";

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationAcceptedMrnAllocated;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is DeclarationAcceptedMrnAllocated, Amendment is allowed", amendmentSuccessMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is DeclarationRegistered, Amendment is allowed", amendmentSuccessMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Released;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is Released, Amendment is allowed", amendmentSuccessMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Amended;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is Amended, Amendment is NOT allowed", amendmentNotAllowedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Amending;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is Amending, Amendment is NOT allowed", amendmentNotAllowedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Invalidated;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is Invalidated, Amendment is NOT allowed", amendmentNotAllowedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.ReleaseRejected;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is ReleaseRejected, Amendment is NOT allowed", amendmentNotAllowedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationRejected;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is DeclarationRejected, Amendment is NOT allowed", amendmentNotAllowedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.TimerExpired;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is TimerExpired, Amendment is NOT allowed", amendmentNotAllowedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					entryHeader.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.UnderControl;
					setEntryAsAmendmentMenuItem.PerformClick();
					AssertEquals("EntryStatus is UnderControl, Amendment is NOT allowed", amendmentNotAllowedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGetEntryLineAdditionalData_IsUCC6EntryLineAdditionalDataUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl(declaration))
			{
				var entryLineAdditionalDataUserControl = messageUserControl.Controls.Find("EntryLineAdditionalDataUserControl", true).SingleOrDefault();
				AssertType(typeof(UCC6EntryLineAdditionalDataUserControl), entryLineAdditionalDataUserControl);
			}
		}

		class MessageUserControlWithOverridenGetFileContentsAndName : MessageUserControl
		{
			protected override (byte[], string) GetFileContentsAndName() => (Encoding.UTF8.GetBytes("contents"), "filename");
		}

		void AssertEntryFeeGrid(MessageUserControl messageUserControl, ZString gridName, ZString expectedCaption)
		{
			var entryFeesGrid = (ZGrid)messageUserControl.Controls.Find(gridName, true).SingleOrDefault();
			AssertEquals(gridName + ": grid caption.", true, entryFeesGrid.CaptionText.Contains(expectedCaption));
			AssertEquals(gridName + ": grid caption is visible.", true, entryFeesGrid.CaptionVisible);
			AssertEquals(gridName + ": ConfirmedFeesGrid should be readonly.", gridName == "ConfirmedFeesGrid", entryFeesGrid.ReadOnly);
			AssertNotNull(gridName + ": ChargeType column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_ChargeType));
			AssertNotNull(gridName + ": Amount column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_ChargeAmount));
			AssertNotNull(gridName + ": MethodOfPayment column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_MethodOfPayment));
			if (gridName == "EntryFeesGrid")
			{
				AssertNotNull("C1_RateOverrideReasonCode column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_RateOverrideReasonCode));
			}
			if (gridName == "ConfirmedFeesGrid")
			{
				AssertNotNull("NationalFeeTypeCode column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.NationalFeeTypeCode));
				AssertNotNull("TaxStatus column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.TaxStatus));
			}
		}
	}
}
