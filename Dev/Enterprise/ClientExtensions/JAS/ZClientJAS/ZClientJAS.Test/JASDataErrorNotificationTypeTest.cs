using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS
{
	[TestedType(typeof(JASDataErrorNotificationType))]
	class JASDataErrorNotificationTypeTest : NotificationSubscriberTypeTest<JASDataErrorNotificationType>
	{
		public void TestErrorNotificationTypes()
		{
			AssertEquals("CannotCreateNewShipmentJob", JASDataErrorNotificationType.CannotCreateNewShipmentJob.Name);
			AssertEquals("Shipment Job cannot be created at this time", JASDataErrorNotificationType.CannotCreateNewShipmentJob.Message);
			AssertEquals("ShipmentJobAlreadyExistWithCharges", JASDataErrorNotificationType.ShipmentJobAlreadyExistWithCharges.Name);
			AssertEquals("Shipment Job and Charges already exist", JASDataErrorNotificationType.ShipmentJobAlreadyExistWithCharges.Message);
		}

		protected override JASDataErrorNotificationType NewNotificationType(string name)
		{
			return new JASDataErrorNotificationType(name, name);
		}

		protected override JASDataErrorNotificationType NewNotificationType(string name, string message)
		{
			return new JASDataErrorNotificationType(name, message);
		}
	}
}
