using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.AdditionalHouseBillOfLadingTypeCollectionRegistryItem;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AdditionalHouseBillOfLadingTypeCollectionRegistryDataType))]
	sealed class AdditionalHouseBillOfLadingTypeCollectionRegistryDataTypeFallBackMergedTest : FallbackMergedRegistryBusinessObjectCollectionDataTypeTestCase
	{
		protected override string ExpectedEditorName => "AdditionalHouseBillOfLadingTypeCollectionRegistryItemEditor";

		protected override IRegistryDataType GetNewDataType()
		{
			return new AdditionalHouseBillOfLadingTypeCollectionRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new AdditionalHouseBillOfLadingTypeCollection();
			var item = collection.AddNew();
			item.Code = "abc";

			var collection2 = new AdditionalHouseBillOfLadingTypeCollection();
			var item2 = collection.AddNew();
			item2.Code = "aec";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new AdditionalHouseBillOfLadingTypeCollectionRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new AdditionalHouseBillOfLadingTypeCollectionRegistryDataType().Serialise(collection2))
			};
		}
	}
}
