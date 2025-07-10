using System;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using static Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeaderLookups;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks
{
	class C2WDeviceDeregistrationRequestMessageProcessor : IMessageProcessor
	{
		public EHubMessageType MessageType => EHubMessageType.C2WDeviceDeregistrationRequest;

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

			var message = messageContainer.GetInternalMessage<C2WDeviceDeregistrationRequestMessage>();
			var device = BYODeviceRegistrationHelper.LoadBYODevice(factory, enterpriseCode, serverCode, message.device_client_identifier);
			if (device != null)
			{
				device.CDH_Status = Statuses.Inactive;
			}
			else
			{
				BYODeviceRegistrationHelper.GenerateResponseMessage(from, factory, EHubMessageType.W2CDeviceRegistrationResponse, new W2CDeviceRegistrationResponseMessage
				{
					device_client_identifier = message.device_client_identifier,
					error_code = DeviceRegistrationRequestError.NotFound
				}.Serialize());
			}
		}
	}
}
