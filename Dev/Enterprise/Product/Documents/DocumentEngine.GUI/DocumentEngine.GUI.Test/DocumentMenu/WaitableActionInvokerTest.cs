using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class WaitableActionInvokerTest : TestCase
	{
		public void TestInvoke()
		{
			string dummyField = null;

			WaitableActionInvoker.Invoke<OdysseyException>("z", null, delegate
			{
				dummyField = "y";
			});
			AssertEquals("Dummy Field", "y", dummyField);

			WaitableActionInvoker.Invoke<OdysseyException>("z", null, delegate
			{
				throw new OdysseyException("Goo!");
			});
			AssertEquals("Caught Exception Message", "Goo!", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			try
			{
				WaitableActionInvoker.Invoke<OdysseyException>("z", null, delegate
				{
					throw new ApplicationException("Gah!");
				});
				Fail("An exception should have been thrown.");
			}
			catch (ApplicationException ex)
			{
				AssertEquals("Thrown Exception Message", "Gah!", ex.Message);
			}
			AssertNull("No exceptions should have been caught.", UnitTestUserNotification.Instance.LastMessage.Text);

			using (Form form = new Form())
			{
				form.Cursor = Cursors.Hand;
				WaitableActionInvoker.Invoke<OdysseyException>("z", form, delegate
				{
					dummyField = "z";
					AssertEquals("Cursor", Cursors.WaitCursor, form.Cursor);
				});
				AssertEquals("Dummy Field", "z", dummyField);
				AssertEquals("Cursor", Cursors.Hand, form.Cursor);
			}
		}
	}
}
