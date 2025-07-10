using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WeightWrapper : ValueAndUnitWrapper, ITotalValueAndUnits
	{
		public WeightWrapper(ZDecimal value, ZString unitCode, ZInt decimalPlaces, CodeDescriptionPairList unitList, BusinessObjectFactory factory)
			: base(value, unitCode, decimalPlaces, unitList, factory) { }

		protected WeightWrapper(ZDecimal value, UnitWrapper unit, ZInt decimalPlaces, BusinessObjectFactory factory)
			: base(value, unit, decimalPlaces, factory) { }

		internal const int StandardDecimalPlaces = 1;

		internal new static WeightWrapper Empty
		{
			get { return new WeightWrapper(ZDecimal.Zero, ZString.Empty, StandardDecimalPlaces, new CodeDescriptionPairList(), null); }
		}

		public WeightWrapper InKilograms
		{
			get { return InAnotherUnit(Core.Constants.Weight.Kilograms); }
		}

		public WeightWrapper InPounds
		{
			get { return InAnotherUnit(Core.Constants.Weight.Pounds); }
		}

		public WeightWrapper InFreightWeightUnit
		{
			get
			{
				string desiredUnit = Env.Registry.FreightWeightUnit;
				if (!Core.Constants.Weight.ContainsCode(desiredUnit))
				{
					desiredUnit = Core.Constants.Weight.Kilograms;
				}
				return InAnotherUnit(desiredUnit);
			}
		}

		public WeightWrapper InPackageWeightUnit
		{
			get
			{
				string desiredUnit = Env.Registry.PackageWeightUnit;
				if (!Core.Constants.Weight.ContainsCode(desiredUnit))
				{
					desiredUnit = Core.Constants.Weight.Kilograms;
				}
				return InAnotherUnit(desiredUnit);
			}
		}

		public ValueAndUnitSelfTotaller GetNewForTotalling()
		{
			return new ValueAndUnitSelfTotaller(Value, Unit.Code);
		}

		public void AddSelfToResult(ValueAndUnitSelfTotaller result)
		{
			if (!result.EncounteredInvalidCode)
			{
				if (result.UnitCode == Unit.Code)
				{
					result.Value += Value;
				}
				else
				{
					if (Constants.Weight.ContainsCode(Unit.Code) && Constants.Weight.ContainsCode(result.UnitCode))
					{
						ZString systemDefaultWeightUnit = Env.Registry.FreightWeightUnit;
						if (result.UnitCode != systemDefaultWeightUnit)
						{
							result.Value = Constants.Weight.Convert(result.Value, result.UnitCode, systemDefaultWeightUnit);
							result.UnitCode = systemDefaultWeightUnit;
						}
						result.Value += Unit.Code == systemDefaultWeightUnit ? Value : (ZDecimal)Constants.Weight.Convert(Value, Unit.Code, systemDefaultWeightUnit);
					}
					else
					{
						result.EncounteredInvalidCode = true;
					}
				}
			}
		}

		WeightWrapper InAnotherUnit(string desiredUnit)
		{
			if (Unit.Code == desiredUnit)
			{
				return this;
			}
			else
			{
				ZDecimal newValue;

				if (!Constants.Weight.ContainsCode(Unit.Code))
				{
					newValue = 0m;
				}
				else
				{
					newValue = Core.Constants.Weight.Convert(Value, Unit.Code, desiredUnit);
				}

				return new WeightWrapper(newValue, new UnitWrapper(desiredUnit, Unit), DecimalPlaces, Factory);
			}
		}
	}
}
