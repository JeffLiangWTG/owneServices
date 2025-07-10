using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EDIMessage = Enterprise.Customs.JP.Common.EDIMessage;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MessageSender))]
	sealed class MessageSenderTest : Customs.Business.Testing.MessageSenderTest
	{
		[TestDate(2015, 1, 1, 01, 15, 08)]
		public void TestSendIDAMessage()
		{
			CombineAssertions(() =>
			{
				TestCanSendMessage(JPProcedureCodeList.Codes.IDA);
			});
		}

		[TestDate(2015, 1, 1, 01, 15, 08)]
		public void TestSendIDCMessage()
		{
			CombineAssertions(() =>
			{
				TestCanSendMessage(JPProcedureCodeList.Codes.IDC);
			});
		}

		[TestDate(2015, 1, 1, 01, 15, 08)]
		public void TestSendEDAMessage()
		{
			CombineAssertions(() =>
			{
				TestCanSendMessage(JPProcedureCodeList.Codes.EDA);
			});
		}

		[TestDate(2015, 1, 1, 01, 15, 08)]
		public void TestSendMSXMessage()
		{
			CombineAssertions(() =>
			{
				TestCanSendMessage(JPProcedureCodeList.Codes.MSX);
				TestMSXMessageCreateMessageAttach();
			});
		}

		public void TestSendMessageAndLog()
		{
			AssertLogEvent("CCC", JobMessageTypeList.Codes.Import, JPProcedureCodeList.Codes.IDA);
			AssertLogEvent("ECM", JobMessageTypeList.Codes.Export, JPProcedureCodeList.Codes.EDA);
			AssertLogEvent("ISN", JobMessageTypeList.Codes.Import, "   IDA                                                                                                                                                                                                                     IDA00345678901200000000003        3456789012                                                                                                                                 002701");
			AssertLogEvent("ISN", JobMessageTypeList.Codes.Export, "   EDA                                                                                                                                                                                                                     EDA00345678901200000000004        3456789012                                                                                                                                 002709");

			void AssertLogEvent(string eventType, string declarationMessageType, string reference)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = declarationMessageType;
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.MessageInitiator = messageInitiator;

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "3456789012";
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var sender = new MessageSender(new MessageSendingObject(entryHeader) { ProcedureCode = declarationMessageType == JobMessageTypeList.Codes.Import ? JPProcedureCodeList.Codes.IDA : JPProcedureCodeList.Codes.EDA });
				sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
				{ Factory.Save(); });
				declaration.Invoices.AddNew();
				Factory.Save();
				sender.SendMessageAndLog();

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType);
				query.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
				AssertEquals("A new log should have been added", 1, Factory.Load<StmALog>(query).Length);
				AssertEquals("Reference should be the same as we set in message", reference, Factory.Load<StmALog>(query).FirstOrDefault().SL_Reference);
			}
		}

		public void TestSendWithError()
		{
			var declaration = Factory.New<DeclarationForTestSendingObject>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var sender = new MessageSender(new MessageSendingObject(entryHeader) { ProcedureCode = JPProcedureCodeList.Codes.EDA });
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });
			declaration.Invoices.AddNew();
			Factory.Save();

			sender.SendMessage();
			var message = (EDIMessage)entryHeader.Messages.Single();
			AssertEquals(false, message.EM_SendWithMessageErrors);

			declaration.CreateMessageErrorForTest = true;
			declaration.Validation.ValidateAll();
			sender.SendMessage();
			message = (EDIMessage)entryHeader.Messages.LastOrDefault();
			AssertEquals(true, message.EM_SendWithMessageErrors);

			declaration.CreateMessageErrorForTest = false;
			declaration.Validation.ValidateAll();
			sender.SendMessage();
			message = (EDIMessage)entryHeader.Messages.LastOrDefault();
			AssertEquals(false, message.EM_SendWithMessageErrors);
		}

		void TestMSXMessageCreateMessageAttach()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Test.pdf", "CIV");
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.Invoices.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var msxMessageSendingObject = new MSXMessageSendingObject(entryHeader);
			var sender = new MessageSender(msxMessageSendingObject);
			var attachment = msxMessageSendingObject.Attachments.AddNew();
			attachment.File = eDoc.UniqueKey;
			attachment.Type = "IS";
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate { Factory.Save(); });
			AssertEquals(0, entryHeader.Messages.Count);

			sender.SendMessage();
			AssertEquals(1, entryHeader.Messages.Count);

			var message = (EDIMessage)entryHeader.Messages.Single();
			var messageAttach = message.MessageAttachments.FirstOrDefault() as Messaging.Business.EDIMessageAttach;
			AssertNotNull("MessageAttach is generated", messageAttach);
			AssertEquals("Storage Doc Guid", eDoc.UniqueKey, messageAttach.EG_StorageDocsGuid);
			AssertEquals("Filename", "Test.pdf", messageAttach.EG_FileName);
			AssertEquals("Document Type", "IS", messageAttach.EG_EdiMsgDocType);
			AssertEquals("Foreign key", message.PK, messageAttach.EG_EM);
		}

		void TestCanSendMessage(string procedureCode)
		{
			new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaffAndReturnPK();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = "AN";

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "3456789012";
			var sender = procedureCode switch
			{
				JPProcedureCodeList.Codes.MSX => new MessageSender(new MSXMessageSendingObject(entryHeader)),
				_ => new MessageSender(new MessageSendingObject(entryHeader) { ProcedureCode = procedureCode }),
			};
			sender.OnSave += Factory.Save;
			AssertEquals(0, entryHeader.Messages.Count);

			sender.SendMessage();
			AssertEquals(0, entryHeader.Messages.Count);
			AssertEquals("Could not find the Invoice for this messaging operation.", messageInitiator.InvalidOperationText);
			messageInitiator.InvalidOperationText = null;

			declaration.Invoices.AddNew();
			Factory.Save();
			sender.SendMessage();
			AssertEquals(1, entryHeader.Messages.Count);

			var message = (EDIMessage)entryHeader.Messages.Single();
			var interchange = Factory.Load<EDIInterchange>(message.EM_EI);

			var messageNum = $"{procedureCode.PadRight(5, '0')}345678901200000000001";

			AssertEquals(nameof(message.EM_MessageType), procedureCode, message.EM_MessageType);
			AssertNullOrEmpty(nameof(EDIMessage.EM_MessageSubType), message.EM_MessageSubType);
			AssertContains("Procedure Code", procedureCode, message.EM_FormattedMessageText);
			AssertEquals(nameof(message.EM_MessageNum), messageNum, message.EM_MessageNum);
			AssertContains("Message Reference", messageNum, message.EM_MessageInterpretation);
			AssertContains("Input Reference", "3456789012", message.EM_MessageInterpretation);
			AssertContains("Message length", message.EM_MessageData.Length.ToString(), message.EM_FormattedMessageText);

			AssertNotNull(interchange);

			var regKey = ObjectFactory.Get<IProductRegistration>().Key;
			AssertEquals($"{regKey.EnterpriseCode}{regKey.ServerCode}_JPC", interchange.EI_To);

			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum);
			AssertEquals(message.EM_ApplicationCode, interchange.EI_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals(EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			AssertEquals("Message data is copied to interchange", message.EM_MessageData.Length, interchange.EI_BodyData.Length);

			AssertEquals("Message status is set", JPMessageStatusList.Codes.Sending, entryHeader.CH_Status);

			var phaseList = new JPProcedureCodeList.PhaseList();
			if (phaseList.ContainsCode(procedureCode))
			{
				AssertEquals("Phase is set", procedureCode, entryHeader.CH_PhaseStatus);
			}
			else
			{
				AssertEquals("Phase is unchanged", string.Empty, entryHeader.CH_PhaseStatus);
			}
		}

		public void TestSendMessageFromVisualObject()
		{
			var declaration = Factory.New<DeclarationForTestSendingObject>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "3456789012";

			declaration.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.EDC });

			var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			sendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault().ShouldSend = true;

			var sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.ProcedureCode = JPProcedureCodeList.Codes.EDC;

			var contentProvider = new MessageContentProvider(Factory, sendingObject);

			var visualObject = new MessageVisualObject(contentProvider);
			var lastMessageDataProperty = visualObject.GetType().GetProperty("LastMessageData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			lastMessageDataProperty.SetValue(visualObject, new byte[] { 20 });

			var sender = new MessageSender(visualObject);
			var message = sender.ManualExportMessage();

			AssertArrayEqualsByElements("Should export the message content from the visual object.", new byte[] { 20 }, message.EM_MessageData);
		}

		public void TestSendingWithOverriddenValuesLog()
		{
			var declaration = Factory.New<DeclarationForTestSendingObject>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			declaration.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.EDC });

			var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			sendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault().ShouldSend = true;

			var sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.ProcedureCode = JPProcedureCodeList.Codes.EDA;

			var contentProvider = new MessageContentProvider(Factory, sendingObject);

			var visualObject = new MessageVisualObject(contentProvider);
			visualObject.Initialize();
			var sender = new MessageSender(visualObject);
			sender.SendMessageAndLog();

			var sendingWithOverriddenValuesLogs = declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.Authorised.Code && log.SL_Reference.StartsWith("Sending with overridden values|EDIMessage Number="));
			AssertEquals(0, sendingWithOverriddenValuesLogs.Count());

			visualObject.Header.Cast<EditableFieldBizObject>().FirstOrDefault().OverrideValue = "X";
			sender.SendMessageAndLog();

			sendingWithOverriddenValuesLogs = declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.Authorised.Code && log.SL_Reference.StartsWith("Sending with overridden values|EDIMessage Number="));
			AssertEquals(1, sendingWithOverriddenValuesLogs.Count());
		}
	}
}
