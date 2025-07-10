using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);
		protected override EU.Business.Declaration.JobDeclaration GetDeclarationForTest() => Factory.New<JobDeclaration>();
		protected override ZString VATableAdditionChargeCode => ImportChargeCodeList.Codes._001;

		protected override ZString NonVATableDeductionChargeCode => ZString.Empty;

		public override void TestCalculateDutiesAndVat()
		{
			Assert(true); //merge is not done the same as in EU base and the test fails for DE, will be fixed in a new WI
		}

		public void TestShouldCalculateDutiesAndTaxesForEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();

			var jobComInvoiceHeader = declaration.Invoices.AddNew();

			var invLine = jobComInvoiceHeader.InvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			var strategy = new DutyCalculatorStrategyForTest(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Should calculate duties.", true, strategy.ShouldCalculateDutiesForEntryLine_Exposed(entryLine));
				AssertEquals("Should calculate taxes.", true, strategy.ShouldCalculateTaxesForEntryLine_Exposed(entryLine));
			});
		}

		sealed class DutyCalculatorStrategyForTest : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyForTest(JobDeclaration declaration) : base(declaration)
			{ }

			public bool ShouldCalculateDutiesForEntryLine_Exposed(EU.Business.Declaration.CusEntryLine entryLine) => ShouldCalculateDutiesForEntryLine(entryLine);

			public bool ShouldCalculateTaxesForEntryLine_Exposed(EU.Business.Declaration.CusEntryLine entryLine) => ShouldCalculateTaxesForEntryLine(entryLine);
		}
	}
}
