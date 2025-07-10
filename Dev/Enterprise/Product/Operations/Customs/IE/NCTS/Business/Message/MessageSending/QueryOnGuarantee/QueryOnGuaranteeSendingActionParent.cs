using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class QueryOnGuaranteeSendingActionParent : BaseMessageSendingObjectParent<QueryOnGuaranteeSendingAction>
	{
		public QueryOnGuaranteeSendingActionParent(NctsHeader header) : base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			MessageType = NCTSOutgoingMessageTypeList.Codes.QueryOnGuarantees;
		}
		public NctsHeader Header { get; }

		public string MessageType { get; }

		public override BusinessObject TopLevelBusinessObject => Header;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<QueryOnGuaranteeSendingAction> GetSendingObjectsCollectionCore() => new QueryOnGuaranteeSendingActionCollection(this);
	}
}
