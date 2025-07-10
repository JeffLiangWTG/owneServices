using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeAccessCodesSendingObjectParent : BaseMessageSendingObjectParent<GuaranteeAccessCodesSendingObject>
	{
		public GuaranteeAccessCodesSendingObjectParent(CusGuaranteeHeader guarantee) : base(guarantee?.Factory)
		{
			CusGuaranteeHeader = Argument.NotNull(guarantee, nameof(guarantee));
		}

		public CusGuaranteeHeader CusGuaranteeHeader { get; }

		public override BusinessObject TopLevelBusinessObject => CusGuaranteeHeader;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		public override bool AllowEmptyDeclaration => true;

		protected override NonPersistentBusinessObjectCollection<GuaranteeAccessCodesSendingObject> GetSendingObjectsCollectionCore()
			=> new GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>(this);
	}
}
