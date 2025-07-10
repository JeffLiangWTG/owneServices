namespace Enterprise.Accounting.Integration
{
	public interface IFinancialInvoiceDataAdapter
	{
		bool RunExtraValidation { get; set; }
	}
}
