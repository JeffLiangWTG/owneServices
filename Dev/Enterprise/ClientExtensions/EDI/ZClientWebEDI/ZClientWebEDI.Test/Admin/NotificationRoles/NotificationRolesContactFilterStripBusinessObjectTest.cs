using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(NotificationRolesContactFilterStripBusinessObject))]
	public class NotificationRolesContactFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new NotificationRolesContactFilterStripBusinessObjectForTest(Factory.NewWithValidTestData<OrgHeader>());
	}
}
