using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	[TestedType(typeof(BindableNotifications))]
	sealed class DocumentNotificationsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var document = new DummyDocument();
			return new BindableNotifications(document.Notifications);
		}

		public void TestNotifications()
		{
			var document = new DummyDocument();

			document.Rows.Add(10);
			document.Rows.Add(10);

			document.Columns.Add(10);

			var cell1 = new DummyCell();
			cell1.MacroExpression = "SomeMacro".CreateExpression();

			cell1.TopRow = 1;
			cell1.BottomRow = 1;
			cell1.LeftColumn = 1;
			cell1.RightColumn = 1;

			var source1 = new DummyNotificationSource
			{
				Description = "Ranger"
			};

			var notification1 = new Notification(source1, NotificationType.Warning, "watch out! bears!");

			var source2 = new DummyNotificationSource
			{
				Description = "Angry shopper"
			};

			var notification2 = new Notification(source2, NotificationType.Error, "coles run out of snickers");

			cell1.Add(notification1);
			cell1.Add(notification2);

			document.AddCell(cell1);

			var cell2 = new DummyCell();
			cell2.MacroExpression = "SomeOtherMacro".CreateExpression();

			cell2.TopRow = 2;
			cell2.BottomRow = 2;
			cell2.LeftColumn = 1;
			cell2.RightColumn = 1;

			document.AddCell(cell1);

			var documentNotifications = new BindableNotifications(document.Notifications);

			var notificationsFormatted = documentNotifications.Notifications
				.Cast<BindableNotification>()
				.Select(n => string.Format("{0}|{1}|{2}", n.Type, n.Source, n.Message));

			AssertContainsExactElementsInAnyOrder("expected notifications",
				new[]
				{
					"Warning|Ranger|watch out! bears!",
					"Error|Angry shopper|coles run out of snickers"
				},
				notificationsFormatted);
		}
	}
}
