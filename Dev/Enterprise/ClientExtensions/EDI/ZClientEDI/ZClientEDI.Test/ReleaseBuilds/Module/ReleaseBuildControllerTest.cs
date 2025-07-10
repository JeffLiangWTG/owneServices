using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ReleaseBuilds.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ReleaseBuilds.Module.Testing
{
	[TestedType(typeof(ReleaseBuildController))]
	class ReleaseBuildControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			using (var form = (ReleaseBuildForm)Controller.ShowNewForm())
			{
				var bizo = (ReleaseBuild)form.BusinessEntity;
				AssertEquals("HL_Product", ZString.Empty, bizo.HL_Product);
				AssertEquals("HasChanges", false, bizo.HasChanges);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.ReleaseBuild;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.New<ReleaseBuild>();
			Factory.Save();
			return bizO;
		}
	}
}
