using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RenameEnableICS2FunctionsRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from EnableICS2FunctionsRegistry to EnableICS2Functions";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("EnableICS2FunctionsRegistry", "EnableICS2Functions");
		}
	}
}
