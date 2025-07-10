using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF03A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCCF03AProcessor : DTBaseProcessor<Ccf03AType>
	{
		public DTCCF03AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CCF03A processor";

		protected override ZString GetNewMessageStatus(Ccf03AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetMessageInterpretation(Ccf03AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			return sb.ToString();
		}

		protected override ZString GetNewDetailedDepartureStatus(Ccf03AType messageObject) => NctsDetailedStatusList.Codes.TransitNotifiedAtDestination;
	}
}
