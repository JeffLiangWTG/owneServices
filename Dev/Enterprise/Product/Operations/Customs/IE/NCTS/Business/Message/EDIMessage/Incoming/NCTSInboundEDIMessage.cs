using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NCTSInboundEDIMessage : IE.Business.InboundEDIMessage
	{
		public NCTSInboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static NCTSOutboundEDIMessage GetPreviousDeclarationMessage(BusinessObjectFactory factory, ZGuid linkUniqueId, ZDateTime systemCreateTimeUtc)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, NCTSOutboundEDIMessage.ApplicationCodes.IECustomsNCTS)
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, NCTSOutboundEDIMessage.Direction.Transmit)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, linkUniqueId)
				.AddToFilter(EDIMessageSchema.EM_Status, new[] { EDIMessageStatusList.Codes.Sent, EDIMessageStatusList.Codes.Acknowledged })
				.AddToFilter(EDIMessageSchema.EM_MessageType, new[] { NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData, NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationAmendment })
				.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, systemCreateTimeUtc);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
			return factory.LoadTop1<NCTSOutboundEDIMessage>(query);
		}

		public NCTSOutboundEDIMessage GetOutboundMessage() => GetOriginalMessage(Factory, EM_ApplicationCode, EM_ApplicationReference) as NCTSOutboundEDIMessage;

		public new NCTSInboundEDIMessageLookups Lookups => (NCTSInboundEDIMessageLookups)base.Lookups;

		protected override EDIMessageLookups GetNewLookups() => new NCTSInboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsNCTS;
		}
	}
}
