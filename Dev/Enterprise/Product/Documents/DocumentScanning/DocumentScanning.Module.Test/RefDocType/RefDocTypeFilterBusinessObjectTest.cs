using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(RefDocTypeFilterBusinessObject))]
	internal sealed class RefDocTypeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefDocTypeFilterBusinessObject();
		}
	}
}
