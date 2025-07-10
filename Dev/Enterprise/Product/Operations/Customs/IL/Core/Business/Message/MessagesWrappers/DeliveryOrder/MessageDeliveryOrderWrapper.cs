using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	public class MessageDeliveryOrderWrapper : IMessageDeliveryOrder
	{
		MessageDeliveryOrderWrapper(DeliveryOrderDocDataObject deliveryOrderDocData, bool isCancelActionTypeCode)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
			this.isCancelActionTypeCode = isCancelActionTypeCode;
		}

		public static IMessageDeliveryOrder NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData, bool isCancelActionTypeCode)
			=> deliveryOrderDocData != null ? new MessageDeliveryOrderWrapper(deliveryOrderDocData, isCancelActionTypeCode) : null;

		ICollection<IMessageDeliveryOrderDetails> IMessageDeliveryOrder.DeliveryOrder
			=> new Collection<IMessageDeliveryOrderDetails>() { MessageDeliveryOrderDetailsWrapper.NewOrNull(deliveryOrderDocData, isCancelActionTypeCode) };

		IRequestContentHeader IMessageDeliveryOrder.RequestContentHeader => RequestContentHeaderWrapper.New();

		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
		readonly bool isCancelActionTypeCode;
	}
}
