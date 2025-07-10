using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class DocumentTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEADAndELoI()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.ZG_StatisticalValueManualOverride = true;
			invoiceLine1.ZG_StatisticalValue = 1234.56m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.ZG_StatisticalValueManualOverride = true;
			invoiceLine2.ZG_StatisticalValue = 2345.67m;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			var entryLine2 = entry1.AllEntryLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);

			var queue = Factory.New<StmPrintQueue>();
			Factory.Save();

			var docPrinter = new SilentDocumentPrinter(Factory, declaration, "Export Accompanying Doc (EAD) and ELOI", "Customs", "");
			docPrinter.Print(queue.PK, 1, false);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_DocumentName, SQLComparisonOperator.Contains, "Export Accompanying Doc"));

			NUnit.Framework.Assert.That(printJob, NUnit.Framework.Is.Not.EqualTo(default(StmPrintJob)), "The print job was not found, it probably crashed - should not be [null]");
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(new string[] { "{AL}-[1234.56]" }, System.Array.Empty<string>(), printJob);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(new string[] { "{AL}-[2345.67]" }, System.Array.Empty<string>(), printJob);
		}

		[ExpectNoExceptions]
		public void TestEADAndELolWillNotBeUnnecessarilyPaginated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var server1 = Factory.New<StmPrintServer>();
			server1.SPS_ServerName = "S1";
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_QueueName = "Q1";
			queue.SQ_SPS_Server = server1.PK;
			Factory.Save();

			var docPrinter = new SilentDocumentPrinter(Factory, declaration, "Export Accompanying Doc (EAD) and ELOI", "Customs", "");
			docPrinter.Print(queue.PK, 1, false);
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_DocumentName, SQLComparisonOperator.Contains, "Export Accompanying Doc"));

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				using (var stream = new MemoryStream())
				using (var pdfExport = new FlexCelPdfExportSafe(excelInterface.Xls, true))
				{
					pdfExport.Export(stream);
					NUnit.Framework.Assert.That(pdfExport.Progress.TotalPage, NUnit.Framework.Is.EqualTo(1), "EAD and ELol document is not paginated.");
				}
			}

			entryLine.CL_Description =
				"MAKE SURE TO TEST IT USING A GOODS ITEM WITH A VERY LONG DESCRIPTION OF GOODS, TO ENCOURAGE VERTICAL EXPANSDION OF BOX 31/2." +
				"MAKE SURE TO TEST IT USING A GOODS ITEM WITH A VERY LONG DESCRIPTION OF GOODS, TO ENCOURAGE VERTICAL EXPANSDION OF BOX 31/2." +
				"MAKE SURE TO TEST IT USING A GOODS ITEM WITH A VERY LONG DESCRIPTION OF GOODS, TO ENCOURAGE VERTICAL EXPANSDION OF BOX 31/2." +
				"MAKE SURE TO TEST IT USING A GOODS ITEM WITH A VERY LONG DESCRIPTION OF GOODS, TO ENCOURAGE VERTICAL EXPANSDION OF BOX 31/2.";
			var server2 = Factory.New<StmPrintServer>();
			server2.SPS_ServerName = "S2";
			queue = Factory.New<StmPrintQueue>();
			queue.SQ_QueueName = "Q2";
			queue.SQ_SPS_Server = server2.PK;
			Factory.Save();

			docPrinter = new SilentDocumentPrinter(Factory, declaration, "Export Accompanying Doc (EAD) and ELOI", "Customs", "");
			docPrinter.Print(queue.PK, 1, false);
			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_DocumentName, SQLComparisonOperator.Contains, "Export Accompanying Doc"));

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				using (var stream = new MemoryStream())
				using (var pdfExport = new FlexCelPdfExportSafe(excelInterface.Xls, true))
				{
					pdfExport.Export(stream);
					NUnit.Framework.Assert.That(pdfExport.Progress.TotalPage, NUnit.Framework.Is.EqualTo(1), "BOX 31/2 is vertically expanded because of the very long description, EAD and ELol document is still not paginated.");
				}
			}
		}
	}
}
