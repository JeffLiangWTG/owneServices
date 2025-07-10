#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class NotificationsPropertyDescriptorWithAttributedNotificationLogicTests : TestCase
	{
		public void TestMandatoryValidation()
		{
			TestComponent entity = new TestComponent();
			entity.NotMandatory = null;
			AssertEquals("Not mandatory, shouldnt have errors", false, MetaData.GetNotifications(entity, TestComponent.Properties.NotMandatory).HasErrors());
			entity.NotMandatory = "splaty";
			AssertEquals("Not mandatory, shouldnt have errors", false, MetaData.GetNotifications(entity, TestComponent.Properties.NotMandatory).HasErrors());

			entity.Mandatory = null;
			AssertEquals("Should have errors as it is mandatory", true, MetaData.GetNotifications(entity, TestComponent.Properties.Mandatory).HasErrors());
			entity.Mandatory = "";
			AssertEquals("Should have errors as it is mandatory", true, MetaData.GetNotifications(entity, TestComponent.Properties.Mandatory).HasErrors());
			entity.Mandatory = "s";
			AssertEquals("Not empty, shouldnt have errors", false, MetaData.GetNotifications(entity, TestComponent.Properties.Mandatory).HasErrors());
		}

		public void TestMandatoryWithOtherNotifications()
		{
			TestComponent entity = new TestComponent();
			entity.PropertyWithMandatoryWithOtherNotifications = null;
			AssertEquals(
				"Should have mandatory error and other errors",
				2, NotificationCollection.Cast(MetaData.GetNotifications(entity, TestComponent.Properties.PropertyWithMandatoryWithOtherNotifications)).Count);
		}

		internal class TestComponent : KComponent
		{
			public class Properties
			{
				public static PropertyDescriptor Mandatory
				{ get { return new TestComponent().GetProperties()["Mandatory"]; } }

				public static PropertyDescriptor NotMandatory
				{ get { return new TestComponent().GetProperties()["NotMandatory"]; } }

				public static PropertyDescriptor PropertyWithMandatoryWithOtherNotifications
				{ get { return new TestComponent().GetProperties()["PropertyWithMandatoryWithOtherNotifications"]; } }
			}

			[Mandatory]
			public string Mandatory
			{
				get { return mandatory; }
				set { mandatory = value; }
			}
			string mandatory;

			[Mandatory]
			[NotificationsMember("OtherNotifications")]
			public string PropertyWithMandatoryWithOtherNotifications
			{
				get { return propertyWithMandatoryWithOtherNotifications; }
				set { propertyWithMandatoryWithOtherNotifications = value; }
			}
			string propertyWithMandatoryWithOtherNotifications;

			public NotificationCollection OtherNotifications
			{
				get
				{
					NotificationCollection result = new NotificationCollection();
					result.AddError("other notification");
					return result;
				}
			}

			public string NotMandatory
			{
				get { return notMandatory; }
				set { notMandatory = value; }
			}
			string notMandatory;
		}
	}
}
#endif
