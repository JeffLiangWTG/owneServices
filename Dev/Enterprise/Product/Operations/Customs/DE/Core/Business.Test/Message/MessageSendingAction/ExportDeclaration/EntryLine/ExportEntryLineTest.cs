using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEntryLine))]
	class ExportEntryLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues() => Assert("Default ShouldSend", exportEntryLine.ShouldSend);

		public void TestLineNumber_Caption()
		{
			AssertEquals("Entry Line No.", DataBoundResourceStrings.GetDataForProperty(exportEntryLine.LineNumberInfo).Caption);
		}

		public void TestLineNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals((ZShort)2, exportEntryLine.LineNumber);
		}

		public void TestTariff()
		{
			invoiceLine.JI_Tariff = "12345";
			AssertEquals("12345", exportEntryLine.Tariff);
		}

		public void TestDescription()
		{
			invoiceLine.JI_Description = "Test Description";
			AssertEquals("Test Description", exportEntryLine.Description);
		}

		protected override BusinessObject GetNewBusinessObject() => exportEntryLine;

		protected override void SetUp()
		{
			base.SetUp();
			var deceleration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = deceleration.PK;
			invoiceLine = deceleration.Invoices.AddNew().InvoiceLines.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			exportEntryLine = new ExportEntryLine(entryLine);
		}
		CusEntryLine entryLine;
		JobComInvoiceLine invoiceLine;
		ExportEntryLine exportEntryLine;
	}
}
