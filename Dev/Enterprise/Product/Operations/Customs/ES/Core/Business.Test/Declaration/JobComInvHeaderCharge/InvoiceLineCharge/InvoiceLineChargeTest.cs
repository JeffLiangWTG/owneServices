using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	public class InvoiceLineChargeTest : EU.Business.Declaration.Testing.InvoiceLineChargeTest
	{
		public new void TestLookups()
		{
			AssertType<InvoiceLineChargeLookups>(Factory.New<InvoiceLineCharge>().Lookups);
		}

		public new void TestValidation()
		{
			AssertType<InvoiceLineChargeValidation>(Factory.New<InvoiceLineCharge>().Validation);
		}

		public void TestUpdateJ7_IsIncludedInInvoiceWhenChargeTypeIsEnteredExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			InvoiceLineCharge charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = ESCustomsChargeTypeList.Codes.OtherNationalPayments;
			CombineAssertions("For Export Declarations", () =>
			{
				AssertEquals("included in invoice", true, charge.J7_Calc_IsIncludedInInvoiceAmount);

				invoice.JZ_IncoTerm = "EXW";
				AssertEquals("not included in invoice", false, charge.J7_Calc_IsIncludedInInvoiceAmount);
			});
		}

		public void TestJ7_IsIncludedInInvoiceImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			CombineAssertions("For Import Declarations", () =>
			{
				AssertJ7_IsIncludedInInvoiceImportForCharge(invoiceLine, ChargeTypeList.Codes.StatisticalValue, false, false);

				AssertJ7_IsIncludedInInvoiceImportForCharge(invoiceLine, ESCustomsChargeTypeList.Codes.TerminalHandlingCharge, true, false);

				AssertJ7_IsIncludedInInvoiceImportForCharge(invoiceLine, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, false, true);

				AssertJ7_IsIncludedInInvoiceImportForCharge(invoiceLine, ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, true, true);

				AssertJ7_IsIncludedInInvoiceImportForCharge(invoiceLine, ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, false, true);
			});
		}

		void AssertJ7_IsIncludedInInvoiceImportForCharge(JobComInvoiceLine invoiceLine, string chargeCode, bool isIncluded, bool isReadOnly)
		{
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = chargeCode;
			AssertEquals("included in invoice for " + chargeCode + " charge", isIncluded, charge.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount readonly for " + chargeCode + " charge", isReadOnly, charge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
		}

		protected override BaseJobDeclaration GetJobDeclarationForTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			return declaration;
		}

		protected override string GetIncotermToTestIsIncludedInInvoice() => "EXW";
	}
}
