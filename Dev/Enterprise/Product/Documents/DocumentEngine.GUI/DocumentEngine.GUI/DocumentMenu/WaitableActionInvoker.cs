using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	static class WaitableActionInvoker
	{
		/// <summary>
		/// Changes the mouse cursor to a wait cursor while the specified action is being performed.
		/// </summary>
		/// <typeparam name="T">The type of the exception to catch.</typeparam>
		/// <param name="errorDescription">The description of the caught exception.</param>
		/// <param name="form">The current form that is being displayed.</param>
		/// <param name="action">The action to perform.</param>
		public static void Invoke<T>(string errorDescription, Form form, WaitableAction action) where T : Exception
		{
			Cursor previousCursor = null;
			if (form != null)
			{
				previousCursor = form.Cursor;
				form.Cursor = Cursors.WaitCursor;
			}

			try
			{
				try
				{
					action.Invoke();
				}
				finally // The finally block is here so that the cursor switches back to the normal one when an exception occurs.
				{
					if (form != null)
					{
						form.Cursor = previousCursor;
					}
				}
			}
			catch (T ex)
			{
				Globals.Message.ShowError(ex.Message, errorDescription);
			}
		}

		public delegate void WaitableAction();
	}
}
