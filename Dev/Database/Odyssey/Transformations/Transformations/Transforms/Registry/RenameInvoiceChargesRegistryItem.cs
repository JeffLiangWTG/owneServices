using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	public class RenameInvoiceChargesRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from InvoiceCharges to InvoiceChargesForExport";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName(registryItemOldName, registryItemNewName);
		}

		const string registryItemOldName = "InvoiceCharges";
		const string registryItemNewName = "InvoiceChargesForExport";
	}
}
