using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC021A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC021AProcessor : DTBaseProcessor<Cc021AType>
	{
		public DTCC021AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC021A processor";

		protected override ZString GetNewMessageStatus(Cc021AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewArrivalStatus(Cc021AType messageObject) => FR.Business.NctsTransitStatusList.Codes.ArrivalRejected;

		protected override ZString GetNewDetailedArrivalStatus(Cc021AType messageObject) => NctsDetailedStatusList.Codes.AlternateDestinationRejected;

		protected override ZString GetMessageInterpretation(Cc021AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetArrivalStatusInterpretation(messageObject));
			sb.Append(GetDetailedArrivalStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Heahea != null)
			{
				sb.Append(GetKeyValuePairInterpretation((NoResString)"Reason", messageObject.Heahea.DivRejCodTxtHea614));
			}
			return sb.ToString();
		}
	}
}
