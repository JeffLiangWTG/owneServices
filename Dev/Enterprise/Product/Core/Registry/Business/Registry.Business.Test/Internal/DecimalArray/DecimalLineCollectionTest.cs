using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Internal.Testing
{
	[TestedType(typeof(DecimalLineCollection))]
	sealed class DecimalLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DecimalLineCollection>
	{
		public void TestDataType()
		{
			DecimalArrayRegistryDataType dataType = new DecimalArrayRegistryDataType();
			DecimalLineCollection collection = new DecimalLineCollection(dataType);
			AssertEquals("DataType", dataType, collection.DataType);
		}

		public void TestParentCollectionForChild()
		{
			AssertEquals("AddNew().ParentCollection", Collection, Collection.AddNew().ParentCollection);
		}

		public void TestPopulate()
		{
			AssertEquals("Count", 0, Collection.Count);

			Collection.Populate(new decimal[] { 1m, 2.2m });

			AssertEquals("Count", 2, Collection.Count);
			AssertEquals("[0].Number", 1m, Collection[0].Number);
			AssertEquals("[1].Number", 2.2m, Collection[1].Number);
		}

		public void TestToDecimalArray()
		{
			Collection.AddNew().Number = 3.4m;
			Collection.AddNew().Number = 4.5m;
			Collection.AddNew().Number = 6m;

			decimal[] numbers = Collection.ToDecimalArray();

			AssertEquals("Length", 3, numbers.Length);
			AssertEquals("[0]", 3.4m, numbers[0]);
			AssertEquals("[1]", 4.5m, numbers[1]);
			AssertEquals("[2]", 6m, numbers[2]);
		}

		protected override DecimalLineCollection GetCollectionToTest()
		{
			DecimalArrayRegistryDataType dataType = new DecimalArrayRegistryDataType();
			return new DecimalLineCollection(dataType);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DecimalLine(Collection);
		}
	}
}
