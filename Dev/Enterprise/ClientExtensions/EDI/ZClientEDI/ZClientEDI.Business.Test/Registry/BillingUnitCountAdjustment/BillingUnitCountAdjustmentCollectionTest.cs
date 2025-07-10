using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingUnitCountAdjustmentCollection))]
	internal class BillingUnitCountAdjustmentCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BillingUnitCountAdjustmentCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BillingUnitCountAdjustmentCollection GetCollectionToTest() => new BillingUnitCountAdjustmentCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new BillingUnitCountAdjustment();
	}
}
