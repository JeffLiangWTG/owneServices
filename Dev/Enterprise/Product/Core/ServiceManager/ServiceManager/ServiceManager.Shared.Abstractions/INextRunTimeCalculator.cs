using System;

namespace ServiceManager.Shared.Abstractions
{
	public interface INextRunTimeCalculator
	{
		public DateTimeOffset CalculateNextRunTime(DateTimeOffset calculateFrom);
	}
}
