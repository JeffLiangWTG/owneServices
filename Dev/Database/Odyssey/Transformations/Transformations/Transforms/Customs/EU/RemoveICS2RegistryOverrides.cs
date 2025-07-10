using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

public class RemoveICS2RegistryOverrides : DeleteRegistryItem
{
	protected override string[] GetRegistryItemNames() => new[] { "EnableICS2Functions", "IncludeMemberStateInSiteID", "ICS2SenderPartyIdRegistry" };
}
