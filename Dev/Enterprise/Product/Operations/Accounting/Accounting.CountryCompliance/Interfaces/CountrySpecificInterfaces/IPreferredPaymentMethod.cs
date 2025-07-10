namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IPreferredPaymentMethod
	{
		string GetPreferredPaymentMethodWarning(string orgCategory, string paymentMethod);
	}
}
