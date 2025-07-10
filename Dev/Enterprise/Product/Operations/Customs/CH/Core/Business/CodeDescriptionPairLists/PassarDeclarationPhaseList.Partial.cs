using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public partial class PassarDeclarationPhaseList
{
	public static ZString GetAppropriateEntryHeaderPhaseStatusCode(string messageType)
	{
		return messageType switch
		{
			PassarMessageTypeList.Codes.NE013 => Codes.Amendment,
			PassarMessageTypeList.Codes.NI013 => Codes.Amendment,
			PassarMessageTypeList.Codes.NE014 => Codes.Cancellation,
			PassarMessageTypeList.Codes.NI014 => Codes.Cancellation,
			PassarMessageTypeList.Codes.NE015 => Codes.Declaration,
			PassarMessageTypeList.Codes.NI015 => Codes.Declaration,
			PassarMessageTypeList.Codes.NE069 => Codes.Rectification,
			PassarMessageTypeList.Codes.NE130 => Codes.EDecToPassarDataTransfer,
			PassarMessageTypeList.Codes.NC016 => Codes.RequestDataJourney,
			PassarMessageTypeList.Codes.NC123 => Codes.Activation,
			_ => ZString.Empty
		};
	}
}
