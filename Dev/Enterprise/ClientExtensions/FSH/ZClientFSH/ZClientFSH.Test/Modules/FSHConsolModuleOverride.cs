using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.FSH.Testing
{
	[TestedType(typeof(FSHConsolModuleOverride))]
	public class FSHConsolModuleOverrideTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobConsol;
		}
	}
}
