using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLBranchMessageProcessor : BranchCustomsMessageProcessor
	{
		public CLBranchMessageProcessor() : base(new ZString[] { EDIMessage.ApplicationCodes.CLCustoms }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			ApplicationTypeMessageProcessor result = null;
			switch (message.EM_MessageType)
			{
				case MessageTypes.Codes.CHA:
				case MessageTypes.Codes.CHB:
					result = new BLMessageProcessor(Logger);
					break;
				case MessageTypes.Codes.CHC:
					result = new BLCancelMessageProcessor(Logger);
					break;
				case MessageTypes.Codes.CHD:
				case MessageTypes.Codes.CHE:
					result = new AWBMessageProcessor(Logger);
					break;
				case MessageTypes.Codes.CHF:
					result = new AWBCancelMessageProcessor(Logger);
					break;
			}
			return result;
		}
	}
}
