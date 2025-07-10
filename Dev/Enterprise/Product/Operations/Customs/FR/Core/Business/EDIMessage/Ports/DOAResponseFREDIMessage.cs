using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DOAResponseFREDIMessage : FREDIMessage
	{
		public DOAResponseFREDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.FRPortMessage;
			EM_MessageType = MessageTypeList.Codes.POR;
			EM_MessageSubType = Enterprise.Customs.FR.Business.MessageSubTypeList.Codes.DOA;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}
	}
}
