using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class DeclarationMessageSendingObjectParent<T> : JobDeclarationMessageSendingObjectParent<T>, IMessageSendingObjectParent where T : DeclarationMessageSendingObject
{
	public DeclarationMessageSendingObjectParent(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

	public ZString CanSendMessage()
	{
		var result = CheckMessageSendingEnvironment();
		if (result.IsEmpty)
		{
			result = CheckDeniedParty(ParentDeclaration);
		}
		return result;
	}

	protected abstract ZString CheckMessageSendingEnvironment();

	ZString CheckDeniedParty(BaseJobDeclaration dec)
	{
		var creditCheckManager = new MessageManagerCreditCheckWithSecurityHelper(dec);
		return creditCheckManager.IsDeniedPartyOKToSend ? string.Empty : creditCheckManager.ReasonForNotAllowed;
	}

	public void UpdateSendingObjectsBeforeSending() { }

	IEnumerable<IMessageSendingObject> IMessageSendingObjectParent.SelectedSendingObjects => SelectedSendingObjects.Cast<IMessageSendingObject>();
}
