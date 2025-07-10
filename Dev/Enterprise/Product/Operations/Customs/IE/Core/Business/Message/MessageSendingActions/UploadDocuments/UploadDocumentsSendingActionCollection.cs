using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class UploadDocumentsSendingActionCollection : CusEntryHeaderMessageSendingActionCollection<UploadDocumentsSendingAction>
	{
		public UploadDocumentsSendingActionCollection(UploadDocumentsSendingActionParent sendingActionParent) : base(sendingActionParent) { }

		protected new UploadDocumentsSendingActionParent Parent => (UploadDocumentsSendingActionParent)base.Parent;

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			((UploadDocumentsSendingAction)businessObject).Validation.CheckHasOpenDocumentsOrIsControl();
		}
	}
}
