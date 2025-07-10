using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportJobComInvoiceLineConsignorValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new Ucc6ExportJobComInvoiceLineConsignorValidator(null));
	}

	public void TestCheckJI_OA_ExporterAddress()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertCustomsAddressValidation(period: "Not Transition period");

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertCustomsAddressValidation(period: "Transition period");
			}
		}
	}

	void AssertCustomsAddressValidation(string period)
	{
		declaration.JE_OH_Supplier = ZGuid.Empty;
		declaration.JE_MessageType = "EXP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		invoiceLine.JI_CEI = entryInstruction.PK;

		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		var propertyInfo = invoiceLine.JI_OA_ExporterAddressInfo;
		invoiceLine.JI_OA_ExporterAddress = consignor.MainAddress.PK;

		var mainAddress = consignor.MainAddress;

		var expectedAddressWarningMessage = $"Consignor Address is longer than 70 characters, it will be truncated in the message.";
		mainAddress.OA_Address1 = "".PadRight(35, 'A');
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertNoWarningContaining($"{period} -", propertyInfo, expectedAddressWarningMessage);

		mainAddress.OA_Address2 = "".PadRight(36, 'A');
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertHasWarningContaining($"{period} -", propertyInfo, expectedAddressWarningMessage);

		var expectedCityWarningMessage = $"Consignor City is longer than 35 characters, it will be truncated in the message.";
		mainAddress.OA_City = "Milan";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertNoWarningContaining($"{period} -", propertyInfo, expectedCityWarningMessage);

		mainAddress.OA_City = "Llanfairpwllgwyngyllgogerychwyrndrobwllllantysilio";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertHasWarningContaining($"{period} -", propertyInfo, expectedCityWarningMessage);

		consignor.OH_FullName = "".PadRight(75, 'A');
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		var expectedCompanyNameWarningMessageForImport = $"Consignor Company Name is longer than 70 characters, it will be truncated in the message.";
		AssertHasWarningContaining($"{period} -", propertyInfo, expectedCompanyNameWarningMessageForImport);

		consignor.OH_FullName = "SHORT";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertNoWarningContaining(propertyInfo, expectedCompanyNameWarningMessageForImport);
	}

	public void TestValidateConsignor_WhenSupplierAndConsignorsForSameEntryInstructionAreEmpty()
	{
		const string expectedMessageError = "Declaration -> Supplier or Invoice Line -> Organizations -> Consignor must be filled";
		declaration.JE_OH_Supplier = ZGuid.Empty;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		var consignor = Factory.New<OrgHeader>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("UCC6", () =>
			{
				invoiceLine2.JI_OA_ExporterAddress = consignor.MainAddress.PK;
				invoiceLine1.Validation.ValidateJI_OA_ExporterAddress();
				AssertHasMessageErrorContaining("Invoice Line 1 with no consignor", invoiceLine1.JI_OA_ExporterAddressInfo, expectedMessageError);

				var line3 = invoice.InvoiceLines.AddNew();
				line3.JI_CEI = entryInstruction1.PK;
				line3.Validation.ValidateJI_OA_ExporterAddress();
				AssertHasMessageErrorContaining("Invoice Line 3 with no consignor", line3.JI_OA_ExporterAddressInfo, expectedMessageError);

				invoiceLine2.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Invoice Line 2 with Consignor", invoiceLine2.JI_OA_ConsigneeAddressInfo, expectedMessageError);

				var line4 = invoice.InvoiceLines.AddNew();
				line4.JI_CEI = entryInstruction2.PK;
				line4.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Invoice Line 4 with no consignor however having another line with a consignor", line4.JI_OA_ConsigneeAddressInfo, expectedMessageError);

				declaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				invoiceLine1.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Supplier is specified and line with no consignor", invoiceLine1.JI_OA_ExporterAddressInfo, expectedMessageError);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("Non-UCC6", () =>
			{
				invoiceLine1.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Invoice Line 1 with no consignor", invoiceLine1.JI_OA_ExporterAddressInfo, expectedMessageError);
			});
		}
	}

	public void TestValidateConsignor_NoMessageError_WhenSupplierIsEmptyAndInvoiceLineConsignorIsFilled()
	{
		var factory = Factory;
		declaration.JE_OH_Supplier = ZGuid.Empty;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		var consignor = factory.New<OrgHeader>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoiceLine1.JI_OA_ExporterAddress = consignor.MainAddress.PK;
			invoiceLine1.Validation.ValidateJI_OA_ExporterAddress();
			AssertNoMessageErrors("Invoice Line with consignor", invoiceLine1.JI_OA_ExporterAddressInfo);
		}
	}

	public void TestValidateConsignor_WhenSupplierAndConsignorAreEmptyExceptOneLineForAnInstructionIsFilled()
	{
		const string expectedMessageError = "You have not entered a Consignor";
		declaration.JE_OH_Supplier = ZGuid.Empty;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		var consignor = Factory.New<OrgHeader>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("UCC6", () =>
			{
				invoiceLine1.JI_OA_ExporterAddress = consignor.MainAddress.PK;
				invoiceLine2.Validation.ValidateJI_OA_ExporterAddress();
				AssertHasMessageErrorContaining("Line having no Consignor and another line having consignor in the same group", invoiceLine2.JI_OA_ExporterAddressInfo, expectedMessageError);

				invoiceLine1.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Line 1 having Consignor", invoiceLine1.JI_OA_ExporterAddressInfo, expectedMessageError);

				invoiceLine2.JI_OA_ExporterAddress = consignor.MainAddress.PK;
				invoiceLine2.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Line 2 having Consignor", invoiceLine2.JI_OA_ExporterAddressInfo, expectedMessageError);

				invoiceLine2.JI_OA_ExporterAddress = ZGuid.Empty;
				declaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				invoiceLine2.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Supplier is specified and line with no consignor", invoiceLine2.JI_OA_ExporterAddressInfo, expectedMessageError);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("Non-UCC6", () =>
			{
				invoiceLine2.Validation.ValidateJI_OA_ExporterAddress();
				AssertNoMessageErrorContaining("Line having no Consignor and another line having consignor in the same group", invoiceLine2.JI_OA_ExporterAddressInfo, expectedMessageError);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
	}

	JobDeclaration declaration;
}
