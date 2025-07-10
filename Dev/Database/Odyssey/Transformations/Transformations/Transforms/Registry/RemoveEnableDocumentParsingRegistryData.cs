using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	class RemoveEnableDocumentParsingRegistryData : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => ["EnableDocumentParsing"];
	}
}
