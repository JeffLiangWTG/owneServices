using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	[TestedType(typeof(FeatureSetModule))]
	public class FeatureSetModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.FeatureSet;
		}
	}
}
