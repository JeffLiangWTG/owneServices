using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
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
					calculationRuleRate.ValueFromInfo.AddError(Res.GetString("12DE5F6E-E233-4B82-AFE8-71A0AE55B8CB", "Value From must be greater than zero."));
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

		string FlatRateOrUpliftRequiredMessage => Res.GetString("DF16FC87-8EB4-4A38-A9E5-E288C8D8D9B3", "Either Flat Rate or Uplift must have a value.");

		#endregion

	}
}
