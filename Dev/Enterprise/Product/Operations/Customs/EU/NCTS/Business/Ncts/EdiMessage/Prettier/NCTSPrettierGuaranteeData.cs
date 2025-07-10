using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierGuaranteeData : INCTSPrettierGuaranteeData, IEquatable<NCTSPrettierGuaranteeData>
	{
		public NCTSPrettierGuaranteeData(ZString type, ZString grn, ZString otherNumber, ZString amount, ZString currency)
		{
			Type = type;
			GRN = grn;
			OtherNumber = otherNumber;
			Amount = amount;
			Currency = currency;
		}

		public override bool Equals(object obj) => obj is NCTSPrettierGuaranteeData other && Equals(other);

		public bool Equals(NCTSPrettierGuaranteeData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Type.Equals(other.Type)
				&& GRN.Equals(other.GRN)
				&& OtherNumber.Equals(other.OtherNumber)
				&& Amount.Equals(other.Amount)
				&& Currency.Equals(other.Currency);
		}

		public override int GetHashCode() => (Type, GRN, OtherNumber, Amount, Currency).GetHashCode();

		public ZString Type { get; }
		public ZString GRN { get; }
		public ZString OtherNumber { get; }
		public ZString Amount { get; }
		public ZString Currency { get; }
	}
}
