using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	[TestedType(typeof(FeatureSetFilterBusinessObject))]
	public class FeatureSetFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new FeatureSetFilterBusinessObject();
		}

		public void TestFilters()
		{
			var obj1 = Factory.NewWithValidTestData<FeatureControlSet>();
			var obj2 = Factory.NewWithValidTestData<FeatureControlSet>();

			obj1.FCS_ProductName = "C001";
			obj2.FCS_ProductName = "C002";

			Factory.Save();

			var filterBizo = new FeatureSetFilterBusinessObject();
			((ModuleTextFilter)filterBizo["Feature Set Name"]).Property = "C002";
			((ModuleTextFilter)filterBizo["Feature Set Name"]).IsActive = true;

			var collection = new FeatureControlSetCollection(Factory);
			collection.AdditionalFilter = filterBizo.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, collection);
		}
	}
}
