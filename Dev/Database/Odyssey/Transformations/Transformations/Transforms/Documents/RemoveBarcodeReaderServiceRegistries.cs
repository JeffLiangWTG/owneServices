using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Documents
{
	public class RemoveBarcodeReaderServiceRegistries : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "BarcodeScanningProvider", "BarcodeScanningServiceUri" };
		}
	}
}
