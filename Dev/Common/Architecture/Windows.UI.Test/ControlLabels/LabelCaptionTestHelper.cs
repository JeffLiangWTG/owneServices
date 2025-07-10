using System.Windows.Forms;

namespace CargoWise.Windows.UI.Testing
{
	public static class LabelCaptionTestHelper
	{
		public static void FireApplicationIdle()
		{
			KForm form = new KForm();

			Timer timer = new Timer();
			timer.Tick += delegate
			{ form.Dispose(); };
			timer.Interval = 200;
			timer.Start();

			form.ShowDialog();
			timer.Dispose();
		}
	}
}
