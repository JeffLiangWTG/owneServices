using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(HasNotificationsTooltipConverter))]
	class HasNotificationsTooltipConverterTest : MultiValueConverterTestCase<HasNotificationsTooltipConverter>
	{
		public override void TestConvert()
		{
			var entity = new Entity();
			AssertConvertResult(null, new object[] { false, entity }, message: "An entity with no notifications gets a null tooltip");
			AssertConvertResult(null, new object[] { true, entity }, message: "An entity with no notifications gets a null tooltip (even though HasNotifications is true)");

			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Error, "Dat Error", "Dis Shape"));
			AssertConvertResult("Dis Shape:\r\n    Error: Dat Error", new object[] { true, entity }, message: "");

			entity.Notifications.Clear();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Warning, "Dis Warning", "Dis Shape"));
			AssertConvertResult("Dis Shape:\r\n    Warning: Dis Warning", new object[] { true, entity }, message: "");

			entity.Notifications.Clear();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Message, "This is a massage", "Dis Shape"));
			AssertConvertResult("Dis Shape:\r\n    Message: This is a massage", new object[] { true, entity }, message: "");
		}

		public void TestConvert_DuplicateNotifications()
		{
			var entity = new Entity();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Error, "Dat Error", "Dis Shape"));
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Error, "Dat Error", "Dis Shape"));

			AssertConvertResult("Dis Shape:\r\n    Error: Dat Error", new object[] { true, entity }, message: "Should filter out duplicate notifications");
		}
	}
}
