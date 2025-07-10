namespace Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax
{
	/// <summary>
	/// Interface to get or create an instance of a IUSSalesTaxCalculator.
	/// </summary>
	public interface IUSSalesTaxCalculatorFactory
	{
		IUSSalesTaxCalculator Get();
	}
}
