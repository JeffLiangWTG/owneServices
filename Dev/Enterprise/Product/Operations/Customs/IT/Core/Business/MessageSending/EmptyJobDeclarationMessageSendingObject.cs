using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class EmptyJobDeclarationMessageSendingObject : JobDeclarationMessageSendingObject
{
	public EmptyJobDeclarationMessageSendingObject(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(header, jobDeclarationMessageSendingObjectParent)
	{
	}

	protected override ZString GetMessageSubType() => ZString.Empty;
}
