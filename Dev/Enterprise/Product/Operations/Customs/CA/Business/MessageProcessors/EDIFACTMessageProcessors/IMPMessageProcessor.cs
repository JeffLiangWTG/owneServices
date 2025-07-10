using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class IMPMessageProcessor : EDIFACTMessageProcessor
	{
		public IMPMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.CAIMP; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return Res.GetString("D356C78B-D084-43fe-8226-A4C65E8F5B35", "CA Customs Import"); }
		}

		protected override CustomsMessageProcessor GetMessageProcessor(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			if (ediMessage is CADMessage)
			{
				return new CADResponseMessageProcessor(Logger);
			}
			else
			{
				return base.GetMessageProcessor(ediMessage);
			}
		}
	}
}
