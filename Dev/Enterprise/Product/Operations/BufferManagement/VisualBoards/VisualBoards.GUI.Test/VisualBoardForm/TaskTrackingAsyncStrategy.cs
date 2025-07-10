using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.VisualBoards.GUI.Test
{
	// To synchronise threads in unit tests and all threads end when the test ends
	public class TaskTrackingAsyncStrategy : TaskTrackingAsyncBaseStrategy
	{
		public TaskTrackingAsyncStrategy(bool createTriggerableAutoRefresher = false) : base(createTriggerableAutoRefresher)
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1049:Using Application.DoEvents", Justification = "For Unit Test")]
		public void AwaitAll(Control control, Task taskToIgnore = null)
		{
			Application.DoEvents();
			control.InvokeEvents();

			AwaitAll(t => t != taskToIgnore, () =>
			{
				Application.DoEvents();
				control.InvokeEvents();
			});
		}
	}

	static class Control_Extensions
	{
		[SuppressMessage("CargoWiseOne", "CW1049:Using Application.DoEvents", Justification = "This code is actually in a test assembly.")]
		public static void InvokeEvents(this Control control)
		{
			if (!control.IsDisposed)
			{
				try
				{
					control.InvokeSafe(Application.DoEvents);
				}
				catch (InvalidAsynchronousStateException)
				{
					// Nom. Sometimes this exceptions thrown by invoke when control is disposed. We do not care here though.
				}
				catch (ObjectDisposedException)
				{
					// Nom. Sometimes this exceptions thrown by invoke when control is disposed. We do not care here though.
				}
				catch (TaskCanceledException)
				{
					// Nom. Sometimes this exceptions thrown by invoke when control is disposed. We do not care here though.
				}
			}
		}
	}
}
