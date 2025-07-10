using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ReleaseBuilds.Module.Testing
{
	[TestedType(typeof(ReleaseBuildModule))]
	public class ReleaseBuildModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.ReleaseBuild;
		}
	}
}
