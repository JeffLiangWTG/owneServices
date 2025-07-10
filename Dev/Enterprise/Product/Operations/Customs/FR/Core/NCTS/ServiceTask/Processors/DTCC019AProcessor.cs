using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC019A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC019AProcessor : DTBaseProcessor<Cc019AType>
	{
		public DTCC019AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC019A processor";

		protected override ZString GetNewMessageStatus(Cc019AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDetailedDepartureStatus(Cc019AType messageObject) => NctsDetailedStatusList.Codes.DiscrepanciesNotification;

		protected override ZString GetMessageInterpretation(Cc019AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			return sb.ToString();
		}
	}
}
