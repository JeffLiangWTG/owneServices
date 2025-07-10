using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentsSendingActionCollection : CusEntryHeaderMessageSendingActionCollection<DocumentsSendingAction>
	{
		public DocumentsSendingActionCollection(DocumentsSendingActionParent sendingActionParent) : base(sendingActionParent) { }

		protected new DocumentsSendingActionParent Parent => (DocumentsSendingActionParent)base.Parent;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((DocumentsSendingAction)bizOAdded).MessageType = Parent.MessageType;
		}
	}
}
