using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow
{
	class RemoveGlowExistingValidationAlertsPreventSave : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "GlowExistingValidationAlertsPreventSave" };
		}
	}
}
