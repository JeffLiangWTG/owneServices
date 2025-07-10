using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;
public abstract class BEJobDeclarationMessageSendingObjectLookups<TSendingAction> : ZLookups
where TSendingAction : BEJobDeclarationMessageSendingObject
{
	public BEJobDeclarationMessageSendingObjectLookups(TSendingAction action) : base(action)
	{
	}

	protected new TSendingAction Parent => (TSendingAction)base.Parent;

	public virtual CodeDescriptionPairList EntryTypeList => null;
}
