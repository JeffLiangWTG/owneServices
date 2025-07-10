using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class VolumeWrapper : ValueAndUnitWrapper, ITotalValueAndUnits
	{
		public VolumeWrapper(ZDecimal value, ZString unitCode, ZInt decimalPlaces, CodeDescriptionPairList unitList, BusinessObjectFactory factory)
			: base(value, unitCode, decimalPlaces, unitList, factory) { }

		public VolumeWrapper(ZDecimal value, ZString unitCode, CodeDescriptionPairList unitList, BusinessObjectFactory factory)
			: base(value, unitCode, StandardDecimalPlaces, unitList, factory) { }

		protected VolumeWrapper(ZDecimal value, UnitWrapper unit, BusinessObjectFactory factory)
			: base(value, unit, StandardDecimalPlaces, factory) { }

		internal const int StandardDecimalPlaces = 3;

		internal new static VolumeWrapper Empty
		{
			get { return new VolumeWrapper(ZDecimal.Zero, ZString.Empty, new CodeDescriptionPairList(), null); }
		}

		public VolumeWrapper InCubicMeters
		{
			get { return InAnotherUnit(Core.Constants.Volume.CubicMetres); }
		}

		public VolumeWrapper InCubicFeet
		{
			get { return InAnotherUnit(Core.Constants.Volume.CubicFeet); }
		}

		public VolumeWrapper InFreightVolumeUnit
		{
			get
			{
				string desiredUnit = Env.Registry.FreightVolumeUnit;
				if (!Core.Constants.Volume.ContainsCode(desiredUnit))
				{
					desiredUnit = Core.Constants.Volume.CubicMetres;
				}
				return InAnotherUnit(desiredUnit);
			}
		}

		public VolumeWrapper InPackageVolumeUnit
		{
			get
			{
				string desiredUnit = Env.Registry.PackageVolumeUnit;
				if (!Core.Constants.Volume.ContainsCode(desiredUnit))
				{
					desiredUnit = Core.Constants.Volume.CubicMetres;
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
					if (Constants.Volume.ContainsCode(Unit.Code) && Constants.Volume.ContainsCode(result.UnitCode))
					{
						ZString systemDefaultWeightUnit = Env.Registry.FreightVolumeUnit;
						if (result.UnitCode != systemDefaultWeightUnit)
						{
							result.Value = Constants.Volume.Convert(result.Value, result.UnitCode, systemDefaultWeightUnit);
							result.UnitCode = systemDefaultWeightUnit;
						}
						result.Value += Unit.Code == systemDefaultWeightUnit ? Value : (ZDecimal)Constants.Volume.Convert(Value, Unit.Code, systemDefaultWeightUnit);
					}
					else
					{
						result.EncounteredInvalidCode = true;
					}
				}
			}
		}

		VolumeWrapper InAnotherUnit(string desiredUnit)
		{
			if (Unit.Code == desiredUnit)
			{
				return this;
			}
			else
			{
				ZDecimal newValue;

				if (!Constants.Volume.ContainsCode(Unit.Code))
				{
					newValue = 0m;
				}
				else
				{
					newValue = Core.Constants.Volume.Convert(Value, Unit.Code, desiredUnit);
				}

				return new VolumeWrapper(newValue, new UnitWrapper(desiredUnit, Unit), Factory);
			}
		}
	}
}
