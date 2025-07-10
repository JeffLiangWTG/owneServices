using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class AVSQueryMessage : EDIMessage
	{
		public AVSQueryMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.CFIAQuery;
			EM_MessageType = MessageTypeList.Codes.AVSQuery;
			EM_MessageSubType = MessageTypeList.Codes.AVSQuery;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		protected override string GetMessageReferenceNumber()
		{
			return "1";
		}
	}
}
