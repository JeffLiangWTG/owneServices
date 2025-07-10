using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR
{
	public class RemoveSADGenerationConfigurationRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { SADGenerationOnConfirmedExitEnabled };
		}

		const string SADGenerationOnConfirmedExitEnabled = "SADGenerationOnConfirmedExitEnabled";
	}
}
