using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC005A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC005AProcessor : DTBaseProcessor<Cc005AType>
	{
		public DTCC005AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC005A processor";

		protected override ZString GetNewMessageStatus(Cc005AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDepartureStatus(Cc005AType messageObject) => NctsHeader.MovementHeader.LastNonIntermediateStatus;

		protected override ZString GetNewDetailedDepartureStatus(Cc005AType messageObject) => NctsDetailedStatusList.Codes.AmendmentRefused;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretation(Cc005AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Heahea != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Rejection reason", messageObject.Heahea.AmeRejMotTexHea605));
				sb.Append(GetKeyValuePairInterpretation("Rejection comment", messageObject.Heahea.ComHea1018));
			}
			sb.Append(GetFunctionalErrorInterpretation(messageObject));
			return sb.ToString();
		}
	}
}
