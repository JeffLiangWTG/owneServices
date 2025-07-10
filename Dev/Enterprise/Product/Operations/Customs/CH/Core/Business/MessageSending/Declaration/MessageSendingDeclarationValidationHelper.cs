using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public sealed class MessageSendingDeclarationValidationHelper
{
	public static void CheckJE_DeclarationLanguage(ZPropertyInfo declarationLanguageInfo)
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(declarationLanguageInfo);
	}

	public static void CheckJE_LocationOfGoods(ZPropertyInfo locationOfGoodsInfo)
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(locationOfGoodsInfo);
	}

	public static void CheckJE_TransportMode(PlausiValidation plausiValidation, ZPropertyInfo transportModeInfo, IMessageSendingDeclaration declaration)
	{
		plausiValidation.CheckCH0001(transportModeInfo, declaration);
		plausiValidation.CheckNS30003_TransportMode(transportModeInfo, declaration);
	}
}
