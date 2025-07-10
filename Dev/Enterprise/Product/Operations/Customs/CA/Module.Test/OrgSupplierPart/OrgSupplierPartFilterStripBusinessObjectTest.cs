using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
	sealed class OrgSupplierPartFilterStripBusinessObjectTest : Customs.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();
	}
}
