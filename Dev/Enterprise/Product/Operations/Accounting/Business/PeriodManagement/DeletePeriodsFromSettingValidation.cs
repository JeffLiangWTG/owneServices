using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class DeletePeriodsFromSettingValidation : ZValidation
	{
		public DeletePeriodsFromSettingValidation(DeletePeriodsFromSetting parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			ValidateDeletePeriodsFrom();
		}

		public void ValidateDeletePeriodsFrom()
		{
			Parent.DeletePeriodsFromInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.DeletePeriodsFromInfo, Res.GetString("ec92a3e0-4851-490b-a5fa-370f3c8ed00d", "Start Date"));
			TypeValidation.CheckValidSmallDateTime(Parent.DeletePeriodsFromInfo);
			if (!Parent.DeletePeriodsFromInfo.HasErrors())
			{
				var firstDayForPeriod = PeriodCalculator.GetFirstDayForPeriod(Parent.DeletePeriodsFrom);
				if (firstDayForPeriod.Date != Parent.DeletePeriodsFrom.Date)
				{
					Parent.DeletePeriodsFromInfo.AddError(Res.GetString("62239CAC-967D-43D8-8048-B2923BB15930", "Start Date must be equal to First Day of an Accounting Period in a Financial Year"));
				}
			}
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		public readonly DeletePeriodsFromSetting Parent;

		AccountingPeriodCalculator PeriodCalculator => calc ?? (calc = new AccountingPeriodCalculator(new BusinessObjectFactory()));
		AccountingPeriodCalculator calc;
	}
}
