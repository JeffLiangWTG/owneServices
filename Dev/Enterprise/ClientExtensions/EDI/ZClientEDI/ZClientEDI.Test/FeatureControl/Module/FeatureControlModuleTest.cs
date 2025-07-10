using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	[TestedType(typeof(FeatureControlModule))]
	public class FeatureControlModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.FeatureControl;
		}
	}
}
