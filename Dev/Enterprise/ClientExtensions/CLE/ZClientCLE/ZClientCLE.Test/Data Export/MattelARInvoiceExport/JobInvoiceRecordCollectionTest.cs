using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public class JobInvoiceRecordCollectionTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			JobInvoiceRecordCollection collection = new JobInvoiceRecordCollection();
			AssertEquals(0, collection.Count);
		}

		public void TestAdd()
		{
			JobInvoiceRecordCollection collection = new JobInvoiceRecordCollection();
			JobInvoiceRecord record = new JobInvoiceRecord(NewShipment, NewDeclaration, NewInvoices);
			collection.Add(record);
			AssertEquals(1, collection.Count);
			JobInvoiceRecord chkRecord = collection[0];
			AssertEquals(record, chkRecord);
			record = new JobInvoiceRecord(NewShipment, NewDeclaration, NewInvoices);
			collection.Add(record);
			AssertEquals(2, collection.Count);
			chkRecord = collection[1];
			AssertEquals(record, chkRecord);
		}

		InvoicingBase[] NewInvoices
		{
			get
			{
				InvoicingBase[] result = new InvoicingBase[1];
				result[0] = Factory.NewWithValidTestData<ARInvoice>();
				return result;
			}
		}

		BaseJobDeclaration NewDeclaration
		{
			get
			{
				return Factory.NewWithValidTestData<BaseJobDeclaration>();
			}
		}

		ForwardingShipment NewShipment
		{
			get
			{
				return Factory.NewWithValidTestData<ForwardingShipment>();
			}
		}
	}
}
