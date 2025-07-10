using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	class NotificationTestListener : BaseTestListener
	{
		#region Singleton

		public static NotificationTestListener Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new NotificationTestListener();
				}

				return fInstance;
			}
		}

		static NotificationTestListener fInstance;

		protected NotificationTestListener()
		{
		}

		#endregion

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);

			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			Assertion.Assert("Globals.Message must be of type UnitTestNotication during Tests", userNotification != null);

			userNotification.ClearMessagesAndAnswers();
		}
	}
}
