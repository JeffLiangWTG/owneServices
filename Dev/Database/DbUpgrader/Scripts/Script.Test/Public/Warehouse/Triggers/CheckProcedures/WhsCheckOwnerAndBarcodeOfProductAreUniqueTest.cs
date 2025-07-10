using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckOwnerAndBarcodeOfProductAreUnique))]
	class WhsCheckOwnerAndBarcodeOfProductAreUniqueTest : DbCreateScriptTest
	{
		// Test in Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing.TG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique
		// Test in Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing.TG_OrgPartRelation_EnsureOwnerOfProductWithSameBarcodeIsUnique
		// Test in Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing.TG_OrgSupplierPart_EnsureActiveProductMustHasUniqueOwnerAndBarcode
	}
}

