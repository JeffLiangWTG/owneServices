using CargoWise.Common;
using CargoWise.Types;
using static Enterprise.Customs.IT.Business.MessageProcessorConstants;

namespace Enterprise.Customs.IT.Business;

static class Ucc6ResponseMessageFactory
{
	public static IResponseMessageWithWrapper GetResponseMessage(ZString responseMessageText, ZString declarationType)
	{
		Argument.NotNullOrEmpty(responseMessageText, nameof(responseMessageText));
		Argument.NotNullOrEmpty(declarationType, nameof(declarationType));

		switch (declarationType)
		{
			case Common.EU.EUJobMessageTypeList.Codes.Import:
				return new Ucc6ImportResponseMessage(responseMessageText);

			case Common.EU.EUJobMessageTypeList.Codes.Export:
				return new Ucc6ExportResponseMessage(responseMessageText);

			case InterchangeTypes.TransitType:
				return new NctsResponseMessage(responseMessageText);

			default:
				throw new CustomsMessageProcessorException("Unexpected application reference found");
		}
	}
}
