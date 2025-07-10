namespace Enterprise.ArchiveManager.Integration
{
	public interface ITableProcessingInfo
	{
		int Count { get; set; }
		bool Purged { get; }
	}
}
