using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF15A;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCCF15AProcessor : DTBaseProcessor<Ccf15AType>
	{
		public DTCCF15AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CCF15A processor";
	}
}
