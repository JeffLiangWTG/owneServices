using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class SupportingDocSendingObjectParent : JobDeclarationSupportingDocSendingObjectParent, IMessageSendingObjectParent
{
	public SupportingDocSendingObjectParent(BaseJobDeclaration declaration) : base(declaration)
	{
	}

	IEnumerable<IMessageSendingObject> IMessageSendingObjectParent.SelectedSendingObjects => SelectedSendingObjects.Cast<IMessageSendingObject>();

	public ZString CanSendMessage() => string.Empty;

	public void UpdateSendingObjectsBeforeSending() { }
}
