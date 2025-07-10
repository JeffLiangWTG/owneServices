using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence
{
	public class DeleteEdwTranslationTableLoadPeriodRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "EdwTranslationTableLoadPeriod" };
	}
}
