using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	sealed class UPEDocumentAutoDeliveryTest_ForCoreFunctionality : UPEDocumentAutoDeliveryTest
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestCantDeliverWhenDocumentSupportableNotInDB()
		{
			Callout unsavedCusHAWB = Factory.New<Callout>();
			TestUPEDocumentAutoDelivery delivery = new TestUPEDocumentAutoDelivery(unsavedCusHAWB);
			delivery.Deliver();
		}

		public void TestAutoEmailDocument_FromContactOrgDocument()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			DeliveryContact.OC_Email = "clinton@edi.com.au";
			Factory.Save();
			Delivery.Deliver();
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("Document should be emailed automatically", 1, printJobs.Length);
			AssertEquals("Email address should be set correctly", "clinton@edi.com.au", printJobs[0].SP_Destination);
			AssertEquals("Job type should be email", Core.Constants.ContactNotifyModes.Email, printJobs[0].SP_JobType);
		}

		public void TestAutoFaxDocument_FromContactOrgDocument()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			DeliveryContact.OC_Fax = "+61280012200";
			Factory.Save();
			Delivery.Deliver();
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("Document should be faxed automatically", 1, printJobs.Length);
			AssertEquals("Fax number should be set correctly", "+61280012200", printJobs[0].SP_Destination);
			AssertEquals("Job type should be fax", Core.Constants.ContactNotifyModes.Fax, printJobs[0].SP_JobType);
		}

		public void TestAutoPrintDocument_FromContactOrgDocument()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;
			Factory.Save();
			Delivery.Deliver();
			UPEPrintBatchItem printItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(ExpectedPrintBatchType, DocumentSupportable.Identifier, ExpectedDocumentCommand.PK);
			AssertNotNull("Document should be queued for batch print now", printItem);
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("No documents should be physically printed", 0, printJobs.Length);
		}

		public void TestAutoBatchPrintDocument_IfNoOtherDeliveryMethodFound()
		{
			OrgDocument.Delete();
			DeliveryContact.OC_Title = "";
			DeliveryContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			Factory.Save();
			CusHAWB.HasChanges = true;
			Delivery.Deliver();
			AssertEquals("The CusHAWB factory should not be used to save the print batch item", true, CusHAWB.HasChanges);
			UPEPrintBatchItem printItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(ExpectedPrintBatchType, DocumentSupportable.Identifier, ExpectedDocumentCommand.PK);
			AssertNotNull("Document should not queued for batch print now", printItem);
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("No documents should be physically printed", 0, printJobs.Length);
		}

		public void TestAutoBatchPrintDocument_IfNoDeliveryContactFound()
		{
			DeliveryOrganisation.Delete();
			CusHAWB.CS_OA_ConsigneeAddress = ZGuid.Empty;
			Factory.Save();
			Delivery.Deliver();
			UPEPrintBatchItem printItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(ExpectedPrintBatchType, DocumentSupportable.Identifier, ExpectedDocumentCommand.PK);
			AssertNotNull("Document should be queued for batch print now", printItem);
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("No documents should be physically printed", 0, printJobs.Length);
		}

		public void TestAutoBatchPrintDocument_UnlessPrintBatchTypeIsEmpty()
		{
			DeliveryOrganisation.Delete();
			CusHAWB.CS_OA_ConsigneeAddress = ZGuid.Empty;
			Factory.Save();
			Delivery.SetPrintBatchType("");
			Delivery.Deliver();
			UPEPrintBatchItem[] printBatchItems = (UPEPrintBatchItem[])Factory.Load(typeof(UPEPrintBatchItem), new ZQuery());
			AssertEquals("Document should not be queued for batch print now", 0, printBatchItems.Length);
		}

		#region Abstract Overrides
		UPECusHAWB CusHAWB
		{
			get
			{
				return (UPECusHAWB)base.DocumentSupportable;
			}
		}

		protected override UPEDocumentAutoDelivery NewDocumentAutoDelivery()
		{
			return new TestUPEDocumentAutoDelivery(CusHAWB);
		}

		protected override IUPEDocumentSupportable NewDocumentSupportable()
		{
			UPECusHAWB result = Factory.NewWithValidTestData<UPECusHAWB>();
			result.CS_OA_ConsigneeAddress = DeliveryOrganisation.MainAddress.PK;
			return result;
		}

		protected override DocumentCommand ExpectedDocumentCommand
		{
			get
			{
				return DocumentLoader.LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			}
		}

		protected override ZString ExpectedPrintBatchType
		{
			get
			{
				return UPEPrintBatchTypes.Codes.ShipmentHeldLetter;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailSubject
		{
			get
			{
				return "Delivery failure for test document";
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailBody
		{
			get
			{
				return @"
Delivery instructions incomplete for Consignee Customer Notification; Generated 11-Nov-05 00:00:00

HAWB=xxx

Error: DeliveryAddress: Please enter a Fax Number.
";
			}
		}

		#endregion
		#region Test Classes
		class TestUPEDocumentAutoDelivery : UPEDocumentAutoDelivery
		{
			public TestUPEDocumentAutoDelivery(UPECusHAWB cusHAWB) : base(cusHAWB)
			{
			}

			public DocumentCommand DocumentCommandForTest
			{
				get
				{
					if (fDocumentCommandForTest == null)
					{
						fDocumentCommandForTest = new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee);
					}

					return fDocumentCommandForTest;
				}
			}

			DocumentCommand fDocumentCommandForTest;
			protected override DocumentCommand DocumentCommand
			{
				get
				{
					return DocumentCommandForTest;
				}
			}

			public void SetPrintBatchType(ZString value)
			{
				fPrintBatchType = value;
			}

			protected override ZString PrintBatchType
			{
				get
				{
					return fPrintBatchType ?? UPEPrintBatchTypes.Codes.ShipmentHeldLetter;
				}
			}

			string fPrintBatchType;
			protected override string DeliveryFailureEmailSubject
			{
				get
				{
					return "Delivery failure for test document";
				}
			}

			protected override string DeliveryFailureDocumentDetails
			{
				get
				{
					return "HAWB=xxx";
				}
			}
		}

		#endregion
		#region Implementation
		new TestUPEDocumentAutoDelivery Delivery
		{
			get
			{
				return (TestUPEDocumentAutoDelivery)base.Delivery;
			}
		}
		#endregion
	}
}
