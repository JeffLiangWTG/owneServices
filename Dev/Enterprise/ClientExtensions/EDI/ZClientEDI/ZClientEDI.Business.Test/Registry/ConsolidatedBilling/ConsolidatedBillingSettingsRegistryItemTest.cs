using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ConsolidatedBillingSettingsRegistryItem))]
	public class ConsolidatedBillingSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<ConsolidatedBillingSettingCollection, ConsolidatedBillingSettingCollection>
	{
		protected override StronglyTypedRegistryItem<ConsolidatedBillingSettingCollection, ConsolidatedBillingSettingCollection> GetNewRegistryItem()
		{
			return new ConsolidatedBillingSettingsRegistryItem(
				"ConsolidatedBillingSettings",
				(NoResString)"LicenceBillingCategory",
				(NoResString)"Products (non-CW1) Enabled for Consolidated Billing",
				(NoResString)$"This will deliver the 'Products (non-CW1) Enabled for Consolidated Billing' registry.\r\nThis will allow multiplie databases to be consolidated into a single billing summary.",
				Integration.RegistryStorageFlags.System
				);
		}
	}

	[TestedType(typeof(ConsolidatedBillingSettingsRegistryDataType))]
	public class ConsolidatedBillingSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ConsolidatedBillingSettingsRegistryDataType>
	{
		protected override ConsolidatedBillingSettingsRegistryDataType GetNewDataType()
		{
			return new ConsolidatedBillingSettingsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new ConsolidatedBillingSettingCollection(null, null);
			collection1.Add(new ConsolidatedBillingSetting()
			{
				ProductCode = "ENT",
				Description = "123456789"
			});

			var collection2 = new ConsolidatedBillingSettingCollection(null, null);
			collection2.Add(new ConsolidatedBillingSetting()
			{
				ProductCode = "ENT",
				Description = "987654321"
			});

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2)),
			};
		}

		protected override string ExpectedEditorName => "ConsolidatedBillingSettingsRegistryEditor";
	}
}
