using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

public class BEJobDeclarationMessageSendingObjectParentForTesting : BEJobDeclarationMessageSendingObjectParent<BEJobDeclarationMessageSendingObjectCollectionForTesting, BEJobDeclarationMessageSendingObjectForTesting>
{
	public BEJobDeclarationMessageSendingObjectParentForTesting(BaseJobDeclaration declaration) : base(declaration, declaration.ActiveEntryHeaders) { }
}
