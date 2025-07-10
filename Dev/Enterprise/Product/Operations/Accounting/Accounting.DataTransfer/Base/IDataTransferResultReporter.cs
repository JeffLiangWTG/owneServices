namespace Enterprise.Accounting.DataTransfer
{
	public interface IDataTransferResultReporter
	{
		bool WasTheLastDataTransferSuccessful { get; }
	}
}