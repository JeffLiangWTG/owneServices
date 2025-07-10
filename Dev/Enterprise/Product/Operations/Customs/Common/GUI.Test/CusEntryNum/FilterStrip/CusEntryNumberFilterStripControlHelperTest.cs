using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.Module.Testing
{
	public class CusEntryNumberFilterStripControlHelperTest : TestCaseWithFactory
	{
		public void TestHandlesCusEntryNumberModuleDateFilter()
		{
			AssertProvidesControlsFor(new CusEntryNumDateFilter("CusEntryNumber Date Test", typeof(CusEntryNumber)));
		}

		public void TestHandlesWorkflowModuleTextFilter()
		{
			AssertProvidesControlsFor(new CusEntryNumTextFilter("CusEntryNumber Text Test", GetSomeFilter, typeof(CusEntryNumber)));
			AssertProvidesControlsFor(new CusEntryNumTextFilter("CusEntryNumber Text Test", typeof(CusEntryNumber)));
		}

		#region Implementation
		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			ZBindingSource bindingSource = new ZBindingSource();
			Assert(CusEntryNumberFilterStripControlHelper.IsModuleFilterSupported(filter));
			using (ZFilterStrip strip = new ZFilterStrip())
			{
				Control[] controls = CusEntryNumberFilterStripControlHelper.GetFilterControls(strip, filter, bindingSource);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);
				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		ZQuery GetSomeFilter(SQLComparisonOperator oper, ZString str)
		{
			return new ZDBOnlySubQuery(typeof(DummyBusinessObject), CusEntryNumSchema.CE_ParentID);
		}
		#endregion
	}
}
