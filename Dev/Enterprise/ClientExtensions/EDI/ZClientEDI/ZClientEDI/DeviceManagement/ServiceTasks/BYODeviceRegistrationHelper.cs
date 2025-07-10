using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks
{
	class BYODeviceRegistrationHelper
	{
		public static void GenerateResponseMessage(string from, BusinessObjectFactory factory, EHubMessageType messageType, byte[] messageContent)
		{
			var message = new EHubMessageContainer
			{
				message_type = messageType,
				message_data = messageContent,
			};

			var serializedBody = EHubMessageSerializer.SerializeToLimit(new List<EHubMessageContainer> { message }, EHubMessageSerializer.DefaultMessageSizeLimitInBytes);
			var messageText = serializedBody.ToString();

			var interchange = factory.New<EDIInterchange>();
			interchange.EI_To = from;
			interchange.EI_From = SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.Telematics;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_BodyText = messageText;

			var ediMessage = interchange.ContainedMessages.AddNew();
			ediMessage.EM_ApplicationCode = ApplicationCodeList.Codes.Telematics;
			ediMessage.EM_MessageSubType = TelematicsMessageList.Codes.ProtobufData;
			ediMessage.EM_MessageText = messageText;
			ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			ediMessage.EM_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			ediMessage.MessageNumberStrategy = new TelematicsMessageNumberStrategy(factory);
		}

		public static LicenceDatabase GetLicence(BusinessObjectFactory factory, string enterpriseCode, string serverCode)
		{
			var licenceQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
			licenceQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);

			var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			licenceQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);

			return factory.LoadTop1<LicenceDatabase>(licenceQuery);
		}

		public static ClientDeviceHeader LoadBYODevice(BusinessObjectFactory factory, string enterpriseCode, string serverCode, string clientIdentifier)
		{
			var query = new ZQuery(DmgDeviceHeaderSchema.CDH_IsBYOD, true)
				.AddToFilter(DmgDeviceHeaderSchema.CDH_EnterpriseCode, enterpriseCode)
				.AddToFilter(DmgDeviceHeaderSchema.CDH_ServerCode, serverCode)
				.AddToFilter(DmgDeviceHeaderSchema.CDH_Description, clientIdentifier);

			return factory.LoadTop1<ClientDeviceHeader>(query);
		}
	}
}
