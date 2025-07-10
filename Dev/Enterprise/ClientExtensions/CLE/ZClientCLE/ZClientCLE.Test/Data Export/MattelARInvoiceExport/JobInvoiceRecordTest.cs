using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	sealed class JobInvoiceRecordTest : TestCaseWithFactory
	{
		public void TestJobInvoiceRecordContent()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = declaration.PK;
			header.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			header.JH_GB = Env.CurrentBranch.PK;
			var invoices = new InvoicingBase[1];
			invoices[0] = Factory.NewWithValidTestData<ARInvoice>();
			invoices[0].AH_JH = header.PK;
			var record = new JobInvoiceRecord(shipment, declaration, invoices);
			AssertEquals("Record's Declaration", declaration.PK, record.Declaration.PK);
			AssertEquals("Record' Shipment", shipment.PK, record.Shipment.PK);
			AssertEquals("Num of Invoices", invoices.Length, record.Invoices.Count);
			AssertEquals("Invoice", invoices[0].PK, record.Invoices[0].PK);
		}
	}
}
