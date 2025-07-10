using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class LinePreferentialTreatmentProviderTest : Customs.Business.Testing.DataProviderTestCase<LinePreferentialTreatmentProvider>
	{
		public void TestRequestedPreferentialTreatment()
		{
			invoiceLine.JI_PrimaryPreference = "ABC";
			AssertEquals("ABC", Provider.RequestedPreferentialTreatment);
		}

		public void TestContingentNumber()
		{
			invoiceLine.JI_ConcessionOrder = "A123456";
			invoiceLine.ZG_SecondQuota = "B123467";
			AssertContainsExactElementsInAnyOrder(new[] { "2345", "2346" }, Provider.ContingentNumber);
		}

		public void TestQuantity()
		{
			invoiceLine.ZG_QuotaQty = 125;
			invoiceLine.ZG_QuotaUQ = "KGM1";

			CombineAssertions(() =>
			{
				var amount = Provider.Quantity;
				AssertEquals(125m, amount.Quantity);
				AssertEquals("KGM", amount.MeasurementUnit);
				AssertEquals("1", amount.Qualifier);
			});
		}

		public void TestQuantity_Zero()
		{
			invoiceLine.ZG_QuotaQty = 0;
			invoiceLine.ZG_QuotaUQ = "KGM1";
			AssertNull(Provider.Quantity);
		}

		protected override LinePreferentialTreatmentProvider GetProvider() => dataProvider;

		protected override IEnumerable<Expression<Func<LinePreferentialTreatmentProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Quantity;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.Charges.AddNew();

			dataProvider = new LinePreferentialTreatmentProvider(invoiceLine);
		}
		JobDeclaration declaration;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
		LinePreferentialTreatmentProvider dataProvider;
	}
}
