using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexDurationRegistryItem))]
	sealed class IndexDurationListRegistryItemTest : StronglyTypedRegistryItemTestCase<IndexDurationList>
	{
		StronglyTypedRegistryItem<IndexDurationList, IndexDurationList> GetRegistryItemWithDefaultValue()
		{
			return new IndexDurationRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, new IndexDurationList { new IndexDuration() { Table = "CarrierShipmentHeader", DurationInMonths = 5 } });
		}

		public void TestRegistryDefaultIndexDurationList()
		{
			var list = GetRegistryItemWithDefaultValue().DefaultValue;
			AssertEquals(1, list.Count);
			var indexDuration = list[0];
			AssertEquals("Default Table", "CarrierShipmentHeader", indexDuration.Table);
			AssertEquals("Default Duration In Months", 5, indexDuration.DurationInMonths);
		}

		protected override StronglyTypedRegistryItem<IndexDurationList, IndexDurationList> GetNewRegistryItem()
		{
			var defaultCollection = new IndexDurationList();
			return new IndexDurationRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
