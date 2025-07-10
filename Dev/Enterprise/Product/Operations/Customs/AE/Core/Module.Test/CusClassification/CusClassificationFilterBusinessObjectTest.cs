using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(CusClassificationFilterBusinessObject))]
public class CusClassificationFilterBusinessObjectTest : Customs.Module.Testing.CusClassificationFilterBusinessObjectTest
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new CusClassificationFilterBusinessObject();
	}
}
