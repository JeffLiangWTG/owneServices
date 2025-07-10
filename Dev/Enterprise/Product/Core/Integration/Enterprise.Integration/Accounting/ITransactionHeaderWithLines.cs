namespace Enterprise.Integration.Accounting
{
	public interface ITransactionHeaderWithLines
	{
		ITransactionLine[] Lines { get; }
	}
}
