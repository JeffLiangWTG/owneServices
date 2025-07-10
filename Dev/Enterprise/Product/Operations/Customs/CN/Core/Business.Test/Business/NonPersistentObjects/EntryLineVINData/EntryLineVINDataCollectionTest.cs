using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryLineVINDataCollection))]
	class EntryLineVINDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryLineVINDataCollection>
	{
		public void TestAllows()
		{
			var testCollection = GetCollectionToTest();
			Assert("Should not allow new", !testCollection.AllowNew);
			Assert("Should not allow remove", !testCollection.AllowRemove);
		}

		public void TestLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV0001";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.BillOfLadingDate = new ZDateTime(2020, 8, 6);
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_LinePrice = 10000;
			invoiceLine1.JI_InvoiceQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_LinePrice = 5000;
			invoiceLine2.JI_InvoiceQuantity = 50;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			var pq1 = invoiceLine1.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "408";
			pq1.CSI_ReferenceNumber = "001";
			pq1.CSI_LineNo = 1;
			pq1.CSI_Quantity = 1;
			pq1.CSI_UnitOfQuantity = "010";
			var pq2 = invoiceLine2.CIQProductQualifications.AddNew();
			pq2.CSI_Code = "408";
			pq2.CSI_ReferenceNumber = "001";
			pq2.CSI_LineNo = 1;
			pq2.CSI_Quantity = 2;
			pq2.CSI_UnitOfQuantity = "010";
			invoiceLine1.VINDataCollection.AddNew();
			invoiceLine1.VINDataCollection.AddNew();
			invoiceLine2.VINDataCollection.AddNew();
			var vins = ((EntryLineProductQualification)entryLine.ProductQualifications.First()).VINs;
			AssertEquals("Count of VINs", 3, vins.Count);
			var vin = vins.First() as EntryLineVINData;
			AssertEquals("BillOfLadingDate", new ZDateTime(2020, 8, 6), vin.BillOfLadingDate);
			AssertEquals("InvoiceNumber", "INV0001", vin.InvoiceNumber);
			AssertEquals("InvoiceQuantity", 150m, vin.InvoiceQuantity);
			AssertEquals("UnitPrice", 100m, vin.UnitPrice);
			entryLine.ClearCachedMergedData();
			invoiceLine1.JI_InvoiceQuantity = 0;
			invoiceLine1.JI_TradeQuantity = 10;
			invoiceLine1.JI_TradeUnitQty = "003";
			invoiceLine2.JI_TradeQuantity = 5;
			invoiceLine2.JI_TradeUnitQty = "003";
			vins = ((EntryLineProductQualification)entryLine.ProductQualifications.First()).VINs;
			AssertEquals("Count of VINs", 3, vins.Count);
			vin = vins.First() as EntryLineVINData;
			AssertEquals("InvoiceQuantity", 15m, vin.InvoiceQuantity);
			AssertEquals("UnitPrice", 1000m, vin.UnitPrice);
		}

		protected override EntryLineVINDataCollection GetCollectionToTest() => new EntryLineVINDataCollection(new EntryLineProductQualification(Factory.New<CusEntryLine>(), new[] { Factory.New<CIQProductQualification>() }, 1));

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EntryLineVINData(new EntryLineProductQualification(Factory.New<CusEntryLine>(), new[] { Factory.New<CIQProductQualification>() }, 1), Factory.New<VINData>());
	}
}
