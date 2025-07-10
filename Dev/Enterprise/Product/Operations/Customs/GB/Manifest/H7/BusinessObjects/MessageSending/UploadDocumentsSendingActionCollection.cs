using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.H7.Business
{
	public sealed class UploadDocumentsSendingActionCollection : EU.H7.Business.MessageSendingObjectCollection<UploadDocumentsSendingAction>
	{
		public UploadDocumentsSendingActionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UploadDocumentsSendingAction(Factory.New<AsycudaBill>());
		}
	}
}
