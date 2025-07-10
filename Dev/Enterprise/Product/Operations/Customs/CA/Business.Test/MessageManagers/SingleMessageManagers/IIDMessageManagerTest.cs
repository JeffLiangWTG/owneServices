using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(IIDMessageManager))]
	class IIDMessageManagerTest : CAMessageManagerTestCase
	{
		public void TestSkipValidationWhenSendMessageOnJobsJustSaved()
		{
			Env.Security.CAACROSSMsgSend.IsAllowed = true;
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 15);

			var validationCount = declaration.RunPreSaveValidationCount;
			var dataWrapper = new IIDMessageWrapper(entryHeader);
			var manager = new IIDMessageManagerForTesting(dataWrapper, new TestMessageInstructionUserNotification());
			manager.ShouldJobBeSavedBeforeSendingMessage_Exposed = false;

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				manager.SendMessage(MessageSubTypes.Create);
			}
			AssertEquals("Validate Count changed", validationCount + 1, declaration.RunPreSaveValidationCount);

			Factory.Save();

			validationCount = declaration.RunPreSaveValidationCount;
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				manager.SendMessage(MessageSubTypes.Create);
			}
			AssertEquals("Validate Count not changed", validationCount, declaration.RunPreSaveValidationCount);
		}

		public void TestIsCreditCheckRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var dataWrapper = new IIDMessageWrapper(entryHeader);
			var manager = new IIDMessageManagerForTesting(dataWrapper, new TestUserNotification());

			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";
			Factory.Save();

			manager.PopulateMessage(MessageSubTypes.Create);
			entryHeader.Messages[0].EM_Status = MessageStatusList.Codes.Sent;
			entryHeader.Messages[0].EM_MessageType = UniversalEventMessageTypes.Codes.IIDResponses;
			entryHeader.Messages[0].EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			AssertEquals("Added message cleared", MessageStatusList.Codes.Sent, entryHeader.Messages[0].EM_Status);
			AssertEquals("IID", UniversalEventMessageTypes.Codes.IIDResponses, entryHeader.Messages[0].EM_MessageType);

			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.CUD, 75m);
			entryLine.CL_CommoditySequence = 1;
			entryLine.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.GST, 23m);
			Assert(entryLine.DutyFeeChangedSinceLastResponse);
			AssertEquals(true, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 75m);
			Factory.Save();
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 23m);
			Factory.Save();
			Assert(!entryLine.DutyFeeChangedSinceLastResponse);
			AssertEquals(false, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));

			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals(true, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));

			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_MessageText = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN'BGM+++9'LOC+41'RFF+TN'RFF+ARA'DOC+785'UNS+D'DMS+0'NAD+SE'DOC+935'LOC+27'PAT+1+CONSIGN'MOA+6'UNS+S'TAX+4+:::K90'MOA+176:7800'TAX+4+:::K92'MOA+176:7800'UNT+19+<<MSGNO PLACEHOLDER>>'";
			sentMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			sentMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);

			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchange.EI_Status = EDIInterchange.Status.Sent;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = GlbCompany.CurrentCompany.GC_Code;
			interchange.EI_To = "CAC";
			interchange.EI_BodyText = sentMessage.EM_MessageText;
			interchange.ContainedMessages.Add(sentMessage);
			entryHeader.Messages.Add(sentMessage);
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			AssertEquals(true, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));

			sentMessage.EM_MessageText = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN'BGM+++9'LOC+41'RFF+TN'RFF+ARA'DOC+785'UNS+D'DMS+0'NAD+SE'DOC+935'LOC+27'PAT+1+CONSIGN'MOA+6'UNS+S'TAX+4+:::K90'MOA+176:9800'TAX+4+:::K92'MOA+176:9800'UNT+19+<<MSGNO PLACEHOLDER>>'";
			Factory.Save();
			AssertEquals(false, manager.PreCheck4CreditOKToSendChecking_Exposed(declaration));
		}

		public void TestOnMessageQueuedForSending()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			Assert("CH_EntrySubmittedDate is not set", entryHeader.CH_EntrySubmittedDate.IsEmpty);
			Assert("JE_EntrySubmittedDate is not set", declaration.JE_EntrySubmittedDate.IsEmpty);

			manager.Call_OnMessageQueuedForSending(MessageSubTypes.Create);

			Assert("CH_EntrySubmittedDate is set to Now", entryHeader.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			Assert("JE_EntrySubmittedDate is set to scheduledTime", declaration.JE_EntrySubmittedDate == entryHeader.CH_EntrySubmittedDate);

			entryHeader.CH_EntryStatus = MessageStatusList.Codes.ErrorReplace;
			manager.Call_OnMessageQueuedForSending(MessageSubTypes.Change);
			AssertEquals("Entry Status reset empty after sending a message", string.Empty, entryHeader.CH_EntryStatus);
		}

		[TestDate(2017, 1, 3)]
		public void TestDefineActionCodeIfUndefined()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			AssertEquals("Action Code", MessageSubTypes.Create, manager.DefineActionCodeIfUndefined());
			var cusEntryNum = CusEntryNumber.New(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
			cusEntryNum.CE_EntryStatus = MessageProcessors.MessageValidationPassedMessageProcessor.IsOnFile;
			AssertEquals("Action Code", MessageSubTypes.Change, manager.DefineActionCodeIfUndefined());
		}

		public override void TestShouldSendMessagesInTestMode()
		{
			Assert(true);
		}

		[TestDate(2017, 1, 3)]
		public void TestGetAdditionalMessageErrors()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			dataWrapper.MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			Assert("IsWaitingForResponse", messageManager.IsWaitingForResponse);
			AssertEquals("GetAdditionalMessageErrors", "Entry is currently waiting for a response from CBSA or has a message already scheduled to send.\r\n\r\nYou are about to send an amendment message, please specify the amendment reason code under Misc Tab -> Amendment Reason.", manager.GetAdditionalMessageErrors(MessageSubTypes.Amend));
			AssertEquals("GetAdditionalMessageErrors", "Entry is currently waiting for a response from CBSA or has a message already scheduled to send.", manager.GetAdditionalMessageErrors(MessageSubTypes.Change));

			dataWrapper.MessageStatus = MessageStatusList.Codes.ErrorOriginal;
			AssertEquals("GetAdditionalMessageErrors", string.Empty, manager.GetAdditionalMessageErrors(MessageSubTypes.Change));
		}

		public override void TestPopulateMessages()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			manager.PopulateMessage(MessageSubTypes.Create);
			AssertEquals("1 message", 1, entryHeader.Messages.Count);
			AssertEquals("EM_MessageSubType", IIDMessageSubTypeList.Codes.Original, entryHeader.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		}

		public override void TestMessageFriendlyName()
		{
			declaration.JE_DeclarationReference = "B00001111";
			AssertEquals("MessageFriendlyName", "Message for Declaration B00001111", messageManager.MessageFriendlyName);
		}

		public void TestMessageGetActionCodeDescription()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			declaration.JE_DeclarationReference = "B00001111";
			Factory.Save();

			AssertEquals("ActionCodeDescription Add", "IID Original Message for Declaration B00001111 has been generated.", manager.LastNotification(MessageSubTypes.Create));
			AssertEquals("ActionCodeDescription Withdraw", "IID Withdraw Message for Declaration B00001111 has been generated.", manager.LastNotification(MessageSubTypes.Withdraw));
			AssertEquals("ActionCodeDescription Change", "IID Change Message for Declaration B00001111 has been generated.", manager.LastNotification(MessageSubTypes.Change));
			AssertEquals("ActionCodeDescription Amendment", "IID Amendment Message for Declaration B00001111 has been generated.", manager.LastNotification(MessageSubTypes.Amend));
		}

		public void TestCanSendThisMessageWithCancellation()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 15);
			AssertEquals("MessageText", "Job not yet saved, Please save before sending.", manager.CanSendThisMessage());
			Factory.Save();
			var msg1 = manager.PopulateMessage(MessageSubTypes.Withdraw);
			msg1[0].EM_Status = MessageStatusList.Codes.Sent;
			msg1[0].EM_MessageType = UniversalEventMessageTypes.Codes.IIDResponses;
			msg1[0].EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			msg1[0].EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-1);

			Assert("Cancel MessageText", !manager.CanSendThisMessage(MessageSubTypes.Withdraw).Contains("this has already been canceled."));

			SetUpForDisplayMessage();

			Factory.Save();

			Assert("Cancel MessageText", manager.CanSendThisMessage(MessageSubTypes.Withdraw).Contains("this has already been canceled."));
		}

		public void TestSendLastSuccessedMessage()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";
			Factory.Save();

			manager.PopulateMessage(MessageSubTypes.Create);
			entryHeader.Messages[0].EM_Status = MessageStatusList.Codes.Sent;
			entryHeader.Messages[0].EM_MessageType = UniversalEventMessageTypes.Codes.IIDResponses;
			entryHeader.Messages[0].EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			AssertEquals("Added message cleared", MessageStatusList.Codes.Sent, entryHeader.Messages[0].EM_Status);
			AssertEquals("IID", UniversalEventMessageTypes.Codes.IIDResponses, entryHeader.Messages[0].EM_MessageType);

			SetUpForDisplayMessage();

			var cancelMessage1 = manager.PopulateMessage(MessageSubTypes.Withdraw);
			AssertEquals("2 message", 2, entryHeader.Messages.Count);
			AssertEquals("EM_MessageSubType", IIDMessageSubTypeList.Codes.Cancellation, entryHeader.Messages[1].EM_MessageSubType);
			AssertEquals("EM_MessageText has a description ABC", true, entryHeader.Messages[1].EM_MessageText.Contains("<Description>ABC</Description>"));

			invoiceLine.JI_Description = "DEF";
			Factory.Save();

			var cancelMessage2 = manager.PopulateMessage(MessageSubTypes.Withdraw);
			AssertEquals("EM_MessageText does not have a description DEF which is changed", false, cancelMessage2[0].EM_MessageText.Contains("<Description>DEF</Description>"));
			AssertEquals("EM_MessageText has a description ABC", true, cancelMessage2[0].EM_MessageText.Contains("<Description>ABC</Description>"));
		}

		readonly string mmaMessageText = @"<s0:UniversalEvent><s0:Event><s0:DataContext><s0:DataTargetCollection><s0:DataTarget><s0:Type>CAIntegratedImportDeclaration</s0:Type><s0:Key>10207000019090</s0:Key></s0:DataTarget></s0:DataTargetCollection><s0:RecipientRoleCollection><s0:RecipientRole><s0:Code>CD4</s0:Code><s0:Description>CA Customs IID/D4 Status Notice</s0:Description></s0:RecipientRole></s0:RecipientRoleCollection></s0:DataContext><s0:EventTime>2018-03-19T11:00:00</s0:EventTime><s0:EventType>MAA</s0:EventType><s0:EventReference>IID Accepted</s0:EventReference><s0:ContextCollection><s0:Context><s0:Type>InterchangeNumber</s0:Type><s0:Value>10484</s0:Value></s0:Context><s0:Context><s0:Type>MessageNumber</s0:Type><s0:Value>1</s0:Value></s0:Context><s0:Context><s0:Type>IsTest</s0:Type><s0:Value>Y</s0:Value></s0:Context><s0:Context><s0:Type>OrganizationReference</s0:Type><s0:Value>B00172152</s0:Value></s0:Context></s0:ContextCollection></s0:Event></s0:UniversalEvent>";

		void SetUpForDisplayMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			var message2 = Factory.New<UniversalEventMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = mmaMessageText;
			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message2.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			var message3 = Factory.New<UniversalEventMessage>();
			message3.EM_MessageText = mmaMessageText;
			stmAlog = shipment.Logs.AddNew();
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message3.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			Factory.Save();
			var entry = (new BusinessObjectFactory()).Load<CusEntryHeader>(entryHeader.PK);
			var messages = entry.MessagesForDisplay;
			AssertEquals("Messages Count", 3, messages.Count);
			Assert("Contains Message", messages.Contains(message2));
			Assert("Contains Message", messages.Contains(message3));
		}

		public void TestNeedValidationForNotificationMessageInstruction()
		{
			var manager = messageManager as IIDMessageManagerForTesting;

			AssertEquals(true, manager.NeedValidationForNotificationMessageInstruction(MessageSubTypes.Create));
			AssertEquals(false, manager.NeedValidationForNotificationMessageInstruction(MessageSubTypes.Withdraw));
		}

		public override void TestGetMessageBuilder()
		{
			var manager = messageManager as IIDMessageManagerForTesting;
			AssertEquals("GetMessageBuilder",
				typeof(IIDMessageBuilder),
				manager.GetMessageBuilder(MessageSubTypes.Create).GetType());
		}

		public override void TestCanSendThisMessage()
		{
			using (ZArchitecture.Environment.Globals.SetIsWinzorForTest(true))
			using (ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(true))
			{
				var manager = messageManager as IIDMessageManagerForTesting;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 7, 15);
				AssertEquals("MessageText", "Job not yet saved, Please save before sending.", manager.CanSendThisMessage());

				Factory.Save();
				Env.Security.CAACROSSMsgSend.IsAllowed = false;
				Assert("MessageText", manager.CanSendThisMessage().Contains("You do not have the appropriate security rights to run this function."));

				Env.Security.CAACROSSMsgSend.IsAllowed = true;
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
				AssertEquals("MessageText", $@"The Network Client ID is not configured,
in the registry for Company - {GlbCompany.CurrentCompany.GC_Code}, Branch - {GlbBranch.CurrentBranch.GB_Code}. Please contact your System Administrator.", manager.CanSendThisMessage());

				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				Assert("CanSendThisMessage", manager.CanSendThisMessage().IsEmpty);

				declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Cancelled;
				declaration.JE_OH_Importer = importer.PK;
				declaration.Importer.CompanyData.OB_AROnCreditHold = true;
				declaration.Importer.CompanyData.OB_IsDebtor = true;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				declaration.JE_EntryStatus = "MAN";
				Factory.Save();

				AssertEquals("Submit message with credit restriction canceled.", manager.CanSendThisMessage());
			}
		}

		public void TestAdditionalWarningsMessage_MQWarningMessage()
		{
			var mQwarningText = CAMessageManager.MQWarningMessage;
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPP");
				var messageWrapper = new IIDMessageWrapper(entryHeader);
				var manager = new IIDMessageManagerForTesting(messageWrapper, new TestUserNotification());
				AssertContains("AdditionalWarnings", mQwarningText, manager.GetAdditionalWarningsMessage(MessageSubTypes.Request));

				CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW");
				messageWrapper = new IIDMessageWrapper(entryHeader);
				manager = new IIDMessageManagerForTesting(messageWrapper, new TestUserNotification());
				AssertNotContains("AdditionalWarnings", mQwarningText, manager.GetAdditionalWarningsMessage(MessageSubTypes.Request));
			}

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var messageWrapper = new IIDMessageWrapper(entryHeader);
				var manager = new IIDMessageManagerForTesting(messageWrapper, new TestUserNotification());
				AssertNotContains("AdditionalWarnings", mQwarningText, manager.GetAdditionalWarningsMessage(MessageSubTypes.Request));
			}
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			return new IIDMessageWrapper(entryHeader);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new IIDMessageManagerForTesting((IIDMessageWrapper)dataWrapper, new TestUserNotification());
		}

		public override void SetTestMode(bool testMode)
		{
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			base.SetUp();
		}

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			var messageWrapper = new IIDMessageWrapper(entryHeader);
			var manager = new IIDMessageManagerForTesting(messageWrapper, new TestUserNotification());
			AssertEquals(expectedMessage, manager.CanSendThisMessage());
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}

		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
	}
}
