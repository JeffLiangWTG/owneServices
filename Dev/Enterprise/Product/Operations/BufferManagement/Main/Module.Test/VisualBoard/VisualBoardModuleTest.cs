using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(VisualBoardModule))]
	class VisualBoardModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.VisualBoard;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);

			BMSTestHelper.CreateBoard(BMSTestHelper.CreateSystem(collection.Factory));
		}
	}
}
