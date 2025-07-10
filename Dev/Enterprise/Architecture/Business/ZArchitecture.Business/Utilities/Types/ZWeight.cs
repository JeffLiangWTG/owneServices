using System;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.ZArchitecture
{
	public struct ZWeight : IQuantity
	{
		#region Constructor

		public ZWeight(ZDecimal amount, ZString unit)
		{
			this.amount = amount;
			this.unit = unit;
			this.isValid = Constants.Weight.ContainsCode(unit);
		}

		#endregion

		/// <summary>
		/// Default empty weight - 0 Kilograms
		/// </summary>
		public static readonly ZWeight Empty = new ZWeight(0, "KG");

		/// <summary>
		/// Invalid Weight - 0 empty units
		/// </summary>
		public static readonly ZWeight Invalid = new ZWeight(0, "");

		#region Operator Overloads

		/// <summary>
		/// Operator overload - will return ZWeight.Invalid if either weight is invalid
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="rHS"></param>
		/// <returns></returns>
		public static ZWeight operator +(ZWeight lHS, ZWeight rHS)
		{
			if (rHS.IsValid && lHS.IsValid)
			{
				return new ZWeight(lHS.Amount + Constants.Weight.Convert(rHS.Amount, rHS.Unit, lHS.Unit), lHS.Unit);
			}
			else
			{
				return ZWeight.Invalid;
			}
		}

		/// <summary>
		/// Operator overload - will return ZWeight.Invalid if either weight is invalid
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="rHS"></param>
		/// <returns></returns>
		public static ZWeight operator -(ZWeight lHS, ZWeight rHS)
		{
			if (rHS.IsValid && lHS.IsValid)
			{
				return new ZWeight(lHS.Amount - Constants.Weight.Convert(rHS.Amount, rHS.Unit, lHS.Unit), lHS.Unit);
			}
			else
			{
				return ZWeight.Invalid;
			}
		}

		public static ZWeight operator *(ZWeight lHS, decimal multiplier)
		{
			return new ZWeight(lHS.Amount * multiplier, lHS.Unit);
		}

		/// <summary>
		/// Operator overload - will throw exception if Divisor is zero
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="divisor"></param>
		/// <returns></returns>
		public static ZWeight operator /(ZWeight lHS, decimal divisor)
		{
			return new ZWeight(lHS.Amount / divisor, lHS.Unit);
		}

		/// <summary>
		/// Operator overload - throws ArgumentException if either weight is invalid
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="rHS"></param>
		/// <returns></returns>
		public static bool operator >(ZWeight lHS, ZWeight rHS)
		{
			CheckBothValid(lHS, rHS);
			return lHS.InKilograms > rHS.InKilograms;
		}

		/// <summary>
		/// Operator overload - throws ArgumentException if either weight is invalid
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="rHS"></param>
		/// <returns></returns>
		public static bool operator >=(ZWeight lHS, ZWeight rHS)
		{
			CheckBothValid(lHS, rHS);
			return lHS.InKilograms >= rHS.InKilograms;
		}

		/// <summary>
		/// Operator overload - throws ArgumentException if either weight is invalid
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="rHS"></param>
		/// <returns></returns>
		public static bool operator <(ZWeight lHS, ZWeight rHS)
		{
			CheckBothValid(lHS, rHS);
			return lHS.InKilograms < rHS.InKilograms;
		}

		/// <summary>
		/// Operator overload - throws ArgumentException if either weight is invalid
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="rHS"></param>
		/// <returns></returns>
		public static bool operator <=(ZWeight lHS, ZWeight rHS)
		{
			CheckBothValid(lHS, rHS);
			return lHS.InKilograms <= rHS.InKilograms;
		}

		/// <summary>
		/// Operator overload - throws ArgumentException if either weight is invalid
		/// </summary>
		/// <param name="lHS"></param>
		/// <param name="rHS"></param>
		/// <returns></returns>
		public static bool operator ==(ZWeight lHS, ZWeight rHS)
		{
			return (lHS.IsValid && rHS.IsValid && lHS.InKilograms == rHS.InKilograms) || (!lHS.IsValid && !rHS.IsValid);
		}

		public static bool operator !=(ZWeight lHS, ZWeight rHS)
		{
			return !(lHS == rHS);
		}

		static void CheckBothValid(ZWeight lHS, ZWeight rHS)
		{
			CheckValid(lHS, "LHS");
			CheckValid(rHS, "RHS");
		}

		static void CheckValid(ZWeight weight, string handside)
		{
			if (!weight.IsValid)
			{
				throw new ArgumentException(string.Format("The value passed was an invalid Weight. Protect calls to this method by checking if the object is valid prior to using this routine. Weight Amount: [{0}], Weight Unit: [{1}], Left or Right-Hand Side: [{2}]", weight.Amount, weight.Unit, handside));
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Converts the current object to the target unit supplied.
		/// This will throw an ArgumentException if the weight is invalid, or the target unit is unknown.
		/// </summary>
		/// <param name="targetUnit"></param>
		/// <returns></returns>
		public ZDecimal ConvertTo(string targetUnit)
		{
			if (IsValid)
			{
				return Constants.Weight.Convert(Amount, Unit, targetUnit);
			}
			else
			{
				throw new ArgumentException("The current ZWeight object is invalid, therefor ConvertTo may not be called.");
			}
		}

		/// <summary>
		/// Converts the current object to the target unit supplied without rounding.
		/// This will throw an ArgumentException if the weight is invalid, or the target unit is unknown.
		/// </summary>
		/// <param name="targetUnit"></param>
		/// <returns></returns>
		public ZDecimal ConvertToUnrounded(string targetUnit)
		{
			if (IsValid)
			{
				return Constants.Weight.Convert(Amount, Unit, targetUnit, false);
			}
			else
			{
				throw new ArgumentException("The current ZWeight object is invalid, therefor ConvertToUnrounded may not be called.");
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns the weight converted to kilograms.
		/// If the weight unit is unknown or empty, it will return 0m.
		/// </summary>
		public ZDecimal InKilogramsSafe
		{
			get { return IsValid ? ConvertTo(Constants.Weight.Kilograms) : ZDecimal.Zero; }
		}

		/// <summary>
		/// Returns the weight converted to kilograms.
		/// This will throw an exception if .IsValid is false
		/// </summary>
		public ZDecimal InKilograms
		{
			get { return ConvertTo(Constants.Weight.Kilograms); }
		}

		/// <summary>
		/// Returns the weight converted to kilograms without rounding.
		/// If the weight unit is unknown or empty, it will return 0m.
		/// </summary>
		public ZDecimal InUnroundedKilogramsSafe
		{
			get { return IsValid ? ConvertToUnrounded(Constants.Weight.Kilograms) : ZDecimal.Zero; }
		}

		/// <summary>
		/// Returns the weight converted to kilograms without rounding.
		/// This will throw an exception if .IsValid is false
		/// </summary>
		public ZDecimal InUnroundedKilograms
		{
			get { return ConvertToUnrounded(Constants.Weight.Kilograms); }
		}

		/// <summary>
		/// Returns the weight converted to pounds.
		/// If the weight unit is unknown or empty, it will return 0m.
		/// </summary>
		public ZDecimal InPoundsSafe
		{
			get { return IsValid ? ConvertTo(Constants.Weight.Pounds) : ZDecimal.Zero; }
		}

		/// <summary>
		/// Returns the weight converted to pounds.
		/// This will throw an exception if .IsValid is false
		/// </summary>
		public ZDecimal InPounds
		{
			get { return ConvertTo(Constants.Weight.Pounds); }
		}

		/// <summary>
		/// Returns whether the weight amount is zero
		/// </summary>
		public bool IsEmpty
		{
			get { return Amount == 0; }
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// The amount the weight object represents, expressed in Units
		/// </summary>
		public ZDecimal Amount { get { return amount; } }
		readonly ZDecimal amount;

		/// <summary>
		/// The unit the weight object represents.
		/// </summary>
		public ZString Unit { get { return unit; } }
		readonly ZString unit;

		/// <summary>
		/// Indicates whether the unit passed to the weight constructor is valid
		/// </summary>
		public ZBool IsValid { get { return isValid; } }
		readonly ZBool isValid;

		#endregion

		#region Overridden methods

		public override string ToString()
		{
			return Amount.ToString() + " " + Unit;
		}

		public override int GetHashCode()
		{
			return InKilograms.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return (obj is ZWeight) && (InKilograms == ((ZWeight)obj).InKilograms);
		}

		#endregion
	}
}
