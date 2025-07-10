namespace Enterprise.Dash.Integration.Validation
{
	public interface IValidationRule<T>
	{
		string Validate(T dashCommercialInvoice);
	}
}
