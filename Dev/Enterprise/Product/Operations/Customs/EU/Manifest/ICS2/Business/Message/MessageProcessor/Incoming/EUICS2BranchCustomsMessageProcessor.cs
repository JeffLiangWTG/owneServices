using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class EUICS2BranchCustomsMessageProcessor : BranchCustomsMessageProcessor
	{
		public EUICS2BranchCustomsMessageProcessor() : base(new ZString[] { EDIInterchange.ApplicationCodes.IC2 }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			if (message.EM_MessageSubType == ICS2InboundEDIMessage.UndefinedSubType)
			{
				return new SoapEnvelopeMessageProcessor(Logger);
			}

			switch (message.EM_MessageType)
			{
				case MessageTypes.Codes.N01:
					return new IE3N01MessageProcessor(Logger);
				case MessageTypes.Codes.N02:
					return new IE3N02MessageProcessor(Logger);
				case MessageTypes.Codes.N03:
					return new IE3N03MessageProcessor(Logger);
				case MessageTypes.Codes.N04:
					return new IE3N04MessageProcessor(Logger);
				case MessageTypes.Codes.N05:
					return new IE3N05MessageProcessor(Logger);
				case MessageTypes.Codes.N07:
					return new IE3N07MessageProcessor(Logger);
				case MessageTypes.Codes.N08:
					return new IE3N08MessageProcessor(Logger);
				case MessageTypes.Codes.N09:
					return new IE3N09MessageProcessor(Logger);
				case MessageTypes.Codes.N10:
					return new IE3N10MessageProcessor(Logger);
				case MessageTypes.Codes.N11:
					return new IE3N11MessageProcessor(Logger);
				case MessageTypes.Codes.N99:
					return new IE3N99MessageProcessor(Logger);
				case MessageTypes.Codes.R01:
					return new IE3R01MessageProcessor(Logger);
				case MessageTypes.Codes.R04:
					return new IE3R04MessageProcessor(Logger);
				case MessageTypes.Codes.R07:
					return new IE3R07MessageProcessor(Logger);
				case MessageTypes.Codes.R08:
					return new IE3R08MessageProcessor(Logger);
				case MessageTypes.Codes.Q01:
					return new IE3Q01MessageProcessor(Logger);
				case MessageTypes.Codes.Q02:
					return new IE3Q02MessageProcessor(Logger);
				case MessageTypes.Codes.Q03:
					return new IE3Q03MessageProcessor(Logger);
				case Constant.MessageTypes.XER:
					return new XERMessageProcessor(Logger);
			}

			return null;
		}
	}
}
