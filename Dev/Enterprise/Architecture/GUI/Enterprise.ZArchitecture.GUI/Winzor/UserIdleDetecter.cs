using System;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	/// <summary>
	/// Detects user mouse and keyboard inactivity for the current application thread.
	/// </summary>
	internal sealed class UserIdleDetecter
	{
		public static bool IsActive => WinzorDispatcher.Current.UserActivity is not null;

		/// <summary>
		/// WARNING: This event fires for every user mouse or keyboard operation.
		/// It is recommended you use very short running methods for the handler of this event.
		/// </summary>
		public static event EventHandler UserActivity
		{
			add
			{
				WinzorDispatcher.Current.UserActivity += value;
			}
			remove
			{
				WinzorDispatcher.Current.UserActivity -= value;
			}
		}

#if DEBUG
		internal static void FireUserActivity()
		{
			OnUserActivity(EventArgs.Empty);
		}
#endif

		#region Implementation

		static void OnUserActivity(EventArgs e) => WinzorDispatcher.Current.UserActivity?.Invoke(null, e);

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	using System.Threading;
	using System.Windows.Forms;
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	class UserIdleDetecterTest : TestCaseWithFactory
	{
		public void TestUserActivityEvent()
		{
			Try3Times(delegate
			{
				var userActivityFired = false;
				EventHandler handler = delegate { userActivityFired = true; }; // putting a breakpoint here may crash your computer
				UserIdleDetecter.UserActivity += handler;
				try
				{
					Form.Show();
					Application.DoEvents();
					AssertEquals("UserActivity not fired when showing a form", false, userActivityFired);
					userActivityFired = false;

					UserIdleDetecter.FireUserActivity();
					AssertEquals("UserActivity fired when a key is pressed", true, userActivityFired);
					userActivityFired = false;

					UserIdleDetecter.FireUserActivity();
					AssertEquals("UserActivity fired when another key is pressed", true, userActivityFired);
					userActivityFired = false;
				}
				finally
				{
					UserIdleDetecter.UserActivity -= handler;
				}
				UserIdleDetecter.FireUserActivity();
				Application.DoEvents();
				AssertEquals("UserActivity not fired when event unhooked", false, userActivityFired);
			});
		}

		public void TestCleanupAfterLastUserActivityUnhook()
		{
			EventHandler handler1 = delegate { }; // putting a breakpoint here may crash your computer
			EventHandler handler2 = delegate { };
			UserIdleDetecter.UserActivity += handler1;
			UserIdleDetecter.UserActivity += handler2;
			AssertEquals(true, UserIdleDetecter.IsActive);

			UserIdleDetecter.UserActivity -= handler1;
			AssertEquals(true, UserIdleDetecter.IsActive);
			UserIdleDetecter.UserActivity -= handler2;
			AssertEquals(false, UserIdleDetecter.IsActive);
		}

		#region Implementation

		void Try3Times(MethodInvoker method)
		{
			var i = 1;
			while (true)
			{
				try
				{
					method();
					break;
				}
				catch (AssertionFailedError)
				{
					if (i == 3)
					{
						throw;
					}
					TearDown();
					Thread.Sleep(1000);
				}
				i++;
			}
		}

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
				form = null;
			}
		}

		#endregion
	}
}

#endif
#endregion
