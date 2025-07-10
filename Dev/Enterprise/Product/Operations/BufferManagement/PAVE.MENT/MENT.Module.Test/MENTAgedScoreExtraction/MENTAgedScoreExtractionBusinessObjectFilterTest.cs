using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Module.Test
{
	[TestedType(typeof(MENTAgedScoreExtractionFilterBusinessObject))]
	class MENTAgedScoreExtractionBusinessObjectFilterTest : FilterStripBusinessObjectTestCase
	{
		public void TestExtractionName()
		{
			var extraction1 = MENTTestHelper.CreateExtraction(Factory, "AAAA", queryCode: "A Query");
			var extraction2 = MENTTestHelper.CreateExtraction(Factory, "BBBB", queryCode: "B Query");

			Factory.Save();

			var bizo = new MENTAgedScoreExtractionFilterBusinessObject();
			var nameFilterStrip = (ModuleTextFilter)bizo["Name"];
			nameFilterStrip.IsActive = true;
			nameFilterStrip.Property = "AAAA";

			var extractions = Factory.Load<MENTAgedScoreExtraction>(bizo.Filter);
			AssertEquals(1, extractions.Length);
			AssertCollectionContains(extraction1, extractions);
		}

		public void TestRelatedQuery()
		{
			var query1 = MENTTestHelper.CreateQuery(Factory, "q1");
			var query2 = MENTTestHelper.CreateQuery(Factory, "r1");

			var extraction1 = MENTTestHelper.CreateExtraction(Factory, "abc", query1);
			var extraction2 = MENTTestHelper.CreateExtraction(Factory, "abc", query2);

			Factory.Save();

			var bizo = new MENTAgedScoreExtractionFilterBusinessObject();
			var queryFilterStrip = (ModuleGuidFilter)bizo["Related Query"];
			queryFilterStrip.IsActive = true;
			queryFilterStrip.Property = query1.PK;

			var extractions = Factory.Load<MENTAgedScoreExtraction>(bizo.Filter);
			AssertEquals(1, extractions.Length);
			AssertCollectionContains(extraction1, extractions);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new MENTAgedScoreExtractionFilterBusinessObject();
		}
	}
}
