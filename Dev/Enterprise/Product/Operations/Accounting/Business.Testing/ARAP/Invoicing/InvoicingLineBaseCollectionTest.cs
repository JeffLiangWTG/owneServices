using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingLineBaseCollection))]
	public class InvoicingLineBaseCollectionTest : DependentTransactionLineCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoicingLineBaseCollection((InvoicingBase)Factory.New(typeof(APInvoice)));
		}

		public void TestSetGSTReadOnlyStateForAllLines()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			InvoicingBase invoicingBase = (InvoicingBase)Factory.New(typeof(APInvoice));
			InvoicingLineBaseCollection testCollection = new InvoicingLineBaseCollection(invoicingBase);

			InvoicingLineBase testLine1 = (InvoicingLineBase)Factory.New(typeof(APInvoiceLine));
			InvoicingLineBase testLine2 = (InvoicingLineBase)Factory.New(typeof(APInvoiceLine));

			testCollection.Add(testLine1);
			testCollection.Add(testLine2);

			testCollection.SetGSTReadOnlyStateForAllLines();

			Assert(testCollection[0].AL_ATInfo.ReadOnly);
			Assert(testCollection[1].AL_ATInfo.ReadOnly);
		}

		public void TestCalculateGSTForAllLines()
		{
			InvoicingBase invoicingBase = (InvoicingBase)Factory.New(typeof(APInvoice));
			InvoicingLineBaseCollection testCollection = new InvoicingLineBaseCollection(invoicingBase);

			InvoicingLineBase testLine1 = (InvoicingLineBase)Factory.New(typeof(APInvoiceLine));
			InvoicingLineBase testLine2 = (InvoicingLineBase)Factory.New(typeof(APInvoiceLine));

			testLine1.AL_AT = ZGuid.NewZGuid();
			testLine2.AL_AT = ZGuid.NewZGuid();

			testLine1.AL_AW = ZGuid.NewZGuid();
			testLine2.AL_AW = ZGuid.NewZGuid();

			testLine2.ApportionmentChargeImportedFrom = Factory.New<ApportionSplitCharge>();

			testCollection.Add(testLine1);
			testCollection.Add(testLine2);

			invoicingBase.SetIsProxyingHeaderValuesForTestOnly(true);
			testCollection.CalculateGSTAndWHTForAllLines();
			Assert("Tax reset to empty as org or company is not gst registered", testCollection[0].AL_AT.IsEmpty);
			Assert("Tax recalculation not done since line is attached to apportioned charge", !testCollection[1].AL_AT.IsEmpty);

			Assert("WHT reset to empty as org or company is not gst registered", testCollection[0].AL_AW.IsEmpty);
			Assert("WHT is not reset since line is attached to apportioned charge", !testCollection[1].AL_AW.IsEmpty);
		}

		public void TestRaiseMutexError()
		{
			MutexErrorRaised = false;
			InvoicingLineBaseCollection invoicingLines = (InvoicingLineBaseCollection)GetCollectionToTest();
			invoicingLines.MutexError += new MutexErrorEventHandler(InvoicingLines_MutexError);
			invoicingLines.RaiseMutexError((InvoicingLineBase)invoicingLines.AddNew(), "MUTEX ERROR");
			Assert("Should have raised Mutex Error", MutexErrorRaised);
		}

		bool MutexErrorRaised;

		void InvoicingLines_MutexError(InvoicingLineBase invoiceLine, MutexErrorEventArgs e)
		{
			MutexErrorRaised = true;
			AssertEquals("Mutex Error Message", "MUTEX ERROR", e.ErrorMessage);
		}

		public virtual void TestDefaultPreviousLineJobForNewChild()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new TestObjectCreator(Factory).CreateJob(shipment);
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();   // since InvoicingLineBase is abstract use ARInvoice
			aRInv.SetIsReversing(false);
			aRInv.SubmittedFromInvoicingForm = true;
			InvoicingLineBaseCollection lines = new InvoicingLineBaseCollection(aRInv);
			ARInvoiceLine line1 = lines.AddNew() as ARInvoiceLine;
			line1.AL_JH = job.PK;

			ARInvoiceLine line2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			lines.SetDefaultsForNewChild_ForTestOnly(line2);
			Assert("Generic Job on Line2 should be empty", line2.AL_JH.IsEmpty);
		}

		public void TestSetDefaultsForNewChildSetPeriodApportionmentMethodToDEF()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			InvoicingLineBaseCollection lines = new InvoicingLineBaseCollection(aRInv);
			InvoicingLineBase line = (InvoicingLineBase)lines.AddNew();
			AssertEquals("DEF", line.PeriodApportionmentMethod);
		}

		public void TestSetDefaultsForNewChildPopulateSuspendedValidation()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();   // since InvoicingLineBase is abstract use ARInvoice
			InvoicingLineBaseCollection lines = new InvoicingLineBaseCollection(aRInv);
			lines.SuspendValidation();
			Assert(lines.IsValidationSuspended);

			InvoicingLineBase line = (InvoicingLineBase)lines.AddNew();
			Assert("New line's validation should be suspended", line.IsValidationSuspended);
		}

		public void TestAllowNew()
		{
			InvoicingBase testInvoicingBase = Factory.New<UAInvoice>();
			testInvoicingBase.AH_TransactionNum = "000001";
			Factory.Save();
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			InvoicingLineBaseCollection testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(testcollection.AllowNew);

			testInvoicingBase = converter.ConvertToAP(testInvoicingBase, false);

			testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(!testcollection.AllowNew);

			testInvoicingBase = Factory.New<APInvoice>();
			testInvoicingBase.AH_TransactionNum = "000002";
			testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(testcollection.AllowNew);

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan);
			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			testInvoicingBase = Factory.New<ARCreditNote>();
			testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(testcollection.AllowNew);

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			testInvoicingBase.AH_TransactionBelongsToGroup = arInvoice.PK;
			Assert(!testcollection.AllowNew);

			arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			testInvoicingBase = Factory.New<ARCreditNote>();
			testInvoicingBase.OriginalTransactionReference = arInvoice.PK;
			testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(!testcollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			InvoicingBase testInvoicingBase = Factory.New<UAInvoice>();
			testInvoicingBase.AH_TransactionNum = "000001";
			Factory.Save();
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			InvoicingLineBaseCollection testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(testcollection.AllowRemove);

			testInvoicingBase = converter.ConvertToAP(testInvoicingBase, false);

			testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(!testcollection.AllowRemove);

			testInvoicingBase = Factory.New<APInvoice>();
			testcollection = new InvoicingLineBaseCollection(testInvoicingBase);
			Assert(testcollection.AllowRemove);
		}
	}
}
