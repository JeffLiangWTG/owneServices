using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.GUI.Testing
{
	public class ControlBasedVisibiltyProviderTest : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			int count = 0;
			ControlBasedVisibiltyProvider enProvider = new ControlBasedVisibiltyProvider(IndependentControl, new Func<bool>(() => count % 2 == 0), "Click");
			DependentControl1.SetVisibilityController(enProvider, new Action(() => DependentControl1.Text = (DependentControl1.Visible ? "Y" : "N")));
			DependentControl2.SetEnabler(enProvider, new Action(() => DependentControl2.Text = (DependentControl2.Enabled ? "Y" : "N")));
			for (count = 0; count < 4; count++)
			{
				IndependentControl.PerformClick();
				if (count % 2 == 0)
				{
					AssertEquals("Provider Visible", true, enProvider.Visible);
					AssertEquals("Visible", true, DependentControl1.Visible);
					AssertEquals("Text", "Y", DependentControl1.Text);
					AssertEquals("Enabled", true, DependentControl2.Visible);
					AssertEquals("Text", "Y", DependentControl2.Text);
				}
				else
				{
					AssertEquals("Provider Visible", false, enProvider.Visible);
					AssertEquals("Visible", false, DependentControl1.Visible);
					AssertEquals("Text", "N", DependentControl1.Text);
					AssertEquals("Enabled", true, DependentControl2.Visible);
					AssertEquals("Text", "N", DependentControl2.Text);
				}
			}
		}

		Button IndependentControl
		{
			get
			{
				return fIndependentControl ?? (fIndependentControl = new Button());
			}
		}
		Button fIndependentControl;

		Control DependentControl1
		{
			get
			{
				return fDependentControl1 ?? (fDependentControl1 = new Control("DependentControl"));
			}
		}
		Control fDependentControl1;

		Control DependentControl2
		{
			get
			{
				return fDependentControl2 ?? (fDependentControl2 = new Control("DependentControl2"));
			}
		}
		Control fDependentControl2;
	}
}
