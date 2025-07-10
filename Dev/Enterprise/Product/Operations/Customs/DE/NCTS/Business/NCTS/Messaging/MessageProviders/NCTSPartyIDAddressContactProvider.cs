using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NCTSPartyIDAddressContactProvider : NCTSPartyIDContactProvider, INCTSPartyIDAddressContact, IEquatable<NCTSPartyIDAddressContactProvider>
	{
		public new static NCTSPartyIDAddressContactProvider NewOrNull(JobDocAddress address, GlbStaff staff = null, bool provideContact = true) =>
			address != null ? new NCTSPartyIDAddressContactProvider(address, staff, provideContact) : null;

		public new static NCTSPartyIDAddressContactProvider NewOrNull(JobDocAddress address, bool fallback) =>
			address != null ? new NCTSPartyIDAddressContactProvider(address, fallback) : null;

		NCTSPartyIDAddressContactProvider(JobDocAddress docAddress, GlbStaff staff, bool provideContact) : base(docAddress, staff, provideContact)
		{
		}

		NCTSPartyIDAddressContactProvider(JobDocAddress address, bool fallback) : base(address, fallback)
		{
		}

		public string PartyName => EoriNumber.IsEmpty() ? DocAddress.CompanyName.ValueOrNullIfEmpty() : null;

		public string Address => EoriNumber.IsEmpty() ? (DocAddress.E2_Address1AndE2_Address2).Trim().ValueOrNullIfEmpty() : null;

		public string City => EoriNumber.IsEmpty() ? DocAddress.City.ValueOrNullIfEmpty() : null;

		public string Postcode => EoriNumber.IsEmpty() ? DocAddress.Postcode.ValueOrNullIfEmpty() : null;

		public string Country => EoriNumber.IsEmpty() ? DocAddress.Country?.Code.ValueOrNullIfEmpty() : null;

		#region IEquatable implementation
		public override bool Equals(object obj)
		{
			if (obj is NCTSPartyIDAddressContactProvider nCTSPartyIDAddressContactProvider)
			{
				return Equals(nCTSPartyIDAddressContactProvider);
			}

			return base.Equals(obj);
		}

		public static bool operator ==(NCTSPartyIDAddressContactProvider first, NCTSPartyIDAddressContactProvider second)
		{
			if ((object)first == null)
			{
				return (object)second == null;
			}

			return first.Equals(second);
		}

		public static bool operator !=(NCTSPartyIDAddressContactProvider first, NCTSPartyIDAddressContactProvider second)
		{
			return !(first == second);
		}

		public bool Equals(NCTSPartyIDAddressContactProvider other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && Equals(PartyName, other.PartyName) && Equals(Address, other.Address) && Equals(City, other.City) && Equals(Postcode, other.Postcode) && Equals(Country, other.Country);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = base.GetHashCode();
				if (PartyName != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(PartyName);
				}

				if (Address != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(Address);
				}

				if (City != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(City);
				}

				if (Postcode != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(Postcode);
				}

				if (Country != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(Country);
				}

				return hashCode;
			}
		}

		#endregion	}
	}
}
