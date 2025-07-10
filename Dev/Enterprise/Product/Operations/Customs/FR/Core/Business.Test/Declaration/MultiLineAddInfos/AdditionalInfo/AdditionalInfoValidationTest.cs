using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business.Testing;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_CodeRuleNAT_041()
		{
			const string expectedErrorMessage = "[NAT_041] Both additional information G6090 and G0008 cannot be present in the same declaration.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			using (var ctx = new AdditionalInfoValidationDeciderTestContext(declaration, true))
			{
				ctx.EnableRule(x => x.IsRuleNAT_041Active);

				var infoA1 = declaration.AdditionalInfos.AddNew();
				infoA1.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB1 = declaration.AdditionalInfos.AddNew();
				infoB1.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
				infoA1.Validation.ValidateCSI_Code();

				AssertHasMessageError("[G6090] should trigger validation error when [G0008] is present in DECLARATION and defer type=2", infoA1.CSI_CodeInfo, expectedErrorMessage);
				AssertHasMessageError("[G0008] should trigger validation error when [G6090] is present in DECLARATION and defer type=2", infoB1.CSI_CodeInfo, expectedErrorMessage);

				ctx.DisableRule(x => x.IsRuleNAT_041Active);
				infoA1.Validation.ValidateCSI_Code();
				infoB1.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when rule disabled and defer type=2", infoA1.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when rule disabled and defer type=2", infoB1.CSI_CodeInfo, expectedErrorMessage);

				declaration.AdditionalInfos.RemoveAndDelete(infoA1);
				declaration.AdditionalInfos.RemoveAndDelete(infoB1);

				ctx.EnableRule(x => x.IsRuleNAT_041Active);

				var infoA2 = invoiceHeader.AdditionalInfos.AddNew();
				infoA2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB2 = invoiceHeader.AdditionalInfos.AddNew();
				infoB2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA2.Validation.ValidateCSI_Code();

				AssertHasMessageError("[G6090] should trigger validation error when [G0008] is present in HEADER and defer type=2", infoA2.CSI_CodeInfo, expectedErrorMessage);
				AssertHasMessageError("[G0008] should trigger validation error when [G6090] is present in HEADER and defer type=2", infoB2.CSI_CodeInfo, expectedErrorMessage);

				ctx.DisableRule(x => x.IsRuleNAT_041Active);
				infoA2.Validation.ValidateCSI_Code();
				infoB2.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when rule disabled and defer type=2", infoA2.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when rule disabled and defer type=2", infoB2.CSI_CodeInfo, expectedErrorMessage);

				invoiceHeader.AdditionalInfos.RemoveAndDelete(infoA2);
				invoiceHeader.AdditionalInfos.RemoveAndDelete(infoB2);

				ctx.EnableRule(x => x.IsRuleNAT_041Active);

				var infoA3 = invoiceLine.AdditionalInfos.AddNew();
				infoA3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB3 = invoiceLine.AdditionalInfos.AddNew();
				infoB3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA3.Validation.ValidateCSI_Code();

				AssertHasMessageError("[G6090] should trigger validation error when [G0008] is present in INVOICE LINE and defer type=2", infoA3.CSI_CodeInfo, expectedErrorMessage);
				AssertHasMessageError("[G0008] should trigger validation error when [G6090] is present in INVOICE LINE and defer type=2", infoB3.CSI_CodeInfo, expectedErrorMessage);

				ctx.DisableRule(x => x.IsRuleNAT_041Active);
				infoA3.Validation.ValidateCSI_Code();
				infoB3.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when rule disabled and defer type=2", infoA3.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when rule disabled and defer type=2", infoB3.CSI_CodeInfo, expectedErrorMessage);

				invoiceLine.AdditionalInfos.RemoveAndDelete(infoA3);
				invoiceLine.AdditionalInfos.RemoveAndDelete(infoB3);

				ctx.EnableRule(x => x.IsRuleNAT_041Active);

				var infoA4 = declaration.AdditionalInfos.AddNew();
				infoA4.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB4 = invoiceHeader.AdditionalInfos.AddNew();
				infoB4.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA4.Validation.ValidateCSI_Code();

				AssertHasMessageError("[G6090] should trigger validation error when [G0008] is present at DECLARATION + HEADER and defer type=2", infoA4.CSI_CodeInfo, expectedErrorMessage);
				AssertHasMessageError("[G0008] should trigger validation error when [G6090] is present at DECLARATION + HEADER and defer type=2", infoB4.CSI_CodeInfo, expectedErrorMessage);

				ctx.DisableRule(x => x.IsRuleNAT_041Active);
				infoA4.Validation.ValidateCSI_Code();
				infoB4.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when rule disabled and defer type=2", infoA4.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when rule disabled and defer type=2", infoB4.CSI_CodeInfo, expectedErrorMessage);

				invoiceHeader.AdditionalInfos.RemoveAndDelete(infoB4);
				declaration.AdditionalInfos.RemoveAndDelete(infoA4);

				ctx.EnableRule(x => x.IsRuleNAT_041Active);

				var infoA5 = declaration.AdditionalInfos.AddNew();
				infoA5.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB5 = invoiceLine.AdditionalInfos.AddNew();
				infoB5.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA5.Validation.ValidateCSI_Code();

				AssertHasMessageError("[G6090] should trigger validation error when [G0008] is present at DECLARATION + INVOICE LINE and defer type=2", infoA5.CSI_CodeInfo, expectedErrorMessage);
				AssertHasMessageError("[G0008] should trigger validation error when [G6090] is present at DECLARATION + INVOICE LINE and defer type=2", infoB5.CSI_CodeInfo, expectedErrorMessage);

				ctx.DisableRule(x => x.IsRuleNAT_041Active);
				infoA5.Validation.ValidateCSI_Code();
				infoB5.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when rule disabled and defer type=2", infoA5.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when rule disabled and defer type=2", infoB5.CSI_CodeInfo, expectedErrorMessage);

				invoiceLine.AdditionalInfos.RemoveAndDelete(infoB5);
				declaration.AdditionalInfos.RemoveAndDelete(infoA5);

				ctx.EnableRule(x => x.IsRuleNAT_041Active);

				var infoA6 = invoiceHeader.AdditionalInfos.AddNew();
				infoA6.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB6 = invoiceLine.AdditionalInfos.AddNew();
				infoB6.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA6.Validation.ValidateCSI_Code();

				AssertHasMessageError($"[G6090] should trigger validation error when [G0008] is present at HEADER + INVOICE LINE and defer type=2", infoA6.CSI_CodeInfo, expectedErrorMessage);
				AssertHasMessageError($"[G0008] should trigger validation error when [G6090] is present at HEADER + INVOICE LINE and defer type=2", infoB6.CSI_CodeInfo, expectedErrorMessage);

				ctx.DisableRule(x => x.IsRuleNAT_041Active);
				infoA6.Validation.ValidateCSI_Code();
				infoB6.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when rule disabled and defer type=2", infoA6.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when rule disabled and defer type=2", infoB6.CSI_CodeInfo, expectedErrorMessage);

				invoiceLine.AdditionalInfos.RemoveAndDelete(infoB6);
				invoiceHeader.AdditionalInfos.RemoveAndDelete(infoA6);
			}

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			using (var ctx = new AdditionalInfoValidationDeciderTestContext(declaration, true))
			{
				ctx.EnableRule(x => x.IsRuleNAT_041Active);

				var infoA1 = declaration.AdditionalInfos.AddNew();
				infoA1.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB1 = invoiceHeader.AdditionalInfos.AddNew();
				infoB1.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA1.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when [G0008] is present at DECLARATION + HEADER and defer type!=2", infoA1.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when [G6090] is present at DECLARATION + HEADER and defer type!=2", infoB1.CSI_CodeInfo, expectedErrorMessage);

				invoiceHeader.AdditionalInfos.RemoveAndDelete(infoB1);
				declaration.AdditionalInfos.RemoveAndDelete(infoA1);

				var infoA2 = declaration.AdditionalInfos.AddNew();
				infoA2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB2 = invoiceLine.AdditionalInfos.AddNew();
				infoB2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA2.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when [G0008] is present at DECLARATION + INVOICE LINE and defer type!=2", infoA2.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when [G6090] is present at DECLARATION + INVOICE LINE and defer type!=2", infoB2.CSI_CodeInfo, expectedErrorMessage);

				invoiceLine.AdditionalInfos.RemoveAndDelete(infoB2);
				declaration.AdditionalInfos.RemoveAndDelete(infoA2);

				var infoA3 = invoiceHeader.AdditionalInfos.AddNew();
				infoA3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption;
				var infoB3 = invoiceLine.AdditionalInfos.AddNew();
				infoB3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

				infoA3.Validation.ValidateCSI_Code();

				AssertNoMessageError("[G6090] should not trigger validation error when [G0008] is present at HEADER + INVOICE LINE and defer type!=2", infoA3.CSI_CodeInfo, expectedErrorMessage);
				AssertNoMessageError("[G0008] should not trigger validation error when [G6090] is present at HEADER + INVOICE LINE and defer type!=2", infoB3.CSI_CodeInfo, expectedErrorMessage);

				invoiceLine.AdditionalInfos.RemoveAndDelete(infoB3);
				invoiceHeader.AdditionalInfos.RemoveAndDelete(infoA3);
			}
		}

		public void TestCheckCSI_CodeRuleC0834_N01()
		{
			const string messageError = "[C0834_N01] You have requested the concession F48, you cannot enter the special mention \"G0008 unidentified VAT person in France\".";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "1234F48";
			var additionalInfo1 = declaration.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;

			using (var context = new AdditionalInfoValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(x => x.IsRuleC0834_N01Active);
				additionalInfo1.Validation.ValidateCSI_Code();
				AssertHasMessageError(additionalInfo1.CSI_CodeInfo, messageError);

				context.DisableRule(x => x.IsRuleC0834_N01Active);
				additionalInfo1.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo1.CSI_CodeInfo, messageError);

				context.EnableRule(x => x.IsRuleC0834_N01Active);
				invoiceLine.JI_Procedure = "1234F49";
				additionalInfo1.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo1.CSI_CodeInfo, messageError);

				invoiceLine.JI_Procedure = "1234F48";
				additionalInfo1.CSI_Code = ZString.Empty;
				additionalInfo1.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo1.CSI_CodeInfo, messageError);

				var additionalInfo2 = invoiceHeader.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
				additionalInfo2.Validation.ValidateCSI_Code();
				AssertHasMessageError(additionalInfo2.CSI_CodeInfo, messageError);

				context.DisableRule(x => x.IsRuleC0834_N01Active);
				additionalInfo2.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo2.CSI_CodeInfo, messageError);

				context.EnableRule(x => x.IsRuleC0834_N01Active);
				invoiceLine.JI_Procedure = "1234F49";
				additionalInfo2.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo2.CSI_CodeInfo, messageError);

				invoiceLine.JI_Procedure = "1234F48";
				additionalInfo2.CSI_Code = ZString.Empty;
				additionalInfo2.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo2.CSI_CodeInfo, messageError);

				var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
				additionalInfo3.Validation.ValidateCSI_Code();
				AssertHasMessageError(additionalInfo3.CSI_CodeInfo, messageError);

				context.DisableRule(x => x.IsRuleC0834_N01Active);
				additionalInfo3.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo3.CSI_CodeInfo, messageError);

				context.EnableRule(x => x.IsRuleC0834_N01Active);
				invoiceLine.JI_Procedure = "1234F49";
				additionalInfo3.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo3.CSI_CodeInfo, messageError);

				invoiceLine.JI_Procedure = "1234F48";
				additionalInfo3.CSI_Code = ZString.Empty;
				additionalInfo3.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo3.CSI_CodeInfo, messageError);

				var additionalInfo4 = entryInstruction.AdditionalInfos.AddNew();
				additionalInfo4.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
				additionalInfo4.Validation.ValidateCSI_Code();
				AssertHasMessageError(additionalInfo4.CSI_CodeInfo, messageError);

				context.DisableRule(x => x.IsRuleC0834_N01Active);
				additionalInfo4.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo4.CSI_CodeInfo, messageError);

				context.EnableRule(x => x.IsRuleC0834_N01Active);
				invoiceLine.JI_Procedure = "1234F49";
				additionalInfo4.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo4.CSI_CodeInfo, messageError);

				invoiceLine.JI_Procedure = "1234F48";
				additionalInfo4.CSI_Code = ZString.Empty;
				additionalInfo4.Validation.ValidateCSI_Code();
				AssertNoMessageError(additionalInfo4.CSI_CodeInfo, messageError);
			}
		}

		public void TestCheckRuleNAT_228()
		{
			var messageError = "[NAT_228] If code is K0001 description is mandatory.";

			var declaration = Factory.New<JobDeclaration>();
			var additionalInfoInDeclarationLevel = declaration.AdditionalInfos.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var additionalInfoInEntryInstructionLevel = entryInstruction.AdditionalInfos.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var additionalInfoInInvoiceHeaderLevel = invoiceHeader.AdditionalInfos.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var additionalInfoInInvoiceLineLevel = invoiceLine.AdditionalInfos.AddNew();

			using (var context = new AdditionalInfoValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(x => x.IsRuleNAT_228Active);
				CombineAssertions("When RuleNAT_228 is enabled:", () =>
				{
					additionalInfoInDeclarationLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInDeclarationLevel.CSI_Description = ZString.Empty;
					AssertHasMessageError("When K0001 is entered at declaration level and CSI_Description is empty, there should be message error.", additionalInfoInDeclarationLevel.CSI_DescriptionInfo, messageError);
					additionalInfoInDeclarationLevel.CSI_Description = "ABC";
					AssertNoMessageError("When CSI_Description is not empty, there should be no message error.", additionalInfoInDeclarationLevel.CSI_DescriptionInfo, messageError);

					additionalInfoInEntryInstructionLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInEntryInstructionLevel.CSI_Description = ZString.Empty;
					AssertHasMessageError("When K0001 is entered at entry instruction level and CSI_Description is empty, there should be message error.", additionalInfoInEntryInstructionLevel.CSI_DescriptionInfo, messageError);
					additionalInfoInEntryInstructionLevel.CSI_Description = "ABC";
					AssertNoMessageError("When CSI_Description is not empty, there should be no message error.", additionalInfoInEntryInstructionLevel.CSI_DescriptionInfo, messageError);

					additionalInfoInInvoiceHeaderLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInInvoiceHeaderLevel.CSI_Description = ZString.Empty;
					AssertHasMessageError("When K0001 is entered at invoiceHeader level and CSI_Description is empty, there should be message error.", additionalInfoInInvoiceHeaderLevel.CSI_DescriptionInfo, messageError);
					additionalInfoInInvoiceHeaderLevel.CSI_Description = "ABC";
					AssertNoMessageError("When CSI_Description is not empty, there should be no message error.", additionalInfoInInvoiceHeaderLevel.CSI_DescriptionInfo, messageError);

					additionalInfoInInvoiceLineLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInInvoiceLineLevel.CSI_Description = ZString.Empty;
					AssertHasMessageError("When K0001 is entered at invoiceLine level and CSI_Description is empty, there should be message error.", additionalInfoInInvoiceLineLevel.CSI_DescriptionInfo, messageError);
					additionalInfoInInvoiceLineLevel.CSI_Description = "ABC";
					AssertNoMessageError("When CSI_Description is not empty, there should be no message error.", additionalInfoInInvoiceLineLevel.CSI_DescriptionInfo, messageError);
				});

				context.DisableRule(x => x.IsRuleNAT_228Active);
				CombineAssertions("When RuleNAT_228 is disabled:", () =>
				{
					additionalInfoInDeclarationLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInDeclarationLevel.CSI_Description = ZString.Empty;
					AssertNoMessageError("When K0001 is entered at declaration level and CSI_Description is empty, there should be no message error.", additionalInfoInDeclarationLevel.CSI_DescriptionInfo, messageError);

					additionalInfoInEntryInstructionLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInEntryInstructionLevel.CSI_Description = ZString.Empty;
					AssertNoMessageError("When K0001 is entered at entry instruction level and CSI_Description is empty, there should be no message error.", additionalInfoInEntryInstructionLevel.CSI_DescriptionInfo, messageError);

					additionalInfoInInvoiceHeaderLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInInvoiceHeaderLevel.CSI_Description = ZString.Empty;
					AssertNoMessageError("When K0001 is entered at invoiceHeader level and CSI_Description is empty, there should be no message error.", additionalInfoInInvoiceHeaderLevel.CSI_DescriptionInfo, messageError);

					additionalInfoInInvoiceLineLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					additionalInfoInInvoiceLineLevel.CSI_Description = ZString.Empty;
					AssertNoMessageError("When K0001 is entered at invoiceLine level and CSI_Description is empty, there should be no message error.", additionalInfoInInvoiceLineLevel.CSI_DescriptionInfo, messageError);
				});
			}
		}

		public void TestCheckRuleNAT_088Bis()
		{
			const string messageError = "[NAT_088_Bis] E0001 can be entered only at declaration level, in Misc. tab.";

			var declaration = Factory.New<JobDeclaration>();
			var additionalInfoInDeclarationLevel = declaration.AdditionalInfos.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var additionalInfoInEntryInstructionLevel = entryInstruction.AdditionalInfos.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var additionalInfoInInvoiceHeaderLevel = invoiceHeader.AdditionalInfos.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var additionalInfoInInvoiceLineLevel = invoiceLine.AdditionalInfos.AddNew();

			using (var context = new AdditionalInfoValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(x => x.IsRuleNAT_088BisActive);
				CombineAssertions("When RuleNAT_088Bis is enabled:", () =>
				{
					additionalInfoInDeclarationLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertNoMessageError("When E0001 is entered at declaration level, there should be no message error.", additionalInfoInDeclarationLevel.CSI_CodeInfo, messageError);

					additionalInfoInEntryInstructionLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertHasMessageError("When E0001 is entered at entry instruction level, there should be message error.", additionalInfoInEntryInstructionLevel.CSI_CodeInfo, messageError);
					additionalInfoInEntryInstructionLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.FretCargo;
					AssertNoMessageError("When additional document other than E0001 is entered at entry instruction level, there should be no message error.", additionalInfoInEntryInstructionLevel.CSI_CodeInfo, messageError);

					additionalInfoInInvoiceHeaderLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertHasMessageError("When E0001 is entered at invoiceHeader level, there should be message error.", additionalInfoInInvoiceHeaderLevel.CSI_CodeInfo, messageError);
					additionalInfoInInvoiceHeaderLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.FretCargo;
					AssertNoMessageError("When additional document other than E0001 is entered at invoiceHeader level, there should be no message error.", additionalInfoInInvoiceHeaderLevel.CSI_CodeInfo, messageError);

					additionalInfoInInvoiceLineLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertHasMessageError("When E0001 is entered at invoiceLine level, there should be message error.", additionalInfoInInvoiceLineLevel.CSI_CodeInfo, messageError);
					additionalInfoInInvoiceLineLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.FretCargo;
					AssertNoMessageError("When additional document other than E0001 is entered at invoiceLine level, there should be no message error.", additionalInfoInInvoiceLineLevel.CSI_CodeInfo, messageError);
				});

				context.DisableRule(x => x.IsRuleNAT_088BisActive);
				CombineAssertions("When RuleNAT_088Bis is disabled:", () =>
				{
					additionalInfoInDeclarationLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertNoMessageError("When E0001 is entered at declaration level, there should be no message error.", additionalInfoInDeclarationLevel.CSI_CodeInfo, messageError);

					additionalInfoInEntryInstructionLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertNoMessageError("When E0001 is entered at entry instruction level, there should be no message error.", additionalInfoInEntryInstructionLevel.CSI_CodeInfo, messageError);

					additionalInfoInInvoiceHeaderLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertNoMessageError("When E0001 is entered at invoiceHeader level, there should be no message error.", additionalInfoInInvoiceHeaderLevel.CSI_CodeInfo, messageError);

					additionalInfoInInvoiceLineLevel.CSI_Code = RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					AssertNoMessageError("When E0001 is entered at invoiceLine level, there should be no message error.", additionalInfoInInvoiceLineLevel.CSI_CodeInfo, messageError);
				});
			}
		}
	}
}
