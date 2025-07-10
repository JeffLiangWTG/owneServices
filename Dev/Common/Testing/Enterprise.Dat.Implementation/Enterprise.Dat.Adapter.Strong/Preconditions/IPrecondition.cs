namespace Enterprise.Dat.Implementation.Preconditions
{
	public interface IPrecondition
	{
		string ErrorMessage { get; }
		bool CheckPreconditionMet();
	}
}
