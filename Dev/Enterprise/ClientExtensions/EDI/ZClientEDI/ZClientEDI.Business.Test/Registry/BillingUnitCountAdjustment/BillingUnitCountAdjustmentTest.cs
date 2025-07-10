using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingUnitCountAdjustment))]
	internal class BillingUnitCountAdjustmentTest : RegistryBusinessObjectTemplateTestCase<BillingUnitCountAdjustment>
	{
		public void TestGetClone()
		{
			var adjustment = NewPopulatedBusinessObject();
			adjustment.PriceCode = "P01";
			adjustment.DefaultAdjustedIncrement = 1.4574;
			var setting = adjustment.AdjustmentSettings.AddNew();
			setting.OriginalUnitCount = 2;
			setting.AdjustedUnitCount = 1.5;

			var clone = (BillingUnitCountAdjustment)adjustment.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals("P01", clone.PriceCode);
			AssertEquals(1.4574m, clone.DefaultAdjustedIncrement);
			AssertEquals(1, clone.AdjustmentSettings.Count);
			AssertEquals(2, clone.AdjustmentSettings[0].OriginalUnitCount);
			AssertEquals(1.5m, clone.AdjustmentSettings[0].AdjustedUnitCount);
		}

		public void TestValidation()
		{
			var collection = new BillingUnitCountAdjustmentCollection();
			var adjustment1 = collection.AddNew();
			adjustment1.RunPreSaveValidation();
			AssertHasError(adjustment1.PriceCodeInfo, "Please enter an Usage Code.");
			adjustment1.PriceCode = "P01";
			AssertNoErrors(adjustment1);

			var adjustment2 = collection.AddNew();
			adjustment2.PriceCode = "P01";
			adjustment2.RunPreSaveValidation();
			AssertHasError(adjustment2.PriceCodeInfo, "The Usage Code has been duplicated and must be unique.");
			adjustment2.PriceCode = "P02";
			AssertNoErrors(adjustment2);

			adjustment2.AdjustmentSettings.AddNew(1, 1);
			adjustment2.AdjustmentSettings.AddNew(2, 1.5);
			var s5 = adjustment2.AdjustmentSettings.AddNew(5, 2.6);
			adjustment2.RunPreSaveValidation();
			AssertHasRowError(adjustment2, "Original Values must be in sequential order from 1.");
			s5.OriginalUnitCount = 3;
			adjustment2.RunPreSaveValidation();
			AssertNoRowErrors(adjustment2);
			AssertNoErrors(adjustment2);
		}

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;

		protected override BillingUnitCountAdjustment GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override BillingUnitCountAdjustment GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		BillingUnitCountAdjustment NewPopulatedBusinessObject() => new BillingUnitCountAdjustment(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
	}
}
