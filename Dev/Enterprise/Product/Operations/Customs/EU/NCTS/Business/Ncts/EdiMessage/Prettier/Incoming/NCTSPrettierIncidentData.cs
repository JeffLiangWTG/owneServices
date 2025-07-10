using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierIncidentData : INCTSIncidentData, IEquatable<NCTSPrettierIncidentData>
	{
		public NCTSPrettierIncidentData(ZString sequenceNumber, ZString code, ZString text, INCTSTranshipmentData transhipment, INCTSAddressData locationAddress, IReadOnlyCollection<INCTSTransportEquipment> transportEquipments)
		{
			SequenceNumber = int.TryParse(sequenceNumber, out var sequenceNo) ? sequenceNo : SequenceNumber;
			Code = int.TryParse(code, out var codeResult) ? codeResult : Code;
			Text = text;
			TransportEquipments = transportEquipments;
			Transhipment = transhipment;
			LocationAddress = locationAddress;
		}

		public override bool Equals(object obj) => obj is NCTSPrettierIncidentData other && Equals(other);

		public bool Equals(NCTSPrettierIncidentData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return SequenceNumber.Equals(other.SequenceNumber)
				&& Code.Equals(other.Code)
				&& Text.Equals(other.Text)
				&& EndorsementDate.Equals(other.EndorsementDate)
				&& EndorsementAuthority.Equals(other.EndorsementAuthority)
				&& EndorsementPlace.Equals(other.EndorsementPlace)
				&& EndorsementCountry.Equals(other.EndorsementCountry)
				&& LocationQualifierOfIdentification.Equals(other.LocationQualifierOfIdentification)
				&& LocationUNLocode.Equals(other.LocationUNLocode)
				&& LocationCountry.Equals(other.LocationCountry)
				&& LocationLatitude.Equals(other.LocationLatitude)
				&& (LocationAddress != null && LocationAddress.Equals(other.LocationAddress))
				&& TransportEquipments.SequenceEqual(other.TransportEquipments)
				&& Transhipment.Equals(other.Transhipment);
		}

		public override int GetHashCode() => (SequenceNumber, Code, Text, EndorsementDate, EndorsementAuthority, EndorsementPlace, EndorsementCountry, LocationQualifierOfIdentification,
			LocationUNLocode, LocationCountry, LocationLatitude, LocationAddress, TransportEquipments, Transhipment).GetHashCode();

		public int SequenceNumber { get; }

		public int Code { get; }

		public ZString Text { get; }

		public DateTime? EndorsementDate { get; set; }

		public ZString EndorsementAuthority { get; set; }

		public ZString EndorsementPlace { get; set; }

		public ZString EndorsementCountry { get; set; }

		public ZString LocationQualifierOfIdentification { get; set; }

		public ZString LocationUNLocode { get; set; }

		public ZString LocationCountry { get; set; }

		public ZString LocationLatitude { get; set; }

		public ZString LocationLongitude { get; set; }

		public INCTSAddressData LocationAddress { get; }

		public IReadOnlyCollection<INCTSTransportEquipment> TransportEquipments { get; }

		public INCTSTranshipmentData Transhipment { get; }
	}
}
