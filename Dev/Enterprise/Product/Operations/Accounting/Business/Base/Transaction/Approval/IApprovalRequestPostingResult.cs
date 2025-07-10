namespace Enterprise.Accounting.Business.TransactionApproval
{
	public interface IApprovalRequestPostingResult
	{
		string ErrorMessage { get; }
		IBulkPostingCache BulkPostingCache { get; }
	}
}
