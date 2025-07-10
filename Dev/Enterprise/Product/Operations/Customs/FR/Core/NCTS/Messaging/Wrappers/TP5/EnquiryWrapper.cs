using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class EnquiryWrapper : IEnquiry
	{
		EnquiryWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
		}

		readonly TP5MessageSendingObject sendingObject;

		public static EnquiryWrapper New(TP5MessageSendingObject sendingObject) => sendingObject == null ? null : new EnquiryWrapper(sendingObject);

		public DateTime? TC11DeliveryDate => sendingObject.TC11DeliveryDate.IsEmpty ? null : sendingObject.TC11DeliveryDate.ToDateTime();

		public string Text => sendingObject.QueryInformation;
	}
}
