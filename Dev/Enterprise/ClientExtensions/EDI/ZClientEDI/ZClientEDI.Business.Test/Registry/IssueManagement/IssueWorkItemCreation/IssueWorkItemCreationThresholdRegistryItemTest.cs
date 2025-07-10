using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.IssueWorkItemCreationThresholdRegistryItem;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IssueWorkItemCreationThresholdRegistryItem))]
	class IssueWorkItemCreationThresholdRegistryItemTest : StronglyTypedRegistryItemTestCase<IssueWorkItemCreationThresholdCollection>
	{
		protected override StronglyTypedRegistryItem<IssueWorkItemCreationThresholdCollection, IssueWorkItemCreationThresholdCollection> GetNewRegistryItem()
		{
			return new IssueWorkItemCreationThresholdRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}
	}

	[TestedType(typeof(IssueWorkItemCreationThresholdRegistryDataType))]
	class IssueWorkItemCreationThresholdRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IssueWorkItemCreationThresholdRegistryDataType>
	{
		protected override string ExpectedEditorName => "IssueWorkItemCreationThresholdRegistryEditor";
		protected override IssueWorkItemCreationThresholdRegistryDataType GetNewDataType()
		{
			return new IssueWorkItemCreationThresholdRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new IssueWorkItemCreationThresholdCollection();
			var item = collection.AddNew();
			item.ThresholdTimespan = 3;
			item.IssueOccurrenceThreshold = 2;

			var collection2 = new IssueWorkItemCreationThresholdCollection();
			var item2 = collection2.AddNew();
			item2.ThresholdTimespan = 5;
			item2.IssueOccurrenceThreshold = 3;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new IssueWorkItemCreationThresholdRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new IssueWorkItemCreationThresholdRegistryDataType().Serialise(collection2))
			};
		}
	}
}
