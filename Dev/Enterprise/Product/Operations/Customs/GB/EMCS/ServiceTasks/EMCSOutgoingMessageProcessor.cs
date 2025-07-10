using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks
{
	public class EMCSOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public EMCSOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new EMCSInterchangeProvider(Logger, readyMessages);
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCustomsEMCS);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, GetOutgoingMessageTypes());
			result.AddToFilter(EDIMessageSchema.EM_MessageOwner, SQLComparisonOperator.NotEqual, ZString.Empty);
			return result;
		}

		internal static string[] GetOutgoingMessageTypes() => new[]
		{
			EMCSGBOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD,
			EMCSGBOutgoingMessageTypeList.Codes.CancellationOfEAD,
			EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination,
			EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery,
			EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage,
			EMCSGBOutgoingMessageTypeList.Codes.PreValidateTrader,
			EMCSGBOutgoingMessageTypeList.Codes.ReportOfReceipt,
			EMCSGBOutgoingMessageTypeList.Codes.Splitting,
			EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD,
		};
	}
}
