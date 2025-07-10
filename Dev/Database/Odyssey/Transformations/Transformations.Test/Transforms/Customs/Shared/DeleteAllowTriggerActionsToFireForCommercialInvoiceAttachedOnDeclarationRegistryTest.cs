using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(DeleteAllowTriggerActionsToFireForCommercialInvoiceAttachedOnDeclarationRegistry))]
	sealed class DeleteAllowTriggerActionsToFireForCommercialInvoiceAttachedOnDeclarationRegistryTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "AllowTriggerActionsToFireForCommercialInvoiceAttachedOnDeclaration" };
		}
	}
}
