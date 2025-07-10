
namespace Enterprise.ResourceStrings.Business
{
	public interface IDocBuilderUsageFinder
	{
		bool IsInitialized { get; }
		void Initialize();
		IDocBuilderUsageCollection Find(string key);
	}
}
