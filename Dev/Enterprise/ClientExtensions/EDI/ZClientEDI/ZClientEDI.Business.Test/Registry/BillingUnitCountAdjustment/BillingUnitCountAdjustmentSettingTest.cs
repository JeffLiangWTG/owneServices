using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingUnitCountAdjustmentSetting))]
	internal class BillingUnitCountAdjustmentSettingTest : RegistryBusinessObjectTemplateTestCase<BillingUnitCountAdjustmentSetting>
	{
		public void TestGetClone()
		{
			var setting = new BillingUnitCountAdjustmentSetting();
			setting.OriginalUnitCount = 2;
			setting.AdjustedUnitCount = 1.5;

			var clone = (BillingUnitCountAdjustmentSetting)setting.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals(2, clone.OriginalUnitCount);
			AssertEquals(1.5m, clone.AdjustedUnitCount);
		}

		public void TestValidation()
		{
			var collection = new BillingUnitCountAdjustmentSettingCollection();
			var setting1 = collection.AddNew();
			setting1.OriginalUnitCount = 1;
			setting1.AdjustedUnitCount = 0.5;
			var setting2 = collection.AddNew();
			setting2.OriginalUnitCount = 1;
			setting2.AdjustedUnitCount = 0.8;

			setting2.RunPreSaveValidation();
			AssertHasError(setting2.OriginalUnitCountInfo, "The Original Usage Count has been duplicated and must be unique.");
			setting2.OriginalUnitCount = 2;
			AssertNoErrors(setting2);
		}

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;

		protected override BillingUnitCountAdjustmentSetting GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override BillingUnitCountAdjustmentSetting GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		BillingUnitCountAdjustmentSetting NewPopulatedBusinessObject() => new BillingUnitCountAdjustmentSetting(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
	}
}
