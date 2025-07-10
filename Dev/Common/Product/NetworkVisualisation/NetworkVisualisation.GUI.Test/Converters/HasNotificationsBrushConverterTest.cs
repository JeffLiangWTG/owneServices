using System.Windows.Media;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test.Converters
{
	[TestedType(typeof(HasNotificationsBrushConverter))]
	class HasNotificationsBrushConverterTest : MultiValueConverterTestCase<HasNotificationsBrushConverter>
	{
		public override void TestConvert()
		{
			var entity = new Entity();
			AssertBrushConvertResult(new Color { A = 0, R = 255, G = 255, B = 255 }, new object[] { false, entity }, message: "An entity with no notifications gets an empty brush");
			AssertBrushConvertResult(new Color { A = 0, R = 255, G = 255, B = 255 }, new object[] { true, entity }, message: "An entity with no notifications gets an empty brush (even though HasNotifications is true)");

			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Error, string.Empty));
			AssertBrushConvertResult(Colors.Red, new object[] { true, entity }, message: "");

			entity.Notifications.Clear();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Warning, string.Empty));
			AssertBrushConvertResult(Colors.Orange, new object[] { true, entity }, message: "");

			entity.Notifications.Clear();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Message, string.Empty));
			AssertBrushConvertResult(Colors.DodgerBlue, new object[] { true, entity }, message: "");
		}
	}
}
