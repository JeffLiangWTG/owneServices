using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Glow
{
	[TestedType(typeof(RemoveGlowExistingValidationAlertsPreventSave))]
	class RemoveGlowExistingValidationAlertsPreventSaveTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "GlowExistingValidationAlertsPreventSave" };
		}
	}
}
