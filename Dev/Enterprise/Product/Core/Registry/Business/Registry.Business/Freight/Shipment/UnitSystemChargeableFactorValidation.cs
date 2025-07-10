
namespace Enterprise.Registry.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using Enterprise.Core;
	using Enterprise.ZArchitecture.Business;

	public class UnitSystemChargeableFactorValidation : ConversionFactorValidation
	{
		public UnitSystemChargeableFactorValidation(ConversionFactorViewModel parent, UnitsSystem unitsSystem)
			: base(parent)
		{
			this.unitsSystem = unitsSystem;
		}

		protected override IEnumerable<UnitsList> GetSupportedUnits()
		{
			if (unitsSystem == UnitsSystem.Metric)
			{
				return new[]
				{
					new UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes.Where(c => !Constants.Weight.IsImperial(c)).ToArray()),
					new UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes.Where(c => !Constants.Volume.IsImperial(c)).ToArray()),
					new UnitsList(MeasureUnitType.LoadingLength, Constants.LoadingLength.Codes.ToArray()),
				};
			}
			else
			{
				return new[]
				{
					new UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes.Where(c => Constants.Weight.IsImperial(c)).ToArray()),
					new UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes.Where(c => Constants.Volume.IsImperial(c)).ToArray()),
				};
			}
		}

		readonly UnitsSystem unitsSystem;
	}
}
