using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NCTSEDIMessageTypeDecider : TypeDecider, Integration.Customs.GBNCTS.INCTSEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result;
			switch (row[EDIMessageSchema.Constants.EM_ReceiveTransmit])
			{
				case EDIMessage.Direction.Transmit:
					result = typeof(NCTSOutboundEDIMessage);
					break;
				case EDIMessage.Direction.Receive:
					result = typeof(NCTSInboundEDIMessage);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(row), EDIMessageSchema.Constants.EM_ReceiveTransmit);
			}
			return result;
		}
	}
}
