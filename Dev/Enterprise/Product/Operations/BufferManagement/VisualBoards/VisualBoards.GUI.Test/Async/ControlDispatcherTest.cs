using System;
using System.Windows.Forms;
using CargoWise.Pipes;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	class ControlDispatcherTest : TestCase
	{
		public void TestDispatch_WhenControlDisposed_ShouldNotAttemptToInvoke()
		{
			var control = new Control();
			var dispatcher = (IDispatcher)new ControlDispatcher(control);

			var dispatchedCount = 0;

			var action = new Action(() => dispatchedCount++);

			using (var form = new Form())
			using (control)
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(0, dispatchedCount);

				dispatcher.Dispatch(action);
				AssertEquals(0, dispatchedCount);

				Application.DoEvents();
				AssertEquals(1, dispatchedCount);
			}

			AssertNoExceptionThrown(() => dispatcher.Dispatch(action));

			Application.DoEvents();
			AssertEquals(1, dispatchedCount);
		}
	}
}
