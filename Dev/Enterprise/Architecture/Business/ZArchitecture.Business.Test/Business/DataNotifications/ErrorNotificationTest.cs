using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ErrorNotificationTest : TestCase
	{
		public void TestErrorTypeAndDisplayMessage()
		{
			ErrorNotification notification = new ErrorNotification(ErrorType.Error, "Cow goes Moo Moo");
			AssertEquals(ErrorType.Error, notification.ErrorType);
			AssertEquals("Error: Cow goes Moo Moo", notification.Message);

			notification = new ErrorNotification(ErrorType.Warning);
			AssertEquals("Error: " + ErrorType.Warning.Message, notification.Message);
		}

		public void TestDisplayMessage_WithErrorTypeError()
		{
			ErrorNotification notification = new ErrorNotification(ErrorType.Error, "AdditionalInfo");
			AssertEquals("Error: AdditionalInfo", notification.Message);
		}

		public void TestDisplayMessage_WithOtherErrorType()
		{
			ErrorNotification notification = new ErrorNotification(ErrorType.RequiredFieldEmpty, "AdditionalInfo");
			AssertEquals("Error: Required field empty (AdditionalInfo)", notification.Message);
		}

		#region Inherited Tests

		[TestedType(typeof(ErrorNotification))]
		sealed class InheritedTest1 : NotificationTest<ErrorNotification>
		{
			protected override ErrorNotification NewTestNotification()
			{
				return new ErrorNotification(ErrorType.EmailNotifyGroupNotExist);
			}
		}

		[TestedType(typeof(ErrorNotification))]
		sealed class InheritedTest2 : NotificationTest<ErrorNotification>
		{
			protected override ErrorNotification NewTestNotification()
			{
				return new ErrorNotification(ErrorType.EmailNotifyGroupNotExist, "AdditionalInfo");
			}
		}

		#endregion
	}
}
