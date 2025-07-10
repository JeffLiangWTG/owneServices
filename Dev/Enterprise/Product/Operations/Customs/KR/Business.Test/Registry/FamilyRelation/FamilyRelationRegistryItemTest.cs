using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FamilyRelationRegistryItem))]
	public class FamilyRelationRegistryItemTest : StronglyTypedRegistryItemTestCase<FamilyRelationCollection>
	{
		protected override StronglyTypedRegistryItem<FamilyRelationCollection, FamilyRelationCollection> GetNewRegistryItem()
		{
			return new FamilyRelationRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new FamilyRelationCollection());
		}
	}

	[TestedType(typeof(FamilyRelationRegistryDataType))]
	public class FamilyRelationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FamilyRelationRegistryDataType>
	{
		protected override FamilyRelationRegistryDataType GetNewDataType() => new FamilyRelationRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new FamilyRelationCollection();
			var item = collection.AddNew();
			item.Code = "11";
			item.Description = "당숙부";

			var collection2 = new FamilyRelationCollection();
			var item2 = collection2.AddNew();
			item2.Code = "12";
			item2.Description = "당숙모";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new FamilyRelationRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new FamilyRelationRegistryDataType().Serialise(collection2))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "FamilyRelationRegistryItemEditor"; }
		}
	}
}
