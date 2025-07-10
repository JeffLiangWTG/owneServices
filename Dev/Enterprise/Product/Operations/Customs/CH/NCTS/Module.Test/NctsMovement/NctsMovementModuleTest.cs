using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementModule))]
sealed class NctsMovementModuleTest : ZModuleBasherTest
{
	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.NctsMovementModule;

	public void TestGetNewFilterBusinessObject()
	{
		using (var module = new NctsMovementModule())
		{
			AssertType<NctsMovementFilterStripBusinessObject>(module.FilterBusinessObject);
		}
	}

	public void TestGetNewFilterControl()
	{
		using (var nctsMovementModule = new NctsMovementModuleForTest())
		{
			var nctsMovementFilterControl = nctsMovementModule.GetNewFilterControl_Exposed();
			AssertType<NctsMovementFilterControl>(nctsMovementFilterControl);
			nctsMovementFilterControl.Dispose();
		}
	}

		public override void TestExceptionsFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestMilestonesFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedTaskStatusFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTasksFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTriggersFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		class NctsMovementModuleForTest : NctsMovementModule
		{
			public IFilterControl GetNewFilterControl_Exposed()
			{
				return base.GetNewFilterControl();
			}
		}
}
