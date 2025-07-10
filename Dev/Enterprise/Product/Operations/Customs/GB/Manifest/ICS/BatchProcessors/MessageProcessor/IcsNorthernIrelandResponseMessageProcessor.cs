using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.ICS
{
	public class IcsNorthernIrelandResponseMessageProcessor : ApplicationTypeMessageProcessor
	{
		public IcsNorthernIrelandResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "ICS NI Response Message Processor";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is IcsNorthernIrelandEDIMessage icsMessage)
			{
				var messageStatus = EDIMessageStatusList.Codes.Failed;
				var manifestHeader = icsMessage.GetManifestUsingICSCorrelationId();

				if (manifestHeader != null)
				{
					icsMessage.EM_LinkedObject = manifestHeader;
					icsMessage.EM_GB = manifestHeader.Branch.PK;
					icsMessage.EM_MessageSubType = ZString.Empty; //to be done
					messageStatus = EDIMessageStatusList.Codes.Received;
				}
				icsMessage.EM_Status = messageStatus;
			}
		}
	}
}
