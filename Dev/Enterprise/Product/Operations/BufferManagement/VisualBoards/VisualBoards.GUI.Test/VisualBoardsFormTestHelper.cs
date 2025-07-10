using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.VisualBoards.GUI.Test
{
	public abstract class VisualBoardsFormTestHelper
	{
		public static IEnumerable<T> GetOpenForms<T>() where T : Form // We are getting an Application form
		{
			return ZApplication.GetOpenForms().OfType<T>();
		}
		public static bool PressHotkeys(Control control, Keys keysToSend)
		{
			var lParam = (IntPtr)(keysToSend & ~(Keys.Control | Keys.Shift | Keys.Alt));
			var msg = new Message { Msg = WindowsMessage.WM_KEYDOWN, LParam = lParam };

			var method = control.GetType().GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic);
			return (bool)method.Invoke(control, new object[] { msg, keysToSend });
		}
	}

	public static class VisualBoardsFormTestHelper_Extensions
	{
		public static void RefreshBoardAndWait(this VisualBoardForm form)
		{
			form.RefreshBoard();
			form.AwaitAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "This is for unit tests only.")]
		public static void AwaitAll(this VisualBoardForm form)
		{
			if (AsyncStrategy.Default is TaskTrackingAsyncBaseStrategy taskTracking)
			{
				do
				{
					taskTracking.AwaitAll(null, () => Application.DoEvents());
				}
				while (form.IsWaitingForRefresh || form.IsInRefresh || form.IsInReload); // This is needed because the Pulsar doesn't add tasks to the queue.
			}
			else
			{
				do
				{
					Application.DoEvents();
				}
				while (form.IsInRefresh || form.IsInReload || form.IsWaitingForRefresh);
			}
		}
	}
}
