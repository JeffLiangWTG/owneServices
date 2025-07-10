using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using static Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeaderLookups;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks.Test
{
	class C2WDeviceDeregistrationNotificationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			AssertEquals(EHubMessageType.C2WDeviceDeregistrationRequest, processor.MessageType);
		}

		public void TestProcess_FromIdIsMissingOrInvalidFormat()
		{
			AssertExceptionThrown<ArgumentException>("invalid from", "'from' parameter must be a valid enterprise licence code, but was 'AAAB' instead.", () => processor.Process(Factory, "AAAB", new EHubMessageContainer()));
			AssertExceptionThrown<ArgumentException>("empty from", "'from' parameter must be a valid enterprise licence code, but was '' instead.", () => processor.Process(Factory, string.Empty, new EHubMessageContainer()));
			AssertExceptionThrown<ArgumentException>("null from", "'from' parameter must be a valid enterprise licence code, but was '' instead.", () => processor.Process(Factory, null, new EHubMessageContainer()));
		}

		public void TestProcess_FromIdInvalidLicence()
		{
			AssertExceptionThrown<ArgumentException>("invalid licence", "Combination of Enterprise code AAA and Server code XYZ does not point to a valid licence.", () => processor.Process(Factory, "AAABVVXYZ", new EHubMessageContainer()));
		}

		public void TestProcess_ExistingDevice()
		{
			C2WDeviceRegistrationRequestMessageProcessorTest.CreateValidLicence(Factory, "ABC", "XYZ");
			var existingDevice = Factory.New<ClientDeviceHeader>();
			existingDevice.CDH_IsBYOD = true;
			existingDevice.CDH_Description = "DEVICEID";
			existingDevice.CDH_EnterpriseCode = "ABC";
			existingDevice.CDH_ServerCode = "XYZ";
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceRegistrationRequest, message_data = new C2WDeviceDeregistrationRequestMessage { device_client_identifier = "DEVICEID", }.Serialize(), };
			processor.Process(Factory, "ABCBLAXYZ", message);
			var devices = Factory.Load<ClientDeviceHeader>(new ZQuery());
			AssertEquals(1, devices.Length);
			var device = devices[0];
			AssertEquals(Statuses.Inactive, device.CDH_Status);
			Assert(device.CDH_IsBYOD);
			AssertEquals("DEVICEID", device.CDH_Description);
			AssertEquals("ABC", device.CDH_EnterpriseCode);
			AssertEquals("XYZ", device.CDH_ServerCode);
		}

		public void TestProcess_IncactivatingInactiveDevice()
		{
			C2WDeviceRegistrationRequestMessageProcessorTest.CreateValidLicence(Factory, "ABC", "XYZ");
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceRegistrationRequest, message_data = new C2WDeviceDeregistrationRequestMessage { device_client_identifier = "DEVICEID", }.Serialize(), };
			processor.Process(Factory, "ABCBLAXYZ", message);
			C2WDeviceRegistrationRequestMessageProcessorTest.AssertGeneratedEDIInterchange(Factory, "EDIAUSSYD", "ABCBLAXYZ", "DEVICEID", DeviceRegistrationRequestError.NotFound);
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new C2WDeviceDeregistrationRequestMessageProcessor();
		}

		IMessageProcessor processor;
	}
}
