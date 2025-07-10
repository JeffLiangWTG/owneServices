using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(BindableNotification))]
	sealed class DocumentNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var notification = new Notification(new DummyNotificationSource(), NotificationType.Warning, "watch out! penguins!");
			return new BindableNotification(notification);
		}
	}
}
