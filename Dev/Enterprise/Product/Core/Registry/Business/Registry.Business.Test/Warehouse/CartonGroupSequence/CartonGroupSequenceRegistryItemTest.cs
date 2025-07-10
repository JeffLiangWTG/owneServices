using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CartonGroupSequenceRegistryItem))]
	sealed class CartonGroupSequenceRegistryItemTest : StronglyTypedRegistryItemTestCase<CartonGroupSequence>
	{
		protected override StronglyTypedRegistryItem<CartonGroupSequence, CartonGroupSequence> GetNewRegistryItem()
		{
			return new CartonGroupSequenceRegistryItem("TestName", (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Branch);
		}

		public void TestConstructorParamsGetPassedThroughCorrectly()
		{
			var stronglyTypedRegistryItem = GetNewRegistryItem();
			AssertEquals("TestName", stronglyTypedRegistryItem.Name);
			AssertEquals("TestCategory", stronglyTypedRegistryItem.Category);
			AssertEquals("TestCaption", stronglyTypedRegistryItem.Caption);
			AssertEquals("TestHint", stronglyTypedRegistryItem.Hint);
			AssertEquals(RegistryStorageFlags.Branch, stronglyTypedRegistryItem.Storage);
		}
	}
}
