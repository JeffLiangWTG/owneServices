using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	sealed class Message1220Processor : BaseMessageProcessorForSingleNumber<MnNg1220Msg22DeliveryOrderFeedBackMessage>
	{
		internal Message1220Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4643EA23-69BD-4C85-964E-737C5B440592", "IL Delivery Order Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { ILMessageTypeList.Codes.DLO };

		protected override ZString CouldNotLocateMessage => Res.GetString("8B660FDA-034B-41F9-A905-9F715D91F5C7", "Could not locate Shipment by Delivery Order Number #");

		protected override ZString MoreThanOneMessage => Res.GetString("81FAA705-908D-4002-A00A-BAE77520F5B7", "Found more than one shipment with the same Delivery Order Number – please check");

		protected override ZString EntryType => CusEntryNumberTypes.Israel.DeliveryOrderNumber;

		protected override ZString GetMessageReferenceNumber(MnNg1220Msg22DeliveryOrderFeedBackMessage responseMessage)
		{
			var deliveryOrderResponse = responseMessage.DeliveryOrderResponse?.FirstOrDefault();
			var deliveryOrderNumber = deliveryOrderResponse?.DeliveryOrderNumber.ToString();
			return deliveryOrderNumber;
		}

		protected override ZDateTimeOffset GetResponseDateTime(MnNg1220Msg22DeliveryOrderFeedBackMessage responseMessage)
			=> responseMessage.ResponseContentHeader.TransmitionDateTime;

		protected override bool SupportsConsol => false;

		protected override ElectronicFormEventLogManager<MnNg1220Msg22DeliveryOrderFeedBackMessage> GetElectronicFormEventLogManager()
			=> new DeliveryOrderEventLogManager();

		protected override void ClearMessageReference(EnterpriseBusinessObject enterpriseBusinessObject)
		{
			if (enterpriseBusinessObject is ForwardingShipment shipment)
			{
				shipment.DeliveryOrderProvider.MessageReferenceNumber = ZString.Empty;
			}
		}

		protected override ZString GetApplicationId(MnNg1220Msg22DeliveryOrderFeedBackMessage responseMessage)
		{
			return responseMessage.ResponseContentHeader?.ApplicationId.ToString();
		}

		protected override ZString GetStatusName(BusinessObjectFactory factory, MnNg1220Msg22DeliveryOrderFeedBackMessage responseMessage)
		{
			if (responseMessage.DeliveryOrderResponse != null
				&& responseMessage.DeliveryOrderResponse.Count >= 1
				&& responseMessage.DeliveryOrderResponse[0] is MnNg1220Msg22DeliveryOrderFeedBackMessageDeliveryOrderResponse deliveryOrderResponse)
			{
				var deliveryOrderResponseStatus = deliveryOrderResponse.ResponseStatus.ToString();
				return new DeliveryOrderResponseStatusList().GetDescriptionFromCode(deliveryOrderResponseStatus);
			}
			return ZString.Empty;
		}
	}
}
