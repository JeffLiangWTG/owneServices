using System;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class AccPeriodSchedule : Schedule
	{
		public AccPeriodSchedule()
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PeriodScope = PeriodScopeList.Codes.This;
		}

		public static bool TryParse(DateTime storageValue, out AccPeriodSchedule result)
		{
			bool canParse;
			if (storageValue.Day == 4 && ValidRange(storageValue))
			{
				result = new AccPeriodSchedule();
				result.PopulatePeriodScopeAndNumber(storageValue);
				canParse = true;
			}
			else
			{
				result = null;
				canParse = false;
			}
			return canParse;
		}

		public int GetSchedulePeriod()
		{
			var result = IsValid ? Calculator.CalculateAccountingPeriod(BaseDateTime, PeriodsToAdd) : 0;
			shouldClearDate = false;
			return result;
		}

		public void CopyChangesFrom(AccPeriodSchedule sourceSchedule)
		{
			PeriodScope = sourceSchedule.PeriodScope;
			PeriodCount = sourceSchedule.PeriodCount;
		}

		internal override string PeriodDescription
		{
			get { return Res.GetString("a1095f0f-e6cf-4e54-86a3-a6f4f7b0a3b2", "accounting period"); }
		}

		protected override DateTime ToStorageValueCore(DateTime periodAdjustedValue)
		{
			return new DateTime(periodAdjustedValue.Year, periodAdjustedValue.Month, 4);
		}

		#region FillData

		internal void FillData(AccPeriodScheduleData data)
		{
			PeriodCount = data.PeriodCount;
			PeriodScope = data.PeriodScope;
		}

		internal AccPeriodScheduleData ExtractData()
		{
			if (!IsValid)
			{
				return null;
			}
			var data = new AccPeriodScheduleData();
			data.PeriodScope = PeriodScope;
			data.PeriodCount = PeriodCount;

			data.CalculatedResult = Description;
			data.StorageValue = ToStorageValue();
			data.SchedulePeriod = GetSchedulePeriod();
			return data;
		}

		#endregion
	}
}
