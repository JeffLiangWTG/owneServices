using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(DefinedDataItemValue))]
	public class DefinedDataItemValueTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T112", DefinedDataItemValue.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DefinedDataItemValue();
		}
	}

	[TestedType(typeof(DefinedDataItemValueCollection))]
	public class DefinedDataItemValueCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DefinedDataItemValueCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefinedDataItemValue();
		}

		protected override DefinedDataItemValueCollection GetCollectionToTest()
		{
			return new DefinedDataItemValueCollection(Factory);
		}
	}
}
