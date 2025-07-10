using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.CusTempStorage;

public class TemporaryStorageMessagingProvider : EU.Business.CusTempStorage.TemporaryStorageMessagingProvider
{
	protected override EU.Business.CusTempStorage.TemporaryStorageMessageBuilder GetTemporaryStorageMessageBuilder(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction) => new TemporaryStorageMessageBuilder((TemporaryStorageMessageSendingObject)messageSendingObject, messageFunction);

	protected override CodeDescriptionPairList GetMessageTypesCore(EU.Business.CusTempStorage.TemporaryStorageHeader header)
	{
		var codeDescriptionPairList = new CodeDescriptionPairList();
		switch (header.AMA_MessageType)
		{
			case PNTSMessageTypeList.Codes.Transfer:
				codeDescriptionPairList.AddPair(PNTSEntryTypeList.Codes.TransferNotification, PNTSEntryTypeList.Descriptions.TransferNotification);
				break;
			case PNTSMessageTypeList.Codes.Deconsolidation:
				codeDescriptionPairList.AddPair(PNTSEntryTypeList.Codes.DeconsolidationNotification, PNTSEntryTypeList.Descriptions.DeconsolidationNotification);
				break;
			case PNTSMessageTypeList.Codes.CombinedTemporaryStorage:
				codeDescriptionPairList.AddPair(PNTSEntryTypeList.Codes.CombinedTemporaryStorage, PNTSEntryTypeList.Descriptions.CombinedTemporaryStorage);
				break;
		}
		return codeDescriptionPairList;
	}

	protected override ZString GetDefaultMessageTypeCore(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject sendingObject)
	{
		var result = ZString.Empty;
		var header = (TemporaryStorageHeader)sendingObject.Header;
		switch ((string)header.AMA_MessageType)
		{
			case PNTSMessageTypeList.Codes.Transfer:
				result = PNTSEntryTypeList.Codes.TransferNotification;
				break;
			case PNTSMessageTypeList.Codes.Deconsolidation:
				result = PNTSEntryTypeList.Codes.DeconsolidationNotification;
				break;
			case PNTSMessageTypeList.Codes.CombinedTemporaryStorage:
				result = PNTSEntryTypeList.Codes.CombinedTemporaryStorage;
				break;
		}

		if (result.IsEmpty)
		{
			result = GetDefaultMessageIfTheThereIsOnlyOne();
		}

		return result;

		string GetDefaultMessageIfTheThereIsOnlyOne()
		{
			var messageTypes = sendingObject.Lookups.MessageTypes;
			return (messageTypes.Count == 1) ? messageTypes.CodesAsString : string.Empty;
		}
	}

	protected override IEnumerable<TemporaryStorageMessageFunction> GetMessageFunctions(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject messageSendingObject)
	{
		yield return new CombinedTSDMessageFunction();
		yield return new DeconsolidationNotificationTSDMessageFunction();
	}
}
