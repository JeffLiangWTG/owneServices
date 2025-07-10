using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportImporterJobDocAddressValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var jobDocAddress = Factory.New<JobDocAddress>();
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new Ucc6ExportImporterJobDocAddressValidation(jobDocAddress, null));
	}

	public void TestImporterValidation_WhenLineLevelIsFilledAndDifferent()
	{
		const string expectedWarning = "The Importer declared in the header will be ignored because all lines have values, which different from the header one.";
		var consignee1 = Factory.New<OrgHeader>();
		var consignee2 = Factory.New<OrgHeader>();
		var consignee3 = Factory.New<OrgHeader>();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				declaration.ImporterDocumentaryAddress.OrganisationPK = consignee1.PK;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When Declaration has no Invoice Lines", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedWarning);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When all Invoice Line Consignees are empty", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedWarning);

				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				AssertNoWarningContaining("When Header Importer is empty", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedWarning);

				invoiceLine1.JI_OA_ConsigneeAddress = consignee2.MainAddress.PK;
				invoiceLine2.JI_OA_ConsigneeAddress = consignee3.MainAddress.PK;
				declaration.ImporterDocumentaryAddress.OrganisationPK = consignee1.PK;
				AssertHasWarningContaining("When all invoice lines Consignees are filled with different consignee", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedWarning);

				invoiceLine2.JI_OA_ConsigneeAddress = consignee2.MainAddress.PK;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When all invoice lines Consignees are filled with same consignee", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedWarning);

				invoiceLine2.JI_OA_ConsigneeAddress = ZGuid.Empty;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When at least invoice lines Consignee is empty", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedWarning);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];

			invoiceLine1.JI_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			invoiceLine2.JI_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = consignee1.PK;
			AssertNoWarningContaining("When all invoice lines Consignees are filled but is not UCC6", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedWarning);
		}
	}

	public void TestImporterValidation_WhenIsEmptyAtBothHeaderAndLineLevel()
	{
		const string expectedMessageError = "Importer must be declared at header or line level.";
		var consignee = Factory.New<OrgHeader>();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("When Declaration has no Invoice Lines", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessageError);

				var invoiceLine = declaration.InvoiceLines.AddNew();

				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("When both Header and Invoice Line Consignee is empty", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessageError);

				invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("When Invoice Line consignee is filled", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessageError);

				invoiceLine.JI_OA_ConsigneeAddress = ZGuid.Empty;
				declaration.ImporterDocumentaryAddress.OrganisationPK = consignee.PK;
				AssertNoMessageErrorContaining("When Header consignee is filled", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessageError);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_OA_ConsigneeAddress = ZGuid.Empty;
			declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageErrorContaining("When both Header and Invoice Line Consignee is empty but is not UCC6", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessageError);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
	}

	JobDeclaration declaration;

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);
}
