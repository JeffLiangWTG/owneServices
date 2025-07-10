using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(FaxPriceRegistryItem))]
	class FaxPriceRegistryItemTest : StronglyTypedRegistryItemTestCase<FaxPriceCollection>
	{
		public void TestConstructor()
		{
			FaxPriceRegistryItem item = (FaxPriceRegistryItem)GetNewRegistryItem();
			AssertEquals("FaxPrice", item.Name);
			AssertEquals("Pricelist", item.Category);
			AssertEquals("Faxing Service Prices", item.Caption);
			AssertEquals("Please specify fax page rate for each currency in use.", item.Hint);
			AssertEquals(typeof(FaxPriceDataType), item.DataType.GetType());
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		protected override StronglyTypedRegistryItem<FaxPriceCollection, FaxPriceCollection> GetNewRegistryItem()
		{
			return new FaxPriceRegistryItem("Pricelist");
		}
	}
}
