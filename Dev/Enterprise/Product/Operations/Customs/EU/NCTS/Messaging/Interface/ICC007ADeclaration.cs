using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ICC007ADeclaration : IDeclaration
	{
		ITrader Destination { get; }
		ZString ArrivalNotificationPlace { get; }
		ZString ArrivalNotificationPlaceLanguage { get; }
		ZString ArrivalAgreedLocationOfGoodsCode { get; }
		ZString ArrivalAgreedLocationOfGoodsLanguage { get; }
		ZString DialogLanguageIndicatorAtDestination { get; }
		ZString ArrivalNotificationDate { get; }
		ZBool IsSimplifiedArrivalProcedure { get; }
		ZString CustomsSubPlace { get; }
		ZString DeclarantTIN { get; }
		ZString CustomsPresentationOfficeRefNumber { get; }
		IReadOnlyCollection<IEnRouteEvent> EnRouteEvents { get; }
	}
}
