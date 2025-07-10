namespace CargoWise.Integration
{
	public interface IDelayedTransactionManager : ITransactionManager
	{
		bool IsRollingback { get; }
		void ReportInvalidRollbackExceptionHandling();
	}
}
