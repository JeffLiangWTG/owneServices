using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestDefaultForNewElementCoreForExport()
		{
			var supplier = GetOrganisation();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = GetOrganisation().PK;
			AssertEquals("Precondition", KRJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			var declarationManufacturer = GetOrganisation();
			declaration.JE_OH_Manufacturer = declarationManufacturer.PK;
			declaration.JE_OA_ManufacturerAddress = declarationManufacturer.MainAddress.PK;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			AssertNull("should not be defaulted for export declaration", invoiceHeader1.SupplierAddress);
			AssertEquals(declaration.Importer, invoiceHeader1.Buyer);
			AssertEquals(declaration.Manufacturer, invoiceHeader1.Manufacturer);
			AssertEquals(declaration.ManufacturerAddress, invoiceHeader1.ManufacturerAddress);

			declaration.Invoices.DeleteAll();
			var newcustomsSupplier = GetOrganisation();
			var customsAddress = newcustomsSupplier.Addresses.AddNew();
			customsAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			declaration.JE_OH_Supplier = newcustomsSupplier.PK;
			AssertEquals("Precondition", KRJobMessageTypeList.Codes.Export, declaration.JE_MessageType);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			AssertNull("should not be defaulted for export declaration", invoiceHeader2.SupplierAddress);
			AssertEquals(declaration.Importer, invoiceHeader2.Buyer);
			AssertEquals(declaration.Manufacturer, invoiceHeader2.Manufacturer);
			AssertEquals(declaration.ManufacturerAddress, invoiceHeader2.ManufacturerAddress);

			var newImporter = GetOrganisation();
			invoiceHeader2.JZ_OH_Buyer = newImporter.PK;

			var newManufacturer = GetOrganisation();
			invoiceHeader2.JZ_OH_Manufacturer = newManufacturer.PK;
			invoiceHeader2.JZ_OA_ManufacturerAddress = newManufacturer.MainAddress.PK;

			var invoiceHeader3 = declaration.Invoices.AddNew();
			AssertEquals(invoiceHeader2.Buyer, invoiceHeader3.Buyer);
			AssertEquals(invoiceHeader2.Manufacturer, invoiceHeader3.Manufacturer);
			AssertEquals(invoiceHeader2.ManufacturerAddress, invoiceHeader3.ManufacturerAddress);

			invoiceHeader3.JZ_OH_Supplier = CargoWise.Types.ZGuid.Empty;
			invoiceHeader3.JZ_OA_SupplierAddress = CargoWise.Types.ZGuid.Empty;
			invoiceHeader3.JZ_OH_Manufacturer = CargoWise.Types.ZGuid.Empty;
			invoiceHeader3.JZ_OA_ManufacturerAddress = CargoWise.Types.ZGuid.Empty;
			invoiceHeader3.JZ_OH_Buyer = CargoWise.Types.ZGuid.Empty;

			declaration.JE_OH_Supplier = GetOrganisation().PK;
			declaration.JE_OH_Importer = GetOrganisation().PK;
			var newDeclarationManufacturer = GetOrganisation();
			declaration.JE_OH_Manufacturer = newDeclarationManufacturer.PK;
			declaration.JE_OA_ManufacturerAddress = newDeclarationManufacturer.MainAddress.PK;

			AssertEquals(declaration.Importer, invoiceHeader3.Buyer);
			AssertEquals(declaration.Manufacturer, invoiceHeader3.Manufacturer);
			AssertEquals(declaration.ManufacturerAddress, invoiceHeader3.ManufacturerAddress);
		}

		public void TestDefaultForNewElementCoreForImport()
		{
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var supplier2 = GetOrganisation();
			declaration2.JE_OH_Supplier = supplier2.PK;
			AssertEquals("Precondition", KRJobMessageTypeList.Codes.Import, declaration2.JE_MessageType);

			var invoiceHeader1_2 = declaration2.Invoices.AddNew();
			AssertEquals(declaration2.Supplier, invoiceHeader1_2.Supplier);
		}

		public void TestDefaultForNewElementCoreForLocalExport()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var supplier1 = GetOrganisation();
			declaration1.JE_OH_Supplier = supplier1.PK;
			AssertEquals("Precondition", KRJobMessageTypeList.Codes.LocalExport, declaration1.JE_MessageType);

			var invoiceHeader1_1 = declaration1.Invoices.AddNew();
			AssertNull(invoiceHeader1_1.Supplier);
			AssertNull(invoiceHeader1_1.SupplierAddress);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var supplier2 = GetOrganisation();
			declaration2.JE_OH_Supplier = supplier2.PK;

			var importer = GetOrganisation();
			declaration2.JE_OH_Importer = importer.PK;

			var invoiceHeader1_2 = declaration2.Invoices.AddNew();
			AssertEquals(declaration2.Supplier, invoiceHeader1_2.Supplier);
			AssertNull(invoiceHeader1_2.Buyer);
		}

		OrgHeader GetOrganisation() => Factory.NewWithValidTestData<OrgHeader>();
	}
}
