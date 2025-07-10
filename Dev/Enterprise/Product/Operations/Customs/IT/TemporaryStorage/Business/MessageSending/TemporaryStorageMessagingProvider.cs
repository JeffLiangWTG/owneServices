using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;
using ITCustomsStatusList = Enterprise.Customs.IT.TemporaryStorage.Business.CodeDescriptionPairLists.PNTSCustomsStatusList;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageMessagingProvider : EU.Business.CusTempStorage.TemporaryStorageMessagingProvider
{
	protected override ZString GetDefaultMessageTypeCore(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject sendingObject)
		=> sendingObject.Header.CustomsStatus == ITCustomsStatusList.Codes.Amending ? EDIMessageTypeList.Codes.Amendment : EDIMessageTypeList.Codes.NewDeclaration;

	protected override CodeDescriptionPairList GetMessageTypesCore(EU.Business.CusTempStorage.TemporaryStorageHeader header)
		=> header.Factory.GetCachedValue($"IT.TemporaryStorageMessagingProvider.GetMessageTypesCore.{header.CustomsStatus}", () =>
		{
			var result = new CodeDescriptionPairList();
			bool isAmending = header.CustomsStatus == ITCustomsStatusList.Codes.Amending;

			var (code, description) = isAmending
				? (EDIMessageTypeList.Codes.Amendment, EDIMessageTypeList.Descriptions.Amendment)
				: (EDIMessageTypeList.Codes.NewDeclaration, EDIMessageTypeList.Descriptions.NewDeclaration);

			result.AddPair(code, description);
			return result;
		});

	protected override TemporaryStorageMessageBuilder GetTemporaryStorageMessageBuilder(EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction)
		=> null;
}
