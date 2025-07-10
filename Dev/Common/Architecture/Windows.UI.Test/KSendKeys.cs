using System;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	public static class KSendKeys
	{
		public static void Send(string keys)
		{ SendKeys.Send(keys); }

		public static void SendWait(string keys)
		{ SendKeys.SendWait(keys); }

		public static void SendWait(string keys, Control control)
		{
			ActivateForm(control);
			SendWait(keys);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Allow for testing")]
		static void ActivateForm(Control control)
		{
			Form form = control.FindForm()
				?? throw new ArgumentException("The control must have a form");
			for (int i = 0; i < 1000000 && form != Form.ActiveForm; i++)
			{
				form.Activate();
#if !WINZOR
				Application.DoEvents();
#endif
			}
			control.Focus();
		}
	}
}
