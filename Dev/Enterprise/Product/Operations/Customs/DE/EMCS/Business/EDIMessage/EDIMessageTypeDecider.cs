using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.DEEMCS.IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationReference = row[EDIMessageSchema.Constants.EM_ApplicationReference].ToString().Trim();
			var direction = row[EDIMessageSchema.Constants.EM_ReceiveTransmit].ToString().Trim();
			return GetEMCSTypeForLoad(applicationReference, direction);
		}

		Type GetEMCSTypeForLoad(string applicationReference, string direction)
		{
			if (direction == EDIMessage.Direction.Receive && EmcsResponseMessageDetails.Instance.ResponseMessages.TryGetValue(applicationReference, out var responseMessageDetails))
			{
				return responseMessageDetails.EDIMessageType;
			}
			else
			{
				return typeof(EmcsEDIMessage);
			}
		}
	}
}
