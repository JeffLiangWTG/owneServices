namespace Enterprise.ZArchitecture.Business
{
	public interface ICanBeExcludedFromOperationalActions
	{
		bool ShouldExclude { get; }
		string ReasonForExclusion { get; }
	}
}
