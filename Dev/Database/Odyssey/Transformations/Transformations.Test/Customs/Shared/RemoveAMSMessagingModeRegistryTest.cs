using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared;

[TestedType(typeof(RemoveAMSMessagingModeRegistry))]
public class RemoveAMSMessagingModeRegistryTest : DeleteRegistryItemTest
{
	protected override string[] GetRegistryItemNames() => new[] { "AMSMessagingMode" };
}

