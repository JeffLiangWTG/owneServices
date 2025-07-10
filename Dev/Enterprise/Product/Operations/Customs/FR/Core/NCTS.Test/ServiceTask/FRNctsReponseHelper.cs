using System.Data;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq.Protected;
using FRBusiness = Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class FRNctsReponseHelper
	{
		int _messageNum = 12;
		int _interchangeNum = 21;

		public void AddLogEntryForTest(FRBusiness.NctsHeader nctsHeader, ZString status)
		{
			var logEntry = nctsHeader.Logs.AddNew();
			using (logEntry.LockForUpdatingKeyFieldsForTesting())
			{
				logEntry.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntry.SL_Reference = status;
			}
		}

		public EDIMessage MakeMessageForTest(BusinessObjectFactory factory, FRBusiness.NctsHeader nctsMovement, string receiveOrTransmit, string messageText, string status, string syscar, bool linkToHeader = true, string messageNum = null, bool requeueMessage = false)
		{
			messageNum = messageNum ?? _messageNum++.ToString();
			var mockMessage = factory.NewMoq<EDIMessageDummyForTest_111>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("111");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EuNcts;
			message.EM_MessageType = "FR";
			message.EM_MessageSubType = nctsMovement.MovementHeader.BM_CustomsStatus;
			message.EM_ReceiveTransmit = receiveOrTransmit;
			message.EM_MessageText = messageText.Replace("BH_JOBREFERENCE", syscar);
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageNum = messageNum;
			message.EM_SendWithMessageErrors = false;
			if (linkToHeader)
			{
				var nctsHeader = nctsMovement as NctsHeader;
				if (nctsHeader != null)
				{
					nctsHeader.Messages.Add(message);
				}
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
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.EuNcts;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = "NCT";
			interchange.EI_To = "NCTS";
			interchange.EI_InterchangeNum = interchangeNum;
			interchange.ContainedMessages.Add(message);
			return interchange;
		}

		public NctsHeader SetupMessagesForTest(BusinessObjectFactory factory, ZString movementType, string messageToTest, ZString transitStatusForTest, bool createOutgoingMessage, string messageStatusForTest = "", string jobNo = null, NctsHeader nctsHeader = null)
		{
			NctsHeader nctsMovement;

			if (nctsHeader != null)
			{
				nctsMovement = nctsHeader;
			}
			else
			{
				nctsMovement = factory.New<NctsHeader>();
				nctsMovement.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsMovement.SetMovementType(movementType);
			}
			nctsMovement.BH_JobReference = jobNo ?? "NCT00050167";

			if (createOutgoingMessage)
			{
				var outgoingMessage = MakeMessageForTest(factory, nctsMovement, EDIMessage.Direction.Transmit, messageToTest, EDIMessage.Status.Received, nctsMovement.BH_JobReference.ToString());
				nctsMovement.MovementHeader.BM_CustomsStatus = transitStatusForTest;
				nctsMovement.EffectiveMessageStatus = messageStatusForTest;

				MakeOutgoingInterchangeForTest(factory, outgoingMessage);
			}

			MakeMessageForTest(factory, nctsMovement, EDIMessage.Direction.Receive, messageToTest, EDIMessage.Status.Queued, nctsMovement.BH_JobReference.ToString());
			AddLogEntryForTest(nctsMovement, transitStatusForTest);

			factory.Save();
			return nctsMovement;
		}

		public ZString GetEmbeddedResourceFile(ZString embeddedResourceFile)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.FR.NCTS.Testing.ServiceTask.Processors.TestFiles." + embeddedResourceFile))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}
	}

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
}
