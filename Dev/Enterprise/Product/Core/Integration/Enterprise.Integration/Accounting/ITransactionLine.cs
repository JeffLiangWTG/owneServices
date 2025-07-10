namespace Enterprise.Integration.Accounting
{
	public interface ITransactionLine
	{
		decimal LocalTotalAmount { get; }
		decimal LocalTaxAmount { get; }
		decimal LocalExTaxAmount { get; }
		string TaxRateCode { get; }
	}
}
