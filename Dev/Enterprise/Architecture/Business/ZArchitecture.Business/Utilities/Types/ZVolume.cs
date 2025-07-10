using System;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.ZArchitecture
{
	public struct ZVolume : IQuantity
	{
		#region Constructor

		public ZVolume(ZDecimal amount, ZString unit)
		{
			this.amount = amount;
			this.unit = unit;
			this.isValid = Constants.Volume.ContainsCode(unit);
		}

		#endregion

		public static readonly ZVolume Empty = new ZVolume(0, "M3");
		public static readonly ZVolume Invalid = new ZVolume(0, "");

		#region Operator Overloads

		public static ZVolume operator +(ZVolume lHS, ZVolume rHS)
		{
			if (lHS.Unit == rHS.Unit)
			{
				return new ZVolume(lHS.Amount + rHS.Amount, lHS.Unit);
			}
			else
			{
				return new ZVolume(lHS.Amount + Constants.Volume.Convert(rHS.Amount, rHS.Unit, lHS.Unit), lHS.Unit);
			}
		}

		public static ZVolume operator -(ZVolume lHS, ZVolume rHS)
		{
			if (lHS.Unit == rHS.Unit)
			{
				return new ZVolume(lHS.Amount - rHS.Amount, lHS.Unit);
			}
			else
			{
				return new ZVolume(lHS.Amount - Constants.Volume.Convert(rHS.Amount, rHS.Unit, lHS.Unit), lHS.Unit);
			}
		}

		public static ZVolume operator *(ZVolume lHS, decimal multiplier)
		{
			return new ZVolume(lHS.Amount * multiplier, lHS.Unit);
		}

		public static ZVolume operator /(ZVolume lHS, decimal divisor)
		{
			return new ZVolume(lHS.Amount / divisor, lHS.Unit);
		}

		public static bool operator >(ZVolume lHS, ZVolume rHS)
		{
			return lHS.InCubicMetres > rHS.InCubicMetres;
		}

		public static bool operator >=(ZVolume lHS, ZVolume rHS)
		{
			return lHS.InCubicMetres >= rHS.InCubicMetres;
		}

		public static bool operator <(ZVolume lHS, ZVolume rHS)
		{
			return lHS.InCubicMetres < rHS.InCubicMetres;
		}

		public static bool operator <=(ZVolume lHS, ZVolume rHS)
		{
			return lHS.InCubicMetres <= rHS.InCubicMetres;
		}

		public static bool operator ==(ZVolume lHS, ZVolume rHS)
		{
			return lHS.InCubicMetres == rHS.InCubicMetres;
		}

		public static bool operator !=(ZVolume lHS, ZVolume rHS)
		{
			return lHS.InCubicMetres != rHS.InCubicMetres;
		}

		#endregion

		#region Overridden Methods

		public override bool Equals(object obj)
		{
			return (obj is ZVolume) && (InCubicMetres == ((ZVolume)obj).InCubicMetres);
		}

		public override int GetHashCode()
		{
			return InCubicMetres.GetHashCode();
		}

		public override string ToString()
		{
			return Amount.ToString() + " " + Unit;
		}

		#endregion

		#region Public Methods

		public ZDecimal ConvertTo(string targetUnit)
		{
			ZDecimal result = 0m;
			try
			{
				result = Enterprise.Core.Constants.Volume.Convert(Amount, Unit, targetUnit);
			}
			catch (ArgumentException)
			{
			}
			return result;
		}

		#endregion

		#region Public Properties

		public ZDecimal InCubicMetres
		{
			get { return ConvertTo("M3"); }
		}

		public bool IsEmpty
		{
			get { return Amount == 0; }
		}

		public ZBool IsValid { get { return isValid; } }
		readonly ZBool isValid;

		#endregion

		#region Public Fields

		public ZDecimal Amount { get { return amount; } }
		readonly ZDecimal amount;

		public ZString Unit { get { return unit; } }
		readonly ZString unit;

		#endregion
	}
}
