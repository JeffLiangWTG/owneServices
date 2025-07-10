namespace Enterprise.AlwaysOn.Setup
{
	public interface IValidationStatus
	{
		bool IsLoaded { get; }
		bool HasErrors { get; }
		string LastErrorMessage { get; }
	}
}
