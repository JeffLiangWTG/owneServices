namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveSystemCache
	{
		void Add<T>(T cacheObject);
		T Retrieve<T>();
	}
}
