using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMBufferTimespanModule))]
	class BMBufferTimespanModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.BMBufferTimespan;
	}
}
