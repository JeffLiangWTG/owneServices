using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FTAMessageSendingObject))]
	sealed class FTAMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new FTAMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5SC);
		}

		public void TestFTAMessageSendingObjectProperties()
		{
			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "TEST1", "Manu AA");
			var manufacturerAddress = manufacturer.MainAddress;
			manufacturerAddress.OA_Address1 = "Address 1";
			manufacturerAddress.OA_Address2 = "Address 2, 110";
			manufacturerAddress.OA_PostCode = "20011";
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "TEST2", "삼성물산");
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "TEST3", "NIKE Inc.");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime("2022-11-11");
			declaration.JE_RL_NKPortOfLoading = "AUMEL";
			declaration.JE_TransshipmentDate = new ZDateTime("2022-12-12");
			declaration.JE_TransshipmentPort = "JPADA";
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoice.JZ_OH_Supplier = supplier.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_FTARelationArticleCode = "1";
			instruction.CEI_StatementNumber5WN = "030112200237050";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1234522000008M";
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoice.InvoiceLines.AddNew().JI_CL = entryLine.PK;

			var messageSendingObject = new FTAMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals("12345-22-000008M", messageSendingObject.FormattedEntryNumber);
			AssertEquals("1", messageSendingObject.LawCode);
			AssertEquals(new ZDateTime("2022-11-11"), messageSendingObject.DepartureDate);
			AssertEquals("Melbourne", messageSendingObject.DeparturePort);
			AssertEquals("030-11-22-0-023705-0", messageSendingObject.CustomsDisbursementBill);
			AssertEquals("Manu AA", messageSendingObject.ManufacturCompanyName);
			AssertEquals("Address 1 Address 2, 110", messageSendingObject.ManufacturAddress);
			AssertEquals("20011", messageSendingObject.ManufacturPostCode);
			AssertEquals("AU", messageSendingObject.DepartureCountry);
			AssertEquals("Y", messageSendingObject.TransshipmentYN);
			AssertEquals(new ZDateTime("2022-12-12"), messageSendingObject.TransshipmentDate);
			AssertEquals("JP", messageSendingObject.TransshipmentCountry);
			AssertEquals("JPADA", messageSendingObject.TransshipmentPort);
			AssertEquals("삼성물산", messageSendingObject.ImporterCompanyName);
			AssertEquals("NIKE Inc.", messageSendingObject.ExporterCompanyName);
		}

		public void TestDetailsData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_SequenceNumber = 3;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 2;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_SequenceNumber = 1;
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_SequenceNumber = 4;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_FTASequenceNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine3.JI_CL = entryLine1.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			entryLine3.CL_FTASequenceNumber = 2;
			invoiceLine4.JI_CL = entryLine3.PK;

			var sendingObj = new FTAMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals(2, sendingObj.FTALines.Count);
			AssertEquals(3, sendingObj.FTAInvoiceLines.Count);
			AssertEquals((ZShort)1, sendingObj.FTAInvoiceLines[0].EntryLineNo);
			AssertEquals((ZShort)1, sendingObj.FTAInvoiceLines[0].InvoiceLineNo);
			AssertEquals((ZShort)1, sendingObj.FTAInvoiceLines[1].EntryLineNo);
			AssertEquals((ZShort)3, sendingObj.FTAInvoiceLines[1].InvoiceLineNo);
			AssertEquals((ZShort)3, sendingObj.FTAInvoiceLines[2].EntryLineNo);
			AssertEquals((ZShort)4, sendingObj.FTAInvoiceLines[2].InvoiceLineNo);
		}
	}
}
