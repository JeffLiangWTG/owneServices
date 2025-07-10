using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public sealed class ExportDeclarationMessageSendingActionParent : BEJobDeclarationMessageSendingObjectParent<ExportEntryMessageSendingActionCollection, ExportEntryMessageSendingAction>
{
	public ExportDeclarationMessageSendingActionParent(JobDeclaration declaration) : base(declaration, declaration.ActiveEntryHeaders)
	{
	}

	public ExportDeclarationMessageSendingActionParent(BaseJobDeclaration declaration, ActiveCusEntryHeaderCollection activeEntryHeaders) : base(declaration, declaration.ActiveEntryHeaders)
	{
	}

	JobDeclaration Declaration => (JobDeclaration)TopLevelBusinessObject;

	protected override NonPersistentBusinessObjectCollection<ExportEntryMessageSendingAction> GetSendingObjectsCollectionCore()
	{
		var result = new ExportEntryMessageSendingActionCollection(Declaration);
		result.PopulateElements();
		return result;
	}

	public new ExportEntryMessageSendingActionCollection SendingObjectsCollection => (ExportEntryMessageSendingActionCollection)base.SendingObjectsCollection;

	protected override IEnumerable<INotification> GetNewMessageErrorCollector()
	{
		return new JobDeclarationMessageSendingNotificationCollector(Declaration, SendingObjectsCollection.Cast<ExportEntryMessageSendingAction>().Where(x => x.ShouldSend).Select(x => x.Header));
	}
}
