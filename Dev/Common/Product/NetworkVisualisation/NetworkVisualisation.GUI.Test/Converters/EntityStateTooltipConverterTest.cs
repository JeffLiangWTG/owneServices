using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(EntityStateTooltipConverter))]
	class EntityStateTooltipConverterTest : MultiValueConverterTestCase<EntityStateTooltipConverter>
	{
		public override void TestConvert()
		{
			var entity = new Entity();

			AssertConvertResult("This shape has been approved", new object[] { EntityState.Fixed | EntityState.Approved | EntityState.NotApproved, entity });
			AssertConvertResult("This shape has not been approved", new object[] { EntityState.Fixed | EntityState.NotApproved, entity });
			AssertConvertResult("This shape's position and size are fixed", new object[] { EntityState.Fixed, entity });

			AssertConvertResult(null, new object[] { EntityState.None, entity });
		}

		public void TestConvert_WhenEntityHasError()
		{
			var entity = new Entity();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Error, "Dat Error", "Dis Shape"));

			AssertConvertResult("Dis Shape:\r\n    Error: Dat Error", new object[] { EntityState.Fixed | EntityState.Approved | EntityState.NotApproved | EntityState.HasErrors, entity });

			entity.EntityState = EntityState.Approved;
			AssertConvertResult("This shape has been approved", new object[] { EntityState.Fixed | EntityState.Approved | EntityState.NotApproved, entity });
		}

		public void TestConvert_WhenEntityHasWarning()
		{
			var entity = new Entity();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Warning, "Dat Warning", "Dis Shape"));

			AssertConvertResult("Dis Shape:\r\n    Warning: Dat Warning", new object[] { EntityState.Fixed | EntityState.Approved | EntityState.NotApproved | EntityState.HasErrors, entity });

			entity.EntityState = EntityState.Approved;
			AssertConvertResult("This shape has been approved", new object[] { EntityState.Fixed | EntityState.Approved | EntityState.NotApproved, entity });
		}

		public void TestConvert_WhenEntityHasMessage()
		{
			var entity = new Entity();
			entity.Notifications.Add(new EntityNotification(EntityNotifcationType.Message, "Dat Message", "Dis Shape"));

			AssertConvertResult("Dis Shape:\r\n    Message: Dat Message", new object[] { EntityState.Fixed | EntityState.Approved | EntityState.NotApproved | EntityState.HasMessages, entity });

			entity.EntityState = EntityState.Approved;
			AssertConvertResult("This shape has been approved", new object[] { EntityState.Fixed | EntityState.Approved | EntityState.NotApproved, entity });
		}
	}
}
