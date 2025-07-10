using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageManager.Testing
{
	class FRMessageManagerCreditCheckWithSecurityHelperTest : TestCaseWithFactory
	{
		public void TestShouldCheckNotLodgedArrived()
		{
			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			FRCustomsDataRegistry.Instance.CheckWhenSendingAnArrivedValidee.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var message = CreateMessageObject(EntryActionCodeList.Codes.VAL);
			Assert(FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));
			Assert("Should Be False when the message type is empty", !FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, ZString.Empty));

			var entryHeader = message.Header;
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			Assert(!FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));

			entryHeader.CH_EntryStatus = ZString.Empty;
			FRCustomsDataRegistry.Instance.CheckWhenSendingAnArrivedValidee.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));

			FRCustomsDataRegistry.Instance.CheckWhenSendingAnArrivedValidee.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));

			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			message.MessageType = EntryActionCodeList.Codes.ANT;
			Assert(!FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));
		}

		public void TestShouldCheckPreLodgedArriving()
		{
			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			FRCustomsDataRegistry.Instance.CheckWhenChangingAnAnticipateToAValidee.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var message = CreateMessageObject(EntryActionCodeList.Codes.VAL);
			var entryHeader = message.Header;
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			Assert(FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));
			Assert("Should Be False when the message type is empty", !FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, ZString.Empty));

			entryHeader.CH_EntryStatus = ZString.Empty;
			Assert(!FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			FRCustomsDataRegistry.Instance.CheckWhenChangingAnAnticipateToAValidee.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));

			FRCustomsDataRegistry.Instance.CheckWhenChangingAnAnticipateToAValidee.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));

			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			message.MessageType = EntryActionCodeList.Codes.ANT;
			Assert(!FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));
		}

		[GuiTest]
		public void TestShouldAlwaysCheckDeniedParty()
		{
			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			FRCustomsDataRegistry.Instance.CheckWhenSendingAnArrivedValidee.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var message = CreateMessageObject(EntryActionCodeList.Codes.VAL);
			var errorCollector = new ErrorCollector();
			var helper = new FRMessageManagerCreditCheckWithSecurityHelper(message, errorCollector);
			Assert(!FRMessageManagerCreditCheckWithSecurityHelper.ShouldCheckCreditForThisDeclarationAndMessage(message.Header.Declaration, message.Header, message.MessageType));
			Assert(helper.WarnAboutCreditChecks());
			AssertEquals("", errorCollector.GetErrorsAsString());

			var declaration = message.Header.Declaration as JobDeclarationForTest;
			declaration.DPSFreightMovementRestricted = true;
			Assert(!helper.WarnAboutCreditChecks());
			AssertEquals("Warning text", "Unable to submit message due to Denied Party Screening cancellation.", errorCollector.GetErrorsAsString());
		}

		public void TestAnyKindOfFRCheckFlagResultsInBaseCreditEnquiryWithPopup()
		{
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var message = CreateMessageObject(EntryActionCodeList.Codes.VAL);
			Factory.Save();
			var messageSender = new DeltaGMessageSender(message, new ErrorCollector());
			messageSender.Send();

			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = message.Header.Declaration as JobDeclarationForTest;
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_IsDebtor = true;
			importer.CompanyData.OB_AROnCreditHold = true;
			importer.OH_Code = "DJC123";
			Factory.Save();
			importer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			messageSender.Send();
			AssertContains("on Credit Hold", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWarnAboutCreditChecks_RemoveCreditCheckErrorInExternalSystem()
		{
			using (ZArchitecture.Environment.Globals.SetIsWinzorForTest(true))
			using (ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(true))
			{
				CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var errorCollector = new ErrorCollector();
				var message = CreateMessageObject(EntryActionCodeList.Codes.VAL);
				Factory.Save();

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_IsDebtor = true;
				org.CompanyData.OB_AROnCreditHold = true;
				org.OH_Code = "DJC123";

				var declaration = message.Header.Declaration as JobDeclarationForTest;
				declaration.JE_CustomsProfile = "XYZ";
				declaration.JE_OH_Supplier = org.PK;
				declaration.JE_OH_Importer = org.PK;
				Factory.Save();

				org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				CombineAssertions(() =>
				{
					using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(
						GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
						AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveLocallyInCW1.Code))
					{
						AssertEquals("Should not be allowed (CW1)", false, new FRMessageManagerCreditCheckWithSecurityHelper(message, errorCollector).WarnAboutCreditChecks());
						AssertEquals("An error was expected", 1, errorCollector.ErrorCount);
						AssertEquals("Submit message with credit restriction canceled.", errorCollector.GetErrors().FirstOrDefault());
						AssertContains("Credit failure expected", "on Credit Hold", UnitTestUserNotification.Instance.LastMessage.Text);
					}

					errorCollector.WipeErrors();

					using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(
						GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
						AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
					{
						AssertEquals("Should not be allowed (EXT)", false, new FRMessageManagerCreditCheckWithSecurityHelper(message, errorCollector).WarnAboutCreditChecks());
						AssertEquals("No error expected", 0, errorCollector.ErrorCount);
						AssertNotContains("No credit failure expected", "on Credit Hold", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				});
			}
		}

		DeltaGJobDeclarationMessageSendingObject CreateMessageObject(ZString messageType)
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = messageType;
			return messageObject;
		}
	}

	class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool IsDPSFreightMovementRestrictedCore()
		{
			return DPSFreightMovementRestricted;
		}
		public bool DPSFreightMovementRestricted { get; set; }
	}
}
