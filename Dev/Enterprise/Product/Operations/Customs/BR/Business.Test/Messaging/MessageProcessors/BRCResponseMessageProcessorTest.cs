using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCResponseMessageProcessorForTesting>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => Array.Empty<string>();

		protected override IReadOnlyList<string> MessageFilterSubTypes => Array.Empty<string>();

		public void TestProcessMessage_Succeed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, responseMessage.EM_HeldUntilDate);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("Logger", "Information: \tProcessing Message 1\r\n", logger.LogMessages.ToString());
				AssertMultilineASCIIEquals("Logger", "\tProcessing Message 1", responseMessage.Notes.FindByDescription("Processing Log").FirstOrDefault().ST_NoteText);
			});
		}

		public void TestProcessMessage_CannotFindLinkedObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var requestInterchange = CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Sent, MessageTypeList.Codes.CDE);
			var requestMessage = CreateMessage(Factory, requestInterchange.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			var responseInterchange = CreateInterchange(Factory, requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Received, MessageTypeList.Codes.CDE);
			var responseMessage = CreateMessage(Factory, responseInterchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertNotEquals("EM_LinkUniqueID", entry.PK, requestMessage.EM_LinkUniqueID);
			AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to locate the related Business Object for CDE message #1\r\n", logger.LogMessages.ToString());
			AssertMultilineASCIIEquals("Logger", "\tUnable to locate the related Business Object for CDE message #1", responseMessage.Notes.FindByDescription("Processing Log").FirstOrDefault().ST_NoteText);
		}

		public void TestProcessMessage_CannotFindOutgoingInterchange()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();

			var responseInterchange = CreateInterchange(Factory, ZGuid.Empty, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Received, MessageTypeList.Codes.CDE);
			var responseMessage = CreateMessage(Factory, responseInterchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertContains("Logger", "Error: \tUnable to locate the related outgoing message for CDE message #1\r\n", logger.LogMessages.ToString());
			AssertMultilineASCIIEquals("Logger", "\tUnable to locate the related outgoing message for CDE message #1\r\n\tUnable to locate the related Business Object for CDE message #1\r\n", responseMessage.Notes.FindByDescription("Processing Log").FirstOrDefault().ST_NoteText);
		}

		public static (BREDIMessage RequestMessage, BREDIMessage ResponseMessage) CreateResponseMessage(BusinessObject businessObject, string responseMessageType, string responseMessageSubType, string requestMessageType = null, string requestMessageSubType = null, string interchangeBodyText = null)
		{
			var requestInterchange = CreateInterchange(businessObject.Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Sent, requestMessageType ?? responseMessageType, interchangeBodyText);
			var requestMessage = CreateMessage(businessObject.Factory, requestInterchange.PK, businessObject, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, requestMessageType ?? responseMessageType, requestMessageSubType ?? EDIMessageSubTypeList.Codes.Original);
			var responseInterchange = CreateInterchange(businessObject.Factory, requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Received, responseMessageType);
			var responseMessage = CreateMessage(businessObject.Factory, responseInterchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, responseMessageType, responseMessageSubType);

			return (requestMessage, responseMessage);
		}

		public static (IEnumerable<BREDIMessage> RequestMessages, BREDIMessage ResponseMessage) CreateMultipleMessagesAndInterchange(BusinessObject[] businessObjects, string responseMessageType, string responseMessageSubType, string requestMessageType = null, string requestMessageSubType = null)
		{
			var requestInterchange = CreateInterchange(businessObjects[0].Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Sent, requestMessageType ?? responseMessageType);
			var requestMessages = new List<BREDIMessage>();
			foreach (var businessObject in businessObjects)
			{
				requestMessages.Add(CreateMessage(businessObjects[0].Factory, requestInterchange.PK, businessObject, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, requestMessageType ?? responseMessageType, requestMessageSubType ?? EDIMessageSubTypeList.Codes.Original));
			}
			var responseInterchange = CreateInterchange(businessObjects[0].Factory, requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Received, responseMessageType);
			var responseMessage = CreateMessage(businessObjects[0].Factory, responseInterchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, responseMessageType, responseMessageSubType);
			return (requestMessages, responseMessage);
		}

		internal static EDIInterchange CreateInterchange(BusinessObjectFactory factory, ZGuid sessionGUID, ZString direction, ZString status, ZString type, ZString bodyText = default)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.BRCustoms;
			interchange.EI_InterchangeType = type;
			interchange.EI_From = direction == EDIInterchange.Direction.Transmit ? GlbCompany.CurrentCompany.LicenceKeyIdentifier : new ZString("BRCustoms");
			interchange.EI_To = direction == EDIInterchange.Direction.Receive ? GlbCompany.CurrentCompany.LicenceKeyIdentifier : new ZString("BRCustoms");
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;
			if (!bodyText.IsEmpty)
			{
				interchange.EI_BodyText = bodyText;
			}
			return interchange;
		}

		internal static BREDIMessage CreateMessage(BusinessObjectFactory factory, ZGuid interchangePK, BusinessObject businessObject, ZString direction, ZString status, ZString type, ZString subType)
		{
			var message = factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_ApplicationReference = "APRN0000001";
			message.EM_MessageNum = "1";
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_EI = interchangePK;
			message.EM_LinkedObject = businessObject;
			return message;
		}
	}

	class BRCResponseMessageProcessorForTesting : BRCResponseMessageProcessor
	{
		public BRCResponseMessageProcessorForTesting(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "BRC Response Message Processor For Testing";

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject != null)
			{
				Logger.Log($"Processing Message {message.EM_MessageNum}");
			}
		}
	}

	public abstract class BRCResponseMessageProcessorAbstractTest<T> : TestCaseWithFactory where T : BRCResponseMessageProcessor
	{
		protected LoggingInformationForTesting ExecuteMessageProcessor(EDIMessage message)
		{
			var logger = new LoggingInformationForTesting();
			var processor = (T)Activator.CreateInstance(typeof(T), logger);
			processor.PreProcessMessage(message);
			if (message.EM_Status != EDIMessage.Status.Failed)
			{
				processor.ProcessMessage(message);
			}
			return logger;
		}

		public void TestMessageFilter() => CombineAssertions(() =>
		{
			var logger = new LoggingInformationForTesting();
			var processor = (T)Activator.CreateInstance(typeof(T), logger);

			var messageTypesCondition = MessageFilterTypes.Count == 0 ? ""
				: MessageFilterTypes.Count == 1 ? $" and EM_MessageType = '{MessageFilterTypes.Single()}'"
				: $" and (EM_MessageType in ('{string.Join("', '", MessageFilterTypes)}'))";

			var messageSubTypesCondition = MessageFilterSubTypes.Count == 0 ? ""
				: MessageFilterSubTypes.Count == 1 ? $" and EM_MessageSubType = '{MessageFilterSubTypes.Single()}'"
				: $" and (EM_MessageSubType in ('{string.Join("', '", MessageFilterSubTypes)}'))";
			AssertEquals("MessageFilter", $"EM_ApplicationCode = 'BRC'{messageTypesCondition}{messageSubTypesCondition}", processor.MessageFilter.LiteralTextADO);
		});

		protected virtual IReadOnlyList<string> MessageFilterTypes => Array.Empty<string>();

		protected virtual IReadOnlyList<string> MessageFilterSubTypes => Array.Empty<string>();
	}
}
