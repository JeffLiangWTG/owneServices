namespace Enterprise.Accounting.Business.TransactionApproval
{
	public interface ISupportMixedLevelAuthorization
	{
		bool EnforceToCheckNextLevelOfAuthorization { get; set; }
	}
}
