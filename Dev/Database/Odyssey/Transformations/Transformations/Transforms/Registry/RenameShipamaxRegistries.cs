using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RenameShipamaxRegistries : RegistryDataTransformation
	{
		public override string UserDescription => "Rename EnableShipamaxIntegration to EnableDocumentParsing, ShipamaxIntegrationUrl to DocumentParserUrl and ShipamaxClientId to DocumentParserClientId";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("EnableShipamaxIntegration", "EnableDocumentParsing");
			UpdateRegistryItemName("ShipamaxIntegrationUrl", "DocumentParserUrl");
			UpdateRegistryItemName("ShipamaxClientId", "DocumentParserClientId");
		}
	}
}
