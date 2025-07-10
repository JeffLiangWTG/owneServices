using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module.Testing
{
	class CusPermitModuleStripTest : Customs.Module.Testing.CusPermitModuleStripTest
	{
		public void TestGetPermitTypeFilterStrip_ReturnType()
		{
			using (var filterStip = new CusPermitModuleStripForTest())
			{
				filterStip.SetDataBinding(new FilterStrip(new ModuleFilterCollection()), "");
				using (var control = filterStip.GetCurrentFilterControlsForTest(new PermitTypeModuleFilter(CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType, null, () => new CodeDescriptionPairList())))
				{
					AssertType<PermitTypeFilterStrip>(control);
				}
			}
		}

		class CusPermitModuleStripForTest : CusPermitModuleStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter)[0];
			}
		}
	}
}
