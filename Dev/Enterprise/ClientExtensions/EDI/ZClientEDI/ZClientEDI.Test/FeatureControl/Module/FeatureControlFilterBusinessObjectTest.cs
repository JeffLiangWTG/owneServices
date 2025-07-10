using System;
using System.Collections.Generic;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	[TestedType(typeof(FeatureControlFilterBusinessObject))]
	public class FeatureControlFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new FeatureControlFilterBusinessObject();
		}

		public void TestFilters()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var obj1 = Factory.NewWithValidTestData<FeatureControlHeader>();
			var obj2 = Factory.NewWithValidTestData<FeatureControlHeader>();
			var obj3 = Factory.NewWithValidTestData<FeatureControlHeader>();

			obj1.FCM_FeatureControlCode = "C001";
			obj2.FCM_FeatureControlCode = "C002";
			obj3.FCM_FeatureControlCode = "C003";

			obj1.FCM_Description = "DEC001";
			obj2.FCM_Description = "DEC002";
			obj3.FCM_Description = "DEC003";

			obj1.FCM_GG_ReleaseGroup = group1.PK;
			obj2.FCM_GG_ReleaseGroup = group2.PK;
			obj3.FCM_GG_ReleaseGroup = group1.PK;

			Factory.Save();

			AssertFilter((f) =>
			{
				((ModuleTextFilter)f["Code"]).Property = "C002";
				((ModuleTextFilter)f["Code"]).IsActive = true;
			}, new[] { obj2 });

			AssertFilter((f) =>
			{
				((ModuleTextFilter)f["Description"]).Property = "DEC001";
				((ModuleTextFilter)f["Description"]).IsActive = true;
			}, new[] { obj1 });

			AssertFilter((f) =>
			{
				((ModuleGuidFilter)f["Release Group"]).Property = group2.PK;
				((ModuleGuidFilter)f["Release Group"]).IsActive = true;
			}, new[] { obj2 });
		}

		void AssertFilter(Action<FeatureControlFilterBusinessObject> applyFilter, IEnumerable<FeatureControlHeader> expected)
		{
			var filterBizo = new FeatureControlFilterBusinessObject();
			applyFilter(filterBizo);
			var collection = new FeatureControlHeaderCollection(Factory);
			collection.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(expected, collection);
		}
	}
}
