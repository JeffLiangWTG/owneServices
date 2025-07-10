using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXBranchMessageProcessor : BranchCustomsMessageProcessor
	{
		public MXBranchMessageProcessor() : base(new ZString[] { EDIMessage.ApplicationCodes.MXCustoms }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			ApplicationTypeMessageProcessor result = null;
			if (message.EM_MessageType == MessageTypes.Codes.MXA)
			{
				result = new SeaFirstResponseProcessor(Logger);
			}
			else if (message.EM_MessageType == MessageTypes.Codes.MXD)
			{
				result = new SeaFinalResponseProcessor(Logger);
			}
			else if (message.EM_MessageType == MXMessageConstants.XER)
			{
				result = new SeaErrorResponseProcessor(Logger);
			}
			else if (message.EM_MessageType == MessageTypes.Codes.MXF)
			{
				result = new AirFirstResponseProcessor(Logger);
			}
			else if (message.EM_MessageType == MessageTypes.Codes.MXG)
			{
				result = new AirFinalResponseProcessor(Logger);
			}
			return result;
		}
	}
}
