using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;
using TransportTypeList = Enterprise.Customs.CA.Business.TransportTypeList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class DeferredB3PostSaveSendingExtensionTest : TestCaseWithFactory
	{
		public void TestOnSaveCompletedOrAborted()
		{
			CombineAssertions(() =>
			{
				TestJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				TestJobDeclaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddMonths(2);
				var invoice = TestJobDeclaration.Invoices.AddNew();
				var line = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Enterprise.Core.Constants.Weight.Kilograms);
				line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
				TestJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				TestJobDeclaration.DoMerge();
				line.DutiesAndTaxes.DeleteAll();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestJobDeclaration.AskForB3ActionIfDeferredMessageExists();
				Assert("Saving, not triggering", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			});

			testJobDeclaration = null;
			CombineAssertions(() =>
			{
				TestJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				TestJobDeclaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddMonths(2);
				var invoice = TestJobDeclaration.Invoices.AddNew();
				var line = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Enterprise.Core.Constants.Weight.Kilograms);
				line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
				TestJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				TestJobDeclaration.DoMerge();
				var b3entryHeader = TestJobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				var testmessage = b3entryHeader.Messages.AddNew();
				testmessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testmessage.EM_Status = "QUE";
				testmessage.EM_ReceiveTransmit = "TRX";
				var existingScheduledTime = DateTime.UtcNow.AddDays(2);
				var existingScheduledTimeTranslated = (ZDateTime)EnvProxy.Instance.Time.GetLocalTimeFromUtc(existingScheduledTime);
				testmessage.EM_HeldUntilDate = existingScheduledTime;
				testmessage.EM_IsActive = true;
				line.DutiesAndTaxes.DeleteAll();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertEquals("ContinueWithSave", ContinueWithSave.Yes, TestJobDeclaration.AskForB3ActionIfDeferredMessageExists());
				Assert("NeedCancelDeferredB3CADMessage", b3entryHeader.NeedCancelDeferredB3CADMessage);
				Assert("NeedResendDeferredB3CADMessage", b3entryHeader.NeedResendDeferredB3CADMessage);

				var expectedMessage = string.Format("You have made changes when there is a deferred Entry message to be sent at {0}. The previous message will be canceled and a new Entry message will be scheduled to replace it.", existingScheduledTimeTranslated);
				Assert("Saving, triggering", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				TestJobDeclaration.ResendDeferredMessageIfRequired();
				AssertEquals("Saving, triggering, Say Yes", string.Format("System cannot send a B3 Message for Declaration B00001001 message as The Network Client ID is not configured,\r\nin the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", existingScheduledTimeTranslated), UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestAskForB3ActionIfMVFEventPosted()
		{
			try
			{
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
				var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddMonths(2);
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = "CAD";
				invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
				var line = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Enterprise.Core.Constants.Weight.Kilograms);
				line.JI_CustomsQuantity = 1000;
				line.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				factory.Save();
				declaration.AskForMessageActionIfMVFEventPosted();
				Assert("No MVF event posted", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				declaration.Logs.AddNew(Events.MessageValidationFailed, ZDateTimeOffset.Now);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.AskForMessageActionIfMVFEventPosted();
				Assert("JE_EntryAuthorisationDate is empty", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddMonths(2);
				declaration.CA_K84AccountingDate = ZDateTime.Now;
				factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.AskForMessageActionIfMVFEventPosted();
				Assert("CA_K84AccountingDate is not empty", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				declaration.CA_K84AccountingDate = ZDateTime.Empty;
				declaration.B3EntryHeader.CH_EntryStatus = "CLR";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory.Save();
				declaration.AskForMessageActionIfMVFEventPosted();
				Assert("B3 Entry status is CLR", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				declaration.B3EntryHeader.CH_EntryStatus = "CNF";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory.Save();
				declaration.AskForMessageActionIfMVFEventPosted();
				Assert("B3 Entry status is CNF", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				declaration.B3EntryHeader.CH_EntryStatus = "";
				declaration.B3EntryHeader.CH_Status = "AWO";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory.Save();
				declaration.AskForMessageActionIfMVFEventPosted();
				Assert("B3 message status is AWO", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				declaration.B3EntryHeader.CH_Status = "";
				declaration.DoMerge();
				factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				declaration.AskForMessageActionIfMVFEventPosted();
				AssertEquals("It appears that you are making changes to a declaration where an attempt to automatically send a CAD failed due to validation errors. Do you want to send the CAD message now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cancel to create B3 message", 0, declaration.B3EntryHeader.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZArchitecture.GUI.ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				declaration.AskForMessageActionIfMVFEventPosted();
				AssertEquals("A B3 message should be created", "B3C", declaration.B3EntryHeader.Messages[0].EM_MessageType);
			}
			finally
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestAskForB3ActionIfDeferredMessageExistsDoesHandlesNull()
		{
			JobDeclaration declaration = null;
			declaration.AskForB3ActionIfDeferredMessageExists();
		}

		[ExpectNoExceptions]
		public void TestResendDeferredB3MessageIfRequiredHandlesNull()
		{
			JobDeclaration declaration = null;
			declaration.ResendDeferredMessageIfRequired();
		}

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		JobDeclaration testJobDeclaration;
		JobDeclaration TestJobDeclaration => testJobDeclaration ?? (testJobDeclaration = Factory.NewWithValidTestData<JobDeclaration>());
	}
}
