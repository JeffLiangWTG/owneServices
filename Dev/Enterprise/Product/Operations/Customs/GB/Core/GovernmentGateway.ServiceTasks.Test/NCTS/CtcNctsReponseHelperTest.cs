using System.Data;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq.Protected;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	public class CtcNctsResponseHelperTest
	{
		public string MessageApplicationCode { get; set; } = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
		public string ResourceNamePrefix { get; set; } = "Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Testing.NCTS.Processors.CTC.TestFiles.";

		public EDIMessage PopulateMessageForTest(BusinessObjectFactory factory,
									   NctsHeader nctsMovement,
									   EDIMessage message,
									   bool linkToHeader = true,
									   string messageNum = null,
									   bool requeueMessage = false)
		{
			messageNum = messageNum ?? _messageNum++.ToString();
			message.EM_ApplicationCode = MessageApplicationCode;
			if (message.EM_MessageType.IsEmpty)
			{
				message.EM_MessageType = "GB";
			}
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageNum = messageNum;
			message.EM_SendWithMessageErrors = false;

			if (linkToHeader)
			{
				nctsMovement?.LinkedMessages.Add(message);
			}

			if (requeueMessage)
			{
				message.EM_MessageType = "";
				message.EM_MessageSubType = "";
				message.EM_Status = EDIMessage.Status.Queued;
				message.EM_LinkedObject = null;
				message.EM_LinkTable = "";
			}
			factory.Save();
			return message;
		}

		public EDIInterchange MakeOutgoingInterchangeForTest(BusinessObjectFactory factory, EDIMessage message, string interchangeNum = null)
		{
			interchangeNum = interchangeNum ?? _interchangeNum++.ToString();
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = MessageApplicationCode;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = "NCT";
			interchange.EI_To = "NCTS";
			interchange.EI_InterchangeNum = interchangeNum;
			interchange.ContainedMessages.Add(message);
			return interchange;
		}

		public EDIInterchange MakeIncomingInterchangeForTest(BusinessObjectFactory factory, EDIMessage message, string interchangeNum = null)
		{
			interchangeNum = interchangeNum ?? _interchangeNum++.ToString();
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = MessageApplicationCode;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "NCTS";
			interchange.EI_To = "NCT";
			interchange.EI_InterchangeNum = interchangeNum;
			interchange.ContainedMessages.Add(message);
			return interchange;
		}

		public NctsHeader SetupMessagesForTest(BusinessObjectFactory factory,
												ZString movementType,
												string messageToTest,
												ZString transitStatusForTest,
												string messageStatusForTest = "",
												string incomingMessageSubType = "",
												string outgoingMessageSubType = "",
												string jobNo = null,
												NctsHeader header = null,
												string applicationReference = "")
		{
			header = CreateDefaultHeaderForTest(factory, movementType, transitStatusForTest, messageStatusForTest, jobNo, header);
			var outgoingMessage = CreateDefaultOutgoingMessage(factory, header.BH_JobReference, messageToTest, ZDateTime.UtcNow.AddMonths(-2), outgoingMessageSubType, applicationReference);
			var incomingMessage = CreateDefaultIncomingMessage(factory, header, messageToTest, ZDateTime.UtcNow.AddMonths(-2), incomingMessageSubType, applicationReference);
			SetupMessagesForTest(factory, outgoingMessage, incomingMessage, header);
			return header;
		}

		public NctsHeader SetupPhase5MessagesForTest(BusinessObjectFactory factory,
												ZString movementType,
												string messageToTest,
												ZString transitStatusForTest,
												string messageStatusForTest = "",
												string incomingMessageSubType = "",
												string outgoingMessageSubType = "",
												string jobNo = null,
												NctsHeader header = null,
												string applicationReference = "")
		{
			header = CreateDefaultHeaderForTest(factory, movementType, transitStatusForTest, messageStatusForTest, jobNo, header);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var outgoingMessage = CreateDefaultOutgoingMessage(factory, header.BH_JobReference, messageToTest, ZDateTime.UtcNow.AddMonths(-2), outgoingMessageSubType, applicationReference);
			var incomingMessage = CreateDefaultIncomingMessage(factory, header, messageToTest, ZDateTime.UtcNow.AddMonths(-2), incomingMessageSubType, applicationReference);
			SetupMessagesForTest(factory, outgoingMessage, incomingMessage, header);
			return header;
		}

		public void SetupMessagesForTest(BusinessObjectFactory factory,
										 EDIMessage outgoingMessage,
										 EDIMessage incomingMessage,
										 NctsHeader header = null)
		{
			PopulateMessageForTest(factory, header, outgoingMessage);
			MakeOutgoingInterchangeForTest(factory, outgoingMessage);
			PopulateMessageForTest(factory, header, incomingMessage);
			factory.Save();
		}

		public NctsHeader CreateDefaultHeaderForTest(BusinessObjectFactory factory,
										 ZString movementType,
										 ZString transitStatusForTest,
										 string messageStatusForTest = "",
										 string jobNo = null,
										 NctsHeader header = null)
		{
			if (header == null)
			{
				header = factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(movementType);
			}
			header.BH_JobReference = jobNo ?? "NCT00050167";
			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			mrn.CE_EntryNum = header.BH_JobReference;
			if (movementType == NctsMovementType.Codes.Arrival)
			{
				header.ArrivalMovementHeader.BM_CustomsStatus = transitStatusForTest;
			}
			else
			{
				header.MovementHeader.BM_CustomsStatus = transitStatusForTest; // Set up fake status
			}
			header.EffectiveMessageStatus = messageStatusForTest; // Set up fake message status
			return header;
		}

		public EDIMessage CreateDefaultIncomingMessage(BusinessObjectFactory factory,
										 NctsHeader header,
										 string messageToTest,
										 ZDateTime createTime,
										 string incomingMessageSubType = "",
										 string incomingApplicationReference = "")
		{
			var mockIncomingMessage = factory.NewMoq<EDIMessageDummyForTest_111>();
			mockIncomingMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("111");
			var incomingMessage = mockIncomingMessage.Object;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageToTest.Replace("BH_JOBREFERENCE", header.BH_JobReference.ToString());
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = header.BH_JobReference.ToString();
			incomingMessage.EM_SystemCreateTimeUtc = createTime;
			incomingMessage.EM_MessageSubType = incomingMessageSubType;
			incomingMessage.EM_ApplicationReference = incomingApplicationReference;
			incomingMessage.EM_MessageType = "GB";
			return incomingMessage;
		}

		public EDIMessage CreateDefaultOutgoingMessage(BusinessObjectFactory factory,
										 string headerJobReference,
										 string messageToTest,
										 ZDateTime createTime,
										 string outgoingMessageSubType = "",
										 string outgoingApplicationReference = "")
		{
			var mockOutgoingMessage = factory.NewMoq<EDIMessageDummyForTest_111>();
			mockOutgoingMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("111");
			var outgoingMessage = mockOutgoingMessage.Object;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText = messageToTest.Replace("BH_JOBREFERENCE", headerJobReference);
			outgoingMessage.EM_Status = EDIMessage.Status.Received;
			outgoingMessage.EM_MessageNum = headerJobReference;
			outgoingMessage.EM_SystemCreateTimeUtc = createTime;
			outgoingMessage.EM_MessageType = outgoingMessageSubType;
			outgoingMessage.EM_ApplicationReference = outgoingApplicationReference;
			return outgoingMessage;
		}

		int _messageNum = 12;
		int _interchangeNum = 21;

		public class EDIMessageDummyForTest_111 : EDIMessage
		{
			public EDIMessageDummyForTest_111(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			protected override string GetMessageReferenceNumber()
			{
				return "111";
			}
		}

		public ZString GetEmbeddedResourceFile(ZString embeddedResourceFile)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceNamePrefix + embeddedResourceFile))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}
	}
}
