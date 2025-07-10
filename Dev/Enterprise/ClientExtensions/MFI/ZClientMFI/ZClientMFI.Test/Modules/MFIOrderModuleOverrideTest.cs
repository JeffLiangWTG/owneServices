using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	[TestedType(typeof(MFIOrderModuleOverride))]
	public class MFIOrderModuleOverrideTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Orders;
		}
	}
}
