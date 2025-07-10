using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuDocumentConfigItemComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var configItemA = Factory.New<StmMenuDocumentConfigItem>();
			configItemA.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItemA.S4_SectionItemName = "AAA";

			var configItemB = Factory.New<StmMenuDocumentConfigItem>();
			configItemB.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItemB.S4_SectionItemName = "AAA";

			var comparer = new StmMenuDocumentConfigItemComparer();
			Assert("comparer.Compare(configItemA, configItemB) == 0", comparer.Compare(configItemA, configItemB) == 0);

			configItemB.S4_SectionItemName = "BBB";
			Assert("comparer.Compare(configItemA, configItemB) == 0", comparer.Compare(configItemA, configItemB) == 0);

			configItemB.S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;
			Assert("comparer.Compare(configItemA, configItemB) > 0", comparer.Compare(configItemA, configItemB) > 0);

			configItemB.S4_SectionType = GenericSectionUsageList.Codes.PageFooterAll;
			Assert("comparer.Compare(configItemA, configItemB) > 0", comparer.Compare(configItemA, configItemB) < 0);
		}

		public void TestCompareWhenTypesAreTheSame()
		{
			var configItemA = Factory.New<StmMenuDocumentConfigItem>();
			configItemA.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItemA.S4_SectionItemName = "AAA";
			configItemA.S4_PrintOrder = 1;

			var configItemB = Factory.New<StmMenuDocumentConfigItem>();
			configItemB.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItemB.S4_SectionItemName = "AAA";
			configItemB.S4_PrintOrder = 2;

			var comparer = new StmMenuDocumentConfigItemComparer();
			Assert("comparer.Compare(configItemA, configItemB) < 0", comparer.Compare(configItemA, configItemB) < 0);
			Assert("comparer.Compare(configItemB, configItemA) > 0", comparer.Compare(configItemB, configItemA) > 0);
		}
	}
}
