using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Environment;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business;

public class FinalizeAVABRMessageSendingActionParent : MessageSendingActionParent
{
	public FinalizeAVABRMessageSendingActionParent(JobDeclaration declaration) : base(declaration,
		declaration.ActiveEntryHeaders, x => ((CusEntryHeader)x).MovementReferenceNumber,
		Env.Security.CustomsDeclarationSendWithMessageErrors)
	{
	}

	protected override bool OnlyOneObjectAllowedToBeSent => true;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

	protected override IEnumerable<INotification> GetNewMessageErrorCollector()
	{
		return new JobDeclarationMessageSendingNotificationCollector(Declaration, SendingObjectsCollection.Cast<FinalizeAVABREntryMessageSendingAction>().Where(x => x.ShouldSend).Select(x => x.MessagingObject)).GetMessageErrors();
	}

	public new FinalizeAVABREntryMessageSendingActionCollection SendingObjectsCollection =>
		(FinalizeAVABREntryMessageSendingActionCollection)base.SendingObjectsCollection;

	protected override NonPersistentBusinessObjectCollection<MessageSendingAction> GetSendingObjectsCollectionCore()
	{
		var result = new FinalizeAVABREntryMessageSendingActionCollection(Declaration, this);
		result.PopulateElements();
		return result;
	}

	readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions =
	[
		new(nameof(FinalizeAVABREntryMessageSendingAction.DeclarationType), true, 105, Res.GetData("CDCB6E38-1715-4830-AE1E-7991DF8017D8", "Declaration Type")),
		new(nameof(FinalizeAVABREntryMessageSendingAction.Description), true, 200, Res.GetData("D2525C0A-B6AD-4E5F-8F0A-D248435D1C59", "Description")),
		new(nameof(FinalizeAVABREntryMessageSendingAction.EntryStatus), true, 80, Res.GetData("2D61576F-CE4A-4BC7-AB0B-EB93A6529298", "Entry Status")),
		new(nameof(FinalizeAVABREntryMessageSendingAction.RegistrationNumber), true, 123, Res.GetData("CE811248-C6A5-40E4-B98E-12CEFB46503A", "Registration Number")),
	];

	JobDeclaration Declaration => (JobDeclaration)TopLevelBusinessObject;
}
