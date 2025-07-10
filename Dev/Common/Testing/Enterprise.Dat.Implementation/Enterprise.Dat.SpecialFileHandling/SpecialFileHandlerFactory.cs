using System.Collections.Generic;
using Dat.Integration.SpecialFileHandling;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dat.SpecialFileHandling
{
	[CodeAlive("DAT Implementation")]
	public class SpecialFileHandlerFactory : ISpecialFileHandlerFactory
	{
		public SpecialFileHandlerFactory(SpecialFileHandlerContext context)
		{
			this.context = context;
		}

		public IEnumerable<ISpecialFileHandler> GetSpecialFileHandlers()
		{
			return new ISpecialFileHandler[]
			{
				new CustomizedDocumentFileHandler(),
				new ResourcesDeltaFileHandler(),
				new UpgradeMapperChangeRequest(context),
				new VisualizerDocumentDataMapperChangeRequest(context),
				new DataVersionChangeRequest(context),
				new DocumentXmlVersionChangeRequest(context),
				new ClientDocumentsXmlVersionChangeRequest(),
				new AnalyticsReportVersionChangeRequest(context),
				new SsasVersionChangeRequest(context),
				new WebPrintClientVersionChangeRequest(context),
				new TransformVersionBump(context),
			};
		}

		readonly SpecialFileHandlerContext context;
	}
}
