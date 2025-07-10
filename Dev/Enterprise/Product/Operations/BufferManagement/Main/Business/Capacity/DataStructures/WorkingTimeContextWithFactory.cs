using System;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Business
{
	public class WorkingTimeContextWithFactory : IWorkingTimeContext
	{
		public WorkingTimeContextWithFactory(WorkingTimeContext internalContext, BusinessObjectFactory factory)
		{
			this.internalContext = internalContext;
			this.factory = factory;
		}

		public Func<DateTime, DateTime, TimeSpan> TimeDifferenceFunc => internalContext.GetWorkTimeArithmetic(factory).TimeDifference;

		public DateTime ToLocalTime(DateTime dateTime)
		{
			var localTime = internalContext.ToLocalTime(dateTime, factory);
			return localTime.IsValid && !localTime.IsEmpty ? localTime.ToDateTime() : default(DateTime);
		}

		readonly WorkingTimeContext internalContext;
		readonly BusinessObjectFactory factory;
	}
}
