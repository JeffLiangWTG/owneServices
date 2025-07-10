using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm;

sealed class AirCgmCMCHI01AdditionalDataProvider : IAirCgmCMCHI01AdditionalDataProvider
{
	public AirCgmCMCHI01AdditionalDataProvider(ManifestMessageSendingObject messageSendingObject)
	{
		var sendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		messageType = sendingObject.MessageType;
	}

	public string GetMessageType(CGMAsycudaManifestHeader businessObject)
	{
		if (messageType == ManifestMessageTypeList.Codes.Amendment)
		{
			return businessObject.MasterBill.ABL_BillStatus == ManifestMessageTypeList.Codes.Amendment
				? ManifestMessageTypeList.Codes.Amendment
				: string.Empty;
		}
		return messageType;
	}

	public string GetMessageType(CGMAsycudaBill businessObject)
	{
		if (messageType == ManifestMessageTypeList.Codes.Amendment)
		{
			return businessObject.ABL_BillStatus != ManifestMessageTypeList.Codes.Fresh
				? businessObject.ABL_BillStatus
				: string.Empty;
		}
		return messageType;
	}

	readonly ZString messageType;
}
