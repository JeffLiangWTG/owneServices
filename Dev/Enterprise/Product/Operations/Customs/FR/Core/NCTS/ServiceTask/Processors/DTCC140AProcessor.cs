using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC140A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC140AProcessor : DTBaseProcessor<Cc140AType>
	{
		public DTCC140AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC140A processor";

		protected override ZString GetNewMessageStatus(Cc140AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDetailedDepartureStatus(Cc140AType messageObject) => NctsDetailedStatusList.Codes.ResearchProcedureNotification;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretation(Cc140AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Heahea != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Answer limit date", GetReadableDate(messageObject.Heahea.DatLimResHea144)));
			}
			if (messageObject.Cusoffdepept != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Customs office of departure", messageObject.Cusoffdepept.RefNumEpt1));
			}
			if (messageObject.Cusoffcomaut != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Authority of recovery", messageObject.Cusoffcomaut.RefNumAut1));
			}
			return sb.ToString();
		}

		protected override void UpdateNCTSHeader(Cc140AType messageObject)
		{
			base.UpdateNCTSHeader(messageObject);

			if (NctsHeader != null)
			{
				NctsHeader.IsQueried = true;
			}
		}
	}
}
