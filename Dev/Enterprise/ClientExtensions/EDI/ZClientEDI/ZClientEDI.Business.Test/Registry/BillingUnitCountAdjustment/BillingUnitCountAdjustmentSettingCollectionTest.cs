using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingUnitCountAdjustmentSettingCollection))]
	internal class BillingUnitCountAdjustmentSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BillingUnitCountAdjustmentSettingCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BillingUnitCountAdjustmentSettingCollection GetCollectionToTest() => new BillingUnitCountAdjustmentSettingCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new BillingUnitCountAdjustmentSetting();
	}
}
