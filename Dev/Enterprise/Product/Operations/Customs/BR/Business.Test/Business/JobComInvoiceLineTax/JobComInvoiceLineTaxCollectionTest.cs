using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxCollection))]
	class JobComInvoiceLineTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewWithType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Taxes Collection must contain one tax", 0, invoiceLine.Taxes.Count);

			var tax = invoiceLine.Taxes.AddNew("TST");
			CombineAssertions(() =>
			{
				AssertEquals("Taxes Collection must contain two taxes", 1, invoiceLine.Taxes.Count);
				AssertEquals("Set JLT_Type", "TST", tax.JLT_Type);
				AssertEquals("HasChanges should NOT be suspended", true, tax.HasChanges);
			});
		}

		public void TestFindByType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.Taxes.AddNew("TS1");
			invoiceLine.Taxes.AddNew("TS2");

			AssertEquals("Tax1 must contain JLT_Type = TS1", "TS1", invoiceLine.Taxes.FindByType("TS1").JLT_Type);
			AssertEquals("Tax2 must contain JLT_Type = TS2", "TS2", invoiceLine.Taxes.FindByType("TS2").JLT_Type);
		}

		public void TestFindByTypeAndMethod()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var tax1 = invoiceLine.Taxes.AddNew("TS1", "QPU");
			var tax2 = invoiceLine.Taxes.AddNew("TS1", "RED");
			var tax3 = invoiceLine.Taxes.AddNew("TS2", "QPU");

			AssertEquals(tax1, invoiceLine.Taxes.FindByTypeAndMethod("TS1", "QPU"));
			AssertEquals(tax2, invoiceLine.Taxes.FindByTypeAndMethod("TS1", "RED"));
			AssertEquals(tax3, invoiceLine.Taxes.FindByTypeAndMethod("TS2", "QPU"));
			AssertEquals(null, invoiceLine.Taxes.FindByTypeAndMethod("TS2", "RED"));
		}

		public void TestFindByMethodOfCalculation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.Taxes.AddNew("TS1", "QPU");
			invoiceLine.Taxes.AddNew("TS2", "QPU");
			AssertEquals("Taxes Collection must contain two taxes", 2, invoiceLine.Taxes.Count);

			var taxes = invoiceLine.Taxes.FindByMethodOfCalculation("QPU");

			AssertEquals("Must contain 2 taxes with JLT_MethodOfCalculation = QPU", 2, taxes.Count());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionalTariffCollection = new JobComInvoiceLineTaxCollection(invoiceLine);
			return additionalTariffCollection;
		}
	}
}
