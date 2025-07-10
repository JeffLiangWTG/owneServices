using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public abstract class BEJobDeclarationMessageSendingObjectParent<TSendingObjectCollection, TSendingAction> : JobDeclarationMessageSendingObjectParent<TSendingAction>
	where TSendingObjectCollection : BEJobDeclarationMessageSendingObjectCollection<TSendingAction>
	where TSendingAction : BEJobDeclarationMessageSendingObject
{
	public BEJobDeclarationMessageSendingObjectParent(BaseJobDeclaration declaration, ActiveCusEntryHeaderCollection activeEntryHeaders) : base(declaration)
	{
		TopLevelBusinessObject = declaration;
		messagingEntities = activeEntryHeaders;
	}

	public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

	readonly IEnumerable<BusinessObject> messagingEntities;

	public override BusinessObject TopLevelBusinessObject { get; }

	protected override NonPersistentBusinessObjectCollection<TSendingAction> GetSendingObjectsCollectionCore()
	{
		var coll = (TSendingObjectCollection)Activator.CreateInstance(typeof(TSendingObjectCollection), messagingEntities, Factory);
		coll.PopulateElements();
		return coll;
	}

	protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
	{
		return header is Declaration.CusEntryHeader beHeader
			? new JobDeclarationMessageSendingObject(beHeader)
			: base.CreateNewJobDeclarationMessageSendingObject(header);
	}

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties { get; } = new[]
	{
		new MessageSendingObjectProperty(nameof(BEJobDeclarationMessageSendingObject.Variant), true, 80, Res.GetData("C4880062-17AD-4BB2-BD31-B822BA9B5632", "Sub Style")),
		new MessageSendingObjectProperty(nameof(BEJobDeclarationMessageSendingObject.ProcedureType), true, 110, Res.GetData("9EDA10B8-093E-4810-971D-E0821755CE3C", "Declaration Type")),
		new MessageSendingObjectProperty(nameof(BEJobDeclarationMessageSendingObject.Description), true, 200, Res.GetData("2D421CB5-21ED-433B-B610-BF28EBFBEB0D", "Description")),
		new MessageSendingObjectProperty(nameof(BEJobDeclarationMessageSendingObject.EntryStatus), true, 80, Res.GetData("90D7A29A-2683-4E41-AFE5-0D807DFE544F", "Entry Status")),
		new MessageSendingObjectProperty(nameof(BEJobDeclarationMessageSendingObject.TypeOfEntry), true, 80, Res.GetData("E7AB52C8-BB6A-407E-BA87-4861F720F483", "Entry Type"))
	};
}
