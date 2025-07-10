using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierIncidentLocationAddressData : INCTSAddressData, IEquatable<NCTSPrettierIncidentLocationAddressData>
	{
		public NCTSPrettierIncidentLocationAddressData(ZString country, ZString postCode, ZString streetAndNumber)
		{
			Country = country;
			Postcode = postCode;
			StreetAndNumber = streetAndNumber;
		}

		public override bool Equals(object obj)
			=> obj is NCTSPrettierIncidentLocationAddressData other && Equals(other);

		public bool Equals(NCTSPrettierIncidentLocationAddressData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return CareOf.Equals(other.CareOf)
				&& City.Equals(other.City)
				&& Country.Equals(other.Country)
				&& Postcode.Equals(other.Postcode)
				&& StreetAndNumber.Equals(other.StreetAndNumber);
		}

		public override int GetHashCode() => (CareOf, City, Country, Postcode, StreetAndNumber).GetHashCode();

		public ZString CareOf { get; }

		public ZString City { get; }

		public ZString Country { get; }

		public ZString Postcode { get; }

		public ZString StreetAndNumber { get; }
	}
}
