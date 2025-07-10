using System;
using System.Drawing;
using System.Threading;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class NotificationIconSchemeTest : TestCase
	{
		//This test has about a 3% chance of getting stuck in an infinite loop if NotificationIconScheme is not thread-safe.
		//Runs 10000 times in a row with no problems if NotificationIconScheme is thread-safe.
		public void TestMultithreadedAccess()
		{
			var instance = NotificationIconScheme.Instance;
			NotificationIconScheme instanceInThread1 = null;
			Thread thread1 = new Thread(() => { instanceInThread1 = NotificationIconScheme.Instance; });
			thread1.Start();
			thread1.Join();
			AssertEquals("Both threads should get same instance", instance, instanceInThread1);
			TestNotificationType[] testTypes = new TestNotificationType[100];
			Bitmap image = SystemIcons.Asterisk.ToBitmap();
			Bitmap image2 = SystemIcons.Application.ToBitmap();
			for (int i = 0; i < 100; ++i)
			{
				testTypes[i] = new TestNotificationType(i, false);
			}
			Thread thread2 = new Thread(() =>
			{
				for (int i = 0; i < 100; ++i)
				{
					NotificationIconScheme.Instance.SetIcon(testTypes[i], SystemIcons.Application);
					NotificationIconScheme.Instance.SetMiniImage(testTypes[i], image2);
					NotificationIconScheme.Instance.GetIcon(testTypes[i]);
					NotificationIconScheme.Instance.GetMiniImage(testTypes[i]);
				}
			});
			thread2.Start();
			for (int i = 0; i < 100; ++i)
			{
				NotificationIconScheme.Instance.SetIcon(testTypes[i], SystemIcons.Asterisk);
				NotificationIconScheme.Instance.SetMiniImage(testTypes[i], image);
				NotificationIconScheme.Instance.GetIcon(testTypes[i]);
				NotificationIconScheme.Instance.GetMiniImage(testTypes[i]);
			}
			thread2.Join();
			Assert(NotificationIconScheme.Instance.GetIcon(testTypes[0]) == SystemIcons.Application || NotificationIconScheme.Instance.GetIcon(testTypes[0]) == SystemIcons.Asterisk);
			Assert(NotificationIconScheme.Instance.GetIcon(testTypes[99]) == SystemIcons.Application || NotificationIconScheme.Instance.GetIcon(testTypes[99]) == SystemIcons.Asterisk);
		}

		public void TestGetSetIcon()
		{
			NotificationIconScheme.Instance.SetIcon(TestNotificationType.TestType, SystemIcons.Asterisk);
			AssertEquals("GetIcon", SystemIcons.Asterisk, NotificationIconScheme.Instance.GetIcon(TestNotificationType.TestType));
			AssertNotNull("GetImage", NotificationIconScheme.Instance.GetImage(TestNotificationType.TestType));
		}

		public void TestGetSetMiniImage()
		{
			Bitmap image = SystemIcons.Asterisk.ToBitmap();
			NotificationIconScheme.Instance.SetMiniImage(TestNotificationType.TestType, image);
			AssertEquals("SetMiniImage / GetMiniImage", image, NotificationIconScheme.Instance.GetMiniImage(TestNotificationType.TestType));
		}

		public void TestGetIcon_WhenNoRegistrationMade()
		{
			AssertEquals(SystemIcons.Information, NotificationIconScheme.Instance.GetIcon(new NotificationTypeWithoutIcon()));
		}

		public void TestGetImage_WhenNoRegistrationMade()
		{
			AssertNotNull(NotificationIconScheme.Instance.GetImage(new NotificationTypeWithoutIcon()));
		}

		public void TestGetMiniImage_WhenNoRegistrationMade()
		{
			AssertNotNull(NotificationIconScheme.Instance.GetMiniImage(new NotificationTypeWithoutIcon()));
		}

		#region Test Classes

		[Serializable]
		class NotificationTypeWithoutIcon : NotificationType
		{
			public NotificationTypeWithoutIcon()
				: base(-1, false)
			{
			}
		}

		[Serializable]
		class TestNotificationType : NotificationType
		{
			public TestNotificationType(int severity, bool isFatal)
				: base(severity, isFatal)
			{
			}

			public TestNotificationType(string name, int severity, bool isFatal)
				: base(name, severity, isFatal)
			{
			}

			public static readonly TestNotificationType TestType = new TestNotificationType(0, false);
		}

		#endregion
	}
}
