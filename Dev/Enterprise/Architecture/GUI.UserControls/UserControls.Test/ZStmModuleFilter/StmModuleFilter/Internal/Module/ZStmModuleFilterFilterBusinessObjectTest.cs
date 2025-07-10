using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZStmModuleFilterFilterBusinessObject))]
	class ZStmModuleFilterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ZStmModuleFilterFilterBusinessObject();
		}
	}
}
