using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceHeaderValidation))]
sealed class JobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest<JobComInvoiceHeaderValidation>
{
	public void TestIsJZ_ValuationCodeMandatory() => CombineAssertions(() =>
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		invoiceHeader.JZ_JE = jobDeclaration.PK;
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var isJZ_ValuationCodeMandatoryProperty = typeof(JobComInvoiceHeaderValidation).GetProperty("IsJZ_ValuationCodeMandatory", BindingFlags.NonPublic | BindingFlags.Instance);

		AssertEquals("Not set on any Invoice Lines but all Invoice Lines have null entry instruction", false, isJZ_ValuationCodeMandatoryProperty.GetValue(invoiceHeader.Validation));

		var cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = cusEntryInstruction.PK;

		AssertJZ_ValuationCodeMandatoryForAllTypes(cusEntryInstruction, isJZ_ValuationCodeMandatoryProperty, invoiceHeader);

		cusEntryInstruction.ZG_TransNature = ZString.Empty;
		invoiceLine.ZG_TransNature = "6";
		AssertEquals("Set on Invoice Line", false, isJZ_ValuationCodeMandatoryProperty.GetValue(invoiceHeader.Validation));
	});

	void AssertJZ_ValuationCodeMandatoryForAllTypes(CusEntryInstruction cusEntryInstruction, PropertyInfo isJZ_ValuationCodeMandatoryProperty, JobComInvoiceHeader invoiceHeader)
	{
		cusEntryInstruction.CEI_Style = "B4";
		AssertEquals("Not set on any Invoice Lines but Declaration type is not one of the valid types", false, isJZ_ValuationCodeMandatoryProperty.GetValue(invoiceHeader.Validation));

		var validDeclarationTypesForMessage = new ZString[] { DeclarationTypeList.Codes.B1, DeclarationTypeList.Codes.B2, DeclarationTypeList.Codes.C1, DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H3, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5, DeclarationTypeList.Codes.I1 };
		foreach(var style in validDeclarationTypesForMessage)
		{
			cusEntryInstruction.ZG_TransNature = ZString.Empty;
			cusEntryInstruction.CEI_Style = style;
			AssertEquals("Not set on any Invoice Lines with Declaration type as one of the valid types and empty transaction nature for the entry instruction", true, isJZ_ValuationCodeMandatoryProperty.GetValue(invoiceHeader.Validation));

			cusEntryInstruction.ZG_TransNature = "72";
			AssertEquals("Not set on any Invoice Lines with Declaration type as one of the valid types but non empty transaction nature for the entry instruction", false, isJZ_ValuationCodeMandatoryProperty.GetValue(invoiceHeader.Validation));
		}
	}

	public void TestCheckJZ_ValuationCode_RuleC0627()
	{
		const string C0627ErrorMessageWhenSubStyleIsNotCOrF = "[C0627] This field is mandatory for this declaration Sub Type.";

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			entryInstruction1.CEI_Style = "L1";
			invoice.JZ_ValuationCode = ZString.Empty;
			AssertNoMessageError("IsRuleC0627Active is true, Sub Style not C/F, but IsJZ_ValuationCodeMandatory is false so doesn't apply", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction1.CEI_Style = "B1";
			entryInstruction1.CEI_SubStyle = "A";
			invoiceLine1.ZG_TransNature = ZString.Empty;
			entryInstruction1.ZG_TransNature = ZString.Empty;
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertHasMessageError("IsRuleC0627Active is true, Sub Style not in C/F and IsJZ_ValuationCodeMandatory is true, JZ_ValuationCode of invoice header is mandatory", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);

			invoiceLine1.ZG_TransNature = "12";
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageError("UCC6, Import, Sub Style is not C/F, but IsJZ_ValuationCodeMandatory is false now, the rule doesn't apply and JZ_ValuationCode of invoice header is not mandatory", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);
		}
	}

	public void TestCheckJZ_ValuationCode_MandatoryValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_ValuationCode = "";
		AssertNoMessageErrorContaining("No InvoiceLine yet", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoice.JZ_ValuationCode = ZString.Empty;
		AssertNoMessageErrorContaining("No declaration type on entry instruction", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction1.CEI_Style = "H2";
		invoice.Validation.ValidateJZ_ValuationCode();
		AssertNoMessageErrorContaining("Declaration type on entry instruction not applicable for mandatory Transaction Nature", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction1.CEI_Style = "H1";
		entryInstruction1.ZG_TransNature = ZString.Empty;
		invoiceLine1.ZG_TransNature = ZString.Empty;
		invoice.JZ_ValuationCode = ZString.Empty;
		invoice.Validation.ValidateJZ_ValuationCode();
		AssertHasMessageErrorContaining("Declaration type on entry instruction applicable for mandatory Transaction Nature", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine1.ZG_TransNature = "X";
		invoice.Validation.ValidateJZ_ValuationCode();
		AssertNoMessageErrorContaining("Transaction Nature filled on Invoice Line", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

	protected override JobComInvoiceHeaderValidation GetValidation() => new JobComInvoiceHeaderValidation(invoice);
}
