using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingUnitCountAdjustmentRegistryItem))]
	class BillingUnitCountAdjustmentRegistryItemTest : StronglyTypedRegistryItemTestCase<BillingUnitCountAdjustmentCollection, BillingUnitCountAdjustmentCollection>
	{
		protected override StronglyTypedRegistryItem<BillingUnitCountAdjustmentCollection, BillingUnitCountAdjustmentCollection> GetNewRegistryItem()
		{
			return new BillingUnitCountAdjustmentRegistryItem("BillingUnitCountAdjustments",
							(NoResString)"LicenceBillingCategory",
							(NoResString)"Billing Adjusted Usage Counts",
							(NoResString)"Setup the adjusted usage count for specific usage codes.\r\n\r\nNote, the Subsequent Adjusted Usage Increment value will be applied when a transactions usage count exceeds the maximum setting in this registry item.\r\n\r\nFor example, the default configuration for the WTU usage code covers usage counts from 1 to 50. When a transaction incurs additional usage from 51 onwards, each subsequent usage increment will be charged an additional 0.245 of adjusted usage.\r\n\r\nAfter a change has been applied to a Usage Code's setup, transactions received prior to the change will need to have their Adjusted Usage Counts re-calculated. Please create a CR9 request, Module = BIL.",
							new BillingUnitCountAdjustmentRegistryEditorInfo(),
							RegistryStorageFlags.System,
							new BillingUnitCountAdjustmentCollection());
		}

		protected override BillingUnitCountAdjustmentCollection ValidValue
		{
			get
			{
				var collection = new BillingUnitCountAdjustmentCollection();
				var adjustment = collection.AddNew();
				adjustment.PriceCode = "P01";
				adjustment.DefaultAdjustedIncrement = 1.3456;
				var setting = adjustment.AdjustmentSettings.AddNew();
				setting.OriginalUnitCount = 1;
				setting.AdjustedUnitCount = 1.5;

				var adjustment2 = collection.AddNew();
				adjustment2.PriceCode = "P02";
				adjustment2.DefaultAdjustedIncrement = 4.6741;
				var setting2 = adjustment2.AdjustmentSettings.AddNew();
				setting2.OriginalUnitCount = 1;
				setting2.AdjustedUnitCount = 1.5;
				adjustment2.AdjustmentSettings.AddNew(2, 2.3);
				adjustment2.AdjustmentSettings.AddNew(3, 3.3);

				return collection;
			}
		}
	}

	[TestedType(typeof(BillingUnitCountAdjustmentRegistryDataType))]
	class BillingUnitCountAdjustmentDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BillingUnitCountAdjustmentRegistryDataType>
	{
		protected override BillingUnitCountAdjustmentRegistryDataType GetNewDataType()
		{
			return new BillingUnitCountAdjustmentRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = GetTestCollection(1);
			var sample2 = GetTestCollection(5);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, DataType.Serialise(sample)),
				new ValidSampleAndBinaryValueInDB(sample2, DataType.Serialise(sample2)),
			};
		}

		BillingUnitCountAdjustmentCollection GetTestCollection(int idx)
		{
			var collection = new BillingUnitCountAdjustmentCollection();
			var adjustment = collection.AddNew();
			adjustment.PriceCode = $"P{idx}";

			for (var curIdx = 1; curIdx <= idx; curIdx++)
			{
				adjustment.AdjustmentSettings.AddNew(curIdx, curIdx * 0.5);
			}

			var adjustment2 = collection.AddNew();
			adjustment2.PriceCode = $"P{idx + 1}";
			for (var curIdx = 1; curIdx <= idx + 1; curIdx++)
			{
				adjustment2.AdjustmentSettings.AddNew(curIdx, curIdx * 0.3);
			}

			return collection;
		}
	}
}
