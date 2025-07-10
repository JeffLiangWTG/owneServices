using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

public class RemoveAMSMessagingModeRegistry : DeleteRegistryItem
{
	protected override string[] GetRegistryItemNames() => new[] { "AMSMessagingMode" };
}
