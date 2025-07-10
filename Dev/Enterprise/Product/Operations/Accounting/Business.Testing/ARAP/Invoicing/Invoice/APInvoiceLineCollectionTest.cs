using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceLineCollection))]
	public class APInvoiceLineCollectionTest : InvoicingLineBaseCollectionTest
	{
		public void TestOnCountChanged_LogWithPAToAPTransactionLineMonitor()
		{
			var collection = GetCollectionToTest() as APInvoiceLineCollection;
			var invoice = (APInvoice)collection.ParentTransactionHeader;
			var monitor = new PAToAPTransactionLineMonitor(invoice);
			Factory.ServiceContainer.AddService(monitor);

			var line1 = CreateInvoiceLine(invoice, NewCurrency, .7M, 100M);

			AssertEquals("Add line1",
@"Last 10 counts of lines:
N/A

Stack trace of last line remove:
N/A

Total calls of adding line: 1
Total calls of removing line: 0", monitor.GetInfo().Trim());

			var line2 = CreateInvoiceLine(invoice, NewCurrency, .7M, 200M);

			AssertEquals("Add line2",
@"Last 10 counts of lines:
N/A

Stack trace of last line remove:
N/A

Total calls of adding line: 2
Total calls of removing line: 0", monitor.GetInfo().Trim());

			invoice.Lines.RemoveAndDelete(line1);

			AssertEquals("Remove line1", true, Enterprise.Accounting.Business.TestObjectCreator.IsContainSubStrings(monitor.GetInfo().Trim(),
@"Last 10 counts of lines:
N/A

Stack trace of last line remove:",
				"at Enterprise.Accounting.Business.PAToAPTransactionLineMonitor.RecordLineItemChange(CollectionCountChangedEventArgs e)",
				"at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBaseCollection.RemoveAndDelete(BusinessObject elementToDelete)",
@"Total calls of adding line: 2
Total calls of removing line: 2"));

			invoice.Lines.RemoveAll();

			AssertEquals("Remove all lines", true, Enterprise.Accounting.Business.TestObjectCreator.IsContainSubStrings(monitor.GetInfo().Trim(),
@"Last 10 counts of lines:
N/A

Stack trace of last line remove:",
				"at Enterprise.Accounting.Business.PAToAPTransactionLineMonitor.RecordLineItemChange(CollectionCountChangedEventArgs e)",
				"at CargoWise.EntityFramework.BusinessObjectCollection.RemoveAll()",
@"Total calls of adding line: 2
Total calls of removing line: 3"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<APInvoice>();
			AssertNotNull(
@"This is just to initialize 'Lines' collection before any lines are created to prevent loading them in it later as side effect of calling bizo properties.
Such 'accidental', from test position, 'Lines' collection load run some collection code that is interfere with test expectations.",
				parent.Lines);
			return new APInvoiceLineCollection(parent);
		}

		public void TestShouldRaiseImportCharges()
		{
			APInvoiceLineCollection lines = Collection;
			Assert("Should not raise charges when event on collection is not hooked", !lines.ShouldShowJobChargesForImport);
			lines.ShowJobChargesForImportEvent += Lines_ShowChargesForImportEvent;
			Assert("Should raise charges when event on collection is not hooked", lines.ShouldShowJobChargesForImport);
		}

		void Lines_ShowChargesForImportEvent(object sender, EventArgs e)
		{
		}

		protected new APInvoiceLineCollection Collection
		{
			get { return base.Collection as APInvoiceLineCollection; }
		}

		public override void TestDefaultPreviousLineJobForNewChild()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new TestObjectCreator(Factory).CreateJob(shipment);
			Factory.Save();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			(Collection.Master as InvoicingBase).SetIsReversing(false);
			(Collection.Master as InvoicingBase).SubmittedFromInvoicingForm = true;

			((IBindingList)Collection).AddNew();
			AssertEquals("There should be 1 element in the collection", 1, Collection.Count);
			Collection[0].AL_JH = job.PK;
			((ICancelAddNew)Collection).EndNew(0);

			((IBindingList)Collection).AddNew();
			AssertEquals("There should be a new line", 2, Collection.Count);
			AssertEquals("The GenericJob on the new line should be the same as on the first line", job.PK, Collection[1].AL_JH);
			Collection[1].HasChanges = true;
			((ICancelAddNew)Collection).EndNew(1);

			((IBindingList)Collection).AddNew();
			Collection[2].AL_AC = chargeCode.PK;
			((ICancelAddNew)Collection).EndNew(2);

			((IBindingList)Collection).Remove(Collection[1]);
			((IBindingList)Collection).AddNew();

			AssertEquals("There should be 3 lines", 3, Collection.Count);
			AssertEquals("The GenericJob on the new line should be the same as on the prior lines", job.PK, Collection[2].AL_JH);

			using (Collection.SuspendListChanged())
			{
				((IBindingList)Collection).AddNew();
			}
			AssertEquals("Collection.Count", 3, Collection.Count);
			AssertEquals("Job on the new line should not be populated from prior line as it was suspended", true, Collection[2].AL_JH.IsEmpty);

			((IBindingList)Collection).AddNew();
			AssertEquals("Collection.Count", 3, Collection.Count);
			AssertEquals("Job on the new line should be populated from prior line as it was not suspended", job.PK, Collection[2].AL_JH);
		}
	}
}
