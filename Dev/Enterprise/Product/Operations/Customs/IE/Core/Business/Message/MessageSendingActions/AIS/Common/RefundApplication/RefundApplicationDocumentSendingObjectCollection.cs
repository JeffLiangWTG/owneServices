using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationDocumentSendingObjectCollection : NonPersistentBusinessObjectCollection<RefundApplicationDocumentSendingObject>
	{
		public RefundApplicationDocumentSendingObjectCollection(CusEntryHeader entryHeader, RefundApplicationMessageSendingAction action)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Action = Argument.NotNull(action, nameof(action));
			MaxCountValidationEnable(99);
		}
		CusEntryHeader EntryHeader { get; }
		RefundApplicationMessageSendingAction Action { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject() => new RefundApplicationDocumentSendingObject(EntryHeader, Action);

		protected override bool AllowNewCore => Action.ShouldSend;
	}
}
