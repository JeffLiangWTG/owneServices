using Enterprise.BatchProcessor;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class ACIMessageProcessor : EDIFACTMessageProcessor
	{
		public ACIMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.CAACI; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return Res.GetString("270b218a-76b5-4bed-9843-034383932064", "CA Customs ACI"); }
		}
	}
}
