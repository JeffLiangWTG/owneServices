using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public class IcsNorthernIrelandResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessageNI()
		{
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN12345";
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.ICS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			manifestHeader.AMA_GB = aaaBranch.PK;

			var outgoingMessage = Factory.New<IcsNorthernIrelandEDIMessage>();
			outgoingMessage.EM_ApplicationReference = correlationId;
			outgoingMessage.EM_MessageText = "";
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "999";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_GB = aaaBranch.PK;
			Factory.Save();
			manifestHeader.Messages.Add(outgoingMessage);

			var incomingMessage = Factory.New<IcsNorthernIrelandEDIMessage>();
			incomingMessage.EM_ApplicationReference = correlationId;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageText = "<CC351A></CC351A>";
			Factory.Save();

			var processor = new IcsNorthernIrelandResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Incoming message status set to RECEIVED", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
			AssertEquals("Incoming message linked to manifest", manifestHeader.PK, incomingMessage.EM_LinkedObject?.PK);
			AssertEquals("Incoming message branch set", manifestHeader.Branch.PK, incomingMessage.EM_GB);

			var newIncomingMessage = Factory.New<IcsNorthernIrelandEDIMessage>();
			newIncomingMessage.EM_ApplicationReference = "Not Found";
			newIncomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			newIncomingMessage.EM_Status = EDIMessage.Status.Queued;
			newIncomingMessage.EM_MessageText = "<CC351A></CC351A>";
			Factory.Save();

			processor = new IcsNorthernIrelandResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(incomingMessage);
			AssertEquals("manifest not found - status set to FAILED", EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
		}

		internal static string correlationId = "87491122139921";
	}
}
