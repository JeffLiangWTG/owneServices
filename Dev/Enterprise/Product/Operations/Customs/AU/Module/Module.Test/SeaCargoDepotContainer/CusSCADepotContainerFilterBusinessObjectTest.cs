using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.SeaCargo.Testing
{
	[TestedType(typeof(CusSCADepotContainerFilterBusinessObject))]
	sealed class CusSCADepotContainerFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusSCADepotContainerFilterBusinessObject();
	}
}
