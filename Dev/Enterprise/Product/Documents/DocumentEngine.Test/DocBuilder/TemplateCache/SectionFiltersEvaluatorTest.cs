using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class SectionFiltersEvaluatorTest : TestCaseWithFactory
	{
		public void TestBuildEntireHash()
		{
			var config = Factory.New<StmMenuDocumentConfig>();
			var item1 = Factory.New<StmMenuDocumentConfigItem>();
			item1.S4_FilterList = "<OH_Code>";
			item1.S4_SectionItemName = "section1";
			var item2 = Factory.New<StmMenuDocumentConfigItem>();
			item2.S4_FilterList = "<OH_FullName>";
			item2.S4_SectionItemName = "section2";
			config.ConfigItems.AddRange(item1, item2);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "Aybeecee";

			var evaluator = new FilterEvaluator(BODocDataProvider.Get(org));
			Assert(SectionFiltersEvaluator.GetEvaluatedSectionFilters(config, evaluator).SequenceEqual(new string[] { "section1+False", "section2+False" }));
		}

		public void TestBuildHashByConfigItem()
		{
			var config = Factory.New<StmMenuDocumentConfig>();
			var item1 = Factory.New<StmMenuDocumentConfigItem>();
			item1.S4_SectionItemName = "section1";
			item1.S4_FilterList = "<OH_Code>";
			var item2 = Factory.New<StmMenuDocumentConfigItem>();
			item2.S4_SectionItemName = "section2";
			item2.S4_FilterList = "<OH_FullName>";
			config.ConfigItems.AddRange(item1, item2);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "Aybeecee";

			var evaluator = new FilterEvaluator(BODocDataProvider.Get(org));
			var builder = new SectionFiltersEvaluator(evaluator);

			builder.EvaluateConfigItem(item1);
			builder.EvaluateConfigItem(item2);

			Assert(builder.GetEvaluatedSectionFilters().SequenceEqual(new string[] { "section1+False", "section2+False" }));
			Assert("Evaluated items should be the same either way they are built", builder.GetEvaluatedSectionFilters().SequenceEqual(SectionFiltersEvaluator.GetEvaluatedSectionFilters(config, evaluator)));
		}

		public void TestHashAreSameForDifferentConfigItemsWithSameEvaluatedValue()
		{
			var config = Factory.New<StmMenuDocumentConfig>();
			var item1 = Factory.New<StmMenuDocumentConfigItem>();
			item1.S4_FilterList = "\"<OH_Code>\" != \"123\"";
			item1.S4_SectionItemName = "section1";
			config.ConfigItems.Add(item1);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ABD";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "123";

			var evaluator1 = new FilterEvaluator(BODocDataProvider.Get(org));
			var evaluatedKeys1 = SectionFiltersEvaluator.GetEvaluatedSectionFilters(config, evaluator1).ToList();
			var evaluator2 = new FilterEvaluator(BODocDataProvider.Get(org2));
			var evaluatedKeys2 = SectionFiltersEvaluator.GetEvaluatedSectionFilters(config, evaluator2).ToList();
			AssertEquals(evaluatedKeys1[0], evaluatedKeys2[0]);

			var evaluator3 = new FilterEvaluator(BODocDataProvider.Get(org3));
			var evaluatedKeys3 = SectionFiltersEvaluator.GetEvaluatedSectionFilters(config, evaluator3).ToList();
			AssertNotEquals(evaluatedKeys1[0], evaluatedKeys3[0]);
		}
	}
}
