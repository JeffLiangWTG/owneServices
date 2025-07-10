using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACusRulingFilterStripBusinessObject))]
	sealed class CACusRulingFilterStripBusinessObjectTest : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		public void TestFiltersDescription()
		{
			var filterStrip = new CACusRulingFilterStripBusinessObject();
			var rulingNumber = (ModuleTextFilter)filterStrip[ZZRefCusRulingFilters.RulingNumber];
			AssertEquals("Remission Number", rulingNumber.MultilingualDescription);

			var rulingType = (ModuleTextFilter)filterStrip[ZZRefCusRulingFilters.RulingType];
			AssertEquals("Remission Type", rulingType.MultilingualDescription);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CACusRulingFilterStripBusinessObject();
	}
}
