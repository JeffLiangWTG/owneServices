using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Documents;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Documents
{
	[TestedType(typeof(RemoveBarcodeReaderServiceRegistries))]
	sealed class RemoveBarcodeReaderServiceRegistriesTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "BarcodeScanningProvider", "BarcodeScanningServiceUri" };
		}
	}
}
