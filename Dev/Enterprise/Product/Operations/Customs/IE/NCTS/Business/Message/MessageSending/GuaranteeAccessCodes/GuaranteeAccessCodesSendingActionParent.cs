using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using GuaranteeAccessCodesSendingObject = Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObject;
using GuaranteeAccessCodesSendingObjectParent = Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeAccessCodesSendingActionParent : GuaranteeAccessCodesSendingObjectParent
	{
		public GuaranteeAccessCodesSendingActionParent(CusGuaranteeHeader cusGuaranteeHeader) : base(cusGuaranteeHeader)
		{
		}

		public new CusGuaranteeHeader CusGuaranteeHeader => (CusGuaranteeHeader)base.CusGuaranteeHeader;

		public new EU.NCTS.Business.IGuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingAction> SendingObjectsCollection =>
			(EU.NCTS.Business.GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingAction>)base.SendingObjectsCollection;

		protected override NonPersistentBusinessObjectCollection<GuaranteeAccessCodesSendingObject> GetSendingObjectsCollectionCore()
			=> (NonPersistentBusinessObjectCollection<GuaranteeAccessCodesSendingObject>)new EU.NCTS.Business.GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingAction>(this);
	}
}
