using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared;

[TestedType(typeof(RemoveFlagshipRegistry))]
public class RemoveFlagshipRegistryTest : DeleteRegistryItemTest
{
	protected override string[] GetRegistryItemNames() => new[] { "FlagshipDownloadDirectory", "FlagshipUploadDirectory", "FlagshipUploadExtension", "FlagshipUseUnlocoIfMissingUSCode" };
}
