using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Module.Testing
{
	[TestedType(typeof(CusClassificationFilterBusinessObject))]
	class CusClassificationFilterBusinessObjectTest : Customs.Module.Testing.CusClassificationFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusClassificationFilterBusinessObject();
	}
}
