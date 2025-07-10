using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class TestKeyStrokeHelper
	{
		public static void SendJunkKeyStrokesToControl(TextBox textBox)
		{
			textBox.Focus();

			for (var i = 0; i < textBox.DataBindings.Count; i++)
			{
				textBox.DataBindings[i].BindingManagerBase.EndCurrentEdit();
			}

			var charsToSend = 30;
			var maxLength = textBox.MaxLength;
			charsToSend = ((maxLength < charsToSend) && (maxLength > 0)) ? maxLength + 1 : charsToSend;

			for (var i = 1; i < charsToSend; i += 3)
			{
				SendKeyToControl(textBox, Keys.Add, false);
				SendKeyToControl(textBox, Keys.D1, false);
				SendKeyToControl(textBox, Keys.Subtract, false);
			}
			Application.DoEvents();
			UserIdleWorker.Flush();

			for (var i = 0; i < textBox.DataBindings.Count; i++)
			{
				try
				{
					textBox.DataBindings[i].BindingManagerBase.EndCurrentEdit();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (!(e is ArgumentOutOfRangeException))
					{
						throw new Environment.OdysseyException(
							"Exception during End Current Edit Binding Manager " + i.ToString() + " Control:" + textBox.Name, e);
					}
				}
			}

			SendKeyToControl(textBox, Keys.Tab);
		}

		public static void SendKeyToControl(Control control, Keys key)
		{
			SendKeyToControl(control, key, true);
		}

		public static void SendKeyToControl(Control control, Keys key, bool doEvents)
		{
			KeySender.PostKeyDown(control, key);
			if (doEvents)
			{
				Application.DoEvents();
			}
		}
	}
}
