using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaDImportFREDIMessage : FREDIMessage
	{
		public DeltaDImportFREDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.IMD;
			EM_MessageSubType = Enterprise.Customs.FR.Business.MessageSubTypeList.Codes.IMD;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		protected override MessageDataObject GenerateMessageDataObject()
		{
			return EM_ReceiveTransmit == EDIMessage.Direction.Receive ? new DeltaDImportResponseMessageDataObject(this) : null;
		}
	}
}
