using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSInboundEDIMessage : GbEDIMessage
	{
		public EMCSInboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new EMCSInboundEDIMessageLookups Lookups => (EMCSInboundEDIMessageLookups)base.Lookups;

		protected override EDIMessageLookups GetNewLookups() => new EMCSInboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsEMCS;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		public EMCSJobDeclaration LinkedDeclaration => EM_LinkedObject as EMCSJobDeclaration;

		public override ZString EM_MessageInterpretation
		{
			get => base.MessageInterpretationNoteManager.Value;
			set
			{
				var oldValue = EM_MessageInterpretation;
				base.EM_MessageInterpretation = value;
				EM_MessageInterpretationInfo.RefreshBinding(oldValue);
			}
		}

		public static EDIMessage GetOriginalMessage(BusinessObjectFactory factory, ZString applicationCode, ZString transactionId)
		{
			EDIMessage originalMessage = null;
			if (!transactionId.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
					.AddToFilter(EDIMessageSchema.EM_ApplicationReference, transactionId);
				query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
				originalMessage = factory.LoadTop1<EDIMessage>(query);
			}
			return originalMessage;
		}

		public static EDIMessage GetOriginalMessageWithoutTID(BusinessObjectFactory factory, ZString applicationCode, ZGuid linkUniqueId, ZDateTime systemCreateTimeUtc)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode)
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, linkUniqueId)
				.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Sent)
				.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, systemCreateTimeUtc);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
			return factory.LoadTop1<EDIMessage>(query);
		}
	}
}
