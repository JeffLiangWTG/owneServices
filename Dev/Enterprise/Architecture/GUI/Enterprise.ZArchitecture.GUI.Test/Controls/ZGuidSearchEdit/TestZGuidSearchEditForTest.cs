using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestZGuidSearchEditForTest : ZGuidSearchEdit
	{
		public DummyDependentWithCodeModule DummyModule;

		protected override ZFilterModule NewModuleFromModuleID()
		{
			var result = filterModuleForTesting ?? base.NewModuleFromModuleID();
			DummyModule = result as DummyDependentWithCodeModule;
			return result;
		}

		public void SetFilterModuleForTesting(ZFilterModule module)
		{
			filterModuleForTesting = module;
		}
		ZFilterModule filterModuleForTesting;
	}
}
