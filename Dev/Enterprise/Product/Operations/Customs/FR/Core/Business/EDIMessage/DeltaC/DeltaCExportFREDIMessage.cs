using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaCExportFREDIMessage : FREDIMessage
	{
		public DeltaCExportFREDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.EXC;
			EM_MessageSubType = Enterprise.Customs.FR.Business.MessageSubTypeList.Codes.EXC;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		protected override MessageDataObject GenerateMessageDataObject()
		{
			return EM_ReceiveTransmit == EDIMessage.Direction.Receive ? new DeltaCExportResponseMessageDataObject(this) : null;
		}
	}
}

