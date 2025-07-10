using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Business
{
	public class ILDLO120RequestMessage : ILEDIRequestMessage
	{
		public ILDLO120RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DLO;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.DeliveryOrderRequest;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILDLO120RequestMessageDataObject(this);

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();

			FillInDeliveryOrderPlaceHolders();
		}

		void FillInDeliveryOrderPlaceHolders()
		{
			const string deliveryOrderNumber = "deliveryOrderNumber";
			const string deliveryOrderNumberDefaultSection = $"<{deliveryOrderNumber}>0</{deliveryOrderNumber}>";
			if (EM_LinkedObject is ForwardingShipment shipment)
			{
				if (!shipment.IsInDatabase || shipment.JS_DLO.IsEmpty)
				{
					shipment.JS_DLO = Env.NumberFountains.ILDeliveryOrderNumber.GetNextFormatted(shipment.Factory);
				}

				var messageText = EM_MessageText;
				if (messageText.IndexOf(deliveryOrderNumberDefaultSection, StringComparison.InvariantCulture) != -1)
				{
					EM_MessageText = messageText.Replace(deliveryOrderNumberDefaultSection, $"<{deliveryOrderNumber}>{shipment.JS_DLO}</{deliveryOrderNumber}>");
				}
			}
		}
	}
}
