using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.SeaCargo.Testing
{
	[TestedType(typeof(CusSCADepotHouseFilterBusinessObject))]
	sealed class CusSCADepotHouseFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusSCADepotHouseFilterBusinessObject();
	}
}
