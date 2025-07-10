using CusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.BE.Business.Testing;

public class BEJobDeclarationMessageSendingObjectForTesting : BEJobDeclarationMessageSendingObject
{
	public BEJobDeclarationMessageSendingObjectForTesting(CusEntryHeader header) : base(header) { }
}
