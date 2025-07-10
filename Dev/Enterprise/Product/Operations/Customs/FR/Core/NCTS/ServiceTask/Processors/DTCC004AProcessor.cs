using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC004A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC004AProcessor : DTBaseProcessor<Cc004AType>
	{
		public DTCC004AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC004A processor";

		protected override ZString GetNewMessageStatus(Cc004AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDepartureStatus(Cc004AType messageObject) => NctsHeader.MovementHeader.LastNonIntermediateStatus;

		protected override ZString GetNewDetailedDepartureStatus(Cc004AType messageObject) => NctsDetailedStatusList.Codes.AmendmentAccepted;

		protected override ZString GetMessageInterpretation(Cc004AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			return sb.ToString();
		}
	}
}
