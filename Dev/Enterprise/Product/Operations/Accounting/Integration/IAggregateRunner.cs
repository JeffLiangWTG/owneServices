
namespace Enterprise.Accounting.Integration
{
	public interface IAggregateRunner
	{
		bool Aggregate();
		bool ReAggregate();
		bool ReAggregateAndReportAsXML();
		string AggregateResult { get; set; }
		bool FailedToAquireMutex { get; }
		string FailedToAquireMutexReason { get; }
		bool FailedWithDeadlock { get; }
	}
}
