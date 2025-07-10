using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm;

sealed class SeaCgmCMCHI21AdditionalDataProvider : ISeaCgmCMCHI21AdditionalDataProvider
{
	public SeaCgmCMCHI21AdditionalDataProvider(ManifestMessageSendingObject messageSendingObject)
	{
		var sendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		messageType = sendingObject.MessageType;
	}

	string ISeaCgmCMCHI21AdditionalDataProvider.GetConsigneeAddress3(CGMAsycudaBill businessObject)
	{
		var consigneeABLAddress = businessObject.ConsigneeABLAddress;
		var builder = new ZStringBuilder();
		builder.AppendIfNotEmpty(consigneeABLAddress.City);
		builder.AppendIfNotEmpty(consigneeABLAddress.RN_NKCountryCode);
		builder.AppendIfNotEmpty(consigneeABLAddress.State);
		builder.AppendIfNotEmpty(consigneeABLAddress.Postcode);
		return builder.ToStringWithDelimiterBetweenAppends(AddressSeparator);
	}

	string ISeaCgmCMCHI21AdditionalDataProvider.GetImcoCode(CGMAsycudaBill businessObject)
	{
		var billImcoCode = businessObject.ImcoCode;
		return billImcoCode.IsEmpty ? NonhazardousImcoCode : billImcoCode;
	}

	string ISeaCgmCMCHI21AdditionalDataProvider.GetImporterAddress3(CGMAsycudaBill businessObject)
	{
		var buyerABLAddress = businessObject.BuyerABLAddress;
		var builder = new ZStringBuilder();
		builder.AppendIfNotEmpty(buyerABLAddress.City);
		builder.AppendIfNotEmpty(buyerABLAddress.RN_NKCountryCode);
		builder.AppendIfNotEmpty(buyerABLAddress.State);
		builder.AppendIfNotEmpty(buyerABLAddress.Postcode);
		return builder.ToStringWithDelimiterBetweenAppends(AddressSeparator);
	}

	string ISeaCgmCMCHI21AdditionalDataProvider.GetUnoCode(CGMAsycudaBill businessObject)
	{
		var billUnoCode = businessObject.UnoCode;
		return billUnoCode.IsEmpty ? NonhazardousUnoCode : billUnoCode;
	}

	string ISeaCgmCMCHI21AdditionalDataProvider.GetMessageType(CGMAsycudaBill businessObject)
	{
		return messageType;
	}

	string ISeaCgmCMCHI21AdditionalDataProvider.GetMessageType(CGMAsycudaPack businessObject)
	{
		return messageType;
	}

	const string NonhazardousUnoCode = "ZZZZZ";
	const string NonhazardousImcoCode = "ZZZ";
	const string AddressSeparator = " ";
	readonly ZString messageType;
}
