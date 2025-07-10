using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryLineUniversalRate))]
	class EntryLineUniversalRateTest : EntryLineUniversalRateAbstractTest<EntryLineUniversalRate>
	{
		public void TestEntryLineUniversalRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine1 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CustomsUnitQty = "035";
			invoiceLine1.JI_CustomsQuantity = 22m;
			invoiceLine1.JI_CustomsSecondUnitQty = "001";
			invoiceLine1.JI_CustomsSecondQuantity = 33m;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CustomsUnitQty = "035";
			invoiceLine2.JI_CustomsQuantity = 21m;
			invoiceLine2.JI_CustomsSecondUnitQty = "001";
			invoiceLine2.JI_CustomsSecondQuantity = 12m;
			entryLine.CL_CustomsValue = 100.21m;
			var universalRate = new EntryLineUniversalRate(entryLine);
			universalRate.CustomsValueFormula = "CV / 2";
			AssertEquals("CustomsValue", 100.21m, universalRate.CustomsValue);
			AssertEquals("ValueForDuty", 50.11m, universalRate.ValueForDuty);
			universalRate.CustomsValueFormula = "CV * 2";
			AssertEquals("CustomsValue", 100.21m, universalRate.CustomsValue);
			AssertEquals("ValueForDuty", 200.42m, universalRate.ValueForDuty);
			AssertEquals(43m, universalRate.UnitOfMeasureValueList.GetValue("035"));
			AssertEquals(45m, universalRate.UnitOfMeasureValueList.GetValue("001"));
		}

		protected override EntryLineUniversalRate GetEntryLineUniversalRate()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			return new EntryLineUniversalRate(entryLine);
		}
	}
}
