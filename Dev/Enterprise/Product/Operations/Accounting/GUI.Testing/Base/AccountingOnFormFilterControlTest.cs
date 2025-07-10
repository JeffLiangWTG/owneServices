using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AccountingOnFormFilterControlTest : ZFilterStripControlTest
	{
		public void TestSetToolStripToReadOnly2()
		{
			using (ZForm form = new ZForm())
			{
				var strip = new AccountingOnFormFilterControl(null, new PeriodicInvoice(Factory).JobsFilter);
				form.Controls.Add(strip);
				form.SetReadOnlyIncludingChildren();
				ReadOnlyTester(form);
			}
		}

		public void ReadOnlyTester(Control control)
		{
			ToolStrip toolStrip = control as ToolStrip;
			if (toolStrip != null)
			{
				var readOnlyAttributeType = typeof(CanBeReadOnlyUIAttribute);
				foreach (ToolStripItem toolStripItem in toolStrip.Items)
				{
					AssertEquals("Tool strip should not be enabled", false, toolStripItem.Enabled);
					AssertNotNull("Should have attribute", TypeDescriptor.GetAttributes(toolStripItem)[readOnlyAttributeType]);
				}
			}
			else
			{
				foreach (Control child in control.Controls)
				{
					ReadOnlyTester(child);
				}
			}
		}
	}
}
