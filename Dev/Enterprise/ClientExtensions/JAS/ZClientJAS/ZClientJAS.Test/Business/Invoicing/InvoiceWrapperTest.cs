using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Invoicing.Testing
{
	[TestedType(typeof(InvoiceWrapper))]
	internal class InvoiceWrapperTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_NullInvoicingBase()
		{
			JASInvoicingBaseForTest jASInvoicingBase = Factory.New<JASInvoicingBaseForTest>();
			InvoiceWrapper wrapper = new InvoiceWrapper(jASInvoicingBase);
		}

		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Invoice, InvoiceWrapper.JASInvoicingBase);
			AssertEquals("Should be assigned in the constructor", Invoice, InvoiceWrapper.Invoice);
			AssertEquals("Should be assigned in the constructor", InvoiceWrapper.JASInvoicingBase.InvoicingBase, InvoiceWrapper.Invoice);
			Assert("Should be registered as editable child object", InvoiceWrapper.IsRegisteredEditableChildObject(Invoice));
		}

		public void TestHumanReadableName()
		{
			AssertEquals(Invoice.HumanReadableName, InvoiceWrapper.HumanReadableName);
		}

		#region IJXCExportHeader
		public void TestSendingForwarder()
		{
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy, InvoiceWrapper.SendingForwarder);
		}

		public void TestReceivingForwarder()
		{
			AssertNull("Pre-condition", InvoiceWrapper.ReceivingForwarder);
			JASOrgHeader header = Factory.New<JASOrgHeader>();
			header.OH_Code = "ORG1";
			Invoice.AH_OH = header.PK;
			AssertEquals(Invoice.Header, InvoiceWrapper.ReceivingForwarder);
		}

		public void TestFreightDest()
		{
			AssertEquals("Pre-condition", "", InvoiceWrapper.FreightDest);
			JobHeader job = CreateNewShipmentJob();
			Invoice.AH_JH = job.PK;
			InvoiceWrapper.Shipment.JS_RL_NKDestination = "USATL";
			AssertEquals("USATL", InvoiceWrapper.FreightDest);
		}

		public void TestFreightDest_NonShipmentJob()
		{
			JobHeader nonShipmentJob = Factory.NewJobForTesting<JobHeader>();
			Invoice.AH_JH = nonShipmentJob.PK;
			JASOrgHeader header = Factory.New<JASOrgHeader>();
			header.OH_Code = "ORG1";
			Invoice.AH_OH = header.PK;
			Invoice.Header.OH_RL_NKClosestPort = "DEFRA";
			AssertEquals("DEFRA", InvoiceWrapper.FreightDest);
		}

		#endregion
		#region TestShipment
		public void TestShipment_InvoiceDoesNotBelongToAnyJob()
		{
			AssertNull("Shipment should be null if not a job related invoice", InvoiceWrapper.Shipment);
			Invoice.Lines.AddNew(typeof(ARInvoiceLine));
			AssertNull("Shipment should be null if not a job related invoice", InvoiceWrapper.Shipment);
		}

		public void TestShipment_InvoiceBelongsToSingleJob()
		{
			JobHeader job = CreateNewShipmentJob();
			Invoice.AH_JH = job.PK;
			AssertEquals("Shipment should be loaded from the foreign key", job.JH_ParentID, InvoiceWrapper.Shipment.PK);
			InvoiceWrapper.fShipment = null;
			ARInvoiceLine line1 = CreateNewInvoiceLine(Invoice, job);
			ARInvoiceLine line2 = CreateNewInvoiceLine(Invoice, job);
			AssertEquals("Invoice lines belong to the same shipment job, should load the shipment from Invoice header", job.JH_ParentID, InvoiceWrapper.Shipment.PK);
		}

		public void TestShipment_InvoiceBelongsToSingleNonShipmentJob()
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = Factory.New(typeof(JASForwardingConsol)).PK;
			job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Invoice.AH_JH = job.PK;
			AssertNull("Should be null, not a shipment job", InvoiceWrapper.Shipment);
			InvoiceWrapper.fShipment = null;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertNull("Should be null, Shipment PK is not valid", InvoiceWrapper.Shipment);
		}

		public void TestShipment_InvoiceBelongsToMultipleJobs()
		{
			JobHeader job1 = CreateNewShipmentJob();
			JobHeader job2 = CreateNewShipmentJob();
			ARInvoiceLine line1 = CreateNewInvoiceLine(Invoice, null);
			ARInvoiceLine line2 = CreateNewInvoiceLine(Invoice, job1);
			ARInvoiceLine line3 = CreateNewInvoiceLine(Invoice, job2);
			AssertEquals("Should get the shipment from the first line with job", job1.JH_ParentID, InvoiceWrapper.Shipment.PK);
			InvoiceWrapper.fShipment = null;
			Invoice.Lines.RemoveAndDelete(line2);
			AssertEquals("Should get the shipment from the first line with job", job2.JH_ParentID, InvoiceWrapper.Shipment.PK);
		}

		public void TestShipment_InvoiceBelongsToMultipleJobsAndNoLinesWithShipmentJob()
		{
			JobHeader job1 = Factory.NewJobForTesting<JobHeader>();
			JobHeader job2 = Factory.NewJobForTesting<JobHeader>();
			ARInvoiceLine line1 = CreateNewInvoiceLine(Invoice, null);
			ARInvoiceLine line2 = CreateNewInvoiceLine(Invoice, job1);
			ARInvoiceLine line3 = CreateNewInvoiceLine(Invoice, job2);
			ARInvoiceLine line4 = CreateNewInvoiceLine(Invoice, null);
			Assert("Sanity check", Invoice.IsBelongToMultipleJobs);
			AssertNull("There is no shipment job associated with this invoice", InvoiceWrapper.Shipment);
		}

		public void TestShipment_ConsolInvoice()
		{
			JobHeader job = CreateNewShipmentJob();
			ARInvoiceLine line = CreateNewInvoiceLine(Invoice, job);
			Invoice.AH_JH = ZGuid.Empty;
			Invoice.AH_ConsolidatedInvoiceRef = "C001928";
			Assert("Sanity check", Invoice.IsConsolInvoice);
			AssertEquals("Should get the Shipment from the Invoice Line", job.JH_ParentID, InvoiceWrapper.Shipment.PK);
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceWrapper(Invoice);
		}

		ARInvoiceLine CreateNewInvoiceLine(ARInvoice invoice, JobHeader job)
		{
			ARInvoiceLine result = (ARInvoiceLine)invoice.Lines.AddNew(typeof(ARInvoiceLine));
			if (job != null)
			{
				result.AL_JH = job.PK;
			}

			return result;
		}

		JobHeader CreateNewShipmentJob()
		{
			JobHeader result = Factory.NewJobForTesting<JobHeader>();
			result.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			result.JH_ParentID = Factory.New(typeof(JASForwardingShipment)).PK;
			return result;
		}

		InvoiceWrapper InvoiceWrapper
		{
			get
			{
				if (fInvoiceWrapper == null)
				{
					fInvoiceWrapper = new InvoiceWrapper(Invoice);
				}

				return fInvoiceWrapper;
			}
		}

		JASARInvoice Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = Factory.New<JASARInvoice>();
				}

				return fInvoice;
			}
		}

		InvoiceWrapper fInvoiceWrapper;
		JASARInvoice fInvoice;
		ZGuid initialProxyOrgPK;
		protected override void SetUp()
		{
			initialProxyOrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "1234567890123456789012345678901234567890";
			org.MainAddress.OA_Address1 = "2345678901234567890123456789012345678901";
			org.MainAddress.OA_Address2 = "3456789012345678901234567890123456789012";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = initialProxyOrgPK;
		}
		#endregion
	}
}
