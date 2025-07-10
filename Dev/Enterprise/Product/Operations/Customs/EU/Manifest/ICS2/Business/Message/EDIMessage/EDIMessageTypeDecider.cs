using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.EU.IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result;
			var receiveTransmitValue = row[EDIMessageSchema.Constants.EM_ReceiveTransmit];
			switch (receiveTransmitValue)
			{
				case EDIMessage.Direction.Transmit:
					result = typeof(ICS2OutboundEDIMessage);
					break;
				case EDIMessage.Direction.Receive:
					result = typeof(ICS2InboundEDIMessage);
					break;
				default:
					throw new ArgumentOutOfRangeException(
						nameof(row),
						$"Invalid value for EM_ReceiveTransmit: {receiveTransmitValue}."
					);
			}
			return result;
		}
	}
}
