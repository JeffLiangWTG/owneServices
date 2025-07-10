using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMComponentModule))]
	class BMComponentModuleTest : ZModuleBasherTest
	{
		public void TestHasActions()
		{
			using (var module = new BMComponentModule())
			{
				Assert(!module.HasActions);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BMComponent;
		}

		#endregion
	}
}
