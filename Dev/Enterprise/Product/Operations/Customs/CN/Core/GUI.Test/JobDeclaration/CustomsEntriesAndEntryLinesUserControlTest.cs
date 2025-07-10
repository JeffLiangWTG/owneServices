using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CustomsEntriesAndEntryLinesUserControlTest : TestCaseWithFactory
	{
		public void TestDutiesAndTaxesControls()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(items.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			Factory.Save();
			using var form = new JobDeclarationForm(items.JobDeclaration);
			form.Show();
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			var entriesGroupBox = enteriesControl.FindSingle<ZGroupBox>("EntriesGroupBox");
			var dutiesAndTaxesGroupBox = entriesGroupBox.FindSingle<ZGroupBox>("DutiesAndTaxesGroupBox");
			AssertNotNull("DutiesAndTaxesGroupBox should be on the EntryLinesTabPage", dutiesAndTaxesGroupBox);
			var totalDutyAmountTextBox = dutiesAndTaxesGroupBox.FindSingle<ZCalcEdit>("TotalDutyAmountCalcEdit");
			var totalGSTVATAmountTextBox = dutiesAndTaxesGroupBox.FindSingle<ZCalcEdit>("TotalGSTVATAmountCalcEdit");
			var totalExciseAmountTextBox = dutiesAndTaxesGroupBox.FindSingle<ZCalcEdit>("TotalExciseAmountCalcEdit");
			var totalAntiDumpingAmountCalcEdit = dutiesAndTaxesGroupBox.FindSingle<ZCalcEdit>("TotalAntiDumpingAmountCalcEdit");
			var totalCountervailingAmountCalcEdit = dutiesAndTaxesGroupBox.FindSingle<ZCalcEdit>("TotalCountervailingAmountCalcEdit");
			AssertNotNull("TotalDutyAmountTextBox should be in the DutiesAndTaxesGroupBox", totalDutyAmountTextBox);
			AssertNotNull("TotalGSTVATAmountTextBox should be in the DutiesAndTaxesGroupBox", totalGSTVATAmountTextBox);
			AssertNotNull("TotalExciseAmountTextBox should be in the DutiesAndTaxesGroupBox", totalExciseAmountTextBox);
			Assert("TotalDutyAmountTextBox should be visible when IsEntering", totalDutyAmountTextBox.Visible);
			Assert("TotalGSTVATAmountTextBox should be visible when IsEntering", totalGSTVATAmountTextBox.Visible);
			Assert("TotalExciseAmountTextBox should be visible when IsEntering", totalExciseAmountTextBox.Visible);
			Assert("TotalAntiDumpingAmountCalcEdit should be visible when IsEntering", totalAntiDumpingAmountCalcEdit.Visible);
			Assert("TotalCountervailingAmountCalcEdit should be visible when IsEntering", totalCountervailingAmountCalcEdit.Visible);
			items.EntryHeader.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			Assert("TotalDutyAmountTextBox should be visible when IsExiting", totalDutyAmountTextBox.Visible);
			Assert("TotalGSTVATAmountTextBox should be invisible when IsExiting", !totalGSTVATAmountTextBox.Visible);
			Assert("TotalExciseAmountTextBox should be invisible when IsExiting", !totalExciseAmountTextBox.Visible);
			Assert("TotalAntiDumpingAmountCalcEdit should be visible when IsEntering", !totalAntiDumpingAmountCalcEdit.Visible);
			Assert("TotalCountervailingAmountCalcEdit should be visible when IsEntering", !totalCountervailingAmountCalcEdit.Visible);
		}

		public void TestArchiveRecords()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(items.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (var form = new JobDeclarationForm(items.JobDeclaration))
			{
				form.Show();
				var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
				var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Archive Entry");
				enteriesControl.EntriesBoundGrid.Select(0);
				menuItem.PerformClick();
				AssertEquals("Success", "All selected entries have been archived.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(items.JobDeclaration.HasChanges);
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var form = new JobDeclarationForm(items.JobDeclaration))
			{
				form.Show();
				items.JobDeclaration.JE_CustomsOffice = "OF1";
				var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
				var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Archive Entry");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(items.JobDeclaration.HasChanges);
				UnitTestUserNotification.Instance.ClearMessages();
				Factory.Save();
				menuItem.PerformClick();
				AssertEquals("Error", "Please select one or more Entries first.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!items.JobDeclaration.HasChanges);
				enteriesControl.EntriesBoundGrid.Select(0);
				menuItem.PerformClick();
				AssertContains("Error for user who has no right", "You do not have the security rights to archive entries.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!items.JobDeclaration.HasChanges);
			}
		}

		public void TestGenerateAttachments()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(items.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			Factory.Save();
			using var form = new JobDeclarationForm(items.JobDeclaration);
			form.Show();
			var declaration = items.JobDeclaration;
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TTT";
			invoice.JZ_InvoiceDate = ZDateTime.Today;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader1.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			entryHeader1.CH_CEI_Instruction = instruction.PK;
			var attachment = new CusStorageDocPivotCollection(instruction).AddNew();
			attachment.CSD_DocType = "00000001";
			attachment.CSD_StorageDocReference = ZGuid.Empty;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Attachments");
			menuItem.PerformClick();
			AssertEquals("Hint Message", "Please select one and only one Entry first.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(!items.JobDeclaration.HasChanges);
			enteriesControl.EntriesBoundGrid.SelectAllElements();
			menuItem.PerformClick();
			AssertEquals("Hint Message", "Please select one and only one Entry first.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(!items.JobDeclaration.HasChanges);
			enteriesControl.EntriesBoundGrid.UnSelectAll();
			enteriesControl.EntriesBoundGrid.Select(0);
			menuItem.PerformClick();
			AssertEquals("Hint Message", @"Document CN Customs Invoice.pdf has been generated in eDocs.
Document CN Purchase Order.pdf has been generated in eDocs.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(items.JobDeclaration.HasChanges);
		}

		public void TestSendAcdAgrMessageMenuItem_Existence()
		{
			using var form = new JobDeclarationForm(Factory.New<JobDeclaration>());
			form.Show();
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Send Agreement of Customs Declaration Agent Message");
			AssertNotNull("Add send ACDA message menu item", menuItem);
		}

		public void TestSendAcdAgrMessageMenuItem_Position()
		{
			using var form = new JobDeclarationForm(Factory.New<JobDeclaration>());
			form.Show();
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			var sendACDAMessageMenuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Send Agreement of Customs Declaration Agent Message");
			var updateEntryNumberMenuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Update Entry Numbers");
			AssertEquals("Next to and above Update Entry Numbers", updateEntryNumberMenuItem.Index - 1, sendACDAMessageMenuItem.Index);
		}

		public void TestClickSendAcdAgrMessageMenuItem_NoEntrySelected()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Send Agreement of Customs Declaration Agent Message");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menuItem.PerformClick();
			AssertEquals("Only available when one entry is selected.", "Please select one and only one Entry first.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestClickSendAcdAgrMessage_NoError()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			MessageBuilderTestHelper.FillMandatoryFields(items);
			using var form = new JobDeclarationForm(items.JobDeclaration);
			form.Show();
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			enteriesControl.EntriesBoundGrid.Select(0);
			var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Send Agreement of Customs Declaration Agent Message");

			var entryHeader = items.EntryHeader;
			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(Factory, entryHeader.Declaration.CompanyPK.ToGuid(), Guid.Empty))
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menuItem.PerformClick();
				AssertEquals("Agreement of Customs Declaration Agent Message message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, entryHeader.Messages.Count);
			}
		}

		public void TestShowEntryNumbersUpdateForm()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(items.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			Factory.Save();
			using var form = new JobDeclarationForm(items.JobDeclaration);
			form.Show();
			items.JobDeclaration.JE_CustomsOffice = "OF1";
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			enteriesControl.EntriesBoundGrid.Select(0);
			var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Update Entry Numbers");
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(items.JobDeclaration.HasChanges);
			UnitTestUserNotification.Instance.ClearMessages();
			Factory.Save();
			menuItem.PerformClick();
			Assert(ZFormModaliser.LastFormShownDialogForTest is EntryNumbersUpdateForm);
		}

		public void TestGridOptionalColumnsVisible()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(items.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			using var form = new JobDeclarationForm(items.JobDeclaration);
			form.Show();
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			var entryLinesGrid = enteriesControl.FindSingle<ZGrid>("EntryLineGrid");
			AssertNotNull(entryLinesGrid.Columns.Any(column => column.ColumnName == nameof(CusEntryLine.GSTVATAmount)));
			AssertNotNull(entryLinesGrid.Columns.Any(column => column.ColumnName == nameof(CusEntryLine.ExciseAmount)));
			items.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertNotNull(entryLinesGrid.Columns.All(column => column.ColumnName != nameof(CusEntryLine.GSTVATAmount)));
			AssertNotNull(entryLinesGrid.Columns.All(column => column.ColumnName != nameof(CusEntryLine.ExciseAmount)));
		}

		public void TestShowAuditForm()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(items.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			Factory.Save();
			using var form = new JobDeclarationForm(items.JobDeclaration);
			form.Show();
			items.JobDeclaration.JE_CustomsOffice = "OF1";
			var enteriesControl = form.FindCustomsEntriesAndEntryLinesUserControl();
			enteriesControl.EntriesBoundGrid.Select(0);
			var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Audit Entry");
			var notification = UnitTestUserNotification.Instance;
			notification.AddAnswer(DialogResult.No);
			menuItem.PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", notification.LastMessage.Text);
			notification.ClearMessages();
			Factory.Save();
			notification.AddAnswer(DialogResult.Cancel);
			menuItem.PerformClick();
			AssertEquals("Should have shown WriteToLogForm", "Audit Entry", ((ZChildForm)ZFormModaliser.LastFormShownDialogForTest).FormCaption);
		}

		public void TestAddEntryHeaderColumns()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using var form = new ZForm(jobDeclaration);
			using var userControl = new CustomsEntriesAndEntryLinesUserControl();
			form.Controls.Add(userControl);
			form.Show();
			CombineAssertions(() =>
			{
				AssertNotNull("User control should have CH_BGMReference column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference]);
				AssertNotNull("User control should have PreEntryNumber column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.PreEntryNumber]);
				AssertNotNull("User control should have DeclarationUnifiedNumber column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationUnifiedNumber]);
				AssertNotNull("User control should have EntryNumber column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber]);
				var messageTypeColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType];
				AssertNotNull("User control should have CH_MessageType column", messageTypeColumn);
				AssertEquals("CH_MessageType column is Mandatory", true, messageTypeColumn.IsMandatory);
				AssertEquals("CH_MessageType column has Group Key", "0FD6C218-2E35-4294-BB33-70E15F657F9E", messageTypeColumn.GroupName.Key);
				var messageTypeDescriptionColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription];
				AssertNotNull("User control should have CH_MessageTypeDescription column", messageTypeDescriptionColumn);
				AssertEquals("CH_MessageTypeDescription column has Group Key", "0FD6C218-2E35-4294-BB33-70E15F657F9E", messageTypeDescriptionColumn.GroupName.Key);
				var entryStatusColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus];
				AssertNotNull("User control should have CH_EntryStatus column", entryStatusColumn);
				AssertEquals("CH_EntryStatus column is Mandatory", true, entryStatusColumn.IsMandatory);
				AssertEquals("CH_EntryStatus column has Group Key", "9151FB19-0A1B-4468-A931-CA269EA45AE6", entryStatusColumn.GroupName.Key);
				var entryHeaderStatusDescriptionColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription];
				AssertNotNull("User control should have EntryHeaderStatusDescription column", entryHeaderStatusDescriptionColumn);
				AssertEquals("EntryHeaderStatusDescription column has Group Key", "9151FB19-0A1B-4468-A931-CA269EA45AE6", entryHeaderStatusDescriptionColumn.GroupName.Key);
				var packageUQColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CEI_PackageUQ];
				AssertNotNull("User control should have CEI_PackageUQ column", packageUQColumn);
				AssertEquals("CEI_PackageUQ column has Group Key", "D3BF1760-1F8C-4374-B9E3-6F58606E8624", packageUQColumn.GroupName.Key);
				var packageUQDescriptionColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.XC_PackageUQDescription];
				AssertNotNull("User control should have XC_PackageUQDescription column", packageUQDescriptionColumn);
				AssertEquals("XC_PackageUQDescription column has Group Key", "D3BF1760-1F8C-4374-B9E3-6F58606E8624", packageUQDescriptionColumn.GroupName.Key);
				AssertNotNull("User control should have ManualNo column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ManualNo]);
				AssertNotNull("User control should have VesselName column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.VesselName]);
				AssertNotNull("User control should have Voyage column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.Voyage]);
				AssertNotNull("User control should have BillOfLading column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.BillOfLading]);
				AssertNotNull("User control should have CustomsProcedureDesc column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CustomsProcedureDesc]);
				AssertNotNull("User control should have IncoTermDesc column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.IncoTermDesc]);
				AssertNotNull("User control should have CountOfLines column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CountOfLines]);
				AssertNotNull("User control should have InvoiceNumbers column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.InvoiceNumbers]);
				AssertNotNull("User control should have ContractNo column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ContractNo]);
				AssertNotNull("User control should have NumberOfContainers column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.NumberOfContainers]);
				var freightFeeMarkDescColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.FreightFeeMarkDesc];
				AssertNotNull("User control should have FreightFeeMarkDesc column", freightFeeMarkDescColumn);
				AssertEquals("FreightFeeMarkDesc column has Group Key", "CCE19205-CA9C-4AC7-8B16-8E249BFFCC48", freightFeeMarkDescColumn.GroupName.Key);
				var freightFeeAmountColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.FreightFeeAmount];
				AssertNotNull("User control should have FreightFeeAmount column", freightFeeAmountColumn);
				AssertEquals("FreightFeeAmount column has Group Key", "CCE19205-CA9C-4AC7-8B16-8E249BFFCC48", freightFeeAmountColumn.GroupName.Key);
				var freightFeeCurrencyCodeColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.FreightFeeCurrencyCode];
				AssertNotNull("User control should have FreightFeeCurrencyCode column", freightFeeCurrencyCodeColumn);
				AssertEquals("FreightFeeCurrencyCode column has Group Key", "CCE19205-CA9C-4AC7-8B16-8E249BFFCC48", freightFeeCurrencyCodeColumn.GroupName.Key);
				AssertNotNull("User control should have GrossWeightInKG column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.GrossWeightInKG]);
				AssertNotNull("User control should have NetWeightInKG column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.NetWeightInKG]);
				var entrySubmittedDateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate];
				AssertNotNull("User control should have CH_EntrySubmittedDate column", entrySubmittedDateColumn);
				AssertEquals("CH_EntrySubmittedDate column is Mandatory", true, entrySubmittedDateColumn.IsMandatory);
				AssertNotNull("User control should have CH_EntryReleaseDate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryReleaseDate]);
				AssertNotNull("User control should have MovementReferenceNumberIssueDate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate]);
				AssertNotNull("User control should have CH_Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status]);
				AssertNotNull("User control should have MessageStatusDescription column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription]);
				AssertNotNull("User control should have CIQNumber column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CIQNumber]);
				AssertNotNull("User control should have CIQStatus column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CIQStatus]);
				AssertNotNull("User control should have CIQStatusDescription column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CIQStatusDescription]);
				var archiveDateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ArchiveDate];
				AssertNotNull("User control should have ArchiveDate column", archiveDateColumn);
				AssertEquals("ArchiveDate column is not visible as default", false, archiveDateColumn.IsVisible);
				var archiveUserColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ArchiveUser];
				AssertNotNull("User control should have archiveUserColumn column", archiveDateColumn);
				AssertEquals("archiveUserColumn column is not visible as default", false, archiveDateColumn.IsVisible);
				AssertNotNull("User control should have LastAuditedDate column", userControl.EntriesBoundGrid.Columns["LastAuditedDate"]);
				AssertNotNull("User control should have LastAuditedUser column", userControl.EntriesBoundGrid.Columns["LastAuditedUser"]);
				var declarationDeadlineColumn = userControl.EntriesBoundGrid.Columns[CustomsEntriesAndEntryLinesUserControl.Constant.DeclarationDeadline];
				AssertNotNull("User control should have DeclarationDeadline column", declarationDeadlineColumn);
				var daysOfDelayedDeclarationColumn = userControl.EntriesBoundGrid.Columns[nameof(CusEntryHeader.DaysOfDelayedDeclaration)];
				AssertNotNull("User control should have DaysOfDelayedDeclaration column", daysOfDelayedDeclarationColumn);
				AssertEquals("DaysOfDelayedDeclaration column is not visible as default", false, daysOfDelayedDeclarationColumn.IsVisible);
				AssertEquals("DaysOfDelayedDeclaration column has Group Key", "5928CC3D-A830-48E7-97FF-ABA2EFF2FBD7", daysOfDelayedDeclarationColumn.GroupName.Key);
				var feeForDelayedDeclarationColumn = userControl.EntriesBoundGrid.Columns[nameof(CusEntryHeader.FeeForDelayedDeclaration)];
				AssertNotNull("User control should have FeeForDelayedDeclaration column", feeForDelayedDeclarationColumn);
				AssertEquals("FeeForDelayedDeclaration column is not visible as default", false, feeForDelayedDeclarationColumn.IsVisible);
				AssertEquals("FeeForDelayedDeclaration column has Group Key", "5928CC3D-A830-48E7-97FF-ABA2EFF2FBD7", feeForDelayedDeclarationColumn.GroupName.Key);
			}

			);
		}

		public void TestAddEntryLineColumns()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using var form = new ZForm(jobDeclaration);
			using var userControl = new CustomsEntriesAndEntryLinesUserControl();
			form.Controls.Add(userControl);
			form.Show();
			var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
			var columnStyle = entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CertOfOriginNumber));
			AssertEquals("Certificate Of Origin", columnStyle.CaptionResourceString.Caption);
			AssertEquals(false, columnStyle.IsVisible);
			columnStyle = entryLineGrid.GetColumnStyle(nameof(CusEntryLine.TradeAgreementCode));
			AssertEquals("Preferential Code", columnStyle.CaptionResourceString.Caption);
			AssertEquals(false, columnStyle.IsVisible);
			columnStyle = entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CertOfOriginCountry));
			AssertEquals("Origin Under FTA", columnStyle.CaptionResourceString.Caption);
			AssertEquals(false, columnStyle.IsVisible);
			columnStyle = entryLineGrid.GetColumnStyle(nameof(CusEntryLine.ItemNoOnCertOfOrigin));
			AssertEquals("Item No of COO", columnStyle.CaptionResourceString.Caption);
			AssertEquals(false, columnStyle.IsVisible);
			columnStyle = entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CertOfOriginType));
			AssertEquals("COO Type", columnStyle.CaptionResourceString.Caption);
			AssertEquals(false, columnStyle.IsVisible);
		}

		public void TestExportExcelForSingleWindow()
		{
			var items = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(items.EntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			items.JobDeclaration.JE_MessageType = "IMP";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (var declarationForm = new JobDeclarationForm(items.JobDeclaration))
			{
				declarationForm.Show();
				var enteriesControl = declarationForm.FindCustomsEntriesAndEntryLinesUserControl();
				var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Export Excel for Single Window Importing");
				enteriesControl.EntriesBoundGrid.Select(0);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Export excel with message errors", lastMessage.Caption);
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", lastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();
				lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals(null, lastMessage.Caption);
				AssertContains($"Excel for {items.EntryHeader.LocalReferenceNumber} has been saved to eDocs.", lastMessage.Text);
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var declarationForm = new JobDeclarationForm(items.JobDeclaration))
			{
				declarationForm.Show();
				var enteriesControl = declarationForm.FindCustomsEntriesAndEntryLinesUserControl();
				var menuItem = enteriesControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Export Excel for Single Window Importing");
				enteriesControl.EntriesBoundGrid.Select(0);
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Export excel with message errors", lastMessage.Caption);
				AssertContains("There are message errors on this job and you don't have security rights to send with message errors.", lastMessage.Text);
			}
		}
	}
}
