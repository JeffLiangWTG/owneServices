using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC028A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC028AProcessor : DTBaseProcessor<Cc028AType>
	{
		public DTCC028AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC028A processor";

		protected override ZString GetNewMessageStatus(Cc028AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDepartureStatus(Cc028AType messageObject) => NctsTransitStatusList.Codes.DeclarationMrnAllocated;

		protected override ZString GetMessageInterpretation(Cc028AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			return sb.ToString();
		}

		protected override void UpdateNCTSHeader(Cc028AType messageObject)
		{
			base.UpdateNCTSHeader(messageObject);
			if (NctsHeader != null)
			{
				if (!string.IsNullOrEmpty(messageObject.Heahea?.DocNumHea5))
				{
					NctsHeader.MovementReferenceEntryNumber.CE_EntryNum = messageObject.Heahea.DocNumHea5;
				}

				if (NctsHeader.MovementHeader != null && NctsHeader.MovementHeader.BM_EntryDate.IsEmpty && ZDateTime.TryParseExact(messageObject.Heahea?.AccDatHea158, out var tolDate, "yyyyMMdd"))
				{
					NctsHeader.MovementHeader.BM_EntryDate = tolDate;
					NctsHeader.MovementHeader.BM_ValuationDate = tolDate;
				}
			}
		}
	}
}
