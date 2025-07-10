using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class MyAccountContactFilterStripBusinessObjectForTest : MyAccountContactFilterStripBusinessObject
	{
		public MyAccountContactFilterStripBusinessObjectForTest() : base(new BusinessObjectFactory())
		{
		}

		public MyAccountContactFilterStripBusinessObjectForTest(OrgHeader org) : base(org.Factory)
		{
			Master = org;
		}

		readonly OrgHeader Master;
		protected override OrgHeader MasterOrg => Master;
	}
}