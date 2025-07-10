using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Business
{
	[System.Diagnostics.DebuggerDisplay("{Factor} {NumeratorUnit}/{DenominatorUnit}")]
	public struct ConversionFactor
	{
		public static class Standard
		{
			public static class Metric
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Air = new ConversionFactor(6000, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms)
				{
					Description = ResString.GetMultilingualString("7eb78966-5753-11e5-b96c-902b34dc814a", "Air")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Sea = new ConversionFactor(1000, Constants.Weight.Kilograms, Constants.Volume.CubicMetres)
				{
					Description = ResString.GetMultilingualString("32a5e838-d2d7-45f0-8fbe-bfda0d33dae0", "Sea")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Rail = new ConversionFactor(1000, Constants.Weight.Kilograms, Constants.Volume.CubicMetres)
				{
					Description = ResString.GetMultilingualString("321dd800-8b50-11e6-868e-fcaa14295823", "Rail")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Road = new ConversionFactor(333, Constants.Weight.Kilograms, Constants.Volume.CubicMetres)
				{
					Description = ResString.GetMultilingualString("d93cde49-b9fe-4e1d-b226-74eba8db1d83", "Trucking (Europe)")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor RoadAUUS = new ConversionFactor(250, Constants.Weight.Kilograms, Constants.Volume.CubicMetres)
				{
					Description = ResString.GetMultilingualString("9720127b-9eff-418d-9767-a8b5ac5eaa2b", "Trucking (AUS, USA)")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor LoadingMeters = new ConversionFactor(1750, Constants.Weight.Kilograms, Constants.LoadingLength.LoadingMeters)
				{
					Description = ResString.GetMultilingualString("3bb6f573-852a-445a-babc-e1d614359b15", "Loading Meters (Default)")
				};
			}

			public static class Imperial
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Air = new ConversionFactor(166, Constants.Volume.CubicInches, Constants.Weight.Pounds)
				{
					Description = ResString.GetMultilingualString("7eb78966-5753-11e5-b96c-902b34dc814a", "Air")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Sea = new ConversionFactor(100, Constants.Weight.Pounds, Constants.Volume.CubicFeet)
				{
					Description = ResString.GetMultilingualString("32a5e838-d2d7-45f0-8fbe-bfda0d33dae0", "Sea")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Rail = new ConversionFactor(100, Constants.Weight.Pounds, Constants.Volume.CubicFeet)
				{
					Description = ResString.GetMultilingualString("321dd800-8b50-11e6-868e-fcaa14295823", "Rail")
				};

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
				public static readonly ConversionFactor Domestic = new ConversionFactor(194, Constants.Volume.CubicInches, Constants.Weight.Pounds)
				{
					Description = ResString.GetMultilingualString("9ffeaf43-bd7e-4b84-ac26-d06c5f289150", "Domestic Movement")
				};
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
			public static readonly IEnumerable<ConversionFactor> All = new[]
			{
				Metric.Air,
				Metric.Sea,
				Metric.Rail,
				Metric.Road,
				Metric.RoadAUUS,
				Metric.LoadingMeters,
				Imperial.Air,
				Imperial.Sea,
				Imperial.Rail,
				Imperial.Domestic
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		public static readonly ConversionFactor Empty = new ConversionFactor();

		public static bool TryParse(ZString factorAsString, out ConversionFactor equation)
		{
			var match = CargoWise.Definitions.ConversionFactorConstants.Regex.Match(factorAsString);
			if (match.Success)
			{
				decimal conversionFactor;

				if (decimal.TryParse(match.Groups[1].Value, out conversionFactor))
				{
					var factor = conversionFactor;
					var numerator = match.Groups[2].Value;
					var denominator = match.Groups[3].Value;

					equation = new ConversionFactor(factor, numerator, denominator);
					return true;
				}
			}

			equation = ConversionFactor.Empty;
			return false;
		}

		public ConversionFactor(decimal factor, string numeratorUnit, string denominatorUnit)
			: this()
		{
			Factor = factor;
			NumeratorUnit = numeratorUnit;
			NumeratorMeasureType = GetMeasureUnitType(NumeratorUnit);
			DenominatorUnit = denominatorUnit;
			DenominatorMeasureType = GetMeasureUnitType(DenominatorUnit);
		}

		public ResourceString Description
		{
			get;
			private set;
		}

		public bool IsValid
		{
			get
			{
				return
				(
					(LoadingLength.LoadingMeters.Equals(DenominatorUnit) && Factor >= 0)
					|| Factor > 0
				)
				&& !string.IsNullOrWhiteSpace(NumeratorUnit)
				&& !string.IsNullOrWhiteSpace(DenominatorUnit)
				&& NumeratorUnit != DenominatorUnit;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return Factor == 0 && string.IsNullOrWhiteSpace(NumeratorUnit) && string.IsNullOrWhiteSpace(DenominatorUnit);
			}
		}

		public UnitsSystem UnitsSystem
		{
			get
			{
				return Constants.Weight.IsImperial(DenominatorUnit) || Constants.Volume.IsImperial(DenominatorUnit)
					? UnitsSystem.Imperial
					: UnitsSystem.Metric;
			}
		}

		public decimal Factor
		{
			get;
			private set;
		}

		public string NumeratorUnit
		{
			get;
			private set;
		}

		public MeasureUnitType NumeratorMeasureType
		{
			get;
			private set;
		}

		public string DenominatorUnit
		{
			get;
			private set;
		}

		public MeasureUnitType DenominatorMeasureType
		{
			get;
			private set;
		}

		static bool TrySameSubstanceUnitConvertion(IQuantity factor, string targetUnit, out IQuantity result)
		{
			result = null;

			if (Constants.Weight.ContainsCode(factor.Unit) && Constants.Weight.ContainsCode(targetUnit))
			{
				result = new ZWeight(Constants.Weight.Convert(factor.Amount, factor.Unit, targetUnit, false), targetUnit);
			}

			if (Constants.Volume.ContainsCode(factor.Unit) && Constants.Volume.ContainsCode(targetUnit))
			{
				result = new ZVolume(Constants.Volume.Convert(factor.Amount, factor.Unit, targetUnit, false), targetUnit);
			}

			return result != null;
		}

		public IQuantity Convert(IQuantity amount)
		{
			if (IsValid && amount != null && !amount.IsEmpty)
			{
				if (amount.Unit == NumeratorUnit)
				{
					var factor = Factor == 0
						? 0
						: ((1 / Factor) * amount.Amount);
					return GetResult(factor, DenominatorUnit);
				}

				if (amount.Unit == DenominatorUnit)
				{
					return GetResult(Factor * amount.Amount, NumeratorUnit);
				}

				IQuantity amountMetricImperialConverted;
				if (TrySameSubstanceUnitConvertion(amount, NumeratorUnit, out amountMetricImperialConverted)
					|| TrySameSubstanceUnitConvertion(amount, DenominatorUnit, out amountMetricImperialConverted))
				{
					return Convert(amountMetricImperialConverted);
				}
			}

			return default(Quantity);
		}

		public static bool operator ==(ConversionFactor factor1, ConversionFactor factor2)
		{
			return factor1.Equals(factor2);
		}

		public static bool operator !=(ConversionFactor factor1, ConversionFactor factor2)
		{
			return !factor1.Equals(factor2);
		}

		public override bool Equals(object obj)
		{
			if (obj is ConversionFactor)
			{
				var other = (ConversionFactor)obj;

				return Factor == other.Factor && NumeratorUnit == other.NumeratorUnit && DenominatorUnit == other.DenominatorUnit;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return Factor.GetHashCode();
		}

		public override string ToString()
		{
			return ToShortString();
		}

		/// <summary>
		///		Converts the value of the current <see cref="ConversionFactor"/> object to its equivalent short string representation.
		///		For example: 166 KG/M3, 166.67 KG/M3, 6000 CI/KG.
		/// </summary>
		public string ToShortString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0:G29} {1}/{2}", Factor, NumeratorUnit, DenominatorUnit);
		}

		/// <summary>
		///		Converts the value of the current <see cref="ConversionFactor"/> object to its equivalent long string representation.
		///		For example: 1 M3 = 166 KG, 1 M3 = 166.67 KG, 1 KG = 6000 CI.
		/// </summary>
		public string ToLongString()
		{
			return string.Format(CultureInfo.CurrentCulture, "1 {0:G29} = {1} {2}", DenominatorUnit, Factor, NumeratorUnit);
		}

		static IQuantity GetResult(decimal factor, string unit)
		{
			if (Constants.Weight.ContainsCode(unit))
			{
				return new ZWeight(factor, unit);
			}

			if (Constants.Volume.ContainsCode(unit))
			{
				return new ZVolume(factor, unit);
			}

			return new Quantity(factor, unit);
		}

		internal static MeasureUnitType GetMeasureUnitType(string unit)
		{
			if (Weight.ContainsCode(unit))
			{
				return MeasureUnitType.Weight;
			}
			else if (Volume.ContainsCode(unit))
			{
				return MeasureUnitType.Volume;
			}
			else if (LoadingLength.ContainsCode(unit))
			{
				return MeasureUnitType.LoadingLength;
			}
			else if (Length.ContainsCode(unit))
			{
				return MeasureUnitType.Length;
			}
			else if (Area.ContainsCode(unit))
			{
				return MeasureUnitType.Area;
			}
			else
			{
				return MeasureUnitType.Unknown;
			}
		}
	}
}
