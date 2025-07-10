using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(EDIReleaseImportMessageManager))]
	sealed class EDIReleaseImportMessageManagerTest : CAMessageManagerTestCase
	{
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

		public void TestLogCustomsCommenced()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();

			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);

			var сommencedEvent = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "CA Import");
			AssertNotNull("Customs Commenced event should exist on declaration", сommencedEvent);

			manager.ResetDeclaration();
			сommencedEvent = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "CA Import");
			AssertNull("Customs Commenced event should be cancelled", сommencedEvent);
			var cancelledEvent = declaration.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Customs Commenced event cancelled should exist on declaration", cancelledEvent);
		}

		public void TestOnMessageQueuedForSending()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();

			var wrapper = new EDIReleaseImportMessageWrapper(entryHeader);
			var testManager = new EDIReleaseImportMessageManagerForTesting(wrapper);

			Assert("CH_EntrySubmittedDate is not set", entryHeader.CH_EntrySubmittedDate.IsEmpty);
			Assert("JE_EntrySubmittedDate is not set", declaration.JE_EntrySubmittedDate.IsEmpty);

			testManager.Call_OnMessageQueuedForSending(MessageSubTypes.Create);

			Assert("CH_EntrySubmittedDate is set to Now", entryHeader.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			Assert("JE_EntrySubmittedDate is set to scheduledTime", declaration.JE_EntrySubmittedDate == entryHeader.CH_EntrySubmittedDate);
		}

		public void TestSubmissionDateOnCustomsCommenced()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();

			Assert(declaration.JE_EntrySubmittedDate.IsEmpty);
			Assert(entryHeader.CH_EntrySubmittedDate.IsEmpty);

			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);

			Assert(!declaration.JE_EntrySubmittedDate.IsEmpty);
			Assert(!entryHeader.CH_EntrySubmittedDate.IsEmpty);
		}

		#region TestPopulateMessages

		public override void TestPopulateMessages()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = helper.Consignor.PK;
			declaration.JE_OH_Importer = helper.Consignee.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00123457";
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			invoice1Line1.JI_CL = entryLine.PK;

			declaration.CA_ServiceOption = ServiceOptions.Codes.IID;
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			manager.PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals("1 message", 1, entryHeader.Messages.Count);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodes.Codes.Original, entryHeader.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		}

		#endregion

		#region TestGetMessageBuilder

		public override void TestGetMessageBuilder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new EDIReleaseImportMessageWrapper(entryHeader);
			AssertEDIReleaseMessageBuilder(wrapper, typeof(EDIReleaseMessageBuilder));
		}

		#endregion

		#region TestMessageFriendlyName

		public override void TestMessageFriendlyName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			var invoice1 = declaration.Invoices.AddNew();
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			invoice1.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			AssertEquals("ACROSS options are not specified", "ACROSS for 12345000067897", manager.MessageFriendlyName);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			AssertEquals("PARS AQ to Follow", "Pre-arrival EDI Release (AQ to follow) for 12345000067897", manager.MessageFriendlyName);

			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
			AssertEquals("PARS AQ", "Pre-arrival EDI Release (Appraisal Quality) for 12345000067897", manager.MessageFriendlyName);

			declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
			AssertEquals("Replace with AQ", "Replace Minimum EDI Release with AQ Data (Appraisal Quality) for 12345000067897", manager.MessageFriendlyName);

			declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			AssertEquals("Invalid combination of ACROSS options", "ACROSS for 12345000067897", manager.MessageFriendlyName);
		}

		#endregion

		#region TestCanSendThisMessage

		public override void TestCanSendThisMessage()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				Env.Security.CAACROSSMsgSend.IsAllowed = false;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ZString.Empty;
				declaration.JE_IsCancelled = true;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				var importer = Factory.NewWithValidTestData<OrgHeader>();

				var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
				ZString messageText;
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertContains("Error Message", "Job not yet saved, Please save before sending.", messageText);

				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				Assert(messageText, messageText.Contains("You do not have the appropriate security rights to run this function."));
				Assert(messageText, messageText.Contains("ACROSS Message Send"));

				Env.Security.CAACROSSMsgSend.IsAllowed = true;
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertContains("Error Message", "Sending messages for this job is not allowed as it has been marked as inactive. Please check why this job has been deactivated and, if required, mark as active so you can send messages (see 'Make Active' on the actions menu).", messageText);

				declaration.JE_IsCancelled = false;
				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertContains("Error Message", "Both a valid Service Option and Assessment Option should be specified.", messageText);

				declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
				declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
				Factory.Save();
				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				AssertContains("Error Message", "Invalid combination of Service Option and Assessment Option.", messageText);

				declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
				declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
				Factory.Save();
				Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));

				declaration.JE_OH_Importer = importer.PK;
				declaration.Importer.CompanyData.OB_AROnCreditHold = true;
				declaration.Importer.CompanyData.OB_IsDebtor = true;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();

				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
				declaration.DeriveDeclarationStatus();
				Factory.Save();

				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
				declaration.DeriveDeclarationStatus();
				Factory.Save();

				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				declaration.Importer.CompanyData.OB_IsDebtor = false;
				declaration.Importer.CompanyData.OB_AROnCreditHold = false;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();

				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				for (int i = entryHeader.AllEntryLines.Count; i < 999; i++)
				{
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					entryLine = entryHeader.AllEntryLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
				}

				manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText);
				Assert(!messageText.Contains("ACROSS entry does not allow more than 999 lines."));
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				entryLine = entryHeader.AllEntryLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				Factory.Save();
				manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText);
				AssertContains("ACROSS entry does not allow more than 999 lines.", messageText);
			}
		}

		public void TestErrorPromptSkippedIfExternalCreditApprovalSystemUsed()
		{
			AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code);

			Env.Security.CAACROSSMsgSend.IsAllowed = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.JE_IsCancelled = false;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));

			Factory.Save();
			Assert("Not OK to send message", !manager.SendMessage(MessageSubTypes.Create));

			var notification = manager.Notification;
			AssertNotNull(notification);
			Assert("Should show pop up if sending is not stopped for Credit Check", notification.LastMessage.Contains("You do not have the appropriate security rights to run this function."));

			manager.Notification.Reset();

			Env.Security.CAACROSSMsgSend.IsAllowed = true;

			Assert("Not OK to send message", !manager.SendMessage(MessageSubTypes.Create));
			notification = manager.Notification;

			AssertEquals("Should not show pop as sending is stopped for Credit Check", string.Empty, notification.LastMessage);
		}

		#endregion

		#region TestSendMessage

		[TestDate(2015, 1, 1)]
		public void TestSendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Env.Security.CAACROSSMsgSend.IsAllowed = true;
			declaration.CA_ServiceOption = ServiceOptions.Codes.ReplaceRMDwithAQ;
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;

			var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;

			var unlocoZ = declaration.Branch.HomePort;
			unlocoZ.RL_R3 = timeZoneSet.PK;
			var effectiveDate = unlocoZ.LocationDateTime;
			declaration.JE_RL_NKPortOfArrival = unlocoZ.RL_Code;
			Factory.Save();

			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));

			#region JE_DateOfFirstArrival is Empty
			declaration.JE_DateOfFirstArrival = ZDateTime.Empty;
			declaration.JE_DateOfArrival = effectiveDate.AddDays(-1);
			Factory.Save();
			var notification = manager.Notification;
			{
				var testingMessageSubtype = MessageSubTypes.Create;
				manager.SendMessage(testingMessageSubtype);
				Assert("not IsWaitingForResponse", !notification.IsWaitingForResponse);
				Assert("ContainsValidationErrors", notification.ContainsValidationErrors);
				AssertContains("ValidationErrorsMessage", MessageSendingValidation.MessageErrorsExistHeaderText, notification.ValidationErrorsMessage);
				AssertContains("ValidationErrorsMessage", MessageSendingValidation.WarningWhenInTestModeText, notification.ValidationErrorsMessage);
				Assert("not ContainsAdditionalWarnings", !notification.ContainsAdditionalWarnings);
				AssertEquals("AdditionalWarningsMessage", ZString.Empty, manager.GetAdditionalWarningsMessage_Exposed(testingMessageSubtype));
				AssertEquals("AdditionalWarningsMessage", ZString.Empty, notification.AdditionalWarningsMessage);
			}

			declaration.JE_DateOfArrival = effectiveDate;
			Factory.Save();
			AssertContainsAdditionalWarnings(manager, false);

			declaration.JE_DateOfArrival = effectiveDate.AddDays(1);
			Factory.Save();
			AssertContainsAdditionalWarnings(manager, true, "This shipment has not yet arrived.");
			#endregion

			#region JE_DateOfFirstArrival is not Empty
			declaration.JE_DateOfFirstArrival = effectiveDate.AddDays(31);
			Factory.Save();
			AssertContainsAdditionalWarnings(manager, true, "ETA First Port of Arrival date cannot be farther than 30 days in the future.");

			declaration.JE_DateOfFirstArrival = effectiveDate.AddHours(73);
			Factory.Save();
			AssertContainsAdditionalWarnings(manager, false);

			declaration.JE_DateOfFirstArrival = effectiveDate.AddHours(73);
			var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Tariff = "0200000000";
			line.InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			AssertContainsAdditionalWarnings(manager, true, "This product from the US is outside the allowable time frame for reporting (more than 72 hours or less than 4 hours).");

			declaration.JE_DateOfFirstArrival = effectiveDate.AddHours(3);
			Factory.Save();
			AssertContainsAdditionalWarnings(manager, true, "This product from the US is outside the allowable time frame for reporting (more than 72 hours or less than 4 hours).");

			line.JI_Tariff = "0300000000";
			line.InvoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.China;
			Factory.Save();
			AssertContainsAdditionalWarnings(manager, false);

			declaration.JE_DateOfFirstArrival = effectiveDate.AddDays(31);
			declaration.JE_DateOfArrival = effectiveDate.AddDays(-1);
			Factory.Save();

			var builder = new ZStringBuilder();
			builder.Append("This job is already clear or has had an ACROSS entry lodged and has already arrived. Are you sure you wish to amend this release entry?");
			builder.Append("ETA First Port of Arrival date cannot be farther than 30 days in the future.");
			AssertEquals("AdditionalWarningsMessage", builder.ToStringWithDelimiterBetweenAppends("\r\n"), manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change));
			#endregion
		}

		void AssertContainsAdditionalWarnings(EDIReleaseImportMessageManagerForTesting manager, bool hasWarning, string message = "")
		{
			var notification = manager.Notification;
			notification.Reset();
			manager.SendMessage(MessageSubTypes.Request, false);
			AssertEquals("ContainsAdditionalWarnings", hasWarning, notification.ContainsAdditionalWarnings);
			AssertEquals("AdditionalWarningsMessage", manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change), notification.AdditionalWarningsMessage);
			AssertEquals("AdditionalWarningsMessage", message, notification.AdditionalWarningsMessage);
		}

		#endregion

		public void TestValidationTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			AssertEquals("ValidationType", ValidateForMessageType.ACROSS, manager.ValidateTypeForTesting);
		}

		[TestDate(2015, 1, 1)]
		public void TestIsContainAdditionalWarnings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;

			var unlocoZ = declaration.Branch.HomePort;
			unlocoZ.RL_R3 = timeZoneSet.PK;

			var effectiveDate = unlocoZ.LocationDateTime;
			declaration.JE_RL_NKPortOfArrival = unlocoZ.RL_Code;
			Factory.Save();

			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			Assert("ContainsAdditionalWarnings false", manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			declaration.JE_DateOfArrival = effectiveDate.AddDays(1);
			Assert("ContainsAdditionalWarnings true", !manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			declaration.JE_DateOfArrival = effectiveDate;
			Assert("ContainsAdditionalWarnings false", manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			declaration.JE_DateOfFirstArrival = effectiveDate.AddDays(31);
			Assert("ContainsAdditionalWarnings true", !manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			declaration.JE_RL_NKPortOfArrival = "XXXXX";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			Assert("ContainsAdditionalWarnings true", !manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			declaration.JE_RL_NKPortOfArrival = unlocoZ.RL_Code;
			declaration.JE_DateOfFirstArrival = effectiveDate.AddHours(3);

			var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Tariff = "0200000000";
			line.InvoiceHeader.CA_RN_NKExport = "US";
			Assert("ContainsAdditionalWarnings true", !manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			declaration.JE_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			Assert("ContainsAdditionalWarnings true", !manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			Assert("AdditionalWarnings for Cancel", manager.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Withdraw).Contains("Electronic Cancel is only allowed for PARS entries that have not arrived"));
		}

		public void TestGetAdditionalWarningsMessageWithoutTimeZoneSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var unlocoZ = declaration.Branch.HomePort;
			var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;
			unlocoZ.RL_R3 = timeZoneSet.PK;

			var effectiveDate = unlocoZ.LocationDateTime;
			declaration.JE_RL_NKPortOfArrival = unlocoZ.RL_Code;
			declaration.JE_DateOfFirstArrival = effectiveDate;
			Factory.Save();

			var manager01 = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			Assert("ContainsAdditionalWarnings false", manager01.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);

			var refUNLOCO01 = Factory.New<RefUNLOCO>();
			refUNLOCO01.RL_R3 = ZGuid.Empty;
			refUNLOCO01.RL_Code = "CATE0";
			declaration.JE_RL_NKPortOfArrival = refUNLOCO01.RL_Code;
			Factory.Save();
			Assert("ContainsAdditionalWarnings true", !manager01.GetAdditionalWarningsMessage_Exposed(MessageSubTypes.Change).IsEmpty);
		}

		[TestDate(2016, 3, 14)]
		public void TestSetEntrySubmittedDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();

			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);
			AssertEquals("JE_EntrySubmittedDate", new ZDateTime(2016, 3, 14), declaration.JE_EntrySubmittedDate);

			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			manager.SendMessage(MessageSubTypes.Amend, false);
			AssertEquals("JE_EntrySubmittedDate", ZDateTime.Empty, declaration.JE_EntrySubmittedDate);
		}

		#region Implementation

		void AssertEDIReleaseMessageBuilder(IEDIReleaseOGD wrapper, Type expectedEDIReleaseMessageBuilderType)
		{
			var assertMessage = string.Format("Message Builder for: Assesment Option - '{0}' and Service Option - '{1}'", wrapper.AssessmentOption, wrapper.ServiceOptionID);
			var manager = new EDIReleaseImportMessageManagerForTesting(wrapper);
			AssertEquals(assertMessage, expectedEDIReleaseMessageBuilderType, manager.GetMessageBuilder_Exposed(MessageSubTypes.Create).GetType());
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new EDIReleaseImportMessageWrapper(entryHeader);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new EDIReleaseImportMessageManagerForTesting((IEDIReleaseOGD)dataWrapper);
		}

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			var manager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			ZString messageText;
			manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText);
			AssertEquals(expectedMessage, messageText);
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			if (!securityAllowed)
			{
				string securityWarning = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs -> Customs Declarations -> Reset to Original";
				resetDeclaration();
				AssertEquals(securityWarning, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				var ediMessageManager = (EDIReleaseImportMessageManagerForTesting)manager;
				ediMessageManager.Notification.NextAnswer = false;
				resetDeclaration();
				AssertEquals("MessageText", @"Resetting a declaration will result in Messages sent to Customs being discarded.
Are you certain you want to continue?", ediMessageManager.Notification.LastMessage);
				ediMessageManager.Notification.NextAnswer = true;
				resetDeclaration();
				var resetedDec = manager.BusinessObject as JobDeclaration;
				AssertEquals("EntryHeaders deleted", 0, resetedDec.ActiveEntryHeaders.Count);
			}
		}

		#endregion
	}
}
