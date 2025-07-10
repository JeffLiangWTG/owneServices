namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public interface IDirectDebitBatchComponent
	{
		bool ShouldValidateDirectDebitBatchComponent { get; }
	}
}