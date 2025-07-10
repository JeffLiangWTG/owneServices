using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class WebSecurityContactFilterStripBusinessObjectForTest : WebSecurityContactFilterStripBusinessObject
	{
		public WebSecurityContactFilterStripBusinessObjectForTest() : base(new BusinessObjectFactory())
		{
		}

		public WebSecurityContactFilterStripBusinessObjectForTest(OrgHeader org) : base(org.Factory)
		{
			Master = org;
		}

		readonly OrgHeader Master;
		protected override OrgHeader MasterOrg => Master;
	}
}