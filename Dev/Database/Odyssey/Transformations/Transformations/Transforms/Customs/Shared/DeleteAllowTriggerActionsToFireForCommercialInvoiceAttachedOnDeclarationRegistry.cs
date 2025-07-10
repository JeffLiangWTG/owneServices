using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	sealed class DeleteAllowTriggerActionsToFireForCommercialInvoiceAttachedOnDeclarationRegistry : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "AllowTriggerActionsToFireForCommercialInvoiceAttachedOnDeclaration" };
		}
	}
}
