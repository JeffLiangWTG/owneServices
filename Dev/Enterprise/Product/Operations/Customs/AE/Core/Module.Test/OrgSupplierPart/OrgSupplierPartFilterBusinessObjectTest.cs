using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
public class OrgSupplierPartFilterBusinessObjectTest : Customs.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new OrgSupplierPartFilterStripBusinessObject();
	}
}
