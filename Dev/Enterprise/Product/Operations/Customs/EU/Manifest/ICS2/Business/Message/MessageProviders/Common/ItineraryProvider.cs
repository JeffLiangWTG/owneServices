using System;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ItineraryProvider : IItinerary
	{
		public ItineraryProvider(RouteEntry route)
		{
			this.route = Argument.NotNull(route, nameof(route));
		}

		readonly RouteEntry route;

		public sbyte Order => Convert.ToSByte(route.CY_Order);

		public string Country => route.CY_Data;
	}
}
