using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class TwoCheckBoxesBoundTestCase : TestCase
	{
		public void TestIt()
		{
			try
			{
				DoIt();
			}
			catch
			{
				Thread.Sleep(1000);
				DoIt();
			}
		}

		void DoIt()
		{
			TestRootEntity entity = new TestRootEntity();
			using (TwoCheckBoxesBoundTestCaseForm form = new TwoCheckBoxesBoundTestCaseForm())
			{
				form.SetDataBinding(entity, "");
				if (form.ShowAndCheckFormIsActive())
				{
					form.cbBoolean1.Focus();

					while (!form.cbBoolean1.Checked)
					{
						KSendKeys.SendWait(" ", form.cbBoolean1);
						Application.DoEvents();
					}
					AssertEquals("Checked on other check box should be toggled because they're bound to the same field", true, form.cbBoolean2.Checked);

					while (form.cbBoolean1.Checked)
					{
						KSendKeys.SendWait(" ", form.cbBoolean1);
						Application.DoEvents();
					}
					AssertEquals("Unchecked on other check box should be toggled because they're bound to the same field", false, form.cbBoolean2.Checked);
				}
			}
		}

		public class TestRootEntity : ComponentModel.Testing.KComponentWithPropertyChange
		{
			public bool Boolean
			{
				get { return boolean; }
				set
				{
					if (boolean != value)
					{
						boolean = value;
						FirePropertyChanged(nameof(Boolean));
					}
				}
			}
			bool boolean;
		}
	}
}
