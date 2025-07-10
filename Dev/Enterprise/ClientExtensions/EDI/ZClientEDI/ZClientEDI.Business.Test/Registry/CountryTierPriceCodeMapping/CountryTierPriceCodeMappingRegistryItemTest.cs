using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CountryTierPriceCodeMappingRegistryItem))]
	class CountryTierPriceCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<CountryTierPriceCodeMappingCollection, CountryTierPriceCodeMappingCollection>
	{
		protected override StronglyTypedRegistryItem<CountryTierPriceCodeMappingCollection, CountryTierPriceCodeMappingCollection> GetNewRegistryItem()
		{
			return new CountryTierPriceCodeMappingRegistryItem("CountryTierPriceCodeMappings",
							(NoResString)EDIDataRegistry.StlBillingCategory,
							(NoResString)"Country Tier Price Code Mapping",
							(NoResString)"This registry allows a user to assign a Country Tier Code against a Price Code for STL usage that requires a Country Tiered Pricing Structure.",
							new CountryTierPriceCodeMappingRegistryEditorInfo(),
							RegistryStorageFlags.System);
		}

		protected override CountryTierPriceCodeMappingCollection ValidValue
		{
			get
			{
				var collection = new CountryTierPriceCodeMappingCollection();
				var mapping1 = collection.AddNew();
				mapping1.PriceCode = "P01";
				mapping1.SystemCode = "X01";
				var mappingLine1 = mapping1.MappingLines.AddNew();
				mappingLine1.CountryCode = "AU";
				mappingLine1.CountryTierCode = "C01";

				var mapping2 = collection.AddNew();
				mapping2.PriceCode = "P02";
				mapping2.SystemCode = "X02";
				var mappingLine2 = mapping2.MappingLines.AddNew();
				mappingLine2.CountryCode = "NZ";
				mappingLine2.CountryTierCode = "C02";
				mapping2.MappingLines.AddNew("US", "C01");
				mapping2.MappingLines.AddNew("SA", "C01");

				return collection;
			}
		}
	}

	[TestedType(typeof(CountryTierPriceCodeMappingRegistryDataType))]
	class CountryTierPriceCodeMappingDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CountryTierPriceCodeMappingRegistryDataType>
	{
		protected override CountryTierPriceCodeMappingRegistryDataType GetNewDataType()
		{
			return new CountryTierPriceCodeMappingRegistryDataType();
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
			var collection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = collection.AddNew();
			mapping1.PriceCode = "P01";
			mapping1.SystemCode = "X01";
			var mappingLine1 = mapping1.MappingLines.AddNew();
			mappingLine1.CountryCode = "AU";
			mappingLine1.CountryTierCode = "C01";

			var mapping2 = collection.AddNew();
			mapping2.PriceCode = "P02";
			mapping2.SystemCode = "X02";
			var mappingLine2 = mapping2.MappingLines.AddNew();
			mappingLine2.CountryCode = "NZ";
			mappingLine2.CountryTierCode = "C02";
			mapping2.MappingLines.AddNew("US", "C01");
			mapping2.MappingLines.AddNew("SA", "C01");

			var collection2 = new CountryTierPriceCodeMappingCollection();
			var mapping3 = collection.AddNew();
			mapping3.PriceCode = "Z01";
			mapping3.SystemCode = "Y01";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2)),
			};
		}
	}
}
