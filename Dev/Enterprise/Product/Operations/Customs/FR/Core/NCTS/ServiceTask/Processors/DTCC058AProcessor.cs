using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC058A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC058AProcessor : DTBaseProcessor<Cc058AType>
	{
		public DTCC058AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC058A processor";

		protected override ZString GetNewMessageStatus(Cc058AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.UnloadingRemarksRejected;

		protected override ZString GetNewDetailedArrivalStatus(Cc058AType messageObject) => FR.Business.NctsDetailedStatusList.Codes.UnloadingRemarksRejected;

		protected override ZString GetMessageInterpretation(Cc058AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDetailedArrivalStatusInterpretation(messageObject));
			sb.Append(GetMessageStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			sb.Append(GetFunctionalErrorInterpretation(messageObject));
			return sb.ToString();
		}
	}
}
