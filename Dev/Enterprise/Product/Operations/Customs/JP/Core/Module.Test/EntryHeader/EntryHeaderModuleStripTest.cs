using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Module.Test
{
	sealed class EntryHeaderModuleStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new EntryHeaderStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new InspectionStatusFilter("InspectionStatus", (a, b) => new ZQuery(), Factory)))
				{
					AssertEquals(typeof(InspectionStatusFilterControl), result.GetType());
				}
			}
		}

		sealed class EntryHeaderStripForTest : EntryHeaderStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
