using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.Testing
{
	[TestedType(typeof(OrganisationConsigneePlugInController))]
	class OrganisationConsigneePlugInControllerTest : ZControllerBasherTest
	{
		public void TestPlugIn()
		{
			var controller = new OrganisationConsigneePlugInController();
			using (var tabControl = new ZTabControl())
			{
				using (var plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl))
				{
					plugIns.Add(controller.ID);
					using (var plugIn = plugIns.GetPlugIn(controller.ID))
					{
						AssertType<GUI.OrganisationConsigneePlugIn>(plugIn);
					}
				}
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.FR.OrganisationConsigneePlugIn;
	}
}
