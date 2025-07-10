using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class DeclarationJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckDeclarationSupplierWithAnyInvoiceHeader()
		{
			var dec = GetDeclarationForTestingImport(GetDeclarationTypeForTestingImport.Import);

			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUP";
			supplier.OH_FullName = "SUPPLIER NAME";

			var supplier1 = OrgHeader.New(Factory);
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "SUPPLIER NAME 1";

			var warning = "This value will not be declared in Import Entries. Please, check the Suppliers in the Invoice Headers.";

			CombineAssertions(() =>
			{
				dec.RunPreSaveValidation();
				AssertNoWarning("No warning when JE_OH_Supplier is empty and not inv.header.", dec.SupplierDocumentaryAddress.OrganisationPKInfo, warning);

				dec.JE_OH_Supplier = supplier.PK;
				dec.RunPreSaveValidation();
				AssertNoWarningContaining("No warning when JE_OH_Supplier is not empty and not inv.header.", dec.SupplierDocumentaryAddress.OrganisationPKInfo, warning);

				var entryHeader = dec.Invoices.AddNew();
				entryHeader.JZ_OH_Supplier = supplier.PK;
				dec.RunPreSaveValidation();
				AssertNoWarningContaining("No warning when JE_OH_Supplier is not empty and one inv.header equals.", dec.SupplierDocumentaryAddress.OrganisationPKInfo, warning);

				entryHeader = dec.Invoices.AddNew();
				entryHeader.JZ_OH_Supplier = supplier.PK;
				dec.RunPreSaveValidation();
				AssertNoWarningContaining("No warning when JE_OH_Supplier is not empty and two inv.header equals.", dec.SupplierDocumentaryAddress.OrganisationPKInfo, warning);

				entryHeader = dec.Invoices.AddNew();
				entryHeader.JZ_OH_Supplier = supplier1.PK;
				dec.RunPreSaveValidation();
				AssertHasWarningContaining("Warning when JE_OH_Supplier is not empty and two inv.header equals and one different.", dec.SupplierDocumentaryAddress.OrganisationPKInfo, warning);
			});
		}

		public void TestCheckImporterMandatoryImport()
		{
			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTestingImport(GetDeclarationTypeForTestingImport.Import);
				var org = Factory.New<OrgHeader>();
				dec.JE_OH_Importer = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Importer has value for Import", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Importer = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Import", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				dec.JE_OH_Importer = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Importer has value for Import with more than 1 entry instruction", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Importer = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Import with more than 1 entry instruction", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckSupplierMandatory()
		{
			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTestingExport(GetDeclarationTypeForTestingExport.Export);
				var org = Factory.New<OrgHeader>();
				dec.JE_OH_Supplier = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Supplier has value for Export", dec.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Supplier = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Supplier has no value for Export", dec.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTestingExport(GetDeclarationTypeForTestingExport.T2LExpedition);
				org = Factory.New<OrgHeader>();
				dec.JE_OH_Supplier = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Supplier has value for T2LExpedition", dec.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Supplier = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Supplier has no value for T2LExpedition", dec.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTestingExport(GetDeclarationTypeForTestingExport.Export);
				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				org = Factory.New<OrgHeader>();
				dec.JE_OH_Supplier = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Supplier has value for Export with more than 1 entry instruction", dec.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Supplier = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Supplier has no value for Export with more than 1 entry instruction", dec.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckSupplierCPC71Or95()
		{
			const string messageWarning = "Supplier data will not be sent using CPC 71(H2) or 95 (H5)";
			var dec = GetDeclarationForTestingImport(GetDeclarationTypeForTestingImport.Import);

			CombineAssertions(() =>
			{
				var org = Factory.New<OrgHeader>();
				dec.JE_OH_Supplier = org.PK;
				dec.RunPreSaveValidation();
				dec.SupplierDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning without H2 and without CPC", dec.SupplierDocumentaryAddress.OrganisationPKInfo, messageWarning);

				var invoiceLine1 = dec.InvoiceLines.AddNew();
				invoiceLine1.JI_FormattedProcedure = "1000";
				dec.RunPreSaveValidation();
				dec.SupplierDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning without H2 and with unique CPC that start with value 10", dec.SupplierDocumentaryAddress.OrganisationPKInfo, messageWarning);

				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction2.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				dec.RunPreSaveValidation();
				dec.SupplierDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning with H2 and with unique CPC that start with value 10", dec.SupplierDocumentaryAddress.OrganisationPKInfo, messageWarning);

				var invoiceLine2 = dec.InvoiceLines.AddNew();
				invoiceLine2.JI_FormattedProcedure = "7100";
				dec.RunPreSaveValidation();
				dec.SupplierDocumentaryAddress.Validation.ValidateAll();
				AssertHasWarningContaining("Assert warning with H2 and with the second line of CPC that start with value 71", dec.SupplierDocumentaryAddress.OrganisationPKInfo, messageWarning);

				invoiceLine2.JI_FormattedProcedure = "1000";
				dec.RunPreSaveValidation();
				dec.SupplierDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning with H2 and with the second line of CPC that start with value 10", dec.SupplierDocumentaryAddress.OrganisationPKInfo, messageWarning);

				invoiceLine2.JI_FormattedProcedure = "9500";
				dec.RunPreSaveValidation();
				dec.SupplierDocumentaryAddress.Validation.ValidateAll();
				AssertHasWarningContaining("Assert warning with H2 and with the second line of CPC that start with value 95", dec.SupplierDocumentaryAddress.OrganisationPKInfo, messageWarning);
			});
		}

		public void TestCheckJE_OH_ImporterCPC76Or77()
		{
			const string messageWarning = "Importer data will not be sent using CPC 76 or 77 - B3 message";
			var dec = GetDeclarationForTestingImport(GetDeclarationTypeForTestingImport.Import);

			CombineAssertions(() =>
			{
				var org = Factory.New<OrgHeader>();
				dec.JE_OH_Importer = org.PK;
				dec.RunPreSaveValidation();
				dec.ImporterDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning without H2 and without CPC", dec.ImporterDocumentaryAddress.OrganisationPKInfo, messageWarning);

				var invoiceLine1 = dec.InvoiceLines.AddNew();
				invoiceLine1.JI_FormattedProcedure = "1000";
				dec.RunPreSaveValidation();
				dec.ImporterDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning without H2 and with unique CPC that start with value 10", dec.ImporterDocumentaryAddress.OrganisationPKInfo, messageWarning);

				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction2.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				dec.RunPreSaveValidation();
				dec.ImporterDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning with H2 and with unique CPC that start with value 10", dec.ImporterDocumentaryAddress.OrganisationPKInfo, messageWarning);

				var invoiceLine2 = dec.InvoiceLines.AddNew();
				invoiceLine2.JI_FormattedProcedure = "7600";
				dec.RunPreSaveValidation();
				dec.ImporterDocumentaryAddress.Validation.ValidateAll();
				AssertHasWarningContaining("Assert warning with H2 and with the second line of CPC that start with value 76", dec.ImporterDocumentaryAddress.OrganisationPKInfo, messageWarning);

				invoiceLine2.JI_FormattedProcedure = "1000";
				dec.RunPreSaveValidation();
				dec.ImporterDocumentaryAddress.Validation.ValidateAll();
				AssertNoWarningContaining("Assert no warning with H2 and with the second line of CPC that start with value 10", dec.ImporterDocumentaryAddress.OrganisationPKInfo, messageWarning);

				invoiceLine2.JI_FormattedProcedure = "7700";
				dec.RunPreSaveValidation();
				dec.ImporterDocumentaryAddress.Validation.ValidateAll();
				AssertHasWarningContaining("Assert warning with H2 and with the second line of CPC that start with value 77", dec.ImporterDocumentaryAddress.OrganisationPKInfo, messageWarning);
			});
		}

		public void TestCheckImporterMandatoryExport()
		{
			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTestingExport(GetDeclarationTypeForTestingExport.Export);
				var org = Factory.New<OrgHeader>();
				dec.JE_OH_Importer = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Importer has value for Export", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Importer = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Export", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTestingExport(GetDeclarationTypeForTestingExport.T2LExpedition);
				org = Factory.New<OrgHeader>();
				dec.JE_OH_Importer = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Importer has value for T2LExpedition", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Importer = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Importer has no value for T2LExpedition", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec = GetDeclarationForTestingExport(GetDeclarationTypeForTestingExport.Export);
				var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				dec.JE_OH_Importer = org.PK;
				dec.RunPreSaveValidation();
				AssertNoMessageErrorContaining("Assert mandatory Importer has value for Export with more than 1 entry instruction", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				dec.JE_OH_Importer = ZGuid.Empty;
				dec.RunPreSaveValidation();
				AssertHasMessageErrorContaining("Assert mandatory Importer has no value for Export with more than 1 entry instruction", dec.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		enum GetDeclarationTypeForTestingImport { Import, T2LReception, T2LClearance }

		JobDeclaration GetDeclarationForTestingImport(GetDeclarationTypeForTestingImport declarationType)
		{
			var dec = Factory.New<JobDeclaration>();
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			switch (declarationType)
			{
				case GetDeclarationTypeForTestingImport.Import:
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					break;
				case GetDeclarationTypeForTestingImport.T2LReception:
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
					break;
				case GetDeclarationTypeForTestingImport.T2LClearance:
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
					break;
				default:
					break;
			}
			return dec;
		}

		enum GetDeclarationTypeForTestingExport { Export, T2LExpedition, T2LClearance }

		JobDeclaration GetDeclarationForTestingExport(GetDeclarationTypeForTestingExport declarationType)
		{
			var dec = Factory.New<JobDeclaration>();
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			switch (declarationType)
			{
				case GetDeclarationTypeForTestingExport.Export:
					dec.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					break;
				case GetDeclarationTypeForTestingExport.T2LExpedition:
					dec.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
					break;
				case GetDeclarationTypeForTestingExport.T2LClearance:
					dec.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
					break;
				default:
					break;
			}
			return dec;
		}
	}
}
