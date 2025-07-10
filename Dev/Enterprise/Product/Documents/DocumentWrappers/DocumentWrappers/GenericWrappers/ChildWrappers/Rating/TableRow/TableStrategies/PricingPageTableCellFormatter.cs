using System;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	internal class PricingPageTableCellFormatter
	{
		public const int UnitColumn = 0;

		public PricingPageTableCellFormatter(string expectedUnit)
		{
			this.expectedUnit = expectedUnit;
		}

		public virtual int RequiredColumnCount => 1;

		public bool TryFormat(RateLine rateLine, DocAmount[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (values.Length < RequiredColumnCount)
			{
				throw new ArgumentException("values array too short", nameof(values));
			}

			DocAmount.Clear(values, 0, RequiredColumnCount);

			if (TryFormatCore(rateLine, values))
			{
				return true;
			}
			else
			{
				values[UnitColumn] = DocAmount.Create((NoResString)FailValue);
				return false;
			}
		}

		protected bool TryFormatCore(RateLine rateLine, DocAmount[] values)
		{
			switch (rateLine.TL_RateCalculator)
			{
				case UnitCalculator.Code:
					return TryFormat(rateLine, (UnitCalculator)rateLine.Calculator, values);

				case FlatCalculator.Code:
					return TryFormat(rateLine, (FlatCalculator)rateLine.Calculator, values);

				case FlatPlusPerUnitCalculator.Code:
					return TryFormat(rateLine, (FlatPlusPerUnitCalculator)rateLine.Calculator, values);

				case MinimumOrPerUnitCalculator.Code:
					return TryFormat(rateLine, (MinimumOrPerUnitCalculator)rateLine.Calculator, values);

				case FirstPlusAdditionalCalculator.Code:
					return TryFormat(rateLine, (FirstPlusAdditionalCalculator)rateLine.Calculator, values);

				case CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode:
				case CompanyTariffOrCostBasedCalculator.CostBasedCode:
					throw new DeveloperNotificationException("Cost or Company Tariff based calculators should not reach this place and should be all replaced by base rate lines by this moment.");

				default:
					return false;
			}
		}

		protected virtual bool TryFormat(RateLine rateLine, FlatCalculator calculator, DocAmount[] values)
		{
			DocAmount value;

			if (TryFormatFlat(rateLine, calculator.BaseRate, out value))
			{
				values[UnitColumn] = value;
				return true;
			}
			else
			{
				return false;
			}
		}

		protected virtual bool TryFormat(RateLine rateLine, UnitCalculator calculator, DocAmount[] values)
		{
			DocAmount value;

			if (TryFormatUnit(rateLine, calculator.PerUnit, out value))
			{
				values[UnitColumn] = value;
				return true;
			}
			else
			{
				return false;
			}
		}

		protected virtual bool TryFormat(RateLine rateLine, FlatPlusPerUnitCalculator calculator, DocAmount[] values)
		{
			DocAmount value;
			bool result;

			if (calculator.BaseRate == 0)
			{
				result = TryFormatUnit(rateLine, calculator.PerUnit, out value);
			}
			else if (calculator.PerUnit == 0)
			{
				result = TryFormatFlat(rateLine, calculator.BaseRate, out value);
			}
			else
			{
				result = false;
				value = null;
			}

			values[UnitColumn] = value;
			return result;
		}

		protected virtual bool TryFormat(RateLine rateLine, MinimumOrPerUnitCalculator calculator, DocAmount[] values)
		{
			DocAmount value;
			bool result;

			if (calculator.Minimum == 0)
			{
				result = TryFormatUnit(rateLine, calculator.PerUnit, out value);
			}
			else
			{
				result = false;
				value = null;
			}

			values[UnitColumn] = value;
			return result;
		}

		protected virtual bool TryFormat(RateLine rateLine, FirstPlusAdditionalCalculator calculator, DocAmount[] values)
		{
			DocAmount value;
			bool result;

			if (calculator.First == calculator.Additional)
			{
				result = TryFormatUnit(rateLine, calculator.Additional, out value);
			}
			else if (calculator.Additional == 0)
			{
				result = TryFormatFlat(rateLine, calculator.First, out value);
			}
			else
			{
				result = false;
				value = null;
			}

			values[UnitColumn] = value;
			return result;
		}

		protected virtual bool TryFormatFlat(RateLine rateLine, ZDecimal amount, out DocAmount value)
		{
			if (amount == 0)
			{
				value = DocAmount.Create(ZDecimal.Zero, rateLine.Currency.Decimals);
			}
			else
			{
				var text = Res.GetString("963D18D6-BA3D-4B9F-B010-7286D9CB3AF2", "Flat");
				value = DocAmount.Create(amount, rateLine.Currency.Decimals, "{0} " + text);
			}

			return true;
		}

		protected virtual bool TryFormatMinimum(RateLine rateLine, ZDecimal amount, out DocAmount value)
		{
			value = DocAmount.Create(amount, rateLine.Currency.Decimals);
			return true;
		}

		protected virtual bool TryFormatUnit(RateLine rateLine, ZDecimal amount, out DocAmount value)
		{
			if (amount == 0 || (rateLine.TL_WeightVolume == expectedUnit && (rateLine.TL_WeightVolumeMultiple == 1 || rateLine.TL_WeightVolumeMultiple == 0)))
			{
				value = DocAmount.Create(amount, rateLine.Currency.Decimals);
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}

		protected static string FailValue => Res.GetString("11ecc807-fed0-4c89-8004-125cfc2d6f07", "See Below");

		readonly string expectedUnit;
	}
}
