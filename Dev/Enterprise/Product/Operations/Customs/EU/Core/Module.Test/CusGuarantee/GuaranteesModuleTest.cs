using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(GuaranteesModule))]
	class GuaranteesModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new GuaranteesModuleForTest())
			using (var control = module.GetNewFilterControl())
			{
				AssertType<GuaranteesFilterControl>(control);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using var module = new GuaranteesModule();
			AssertType<GuaranteesFilterStripBusinessObject>(module.FilterBusinessObject);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.Guarantees;

		class GuaranteesModuleForTest : GuaranteesModule
		{
			public new IFilterControl GetNewFilterControl() => base.GetNewFilterControl();
		}
	}
}
