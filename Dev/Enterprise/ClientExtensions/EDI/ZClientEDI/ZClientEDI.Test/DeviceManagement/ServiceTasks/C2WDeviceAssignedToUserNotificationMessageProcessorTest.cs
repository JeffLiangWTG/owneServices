using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Client.EDI.DeviceManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks.Test
{
	class C2WDeviceAssignedToUserNotificationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			AssertEquals(EHubMessageType.C2WDeviceAssignedToUserNotification, processor.MessageType);
		}

		[ExpectNoExceptions]
		public void TestDoesNotCrashIfDeviceNotFound()
		{
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceAssignedToUserNotification, message_data = new C2WDeviceAssignedToUserNotificationMessage { device_friendly_identifier = "ZXCASDQWEASDQWEASDAZXC", parent_code = "BOB", parent_description = "Bob The Builder", parent_type = "GS" }.Serialize() };
			processor.Process(Factory, string.Empty, message);
		}

		public void TestSetsDeviceParentInfo()
		{
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceAssignedToUserNotification, message_data = new C2WDeviceAssignedToUserNotificationMessage { device_friendly_identifier = DeviceID, parent_code = "BOB", parent_description = "Bob The Builder", parent_type = "GS" }.Serialize() };
			processor.Process(Factory, string.Empty, message);
			AssertEquals("Bob The Builder (BOB)", device.CDH_ClientParentID);
			AssertEquals("GS", device.CDH_ClientParentType);
		}

		public void TestClearsDeviceParentInfo()
		{
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceAssignedToUserNotification, message_data = new C2WDeviceAssignedToUserNotificationMessage { device_friendly_identifier = DeviceID }.Serialize() };
			processor.Process(Factory, string.Empty, message);
			AssertEquals(string.Empty, device.CDH_ClientParentID);
			AssertEquals(string.Empty, device.CDH_ClientParentType);
		}

		const string DeviceID = "TEST00001";
		C2WDeviceAssignedToUserNotificationMessageProcessor processor;
		ClientDeviceHeader device;
		protected override void SetUp()
		{
			base.SetUp();
			device = Factory.New<ClientDeviceHeader>();
			device.CDH_Identifier = DeviceID;
			device.CDH_ModelID = "TESTDEV";
			device.CDH_IsTemplate = false;
			Factory.Save();
			processor = new C2WDeviceAssignedToUserNotificationMessageProcessor();
		}

		protected override void TearDown()
		{
			processor = null;
			base.TearDown();
		}
	}
}
