using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(LineMerger))]
	sealed class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		public void TestGetNewDutyCalculatorStrategy()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var lineMerger = new LineMergerForTesting(jobDeclaration);

			var dutyCalculatorStrategy = lineMerger.GetNewDutyCalculatorStrategy_Expose();

			AssertNotNull("DutyCalculatorStrategy should not be null", dutyCalculatorStrategy);
			AssertType<DutyCalculatorStrategy>("DutyCalculatorStrategy should be of type DutyCalculatorStrategy", dutyCalculatorStrategy);
		}

		public void TestInvoiceAmountAndCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(3m, entryLine.CL_InvoiceAmount);
			AssertEquals("EUR", entryLine.CL_RX_NKInvoiceAmountCurrency);
		}

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

		protected override bool AllowDeleteEntryLineForRegistedEntry => false;

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);
	}

	sealed class LineMergerForTesting : LineMerger
	{
		public LineMergerForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		internal IDutyCalculatorStrategy GetNewDutyCalculatorStrategy_Expose()
			=> base.GetNewDutyCalculatorStrategy();
	}
}
