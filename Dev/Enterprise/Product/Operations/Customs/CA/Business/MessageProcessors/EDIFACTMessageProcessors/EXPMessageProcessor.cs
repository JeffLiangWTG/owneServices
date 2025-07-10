using Enterprise.BatchProcessor;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class EXPMessageProcessor : EDIFACTMessageProcessor
	{
		public EXPMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.CAEXP; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return Res.GetString("5c4f804c-55f0-4ebd-b72c-5504f96cee1c", "CA Customs G7 Export"); }
		}
	}
}
