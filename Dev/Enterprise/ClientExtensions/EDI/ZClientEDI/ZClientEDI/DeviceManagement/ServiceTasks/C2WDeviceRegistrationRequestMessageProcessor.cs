using System;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using CargoWise.Types;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using static Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeaderLookups;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks
{
	class C2WDeviceRegistrationRequestMessageProcessor : IMessageProcessor
	{
		public EHubMessageType MessageType => EHubMessageType.C2WDeviceRegistrationRequest;

		public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer messageContainer)
		{
			if (string.IsNullOrEmpty(from) || from.Length != 9)
			{
				throw new ArgumentException($"'from' parameter must be a valid enterprise licence code, but was '{from}' instead.");
			}

			var enterpriseCode = from.Substring(0, 3);
			var serverCode = from.Substring(6, 3);

			// We will need licence.LicEnterprise.LE_OH in the Stage 2 for billing
			var licence = BYODeviceRegistrationHelper.GetLicence(factory, enterpriseCode, serverCode) ?? throw new ArgumentException($"Combination of Enterprise code {enterpriseCode} and Server code {serverCode} does not point to a valid licence.");

			var message = messageContainer.GetInternalMessage<C2WDeviceRegistrationRequestMessage>();

			var device = BYODeviceRegistrationHelper.LoadBYODevice(factory, enterpriseCode, serverCode, message.device_client_identifier);
			if (device == null)
			{
				device = factory.New<ClientDeviceHeader>();
				device.CDH_IsBYOD = true;
				device.CDH_Description = message.device_client_identifier;
				device.CDH_EnterpriseCode = enterpriseCode;
				device.CDH_ServerCode = serverCode;
				device.CDH_IsTemplate = false;
				device.CDH_ModelID = message.device_model;
				if (message.device_key != null)
				{
					device.CDH_DeviceKind = GlbDeviceKindCodes.Get(message.device_key.kind);
					device.CDH_DeviceIdentifier = message.device_key.identifier;
				}
			}
			else if (device.CDH_Status == Statuses.Inactive)
			{
				var deviceKind = message.device_key != null ? GlbDeviceKindCodes.Get(message.device_key.kind) : GlbDeviceKindCodes.Unknown;
				var deviceIdentifier = message.device_key != null ? (ZString)message.device_key.identifier : default;
				if (device.CDH_DeviceKind != deviceKind || device.CDH_DeviceIdentifier != deviceIdentifier)
				{
					BYODeviceRegistrationHelper.GenerateResponseMessage(from, factory, EHubMessageType.W2CDeviceRegistrationResponse, new W2CDeviceRegistrationResponseMessage
					{
						device_client_identifier = message.device_client_identifier,
						error_code = DeviceRegistrationRequestError.DetailsChanged
					}.Serialize());
				}
				else
				{
					device.CDH_Status = Statuses.Active;
				}
			}
			else
			{
				BYODeviceRegistrationHelper.GenerateResponseMessage(from, factory, EHubMessageType.W2CDeviceRegistrationResponse, new W2CDeviceRegistrationResponseMessage
				{
					device_client_identifier = message.device_client_identifier,
					error_code = DeviceRegistrationRequestError.Duplicated
				}.Serialize());
			}
		}
	}
}
