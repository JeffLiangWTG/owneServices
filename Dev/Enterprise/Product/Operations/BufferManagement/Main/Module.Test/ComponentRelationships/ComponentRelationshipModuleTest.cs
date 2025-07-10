using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ComponentRelationshipModule))]
	class ComponentRelationshipModuleTest : ZModuleBasherTest
	{
		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ComponentRelationship;
		}

		#endregion
	}
}
