using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace CargoWise.Types
{
	public struct ZUnit
	{
		readonly string code;
		readonly ZUnitType type;
		readonly bool _isNotEmpty;
		ZUnit(string code, ZUnitType type)
		{
			this.code = code;
			this.type = type;
			_isNotEmpty = true;
		}

		public ZUnit(IZUnit iUnit) : this(iUnit.Code, iUnit.Type) { }

		public bool IsAny => !IsEmpty && string.IsNullOrEmpty(code);

		public bool IsEmpty => !_isNotEmpty;
		public static ZUnit Empty => default(ZUnit);

		public static class Time
		{
			public static ZUnit Hour => new ZUnit(Constants.Time.Hours, ZUnitType.Time);
		}

		public static class Weight
		{
			public static ZUnit KG => new ZUnit(Constants.Weight.Kilograms, ZUnitType.Weight);
			public static ZUnit T => new ZUnit(Constants.Weight.Tonnes, ZUnitType.Weight);
			public static ZUnit LB => new ZUnit(Constants.Weight.Pounds, ZUnitType.Weight);
		}

		public static class Volume
		{
			public static ZUnit L => new ZUnit(Constants.Volume.Litre, ZUnitType.Volume);
			public static ZUnit M3 => new ZUnit(Constants.Volume.CubicMetres, ZUnitType.Volume);
			public static ZUnit CF => new ZUnit(Constants.Volume.CubicFeet, ZUnitType.Volume);
		}

		public static class Length
		{
			public static ZUnit KM => new ZUnit(Constants.Length.Kilometres, ZUnitType.Length);
		}

		public static class Mathematics
		{
			public static ZUnit DecimalPlace => new ZUnit(Constants.Mathematics.Decimals, ZUnitType.Mathematics);
		}

		public static class RefContainer
		{
			public static ZUnit Any => new ZUnit(string.Empty, ZUnitType.RefContainer);
		}

		public ZString Code
		{
			get { return string.IsNullOrEmpty(code) ? GetAnyUnitForUnitType(ZUnitType.RefContainer) : code; }
		}

		string GetAnyUnitForUnitType(ZUnitType unitType)
		{
			switch (unitType)
			{
				case ZUnitType.RefContainer:
					return Constants.BusinessQuantityUnit.Container;
				default:
					throw new InvalidOperationException(ZString.Format("No 'Any' code registered for {0} UnitType", unitType));
			}
		}

		public ZUnitType Type
		{
			get { return type; }
		}

		public override bool Equals(object obj)
		{
			if (obj is ZUnit)
			{
				var other = (ZUnit)obj;
				return other.code == code && other.Type == Type;
			}

			return false;
		}

		public static bool operator ==(ZUnit u1, ZUnit u2) => u1.Equals(u2);
		public static bool operator !=(ZUnit u1, ZUnit u2) => !u1.Equals(u2);

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;

				hash = hash * 23 + Code.GetHashCode();
				hash = hash * 23 + Type.GetHashCode();

				return hash;
			}
		}

		public override string ToString()
		{
			return Code;
		}

		public ZAmount FindFactorToConvertInto(ZUnit targetUnit)
		{
			if (Type != targetUnit.Type)
			{
				return ZAmount.Empty;
			}

			var source = Res.GetString("6DD94745-866E-4577-8BFC-4721EB0546FC", "{0} to {1} conversion factor", Code, targetUnit.Code);

			if (Equals(targetUnit))
			{
				return ZAmount.Empty;
			}

			decimal factor;

			switch (targetUnit.Type)
			{
				case ZUnitType.Weight:
					factor = Constants.Weight.Convert(1, Code, targetUnit.Code, false);
					break;

				case ZUnitType.Volume:
					factor = Constants.Volume.Convert(1, Code, targetUnit.Code, false);
					break;

				case ZUnitType.Length:
					factor = Constants.Length.Convert(1, Code, targetUnit.Code);
					break;

				default:
					return ZAmount.Empty;
			}

			return ZAmount.CreateConversionFactor(new ZAmount(factor, targetUnit, source), this);
		}

		#region Implicit

#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public static implicit operator ZUnit(string unit)
		{
			switch (unit)
			{
				case Constants.Weight.Kilograms:
					return Weight.KG;

				case Constants.Weight.Tonnes:
					return Weight.T;

				case Constants.Weight.Pounds:
					return Weight.LB;

				case Constants.Volume.Litre:
					return Volume.L;

				case Constants.Volume.CubicMetres:
					return Volume.M3;

				case Constants.Volume.CubicFeet:
					return Volume.CF;

				case Constants.Length.Kilometres:
					return Length.KM;

				case Constants.Time.Hours:
					return Time.Hour;

				case Constants.Mathematics.Decimals:
					return Mathematics.DecimalPlace;
			}

			if (Constants.CurrencyCodes.All.Contains(unit))
			{
				return new ZUnit(unit, ZUnitType.Currency);
			}

			throw new ArgumentException(ZString.Format("Bad unit {0}", unit));
		}

#endif

		#endregion
	}

	public interface IZUnit
	{
		ZString Code { get; }
		ZUnitType Type { get; }
	}

	public enum ZUnitType       //Todo: move to Core.Constants
	{
		Weight,
		Volume,
		Currency,
		Time,
		Length,
		Mathematics,
		RefPackType,
		RefContainer
	}
}
