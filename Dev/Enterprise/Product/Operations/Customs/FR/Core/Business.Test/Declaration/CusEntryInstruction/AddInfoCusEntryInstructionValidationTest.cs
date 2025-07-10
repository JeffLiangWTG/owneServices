using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class AddInfoCusEntryInstructionValidationTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionValidationTest
	{
		public void TestCheckZG_TransNature()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "12", "12 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "Delta IE", eun);
			var importCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "13", "13 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsImport", "Is For Import", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);
			importCode.Attributes.AddNew("IsImport", Core.Constants.BooleanTrueString);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
			ValidationTestHelper.AssertInvalidCodeMessageError(cusEntryInstruction.ZG_TransNatureInfo, "NO", "12");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = "IMP";
			ValidationTestHelper.AssertInvalidCodeMessageError(cusEntryInstruction.ZG_TransNatureInfo, "NO", "13");
		}

		public void TestCheckZG_TransNature_RuleC0627()
		{
			const string C0627ErrorMessageWhenSubStyleIsCOrF = "[C0627] Transaction Nature must be empty in case of Declaration Sub Type C or F.";
			const string C0627ErrorMessageWhenSubStyleIsNotCOrF = "[C0627] Transaction Nature is mandatory for this Declaration Sub Type.";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using var context = new EntryInstructionValidationDeciderTestContext(entryInstruction, true);
			context.EnableRule(x => x.IsRuleC0627Active);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
			entryInstruction.ZG_TransNature = "12";
			AssertHasMessageError("UCC6, Import, Sub Style in C/F, ZG_TransNature of entry instruction should be empty", entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

			context.DisableRule(x => x.IsRuleC0627Active);
			entryInstruction.ZG_TransNature = "12";
			AssertNoMessageError("Rule C0627 does not apply to EXP", entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

			context.EnableRule(x => x.IsRuleC0627Active);
			entryInstruction.ZG_TransNature = ZString.Empty;
			AssertNoMessageError(entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			invoiceLine1.ZG_TransNature = "12";
			entryInstruction.ZG_TransNature = ZString.Empty;
			AssertHasMessageError("UCC6, Import, Sub Style is not C/F, not all NT on GI have values, ZG_TransNature of entry instruction is mandatory", entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);

			context.DisableRule(x => x.IsRuleC0627Active);
			entryInstruction.ZG_TransNature = ZString.Empty;
			AssertNoMessageError("Rule C0627 does not apply to EXP", entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);

			context.EnableRule(x => x.IsRuleC0627Active);
			invoiceLine2.ZG_TransNature = "34";
			entryInstruction.ZG_TransNature = ZString.Empty;
			AssertNoMessageError("All NT on GI have values", entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);
			invoiceLine2.ZG_TransNature = ZString.Empty;
			entryInstruction.ZG_TransNature = "56";
			AssertNoMessageError(entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);

			context.DisableRule(x => x.IsRuleC0627Active);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
			entryInstruction.ZG_TransNature = "12";
			AssertNoMessageError("Rule C0627 does not apply to non UCC6", entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			invoiceLine1.ZG_TransNature = "12";
			invoiceLine2.ZG_TransNature = ZString.Empty;
			entryInstruction.ZG_TransNature = ZString.Empty;
			AssertNoMessageError("Rule C0627 does not apply to non UCC6", entryInstruction.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);
		}

		public void TestCheckZG_BypassReason()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			AssertNoMessageError("ZG_BypassReason can be empty when no ZG_BypassCode is specified", cusEntryInstruction.ZG_BypassReasonInfo, "Valuation bypass reason is required when valuation bypass code is 'I'");

			cusEntryInstruction.ZG_BypassCode = FRConstants.ValuationBypassCodes.ReasonRequiredCode;
			cusEntryInstruction.AddInfoValidation.ValidateZG_BypassReason();
			AssertHasMessageError("ZG_BypassReason must be provided when ZG_BypassCode is 'I'", cusEntryInstruction.ZG_BypassReasonInfo, "Valuation bypass reason is required when valuation bypass code is 'I'");

			cusEntryInstruction.ZG_BypassReason = "1";
			AssertNoMessageError("ZG_BypassReason can be empty when ZG_BypassCode is different from 'I'", cusEntryInstruction.ZG_BypassReasonInfo, "Valuation bypass reason is required when valuation bypass code is 'I'");
		}

		public void TestCheckRuleR0012AndC0002()
		{
			CombineAssertions("Rules R001 and C002 should not apply to not UCC6 declarations.", () =>
			{
				AssertRuleR0012AndC0002(false, false, "", "", "", false, false);
				AssertRuleR0012AndC0002(false, false, "", "11", "", false, false);
				AssertRuleR0012AndC0002(false, false, "", "11", "12", false, false);
				AssertRuleR0012AndC0002(false, false, "10", "", "", false, false);
				AssertRuleR0012AndC0002(false, false, "10", "11", "", false, false);
				AssertRuleR0012AndC0002(false, false, "10", "11", "12", false, false);
				AssertRuleR0012AndC0002(false, true, "", "", "", false, false);
				AssertRuleR0012AndC0002(false, true, "", "11", "", false, false);
				AssertRuleR0012AndC0002(false, true, "", "11", "12", false, false);
				AssertRuleR0012AndC0002(false, true, "10", "", "", false, false);
				AssertRuleR0012AndC0002(false, true, "10", "11", "", false, false);
				AssertRuleR0012AndC0002(false, true, "10", "11", "12", false, false);
			});

			CombineAssertions("Rules R001 and C002 should not apply to UCC6 Export declarations.", () =>
			{
				AssertRuleR0012AndC0002(true, false, "", "", "", false, false);
				AssertRuleR0012AndC0002(true, false, "", "11", "", false, false);
				AssertRuleR0012AndC0002(true, false, "", "11", "12", false, false);
				AssertRuleR0012AndC0002(true, false, "10", "", "", false, false);
				AssertRuleR0012AndC0002(true, false, "10", "11", "", false, false);
				AssertRuleR0012AndC0002(true, false, "10", "11", "12", false, false);
			});

			CombineAssertions("Rules R001 and C002 should apply to UCC6 Import declarations.", () =>
			{
				AssertRuleR0012AndC0002(true, true, "", "", "", false, false);
				AssertRuleR0012AndC0002(true, true, "", "11", "", true, false);
				AssertRuleR0012AndC0002(true, true, "", "11", "12", false, false);
				AssertRuleR0012AndC0002(true, true, "10", "", "", false, false);
				AssertRuleR0012AndC0002(true, true, "10", "11", "", false, true);
				AssertRuleR0012AndC0002(true, true, "10", "11", "12", false, false);
			});
		}

		void AssertRuleR0012AndC0002(bool isUCC6, bool isImport, string instructionZG_TransNature, string invoiceLine1ZG_TransNature, string invoiceLine2ZG_TransNature, bool shouldHaveMessageError, bool shouldHaveWarning)
		{
			var expectedMessageError = "[R0012 & C0002] If Nature Of Transaction is not entered on all Invoice Lines, it must be entered in Entry Instruction.";
			var expectedWarning = "Invoice Lines Nature of Transaction will be mapped from the Entry Instruction value.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = isImport ? Common.EU.EUJobMessageTypeList.Codes.Import : Common.EU.EUJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = isUCC6 ? DeclarationApplicationCodeList.Codes.DeltaIE : DeclarationApplicationCodeList.Codes.DeltaG;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cusEntryInstruction.PK;
			invoiceLine1.ZG_TransNature = invoiceLine1ZG_TransNature;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cusEntryInstruction.PK;
			invoiceLine2.ZG_TransNature = invoiceLine2ZG_TransNature;

			cusEntryInstruction.ZG_TransNature = instructionZG_TransNature;

			if (shouldHaveMessageError)
			{
				AssertHasMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedMessageError);
			}
			else
			{
				AssertNoMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedMessageError);
			}
			if (shouldHaveWarning)
			{
				AssertHasWarning(cusEntryInstruction.ZG_TransNatureInfo, expectedWarning);
			}
			else
			{
				AssertNoWarning(cusEntryInstruction.ZG_TransNatureInfo, expectedWarning);
			}
		}
	}
}
