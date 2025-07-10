using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobInvoiceDescriptionControl))]
	class JobInvoiceDescriptionControlTest : RegistryZUserControlTestCase
	{
		public void TestGetTypesForMapPresenterForALL()
		{
			using (var control = GetNewControl() as JobInvoiceDescriptionControl)
			{
				AssertArrayEqualsByElements("JobHeader type for JobType ALL", new Type[] { typeof(JobHeader) }, control.GetTypesForMapPresenter("ALL"));
				AssertArrayEqualsByElements("JobHeader type for unknown JobType", new Type[] { typeof(JobHeader) }, control.GetTypesForMapPresenter("@#$"));
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new JobInvoiceDescriptionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((JobInvoiceDescriptionControl)control).ConfigurationGrid.ReadOnly;
		}

		public void TestMacroButtonWithFocusOnANewLine()
		{
			using (var control = GetNewControl() as JobInvoiceDescriptionControl)
			{
				using (ZForm form = new ZForm())
				{
					IBusiness businessEntity = GetNewBusinessEntity();
					control.SetDataBinding(businessEntity, null);
					form.Controls.Add(control);

					form.Show();
					form.Cursor = Cursors.Arrow;
					AssertNotEquals(Cursors.WaitCursor, form.Cursor);

					control.ConfigurationGrid.CurrentCell = new DataGridCell(0, 3);
					form.Cursor = Cursors.WaitCursor;   // To invoke the CursorChanged event

					AssertEquals("Precondtion: focus on a new line", 0, control.ConfigurationGrid.CurrentCell.RowNumber);
					AssertEquals("Precondtion: focus on the Job Invoice Description column", 3, control.ConfigurationGrid.CurrentCell.ColumnNumber);

					AssertNoExceptionThrown(() => control.MacroButton.PerformClick());
				}
			}
		}
	}
}
