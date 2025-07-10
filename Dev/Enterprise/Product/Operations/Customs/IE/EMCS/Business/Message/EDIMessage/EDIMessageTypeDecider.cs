using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.IEEMCS.IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result;
			switch (row[EDIMessageSchema.Constants.EM_ReceiveTransmit])
			{
				case EDIMessage.Direction.Transmit:
					result = typeof(EMCSOutboundEDIMessage);
					break;
				case EDIMessage.Direction.Receive:
					result = typeof(EMCSInboundEDIMessage);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(row), EDIMessageSchema.Constants.EM_ReceiveTransmit);
			}
			return result;
		}
	}
}
