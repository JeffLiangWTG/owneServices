using System;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class WeightUQCalculatorTest : Customs.Business.Testing.WeightUQCalculatorTest
	{
		protected override Type JobComInvoiceHeaderType
		{
			get { return typeof(JobComInvoiceHeader); }
		}

		protected override Type CusEntryHeaderType
		{
			get { return typeof(CusEntryHeader); }
		}

		public void TestGrossWeightForN30WhereDeclarationAndInvoiceWeightsAreNotExposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_ApplicationCode = "CMR";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 1638.92m;
			invoiceLine.JI_WeightUQ = "T";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(1638.92m, entry.GrossWeight.Amount);
		}

		public void Test6040PackageSplitNature10()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Nature10PackCount = 60;
			invoiceHeader.JZ_BondPackCount = 40;
			invoiceHeader.JZ_Weight = 500m;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_IsPackToBondForLine = true;

			var entryN10 = declaration.ActiveEntryHeaders.AddNew();
			var entryLineN10 = entryN10.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLineN10.PK;

			var entry20 = declaration.ActiveEntryHeaders.AddNew();
			var entryLineN20 = entry20.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLineN20.PK;

			AssertEquals(300m, entryN10.GrossWeight.Amount);
		}

		public void Test6040PackageSplitNature20()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "LEG";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Nature10PackCount = 60;
			invoiceHeader.JZ_BondPackCount = 40;
			invoiceHeader.JZ_Weight = 500m;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_IsPackToBondForLine = true;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			var entryN10 = declaration.ActiveEntryHeaders.AddNew();
			var entryLineN10 = entryN10.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLineN10.PK;

			var entry20 = declaration.ActiveEntryHeaders.AddNew();
			var entryLineN20 = entry20.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLineN20.PK;

			AssertEquals(200m, entry20.GrossWeight.Amount);
		}
	}
}
