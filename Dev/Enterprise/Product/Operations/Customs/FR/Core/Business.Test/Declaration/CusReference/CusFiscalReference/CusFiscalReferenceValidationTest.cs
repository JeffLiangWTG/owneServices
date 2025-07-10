
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_CodeRuleNat_030()
		{
			var messageError = "[NAT_030] Fiscal reference FR5 should only be used for concession F48.";
			var messageError2 = "[NAT_030] There should be only one fiscal reference FR5 used for concession F48.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.H1;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			using (var context = new EntryInstructionValidationDeciderTestContext(entryInstruction, true))
			{
				context.EnableRule(x => x.IsRuleNAT_030Active);
				cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
				AssertHasMessageError("Fiscal reference FR5 should only be used for concession F48.", cusFiscalReference.CFR_CodeInfo, messageError);
				AssertNoMessageError("Only one fiscal reference FR5.", cusFiscalReference.CFR_CodeInfo, messageError2);

				context.DisableRule(x => x.IsRuleNAT_030Active);
				cusFiscalReference.Validation.ValidateCFR_Code();
				AssertNoMessageError("Rule NAT_030Active disabled. No message error should show.", cusFiscalReference.CFR_CodeInfo, messageError);

				context.EnableRule(x => x.IsRuleNAT_030Active);
				invoiceLine.JI_Procedure = "1234F48";
				cusFiscalReference.Validation.ValidateCFR_Code();
				AssertNoMessageError("Fiscal reference FR5 used for concession F48.", cusFiscalReference.CFR_CodeInfo, messageError);
				AssertNoMessageError("Only one fiscal reference FR5 used for concession F48.", cusFiscalReference.CFR_CodeInfo, messageError2);

				invoiceLine.JI_Procedure = "1234F49";
				cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
				cusFiscalReference.Validation.ValidateCFR_Code();
				AssertNoMessageError("Fiscal reference not F5 used for any other concessions.", cusFiscalReference.CFR_CodeInfo, messageError);
				AssertNoMessageError("No fiscal reference FR5 used for other concession.", cusFiscalReference.CFR_CodeInfo, messageError2);
			}

			var cusFiscalReference2 = invoiceLine.FiscalReferences.AddNew();
			using (var context2 = new ImportInvoiceLineValidationTestContext(invoiceLine))
			{
				context2.EnableRule(x => x.IsRuleNAT_030Active);
				cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
				AssertHasMessageError("Fiscal reference FR5 should only be used for concession F48.", cusFiscalReference2.CFR_CodeInfo, messageError);
				AssertNoMessageError("Only one fiscal reference FR5.", cusFiscalReference.CFR_CodeInfo, messageError2);

				context2.DisableRule(x => x.IsRuleNAT_030Active);
				cusFiscalReference2.Validation.ValidateCFR_Code();
				AssertNoMessageError("Rule NAT_030Active disabled. No message error should show.", cusFiscalReference2.CFR_CodeInfo, messageError);

				context2.EnableRule(x => x.IsRuleNAT_030Active);
				invoiceLine.JI_Procedure = "1234F48";
				cusFiscalReference2.Validation.ValidateCFR_Code();
				AssertNoMessageError("Fiscal reference FR5 used for concession F48.", cusFiscalReference2.CFR_CodeInfo, messageError);
				AssertNoMessageError("Only one fiscal reference FR5 used for concession F48.", cusFiscalReference.CFR_CodeInfo, messageError2);

				invoiceLine.JI_Procedure = "1234F49";
				cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
				cusFiscalReference2.Validation.ValidateCFR_Code();
				AssertNoMessageError("Fiscal reference not F5 used for any other concessions.", cusFiscalReference2.CFR_CodeInfo, messageError);
				AssertNoMessageError("No fiscal reference FR5 used for other concession.", cusFiscalReference.CFR_CodeInfo, messageError2);

				invoiceLine.JI_Procedure = "1234F48";
				cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
				cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
				AssertHasMessageError("There should be only one fiscal reference FR5 used for concession F48.", cusFiscalReference2.CFR_CodeInfo, messageError2);

				context2.DisableRule(x => x.IsRuleNAT_030Active);
				cusFiscalReference2.Validation.ValidateCFR_Code();
				AssertNoMessageError("Rule NAT_030Active disabled. No message error should show.", cusFiscalReference2.CFR_CodeInfo, messageError2);
			}
		}

		public void TestCheckCFR_CodeRuleNat_031()
		{
			var messageError = "It is not possible to provide fiscal references other than FR5 for concession F48.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			AssertNoMessageError("No other fiscal reference than FR5", cusFiscalReference.CFR_CodeInfo, messageError);

			invoiceLine.JI_Procedure = "1234F48";
			cusFiscalReference.Validation.ValidateCFR_Code();
			AssertNoMessageError("No other fiscal reference than FR5 for concession F48", cusFiscalReference.CFR_CodeInfo, messageError);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			cusFiscalReference.Validation.ValidateCFR_Code();
			AssertHasMessageError("It is not possible to provide fiscal references other than FR5 for concession F48.", cusFiscalReference.CFR_CodeInfo, messageError);

			var cusFiscalReference2 = invoiceLine.FiscalReferences.AddNew();
			cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			AssertNoMessageError("No other fiscal reference than FR5", cusFiscalReference2.CFR_CodeInfo, messageError);

			invoiceLine.JI_Procedure = "1234F48";
			cusFiscalReference2.Validation.ValidateCFR_Code();
			AssertNoMessageError("No other fiscal reference than FR5 for concession F48", cusFiscalReference2.CFR_CodeInfo, messageError);

			cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			cusFiscalReference2.Validation.ValidateCFR_Code();
			AssertHasMessageError("It is not possible to provide fiscal references other than FR5 for concession F48.", cusFiscalReference2.CFR_CodeInfo, messageError);
		}
	}
}
