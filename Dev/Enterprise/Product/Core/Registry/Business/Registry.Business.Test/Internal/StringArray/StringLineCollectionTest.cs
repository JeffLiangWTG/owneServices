using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Internal.Testing
{
	[TestedType(typeof(StringLineCollection))]
	sealed class StringLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StringLineCollection>
	{
		public void TestDataType()
		{
			AssertEquals("DataType", DataType, Collection.DataType);
		}

		public void TestPopulate()
		{
			AssertEquals("Count", 0, Collection.Count);

			Collection.Populate(new string[] { "hello world", "goodbye world" });

			AssertEquals("Count", 2, Collection.Count);
			AssertEquals("[0].Value", "hello world", Collection[0].Value);
			AssertEquals("[1].Value", "goodbye world", Collection[1].Value);
		}

		public void TestToStringArray()
		{
			Collection.AddNew().Value = "hello world";
			Collection.AddNew().Value = "how are you?";
			Collection.AddNew().Value = "goodbye world";

			string[] strings = Collection.ToStringArray();

			AssertEquals("Length", 3, strings.Length);
			AssertEquals("[0]", "hello world", strings[0]);
			AssertEquals("[1]", "how are you?", strings[1]);
			AssertEquals("[2]", "goodbye world", strings[2]);
		}

		protected override StringLineCollection GetCollectionToTest()
		{
			return new StringLineCollection(DataType);
		}

		StringArrayRegistryDataType DataType
		{
			get { return dataType ?? (dataType = new DelimitedStringArrayRegistryDataType()); }
		}
		StringArrayRegistryDataType dataType;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StringLine(DataType);
		}
	}
}
