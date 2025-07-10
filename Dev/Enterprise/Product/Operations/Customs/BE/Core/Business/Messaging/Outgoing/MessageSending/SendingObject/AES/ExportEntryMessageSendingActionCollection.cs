using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public sealed class ExportEntryMessageSendingActionCollection : BEJobDeclarationMessageSendingObjectCollection<ExportEntryMessageSendingAction>
{
	public ExportEntryMessageSendingActionCollection(JobDeclaration declaration) : base(declaration.ActiveEntryHeaders, declaration.Factory)
	{
	}

	public ExportEntryMessageSendingActionCollection(IEnumerable<BusinessObject> messagingObjects, BusinessObjectFactory factory) : base(messagingObjects, factory)
	{
	}

	protected override BEJobDeclarationMessageSendingObject GetSendingAction(BusinessObject messagingObject) => new ExportEntryMessageSendingAction((CusEntryHeader)messagingObject);

	public ExportEntryMessageSendingAction AddNew(CusEntryHeader entry)
	{
		var result = (ExportEntryMessageSendingAction)GetSendingAction(entry);
		Add(result);
		return result;
	}
}
