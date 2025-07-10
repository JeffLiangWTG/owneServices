using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public sealed class ImportEntryMessageSendingActionCollection : BEJobDeclarationMessageSendingObjectCollection<ImportEntryMessageSendingAction>
{
	public ImportEntryMessageSendingActionCollection(JobDeclaration declaration) : base(declaration.ActiveEntryHeaders, declaration.Factory)
	{
	}

	public ImportEntryMessageSendingActionCollection(IEnumerable<BusinessObject> messagingObjects, BusinessObjectFactory factory) : base(messagingObjects, factory)
	{
	}

	protected override BEJobDeclarationMessageSendingObject GetSendingAction(BusinessObject messagingObject) => new ImportEntryMessageSendingAction((CusEntryHeader)messagingObject);

	public ImportEntryMessageSendingAction AddNew(CusEntryHeader entry)
	{
		var result = (ImportEntryMessageSendingAction)GetSendingAction(entry);
		Add(result);
		return result;
	}
}
