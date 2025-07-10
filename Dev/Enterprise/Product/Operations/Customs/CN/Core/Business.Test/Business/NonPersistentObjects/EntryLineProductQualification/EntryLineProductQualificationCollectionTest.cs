using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryLineProductQualificationCollection))]
	class EntryLineProductQualificationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryLineProductQualificationCollection>
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
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			var pq1 = invoiceLine1.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "106";
			pq1.CSI_ReferenceNumber = "001";
			pq1.CSI_LineNo = 1;
			pq1.CSI_Quantity = 1;
			pq1.CSI_UnitOfQuantity = "010";
			var pq2 = invoiceLine2.CIQProductQualifications.AddNew();
			pq2.CSI_Code = "106";
			pq2.CSI_ReferenceNumber = "001";
			pq2.CSI_LineNo = 1;
			pq2.CSI_Quantity = 2;
			pq2.CSI_UnitOfQuantity = "010";
			AssertEquals(1, entryLine.ProductQualifications.Count());
			AssertEquals((ZInt)1, entryLine.ProductQualifications.First().Sequence);
			AssertEquals("106:001/1/3 010", entryLine.ProductQualifications.JoinAsString());
			var pq3 = invoiceLine2.CIQProductQualifications.AddNew();
			pq3.CSI_Code = "107";
			pq3.CSI_ReferenceNumber = "002";
			pq3.CSI_LineNo = 2;
			pq3.CSI_Quantity = 1;
			pq3.CSI_UnitOfQuantity = "010";
			entryLine.ClearCachedMergedData();
			AssertEquals(2, entryLine.ProductQualifications.Count());
			AssertEquals((ZInt)1, entryLine.ProductQualifications.First().Sequence);
			AssertEquals((ZInt)2, entryLine.ProductQualifications.Last().Sequence);
			AssertEquals("106:001/1/3 010,107:002/2/1 010", entryLine.ProductQualifications.JoinAsString());
		}

		protected override EntryLineProductQualificationCollection GetCollectionToTest() => new EntryLineProductQualificationCollection(Factory.New<CusEntryLine>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EntryLineProductQualification(Factory.New<CusEntryLine>(), new[] { Factory.New<CIQProductQualification>() }, 1);
	}
}
