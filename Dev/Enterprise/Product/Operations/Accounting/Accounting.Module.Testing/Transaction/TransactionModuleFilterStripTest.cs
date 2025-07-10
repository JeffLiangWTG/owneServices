using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public class TransactionModuleFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (TransactionModuleFilterStrip strip = new TransactionModuleFilterStrip())
			{
				strip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				Control[] result = strip.GetCurrentFilterControls_ForTestOnly(new OrgWithAddressFilter("hello", (x, y) => { return new CargoWise.EntityFramework.ZQuery(); }, true));
				AssertEquals(1, result.Length);
				AssertEquals(typeof(OrgWithAddressFilterControl), result[0].GetType());
				result[0].Dispose();

				result = strip.GetCurrentFilterControls_ForTestOnly(new ModuleTextFilter("hello", GlbStaffSchema.GS_Code));
				AssertNull(result);
			}
		}
	}
}
