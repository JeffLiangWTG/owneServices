using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSErrorResponseEDIMessage : CDSEDIMessage
	{
		public CDSErrorResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.EHubErrorResponse;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}
	}
}
