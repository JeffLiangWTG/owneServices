using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC014A;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC014AProcessor : DTBaseProcessor<Cc014AType>
	{
		public DTCC014AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC014A processor";
	}
}
