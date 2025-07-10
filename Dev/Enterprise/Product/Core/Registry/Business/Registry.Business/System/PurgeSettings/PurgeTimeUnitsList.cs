using Enterprise.ZArchitecture.Core;
namespace Enterprise.Registry.Business;

public class PurgeTimeUnitsList : CodeDescriptionPairList
{
	public PurgeTimeUnitsList()
	{
		AddPair(TimeUnit.Week, TimeUnit.WeekDescription, TimeUnit.WeekDescription);
		AddPair(TimeUnit.Month, TimeUnit.MonthDescription, TimeUnit.MonthDescription);
		AddPair(TimeUnit.Year, TimeUnit.YearDescription, TimeUnit.YearDescription);
	}
}
