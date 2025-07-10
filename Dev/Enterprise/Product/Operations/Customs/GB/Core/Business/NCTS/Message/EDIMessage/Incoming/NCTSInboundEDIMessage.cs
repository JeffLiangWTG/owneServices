using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NCTSInboundEDIMessage : GbEDIMessage
	{
		public NCTSInboundEDIMessage(BusinessObjectFactory fact, DataRow row) : base(fact, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsNCTS;
		}
	}
}
