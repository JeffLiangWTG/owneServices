using System;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class OffsetToDateConverter
	{
		protected internal OffsetToDateConverter(WorkingTimeContext context, BusinessObjectFactory factory)
		{
			this.context = Argument.NotNull(context, "context");
			this.factory = Argument.NotNull(factory, "factory");
			workTimeArithmetic = context.GetWorkTimeArithmetic(factory);
		}

		readonly WorkingTimeContext context;
		readonly BusinessObjectFactory factory;
		readonly IWorkTimeArithmetic workTimeArithmetic;

		protected WorkingTimeContext Context
		{
			get { return context; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		protected IWorkTimeArithmetic WorkingDays
		{
			get { return workTimeArithmetic; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public virtual ZDateTime GetTimeInUtcForTimeOffset(decimal hoursOffset)
		{
			var now = context.GetCurrentLocalTime(factory).ToDateTime();
			var futureTime = new ZDateTime(workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(now, (double)hoursOffset), DateTimeKind.Local);

			return futureTime.ToUniversalBranchTime(context.Branch);
		}
	}
}
