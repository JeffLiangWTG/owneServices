using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Module.Testing
{
	[TestedType(typeof(PBNFilterStripBusinessObject))]
	sealed class PBNFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestPBNIdsFilter()
		{
			var filterObj = new PBNFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[PBNFilterStripBusinessObject.PBNFilterConstants.PBNIds];
			AssertNotNull(filter);
		}

		public void TestManifestNatureFilter()
		{
			var filterObj = new PBNFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[PBNFilterStripBusinessObject.PBNFilterConstants.ManifestNature];
			AssertNotNull(filter);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new PBNFilterStripBusinessObject();
	}
}
