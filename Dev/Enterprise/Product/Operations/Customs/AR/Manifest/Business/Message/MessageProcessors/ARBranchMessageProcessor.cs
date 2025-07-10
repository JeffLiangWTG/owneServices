using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class ARBranchMessageProcessor : BranchCustomsMessageProcessor
	{
		public ARBranchMessageProcessor() : base(new ZString[] { EDIMessage.ApplicationCodes.ARCustoms }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			ApplicationTypeMessageProcessor result = null;

			if (message.EM_MessageType == MessageTypes.Codes.ARB)
			{
				result = new BLMessageProcessor(Logger);
			}
			else if (message.EM_MessageType == MessageTypes.Codes.ARE)
			{
				result = new AWBMessageProcessor(Logger);
			}

			return result;
		}
	}
}
