using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	class TempStorageRegisterLinesModuleStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new TempStorageRegisterLinesModuleStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new PremisesModuleFilter("Test", (a) => new ZQuery())))
				{
					AssertEquals(typeof(PremisesFilterStrip), result.GetType());
				}
			}
		}

		sealed class TempStorageRegisterLinesModuleStripForTest : TempStorageRegisterLinesModuleStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
