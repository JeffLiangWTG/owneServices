using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestDutyReductionAmount()
		{
			var declaration = ImportJobDeclaration;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.JobComInvoiceLines.AddNew() as JobComInvoiceLine;
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew() as JobComInvoiceLine;
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_DutyReductionAmount = 123;
			line2.JI_DutyReductionAmount = 456;

			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0] as CusEntryLine;
			AssertEquals(line1.JI_DutyReductionAmount + line2.JI_DutyReductionAmount, entryLine.DutyReductionAmount);
		}

		public void TestCustomsValueIsUsingTheCorrectDateForExchangeRate()
		{
			var declaration = ImportJobDeclaration;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2024, 8, 15);
			entryInstruction2.CEI_DateForDuty = new ZDateTime(2024, 8, 30);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var line1 = invoiceHeader.JobComInvoiceLines.AddNew() as JobComInvoiceLine;
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew() as JobComInvoiceLine;
			line1.JI_LinePrice = 1m;
			line2.JI_LinePrice = 1m;
			line1.JI_CEI = entryInstruction1.PK;
			line2.JI_CEI = entryInstruction2.PK;

			using (var company = GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Japan))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
				currency.SetCustomsRate(new ZDateTime(2024, 8, 11), new ZDateTime(2024, 8, 17), 10m);
				currency.SetCustomsRate(new ZDateTime(2024, 8, 25), new ZDateTime(2024, 8, 31), 20m);
				Factory.Save();

				DoMerge(declaration);
				AssertEquals(2, declaration.ActiveEntryHeaders.Count);
				var entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0] as CusEntryLine;
				var entryLine2 = declaration.ActiveEntryHeaders[1].MergedLines[0] as CusEntryLine;
				AssertEquals(10m, entryLine1.CL_CustomsValue);
				AssertEquals(20m, entryLine2.CL_CustomsValue);
			}
		}

		public void TestBasicPrice()
		{
			var declaration = ImportJobDeclaration;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2024, 8, 15);
			entryInstruction2.CEI_DateForDuty = new ZDateTime(2024, 8, 30);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var charge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 2000m, invoiceHeader.JZ_RX_NKInvoice_Currency);
			charge.J7_IsDutiable = true;
			var line1 = invoiceHeader.JobComInvoiceLines.AddNew() as JobComInvoiceLine;
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew() as JobComInvoiceLine;
			line1.JI_LinePrice = 1m;
			line2.JI_LinePrice = 1m;
			line1.JI_CEI = entryInstruction1.PK;
			line2.JI_CEI = entryInstruction2.PK;

			using (var company = GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Japan))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				DoMerge(declaration);
				var entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0] as CusEntryLine;
				var entryLine2 = declaration.ActiveEntryHeaders[1].MergedLines[0] as CusEntryLine;
				CombineAssertions(() =>
				{
					AssertEquals(1001m, entryLine1.BasicPrice);
					AssertEquals(Core.Constants.CurrencyCodes.Australia, entryLine1.BasicPriceCurrencyCode);
					AssertEquals(1001m, entryLine2.BasicPrice);
					AssertEquals(Core.Constants.CurrencyCodes.Australia, entryLine1.BasicPriceCurrencyCode);
				});
			}
		}

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(new ZDateTime(2005, 6, 1), new ZDateTime(2005, 6, 5), RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;

			var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			var line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line1OFT.J7_Amount = 10.0m;
			var line2ONS = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			var line2OFT = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line2OFT.J7_Amount = 20.0m;

			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("FOB", 690.0m, entryLine.FOBInLocalCurrency.Amount);
				AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
				AssertEquals("Overseas Freight", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
				AssertEquals("Overseas Insurance", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
				AssertEquals("T and I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
			});
		}

		public override void TestDescriptionFromLines()
		{
			var declaration = ImportJobDeclaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew() as CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			entryInstruction.CEI_GoodsDescription = "Description on Instruction";
			invoiceLine.JI_Description = "Description on Line";

			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Description on Instruction", entryLine.Description);

			var newInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew() as JobComInvoiceLine;
			DoMerge(declaration);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Description on Line", entryLine.Description);
		}

		public void TestCustomsQuantity1()
		{
			var declaration = ImportJobDeclaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine2.JI_CustomsQuantity = 1m;

			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0] as CusEntryLine;
			AssertEquals(1m, entryLine.CustomsQuantity1);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(2m, entryLine.CustomsQuantity1);

			invoiceLine2.JI_CustomsUnitQty = "KG";
			AssertEquals(0m, entryLine.CustomsQuantity1);
		}

		public void TestCustomsQuantityQty()
		{
			var declaration = ImportJobDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsUnitQty = "KG";

			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0] as CusEntryLine;
			AssertEquals("KG", entryLine.CustomsQuantityUnit1);

			invoiceLine2.JI_CustomsUnitQty = "LB";
			AssertEquals(ZString.Empty, entryLine.CustomsQuantityUnit1);
		}

		public void TestCustomsQuantity2()
		{
			var declaration = ImportJobDeclaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine2.JI_CustomsSecondQuantity = 1m;

			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0] as CusEntryLine;
			AssertEquals(1m, entryLine.CustomsQuantity2);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(2m, entryLine.CustomsQuantity2);

			invoiceLine2.JI_CustomsSecondUnitQty = "KG";
			AssertEquals(0m, entryLine.CustomsQuantity2);
		}

		public void TestCustomsQuantityQty2()
		{
			var declaration = ImportJobDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine2.JI_CustomsSecondUnitQty = "KG";

			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0] as CusEntryLine;
			AssertEquals("KG", entryLine.CustomsQuantityUnit2);

			invoiceLine2.JI_CustomsSecondUnitQty = "LB";
			AssertEquals(ZString.Empty, entryLine.CustomsQuantityUnit2);
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var declaration = base.ImportJobDeclaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				return declaration;
			}
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
	}
}
