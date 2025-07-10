using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccApportionmentTemplateFilterBusinessObject))]
	public class AccApportionmentTemplateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccApportionmentTemplateFilterBusinessObject();
		}

		protected override ModuleFilter SetupFilterForTest(ModuleFilter moduleFilter)
		{
			return moduleFilter.Description != "Notes" ? base.SetupFilterForTest(moduleFilter) : null;
		}
	}
}
