using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EMMRMessageProcessor : ManifestMessageResponseProcessor
	{
		public EMMRMessageProcessor(LoggingInformation logger)
			: base(logger, "EMM", "Export Main Manifest Response(EMMR)")
		{
		}
	}
}
