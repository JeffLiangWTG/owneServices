using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ImportEntryMessageSendingActionCollection : MessageSendingActionCollection
	{
		public ImportEntryMessageSendingActionCollection(JobDeclaration declaration, MessageSendingActionParent actionParent) : base(declaration.ActiveEntryHeaders, x => ((CusEntryHeader)x).MovementReferenceNumber, declaration.Factory)
		{
			this.actionParent = actionParent;
		}
		readonly MessageSendingActionParent actionParent;

		protected override MessageSendingAction GetSendingAction(BusinessObject messagingObject) => new ImportEntryMessageSendingAction((CusEntryHeader)messagingObject, actionParent);

		public new ImportEntryMessageSendingAction AddNew() => (ImportEntryMessageSendingAction)base.AddNew();

		public new ImportEntryMessageSendingAction this[int index] => (ImportEntryMessageSendingAction)base[index];

		public ImportEntryMessageSendingAction AddNew(CusEntryHeader entry)
		{
			var result = (ImportEntryMessageSendingAction)GetSendingAction(entry);
			Add(result);
			return result;
		}
	}
}
