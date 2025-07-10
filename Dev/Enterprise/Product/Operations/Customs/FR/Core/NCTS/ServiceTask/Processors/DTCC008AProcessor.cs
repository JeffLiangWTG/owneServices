using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC008A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC008AProcessor : DTBaseProcessor<Cc008AType>
	{
		public DTCC008AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC008A processor";

		protected override ZString GetNewMessageStatus(Cc008AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationRejected;

		protected override ZString GetNewArrivalStatus(Cc008AType messageObject) => FR.Business.NctsTransitStatusList.Codes.ArrivalRejected;

		protected override ZString GetNewDetailedArrivalStatus(Cc008AType messageObject) => NctsDetailedStatusList.Codes.ArrivalNotificationRejected;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected override ZString GetMessageInterpretation(Cc008AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetArrivalStatusInterpretation(messageObject));
			sb.Append(GetDetailedArrivalStatusInterpretation(messageObject));
			sb.Append(GetMessageStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Heahea != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Reason", messageObject.Heahea.ArrRejReaHea242));
				sb.Append(GetKeyValuePairInterpretation("Action required", messageObject.Heahea.ActToBeTakHea238));
			}
			sb.Append(GetFunctionalErrorInterpretation(messageObject));
			return sb.ToString();
		}
	}
}
