using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ExportEntryMessageSendingActionCollection : MessageSendingActionCollection
	{
		public ExportEntryMessageSendingActionCollection(JobDeclaration declaration) : base(declaration.ActiveEntryHeaders, x => ((CusEntryHeader)x).MovementReferenceNumber, declaration.Factory)
		{
		}

		public new ExportEntryMessageSendingAction AddNew() => (ExportEntryMessageSendingAction)base.AddNew();

		public new ExportEntryMessageSendingAction this[int index] => (ExportEntryMessageSendingAction)base[index];

		public ExportEntryMessageSendingAction AddNew(CusEntryHeader entry)
		{
			var result = (ExportEntryMessageSendingAction)GetSendingAction(entry);
			Add(result);
			return result;
		}

		protected override MessageSendingAction GetSendingAction(BusinessObject messagingObject) => new ExportEntryMessageSendingAction((CusEntryHeader)messagingObject, this);
	}
}
