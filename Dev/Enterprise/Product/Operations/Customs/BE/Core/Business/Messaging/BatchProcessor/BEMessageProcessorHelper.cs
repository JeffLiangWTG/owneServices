using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public static class BEMessageProcessorHelper
{
	public static string[] GetOutgoingMessageTypes() => new[]
	{
		SendMessageTypes.Codes.NCT,
		SendMessageTypes.Codes.AES
	};

	public static ZString GetMessageDomainCode(ZString messageType)
	{
		switch (messageType)
		{
			case SendMessageTypes.Codes.NCT:
				return MessageVersionRegistry.NCTSP5DomainCode;
			case SendMessageTypes.Codes.AES:
				return MessageVersionRegistry.AESDomainCode;
			case SendMessageTypes.Codes.PN:
			case SendMessageTypes.Codes.TS:
				return MessageVersionRegistry.PNTSDomainCode;
			case SendMessageTypes.Codes.IMP:
				return MessageVersionRegistry.IDMSDomainCode;
			case SendMessageTypes.Codes.TSD:
				return MessageVersionRegistry.TSDDomainCode;
			case SendMessageTypes.Codes.REN:
				return MessageVersionRegistry.RENDomainCode;
			default:
				return ZString.Empty;
		}
	}
}
