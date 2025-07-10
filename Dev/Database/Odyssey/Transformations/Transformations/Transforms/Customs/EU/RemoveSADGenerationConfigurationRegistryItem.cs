using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	public class RemoveSADGenerationConfigurationRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { SADGenerationOnClearanceEnabled };
		}

		const string SADGenerationOnClearanceEnabled = "SADGenerationOnClearanceEnabled";
	}
}
