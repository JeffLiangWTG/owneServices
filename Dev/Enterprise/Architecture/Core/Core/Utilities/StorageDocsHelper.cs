using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public static class StorageDocsHelper
	{
		public static string GetMaximumLimitSizeNotifications(IntRegistryItem eDocsMaximumFilesize)
			=> SourceGenerated.Res.GetString("AAF48377-DAF7-4340-9118-7323C6493A3B", "The following files are larger than the maximum file size ({0}MB) specified in the registry '{1}'", eDocsMaximumFilesize.Value, ((IMultilingualRegistryItem)eDocsMaximumFilesize).LocationMultilingual);
	}
}
