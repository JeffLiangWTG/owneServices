using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaCImportFREDIMessage : FREDIMessage
	{
		public DeltaCImportFREDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.IMC;
			EM_MessageSubType = Enterprise.Customs.FR.Business.MessageSubTypeList.Codes.IMC;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		protected override MessageDataObject GenerateMessageDataObject()
		{
			return EM_ReceiveTransmit == EDIMessage.Direction.Receive ? new DeltaCImportResponseMessageDataObject(this) : null;
		}
	}
}
