namespace Enterprise.Integration.Accounting
{
	public interface IBatchCreatorStrategy
	{
		/// <summary>
		/// Returns true if a country supports the behavior of wait for the original transaction for processing amending transactions
		/// </summary>
		bool SupportsWaitForOriginalTransactionForAmending { get; }
	}
}
