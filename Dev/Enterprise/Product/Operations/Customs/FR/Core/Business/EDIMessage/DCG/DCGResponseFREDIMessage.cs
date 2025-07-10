using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DCGResponseFREDIMessage : FREDIMessage
	{
		public DCGResponseFREDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.DCG;
			EM_MessageSubType = Enterprise.Customs.FR.Business.MessageSubTypeList.Codes.DCG;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		protected override MessageDataObject GenerateMessageDataObject()
		{
			return EM_ReceiveTransmit == EDIMessage.Direction.Receive ? new DCGResponseMessageDataObject(this) : null;
		}
	}
}
