using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(DefinedDataItem))]
	public class DefinedDataItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T111", DefinedDataItem.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DefinedDataItem();
		}
	}

	[TestedType(typeof(DefinedDataItemCollection))]
	public class DefinedDataItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DefinedDataItemCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefinedDataItem();
		}

		protected override DefinedDataItemCollection GetCollectionToTest()
		{
			return new DefinedDataItemCollection(Factory);
		}
	}
}
