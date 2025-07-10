using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeaderLookups;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks.Test
{
	class C2WDeviceRegistrationRequestMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			AssertEquals(EHubMessageType.C2WDeviceRegistrationRequest, processor.MessageType);
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

		public void TestProcess_NewDevice()
		{
			CreateValidLicence(Factory, "ABC", "XYZ");
			var devices = Factory.Load<ClientDeviceHeader>(new ZQuery());
			AssertEquals("Precondition: should not contain any dmg device headers", 0, devices.Length);
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceRegistrationRequest, message_data = new C2WDeviceRegistrationRequestMessage { device_client_identifier = "DEVICEID", device_key = new DeviceKey { kind = DeviceKind.Android, identifier = "436453643742" }, device_model = "TC77", }.Serialize(), };
			processor.Process(Factory, "ABCBLAXYZ", message);
			devices = Factory.Load<ClientDeviceHeader>(new ZQuery());
			AssertEquals(1, devices.Length);
			var device = devices[0];
			AssertEquals(Statuses.Active, device.CDH_Status);
			Assert(device.CDH_IsBYOD);
			AssertEquals("DEVICEID", device.CDH_Description);
			AssertEquals("ABC", device.CDH_EnterpriseCode);
			AssertEquals("XYZ", device.CDH_ServerCode);
			Assert(!device.CDH_IsTemplate);
			AssertEquals("436453643742", device.CDH_DeviceIdentifier);
			AssertEquals(Kinds.Android, device.CDH_DeviceKind);
			AssertEquals("TC77", device.CDH_ModelID);
		}

		public void TestProcess_ActivatingInactiveDevice()
		{
			CreateValidLicence(Factory, "ABC", "XYZ");
			var existingDevice = Factory.New<ClientDeviceHeader>();
			existingDevice.CDH_IsBYOD = true;
			existingDevice.CDH_Description = "DEVICEID";
			existingDevice.CDH_EnterpriseCode = "ABC";
			existingDevice.CDH_ServerCode = "XYZ";
			existingDevice.CDH_Status = Statuses.Inactive;
			existingDevice.CDH_DeviceKind = GlbDeviceKindCodes.Android;
			existingDevice.CDH_DeviceIdentifier = "436453643742";
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceRegistrationRequest, message_data = new C2WDeviceRegistrationRequestMessage { device_client_identifier = "DEVICEID", device_key = new DeviceKey { kind = DeviceKind.Android, identifier = "436453643742" }, device_model = "TC77", }.Serialize(), };
			processor.Process(Factory, "ABCBLAXYZ", message);
			var devices = Factory.Load<ClientDeviceHeader>(new ZQuery());
			AssertEquals(1, devices.Length);
			var device = devices[0];
			AssertEquals(Statuses.Active, device.CDH_Status);
			Assert(device.CDH_IsBYOD);
			AssertEquals(string.Empty, device.CDH_ModelID);
			AssertEquals("DEVICEID", device.CDH_Description);
			AssertEquals("ABC", device.CDH_EnterpriseCode);
			AssertEquals("XYZ", device.CDH_ServerCode);
			Assert(!device.CDH_IsTemplate);
			AssertEquals("436453643742", device.CDH_DeviceIdentifier);
			AssertEquals(Kinds.Android, device.CDH_DeviceKind);
		}

		public void TestProcess_ActivatingInactiveDeviceWhenDetailsChanged()
		{
			CreateValidLicence(Factory, "ABC", "XYZ");
			var existingDevice = Factory.New<ClientDeviceHeader>();
			existingDevice.CDH_IsBYOD = true;
			existingDevice.CDH_Description = "DEVICEID";
			existingDevice.CDH_EnterpriseCode = "ABC";
			existingDevice.CDH_ServerCode = "XYZ";
			existingDevice.CDH_Status = Statuses.Inactive;
			existingDevice.CDH_DeviceKind = GlbDeviceKindCodes.AppleMobile;
			existingDevice.CDH_DeviceIdentifier = "436453643742";
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceRegistrationRequest, message_data = new C2WDeviceRegistrationRequestMessage { device_client_identifier = "DEVICEID", device_key = new DeviceKey { kind = DeviceKind.Android, identifier = "436453643742" }, device_model = "TC77", }.Serialize(), };
			processor.Process(Factory, "ABCBLAXYZ", message);
			var devices = Factory.Load<ClientDeviceHeader>(new ZQuery());
			AssertEquals(1, devices.Length);
			var device = devices[0];
			AssertEquals(Statuses.Inactive, device.CDH_Status);
			AssertGeneratedEDIInterchange(Factory, "EDIAUSSYD", "ABCBLAXYZ", "DEVICEID", DeviceRegistrationRequestError.DetailsChanged);
		}

		public void TestProcess_ActiveDeviceAlreadyRegistered()
		{
			CreateValidLicence(Factory, "ABC", "XYZ");
			var existingDevice = Factory.New<ClientDeviceHeader>();
			existingDevice.CDH_IsBYOD = true;
			existingDevice.CDH_Description = "DEVICEID";
			existingDevice.CDH_EnterpriseCode = "ABC";
			existingDevice.CDH_ServerCode = "XYZ";
			var message = new EHubMessageContainer { message_type = EHubMessageType.C2WDeviceRegistrationRequest, message_data = new C2WDeviceRegistrationRequestMessage { device_client_identifier = "DEVICEID", device_key = new DeviceKey { kind = DeviceKind.Android, identifier = "436453643742" }, device_model = "TC77", }.Serialize(), };
			processor.Process(Factory, "ABCBLAXYZ", message);
			var devices = Factory.Load<ClientDeviceHeader>(new ZQuery());
			AssertEquals(1, devices.Length);
			var device = devices[0];
			Assert(device.CDH_IsBYOD);
			AssertEquals(Statuses.Active, device.CDH_Status);
			AssertEquals(string.Empty, device.CDH_ModelID);
			AssertEquals("DEVICEID", device.CDH_Description);
			AssertEquals("ABC", device.CDH_EnterpriseCode);
			AssertEquals("XYZ", device.CDH_ServerCode);
			Assert(!device.CDH_IsTemplate);
			AssertEquals(string.Empty, device.CDH_DeviceIdentifier);
			AssertEquals(Kinds.Unknown, device.CDH_DeviceKind);
			AssertGeneratedEDIInterchange(Factory, "EDIAUSSYD", "ABCBLAXYZ", "DEVICEID", DeviceRegistrationRequestError.Duplicated);
		}

		internal static void CreateValidLicence(BusinessObjectFactory factory, string enterpriseCode, string serverCode)
		{
			var licenceEnterprise = factory.New<LicenceEnterprise>();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "OOO";
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = enterpriseCode;
			var licenceDatabase = factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = serverCode;
			factory.Save();
		}

		internal static void AssertGeneratedEDIInterchange(BusinessObjectFactory factory, string from, string to, string deviceID, DeviceRegistrationRequestError errorCode)
		{
			var generatedInterchanges = factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_From, from).AddToFilter(EDIInterchangeSchema.EI_To, to).AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit).AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.Telematics).AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued).AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.eHub));
			AssertEquals(1, generatedInterchanges.Length);
			var interchange = generatedInterchanges[0];
			var element = XElement.Parse(interchange.EI_BodyText);
			var innerMessages = EHubMessageSerializer.Deserialize(element);
			AssertEquals(1, innerMessages.Count());
			var internalResponseMessage = innerMessages.First().GetInternalMessage<W2CDeviceRegistrationResponseMessage>();
			AssertNotNull(internalResponseMessage);
			AssertEquals(deviceID, internalResponseMessage.device_client_identifier);
			AssertEquals(errorCode, internalResponseMessage.error_code);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(ApplicationCodeList.Codes.Telematics, interchange.ContainedMessages[0].EM_ApplicationCode);
			AssertEquals(TelematicsMessageList.Codes.ProtobufData, interchange.ContainedMessages[0].EM_MessageSubType);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, interchange.ContainedMessages[0].EM_ReceiveTransmit);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, interchange.ContainedMessages[0].EM_Status);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, interchange.ContainedMessages[0].EM_Status);
			element = XElement.Parse(interchange.ContainedMessages[0].EM_MessageText);
			innerMessages = EHubMessageSerializer.Deserialize(element);
			AssertEquals(1, innerMessages.Count());
			internalResponseMessage = innerMessages.First().GetInternalMessage<W2CDeviceRegistrationResponseMessage>();
			AssertNotNull(internalResponseMessage);
			AssertEquals(deviceID, internalResponseMessage.device_client_identifier);
			AssertEquals(errorCode, internalResponseMessage.error_code);
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new C2WDeviceRegistrationRequestMessageProcessor();
		}

		IMessageProcessor processor;
	}
}
