using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC035A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC035AProcessor : DTBaseProcessor<Cc035AType>
	{
		public DTCC035AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC035A processor";

		protected override ZString GetNewMessageStatus(Cc035AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDetailedDepartureStatus(Cc035AType messageObject) => NctsDetailedStatusList.Codes.RecoveryProcedure;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretation(Cc035AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Heahea != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Recovery date", GetReadableDate(messageObject.Heahea.AccDatHea158)));
				sb.Append(GetKeyValuePairInterpretation("Notification date", GetReadableDate(messageObject.Heahea.RecNotDatHea766)));
				sb.Append(GetKeyValuePairInterpretation("Detail", messageObject.Heahea.RecNotTexHea730));
				sb.Append(GetKeyValuePairInterpretation("Amount claimed", FormattableString.Invariant($"{messageObject.Heahea.AmoClaHea732}{messageObject.Heahea.CurHea734}")));
			}
			if (messageObject.Guarantor != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Guarantee ID", messageObject.Guarantor.NamGu620));
			}
			if (messageObject.Cusoffdepept != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Office of departure", messageObject.Cusoffdepept.RefNumEpt1));
			}
			if (messageObject.Custoffcomautrec != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Authority of recovery", messageObject.Custoffcomautrec.RefNumRec1));
			}
			return sb.ToString();
		}

		protected override void UpdateNCTSHeader(Cc035AType messageObject)
		{
			base.UpdateNCTSHeader(messageObject);

			if (NctsHeader != null)
			{
				NctsHeader.IsQueried = false;
			}
		}
	}
}
