using Enterprise.Customs.Common.CA;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DocDutyOrTax))]
	sealed class DocDutyOrTaxTest : DocumentWrapperTestCase
	{
		public void TestDocDutyOrTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.DutiesAndTaxes.DeleteAll();
			AddDutyOrTax(invoiceLine, DutyAndTaxTypes.Codes.ADD, SIMACodes.Codes.C10, 0m, string.Empty, 0m, string.Empty);
			AddDutyOrTax(invoiceLine, DutyAndTaxTypes.Codes.ExciseTax, ExciseTaxExemptionCodes.Codes.C94, 0m, string.Empty, 0m, string.Empty);
			AddDutyOrTax(invoiceLine, DutyAndTaxTypes.Codes.GST, string.Empty, 5m, RateTypes.Codes.AdValorem, 50m, string.Empty);
			AddDutyOrTax(invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, string.Empty, 10m, RateTypes.Codes.Specific, 50m, string.Empty);
			AddDutyOrTax(invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, string.Empty, 0.000745m, RateTypes.Codes.Specific, 50m, UnitOfWeightList.Codes.Gram);
			AddDutyOrTax(invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, string.Empty, 0.000745m, RateTypes.Codes.AdValorem, 50m, UnitOfWeightList.Codes.Kilogram);

			var docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.SIMADuty);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, SIMACodes.Codes.C10, 0m, "0.00", string.Empty, 0m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.ExciseTax);
			AssertDocDutyOrTax(docDutyOrTax, ExciseTaxExemptionCodes.Codes.C94, ExciseTaxExemptionCodes.Codes.C94, 0m, string.Empty, string.Empty, 0m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.GST);
			AssertDocDutyOrTax(docDutyOrTax, "5.0", string.Empty, 5m, "5.0", RateTypes.Codes.AdValorem, 50m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 0);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 10m, "10.00", RateTypes.Codes.Specific, 50m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 0.00075m, "0.00075", RateTypes.Codes.Specific, 50m, UnitOfWeightList.Codes.Gram);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 2);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 0.00075m, "0.00075", RateTypes.Codes.AdValorem, 50m, UnitOfWeightList.Codes.Kilogram);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 10m, "10.00", RateTypes.Codes.Specific, 150m, UnitOfWeightList.Codes.Gram);

			invoiceLine.DutiesAndTaxes.DeleteAll();
			AddDutyOrTax(invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, string.Empty, 10m, RateTypes.Codes.Specific, 50m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.ExciseTax);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 0m, string.Empty, string.Empty, 0m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 0);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 10m, "10.00", RateTypes.Codes.Specific, 50m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 0m, "0.00", string.Empty, 0m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty, 2);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 0m, "0.00", string.Empty, 0m, string.Empty);

			docDutyOrTax = DocDutyOrTax.New(Factory, invoiceLine, DutyAndTaxTypes.Codes.CustomsDuty);
			AssertDocDutyOrTax(docDutyOrTax, string.Empty, string.Empty, 10m, "10.00", RateTypes.Codes.Specific, 50m, string.Empty);
		}

		internal static void AddDutyOrTax(JobComInvoiceLine invoiceLine, string type, string exemptCode, decimal rate, string rateType, decimal amount, string uom)
		{
			var dutyOrTax = invoiceLine.DutiesAndTaxes.AddNew();
			dutyOrTax.C1_TaxType = type;
			dutyOrTax.C1_Override = true;
			dutyOrTax.C1_ExemptCode = exemptCode;
			dutyOrTax.C1_Rate = rate;
			dutyOrTax.C1_RateType = rateType;
			dutyOrTax.C1_UnitOfMeasure = uom;
			dutyOrTax.C1_Amount = amount;
		}

		internal static void AssertDocDutyOrTax(DocDutyOrTax docDutyOrTax, string exemptCodeOrRateFormatted, string exemptCode, decimal rate, string rateFormatted, string rateType, decimal amount, string uom)
		{
			AssertEquals("ExemptCodeOrRateFormatted", exemptCodeOrRateFormatted, docDutyOrTax.ExemptCodeOrRateFormatted);
			AssertEquals("ExemptCode", exemptCode, docDutyOrTax.ExemptCode);
			AssertEquals("Rate", rate, docDutyOrTax.Rate);
			AssertEquals("RateFormatted", rateFormatted, docDutyOrTax.RateFormatted);
			AssertEquals("RateType", rateType, docDutyOrTax.RateType);
			AssertEquals("UnitOfMeasure", uom, docDutyOrTax.UnitOfMeasure);
			AssertEquals("Amount", amount, docDutyOrTax.Amount);
		}

		#region Overrides of DocumentWrapperTestCase

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new[] { CreateDocumentWrapperFromStaticNewMethod() };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocDutyOrTax.New(Factory, Factory.New<JobComInvoiceLine>(), DutyAndTaxTypes.Codes.SIMADuty);
		}

		#endregion
	}
}
