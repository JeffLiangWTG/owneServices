using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business;

public class FinalizeAVABREntryMessageSendingActionCollection : MessageSendingActionCollection
{
	public FinalizeAVABREntryMessageSendingActionCollection(JobDeclaration declaration, MessageSendingActionParent actionParent)
		: base(declaration.ActiveEntryHeaders, x => ((CusEntryHeader)x).MovementReferenceNumber, declaration.Factory)
	{
		this.actionParent = actionParent;
	}

	readonly MessageSendingActionParent actionParent;

	public new FinalizeAVABREntryMessageSendingAction AddNew() => (FinalizeAVABREntryMessageSendingAction)base.AddNew();

	public new FinalizeAVABREntryMessageSendingAction this[int index] =>
		(FinalizeAVABREntryMessageSendingAction)base[index];

	public FinalizeAVABREntryMessageSendingAction AddNew(CusEntryHeader entry)
	{
		var result = (FinalizeAVABREntryMessageSendingAction)GetSendingAction(entry);
		Add(result);
		return result;
	}

	protected override MessageSendingAction GetSendingAction(BusinessObject messagingObject) =>
		new FinalizeAVABREntryMessageSendingAction((CusEntryHeader)messagingObject, actionParent);
}
