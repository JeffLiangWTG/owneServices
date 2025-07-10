using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC044A;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC044AProcessor : DTBaseProcessor<Cc044AType>
	{
		public DTCC044AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC044A processor";
	}
}
