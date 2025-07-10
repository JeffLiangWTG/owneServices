namespace Enterprise.Services.OperationalActions.Support
{
	public interface IOverrideSelectionCount
	{
		int SelectionCount { get; }

		bool SupportRunningOnAllMatchedRecords { get; }
	}
}
