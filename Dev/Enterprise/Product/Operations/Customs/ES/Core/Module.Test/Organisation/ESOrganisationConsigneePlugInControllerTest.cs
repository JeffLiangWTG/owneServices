using Enterprise.Customs.ES.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(ESOrganisationConsigneePlugInController))]
	public class ESOrganisationConsigneePlugInControllerTest : ZControllerBasherTest
	{
		public void TestPlugIn()
		{
			var controller = new ESOrganisationConsigneePlugInController();
			using (var tabControl = new ZTabControl())
			{
				var plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl);
				plugIns.Add(controller.ID);
				using (var plugIn = plugIns.GetPlugIn(controller.ID))
				{
					AssertEquals(typeof(OrganisationConsigneePlugIn), plugIn.GetType());
				}
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.ES.OrganisationConsigneePlugIn;
		}
	}
}
