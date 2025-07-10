using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CODSendMessage : FREDIMessage
	{
		public CODSendMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.FRCustomsMessage;
			EM_MessageType = MessageTypeList.Codes.COD;
			EM_MessageSubType = MessageTypeList.Codes.COD;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}
	}
}
