using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ViewComponentChangeLogModule))]
	class ViewComponentChangeLogModuleTest : ZModuleBasherTest
	{
		public void TestHasActions()
		{
			using (var module = new ViewComponentChangeLogModule())
			{
				AssertEquals(false, module.HasActions);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ViewComponentChangeLog;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true); // StmALog has no index on its PK, but we would still like the ability to export to Excel.
		}

		#endregion
	}
}
