using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderValidation))]
	class CusEntryHeaderValidationTest : EU.Business.Declaration.Testing.CusEntryHeaderValidationTest
	{
		public void TestCheckRuleNAT_185()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("OTH");
			Factory.Save();

			SetUpLimits(helper, 400, 0, 0, FRConstants.ThresholdsAndLimits.PromotionalProductToDROMValueLimit);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_LinePrice = 200m;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine2.JI_CL = entryLine1.PK;
			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 500m;
			invoiceLine3.JI_CL = entryLine2.PK;

			var errorMessage = "[NAT_185] Total Invoiced Amount (EUR) should not be greater than 400 EUR when CANA 0090(promotional product to DROM) is selected.";

			using var testContext = new EntryHeaderValidationDeciderTestContext(entry);
			testContext.EnableRule(x => x.IsRuleNAT_185Active);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			entry.ClearAllNotifications();
			entry.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is promotional product to DROM.", entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is not promotional product to DROM.", !entryLine2.IsPromotionalProductToDROM);
			AssertHasMessageError("The total invoiced amount for entry lines that are promotional product to DROM (200 + 300) is greater than 400. A message error should show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			testContext.DisableRule(x => x.IsRuleNAT_185Active);
			entry.Validation.ValidateAll();
			AssertNoMessageError("No message error should show when RuleNAT_185 is not activated.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			testContext.EnableRule(x => x.IsRuleNAT_185Active);
			invoiceLine2.JI_LinePrice = 50m;
			entry.Validation.ValidateAll();
			AssertNoMessageError("The total invoiced amount for entry lines that are promotional product to DROM (200 + 50) is lower than 400. No message error should show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			invoiceLine3.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			entry.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is promotional product to DROM.", entryLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second entry line is promotional product to DROM.", entryLine2.IsPromotionalProductToDROM);
			AssertHasMessageError("The total invoiced amount for entry lines that are promotional product to DROM (200 + 50 + 500) is greater than 400. A message error should show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			invoiceLine3.JI_LinePrice = 150m;
			entry.Validation.ValidateAll();
			AssertNoMessageError("The total invoiced amount for entry lines that are promotional product to DROM (200 + 50 + 150) is exactly 400. No message error should show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);
		}

		public void TestCheckRuleNAT_174()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("OTH");
			Factory.Save();

			SetUpLimits(helper, 150, 0, 0, FRConstants.ThresholdsAndLimits.NegligibleValueLimit);
			Factory.Save();

			CombineAssertions("Rule NAT_174 should trigger an error for UCC6 Import only, when all entry lines have C07 concession and CH_CalcTotalInvoicedAmountInLocalCurrency is greater than Negligible Value Limit from RefDB.", () =>
			{
				AssertRuleNAT_174(true, 0, "1122000", "1122000", false);
				AssertRuleNAT_174(true, 0, "1122C07", "1122000", false);
				AssertRuleNAT_174(true, 0, "1122C07", "1122C07", false);
				AssertRuleNAT_174(true, 200, "1122000", "1122000", false);
				AssertRuleNAT_174(true, 200, "1122C07", "1122000", false);
				AssertRuleNAT_174(true, 200, "1122C07", "1122C07", true);
				AssertRuleNAT_174(false, 200, "1122C07", "1122C07", false);
			});

			void AssertRuleNAT_174(bool isRuleNAT_174Active, decimal entryHeaderAmount, string firstInvoiceLineProcedure, string secondInvoiceLineProcedure, bool shouldHavemessageError)
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
				invoiceLine1.JI_LinePrice = entryHeaderAmount;

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;
				invoiceLine2.JI_Procedure = secondInvoiceLineProcedure;
				invoiceLine2.JI_LinePrice = 0m;

				using var testContext = new EntryHeaderValidationDeciderTestContext(entryHeader);
				if (isRuleNAT_174Active)
				{
					testContext.EnableRule(x => x.IsRuleNAT_174Active);
				}

				entryHeader.Validation.ValidateAll();

				var messageError = "[NAT_174] Total Invoiced Amount (EUR) should not be greater than 150 EUR when all items have C07 (negligible value) concession.";

				if (shouldHavemessageError)
				{
					AssertHasMessageError(entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
				else
				{
					AssertNoMessageError(entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
			}
		}

		public void TestCheckRuleNAT_177()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("OTH");
			Factory.Save();

			SetUpLimits(helper, 45, 0, 0, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule177);
			Factory.Save();

			CombineAssertions("Rule NAT_177 should trigger an error for UCC6 Import only, when one entry line has not a C08 concession and CH_CalcTotalInvoicedAmountInLocalCurrency is Lesser of Equals to 45 and at least one other entry Line has C08 concession.", () =>
			{
				AssertRuleNAT_177(true, 45, "1122000", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 45, no invoice has not a C08 concession.");
				AssertRuleNAT_177(true, 45, "1122C08", "1122000", true, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 45, one invoice only has a C08 concession.");
				AssertRuleNAT_177(true, 45, "1122C08", "1122C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 45, both invoice have a C08 concession.");

				AssertRuleNAT_177(true, 10, "1122000", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency < 45, no invoice has not a C08 concession.");
				AssertRuleNAT_177(true, 10, "1122C08", "1122000", true, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency < 45, one invoice only has a C08 concession.");
				AssertRuleNAT_177(true, 10, "1122C08", "1122C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency < 45, both invoice have a C08 concession.");

				AssertRuleNAT_177(false, 10, "1122C08", "1122000", false, "Rule is disable, CH_CalcTotalInvoicedAmountInLocalCurrency < 45, one invoice only has a C08 concession.");

				AssertRuleNAT_177(true, 70, "1122000", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency > 45, no invoice has not a C08 concession.");
				AssertRuleNAT_177(true, 70, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency > 45, one invoice only has a C08 concession.");
				AssertRuleNAT_177(true, 70, "1122C08", "1122C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency > 45, both invoice have a C08 concession.");
			});

			void AssertRuleNAT_177(bool isRuleNAT_177Active, decimal entryHeaderAmount, string firstInvoiceLineProcedure, string secondInvoiceLineProcedure, bool shouldHavemessageError, string comment)
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
				invoiceLine1.JI_LinePrice = entryHeaderAmount;

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;
				invoiceLine2.JI_Procedure = secondInvoiceLineProcedure;
				invoiceLine2.JI_LinePrice = 0m;

				using var testContext = new EntryHeaderValidationDeciderTestContext(entryHeader);
				if (isRuleNAT_177Active)
				{
					testContext.EnableRule(x => x.IsRuleNAT_177Active);
				}

				entryHeader.Validation.ValidateAll();

				var messageError = "[NAT_177] If one entry line has concession C08, and total invoice amount is lesser or equal to 45, all other lines must have concession C08 also.";

				if (shouldHavemessageError)
				{
					AssertHasMessageError(comment, entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
				else
				{
					AssertNoMessageError(comment, entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
			}
		}

		public void TestCheckRuleNAT_178()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("OTH");
			Factory.Save();

			SetUpLimits(helper, 0, 150, 45, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule178);
			Factory.Save();

			CombineAssertions("Rule NAT_178 should trigger an error for UCC6 Import only, when at least one entry line has a C08 concession and 45 < CH_CalcTotalInvoicedAmountInLocalCurrency <= 150.", () =>
			{
				AssertRuleNAT_178(true, 10, "1122000", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency < 45, no invoice has not a C08 concession.");
				AssertRuleNAT_178(true, 10, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency < 45, one invoice only has a C08 concession.");
				AssertRuleNAT_178(true, 10, "1122C08", "1122C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency < 45, both invoice have a C08 concession.");

				AssertRuleNAT_178(true, 45, "1122000", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 45, no invoice has not a C08 concession.");
				AssertRuleNAT_178(true, 45, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 45, one invoice only has a C08 concession.");
				AssertRuleNAT_178(true, 45, "1122C08", "1122C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 45, both invoice have a C08 concession.");

				AssertRuleNAT_178(false, 70, "1122C08", "1122000", false, "Rule is disable, 45 < CH_CalcTotalInvoicedAmountInLocalCurrency < 150, one invoice only has a C08 concession.");
				AssertRuleNAT_178(false, 150, "1122C08", "1122000", false, "Rule is disable, CH_CalcTotalInvoicedAmountInLocalCurrency = 150, one invoice only has a C08 concession.");

				AssertRuleNAT_178(true, 70, "1122000", "1122000", false, "Rule is enable, 45 < CH_CalcTotalInvoicedAmountInLocalCurrency < 150, no invoice has not a C08 concession.");
				AssertRuleNAT_178(true, 70, "1122C08", "1122000", true, "Rule is enable, 45 < CH_CalcTotalInvoicedAmountInLocalCurrency < 150, one invoice only has a C08 concession.");
				AssertRuleNAT_178(true, 70, "1122C08", "1122C08", true, "Rule is enable, 45 < CH_CalcTotalInvoicedAmountInLocalCurrency < 150, both invoice have a C08 concession.");

				AssertRuleNAT_178(true, 150, "1122000", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 150, no invoice has not a C08 concession.");
				AssertRuleNAT_178(true, 150, "1122C08", "1122000", true, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 150, one invoice only has a C08 concession.");
				AssertRuleNAT_178(true, 150, "1122C08", "1122C08", true, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency = 150, both invoice have a C08 concession.");

				AssertRuleNAT_178(true, 170, "1122000", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, no invoice has not a C08 concession.");
				AssertRuleNAT_178(true, 170, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, one invoice only has a C08 concession.");
				AssertRuleNAT_178(true, 170, "1122C08", "1122C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, both invoice have a C08 concession.");
			});

			void AssertRuleNAT_178(bool isRuleNAT_178Active, decimal entryHeaderAmount, string firstInvoiceLineProcedure, string secondInvoiceLineProcedure, bool shouldHavemessageError, string comment)
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
				invoiceLine1.JI_LinePrice = entryHeaderAmount;

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;
				invoiceLine2.JI_Procedure = secondInvoiceLineProcedure;
				invoiceLine2.JI_LinePrice = 0m;

				using var testContext = new EntryHeaderValidationDeciderTestContext(entryHeader);
				if (isRuleNAT_178Active)
				{
					testContext.EnableRule(x => x.IsRuleNAT_178Active);
				}
				entryHeader.Validation.ValidateAll();

				var messageError = "[NAT_178] At least one entry line uses concession C08, but it is not allowed when total invoiced amount is greater than 45 and lesser or equal to 150 EUR.";

				if (shouldHavemessageError)
				{
					AssertHasMessageError(comment, entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
				else
				{
					AssertNoMessageError(comment, entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
			}
		}

		public void TestCheckRuleNAT_179()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("OTH");
			Factory.Save();

			SetUpLimits(helper, 150, 0, 0, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule179A);
			SetUpLimits(helper, 45, 0, 0, FRConstants.ThresholdsAndLimits.C2CValueProcedureRule179B);
			Factory.Save();

			CombineAssertions("Rule NAT_179 should trigger an error for UCC6 Import only, when sum of CH_CalcTotalInvoicedAmountInLocalCurrency of the C08 entry lines > 45 and CH_CalcTotalInvoicedAmountInLocalCurrency of all entry lines > 150 .", () =>
			{
				AssertRuleNAT_179(true, 10, 50, 0, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 < 45, CH_CalcTotalInvoicedAmountInLocalCurrency < 150, one invoice a C08 concession.");
				AssertRuleNAT_179(true, 10, 20, 0, "1122C08", "11220C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 < 45, CH_CalcTotalInvoicedAmountInLocalCurrency < 150, two invoice has a C08 concession.");

				AssertRuleNAT_179(true, 45, 50, 0, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 = 45, CH_CalcTotalInvoicedAmountInLocalCurrency < 150, one invoice a C08 concession.");
				AssertRuleNAT_179(true, 20, 25, 0, "1122C08", "11220C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 = 45, CH_CalcTotalInvoicedAmountInLocalCurrency < 150, two invoice has a C08 concession.");

				AssertRuleNAT_179(true, 50, 50, 0, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency < 150, one invoice a C08 concession.");
				AssertRuleNAT_179(true, 20, 30, 0, "1122C08", "11220C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency < 150, two invoice has a C08 concession.");

				AssertRuleNAT_179(true, 10, 50, 150, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 < 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, one invoice a C08 concession.");
				AssertRuleNAT_179(true, 10, 20, 150, "1122C08", "11220C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 < 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, two invoice has a C08 concession.");

				AssertRuleNAT_179(true, 45, 50, 150, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 = 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, one invoice a C08 concession.");
				AssertRuleNAT_179(true, 20, 25, 150, "1122C08", "11220C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 = 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, two invoice has a C08 concession.");

				AssertRuleNAT_179(true, 50, 50, 150, "1122C08", "1122000", true, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, one invoice a C08 concession.");
				AssertRuleNAT_179(true, 20, 30, 150, "1122C08", "11220C08", true, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, two invoice has a C08 concession.");

				AssertRuleNAT_179(false, 50, 50, 150, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, one invoice a C08 concession.");
				AssertRuleNAT_179(false, 20, 30, 150, "1122C08", "11220C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency > 150, two invoice has a C08 concession.");

				AssertRuleNAT_179(true, 50, 50, 50, "1122C08", "1122000", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency = 150, one invoice a C08 concession.");
				AssertRuleNAT_179(true, 20, 30, 100, "1122C08", "11220C08", false, "Rule is enable, CH_CalcTotalInvoicedAmountInLocalCurrency with C08 > 45, CH_CalcTotalInvoicedAmountInLocalCurrency = 150, two invoice has a C08 concession.");
			});

			void AssertRuleNAT_179(bool isRuleNAT_179Active, decimal firstInvoiceAmount, decimal secondInvoiceAmount, decimal thirdInvoiceAmount, string firstInvoiceLineProcedure, string secondInvoiceLineProcedure, bool shouldHavemessageError, string comment)
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();
				var entryLine3 = entryHeader.MergedLines.AddNew();

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;
				invoiceLine1.JI_Procedure = firstInvoiceLineProcedure;
				invoiceLine1.JI_LinePrice = firstInvoiceAmount;

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;
				invoiceLine2.JI_Procedure = secondInvoiceLineProcedure;
				invoiceLine2.JI_LinePrice = secondInvoiceAmount;

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_CL = entryLine3.PK;
				invoiceLine3.JI_Procedure = "1122000";
				invoiceLine3.JI_LinePrice = thirdInvoiceAmount;

				using var testContext = new EntryHeaderValidationDeciderTestContext(entryHeader);
				if (isRuleNAT_179Active)
				{
					testContext.EnableRule(x => x.IsRuleNAT_179Active);
				}
				entryHeader.Validation.ValidateAll();

				var messageError = "[NAT_179] Cumulative total invoice amount for concession C08 should not be greater than 45 EUR when total invoiced amount exceeds 150 EUR.";

				if (shouldHavemessageError)
				{
					AssertHasMessageError(comment, entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
				else
				{
					AssertNoMessageError(comment, entryHeader.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, messageError);
				}
			}
		}

		public void TestCheckRuleNAT_189()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("OTH");
			Factory.Save();

			SetUpLimits(helper, 22, 0, 0, FRConstants.ThresholdsAndLimits.ProductOfNegligibleValueToDROM);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var (invoiceHeader1, invoiceLine1) = SetupInvoiceHeaderAndInvoiceLine(entryLine1, 10m);
			var (invoiceHeader2, invoiceLine2) = SetupInvoiceHeaderAndInvoiceLine(entryLine1, 30m);
			var (invoiceHeader3, invoiceLine3) = SetupInvoiceHeaderAndInvoiceLine(entryLine2, 50m);

			var errorMessage = "[NAT_189] Total Invoiced Amount (EUR) should not be greater than 22 EUR when CANA 0089(product of negligible value to DROM) is selected.";

			using var testContext = new EntryHeaderValidationDeciderTestContext(entry);
			testContext.EnableRule(x => x.IsRuleNAT_189Active);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			entry.ClearAllNotifications();
			entry.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is product of negligible value to DROM.", entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is not product of negligible value to DROM.", !entryLine2.IsProductOfNegligibleValueToDROM);
			AssertHasMessageError("The total invoiced amount for entry lines that are product of negligible value to DROM (10 + 30) is greater than 22. A message error should show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			testContext.DisableRule(x => x.IsRuleNAT_189Active);
			entry.Validation.ValidateAll();
			AssertNoMessageError("No message error should show when RuleNAT_189 is not activated.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			testContext.EnableRule(x => x.IsRuleNAT_189Active);
			invoiceLine2.JI_LinePrice = 8m;
			entry.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is product of negligible value to DROM.", entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is not product of negligible value to DROM.", !entryLine2.IsProductOfNegligibleValueToDROM);
			AssertNoMessageError("The total invoiced amount for entry lines that are product of negligible value to DROM (10 + 8) is lesser than 22. No message error should show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			invoiceLine3.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			entry.Validation.ValidateAll();
			Assert("Prerequisite: first entry line is product of negligible value to DROM.", entryLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second entry line is product of negligible value to DROM.", entryLine2.IsProductOfNegligibleValueToDROM);
			AssertHasMessageError("The total invoiced amount for entry lines that are product of negligible value to DROM (10 + 8 + 50) is greater than 22. A message error should show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			invoiceLine3.JI_LinePrice = 4m;
			entry.Validation.ValidateAll();
			AssertNoMessageError("The total invoiced amount for entry lines that are product of negligible value to DROM (10 + 8 + 4) is exactly 22. No message error should not show.", entry.CH_CalcTotalInvoicedAmountInLocalCurrencyInfo, errorMessage);

			(JobComInvoiceHeader, JobComInvoiceLine) SetupInvoiceHeaderAndInvoiceLine(CusEntryLine entryLine, ZDecimal linePrice)
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_LinePrice = linePrice;

				return (invoiceHeader, invoiceLine);
			}
		}

		void SetUpLimits(UniversalReferenceTestDataHelper helper, int threshold, int maximum, int minimum, string code)
		{
			var c2cTax = helper.CreateTaxOrFee(code, 0, Core.Constants.CountryCodes.France);
			c2cTax.ZZF_StartDate = ZDate.Today.AddDays(-1);
			c2cTax.ZZF_EndDate = ZDate.Today.AddDays(+1);
			c2cTax.ZZF_Threshold = threshold;
			c2cTax.ZZF_Minimum = minimum;
			c2cTax.ZZF_Maximum = maximum;
			c2cTax.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			Factory.Save();
		}

		public override void TestValidationDecider()
		{
			var deltaGDeclaration = Factory.New<JobDeclaration>();
			var deltaGEntryHeader = deltaGDeclaration.CustomsEntryHeaders.AddNew();

			deltaGDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertNull(deltaGEntryHeader.Validation.ValidationDecider);

			var exportDeltaIEDeclaration = Factory.New<JobDeclaration>();
			var exportDeltaIEEntryHeader = exportDeltaIEDeclaration.CustomsEntryHeaders.AddNew();
			exportDeltaIEDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			exportDeltaIEDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertNull(exportDeltaIEEntryHeader.Validation.ValidationDecider);

			var importDeltaIEDeclaration = Factory.New<JobDeclaration>();
			var importDeltaIEEntryHeader = importDeltaIEDeclaration.CustomsEntryHeaders.AddNew();
			importDeltaIEDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			importDeltaIEDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<UCC6ImportEntryHeaderValidationDecider>(importDeltaIEEntryHeader.Validation.ValidationDecider);
		}

		public void TestCheckCH_TriggeringPointForValidation()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_TriggeringPointForValidation = "BLA";
			AssertHasErrorContaining(entryHeader.CH_TriggeringPointForValidationInfo, ListValidation.InvalidCodeError);
			entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.NUL;
			AssertNoErrorContaining(entryHeader.CH_TriggeringPointForValidationInfo, ListValidation.InvalidCodeError);
		}
	}
}
