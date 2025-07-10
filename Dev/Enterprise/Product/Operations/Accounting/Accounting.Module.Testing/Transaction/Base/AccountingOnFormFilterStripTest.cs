using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public class AccountingOnFormFilterStripTest : TestCaseWithFactory
	{
		public void TestAccountingOnFormFilterStrip()
		{
			using (var strip = new AccountingOnFormFilterStrip())
			{
				var filterControlBindingSource = new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection());
				strip.SetDataBinding(filterControlBindingSource, "");

				var referenceNumberFilter = new ReferenceNumberFilter("Reference Number",
					new ReferenceNumberFilterHelper<ForwardingShipment>().GetReferenceNumberFilter,
					new RefCountryCollection(Factory)).WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);

				var controls = strip.GetCurrentFilterControls_ForTestOnly(referenceNumberFilter);
				AssertEquals(6, controls.Length);

				DisposeControls(controls);
			}
		}

		void DisposeControls(Control[] controls)
		{
			foreach (var control in controls)
			{
				control.Dispose();
			}
		}
	}
}
