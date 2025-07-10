using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.eHubMessaging.Business
{
	public static class Utils
	{
		public static string GetFullPath(IRegistryItem registryItem)
		{
			var multilingualRegistryItem = registryItem as IMultilingualRegistryItem;
			return ResString.GetMultilingualString("1c1a85a3-eb5e-42a2-ad9b-d29980ef7db1", "[System] > [Registry] > [{0}] > [{1}]", multilingualRegistryItem.CategoryMultilingual.Replace("/", "] > ["), multilingualRegistryItem.CaptionMultilingual);
		}
	}
}
