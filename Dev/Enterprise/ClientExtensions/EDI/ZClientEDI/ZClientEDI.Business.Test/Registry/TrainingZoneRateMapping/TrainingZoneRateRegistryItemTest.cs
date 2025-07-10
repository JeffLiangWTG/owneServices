using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TrainingZoneRateRegistryItem))]
	class TrainingZoneRateRegistryItemTest : StronglyTypedRegistryItemTestCase<TrainingZoneRateCollection>
	{
		public void TestConstructor()
		{
			TrainingZoneRateRegistryItem item = (TrainingZoneRateRegistryItem)GetNewRegistryItem();
			AssertEquals("TrainingZoneRate", item.Name);
			AssertEquals("Training", item.Category);
			AssertEquals("Training Zone Rates", item.Caption);
			AssertEquals("Please specify rates for training zones", item.Hint);
			AssertEquals(typeof(TrainingZoneRateDataType), item.DataType.GetType());
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		protected override StronglyTypedRegistryItem<TrainingZoneRateCollection, TrainingZoneRateCollection> GetNewRegistryItem()
		{
			return new TrainingZoneRateRegistryItem("Training");
		}
	}
}
