using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APCreditNoteLine))]
	public class APCreditNoteLineTest : InvoicingLineBaseTest
	{
		protected override Type MasterHeaderType
		{
			get { return typeof(APCreditNote); }
		}

		public void TestReopenClosedJobWithAPCreditNoteLine()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("00001");
			Job job = TestObjectCreator.CreateJob(shipment);

			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			apCreditNote.AH_TransactionNum = "CN001";
			apCreditNote.AH_JH = job.PK;

			var apLine = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.AUD, 1M, 100M);
			apLine.AL_JH = job.PK;

			TestObjectCreator.CreateCharge(apLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Env.Security.ReopenJob.IsAllowed = false;

			job.JH_Status = JobHeaderStatus.Closed.Code;

			Factory.Save();
			AssertEquals("[ReopenJob Allowed] Job Status set to CLS", JobHeaderStatus.Closed.Code, apLine.Job.JH_Status);

			apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			apCreditNote.AH_TransactionNum = "CN002";
			apCreditNote.AH_JH = job.PK;

			apLine = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.AUD, 1M, 100M);
			apLine.AL_JH = job.PK;

			TestObjectCreator.CreateCharge(apLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Env.Security.ReopenJob.IsAllowed = true;

			job.JH_Status = JobHeaderStatus.Closed.Code;

			Factory.Save();
			AssertEquals("[ReopenJob Allowed] Job Status set to WRK", JobHeaderStatus.Working.Code, apLine.Job.JH_Status);
		}

		public void TestAL_ExchangeRate_ReadOnly()
		{
			APCreditNote.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Line.AL_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			APCreditNote.AH_PostedToEFT = false;
			AssertEquals(true, Line.AL_ExchangeRateInfo.ReadOnly);
			APCreditNote.AH_PostedToEFT = true;
			AssertEquals(false, Line.AL_ExchangeRateInfo.ReadOnly);
			Env.Security.AllowAPCreditNoteLineExchangeRateOverride.IsAllowed = false;
			AssertEquals(true, Line.AL_ExchangeRateInfo.ReadOnly);
			Env.Security.AllowAPCreditNoteLineExchangeRateOverride.IsAllowed = true;
			AssertEquals(false, Line.AL_ExchangeRateInfo.ReadOnly);
		}

		public void TestValidationForIncompleteTransactionLine()
		{
			var invoiceLine = (InvoicingLineBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());

			invoiceLine.Factory.SetContext(BusinessContext.SavingIncompleteTransaction);
			AssertEquals("Validation Type for Incomplete", typeof(IncompleteInvoicingLineBaseValidation), invoiceLine.Validation.GetType());

			invoiceLine.Factory.RemoveContext(BusinessContext.SavingIncompleteTransaction);
			AssertNotEquals("Validation Type for Regular", typeof(IncompleteInvoicingLineBaseValidation), invoiceLine.Validation.GetType());
		}

		public override void TestSetDefaultAL_JH()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(newFactory);
			ForwardingShipment shipment = testObjectCreator.CreateShipment("S00001234");
			Job job = testObjectCreator.CreateJob(shipment);

			newFactory.Save();

			InvoicingLine.InvoiceBase.SetIsReversing(true);
			InvoicingLine.InvoiceBase.SubmittedFromInvoicingForm = false;
			InvoicingLine.SetDefaultAL_JH(job.PK);
			Assert("Generic Job on APCreditNoteLine should not be set", InvoicingLine.AL_JH.IsEmpty);

			InvoicingLine.InvoiceBase.SetIsReversing(false);
			InvoicingLine.SetDefaultAL_JH(job.PK);
			Assert("Generic Job on APCreditNoteLine should not be set", InvoicingLine.AL_JH.IsEmpty);

			InvoicingLine.InvoiceBase.SetIsReversing(true);
			InvoicingLine.InvoiceBase.SubmittedFromInvoicingForm = true;
			InvoicingLine.SetDefaultAL_JH(job.PK);
			Assert("Generic Job on APCreditNoteLine should not be set", InvoicingLine.AL_JH.IsEmpty);

			InvoicingLine.InvoiceBase.SetIsReversing(false);
			InvoicingLine.SetDefaultAL_JH(job.PK);
			AssertEquals("Generic Job on APCreditNoteLine should be set", job.PK, InvoicingLine.AL_JH);
		}

		public override void TestDefaultPreviousLineJobToNextLine()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ForwardingShipment shipment = newFactory.NewWithValidTestData<ForwardingShipment>();
			Job job = new TestObjectCreator(newFactory).CreateJob(shipment);
			newFactory.Save();

			InvoicingBase invBase = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
			invBase.SubmittedFromInvoicingForm = false;
			invBase.SetIsReversing(true);
			InvoicingLineBase invLine = invBase.Lines.AddNew() as InvoicingLineBase;
			invLine.AL_JH = job.PK;

			invBase.Lines.AddNew();
			Assert("AL_JH should not be copied to the next line", invBase.Lines[1].AL_JH.IsEmpty);

			invBase.Lines.Remove(invBase.Lines[1]);
			invBase.SubmittedFromInvoicingForm = true;
			invBase.Lines.AddNew();
			Assert("AL_JH should not be copied to the next line", invBase.Lines[1].AL_JH.IsEmpty);

			invBase.Lines.Remove(invBase.Lines[1]);
			invBase.SetIsReversing(false);
			invBase.SubmittedFromInvoicingForm = false;
			invBase.Lines.AddNew();
			Assert("AL_JH should not be copied to the next line", invBase.Lines[1].AL_JH.IsEmpty);

			invBase.Lines.Remove(invBase.Lines[1]);
			invBase.SubmittedFromInvoicingForm = true;
			invBase.Lines.AddNew();
			AssertEquals("AL_JH should be copied to the next line", job.PK, invBase.Lines[1].AL_JH);
		}

		public void TestJobReopenNoSequrityRight()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);

			var objectCreator = new TestObjectCreator(Factory);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = objectCreator.CreateJob(shipment);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote));
			var invLine = apCreditNote.Lines.AddNew() as InvoicingLineBase;
			invLine.AL_JH = job.PK;
			invLine.AL_AC = objectCreator.CC1.PK;
			objectCreator.CreateJobCharge(invLine, job, objectCreator.CC1, objectCreator.AUD);
			Factory.Save();

			AssertEquals("Job should be reopened", JobHeaderStatus.Working.Code, job.JH_Status);
		}

		#region Implementation

		APCreditNote APCreditNote
		{
			get { return (APCreditNote)APLine.MasterTransactionHeader; }
		}

		APCreditNoteLine APLine
		{
			get { return (APCreditNoteLine)Line; }
		}

		#endregion
	}
}
