using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class JobDeclarationMessageSendingObjectWithNullSubTypeForTest : JobDeclarationMessageSendingObject
{
	public JobDeclarationMessageSendingObjectWithNullSubTypeForTest(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		: base(header, jobDeclarationMessageSendingObjectParent)
	{
	}

	protected override ZString GetMessageSubType() => null;
}
