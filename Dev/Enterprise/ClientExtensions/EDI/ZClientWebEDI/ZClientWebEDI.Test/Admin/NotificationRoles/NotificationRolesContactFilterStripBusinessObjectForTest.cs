using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class NotificationRolesContactFilterStripBusinessObjectForTest : NotificationRolesContactFilterStripBusinessObject
	{
		public NotificationRolesContactFilterStripBusinessObjectForTest() : base(new BusinessObjectFactory())
		{
		}

		public NotificationRolesContactFilterStripBusinessObjectForTest(OrgHeader org) : base(org.Factory)
		{
			Master = org;
		}

		readonly OrgHeader Master;
		protected override OrgHeader MasterOrg => Master;
	}
}