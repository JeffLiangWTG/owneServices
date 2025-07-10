using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(ImportDeclaration))]
	public class ImportDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSupplierAndImporterShouldBeImported()
		{
			var helper = new CreateDeclarationHelper();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "s0001";

			var dec = Factory.New<JobDeclaration>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_OH_Importer = importer.PK;

			var importJobDeclaration = helper.GetNewImportJobDeclaration(shipment);
			importJobDeclaration.DeclarationPK = dec.PK;
			importJobDeclaration.CreateNewStandAloneDeclaration(Factory);

			var importedDeclartion = (BaseJobDeclaration)ReflectionUtil.GetPropertyValue(importJobDeclaration, "ImportedDeclaration");
			AssertEquals("Supplier should be copied", supplier.PK, importedDeclartion.JE_OH_Supplier);
			AssertEquals("Importer should be copied", importer.PK, importedDeclartion.JE_OH_Importer);
		}
	}
}
