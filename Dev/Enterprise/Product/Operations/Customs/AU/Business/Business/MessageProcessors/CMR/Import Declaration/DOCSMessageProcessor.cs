using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DOCSMessageProcessor : BaseImportDeclarationMessageProcessor
	{
		public DOCSMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.DOCS, "Assessment Processing for Import Declarations (DOCS)")
		{
		}

		protected override bool IsUnsolicitedMessage
		{
			get { return true; }
		}
	}
}
