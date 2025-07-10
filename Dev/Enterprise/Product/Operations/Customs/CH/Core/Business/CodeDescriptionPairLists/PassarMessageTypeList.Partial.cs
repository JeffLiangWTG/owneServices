using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public partial class PassarMessageTypeList
{
	public static ZString GetMessageSubType(ZString messageType, ZString passarMessageType)
	{
		return typeMap.Where(x => x.messageType == messageType && x.passarMessageType == passarMessageType).Select(x => x.messageSubType).FirstOrDefault();
	}

	public static ZString GetPassarMessageType(ZString messageType, ZString messageSubType)
	{
		return typeMap.Where(x => x.messageType == messageType && x.messageSubType == messageSubType).Select(x => x.passarMessageType).FirstOrDefault();
	}

	public static bool IsCancellationMessage(string messageType)
	{
		return messageType switch
		{
			PassarMessageTypeList.Codes.NE014 => true,
			PassarMessageTypeList.Codes.NI014 => true,
			_ => false
		};
	}

	public static bool IsDataRequestMessage(string messageType)
	{
		return messageType switch
		{
			PassarMessageTypeList.Codes.NC016 => true,
			PassarMessageTypeList.Codes.NI016 => true,
			_ => false
		};
	}

	[SuppressThreadStaticFieldMessage]
	public readonly static (string messageType, string passarMessageType, string messageSubType)[] typeMap = new[]
	{
			(MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI013, MessageSubTypeCodeList.Codes.ImportAmendment),
			(MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI014, MessageSubTypeCodeList.Codes.ImportCancellation),
			(MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI015, MessageSubTypeCodeList.Codes.ImportDeclaration),
			(MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI016, MessageSubTypeCodeList.Codes.ImportLastResponse),
			(MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NC016, MessageSubTypeCodeList.Codes.PassarRequestDataJourney),
			(MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NC123, MessageSubTypeCodeList.Codes.NctsActivationAtDomicile),
			(MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE013, MessageSubTypeCodeList.Codes.PassarAmendment),
			(MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE014, MessageSubTypeCodeList.Codes.PassarWithdrawal),
			(MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE015, MessageSubTypeCodeList.Codes.PassarDeclaration),
			(MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE069, MessageSubTypeCodeList.Codes.PassarExportDeclarationRectification),
			(MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE130, MessageSubTypeCodeList.Codes.PassarEDecToPassarDataTransfer),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NC016, MessageSubTypeCodeList.Codes.PassarRequestDataJourney),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NC123, MessageSubTypeCodeList.Codes.NctsActivationAtDomicile),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT007, MessageSubTypeCodeList.Codes.PassarArrivalNotification),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT013, MessageSubTypeCodeList.Codes.PassarAmendment),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT014, MessageSubTypeCodeList.Codes.PassarWithdrawal),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT015, MessageSubTypeCodeList.Codes.PassarDeclaration),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT044, MessageSubTypeCodeList.Codes.PassarInventoryResult),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT141, MessageSubTypeCodeList.Codes.NctsNotArrivedTransitMovement),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT513, MessageSubTypeCodeList.Codes.NctsNationalDepartureAmendment),
			(MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT515, MessageSubTypeCodeList.Codes.NctsNationalDeparture),
		};
}
