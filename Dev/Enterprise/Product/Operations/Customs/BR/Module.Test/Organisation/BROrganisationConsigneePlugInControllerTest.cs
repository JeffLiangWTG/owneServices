using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(BROrganisationConsigneePlugInController))]
	public class BROrganisationConsigneePlugInControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.BR.OrganisationConsigneePlugIn;
		}

		public void TestPlugIn()
		{
			var controller = new BROrganisationConsigneePlugInController();
			using (ZTabControl tabControl = new ZTabControl())
			{
				PlugIns plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl);
				plugIns.Add(controller.ID);
				using (var plugIn = plugIns.GetPlugIn(controller.ID))
				{
					AssertType<GUI.OrganisationConsigneePlugIn>(plugIn);
				}
			}
		}
	}
}
