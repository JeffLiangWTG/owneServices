using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by validation via reflection")]
	public class CusCalculationRuleRateValidation : ZValidation
	{
		public CusCalculationRuleRateValidation(CusCalculationRuleRate parent)
			: base(parent)
		{
			this.calculationRuleRate = parent;
		}

		readonly CusCalculationRuleRate calculationRuleRate;

		public override void ValidateAll()
		{
			ValidateValueFrom();
			ValidateFlatRate();
			ValidateUplift();
		}

		public override Type AutoValidationType
		{
			get { return typeof(CusCalculationRuleRateValidation); }
		}

		#region Value From

		public void ValidateValueFrom()
		{
			ValidateCalculatedProperty(calculationRuleRate.ValueFromInfo);
		}

		void CheckValueFrom()
		{
			if (!calculationRuleRate.IsFirstRate)
			{
				var valueFrom = calculationRuleRate.ValueFrom;
				if (valueFrom < 0)
				{
					calculationRuleRate.ValueFromInfo.AddError(Res.GetString("BCAF8559-7B50-467D-BEC0-00D1D2A752F8", "Value From must be greater than zero."));
				}

				if (!calculationRuleRate.ValueFromInfo.HasErrors())
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(calculationRuleRate.ValueFromInfo, true);
				}
			}
		}

		#endregion

		#region Flat Rate

		public void ValidateFlatRate()
		{
			ValidateCalculatedProperty(calculationRuleRate.FlatRateInfo);
		}

		void CheckFlatRate() => CheckFlatRateOrUpliftIsSpecified(calculationRuleRate.FlatRateInfo);

		void CheckFlatRateOrUpliftIsSpecified(ZPropertyInfo propertyInfo)
		{
			var flatRate = calculationRuleRate.FlatRate;
			var uplift = calculationRuleRate.Uplift;
			if (flatRate.IsEmpty && uplift.IsEmpty)
			{
				propertyInfo.AddWarning(FlatRateOrUpliftRequiredMessage);
			}
		}

		#endregion

		#region Uplift

		public void ValidateUplift()
		{
			ValidateCalculatedProperty(calculationRuleRate.UpliftInfo);
		}

		void CheckUplift() => CheckFlatRateOrUpliftIsSpecified(calculationRuleRate.UpliftInfo);

		string FlatRateOrUpliftRequiredMessage => Res.GetString("A28A21A8-9A72-4BD4-A99E-BB99D4B0A2B0", "Either Flat Rate or Uplift must have a value.");

		#endregion

	}
}
