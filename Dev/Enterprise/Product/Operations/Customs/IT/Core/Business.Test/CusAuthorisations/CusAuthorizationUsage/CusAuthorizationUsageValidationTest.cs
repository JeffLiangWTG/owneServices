using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusAuthorizationUsageValidation))]
sealed class CusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAGC_OH_Owner_MissingEori()
	{
		CombineAssertions(() =>
		{
			const string messageError = "EORI Code is required. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter one.";

			var organization = Factory.New<OrgHeader>();
			var authorizationUsage = Factory.New<CusAuthorizationUsage>();

			authorizationUsage.AGC_OH_Owner = ZGuid.Empty;
			AssertNoMessageErrorContaining("When organization is not set", authorizationUsage.AGC_OH_OwnerInfo, messageError);

			authorizationUsage.AGC_OH_Owner = organization.PK;
			AssertHasMessageErrorContaining("When organization has not an EORI code", authorizationUsage.AGC_OH_OwnerInfo, messageError);

			organization.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123");
			authorizationUsage.AGC_OH_Owner = organization.PK;
			AssertNoMessageErrorContaining("When organization has an EORI code", authorizationUsage.AGC_OH_OwnerInfo, messageError);
		});
	}

	public void TestCheckAGC_OH_Owner_ValidateRuleC0848()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var expectedErrorMessage = "[C0848] You have not entered an Owner";
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		var validation = authorizationUsage.Validation;

		declaration.JE_MessageType = "IMP";
		authorizationUsage.AGC_Code = "BOI";
		validation.ValidateAGC_OH_Owner();
		AssertNoMessageErrorContaining("With IMP declaration, even if no Owner has been provided, no error message is expected", authorizationUsage.AGC_OH_OwnerInfo, expectedErrorMessage);

		declaration.JE_MessageType = "EXP";
		validation.ValidateAGC_OH_Owner();
		AssertNoMessageErrorContaining("With non UCC6 declaration, even if no Owner has been provided, no error message is expected", authorizationUsage.AGC_OH_OwnerInfo, expectedErrorMessage);

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			authorizationUsage.AGC_Code = "XXX";
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("When AGC_Code is not BOI or BTI, Even if no Owner has been provided, no error message is expected", authorizationUsage.AGC_OH_OwnerInfo, expectedErrorMessage);

			authorizationUsage.AGC_Code = "BOI";
			validation.ValidateAGC_OH_Owner();
			AssertHasMessageErrorContaining("When no Owner has been provided, error message is expected", authorizationUsage.AGC_OH_OwnerInfo, expectedErrorMessage);

			authorizationUsage.AGC_Code = "BOI";
			var organization = Factory.New<OrgHeader>();
			authorizationUsage.AGC_OH_Owner = organization.PK;
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("When valid Owner has been provided, no error message is expected", authorizationUsage.AGC_OH_OwnerInfo, expectedErrorMessage);
		}
	}

	public void TestCheckCusAuthorizationUsagesAGC_OH_Owner_ValidateRuleG089UCC6_Export()
	{
		var (declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, validation) = GetNewDeclarationAuthorizationsAndValidation("IMP");
		var expectedErrorMessage = "[G0089] You have not entered an Owner";

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			(declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, validation) = GetNewDeclarationAuthorizationsAndValidation("EXP");
			invoiceLineAuthorization1.AGC_Code = "BOI";
			invoiceLineAuthorization2.AGC_Code = "XXX";
			entryInstructionAuthorization.AGC_OH_Owner = ZGuid.Empty;
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("With unmerged declaration, even if AGC_Code is BOI or BTI, Even if no Owner has been provided, no error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

			declaration.DoMerge();
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("When no Owner has been provided, no error message is expected if one invoicel lineauthorization has not BOI or BTI code", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

			invoiceLineAuthorization2.AGC_Code = "BOI";
			validation.ValidateAGC_OH_Owner();
			AssertHasMessageErrorContaining("When no Owner has been provided, error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

			var organization = Factory.New<OrgHeader>();
			entryInstructionAuthorization.AGC_OH_Owner = organization.PK;
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("When valid Owner has been provided, no error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

			(declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, validation) = GetNewDeclarationAuthorizationsAndValidation("EXP");
			invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge();
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("When even one invoice line has not an Authorization, no error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);
		}
	}

	public void TestCheckCusAuthorizationUsagesAGC_OH_Owner_ValidateRuleG089_NonUCC6Export()
	{
		(declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, validation) = GetNewDeclarationAuthorizationsAndValidation("IMP");
		var expectedErrorMessage = "[G0089] You have not entered an Owner";

		(declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, validation) = GetNewDeclarationAuthorizationsAndValidation("EXP");
		declaration.DoMerge();
		invoiceLineAuthorization1.AGC_Code = "BOI";
		invoiceLineAuthorization2.AGC_Code = "BTI";
		validation.ValidateAGC_OH_Owner();
		AssertNoMessageErrorContaining("With non UCC6 declaration, even if no Owner has been provided, no error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			(declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, validation) = GetNewDeclarationAuthorizationsAndValidation("EXP");
			invoiceLineAuthorization1.AGC_Code = "BOI";
			invoiceLineAuthorization2.AGC_Code = "XXX";
			entryInstructionAuthorization.AGC_OH_Owner = ZGuid.Empty;
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("With unmerged declaration, even if AGC_Code is BOI or BTI, Even if no Owner has been provided, no error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

			declaration.DoMerge();
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("When no Owner has been provided, no error message is expected if one invoicel lineauthorization has not BOI or BTI code", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

			invoiceLineAuthorization2.AGC_Code = "BOI";
			validation.ValidateAGC_OH_Owner();
			AssertHasMessageErrorContaining("When no Owner has been provided, error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);

			var organization = Factory.New<OrgHeader>();
			entryInstructionAuthorization.AGC_OH_Owner = organization.PK;
			validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("When valid Owner has been provided, no error message is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo, expectedErrorMessage);
		}
	}

	public void TestCheckCusAuthorizationUsagesAGC_OH_Owner_ValidateRuleG089_Import()
	{
		var (declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, validation) = GetNewDeclarationAuthorizationsAndValidation("IMP");

		declaration.DoMerge();
		invoiceLineAuthorization1.AGC_Code = "BOI";
		invoiceLineAuthorization2.AGC_Code = "BTI";
		validation.ValidateAGC_OH_Owner();
		AssertNoMessageErrors("With IMP declaration, even if no Owner has been provided, no error message G0089 is expected", entryInstructionAuthorization.AGC_OH_OwnerInfo);
	}

	(JobDeclaration declaration, CusAuthorizationUsage invoiceLineAuthorization1, CusAuthorizationUsage invoiceLineAuthorization2, CusAuthorizationUsage entryInstructionAuthorization, CusAuthorizationUsageValidation validation) GetNewDeclarationAuthorizationsAndValidation(string messageType)
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MessageType = messageType;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionAuthorization = entryInstruction.CusAuthorizationUsages.AddNew();

		invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		invoiceLine1.CusAuthorizationUsages.RemoveAndDeleteAll();
		invoiceLine2.CusAuthorizationUsages.RemoveAndDeleteAll();
		invoiceLineAuthorization1 = invoiceLine1.CusAuthorizationUsages.AddNew();
		invoiceLineAuthorization2 = invoiceLine2.CusAuthorizationUsages.AddNew();

		return (declaration, invoiceLineAuthorization1, invoiceLineAuthorization2, entryInstructionAuthorization, entryInstructionAuthorization.Validation);
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	CusAuthorizationUsage invoiceLineAuthorization1;
	CusAuthorizationUsage invoiceLineAuthorization2;
	CusAuthorizationUsage entryInstructionAuthorization;
	CusAuthorizationUsageValidation validation;
}
