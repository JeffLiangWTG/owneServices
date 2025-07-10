using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IContainerPenaltyDayExclusion
	{
		ZBool CEX_Monday { get; }
		ZBool CEX_Tuesday { get; }
		ZBool CEX_Wednesday { get; }
		ZBool CEX_Thursday { get; }
		ZBool CEX_Friday { get; }
		ZBool CEX_Saturday { get; }
		ZBool CEX_Sunday { get; }
		ZBool CEX_Weekend { get; }
		ZBool CEX_Holiday { get; }

		IReadOnlyCollection<DayOfWeek> GetExcludedDaysOfWeek();
		bool IsEmpty();
	}
}
