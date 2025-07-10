namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors
{
	public class DummyProviderHelper
	{
		public ArchiveableBusinessObjectProviderCache GetAndRegisterProviderCache(ArchiveableBusinessObjectProviderCache providerCache)
		{
			providerCache = new ArchiveableBusinessObjectProviderCache();

			var dummyProvider = new DummyArchiveableBusinessObjectProvider();
			providerCache.RegisterProvider(dummyProvider);
			return providerCache;
		}
	}
}
