using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ExRateType))]
	public class ExRateTypeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T104", ExRateType.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExRateType();
		}
	}

	[TestedType(typeof(ExRateTypeCollection))]
	public class ExRateTypeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExRateTypeCollection>
	{
		public void TestDefaultValues()
		{
			ExRateTypeCollection collection = new ExRateTypeCollection(Factory);
			AssertEquals(2, collection.Count);
			AssertEquals("1", collection[0].ExRateTypeNumber);
			AssertEquals("买入汇率", collection[0].ExRateTypeName);
			AssertEquals("2", collection[1].ExRateTypeNumber);
			AssertEquals("卖出汇率", collection[1].ExRateTypeName);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExRateType();
		}

		protected override ExRateTypeCollection GetCollectionToTest()
		{
			return new ExRateTypeCollection(Factory);
		}
	}
}
