using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		public void TestCalculateEntryLineVatFee()
		{
			(var declaration, var instruction, var invoice, var invoiceLine) = SetupData();
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var fees = entryLine.Fees.Cast<CusEntryLineFee>();
			CombineAssertions("EntryLine B00 test cases.", () =>
			{
				AssertEquals("To make sure UCC5", true, declaration.IsUCC5);
				AssertEquals("UCC5, without 1A01 Doc, should have B00 Fee.", true, fees.Any(fee => fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));

				instruction.SupportingDocuments.AddNew().CSI_Code = "1A01";
				merger.DoMerge();
				AssertEquals("UCC5, 1A01 on Instruction, should NOT have B00 Fee.", false, fees.Any(fee => fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));

				instruction.SupportingDocuments.RemoveAndDeleteAll();
				invoice.SupportingDocuments.AddNew().CSI_Code = "1A01";
				merger.DoMerge();
				AssertEquals("UCC5, 1A01 on InvoiceHeader, should NOT have B00 Fee.", false, fees.Any(fee => fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));

				invoice.SupportingDocuments.RemoveAndDeleteAll();
				merger.DoMerge();
				AssertEquals("To make sure removing SupportingDocuments on header level enables B00 back.", true, fees.Any(fee => fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));

				invoiceLine.SupportingDocuments.AddNew().CSI_Code = "1A01";
				merger.DoMerge();
				entryLine = entryHeader.MergedLines[0];
				AssertEquals("UCC5, WITH 1A01 Doc, should NOT have B00 Fee.", false, fees.Any(fee => fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				merger.DoMerge();
				AssertEquals("Non-UCC5, should have B00 fee even with 1A01 Doc.", 0, entryLine.Fees.Count);
			});
		}

		public void TestCalculateEntryLineVatFee_NewAISCodes()
		{
			(var declaration, var instruction, var invoice, var invoiceLine) = SetupData();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var fees = entryLine.Fees.Cast<CusEntryLineFee>();
			CombineAssertions("EntryLine B00 test cases.", () =>
			{
				AssertEquals("Without 1A05 or 1A06, Entry Line should have B00 Fee.", true, fees.Any(fee => fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));

				instruction.SupportingDocuments.AddNew().CSI_Code = "1A05";
				merger.DoMerge();
				AssertEquals("When SupportingDoc is 1A05, Fee type is 1B2.", true, fees.Any(fee => fee.CF_ChargeType == "1B2"));

				instruction.SupportingDocuments.RemoveAndDeleteAll();
				invoice.SupportingDocuments.AddNew().CSI_Code = "1A06";
				merger.DoMerge();
				AssertEquals("When SupportingDoc is 1A06, Fee type is 1B3.", true, fees.Any(fee => fee.CF_ChargeType == "1B3"));

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H5;
				instruction.SupportingDocuments.RemoveAndDeleteAll();
				invoice.SupportingDocuments.AddNew().CSI_Code = "1A05";
				merger.DoMerge();
				AssertEquals("When SupportingDoc is 1A05, Fee type is 1B2.", true, fees.Any(fee => fee.CF_ChargeType == "1B2"));

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
				invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
				instruction.SupportingDocuments.AddNew().CSI_Code = "1A05";
				merger.DoMerge();
				AssertEquals("When Declaration type isn't H1 or H5, Fee type is B00.", true, fees.Any(fee => fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));
				AssertEquals("When Declaration type isn't H1 or H5, Fee type is not 1B2.", false, fees.Any(fee => fee.CF_ChargeType == "1B2"));
				AssertEquals("When Declaration type isn't H1 or H5, Fee type is not 1B3.", false, fees.Any(fee => fee.CF_ChargeType == "1B3"));
			});
		}

		(JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction, JobComInvoiceHeader invHeader, JobComInvoiceLine invLine) SetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			var vat = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, "VAT");
			var vatCode = helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, vat.PK);
			var taxOrFee = helper.CreateTaxOrFee(UniversalReferenceConstants.TaxOrFeeType.Codes.Standard, 0.2m, Core.Constants.CountryCodes.Ireland);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			var instruction = declaration.CustomsEntryInstructions.FirstOrAddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_ZZF_NKTaxType = taxOrFee.ZZF_Code;
			invoiceLine.JI_LinePrice = 1000;

			return (declaration, instruction, invoice, invoiceLine);
		}

		public void TestShouldCalculateDutiesForEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var instruction = declaration.CustomsEntryInstructions.FirstOrAddNew();
			instruction.CEI_Style = "H2";

			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var strategy = new DutyCalculatorStrategyForTest(declaration);

			CombineAssertions("ShouldCalculateDutiesForEntryLine test cases.", () =>
			{
				AssertEquals("UCC5AndIsImport, H2, should calculate duties.", true, strategy.ShouldCalculateDutiesForEntryLine_Exposed(entryLine));

				instruction.CEI_Style = "H3";
				AssertEquals("UCC5AndIsImport, H3, should NOT calculate duties.", false, strategy.ShouldCalculateDutiesForEntryLine_Exposed(entryLine));

				instruction.CEI_Style = "H4";
				AssertEquals("UCC5AndIsImport, H4, should NOT calculate duties.", false, strategy.ShouldCalculateDutiesForEntryLine_Exposed(entryLine));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("Non-UCC5 should calculate duties.", true, strategy.ShouldCalculateDutiesForEntryLine_Exposed(entryLine));
			});
		}

		public void TestShouldCalculateTaxesForEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var instruction = declaration.CustomsEntryInstructions.FirstOrAddNew();
			instruction.CEI_Style = "H2";

			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var strategy = new DutyCalculatorStrategyForTest(declaration);

			CombineAssertions("ShouldCalculateTaxesForEntryLine test cases.", () =>
			{
				AssertEquals("UCC5AndIsImport, H2, should calculate taxes.", true, strategy.ShouldCalculateTaxesForEntryLine_Exposed(entryLine));

				instruction.CEI_Style = "H3";
				AssertEquals("UCC5AndIsImport, H3, should NOT calculate taxes.", false, strategy.ShouldCalculateTaxesForEntryLine_Exposed(entryLine));

				instruction.CEI_Style = "H4";
				AssertEquals("UCC5AndIsImport, H4, should NOT calculate taxes.", false, strategy.ShouldCalculateTaxesForEntryLine_Exposed(entryLine));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("Non-UCC5 should calculate taxes.", true, strategy.ShouldCalculateTaxesForEntryLine_Exposed(entryLine));
			});
		}

		protected override ZString VATableAdditionChargeCode => AISChargeCodeList.Codes.AB;

		protected override ZString NonVATableDeductionChargeCode => ZString.Empty;

		sealed class DutyCalculatorStrategyForTest : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyForTest(JobDeclaration declaration) : base(declaration)
			{ }

			public bool ShouldCalculateDutiesForEntryLine_Exposed(EU.Business.Declaration.CusEntryLine entryLine) => ShouldCalculateDutiesForEntryLine(entryLine);

			public bool ShouldCalculateTaxesForEntryLine_Exposed(EU.Business.Declaration.CusEntryLine entryLine) => ShouldCalculateTaxesForEntryLine(entryLine);
		}
	}
}
