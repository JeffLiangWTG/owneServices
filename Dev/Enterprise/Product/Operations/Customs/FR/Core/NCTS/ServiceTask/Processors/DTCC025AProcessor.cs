using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC025A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC025AProcessor : DTBaseProcessor<Cc025AType>
	{
		public DTCC025AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC025A processor";

		protected override ZString GetNewMessageStatus(Cc025AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewArrivalStatus(Cc025AType messageObject) => NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;

		protected override ZString GetNewDetailedDepartureStatus(Cc025AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				switch (messageObject.Heahea.IrrHea1020)
				{
					case Flag.Item0:
						return NctsDetailedStatusList.Codes.NoIrregularities;
					case Flag.Item1:
						return NctsDetailedStatusList.Codes.Irregularities;
				}
			}
			return ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretation(Cc025AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetArrivalStatusInterpretation(messageObject));
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Heahea != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Release Date", GetReadableDate(messageObject.Heahea.GooRelDatHea176)));
				sb.Append(GetKeyValuePairInterpretation("Irregularities", GetReadableDate(messageObject.Heahea.IrrHea1020 == Flag.Item1 ? "Yes" : "No")));
			}
			return sb.ToString();
		}
	}
}
