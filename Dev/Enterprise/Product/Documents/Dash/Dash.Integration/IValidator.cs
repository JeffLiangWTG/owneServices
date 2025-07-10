namespace Enterprise.Dash.Integration
{
	public interface IValidator<T>
	{
		string Validate(T businessObject);
	}
}
