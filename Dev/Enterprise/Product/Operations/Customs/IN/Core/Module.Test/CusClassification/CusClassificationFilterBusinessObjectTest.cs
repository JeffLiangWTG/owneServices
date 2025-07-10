using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(CusClassificationFilterBusinessObject))]
sealed class CusClassificationFilterBusinessObjectTest : Customs.Module.Testing.CusClassificationFilterBusinessObjectTest
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusClassificationFilterBusinessObject();
}
