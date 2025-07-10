using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	class IIDUniversalShipmentMessageInterpretationGenerator
	{
		internal static ZString GetInterpretatedHTML(IIDUniversalShipmentMessage message)
		{
			ZString messageDetail = message != null ? ZString.Empty : ZString.Empty;
			return messageDetail;
		}
	}
}
