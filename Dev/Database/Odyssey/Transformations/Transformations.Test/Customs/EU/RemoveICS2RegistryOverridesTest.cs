using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.EU;

[TestedType(typeof(RemoveICS2RegistryOverrides))]
public class RemoveICS2RegistryOverridesTest : DeleteRegistryItemTest
{
	protected override string[] GetRegistryItemNames() => new[] { "EnableICS2Functions", "IncludeMemberStateInSiteID", "ICS2SenderPartyIdRegistry" };
}
