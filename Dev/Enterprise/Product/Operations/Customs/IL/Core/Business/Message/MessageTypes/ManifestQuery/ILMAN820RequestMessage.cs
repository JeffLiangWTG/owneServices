using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN820RequestMessage : ILEDIRequestMessage
	{
		public ILMAN820RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.MAN;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ManifestQueryRequest;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILMAN820RequestMessageDataObject(this);
	}
}
