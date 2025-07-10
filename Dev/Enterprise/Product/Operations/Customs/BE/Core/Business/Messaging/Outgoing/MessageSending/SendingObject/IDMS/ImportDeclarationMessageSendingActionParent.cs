using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public sealed class ImportDeclarationMessageSendingActionParent : BEJobDeclarationMessageSendingObjectParent<ImportEntryMessageSendingActionCollection, ImportEntryMessageSendingAction>
{
	public ImportDeclarationMessageSendingActionParent(JobDeclaration declaration) : base(declaration, declaration.ActiveEntryHeaders)
	{
	}

	public ImportDeclarationMessageSendingActionParent(BaseJobDeclaration declaration, ActiveCusEntryHeaderCollection activeEntryHeaders) : base(declaration, declaration.ActiveEntryHeaders)
	{
	}

	JobDeclaration Declaration => (JobDeclaration)TopLevelBusinessObject;

	protected override NonPersistentBusinessObjectCollection<ImportEntryMessageSendingAction> GetSendingObjectsCollectionCore()
	{
		var result = new ImportEntryMessageSendingActionCollection(Declaration);
		result.PopulateElements();
		return result;
	}

	public new ImportEntryMessageSendingActionCollection SendingObjectsCollection => (ImportEntryMessageSendingActionCollection)base.SendingObjectsCollection;

	protected override IEnumerable<INotification> GetNewMessageErrorCollector()
	{
		return new JobDeclarationMessageSendingNotificationCollector(Declaration, SendingObjectsCollection.Cast<ImportEntryMessageSendingAction>().Where(x => x.ShouldSend).Select(x => x.Header));
	}
}
