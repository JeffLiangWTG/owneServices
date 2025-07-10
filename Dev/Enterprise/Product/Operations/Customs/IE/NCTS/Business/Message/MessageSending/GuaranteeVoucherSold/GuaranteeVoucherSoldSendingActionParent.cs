using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeVoucherSoldSendingActionParent : BaseMessageSendingObjectParent<GuaranteeVoucherSoldSendingAction>
	{
		public GuaranteeVoucherSoldSendingActionParent(CusGuaranteeHeader header) : base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			MessageType = NCTSOutgoingMessageTypeList.Codes.GuaranteeVoucherSold;
		}
		public CusGuaranteeHeader Header { get; }

		public string MessageType { get; }

		public override BusinessObject TopLevelBusinessObject => Header;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<GuaranteeVoucherSoldSendingAction> GetSendingObjectsCollectionCore() => new GuaranteeVoucherSoldSendingActionCollection(this);
	}
}
