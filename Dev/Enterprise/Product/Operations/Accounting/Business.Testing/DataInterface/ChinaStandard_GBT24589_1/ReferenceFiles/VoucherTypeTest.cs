using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(VoucherType))]
	public class VoucherTypeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassConstProperties()
		{
			AssertEquals("T103", VoucherType.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VoucherType();
		}
	}

	[TestedType(typeof(VoucherTypeCollection))]
	public class VoucherTypeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VoucherTypeCollection>
	{
		public void TestDefaultValues()
		{
			VoucherTypeCollection collection = new VoucherTypeCollection(0, Factory);
			AssertEquals(0, collection.Count);
			collection = new VoucherTypeCollection(1, Factory);
			AssertEquals(1, collection.Count);
			AssertEquals("1", collection[0].VoucherTypeNumber);
			AssertEquals("记账凭证", collection[0].VoucherTypeName);
			AssertEquals("记", collection[0].VoucherTypeAbbreviation);
			collection = new VoucherTypeCollection(3, Factory);
			AssertEquals(3, collection.Count);
			AssertEquals("1", collection[0].VoucherTypeNumber);
			AssertEquals("收款凭证", collection[0].VoucherTypeName);
			AssertEquals("收", collection[0].VoucherTypeAbbreviation);
			AssertEquals("2", collection[1].VoucherTypeNumber);
			AssertEquals("付款凭证", collection[1].VoucherTypeName);
			AssertEquals("付", collection[1].VoucherTypeAbbreviation);
			AssertEquals("3", collection[2].VoucherTypeNumber);
			AssertEquals("转账凭证", collection[2].VoucherTypeName);
			AssertEquals("转", collection[2].VoucherTypeAbbreviation);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new VoucherType();
		}

		protected override VoucherTypeCollection GetCollectionToTest()
		{
			return new VoucherTypeCollection(0, Factory);
		}
	}
}
