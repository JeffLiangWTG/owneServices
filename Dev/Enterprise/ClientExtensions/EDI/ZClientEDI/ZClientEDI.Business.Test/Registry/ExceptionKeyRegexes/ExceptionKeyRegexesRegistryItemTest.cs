using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.Registry.Business.ExceptionKeyRegexesRegistryItem;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ExceptionKeyRegexesRegistryItem))]
	public class ExceptionKeyRegexesRegistryItemTest : StronglyTypedRegistryItemTestCase<ExceptionKeyRegexCollection>
	{
		public void TestDefaultValue()
		{
			ExceptionKeyRegexCollection collection = new ExceptionKeyRegexCollection();
			ExceptionKeyRegex regex1 = collection.AddNew();
			regex1.Regex = "Test Regex 1";
			ExceptionKeyRegex regex2 = collection.AddNew();
			regex2.Regex = "Test Regex 2";

			ExceptionKeyRegexesRegistryItem item = new ExceptionKeyRegexesRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached, collection);
			AssertEquals(2, item.DefaultValue.Count);
		}

		protected override StronglyTypedRegistryItem<ExceptionKeyRegexCollection, ExceptionKeyRegexCollection> GetNewRegistryItem()
		{
			return new ExceptionKeyRegexesRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}
	}

	[TestedType(typeof(ExceptionKeyRegexesRegistryDataType))]
	class ExceptionKeyRegexesRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ExceptionKeyRegexesRegistryDataType>
	{
		protected override string ExpectedEditorName => "ExceptionKeyRegexesRegistryEditor";
		protected override ExceptionKeyRegexesRegistryDataType GetNewDataType()
		{
			return new ExceptionKeyRegexesRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ExceptionKeyRegexCollection();
			var item = collection.AddNew();
			item.Regex = "test";

			var collection2 = new ExceptionKeyRegexCollection();
			var item2 = collection.AddNew();
			item2.Regex = "regex";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new ExceptionKeyRegexesRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new ExceptionKeyRegexesRegistryDataType().Serialise(collection2))
			};
		}
	}
}
