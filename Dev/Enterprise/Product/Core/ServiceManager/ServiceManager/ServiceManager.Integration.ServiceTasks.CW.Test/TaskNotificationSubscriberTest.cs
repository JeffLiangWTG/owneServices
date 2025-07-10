using System;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	class TaskNotificationSubscriberTest : TestCase
	{
		public void TestConstructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
		{
			// Arrange
			ILogger nullLogger = null;

			// Act & Assert
			AssertExceptionThrown<ArgumentNullException>(() => new TaskNotificationSubscriber(nullLogger));
		}

		public void TestAddError_ShouldLogErrorMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber.AddError(message);

			// Assert
			AssertEquals("Error|Message", logger![0]);
		}

		public void TestAddWarning_ShouldLogWarningMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber.AddWarning(message);

			// Assert
			AssertEquals("Warning|Message", logger![0]);
		}

		public void TestAdd_WithErrorNotificationType_ShouldLogErrorMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber.Add(NotificationType.Error, message);

			// Assert
			AssertEquals("Error|Message", logger![0]);
		}

		public void TestAdd_WithWarningNotificationType_ShouldLogWarningMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber!.Add(NotificationType.Warning, message);

			// Assert
			AssertEquals("Warning|Message", logger![0]);
		}

		public void TestAdd_WithErrorNotification_ShouldLogErrorNotificationMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber!.Add(new ErrorNotification(ErrorType.Error, message));

			// Assert
			AssertEquals("Error|Error: Message", logger![0]);
		}

		public void TestAdd_WithWarningNotification_ShouldLogWarningNotificationMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber!.Add(new WarningNotification(message));

			// Assert
			AssertEquals("Warning|Warning: Message", logger![0]);
		}

		public void TestAdd_WithInfoNotification_ShouldLogInfoMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber!.Add(new InfoNotification(message));

			// Assert
			AssertEquals("Information|Message", logger![0]);
		}

		public void TestAdd_WithVerboseInfoNotification_ShouldLogVerboseInfoMessage()
		{
			// Arrange
			var message = "Message";

			// Act
			subscriber!.Add(new VerboseInfoNotification(message));

			// Assert
			AssertEquals("Debug|Message", logger![0]);
		}

		public void TestAdd_WithProgressNotification_ShouldLogProgressMessage()
		{
			// Arrange
			var progress = 18;

			// Act
			subscriber!.Add(new ProgressNotification(progress));

			// Assert
			AssertEquals("Debug|Progress 18%", logger![0]);
		}

		protected override void SetUp()
		{
			logger = new TestServiceLogger();
			subscriber = new TaskNotificationSubscriber(logger);
		}

		TestServiceLogger logger;
		INotifications subscriber;
	}
}
