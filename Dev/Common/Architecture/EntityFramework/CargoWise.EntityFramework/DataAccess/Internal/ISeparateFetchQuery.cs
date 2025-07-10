namespace CargoWise.EntityFramework
{
	public interface ISeparateFetchQuery
	{
		bool CannotBeJoinedInFetchHint { get; set; }
	}
}
