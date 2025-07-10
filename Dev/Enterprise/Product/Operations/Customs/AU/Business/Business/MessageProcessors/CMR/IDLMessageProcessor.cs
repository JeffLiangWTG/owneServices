using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IDLMessageProcessor : CMRMessageResponseProcessor
	{
		public IDLMessageProcessor(LoggingInformation logger) : base(logger, "IDL", "Idle EDN/CRN Advice (IDL)")
		{
		}

		protected override bool IsUnsolicitedMessage
		{
			get { return true; }
		}
	}
}
