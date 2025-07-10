using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalReferencesGroup
	{
		#region Fields For RFF

		ZString TransitNumber { get; }
		ZString PreviousSummaryDeclarationNumber { get; }
		IReadOnlyCollection<IArrivalRouteEvent> RouteEvents { get; }

		#endregion
	}
}
