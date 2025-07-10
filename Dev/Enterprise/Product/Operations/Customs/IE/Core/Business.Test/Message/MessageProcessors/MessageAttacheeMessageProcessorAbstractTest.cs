using System;
using System.IO;
using System.Reflection;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestsSubclassesOf(typeof(MessageAttacheeMessageProcessor<,>))]
	public abstract class MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, TMessageAttachee, TRelatedJob> : MessageProcessorAbstractTest<TMessageProcessor, TInboundEDIMessage, TDataProvider>
		where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
		where TInboundEDIMessage : InboundEDIMessage
		where TOutboundEDIMessage : OutboundEDIMessage
		where TMessageAttachee : BusinessObject, IMessageAttachee
		where TRelatedJob : BusinessObject, IRelatedJob
	{
		public void TestEndToEndProcessing()
		{
			var (relatedJob, messageAttachee, outgoingMessage, incomingMessage) = CreateSetupData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var responseDetail = ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType, incomingMessage.EM_MessageText);
				var processorType = responseDetail.ProcessorType;
				var processor = (TMessageProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				processor.PreProcessMessage(incomingMessage);
				CombineAssertions("PreProcess", () =>
				{
					AssertEquals("incomingMessage.EM_GB", messageAttachee.Branch.PK, incomingMessage.EM_GB);
					AssertEquals("incomingMessage.EM_LinkUniqueID", messageAttachee.PK, incomingMessage.EM_LinkUniqueID);
					AssertEquals("incomingMessage.EM_LinkTable", messageAttachee.TableName, incomingMessage.EM_LinkTable);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				});
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertProcessResult(messageAttachee, incomingMessage);
				});
			}
		}

		public virtual void TestEnsureMessageTextIsValid()
		{
			var xmlObjectTypeProperty = typeof(MessageProcessor<TInboundEDIMessage, TDataProvider>).GetField("xmlObjectType", BindingFlags.NonPublic | BindingFlags.Instance);
			var xmlObjectType = (Type)xmlObjectTypeProperty.GetValue(Processor);
			var deserializeMethod = typeof(IEXmlObjectSerializer).GetMethod(nameof(IEXmlObjectSerializer.Deserialize), BindingFlags.Static | BindingFlags.Public);
			var deserializeMethodGeneric = deserializeMethod.MakeGenericMethod(xmlObjectType);

			using (var reader = new StringReader(MessageText))
			{
				var xmlObject = deserializeMethodGeneric.Invoke(null, new object[] { reader, true });
				AssertNotNull(xmlObject);
			}
		}

		protected void AssertMessageInterpretation(TInboundEDIMessage incomingMessage, ZString expectedInterpretation)
		{
			AssertXMLEquals(
				"EM_MessageInterpretation",
				expectedInterpretation.RemoveLineBreakingsAndIndents(),
				incomingMessage.EM_MessageInterpretation.RemoveLineBreakingsAndIndents()
			);
		}

		protected void AssertProcessResult(TMessageAttachee messageAttachee, TInboundEDIMessage incomingMessage)
		{
			AssertEquals("Message should have been set PRS.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertProcessResultCore(messageAttachee, incomingMessage);
		}

		protected virtual void AssertProcessResultCore(TMessageAttachee messageAttachee, TInboundEDIMessage incomingMessage) { }

		protected virtual (TRelatedJob declaration, TMessageAttachee messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData() => CreateSetupData(null);

		protected abstract (TRelatedJob declaration, TMessageAttachee messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null);

		protected GlbStaff Staff => staff ?? (staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory));
		GlbStaff staff;

		protected virtual TInboundEDIMessage CreateNewIncomingMessage(string incomingMessageText = null)
		{
			var message = Factory.New<TInboundEDIMessage>();
			message.EM_ApplicationReference = TransactionID;
			message.EM_MessageType = MessageType;
			message.EM_MessageText = incomingMessageText ?? InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, MessageText, includeResponseWrap: false);
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			return message;
		}
		protected const string TransactionID = "5B625BFB-BF2A-491C-8C23-BDBB2DECA438";

		protected abstract ZString MessageType { get; }
		protected abstract ZString MessageText { get; }

		protected ZString Serialize<T>(T data)
		{
			return IEXmlObjectSerializer.Serialize(data);
		}
	}
}
