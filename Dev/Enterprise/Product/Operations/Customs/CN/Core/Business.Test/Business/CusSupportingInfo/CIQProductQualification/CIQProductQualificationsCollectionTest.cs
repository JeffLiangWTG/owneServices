using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CIQProductQualificationCollection))]
	class CIQProductQualificationsCollectionTest : CusSupportingInfoCollectionTest<CIQProductQualification>
	{
		public void TestCIQProductQualificationsMergeKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();
			var pq2 = invoiceLine.CIQProductQualifications.AddNew();
			pq2.CSI_Code = "PQ2";
			pq2.CSI_ReferenceNumber = "NUM2";
			pq2.CSI_UnitOfQuantity = "UQ";
			pq2.CSI_LineNo = 02;
			var pq3 = invoiceLine.CIQProductQualifications.AddNew();
			pq3.CSI_Code = "PQ3";
			pq3.CSI_ReferenceNumber = "NUM3";
			pq3.CSI_UnitOfQuantity = "UQ";
			pq3.CSI_LineNo = 03;
			var pq1 = invoiceLine.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "PQ1";
			pq1.CSI_ReferenceNumber = "NUM1";
			pq1.CSI_UnitOfQuantity = "UQ";
			pq1.CSI_LineNo = 01;
			AssertEquals("PQ1|NUM1|1|UQ|PQ2|NUM2|2|UQ|PQ3|NUM3|3|UQ", invoiceLine.CIQProductQualifications.MergeKey);
		}

		public void TestIsProvidedAny()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			var pq1 = invoiceLine.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "PQ1";
			pq1.CSI_ReferenceNumber = "NUM1";
			pq1.CSI_UnitOfQuantity = "UQ";
			pq1.CSI_LineNo = 01;
			var pq2 = invoiceLine.CIQProductQualifications.AddNew();
			pq2.CSI_Code = "PQ2";
			pq2.CSI_ReferenceNumber = "NUM2";
			pq2.CSI_UnitOfQuantity = "UQ";
			pq2.CSI_LineNo = 02;
			var pq3 = invoiceLine.CIQProductQualifications.AddNew();
			pq3.CSI_Code = "PQ3";
			pq3.CSI_ReferenceNumber = "";
			pq3.CSI_UnitOfQuantity = "UQ";
			pq3.CSI_LineNo = 03;
			Assert(invoiceLine.CIQProductQualifications.IsProvidedAny("PQ1"));
			Assert(invoiceLine.CIQProductQualifications.IsProvidedAny("PQ2", "PQ4"));
			Assert(!invoiceLine.CIQProductQualifications.IsProvidedAny("PQ3"));
			Assert(!invoiceLine.CIQProductQualifications.IsProvidedAny("PQ4", "PQ5"));
		}

		protected override CusSupportingInfoCollection<CIQProductQualification> GetCusSupportingInfoCollection()
		{
			return new CIQProductQualificationCollection(Factory.New<JobComInvoiceLine>());
		}
	}
}
