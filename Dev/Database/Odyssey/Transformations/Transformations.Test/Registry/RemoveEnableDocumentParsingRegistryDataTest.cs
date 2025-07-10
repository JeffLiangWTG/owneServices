using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(RemoveEnableDocumentParsingRegistryData))]
	class RemoveEnableDocumentParsingRegistryDataTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => ["EnableDocumentParsing"];
	}
}
