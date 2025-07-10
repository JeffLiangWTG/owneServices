using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public abstract class GldDateRangeSettingValidation : ZValidation
	{
		public GldDateRangeSettingValidation(GldDateRangeSetting parent)
			: base(parent)
		{
			Parent = parent;
		}

		public override void ValidateAll()
		{
			ValidateStartDate();
			ValidateEndDate();
		}

		public void ValidateStartDate()
		{
			ValidateCalculatedProperty(Parent.StartDateInfo);
		}

		protected virtual void CheckStartDate()
		{
		}

		public void ValidateEndDate()
		{
			ValidateCalculatedProperty(Parent.EndDateInfo);
		}

		protected virtual void CheckEndDate()
		{
		}

		public override Type AutoValidationType => GetType();

		GldDateRangeSetting Parent { get; }

		protected AccountingPeriodCalculator PeriodCalculator => calc ?? (calc = new AccountingPeriodCalculator(new BusinessObjectFactory()));
		AccountingPeriodCalculator calc;
	}
}