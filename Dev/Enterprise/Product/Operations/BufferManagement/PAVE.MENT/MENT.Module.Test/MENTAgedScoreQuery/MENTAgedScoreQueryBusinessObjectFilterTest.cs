using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Module.Test
{
	[TestedType(typeof(MENTAgedScoreQueryFilterBusinessObject))]
	class MENTAgedScoreQueryFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestQueryCode()
		{
			var query1 = MENTTestHelper.CreateQuery(Factory, "WORK");
			var query2 = MENTTestHelper.CreateQuery(Factory, "NONWORK");

			Factory.Save();

			var bizo = new MENTAgedScoreQueryFilterBusinessObject();
			var codeFilterStrip = (ModuleTextFilter)bizo["Code"];
			codeFilterStrip.IsActive = true;
			codeFilterStrip.Property = "WORK";

			var queries = Factory.Load<MENTAgedScoreQuery>(bizo.Filter);
			AssertEquals(1, queries.Length);
			AssertCollectionContains(query1, queries);
		}

		public void TestIsSystemFilter()
		{
			var query1 = MENTTestHelper.CreateQuery(Factory, "SYS");
			query1.MAQ_IsSystem = false;
			var query2 = MENTTestHelper.CreateQuery(Factory, "NONSYS");
			query2.MAQ_IsSystem = true;
			Factory.Save();

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.MENTAgedScoreQuery))
			{
				var bizo = module.FilterBusinessObject;
				var isSystemFilter = (ModuleTextFilter)bizo["Is System Defined"];
				isSystemFilter.IsActive = true;
				isSystemFilter.Property = "System";

				var queries = Factory.Load<MENTAgedScoreQuery>(bizo.Filter);
				AssertEquals(1, queries.Length);
				AssertCollectionContains(query2, queries);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new MENTAgedScoreQueryFilterBusinessObject();
		}
	}
}
