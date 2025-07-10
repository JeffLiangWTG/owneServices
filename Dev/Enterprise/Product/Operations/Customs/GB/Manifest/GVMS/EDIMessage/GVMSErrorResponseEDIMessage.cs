using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSErrorResponseEDIMessage : GVMSEDIMessage
	{
		public GVMSErrorResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsGVMSManifest;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			EM_MessageType = Constants.GVMSMessageSubTypes.EHUBERRORRESPONSE;
		}
	}
}
