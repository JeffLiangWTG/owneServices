using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Types
{
	public struct ZAmount : IComparable, IEquatable<ZAmount>, IQuantity
	{
		public ZAmount(ZDecimal value, ZUnit unit, ZString source)
			: this(value, unit, Enumerable.Empty<ZAmount>(), Operator.None, source) { }

		ZAmount(ZDecimal value, CompoundUnit compoundUnit, IEnumerable<ZAmount> operands, Operator op, string source = "")
		{
			_source = source;
			_value = value;
			_isNotEmpty = true;
			_operands = new List<ZAmount>(operands.Where(x => !x.IsEmpty));
			_operator = op;
			_compoundUnit = compoundUnit;

			if (!_operands.Any() && string.IsNullOrWhiteSpace(source))
			{
				throw new ArgumentException("Source should not be empty");
			}
		}

#if DEBUG
		public ZAmount(ZDecimal value, string unit, ZString source)
			: this(value, unit, Enumerable.Empty<ZAmount>(), Operator.None, source) { }
#endif

		readonly ZDecimal _value;
		readonly CompoundUnit _compoundUnit;
		readonly string _source;
		readonly List<ZAmount> _operands;
		readonly Operator _operator;
		readonly bool _isNotEmpty;

		public List<ZAmount> Operands => _operands ?? new List<ZAmount>();

		public ZDecimal Value => _value;

		public string Unit => _compoundUnit.ToString();

		public bool IsAnyUnit => _compoundUnit.IsAny;

		public string Source => string.Join(System.Environment.NewLine, SourcesRecursive.Distinct().OrderBy(x => x));
		IEnumerable<string> SourcesRecursive => Operands.Any()
			? Operands.SelectMany(x => x.SourcesRecursive)
			: new[] { _source };

		public Operator Operator => _operator;

		public static ZAmount Empty => default(ZAmount);

		public bool IsEmpty => !_isNotEmpty;

		public static ZAmount CreateConversionFactor(ZAmount factorAndTargetUnit, ZAmount amountWithSourceUnit) => CreateConversionFactorCore(factorAndTargetUnit, amountWithSourceUnit._compoundUnit);
		public static ZAmount CreateConversionFactor(ZAmount factorAndTargetUnit, ZUnit sourceUnit) => CreateConversionFactorCore(factorAndTargetUnit, sourceUnit);
		static ZAmount CreateConversionFactorCore(ZAmount factorAndTargetUnit, CompoundUnit sourceUnit)
		{
			return new ZAmount(factorAndTargetUnit.Value, factorAndTargetUnit._compoundUnit / sourceUnit, Enumerable.Empty<ZAmount>(), Operator.None, factorAndTargetUnit.Source);
		}

		ZAmount Reduce() => _compoundUnit.GetDenominatorReduced(this);

		#region Equals

		public override bool Equals(object obj)
		{
			if (obj is ZAmount)
			{
				return Equals((ZAmount)obj);
			}

			if (obj is decimal || obj is ZDecimal)
			{
				return ((decimal)this).Equals(obj);
			}

			if (obj is string || obj is ZString)
			{
				return ((string)this).Equals(obj);
			}

			return false;
		}

		public bool Equals(ZAmount other)
		{
			return _value.Equals(other._value) && _compoundUnit.Equals(other._compoundUnit);
		}

		public static bool operator ==(ZAmount a1, ZAmount a2) => a1.Equals(a2);
		public static bool operator !=(ZAmount a1, ZAmount a2) => !a1.Equals(a2);

		#endregion

		#region Implicit

		public static implicit operator string(ZAmount value)
		{
			return value.ToString();
		}

		public static implicit operator ZString(ZAmount value)
		{
			return value.ToString();
		}

		public static implicit operator decimal(ZAmount value)
		{
			return value.Value;
		}

		public static implicit operator ZDecimal(ZAmount value)
		{
			return value.Value;
		}

		#endregion

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;

				hash = hash * 23 + _value.GetHashCode();
				hash = hash * 23 + _compoundUnit.GetHashCode();

				return hash;
			}
		}

		public override string ToString()
		{
			return ZString.Format("{0} {1}", Value.ToString("G" + GetDecimals(), CultureInfo.CurrentCulture), _compoundUnit).ToString();
		}

		static int GetDecimals()
		{
			return 6;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Math formula")]
		public string ToFormula()
		{
			if (Operands.Any())
			{
				switch (Operator)
				{
					case Operator.Plus:
						return ZString.Format("{0} + {1}", Operands[0].ToFormulaRecursive(), Operands[1].ToFormulaRecursive());

					case Operator.Minus:
						return ZString.Format("{0} - {1}", Operands[0].ToFormulaRecursive(), Operands[1].ToFormulaRecursive());

					case Operator.Multiply:
						return ZString.Format("{0} x {1}", Operands[0].ToFormulaRecursive(), Operands[1].ToFormulaRecursive());

					case Operator.Divide:
						return ZString.Format("{0} / {1}", Operands[0].ToFormulaRecursive(), Operands[1].ToFormulaRecursive());

					case Operator.Max:
						return ZString.Format("MAX({0})", new ZStringBuilder(Operands.Select(x => x.ToFormulaRecursive())).ToStringWithDelimiterBetweenAppends(", "));

					case Operator.Min:
						return ZString.Format("MIN({0})", new ZStringBuilder(Operands.Select(x => x.ToFormulaRecursive())).ToStringWithDelimiterBetweenAppends(", "));

					case Operator.Percentage:
						var percentage = Operands[1].Value;
						return Operands[0].ToFormulaRecursive(percentage + "% of({0})");

					case Operator.Round:
						return Operands[0].ToFormulaRecursive("Round({0}, " + Convert.ToInt32(Operands[1].Value) + ")");
				}
			}

			return ToString();
		}

		string ToFormulaRecursive(string wrap = "")
		{
			if (!string.IsNullOrWhiteSpace(wrap))
			{
				return ZString.Format(wrap, ToFormula());
			}

			return Operands.Any() && Operator != Operator.Multiply
				? ZString.Format("({0})", ToFormula()).ToString()
				: ToFormula();
		}

		#region Operators

		#region Comparison Operators

		public static bool operator >(ZAmount a1, ZAmount a2)
		{
			return Compare(a1, a2) == 1;
		}

		public static bool operator <(ZAmount a1, ZAmount a2)
		{
			return Compare(a1, a2) == -1;
		}

		public static bool operator >=(ZAmount a1, ZAmount a2)
		{
			return Compare(a1, a2) >= 0;
		}

		public static bool operator <=(ZAmount a1, ZAmount a2)
		{
			return Compare(a1, a2) <= 0;
		}

		public static int Compare(ZAmount a1, ZAmount a2)
		{
			a2 = a2.ConvertInto(a1._compoundUnit);
			return a1.Value.CompareTo(a2.Value);
		}

		public int CompareTo(object obj)
		{
			if (obj is ZAmount)
			{
				return Compare(this, (ZAmount)obj);
			}

			return 0;
		}

		#endregion

		public static ZAmount operator *(ZAmount a1, ZAmount a2)
		{
			const Operator operation = Operator.Multiply;
			var unit = a1._compoundUnit * a2._compoundUnit;
			var value = a1.Value * a2.Value;

			return new ZAmount(value, unit, new List<ZAmount> { a1, a2 }, operation).Reduce();
		}

		public ZAmount Multiply(ZAmount other) => this * other;

		public static ZAmount operator /(ZAmount a1, ZAmount a2)
		{
			const Operator operation = Operator.Divide;
			var unit = a1._compoundUnit / a2._compoundUnit;
			var value = a1.Value / a2.Value;

			return new ZAmount(value, unit, new List<ZAmount> { a1, a2 }, operation).Reduce();
		}

		public ZAmount Divide(ZAmount other) => this / other;

		public static ZAmount operator +(ZAmount a1, ZAmount a2)
		{
			if (a1.IsEmpty || a2.IsEmpty)
			{
				return a2.IsEmpty ? a1 : a2;
			}

			const Operator operation = Operator.Plus;

			a2 = a2.ConvertInto(a1._compoundUnit);

			var unit = a1._compoundUnit + a2._compoundUnit;
			var value = a1.Value + a2.Value;

			return new ZAmount(value, unit, new List<ZAmount> { a1, a2 }, operation);
		}

		public ZAmount Add(ZAmount other) => this + other;

		public static ZAmount operator -(ZAmount a1, ZAmount a2)
		{
			if (a2.IsEmpty)
			{
				return a1;
			}

			if (a1.IsEmpty)
			{
				throw new InvalidOperationException("Should not subtract from Empty amount");
			}

			const Operator operation = Operator.Minus;

			a2 = a2.ConvertInto(a1._compoundUnit);

			var unit = a1._compoundUnit - a2._compoundUnit;
			var value = a1.Value - a2.Value;

			return new ZAmount(value, unit, new List<ZAmount> { a1, a2 }, operation);
		}

		public ZAmount Subtract(ZAmount other) => this - other;

		public ZAmount Percentage(ZDecimal percentage, string percentageSource)
		{
			const Operator operation = Operator.Percentage;
			var percentageAmount = new ZAmount(percentage, ZUnit.Empty, percentageSource);

			return new ZAmount(percentage / 100 * Value, _compoundUnit, new List<ZAmount> { this, percentageAmount }, operation);
		}

		public ZAmount Ratio(ZDecimal ratio, string ratioSource) => Percentage(ratio * 100, ratioSource);

		static ZAmount Round(ZAmount amount, ZAmount decimals)
		{
			const Operator operation = Operator.Round;

			var newValue = Utilities.Round(amount.Value, decimals.Value.ToZInt());

			return new ZAmount(newValue, amount._compoundUnit, new List<ZAmount> { amount, decimals }, operation);
		}

		public ZAmount Round(int decimals, string source)
		{
			return Round(this, new ZAmount(decimals, ZUnit.Mathematics.DecimalPlace, source));
		}

		public static ZAmount Min(params ZAmount[] amounts)
		{
			return MinMax(Operator.Min, (x, y) => x < y, amounts);
		}

		public static ZAmount Max(params ZAmount[] amounts)
		{
			return MinMax(Operator.Max, (x, y) => x > y, amounts);
		}

		static ZAmount MinMax(Operator op, Func<ZAmount, ZAmount, bool> comprarisonFunc, params ZAmount[] amounts)
		{
			var nonEmpties = amounts.Where(x => !x.IsEmpty).ToArray();
			if (nonEmpties.Length == 1)
			{
				return nonEmpties[0];
			}

			var minOrMax = nonEmpties.FirstOrDefault();
			if (minOrMax.IsEmpty)
			{
				return Empty;
			}

			foreach (var amount in nonEmpties)
			{
				if (!amount.IsEmpty && comprarisonFunc(amount, minOrMax))
				{
					minOrMax = amount;
				}
			}

			return new ZAmount(minOrMax.Value, minOrMax._compoundUnit, nonEmpties, op);
		}

		ZAmount ConvertInto(CompoundUnit targetCompoundUnit)
		{
			var result = targetCompoundUnit.ConvertAmount(this);

			if (!targetCompoundUnit.Equals(result._compoundUnit))
			{
				throw new InvalidOperationException(ZString.Format("Cannot convert {0} to {1}", _compoundUnit, targetCompoundUnit));
			}

			return result;
		}

		#endregion

		#region IQuantity
		ZDecimal IQuantity.Amount => Value;

		ZString IQuantity.Unit => Unit;

		ZBool IQuantity.IsValid => true;

		#endregion

		struct CompoundUnit
		{
			CompoundUnit(IEnumerable<ZUnit> nominator, IEnumerable<ZUnit> denominator)
			{
				_isNotEmpty = true;
				_nominator = new List<ZUnit>(Argument.NotNull(nominator, "nominator"));
				_denominator = new List<ZUnit>(denominator ?? Enumerable.Empty<ZUnit>());

				if (_nominator.Any() && _denominator.Any())
				{
					for (var i = _nominator.Count - 1; i >= 0; i--)
					{
						foreach (var d in _denominator)
						{
							if (_nominator[i].Equals(d) || (d.IsAny && _nominator[i].Type == d.Type))
							{
								_nominator.RemoveAt(i);
								_denominator.Remove(d);
								break;
							}
						}
					}
				}
			}

			List<ZUnit> Nominator => _nominator ?? new List<ZUnit>();
			List<ZUnit> Denominator => _denominator ?? new List<ZUnit>();

			readonly List<ZUnit> _nominator;
			readonly List<ZUnit> _denominator;

			readonly bool _isNotEmpty;
			public bool IsEmpty => !_isNotEmpty;
			public static CompoundUnit Empty => default(CompoundUnit);

			public bool IsAny => !Denominator.Any() && Nominator.Count == 1 && Nominator[0].IsAny;

			public ZAmount ConvertAmount(ZAmount sourceAmount)
			{
				var nominatorFactors = GetConversionFactors(Nominator, sourceAmount._compoundUnit.Nominator);
				var denominatorFactors = GetConversionFactors(Denominator, sourceAmount._compoundUnit.Denominator);

				foreach (var nFactor in nominatorFactors)
				{
					sourceAmount *= nFactor;
				}

				foreach (var dFactor in denominatorFactors)
				{
					sourceAmount /= dFactor;
				}

				return sourceAmount;
			}

			public ZAmount GetDenominatorReduced(ZAmount sourceAmount)
			{
				var result = sourceAmount;
				var conversionFactors = GetConversionFactors(Nominator, Denominator);

				foreach (var factor in conversionFactors)
				{
					result /= factor;
				}

				return result;
			}

			List<ZAmount> GetConversionFactors(List<ZUnit> targetUnits, List<ZUnit> unitsToConvert)
			{
				var conversionFactors = new List<ZAmount>();
				var targetUnitsCopy = new List<ZUnit>(targetUnits);

				foreach (var unit in unitsToConvert)
				{
					foreach (var targetUnit in targetUnitsCopy)
					{
						if (targetUnit.Equals(unit))
						{
							targetUnitsCopy.Remove(targetUnit);
							break;
						}

						var factor = unit.FindFactorToConvertInto(targetUnit);
						if (!factor.IsEmpty)
						{
							conversionFactors.Add(factor);
							targetUnitsCopy.Remove(targetUnit);
							break;
						}
					}
				}

				return conversionFactors;
			}

			#region Implicit operators

			public static implicit operator CompoundUnit(ZUnit unit)
			{
				if (unit.IsEmpty)
				{
					return Empty;
				}

				return new CompoundUnit(new[] { unit }, Enumerable.Empty<ZUnit>());
			}

			#endregion

			#region Overrides

			public override bool Equals(object obj)
			{
				var u2 = (CompoundUnit)obj;
				return Nominator.EqualIgnoringOrder(u2.Nominator) && Denominator.EqualIgnoringOrder(u2.Denominator);
			}

			public static bool operator ==(CompoundUnit u1, CompoundUnit u2) => u1.Equals(u2);
			public static bool operator !=(CompoundUnit u1, CompoundUnit u2) => !u1.Equals(u2);

			public override int GetHashCode()
			{
				return Nominator.Count.GetHashCode() ^ Denominator.Count.GetHashCode();
			}

			public override string ToString()
			{
				var nominator = ToString(Nominator);
				var denominator = ToString(Denominator);

				return string.IsNullOrEmpty(denominator)
					? (ZString)nominator :
					ZString.Format("{0}/{1}", nominator, denominator);
			}

			#endregion

			#region Operators

			public static CompoundUnit operator *(CompoundUnit u1, CompoundUnit u2)
			{
				return new CompoundUnit(u1.Nominator.Concat(u2.Nominator), u1.Denominator.Concat(u2.Denominator));
			}

			public static CompoundUnit operator /(CompoundUnit u1, CompoundUnit u2)
			{
				return new CompoundUnit(u1.Nominator.Concat(u2.Denominator), u1.Denominator.Concat(u2.Nominator));
			}

			public static CompoundUnit operator +(CompoundUnit u1, CompoundUnit u2)
			{
				CheckEqual(u1, u2, Operator.Plus);
				return u1;
			}

			public static CompoundUnit operator -(CompoundUnit u1, CompoundUnit u2)
			{
				CheckEqual(u1, u2, Operator.Minus);
				return u1;
			}

			static void CheckEqual(CompoundUnit u1, CompoundUnit u2, Operator operation)
			{
				if (!u1.Equals(u2))
				{
					throw new InvalidOperationException(ZString.Format("Units are supposed to be equal to perform {0} operation, but were {1} and {2}", operation, u1, u2));
				}
			}

			#endregion

			string ToString(IEnumerable<ZUnit> units)
			{
				var sortedCodes = units.Select(x => x.Code).OrderBy(x => x).ToArray();
				var result = string.Join("*", sortedCodes);

				return sortedCodes.Length > 1
					? ZString.Format("({0})", result).ToString()
					: result;
			}

			#region String Parse For Testing
#if DEBUG
			public static implicit operator CompoundUnit(string unit) => Parse(unit);

			static CompoundUnit Parse(string unit)
			{
				//Todo - enhance to be able to parse any string. Not important at this stage because we don't really need to parse from string at all.
				var unbracked = Unbracket(unit);

				string[] divisionOperands;
				if (Split(unbracked, '/', out divisionOperands))
				{
					return Parse(divisionOperands[0]) / Parse(divisionOperands[1]);
				}

				string[] multiplicationOperands;
				if (Split(unbracked, '*', out multiplicationOperands))
				{
					return Parse(multiplicationOperands[0]) * Parse(multiplicationOperands[1]);
				}

				if (string.IsNullOrWhiteSpace(unbracked))
				{
					throw new ArgumentException("Empty unit");
				}

				return new CompoundUnit(new ZUnit[] { unbracked }, Enumerable.Empty<ZUnit>());
			}

			static bool Split(string unit, char separator, out string[] units)
			{
				if (unit.Length - unit.Replace("*", string.Empty).Replace("/", string.Empty).Length > 2)
				{
					throw new ArgumentException(ZString.Format("Not supported more than 2 division or multiplication: {0}", unit));
				}

				try
				{
					units = Unbracket(unit)
					.Split(separator)
					.Select(Unbracket)
					.ToArray();
				}
				catch (ArgumentException e)
				{
					throw new ArgumentException(ZString.Format("Bad unit {0}", unit), e);
				}

				if (units.Length < 1 || units.Length > 2)
				{
					throw new ArgumentException(ZString.Format("Bad unit {0}", unit));
				}

				return units.Length == 2;
			}

			static string Unbracket(string unit)
			{
				if (unit.Replace("(", string.Empty).Length - unit.Replace(")", string.Empty).Length != 0)
				{
					throw new ArgumentException(ZString.Format("Bad unit {0}", unit));
				}

				var result = unit.Replace(" ", string.Empty);

				if (string.IsNullOrEmpty(result))
				{
					throw new ArgumentException("Empty unit");
				}

				if (result[0] == '(' && result[result.Length - 1] == ')')
				{
					return Unbracket(result.Substring(1, result.Length - 2));
				}

				return result;
			}

#endif
			#endregion
		}
	}

	public enum Operator
	{
		Multiply,
		Plus,
		Minus,
		Min,
		Max,
		None,
		Divide,
		Round,
		Percentage
	}
}
