using System;
using System.IO;
using System.Linq;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DrawingTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNotificationForInvalidDrawingImage()
		{
			var cell = new DummyCell();
			AssertEquals("Prerequisite: no notification added", 0, cell.Notifications.Count());

			var invalidImageFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\ExcelTemplates\Forms\Forwarding\ShippingInstruction2.xls");
			var invalidImageData = File.ReadAllBytes(invalidImageFilePath);

			var notification = new Notification(
					cell.CreateNotificationSource(),
					NotificationType.Error,
					"error");

			var errorCallback = new Action<string>((message) => cell.Add(notification));
			var drawing = new DocumentVisualizer.GUI.Drawing(new System.Drawing.PointF(1, 1), new System.Drawing.SizeF(1, 1), invalidImageData, errorCallback);

			AssertNull(drawing.Image);
			AssertEquals("error notification should be added", 1, cell.Notifications.Count());

			var drawing2 = drawing.Image;
			AssertEquals("error notification should not be added twice", 1, cell.Notifications.Count());
		}
	}
}
