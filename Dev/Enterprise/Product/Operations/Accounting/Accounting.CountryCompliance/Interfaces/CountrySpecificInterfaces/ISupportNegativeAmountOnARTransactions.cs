namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface ISupportNegativeAmountOnARTransactions
	{
		bool IsNegativeChargesAllowed { get; }
	}
}
