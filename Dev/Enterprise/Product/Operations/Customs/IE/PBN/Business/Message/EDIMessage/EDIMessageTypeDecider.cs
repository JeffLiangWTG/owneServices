using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.PBN.Business
{
	public sealed class EDIMessageTypeDecider : TypeDecider, Integration.Customs.IEPBN.IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result;
			switch (row[EDIMessageSchema.Constants.EM_ReceiveTransmit])
			{
				case EDIMessage.Direction.Receive:
					result = typeof(PBNInboundEDIMessage);
					break;
				case EDIInterchange.Direction.Transmit:
					result = typeof(PBNOutboundEDIMessage);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(row), EDIMessageSchema.Constants.EM_ReceiveTransmit);
			}
			return result;
		}
		public override Type GetTypeForNew() => null;
	}
}
