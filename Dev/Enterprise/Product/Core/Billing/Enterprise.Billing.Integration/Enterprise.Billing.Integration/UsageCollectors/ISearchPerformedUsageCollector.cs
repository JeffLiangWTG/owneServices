namespace Enterprise.Billing.Integration;

public interface ISearchPerformedUsageCollector
{
	void Report(string searchType, string moduleId, string filters);
}
