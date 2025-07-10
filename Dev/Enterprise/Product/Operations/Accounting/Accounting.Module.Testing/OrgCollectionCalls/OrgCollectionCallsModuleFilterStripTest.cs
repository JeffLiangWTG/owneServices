using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public class OrgCollectionCallsModuleFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (OrgCollectionCallsModuleFilterStrip strip = new OrgCollectionCallsModuleFilterStrip())
			{
				strip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				Control[] result = strip.GetCurrentFilterControls_ForTestOnly(new DaysAndAmountOverdueModuleFilter("hello"));
				AssertEquals(1, result.Length);
				AssertEquals(typeof(DaysAndAmountOverdueModuleFilterControl), result[0].GetType());
				result[0].Dispose();

				result = strip.GetCurrentFilterControls_ForTestOnly(new ModuleTextFilter("hello", GlbStaffSchema.GS_Code));
				AssertNull(result);
			}
		}
	}
}
