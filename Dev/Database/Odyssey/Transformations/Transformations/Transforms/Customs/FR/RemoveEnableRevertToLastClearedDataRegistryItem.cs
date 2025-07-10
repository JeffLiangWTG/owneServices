using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR
{
	public class RemoveEnableRevertToLastClearedDataRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { EnableRevertToLastClearedData };
		}

		const string EnableRevertToLastClearedData = "EnableRevertToLastClearedData";
	}
}
