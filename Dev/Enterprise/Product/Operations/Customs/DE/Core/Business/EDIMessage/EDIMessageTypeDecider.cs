using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.Business
{
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.DE.IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			var applicationReference = row[EDIMessageSchema.Constants.EM_ApplicationReference].ToString().Trim();
			var direction = row[EDIMessageSchema.Constants.EM_ReceiveTransmit].ToString().Trim();
			var messageSubType = row[EDIMessageSchema.Constants.EM_MessageSubType].ToString().Trim();
			switch (messageType)
			{
				case EDIMessageTypeList.Codes.AES:
					return GetAesTypeForLoad(applicationReference, direction, messageSubType);
				case EDIMessageTypeList.Codes.EMCS:
					return ((TypeDecider)ObjectFactory.Get<Integration.Customs.DEEMCS.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessageTypeList.Codes.NCTS:
				case EDIMessageTypeList.Codes.TemporaryStorage:
				case EDIMessageTypeList.Codes.Import:
					return GetATLASTypeForLoad(applicationReference, direction, messageSubType);
				default:
					return typeof(EDIMessage);
			}
		}

		Type GetAesTypeForLoad(string applicationReference, string direction, string messageSubType)
		{
			if (AesResponseMessageDetails.Instance.ResponseMessages.TryGetValue(applicationReference, out var responseMessageDetails))
			{
				return responseMessageDetails.EDIMessageType;
			}
			else if (direction == Direction.Transmit && messageSubType == ExportMessageSubTypeList.Codes.EXQ)
			{
				return typeof(StatusRequest);
			}
			else
			{
				return typeof(AesEDIMessage);
			}
		}

		Type GetATLASTypeForLoad(ZString applicationReference, string direction, string messageSubType)
		{
			if (direction == Direction.Transmit && messageSubType == NctsMessageSubTypeList.Codes.StatusRequestMessage)
			{
				return typeof(StatusRequest);
			}
			else if (ATLASResponseMessageDetails.Instance.ResponseMessages.TryGetValue(applicationReference, out var responseMessageDetailsCollective))
			{
				return responseMessageDetailsCollective.EDIMessageType;
			}

			else
			{
				return typeof(AtlasEDIMessage);
			}
		}
	}
}
