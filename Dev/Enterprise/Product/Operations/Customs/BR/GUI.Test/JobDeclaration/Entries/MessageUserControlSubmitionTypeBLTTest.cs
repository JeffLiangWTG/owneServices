using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class MessageUserControlSubmitionTypeBLTTest : TestCaseWithFactory
	{
		public void TestSetupExportEntryHeaderColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("User control should have Reference Number column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference]);
					AssertNotNull("User control should have Entry Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus]);
					AssertNotNull("User control should have Issue Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate]);
					AssertNotNull("User control should have Entry Submitted Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate]);
					AssertNotNull("User control should have Message Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status]);
					AssertNotNull("User control should have Message Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription]);
					AssertNotNull("User control should have MRN column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumber]);
					AssertNotNull("User control should have Message Type column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType]);
					AssertNotNull("User control should have Message Type Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription]);
					AssertNotNull("User control should have Entry Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription]);
					AssertNotNull("User control should have Message Declaration UCR column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationUCR]);
					AssertNotNull("User control should have Entry Access Key column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryAccessKey]);
					AssertNotNull("User control should have Cargo Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_CargoStatus]);
					AssertNotNull("User control should have Cargo Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CargoStatusDescription]);
					AssertNotNull("User control should have Administrative Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AdministrativeStatus]);
					AssertNotNull("User control should have Administrative Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AdministrativeStatusDescription]);
					AssertNotNull("User control should have Risk Channel column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_RiskChannel]);
					AssertNotNull("User control should have Risk Channel Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RiskChannelDescription]);
					AssertNull("User control should NOT have Import License Identifier column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportLicenseIdentifier]);
				});
			}
		}

		public void TestSetupImportEntryHeaderColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("User control should have Entry Number column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber]);
					AssertNotNull("User control should have Reference Number column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference]);
					AssertNotNull("User control should have Entry Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus]);
					AssertNotNull("User control should have Issue Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate]);
					AssertNotNull("User control should have Entry Submitted Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate]);
					AssertNotNull("User control should have Message Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status]);
					AssertNotNull("User control should have Message Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription]);
					AssertNull("User control should NOT have MRN column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumber]);
					AssertNotNull("User control should have Message Type column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType]);
					AssertNotNull("User control should have Message Type Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription]);
					AssertNotNull("User control should have Entry Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription]);
					AssertNull("User control should NOT have Message Declaration UCR column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationUCR]);
					AssertNotNull("User control should have Entry Access Key column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryAccessKey]);
					AssertNotNull("User control should have Cargo Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_CargoStatus]);
					AssertNotNull("User control should have Cargo Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CargoStatusDescription]);
					AssertNotNull("User control should have Administrative Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AdministrativeStatus]);
					AssertNotNull("User control should have Administrative Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AdministrativeStatusDescription]);
					AssertNotNull("User control should have Risk Channel column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_RiskChannel]);
					AssertNotNull("User control should have Risk Channel Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RiskChannelDescription]);
					AssertNull("User control should NOT have Import License Identifier column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportLicenseIdentifier]);
					AssertNotNull("User control should have Authority Version column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AuthorityVersion]);
				});
			}
		}

		public void TestSetupImportSicomexEntryHeaderColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("User control should have Entry Number column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber]);
					AssertNotNull("User control should have Reference Number column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference]);
					AssertNotNull("User control should have Entry Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus]);
					AssertNotNull("User control should have Issue Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate]);
					AssertNotNull("User control should have Entry Submitted Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate]);
					AssertNotNull("User control should have Message Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status]);
					AssertNotNull("User control should have Message Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription]);
					AssertNull("User control should NOT have MRN column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumber]);
					AssertNotNull("User control should have Message Type column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType]);
					AssertNotNull("User control should have Message Type Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription]);
					AssertNotNull("User control should have Entry Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription]);
					AssertNull("User control should NOT have Message Declaration UCR column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationUCR]);
					AssertNull("User control should NOT have Entry Access Key column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryAccessKey]);
					AssertNull("User control should NOT have Cargo Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_CargoStatus]);
					AssertNull("User control should NOT have Cargo Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CargoStatusDescription]);
					AssertNull("User control should NOT have Administrative Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AdministrativeStatus]);
					AssertNull("User control should NOT have Administrative Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AdministrativeStatusDescription]);
					AssertNotNull("User control should have Risk Channel column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_RiskChannel]);
					AssertNotNull("User control should have Risk Channel Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RiskChannelDescription]);
					AssertNull("User control should NOT have Import License Identifier column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportLicenseIdentifier]);
					AssertNull("User control should have NOT Authority Version column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AuthorityVersion]);
				});
			}
		}

		public void TestResetToOriginalPopUpMenu()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00000001-1";
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var resetToOriginalMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Reset to Original");

				userControl.EntriesBoundGrid.Select(0);
				userControl.EntriesBoundGrid.ContextMenu.DoPopup();
				AssertEquals(false, resetToOriginalMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				entryHeader.CH_MessageType = MessageTypeList.Codes.CDE;
				userControl.EntriesBoundGrid.Select(0);
				userControl.EntriesBoundGrid.ContextMenu.DoPopup();
				AssertEquals(true, resetToOriginalMenuItem.Visible);

				resetToOriginalMenuItem.PerformClick();
				AssertEquals("Entry B00000001-1 has been 'Reset to Original'", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsultEntryPopUpMenu()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00000001-1";
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDE;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var consultEntryMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Consult Entry");

				userControl.EntriesBoundGrid.Select(0);
				userControl.EntriesBoundGrid.ContextMenu.DoPopup();
				AssertEquals(false, consultEntryMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;
				userControl.EntriesBoundGrid.Select(0);
				userControl.EntriesBoundGrid.ContextMenu.DoPopup();
				AssertEquals(true, consultEntryMenuItem.Visible);

				consultEntryMenuItem.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consultEntryMenuItem.PerformClick();
				AssertEquals("The selected entry cannot be consulted. The entry version is zero or empty.", UnitTestUserNotification.Instance.LastMessage.Text);

				entryHeader.CH_AuthorityVersion = "0";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consultEntryMenuItem.PerformClick();
				AssertEquals("The selected entry cannot be consulted. The entry version is zero or empty.", UnitTestUserNotification.Instance.LastMessage.Text);

				entryHeader.CH_AuthorityVersion = "1";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consultEntryMenuItem.PerformClick();
				var newMessageCompleteConsult = entryHeader.Messages.LastMessage;
				AssertEquals(MessageTypeList.Codes.CIH, newMessageCompleteConsult.EM_MessageType);
				AssertEquals(EDIMessageSubTypeList.Codes.CompleteConsult, newMessageCompleteConsult.EM_MessageSubType);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRetriggerEntryDataRequestOption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "TST_DEC1";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00000001-1";
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var triggerEntryDataRequest = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Re-trigger Entry Data Request");

				userControl.EntriesBoundGrid.Select(0);
				userControl.EntriesBoundGrid.ContextMenu.DoPopup();
				AssertEquals(false, triggerEntryDataRequest.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				entryHeader.CH_MessageType = MessageTypeList.Codes.CDE;
				userControl.EntriesBoundGrid.Select(0);
				userControl.EntriesBoundGrid.ContextMenu.DoPopup();
				AssertEquals(true, triggerEntryDataRequest.Visible);

				triggerEntryDataRequest.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var test = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				Factory.Save();

				entryHeader.MovementReferenceNumberSetter("23BR0010362485");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				triggerEntryDataRequest.PerformClick();
				AssertEquals("No COM - Complete Consult Message has been detected.", UnitTestUserNotification.Instance.LastMessage.Text);

				var messageCompleteConsult = Factory.New<BREDIMessage>();
				messageCompleteConsult.EM_MessageType = MessageTypeList.Codes.CDE;
				messageCompleteConsult.EM_MessageSubType = EDIMessageSubTypeList.Codes.CompleteConsult;
				messageCompleteConsult.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				var interchangeCompleteConsult = Factory.New<EDIInterchange>();
				interchangeCompleteConsult.EI_SessionGUID = ZGuid.NewZGuid();
				interchangeCompleteConsult.EI_From = BREDIInterchange.BRCustoms;
				interchangeCompleteConsult.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
				messageCompleteConsult.EM_EI = interchangeCompleteConsult.PK;
				entryHeader.Messages.Add(messageCompleteConsult);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				triggerEntryDataRequest.PerformClick();
				AssertEquals("No XER - Customs Error Message has been detected to re-trigger the request.", UnitTestUserNotification.Instance.LastMessage.Text);

				var messageXER = Factory.New<BREDIMessage>();
				messageXER.EM_MessageType = MessageTypeList.Codes.XER;
				messageXER.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				messageXER.EM_EI = Factory.New<EDIInterchange>().PK;
				messageXER.Interchange.EI_SessionGUID = messageCompleteConsult.Interchange.EI_SessionGUID;
				messageXER.Interchange.EI_From = BREDIInterchange.BRCustoms;
				messageXER.Interchange.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
				entryHeader.Messages.Add(messageXER);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				triggerEntryDataRequest.PerformClick();
				AssertEquals("A new request will be sent to Customs to retrieve data from the selected Entry. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Re-trigger Entry Data Confirmation", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(2, entryHeader.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				triggerEntryDataRequest.PerformClick();
				AssertEquals(3, entryHeader.Messages.Count);
				var newMessageCompleteConsult = entryHeader.Messages.LastMessage;
				AssertEquals(MessageTypeList.Codes.CDE, newMessageCompleteConsult.EM_MessageType);
				AssertEquals(EDIMessageSubTypeList.Codes.CompleteConsult, newMessageCompleteConsult.EM_MessageSubType);

				newMessageCompleteConsult.EM_EI = Factory.NewWithValidTestData<EDIInterchange>().PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				triggerEntryDataRequest.PerformClick();
				AssertEquals("No XER - Customs Error Message has been detected to re-trigger the request.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEntriesBoundGridColumnsVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Reference Number column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference].IsVisible);
					AssertEquals("Message Type column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType].IsVisible);
					AssertEquals("Message Type Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription].IsVisible);
					AssertEquals("Entry Submitted Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate].IsVisible);
					AssertEquals("Issue Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate].IsVisible);
					AssertEquals("Entry Number column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber].IsVisible);
					AssertEquals("CH_AuthorityVersion column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AuthorityVersion].IsVisible);
					AssertEquals("Entry Status column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus].IsVisible);
					AssertEquals("Entry Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription].IsVisible);
					AssertEquals("Risk Channel Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RiskChannelDescription].IsVisible);
					AssertEquals("Message Status column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status].IsVisible);
					AssertEquals("Message Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription].IsVisible);
				});
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Reference Number column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference].IsVisible);
					AssertEquals("Message Type column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType].IsVisible);
					AssertEquals("Message Type Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription].IsVisible);
					AssertEquals("Entry Submitted Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate].IsVisible);
					AssertEquals("Issue Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate].IsVisible);
					AssertEquals("Entry Number column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber].IsVisible);
					AssertEquals("Entry Status column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus].IsVisible);
					AssertEquals("Entry Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription].IsVisible);
					AssertEquals("Risk Channel Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RiskChannelDescription].IsVisible);
					AssertEquals("Message Status column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status].IsVisible);
					AssertEquals("Message Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription].IsVisible);
				});
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Entry Number column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber].IsVisible);
					AssertEquals("Reference Number column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference].IsVisible);
					AssertEquals("Message Type Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription].IsVisible);
					AssertEquals("Entry Status column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus].IsVisible);
					AssertEquals("Issue Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate].IsVisible);
					AssertEquals("Entry Submitted Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate].IsVisible);
					AssertEquals("Message Status column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status].IsVisible);
					AssertEquals("Message Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription].IsVisible);
					AssertEquals("MRN column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumber].IsVisible);
					AssertEquals("Message Type column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType].IsVisible);
					AssertEquals("Message Type Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription].IsVisible);
					AssertEquals("Entry Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription].IsVisible);
					AssertEquals("Message Declaration UCR column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationUCR].IsVisible);
					AssertEquals("Entry Access Key column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryAccessKey].IsVisible);
					AssertEquals("Cargo Status column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_CargoStatus].IsVisible);
					AssertEquals("Cargo Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CargoStatusDescription].IsVisible);
					AssertEquals("Administrative Status column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AdministrativeStatus].IsVisible);
					AssertEquals("Administrative Status Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AdministrativeStatusDescription].IsVisible);
					AssertEquals("Risk Channel column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_RiskChannel].IsVisible);
					AssertEquals("Risk Channel Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RiskChannelDescription].IsVisible);
				});
			}
		}

		public void TestEntryLineGridWithColumnsOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var entriesGrid = userControl.EntriesBoundGrid;
				entriesGrid.ResetColumns();

				for (int i = 0; i < ExpectedImportColumnNamesListOnThisOrder.Count; i++)
				{
					var expectedColumnName = ExpectedImportColumnNamesListOnThisOrder[i];
					AssertEquals("Expected", expectedColumnName, entriesGrid.Columns[i].ColumnStyle.MappingName);
				}
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLT())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var entriesGrid = userControl.EntriesBoundGrid;
				entriesGrid.ResetColumns();

				for (int i = 0; i < ExpectedImportSiscomexColumnNamesListOnThisOrder.Count; i++)
				{
					var expectedColumnName = ExpectedImportSiscomexColumnNamesListOnThisOrder[i];
					AssertEquals("Expected", expectedColumnName, entriesGrid.Columns[i].ColumnStyle.MappingName);
				}
			}
		}

		public void TestEntryLineAdditionalDataUserControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLTForTesting())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				userControl.EntryLinesMessagesTabControl_Exposed.SelectedTab = userControl.EntryLinesTabPage_Exposed;

				AssertEquals(true, userControl.EntryLineAdditionalDataUserControl.Visible);
				AssertEquals(false, userControl.ExtendedInfoGroupBox_Exposed.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

				AssertEquals(false, userControl.EntryLineAdditionalDataUserControl.Visible);
				AssertEquals(true, userControl.ExtendedInfoGroupBox_Exposed.Visible);
			}
		}

		public void TestBaseMessagesTabUserControlType()
		{
			using (var control = new MessageUserControlSubmitionTypeBLTForTesting())
			{
				AssertEquals(typeof(MessagesTabUserControl), control.GetBaseMessagesTabUserControlTypeExposed());
			}
		}

		public void TestGenerateImportLicenseMenu()
		{
			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC_DEC";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00000001-1";
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CH = entryHeader.PK;
			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine.PK;
			var invLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine.PK;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLTForTesting())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var generateImportLicense = userControl.EntryLineGrid_Exposed.ContextMenu.MenuItems.FindByText("Generate Import License");
				userControl.EntriesBoundGrid.Select(0);
				userControl.EntryLinesMessagesTabControl_Exposed.SelectedTab = userControl.EntryLinesTabPage_Exposed;
				userControl.EntryLineGrid_Exposed.ContextMenu.DoPopup();
				Assert("Generate Import License must NOT be Visible", !generateImportLicense.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				userControl.EntryLineGrid_Exposed.Select(0);
				userControl.EntryLineGrid_Exposed.ContextMenu.DoPopup();
				Assert("Generate Import License must be Visible", generateImportLicense.Visible);

				generateImportLicense.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Ask saving job", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType<GenerateImportLicenseForm>(ZFormModaliser.LastFormShownDialogForTest);

				invLine1.ImportLicenseNumber = "123456";
				Factory.Save();

				generateImportLicense.PerformClick();
				AssertEquals("There is already an Import License registered from this Entry Line", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEntriesBoundGridBindingSource()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLTForTesting())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals($"{userControl.EntriesBoundGrid.Name} should bind to FormalEntryHeaders", "FormalEntryHeaders", userControl.EntriesBoundGrid.GetBindingMember());
					AssertEquals($"{userControl.SiscomexUsageFeeUserControl.Name} should bind to FormalEntryHeaders", "FormalEntryHeaders.SiscomexUsageFees", userControl.SiscomexUsageFeeUserControl.GetBindingMember());
					AssertEquals($"{userControl.EntryLineGrid_Exposed.Name} should bind to FormalEntryHeaders", "FormalEntryHeaders.AllEntryLines", userControl.EntryLineGrid_Exposed.GetBindingMember());

					AssertBindingMember(userControl);
				});

				void AssertBindingMember(Control control)
				{
					Assert($"{control.Name} should bind to FormalEntryHeaders instead of CustomsEntryHeaders", !control.GetBindingMember().StartsWith("CustomsEntryHeaders"));

					foreach (Control child in control.Controls)
					{
						AssertBindingMember(child);
					}
				}
			}
		}

		public void TestSiscomexUsageFeeTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlSubmitionTypeBLTForTesting())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var siscomexUsageFeeTabPage = userControl.SiscomexUsageFeeTabPage_Exposed;
				AssertEquals("SISCOMEX Usage Fee", siscomexUsageFeeTabPage.CaptionResourceString.Caption);
				Assert(siscomexUsageFeeTabPage.TabVisible);
				AssertType<SiscomexUsageFeeUserControl>(siscomexUsageFeeTabPage.Controls.Find("SiscomexUsageFeeUserControl", true)[0]);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				Assert(!siscomexUsageFeeTabPage.TabVisible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				Assert(!siscomexUsageFeeTabPage.TabVisible);
			}
		}

		List<ZString> ExpectedImportColumnNamesListOnThisOrder
		{
			get
			{
				if (expectedImportColumnNamesListOnThisOrder == null)
				{
					expectedImportColumnNamesListOnThisOrder = new List<ZString>();

					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_BGMReference);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_MessageType);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_MessageTypeDescription);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_EntrySubmittedDate);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.MovementReferenceNumberIssueDate);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.EntryNumber);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_AuthorityVersion);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_EntryStatus);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.EntryHeaderStatusDescription);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.RiskChannelDescription);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_Status);
					expectedImportColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.MessageStatusDescription);
				}
				return expectedImportColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedImportColumnNamesListOnThisOrder;

		List<ZString> ExpectedImportSiscomexColumnNamesListOnThisOrder
		{
			get
			{
				if (expectedImportSiscomexColumnNamesListOnThisOrder == null)
				{
					expectedImportSiscomexColumnNamesListOnThisOrder = new List<ZString>();

					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_BGMReference);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_MessageType);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_MessageTypeDescription);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_EntrySubmittedDate);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.MovementReferenceNumberIssueDate);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.EntryNumber);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_EntryStatus);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.EntryHeaderStatusDescription);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.RiskChannelDescription);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.CH_Status);
					expectedImportSiscomexColumnNamesListOnThisOrder.Add(CusEntryHeader.Schema.MessageStatusDescription);
				}
				return expectedImportSiscomexColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedImportSiscomexColumnNamesListOnThisOrder;

		class MessageUserControlSubmitionTypeBLTForTesting : MessageUserControlSubmitionTypeBLT
		{
			public ZTabControl EntryLinesMessagesTabControl_Exposed => EntryLinesMessagesTabControl;
			public ZTabPage EntryLinesTabPage_Exposed => EntryLinesTabPage;
			public ZGroupBox ExtendedInfoGroupBox_Exposed => ExtendedInfoGroupBox;
			public ZGrid EntryLineGrid_Exposed => EntryLineGrid;
			public ZTabPage SiscomexUsageFeeTabPage_Exposed => SiscomexUsageFeeTabPage;
			public Type GetBaseMessagesTabUserControlTypeExposed() => base.GetBaseMessagesTabUserControlType();
		}
	}
}
