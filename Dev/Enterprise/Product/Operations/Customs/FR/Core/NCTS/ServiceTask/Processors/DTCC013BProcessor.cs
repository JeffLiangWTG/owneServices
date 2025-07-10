using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC013B;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC013BProcessor : DTBaseProcessor<Cc013BType>
	{
		public DTCC013BProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC013B processor";
	}
}
