namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class CusEntryLineValidationTest : EU.Business.Declaration.Testing.CusEntryLineValidationTest
	{
		public void TestCheckRule_NAT184()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine1.PK;
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;

			var errorMessage = "[NAT_184] if one entry line has CANA 0090 (promotional product to DROM), all other lines must have CANA 0090 also.";

			using var testContext = new EntryLineValidationDeciderTestContext(entryLine1);
			testContext.EnableRule(x => x.IsRuleNAT_184Active);

			entryLine1.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is not promotional product to DROM.", !entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is not promotional product to DROM.", !entryLine2.IsPromotionalProductToDROM);
			AssertNoRowMessageError("IsPromotionalProductToDROM of entryLine1 is false. No message error should show.", entryLine1, errorMessage);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			entryLine1.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is promotional product to DROM.", entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is not promotional product to DROM.", !entryLine2.IsPromotionalProductToDROM);
			AssertHasRowMessageError("IsPromotionalProductToDROM of entryLine1 is true but another entry line has IsPromotionalProductToDROM false.", entryLine1, errorMessage);

			testContext.DisableRule(x => x.IsRuleNAT_184Active);
			entryLine1.Validation.ValidateAll();
			AssertNoRowMessageError("RuleNAT_184 is not activated. No message error should show.", entryLine1, errorMessage);

			testContext.EnableRule(x => x.IsRuleNAT_184Active);
			invoiceLine3.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			entryLine1.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is promotional product to DROM.", entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is promotional product to DROM.", entryLine2.IsPromotionalProductToDROM);
			Assert("Prerequisite: the entry should be considered as promotional product to DROM when all entry line are.", entry.IsPromotionalProductToDROM);
			AssertNoRowMessageError("All EntryLines are Promotional Products to DROM. No message error should show.", entryLine1, errorMessage);
		}

		public void TestCheckRule_NAT175()
		{
			CombineAssertions("Rule NAT_175 should trigger an error for UCC6 Import only, when one entry line has C07 concession while any other entry line doesn't.", () =>
			{
				AssertRuleNat_175(true, "1122000", "1122000", false);
				AssertRuleNat_175(true, "1122C07", "1122000", false);
				AssertRuleNat_175(true, "1122C07", "1122C07", false);
				AssertRuleNat_175(true, "1122000", "1122C07", true);
				AssertRuleNat_175(false, "1122000", "1122C07", false);
			});

			void AssertRuleNat_175(bool isRuleNAT_175Active, string firstInvoiceLineProcedure, string siblingInvoiceLineProcedure, bool shouldHavemessageError)
			{
				var declaration = Factory.New<JobDeclaration>();

				var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;
				invoiceLine1.JI_Procedure = firstInvoiceLineProcedure;

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;
				invoiceLine2.JI_Procedure = siblingInvoiceLineProcedure;

				using var testContext = new EntryLineValidationDeciderTestContext(entryLine1);
				if (isRuleNAT_175Active)
				{
					testContext.EnableRule(x => x.IsRuleNAT_175Active);
				}

				entryLine1.Validation.ValidateAll();

				var errorMessage = "[NAT_175] If one invoice line has concession C07, all other lines must have concession C07 (negligible value) also.";
				if (shouldHavemessageError)
				{
					AssertHasRowMessageError(entryLine1, errorMessage);
				}
				else
				{
					AssertNoRowMessageError(entryLine1, errorMessage);
				}
			}
		}

		public void TestCheckRule_NAT188()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine1.PK;
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;

			var errorMessage = "[NAT_188] if one entry line has CANA 0089 (product of negligible value to DROM), all other lines must have CANA 0089 also.";

			using var testContext = new EntryLineValidationDeciderTestContext(entryLine1);
			testContext.EnableRule(x => x.IsRuleNAT_188Active);

			entryLine1.ClearAllNotifications();
			entryLine1.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is not product of negligible value to DROM.", !entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is not product of negligible value DROM.", !entryLine2.IsProductOfNegligibleValueToDROM);
			AssertNoRowMessageError("No message error should show when IsProductOfNegligibleValueToDROM of EntryLine is false", entryLine1, errorMessage);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			entryLine1.ClearAllNotifications();
			entryLine1.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is product of negligible value to DROM.", entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is not product of negligible value to DROM.", !entryLine2.IsProductOfNegligibleValueToDROM);
			AssertHasRowMessageError("Error message should show when one of the entry lines has IsProductOfNegligibleValueToDROM as true", entryLine1, errorMessage);

			testContext.DisableRule(x => x.IsRuleNAT_188Active);
			entryLine1.ClearAllNotifications();
			entryLine1.Validation.ValidateAll();
			AssertNoRowMessageError("RuleNAT_188 is not activated. No message error should show.", entryLine1, errorMessage);

			testContext.EnableRule(x => x.IsRuleNAT_188Active);
			invoiceLine3.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			entryLine1.ClearAllNotifications();
			entryLine1.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is product of negligible value DROM.", entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is product of negligible value to DROM.", entryLine2.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: the entry should be considered as product of negligible value to DROM when all entry line are.", entry.IsProductOfNegligibleValueToDROM);
			AssertNoRowMessageError("All EntryLines are products of negligible value. No message error should show.", entryLine1, errorMessage);
		}

		public override void TestValidationDecider()
		{
			var deltaGDeclaration = Factory.New<JobDeclaration>();
			var deltaGEntryLine = deltaGDeclaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();

			deltaGDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertNull(deltaGEntryLine.Validation.ValidationDecider);

			var exportDeltaIEDeclaration = Factory.New<JobDeclaration>();
			var exportDeltaIEEntryLine = exportDeltaIEDeclaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			exportDeltaIEDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			exportDeltaIEDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertNull(exportDeltaIEEntryLine.Validation.ValidationDecider);

			var importDeltaIEDeclaration = Factory.New<JobDeclaration>();
			var importDeltaIEEntryLine = importDeltaIEDeclaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			importDeltaIEDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			importDeltaIEDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<UCC6ImportEntryLineValidationDecider>(importDeltaIEEntryLine.Validation.ValidationDecider);
		}
	}
}
