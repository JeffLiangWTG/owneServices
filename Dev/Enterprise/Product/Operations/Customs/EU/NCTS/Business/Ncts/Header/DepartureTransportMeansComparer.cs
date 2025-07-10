using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DepartureTransportMeansComparer : IEqualityComparer<IDepartureTransportMeansProvider>
	{
		public bool Equals(IDepartureTransportMeansProvider x, IDepartureTransportMeansProvider y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x is null || y is null)
			{
				return false;
			}

			if (!stringChecks.All(f => string.Equals(f(x), f(y))))
			{
				return false;
			}

			if (x.AdditionalWagons is null && y.AdditionalWagons is null)
			{
				return true;
			}

			if (x.AdditionalWagons is null || y.AdditionalWagons is null || x.AdditionalWagons.Count != y.AdditionalWagons.Count)
			{
				return false;
			}

			for (int i = 0; i < x.AdditionalWagons.Count; i++)
			{
				var xWagon = x.AdditionalWagons[i];
				var yWagon = y.AdditionalWagons[i];

				if (!string.Equals(xWagon.WagonNumber, yWagon.WagonNumber) ||
					!string.Equals(xWagon.WagonNationality, yWagon.WagonNationality))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(IDepartureTransportMeansProvider obj)
		{
			if (obj is null)
			{
				return 0;
			}

			unchecked
			{
				var hash = 17;
				hash = hash * 23 + obj.InlandTransportModeAtDeparture.GetHashCode();
				hash = hash * 23 + obj.VesselNameAtDeparture.GetHashCode();
				hash = hash * 23 + obj.VesselCountryAtDeparture.GetHashCode();
				hash = hash * 23 + obj.TransportTypeAtDeparture.GetHashCode();
				hash = hash * 23 + obj.TransportAtDeparture.GetHashCode();
				hash = hash * 23 + obj.TransportCountryAtDeparture.GetHashCode();
				hash = hash * 23 + obj.Trailer1IDAtDeparture.GetHashCode();
				hash = hash * 23 + obj.Trailer1NationalityAtDeparture.GetHashCode();
				hash = hash * 23 + obj.Trailer2IDAtDeparture.GetHashCode();
				hash = hash * 23 + obj.Trailer2NationalityAtDeparture.GetHashCode();
				hash = hash * 23 + obj.AircraftIDAtDeparture.GetHashCode();
				hash = hash * 23 + obj.IsInPhase5TransitionPeriod.GetHashCode();
				hash = hash * 23 + (obj.AdditionalWagons?.Count ?? 0);

				return hash;
			}
		}

		readonly List<Func<IDepartureTransportMeansProvider, string>> stringChecks =
			new()
			{
				p => p.AircraftIDAtDeparture,
				p => p.InlandTransportModeAtDeparture,
				p => p.Trailer1IDAtDeparture,
				p => p.Trailer1NationalityAtDeparture,
				p => p.Trailer2IDAtDeparture,
				p => p.Trailer2NationalityAtDeparture,
				p => p.TransportAtDeparture,
				p => p.TransportCountryAtDeparture,
				p => p.TransportTypeAtDeparture,
				p => p.VesselCountryAtDeparture,
				p => p.VesselNameAtDeparture
			};
	}
}
