using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class NotificationWithGroupKeyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertConstructor("group key", "AAA", NotificationType.Error);
			AssertConstructor("group key1", "AAAB", NotificationType.Warning);
			AssertConstructor("group key2", "AAAC", NotificationType.Information);
		}

		void AssertConstructor(string groupKey, string message, INotificationType type)
		{
			var mockNotifitcation = new Mock<INotification>();
			mockNotifitcation.Setup(x => x.Message).Returns(message);
			mockNotifitcation.Setup(x => x.Type).Returns(type);

			var groupNotification = new NotificationWithGroupKey(groupKey, mockNotifitcation.Object);
			AssertEquals("Type", type, groupNotification.Type);
			AssertEquals("Message", message, groupNotification.Message);
			AssertEquals("GroupKey", groupKey, groupNotification.GroupKey);
		}

		public void TestReplaceMessage()
		{
			var mockNotifitcation = new Mock<INotification>();
			mockNotifitcation.Setup(x => x.Message).Returns("AAAAA");
			mockNotifitcation.Setup(x => x.Type).Returns(NotificationType.Warning);

			mockNotifitcation.Setup(x => x.ReplaceMessage(It.IsAny<string>())).Returns(() =>
			{
				var replaceNotification = new Mock<INotification>();
				mockNotifitcation.Setup(x => x.Message).Returns("replaced message");
				mockNotifitcation.Setup(x => x.Type).Returns(NotificationType.Error);
				return mockNotifitcation.Object;
			});

			var groupNotification = new NotificationWithGroupKey("fixed group key", mockNotifitcation.Object);
			CombineAssertions("Before ReplaceMessage", () => {
				AssertEquals("Type", NotificationType.Warning, groupNotification.Type);
				AssertEquals("Message", "AAAAA", groupNotification.Message);
				AssertEquals("GroupKey", "fixed group key", groupNotification.GroupKey);
			});

			var replacedNotification = groupNotification.ReplaceMessage("dummy value");
			AssertEquals("Replaced Type", NotificationType.Error, replacedNotification.Type);
			AssertEquals("Replaced Message", "replaced message", replacedNotification.Message);
			CombineAssertions("After ReplaceMessage, value should be same to replacedNotification", () => {
				AssertEquals("Type", replacedNotification.Type, groupNotification.Type);
				AssertEquals("Message", replacedNotification.Message, groupNotification.Message);
				AssertEquals("but GroupKey should be still fixed", "fixed group key", groupNotification.GroupKey);
			});
		}
	}
}
