using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ICC044ADeclaration : IDeclaration
	{
		IUnloadingRemarkInterface UnloadingRemark { get; }
		ZString HeaderUnloadingNotes { get; }
		ZString HeaderUnloadingNotesLanguage { get; }
		IReadOnlyCollection<IControlResult> ControlResultList { get; }
		IReadOnlyCollection<IEnRouteEvent> EnRouteEvents { get; }
		ZString IdentityOfMeansOfTransportAtDeparture { get; }
		ZString IdentityOfMeansOfTransportAtDepartureLanguage { get; }
		ZString NationalityOfMeansOfTransportAtDeparture { get; }
		ZInt TotalNumberOfItems { get; }
		ZLong TotalNumberOfPackages { get; }
		ZDecimal TotalGrossMass { get; }

		ITrader DestinationTrader { get; }

		IReadOnlyCollection<ICommonGoodsItem> ExpectedGoodsItems { get; }
	}
}
