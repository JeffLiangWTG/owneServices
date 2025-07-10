using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	abstract class UPEDocumentAutoDeliveryTest : TestCaseWithClientSpecificDocuments
	{
		public void TestDeliverByQueueForBatchPrint()
		{
			if (ExpectedPrintBatchType.IsEmpty)
			{
				Assert("Queue for batch print explicitly excluded", true);
			}
			else
			{
				OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;
				Factory.Save();
				Delivery.Deliver();
				OrgHeader organisation = Factory.New<OrgHeader>();
				UPEPrintBatchItem printItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(ExpectedPrintBatchType, DocumentSupportable.Identifier, ExpectedDocumentCommand.PK);
				AssertEquals("Document should be queued with the correct document", ExpectedDocumentCommand.PK, printItem.T6_SU);
				AssertEquals("Document should be queued with the correct print batch type", ExpectedPrintBatchType, printItem.PrintBatch.T7_BatchType);
				StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
				AssertEquals("Document queued for batch print, no documents should be physically printed", 0, printJobs.Length);
			}
		}

		protected abstract UPEDocumentAutoDelivery NewDocumentAutoDelivery();
		protected abstract IUPEDocumentSupportable NewDocumentSupportable();
		protected abstract DocumentCommand ExpectedDocumentCommand { get; }
		protected virtual DocumentCommand ExpectedDocumentCommandForFailureEmail => ExpectedDocumentCommand;

		protected abstract ZString ExpectedPrintBatchType { get; }

		protected abstract ZString ExpectedDeliveryFailureEmailSubject { get; }

		protected abstract ZString ExpectedDeliveryFailureEmailBody { get; }

		#region SendDocumentDeliveryFailureEmail
		[TestDate(2005, 11, 11)]
		public void TestSendDocumentDeliveryFailureEmail()
		{
			GlbStaff failureEmailRecipient = DocumentAutoDeliveryNotificationGroup.Staff.AddNew();
			failureEmailRecipient.GS_EmailAddress = "ted@edi.com.au";
			failureEmailRecipient.GS_Code = "ZAC";
			Factory.Save();
			OrgDocumentForFailureEmail.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			OrgDocumentForFailureEmail.OD_SU_MenuItem = ExpectedDocumentCommandForFailureEmail.PK;
			DeliveryContact.OC_Fax = ""; // invalid delivery details
			Factory.Save();
			Delivery.Deliver();
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("No document should be delivered", 0, printJobs.Length);
			MailItem[] deliveryFailureEmails = LoadInvoiceDeliveryFailureMailItems();
			AssertEquals("There should be 1 delivery failure email for the test", 1, deliveryFailureEmails.Length);
			AssertEquals("Delivery failure email subject should be correct", ExpectedDeliveryFailureEmailSubject, deliveryFailureEmails[0].MI_Subject);
			AssertMultilineASCIIEquals("Delivery failure email body should be correct", ExpectedDeliveryFailureEmailBody.Trim(), deliveryFailureEmails[0].MI_Body.Trim());
		}

		MailItem[] LoadInvoiceDeliveryFailureMailItems()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, "delivery");
			MailItem[] result = (MailItem[])Factory.Load(typeof(MailItem), filter);
			return result;
		}

		#endregion
		#region Test Objects
		#region Delivery
		protected UPEDocumentAutoDelivery Delivery
		{
			get
			{
				if (fDelivery == null)
				{
					TestCaseHelper.RunClientDbCreateScripts();
					fDelivery = NewDocumentAutoDelivery();
				}

				return fDelivery;
			}
		}

		UPEDocumentAutoDelivery fDelivery;
		#endregion
		#region DocumentSupportable
		protected IUPEDocumentSupportable DocumentSupportable
		{
			get
			{
				if (fDocumentSupportable == null)
				{
					fDocumentSupportable = NewDocumentSupportable();
				}

				return fDocumentSupportable;
			}
		}

		IUPEDocumentSupportable fDocumentSupportable;
		#endregion
		#region DeliveryOrganisation
		protected UPEOrgHeader DeliveryOrganisation
		{
			get
			{
				if (fDeliveryOrganisation == null)
				{
					fDeliveryOrganisation = (UPEOrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
					fDeliveryOrganisation.OH_FullName = "Delivery Org";
					fDeliveryOrganisation.OH_Code = "DLVORG";
				}

				return fDeliveryOrganisation;
			}
		}

		UPEOrgHeader fDeliveryOrganisation;
		#endregion
		#region DeliveryContact
		protected OrgContact DeliveryContact
		{
			get
			{
				if (fDeliveryContact == null)
				{
					fDeliveryContact = DeliveryOrganisation.Contacts.AddNew();
					fDeliveryContact.OC_ContactName = "InvoiceReceivingChief";
				}

				return fDeliveryContact;
			}
		}

		OrgContact fDeliveryContact;
		#endregion
		#region OrgDocument
		protected OrgDocument OrgDocument
		{
			get
			{
				if (fOrgDocument == null)
				{
					fOrgDocument = DeliveryContact.Documents.AddNew();
					fOrgDocument.OD_SU_MenuItem = ExpectedDocumentCommand.PK;
				}

				return fOrgDocument;
			}
		}

		OrgDocument fOrgDocument;

		protected OrgDocument OrgDocumentForFailureEmail
		{
			get
			{
				if (fOrgDocumentForFailureEmail == null)
				{
					fOrgDocumentForFailureEmail = DeliveryContact.Documents.AddNew();
					fOrgDocumentForFailureEmail.OD_SU_MenuItem = ExpectedDocumentCommandForFailureEmail.PK;
				}

				return fOrgDocumentForFailureEmail;
			}
		}

		OrgDocument fOrgDocumentForFailureEmail;
		#endregion
		#region DocumentLoader
		protected UPEDocumentMenuItemLoader DocumentLoader
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory);
			}
		}

		#endregion
		#region DocumentAutoDeliveryNotificationGroup
		protected GlbGroup DocumentAutoDeliveryNotificationGroup
		{
			get
			{
				GlbGroup result = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, UPEDataRegistry.Instance.DocumentAutoDeliveryNotificationGroup.Value);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<GlbGroup>();
					result.GG_Code = UPEDataRegistry.Instance.DocumentAutoDeliveryNotificationGroup.Value;
				}

				return result;
			}
		}

		#endregion
		#region FailureEmailRecipient
		protected GlbStaff FailureEmailRecipient
		{
			get
			{
				if (fFailureEmailRecipient == null)
				{
					fFailureEmailRecipient = DocumentAutoDeliveryNotificationGroup.Staff.AddNew();
					fFailureEmailRecipient.GS_EmailAddress = "bob@edi.com.au";
					fFailureEmailRecipient.GS_Code = "ZAC";
				}

				return fFailureEmailRecipient;
			}
		}

		GlbStaff fFailureEmailRecipient;
		#endregion
		#endregion

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			IDocumentSupportable loadedDocumentSupportable = DocumentSupportable;
		}
		#endregion
	}
}
