using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(OpportunityValueAnalysisDefaultRegistryItem))]
	class OpportunityValueAnalysisDefaultRegistryItemTest : StronglyTypedRegistryItemTestCase<OpportunityValueAnalysisDefaultCollection>
	{
		public void TestConstructor()
		{
			OpportunityValueAnalysisDefaultRegistryItem item = (OpportunityValueAnalysisDefaultRegistryItem)GetNewRegistryItem();
			AssertEquals("OpportunityValueAnalysisDefault", item.Name);
			AssertEquals("ABC", item.Category);
			AssertEquals("Opportunity Value Analysis defaults", item.Caption);
			AssertEquals("Opportunity Value Analysis defaults", item.Hint);
			AssertEquals(typeof(OpportunityValueAnalysisDefaultDataType), item.DataType.GetType());
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		protected override StronglyTypedRegistryItem<OpportunityValueAnalysisDefaultCollection, OpportunityValueAnalysisDefaultCollection> GetNewRegistryItem()
		{
			return new OpportunityValueAnalysisDefaultRegistryItem("ABC");
		}
	}
}
