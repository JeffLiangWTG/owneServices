using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ExitMessageSendingTest : TestCaseWithFactory
	{
		public void TestCustomsClearanceCommencedLogAddedOnEXDMessageSent()
		{
			Env.Registry.AUCustomsSenderID = ZString.Empty;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			StmALog mostRecentLog = declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
			AssertNull("CustomsClearacneLog not added", mostRecentLog);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();
			mostRecentLog = declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.ExportCustomsCommenced);
			AssertNotNull("CustomsClearedLog added now", mostRecentLog);
		}

		public void TestSendDeclarationForAnOldExit1Message()
		{
			Env.Registry.AUCustomsSenderID = ZString.Empty;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.DeclarationNumber = "12345";
			declaration.ExportEntryNumber.CE_EntryType = "ECN";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();
			AssertEquals("Exit1 is no longer available. It is not possible to send any further messages for this declaration.", messageInitiator.InvalidOperationText);

			Assert("No Message Sent", declaration.Messages.Count == 0);
		}

		[ExpectNoExceptions]
		public void TestSendWithBlankAUCustomsSenderIDWhenModeIsCMR()
		{
			Env.Registry.AUCustomsSenderID = ZString.Empty;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();

			Assert("Message Sent", declaration.Messages.Count > 0);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendEXDWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendExportDeclaration();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendExportDeclaration();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Only export declarations") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendExportDeclaration();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWhenWithdrawn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearWithdrawal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendExportDeclaration();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("withdrawn") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWithNoInvoices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendExportDeclaration();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Please ensure that there is at least one Invoice Header and that each Invoice Header has at least one Invoice Line.") != -1);
				throw;
			}
		}

		public void TestPerformApportionmentIfDirtyWhenSendingExportDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;

			declaration.ApportionmentDirty = true;

			AssertEquals("Apportionment Dirty", true, declaration.ApportionmentDirty);
			declaration.SendExportDeclaration();

			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("Apportionment performed", false, declaration.ApportionmentDirty);
		}

		public void TestAnswerYesToSendWithMessageErrors()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				messageInitiator.AnswerToContinueWithAction = true;
				declaration.MessageInitiator = messageInitiator;
				declaration.SendExportDeclaration();
				AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
				AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingOriginal.Code, declaration.JE_EntryStatus);
				AssertEquals("Sent With Message Errors", true, declaration.SentWithMessageErrors);
			}
		}

		public void TestEXDShouldSendMessagesInTestMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				Env.Registry.CMRTestMode = true;
				messageInitiator.AnswerToContinueWithAction = true;
				declaration.JE_EntryStatus = CustomsEntryStatus.NotSent.Code;
				declaration.MessageInitiator = messageInitiator;
				declaration.SendExportDeclaration();
				Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
			}
		}

		public void TestHasChangesIsFalseAfterSending()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals("HasChanges", false, declaration.HasChanges);
		}

		public void TestSendDecTwice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();

			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingOriginal.Code, declaration.JE_EntryStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendExportDeclaration();
			AssertEquals("Invalid Op Text", "It is not possible to send this Declaration as another person in your company or a batch processor has changed the status of the declaration.", messageInitiator2.InvalidOperationText);
		}

		public void TestAnswerNoToSendWithMessageErrors()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();
			AssertEquals("DeclarationState", CustomsEntryStatus.NotSent.Code, declaration.JE_EntryStatus);
		}

		public void TestMessageIsNotSentIfIsWithdrawn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearWithdrawal.Code;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.ThrowExceptionOnInvalidOperation = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWithdrawal.Code, declaration.JE_EntryStatus);
		}

		public void TestContinueWithSaveWithMessageErrorsAnsweringYesToSend()
		{
			//			JobDeclaration Declaration = PrepareDeclarationReadyForAmendment();
			//
			//			SendsMessagesToCustomsShutterUpperer MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			//			MessageInitiator.AnswerToContinueWithAction = false;
			//			Declaration.MessageInitiator = MessageInitiator;
			//
			//			bool Continue = Declaration.CanContinueWithSaveSendingAnAmendmentIfNeeded();
			//			Factory.Save();
			//			Assert("Continue", Continue);
			//			AssertEquals("Message Count", 2, Declaration.Messages.Count);
			//			AssertEquals("Sent With Message Errors", true, Declaration.SentWithMessageErrors);
			//			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingReplacement.Code, Declaration.JE_EntryStatus);
			Assert("Joo:ToDo, need to modify SendsMessagesToCustomsShutterUpperer so that it takes different answers", true);
		}

		public void TestContinueWithSaveWithMessageErrorsAnsweringNoToSend()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForAmendment();
			AssertEquals("Precondition: Message Count", 1, declaration.Messages.Count);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;

			bool @continue = declaration.CanContinueWithSaveSendingAnAmendmentIfNeeded();
			Assert("DontContinue", !@continue);
			AssertEquals("Message Count is unchanged", 1, declaration.Messages.Count);
			AssertEquals("DeclarationState", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
		}

		public void TestContinueWithSaveIgnoresAQIS()
		{
			var declaration = PrepareDeclarationReadyForAmendment();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.Invoices[0].QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			AssertEquals("Precondition: Message Count", 1, declaration.Messages.Count);
			AssertEquals("Precondition: Declaration State", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer() { AnswerToContinueWithAction = false }; // Prompt: If you need to send an amendment message, please select 'NO'.

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				Assert("Precondition: NEXDOCS is inactive", !declaration.IsNEXDOCSActive);

				bool @continue = declaration.CanContinueWithSaveSendingAnAmendmentIfNeeded();
				Assert("Does not prompt. Can Continue", @continue);
				AssertEquals("Message Count is unchanged", 1, declaration.Messages.Count);
				AssertEquals("Declaration State is unchanged", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				Assert("Precondition: NEXDOCS is active", declaration.IsNEXDOCSActive);

				bool @continue = declaration.CanContinueWithSaveSendingAnAmendmentIfNeeded();
				Assert("Does not prompt. Can Continue", @continue);
				AssertEquals("Message Count is unchanged", 1, declaration.Messages.Count);
				AssertEquals("Declaration State is unchanged", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			}

			declaration.Invoices[0].QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			Assert("Precondition: NEXDOCS is active for EGG Commodity", declaration.IsNEXDOCSActive);
			Assert("Does not prompt. Can Continue", declaration.CanContinueWithSaveSendingAnAmendmentIfNeeded());
			AssertEquals("Message Count is unchanged", 1, declaration.Messages.Count);
			AssertEquals("Declaration State is unchanged", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
		}

		public void TestContinueWithSave_NotificationWhenServiceTaskIsNotRunning()
		{
			var declaration = PrepareDeclarationReadyForAmendment();
			AssertEquals("Precondition: Message Count", 1, declaration.Messages.Count);

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.OnContinueWithAction += (object sender, WarningEventArgs evt) =>
			{
				if (evt.Message.StartsWith("Declaration changes have been made."))
				{
					messageInitiator.AnswerToContinueWithAction = false;
				}
				else
				{
					messageInitiator.AnswerToContinueWithAction = true;
				}
			};

			declaration.MessageInitiator = messageInitiator;

			using (AUCustomsDataRegistry.Instance.EnableAUServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				serviceTask.S5_IsActive = false;
				serviceTask.S5_ScheduleType = "AUS";
				serviceTask.S5_TypeOfDocument = "AUC";

				AssertNoExceptionThrown(() => declaration.CanContinueWithSaveSendingAnAmendmentIfNeeded());
				AssertEquals("Message Sent", 2, declaration.Messages.Count);
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestWithdrawDeclarationQuestionIsNotAskedIFWeCantWithdraw()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;

			try
			{
				declaration.WithdrawDeclaration();
			}
			catch (ApplicationException e)
			{
				Assert("Correct Error Message", e.Message.IndexOf("You can't withdraw this declaration because it is has not been declared successfully yet.") != -1);
				throw;
			}
		}

		public void TestWithdrawExit1Declaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.DeclarationNumber = "123";
			declaration.ExportEntryNumber.CE_EntryType = "ECN";
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.HasChanges = false;

			declaration.WithdrawDeclaration();
			AssertEquals("Invalid Operation Text", "You can't send any further messages for this declaration as Exit1 no longer exists.", messageInitiator.InvalidOperationText);
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestSendDeclarationOriginalWhenFactorySaveFails()
		{
			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "abc";
			header1.OH_ScreeningStatus = "abc";
			OrgHeader header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "def";
			header2.OH_ScreeningStatus = "def";
			// Org has no relevance to this test other than to ensure saving will fail due to invalid columns.

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendExportDeclaration();
			AssertEquals("DecState", ZString.Empty, declaration.JE_EntryStatus);
			if (declaration.Messages.Count > 0)
			{
				Assert(declaration.Messages[0].IsDeleted);
			}
			AssertEquals("Invalid Operation Text", "A system error has occured, declaration message not sent. Please reload the form and try again.", messageInitiator.InvalidOperationText);
		}

		public void TestNoteTypes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			ZBool foundLetterOfCreditNote = false;
			ZBool foundAdditionalInformationNote = false;
			ZBool foundNotifyTextNote = false;
			ZBool foundAmendmentTextNote = false;
			ZBool foundCancellationReasonTextNote = false;
			foreach (PredefinedNoteType noteType in declaration.NoteTypes)
			{
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description)
				{
					foundLetterOfCreditNote = true;
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description)
				{
					foundAdditionalInformationNote = true;
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCNotifyText.Description)
				{
					foundNotifyTextNote = true;
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description)
				{
					foundAmendmentTextNote = true;
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.NEXDOCCancellationReason.Description)
				{
					foundCancellationReasonTextNote = true;
				}
			}
			Assert("Letter of Credit Note should be avaialble for Quarantine", foundLetterOfCreditNote);
			Assert("Additional Information should be avaialble for Quarantine", foundAdditionalInformationNote);
			Assert("Notify Text should be avaialble for Quarantine", foundNotifyTextNote);
			Assert("Amendment Text should be avaialble for Quarantine", foundAmendmentTextNote);
			Assert("NEXDOCCancellation Reason Text should be avaialble for Quarantine", foundCancellationReasonTextNote);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			foreach (PredefinedNoteType noteType in declaration.NoteTypes)
			{
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description)
				{
					Fail("Letter of credit note should not be found for jobs other than quarantine.");
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description)
				{
					Fail("Additional Information note should not be found for jobs other than quarantine.");
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCNotifyText.Description)
				{
					Fail("Notify Text note should not be found for jobs other than quarantine.");
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description)
				{
					Fail("Amendment Text note should not be found for jobs other than quarantine.");
				}
				if (noteType.Description == PredefinedNoteTypes.Instance.NEXDOCCancellationReason.Description)
				{
					Fail("NEXDOCCancellation Reason Text note should not be found for jobs other than quarantine.");
				}
			}
		}

		public void TestNotificationWhenServiceTaskIsNotRunning()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;

			using (AUCustomsDataRegistry.Instance.EnableAUServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				serviceTask.S5_IsActive = false;
				serviceTask.S5_ScheduleType = "AUS";
				serviceTask.S5_TypeOfDocument = "AUC";

				AssertNoExceptionThrown(() => declaration.SendExportDeclaration());
				Assert("Message Sent", declaration.Messages.Count > 0);
			}
		}

		#region Implementation

		protected JobDeclaration PrepareDeclarationReadyForAmendment()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
				declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Description = "DESCRIPTION A";

				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				messageInitiator.AnswerToContinueWithAction = true;
				declaration.MessageInitiator = messageInitiator;
				declaration.SendExportDeclaration();
				AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
				AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingOriginal.Code, declaration.JE_EntryStatus);
				AssertEquals("Sent With Message Errors", true, declaration.SentWithMessageErrors);

				declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
				declaration.DeclarationNumber = "12345";
				declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Description = "DESCRIPTION B";
				return declaration;
			}
		}

		#endregion
	}
}
