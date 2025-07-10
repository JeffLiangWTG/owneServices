using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Module.Testing
{
	[TestedType(typeof(GlbReleaseNoteController))]
	public class GlbReleaseNoteControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.GlbReleaseNote;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.New<Enterprise.MasterFiles.Business.GlbReleaseNote>();
		}

		[StressTest]
		public override void TestNewForm()
		{
			base.TestNewForm();
		}
	}
}
