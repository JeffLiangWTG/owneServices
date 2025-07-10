using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMSystemsModule))]
	class BMSystemsModuleTest : ZModuleBasherTest
	{
		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BMSystems;
		}

		#endregion
	}
}
