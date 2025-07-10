using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierSealData : INCTSSeal, IEquatable<NCTSPrettierSealData>
	{
		public NCTSPrettierSealData(ZString sequenceNumber, ZString identifier)
		{
			SequenceNumber = int.TryParse(sequenceNumber, out var sequenceNo) ? sequenceNo : SequenceNumber;
			Identifier = identifier;
		}

		public override bool Equals(object obj)
			=> obj is NCTSPrettierSealData other && Equals(other);

		public bool Equals(NCTSPrettierSealData other)
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
				&& Identifier.Equals(other.Identifier);
		}

		public override int GetHashCode() => (SequenceNumber, Identifier).GetHashCode();

		public int SequenceNumber { get; }

		public string Identifier { get; }
	}
}
