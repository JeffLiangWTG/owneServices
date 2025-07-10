using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Organisation.Testing
{
	[TestedType(typeof(GBOrganisationConsigneePlugInController))]
	public class GBOrganisationConsigneePlugInControllerTest : ZControllerBasherTest
	{
		public void TestPlugIn()
		{
			GBOrganisationConsigneePlugInController controller = new GBOrganisationConsigneePlugInController();
			using (ZTabControl tabControl = new ZTabControl())
			{
				PlugIns plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl);
				plugIns.Add(controller.ID);
				using (ZPlugIn plugIn = plugIns.GetPlugIn(controller.ID))
				{
					AssertEquals(typeof(GUI.OrganisationConsigneePlugIn), plugIn.GetType());
				}
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.GB.OrganisationConsigneePlugIn;
		}
	}
}
