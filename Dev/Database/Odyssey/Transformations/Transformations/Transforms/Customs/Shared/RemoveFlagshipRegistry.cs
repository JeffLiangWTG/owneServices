using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

public class RemoveFlagshipRegistry : DeleteRegistryItem
{
	protected override string[] GetRegistryItemNames() => new[] { "FlagshipDownloadDirectory", "FlagshipUploadDirectory", "FlagshipUploadExtension", "FlagshipUseUnlocoIfMissingUSCode" };
}
