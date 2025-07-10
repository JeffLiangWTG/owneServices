using System.Collections;
using CargoWise.Application;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Business
{
	class EnterpriseArchiveableBusinessObjectProviderCache : ArchiveableBusinessObjectProviderCache
	{
		public EnterpriseArchiveableBusinessObjectProviderCache()
		{
			var providerList = ObjectFactory.Get<IEnumerable>("ArchiveableBusinessObjectProviderList");

			foreach (IArchiveableBusinessObjectProvider provider in providerList)
			{
				RegisterProvider(provider);
			}
		}
	}
}
