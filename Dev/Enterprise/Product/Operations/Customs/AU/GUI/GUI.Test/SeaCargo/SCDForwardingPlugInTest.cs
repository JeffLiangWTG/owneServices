using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	class SCDForwardingPlugInTest : Declaration.Business.Testing.SeaCargoDepotTestCase
	{
		public void TestMenuNotPresent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			using (SCDForwardingPlugIn testPlugIn = new SCDForwardingPlugIn(consol))
			{
				AssertNull("No Menu should exist in the forwarding plug in for depot", testPlugIn.TopLevelMenu);
			}
		}

		public void TestUserControlNotPresent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			using (SCDForwardingPlugIn testPlugIn = new SCDForwardingPlugIn(consol))
			{
				AssertNull("User Control should be null for depot plug in to forwarding", testPlugIn.UserControl);
			}
		}

		protected CFSLoadListConsol GetConsolWithContainer()
		{
			CFSLoadListConsol result = GetImportLCLConsol();
			result.JK_OA_UnpackDepotAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			result.Containers.AddNew();
			return result;
		}
	}
}
