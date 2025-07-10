using System;
using CargoWise.Customs.IL.MessageDefinitions.DLO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class MessageDeliveryOrderHeaderWrapper : IMessageDeliveryOrderHeader
	{
		MessageDeliveryOrderHeaderWrapper(DeliveryOrderDocDataObject deliveryOrderDocData, bool isCancelActionTypeCode)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
			this.isCancelActionTypeCode = isCancelActionTypeCode;
		}

		public static IMessageDeliveryOrderHeader NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData, bool isCancelActionTypeCode)
			=> deliveryOrderDocData != null ? new MessageDeliveryOrderHeaderWrapper(deliveryOrderDocData, isCancelActionTypeCode) : null;

		int IMessageDeliveryOrderHeader.ActionTypeCode => isCancelActionTypeCode ? CustomsDeliveryOrder.ActionTypeCodeCancel : CustomsDeliveryOrder.ActionTypeCodeNew;

		ICargoIdentifier IMessageDeliveryOrderHeader.CargoIdentifier => CargoIdentifierWrapper.NewOrNull(deliveryOrderDocData);

		DateTime? IMessageDeliveryOrderHeader.DeliveryOrderDate => ZDateTime.UtcNow.ToDateTime();

		int IMessageDeliveryOrderHeader.DeliveryOrderNumber => ZInt.Zero;

		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
		readonly bool isCancelActionTypeCode;
	}
}
