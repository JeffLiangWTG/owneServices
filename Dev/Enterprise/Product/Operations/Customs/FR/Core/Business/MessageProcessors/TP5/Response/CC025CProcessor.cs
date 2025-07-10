using CargoWise.Customs.FR.MessageDefinitions.TP5.CC025C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC025CProcessor : TP5BaseProcessor<Cc025CType>
	{
		public CC025CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.GoodsReleaseNotification;

		protected override ZString GetMRNFromResponseMessage(Cc025CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZDateTime GetNewReleaseDateFromResponseMessage(Cc025CType messageObject) => messageObject.TransitOperation?.ReleaseDate ?? ZDateTime.Empty;

		protected override ZString GetNewArrivalStatus(Cc025CType messageObject)
		{
			return messageObject.TransitOperation.ReleaseIndicator switch
			{
				"1" => NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
				"2" => NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease,
				"3" => NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease,
				"4" => NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease,
				_ => ZString.Empty
			};
		}

		protected override ZString GetNewMessageStatus(Cc025CType messageObject)
		{
			return messageObject.TransitOperation.ReleaseIndicator switch
			{
				"1" or "3" => LogicalStatusList.Codes.Sent,
				"2" or "4" => LogicalStatusList.Codes.Accepted,
				_ => ZString.Empty
			};
		}

		protected override ZString GetNewPhase(Cc025CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Arrival;
	}
}
