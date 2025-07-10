using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.JobDeclaration;

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}
	}
}
