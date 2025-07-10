using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;
public abstract class BEJobDeclarationMessageSendingObjectValidation<TSendingAction> : Customs.Business.JobDeclarationMessageSendingObjectValidation
where TSendingAction : BEJobDeclarationMessageSendingObject
{
	public BEJobDeclarationMessageSendingObjectValidation(TSendingAction action) : base(action)
	{
	}

	protected override void CheckShouldSend()
	{
		base.CheckShouldSend();

		if (Parent.ShouldSend)
		{
			ValidateTypeOfEntry();
		}
	}

	public virtual void ValidateTypeOfEntry()
	{
		Parent.TypeOfEntryInfo.ClearAllNotifications();
		if (Parent.TypeOfEntry.IsEmpty)
		{
			Parent.TypeOfEntryInfo.AddError(Res.GetString("C3971597-5C94-4E46-AD46-046731EED37D", "Type of Entry is mandatory."));
		}
		else
		{
			ListValidation.ErrorIfInvalidCode(Parent.TypeOfEntryInfo);
		}
	}

	protected new TSendingAction Parent => (TSendingAction)base.Parent;
}
