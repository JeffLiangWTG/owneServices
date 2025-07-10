using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APCreditNoteLineCollection))]
	public class APCreditNoteLineCollectionTest : InvoicingLineBaseCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<APCreditNote>();
			AssertNotNull(
@"This is just to initialize 'Lines' collection before any lines are created to prevent loading them in it later as side effect of calling bizo properties.
Such 'accidental', from test position, 'Lines' collection load run some collection code that is interfere with test expectations.",
				parent.Lines);
			return new APCreditNoteLineCollection(parent);
		}

		protected new APCreditNoteLineCollection Collection
		{
			get { return base.Collection as APCreditNoteLineCollection; }
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
