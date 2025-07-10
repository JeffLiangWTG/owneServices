using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.Testing
{
	[TestedType(typeof(ThreadMonitorForm))]
	sealed class ThreadMonitorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var threadMonitor = new ThreadMonitor(Thread.CurrentThread);
			var frm = new ThreadMonitorForm(threadMonitor);

			var txtStackTrace = frm.Controls.Find("TxtStackTrace", true).Single();
			MissingResourceStringChecker.ExcludeFromTest(txtStackTrace);

			return frm;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "TxtStackTrace";
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}
	}
}
