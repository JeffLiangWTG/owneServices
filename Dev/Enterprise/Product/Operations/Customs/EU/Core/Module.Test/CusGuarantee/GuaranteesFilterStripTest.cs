using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(GuaranteesFilterStrip))]
	sealed class GuaranteesFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using var filterStrip = CreateGuaranteesFilterStripWithBoilerplate();
			var filterControls = filterStrip.CurrentFilterControls;
			AssertEquals("CurrentFilterControls Count", 1, filterControls.Count);
			AssertType<CusAuthorisationsRuleModuleFilterStrip>("CurrentFilterControl", filterControls[0]);
		}

		public static GuaranteesFilterStrip CreateGuaranteesFilterStripWithBoilerplate()
		{
			var filters = new GuaranteesFilterStripBusinessObject().ModuleFilters;
			var strip = new FilterStrip(filters);
			strip.FilterDescription = CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeRule;
			var filterStrip = new GuaranteesFilterStrip();
			filterStrip.SetDataBinding(strip, "");
			return filterStrip;
		}
	}
}
