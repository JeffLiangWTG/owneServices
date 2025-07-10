using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public CDSOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new CDSInterchangeProvider(Logger, readyMessages);
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCustomsDeclarationServices);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, GetOutgoingMessageTypes());
			return result;
		}

		internal static string[] GetOutgoingMessageTypes() => new[]
		{
			CDSEDIMessageTypeList.Codes.NewDeclaration,
			CDSEDIMessageTypeList.Codes.AmendDeclaration,
			CDSEDIMessageTypeList.Codes.CancelDeclaration,
			CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest,
			CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest,
			CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest,
			CDSEDIMessageTypeList.Codes.ArrivalNotification,
			CDSEDIMessageTypeList.Codes.FecChallenge,
			CDSEDIMessageTypeList.Codes.NilAmendment,
			CDSEDIMessageTypeList.Codes.CDSPentantAcaMessage,
			CDSEDIMessageTypeList.Codes.MasterQueryDeclaration
		};
	}
}
