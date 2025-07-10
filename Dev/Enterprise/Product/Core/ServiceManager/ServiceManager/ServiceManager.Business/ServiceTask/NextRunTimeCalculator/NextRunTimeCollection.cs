using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class NextRunTimeRecordCollection : NonPersistentBusinessObjectCollection<NextRunTimeRecord>
	{
		public NextRunTimeRecordCollection() { }

		public void Load(StmServiceTask serviceTask)
		{
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				var previousRunTime = serviceTask.NextRunTime.ToOffset().ToDateTimeOffsetSafe();

				if (!previousRunTime.HasValue || serviceTask.NextRunTimeCalculator is null)
				{
					return;
				}

				for (var i = 0; i < NextRunTimeEstimatesLimit; i++)
				{
					var nextRunTime = serviceTask.NextRunTimeCalculator
						.CalculateNextRunTime(previousRunTime.Value.AddSeconds(DailyOrGreaterCalculatorAdjustment(serviceTask.NextRunTimeCalculator)));

					Add(new NextRunTimeRecord
					{
						NextRunTime = new ZDateTime(nextRunTime.UtcDateTime),
						NextRunTimeLocal = new ZDateTime(nextRunTime.LocalDateTime)
					});
					previousRunTime = nextRunTime;
				}
			}
		}

		internal const int NextRunTimeEstimatesLimit = 20;

		int DailyOrGreaterCalculatorAdjustment(INextRunTimeCalculator calculator) =>
			calculator switch
			{
				NextRunTimeCalculatorDays or
					NextRunTimeCalculatorWorkingDays or
					NextRunTimeCalculatorWeeks or
					NextRunTimeCalculatorMonthsByDate or
					NextRunTimeCalculatorMonthsByDayOfWeek or
					NextRunTimeCalculatorMonthsByLastDay or
					NextRunTimeCalculatorYearsByDate or
					NextRunTimeCalculatorYearsByDayOfMonth => 1,
				_ => 0
			};

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NextRunTimeRecord { NextRunTime = ZDateTime.Empty, NextRunTimeLocal = ZDateTime.Empty };
		}

		#endregion
	}
}

