using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportSupplierJobDocAddressValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new Ucc6ExportSupplierJobDocAddressValidation(Factory.New<JobDocAddress>(), null));
	}

	public void TestSupplierValidation_WhenEmpty()
	{
		const string expectedMessageError = "Declaration -> Supplier or Invoice Line -> Organizations -> Consignor must be filled";

		var supplierOrganizationPkInfo = declaration.SupplierDocumentaryAddress.OrganisationPKInfo;
		var consignor = Factory.New<OrgHeader>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("When all invoice lines do have an empty consignor", supplierOrganizationPkInfo, expectedMessageError);

				line1.JI_OA_ExporterAddress = consignor.MainAddress.PK;
				declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("When line1 has a consignor", supplierOrganizationPkInfo, expectedMessageError);

				line1.JI_OA_ExporterAddress = ZGuid.Empty;
				line3.JI_OA_ExporterAddress = consignor.MainAddress.PK;
				declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("When line3 has a consignor", supplierOrganizationPkInfo, expectedMessageError);
			});
		}
	}

	public void TestSupplierValidation_WhenNotEmpty()
	{
		const string expectedWarning = "The Supplier declared in the header will be ignored because all Invoice Line -> Consignor have values different from the Supplier";
		var supplier = Factory.New<OrgHeader>();
		var conignor1 = Factory.New<OrgHeader>();
		var consignor2 = Factory.New<OrgHeader>();

		var supplierOrganizationPkInfo = declaration.SupplierDocumentaryAddress.OrganisationPKInfo;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_OH_Supplier = supplier.PK;
			SetConsignorOnLines(conignor1, conignor1, consignor2);
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasWarningContaining("When Consignors are different on all lines than supplier", supplierOrganizationPkInfo, expectedWarning);

			SetConsignorOnLines(conignor1, null, consignor2);
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarningContaining("When Consignors are not specified on Line 2", supplierOrganizationPkInfo, expectedWarning);

			SetConsignorOnLines(conignor1, null, null);
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarningContaining("When Consignors are not specified on Line 2 and 3", supplierOrganizationPkInfo, expectedWarning);

			SetConsignorOnLines(supplier, supplier, supplier);
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarningContaining("When Consignors are specified on all lines and are same as that of Supplier", supplierOrganizationPkInfo, expectedWarning);
		}

		void SetConsignorOnLines(OrgHeader consignorForLine1, OrgHeader consignorForLine2, OrgHeader consignorForLine3)
		{
			line1.JI_OA_ExporterAddress = consignorForLine1?.MainAddress.PK ?? ZGuid.Empty;
			line2.JI_OA_ExporterAddress = consignorForLine2?.MainAddress.PK ?? ZGuid.Empty;
			line3.JI_OA_ExporterAddress = consignorForLine3?.MainAddress.PK ?? ZGuid.Empty;
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();

		line1 = invoice1.InvoiceLines.AddNew();
		line2 = invoice2.InvoiceLines.AddNew();
		line3 = invoice2.InvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceLine line1, line2, line3;
}
