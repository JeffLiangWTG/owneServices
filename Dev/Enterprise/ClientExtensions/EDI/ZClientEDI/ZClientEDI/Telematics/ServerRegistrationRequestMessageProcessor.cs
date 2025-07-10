using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Telematics.ServiceTasks;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Client.EDI.Telematics
{
	class ServerRegistrationRequestMessageProcessor : ITelematicsXmlMessageProcessor
	{
		public ServerRegistrationRequestMessageProcessor(ILogger logger) : this(logger, new EHubMessageSender())
		{
		}

		internal ServerRegistrationRequestMessageProcessor(ILogger logger, IEHubMessageSender eHubMessageSender)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.eHubMessageSender = eHubMessageSender ?? throw new ArgumentNullException(nameof(eHubMessageSender));
		}

		static void UpdateRegisteredMiddlewareServices(string eHubId)
		{
			var description = (NoResString)"Middle-ware Service";
			var list = EDIDataRegistry.Instance.ActiveMiddlewareService.Value.Count == 0
				? EDIDataRegistry.Instance.ActiveMiddlewareService.DefaultValue
				: EDIDataRegistry.Instance.ActiveMiddlewareService.Value;

			if (list.FindByCode(eHubId) is CodeDescriptionBool codeDescriptionBool)
			{
				codeDescriptionBool.Description = description;
				codeDescriptionBool.Bool = true;
			}
			else
			{
				list.Add(TelematicsServiceCodeMaxLength, eHubId, description, true);
			}

			EDIDataRegistry.Instance.ActiveMiddlewareService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
		}

		void SendResponseMessage(BusinessObjectFactory factory, ServerRegistrationRequestMessage configRequest)
		{
			DeviceAssignmentChangeMessage GenerateDeviceAssignmentChangeMessage()
			{
				return new DeviceAssignmentChangeMessage
				{
					AssignDevicesToClients = LoadDeviceHeaders()
						.Select(header => new AssignDeviceToClient
						{
							CargoWiseOneLicense = header.Licence?.LicenceCodeForSystemMessage,
							DeviceHardwareIdentifier = header.CDH_DeviceIdentifier,
							DeviceModel = header.CDH_Description,
							DeviceHumanReadableIdentifier = header.CDH_Identifier,
							DeviceAssignmentTime = ZDateTimeOffset.Now.ToDateTimeOffset(),
						})
						.Where(client => !string.IsNullOrWhiteSpace(client.CargoWiseOneLicense))
						.ToList(),
				};

				ClientDeviceHeader[] LoadDeviceHeaders()
					{
					var zQuery = new ZQuery(DmgDeviceHeaderSchema.CDH_DeviceKind, SQLComparisonOperator.Equal, ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded)
							.AddToFilter(DmgDeviceHeaderSchema.CDH_IsTemplate, SQLComparisonOperator.Equal, false)
							.AddToFilter(DmgDeviceHeaderSchema.CDH_Status, SQLComparisonOperator.Equal, ClientDeviceHeaderLookups.Statuses.Active);
						return factory.Load<ClientDeviceHeader>(zQuery);
					}
			}

			var messages = new[]
			{
				XmlDataSerializer.SerializeToTelematicsXmlData(GenerateDeviceAssignmentChangeMessage())
			};

			eHubMessageSender.Send(
				factory,
				messages,
				new[] { configRequest.EHubId });
		}

		public int Process(BusinessObjectFactory factory, string messageText)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			if (string.IsNullOrWhiteSpace(messageText) || !XmlDataSerializer.TryDeserialize<ServerRegistrationRequestMessage>(messageText, out var configRequest))
			{
				return 0;
			}

			if (Env.CurrentCompany.GetLicenceCode() != SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value)
			{
				logger.Warning($"{Env.CurrentCompany.GetLicenceCode()} is invalid for this use. Functionality only available on {SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value}");
				return 0;
			}

			SendResponseMessage(factory, configRequest);

			UpdateRegisteredMiddlewareServices(configRequest.EHubId);

			logger.Debug($"Telematics Middle-ware Service [{configRequest.EHubId}] is registered.");
			return 1;
		}

		internal const int TelematicsServiceCodeMaxLength = 32;
		readonly IEHubMessageSender eHubMessageSender;
		readonly ILogger logger;
	}
}
