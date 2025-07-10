using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	public class TableProcessingInfo : ITableProcessingInfo
	{
		public TableProcessingInfo(int count, bool purged = true)
		{
			Count = count;
			Purged = purged;
		}

		public int Count { get; set; }
		public bool Purged { get; }
	}
}
