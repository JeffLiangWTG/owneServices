using System.Windows.Forms;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[TestedType(typeof(NotificationsView))]
	sealed class DocumentNotificationsViewTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestForm()
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

			var notification1 = new Notification(new DummyNotificationSource(), NotificationType.Warning, "watch out! bears!");
			var notification2 = new Notification(new DummyNotificationSource(), NotificationType.Error, "coles run out of snickers");

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

			using (var form = new NotificationsView(documentNotifications))
			{
				form.Show();
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var document = new DummyDocument();
			var documentNotifications = new BindableNotifications(document.Notifications);
			return new NotificationsView(documentNotifications);
		}

		#endregion
	}
}
