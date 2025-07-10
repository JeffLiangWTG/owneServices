using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	sealed partial class CompactTableStrategy
	{
		sealed class ColumnKey : IEquatable<ColumnKey>
		{
			public static ColumnKey New(RateLine rateLine)
			{
				var chargeCode = rateLine.ChargeCode;

				if (chargeCode != null)
				{
					return new ColumnKey(chargeCode, rateLine.TL_RX_NKCurrency, rateLine.TL_WeightVolumeMultiple, rateLine.TL_WeightVolume);
				}
				else
				{
					return null;
				}
			}

			ColumnKey(AccChargeCode charge, ZString currency, ZDecimal multiple, ZString unit)
			{
				this.Charge = charge;
				this.Currency = currency;
				this.Multiple = multiple == 0 ? 1 : multiple;
				this.Unit = unit;
			}

			AccChargeCode Charge { get; }
			ZDecimal Multiple { get; }
			ZString Unit { get; }
			public ZString Currency { get; }

			public bool Equals(ColumnKey other)
				=> other != null
					&& other.Charge == Charge
					&& other.Currency == Currency
					&& other.Multiple == Multiple
					&& other.Unit == Unit;

			public override bool Equals(object obj) => Equals(obj as ColumnKey);

			public override int GetHashCode()
			{
				unchecked
				{
					uint tmp = (uint)Charge.GetHashCode();
					tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)Currency.GetHashCode();
					tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)Multiple.GetHashCode();
					tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)Unit.GetHashCode();
					return (int)tmp;
				}
			}

			#region SuppressResourceStringsCheckRegion

			public override string ToString()
			{
				if (Unit.IsEmpty)
				{
					return string.Format("{0}\n({1})", Charge.AC_Code, Currency);
				}
				else if (Multiple == 1)
				{
					return string.Format("{0}\n({1}/{2})", Charge.AC_Code, Currency, Unit);
				}
				else
				{
					return string.Format("{0}\n({1}/{2:0}{3})", Charge.AC_Code, Currency, Multiple, Unit);
				}
			}

			#endregion
		}
	}
}
