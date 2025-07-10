using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(BindableNotificationsCollection))]
	sealed class DocumentNotificationsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BindableNotificationsCollection>
	{
		protected override BindableNotificationsCollection GetCollectionToTest()
		{
			return new BindableNotificationsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var notification = new Notification(new DummyNotificationSource(), NotificationType.Warning, "warning");
			return new BindableNotification(notification);
		}
	}
}
