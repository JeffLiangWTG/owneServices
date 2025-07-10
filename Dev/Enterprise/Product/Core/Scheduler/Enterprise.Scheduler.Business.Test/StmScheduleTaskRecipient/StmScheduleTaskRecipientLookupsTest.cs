using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;

namespace Enterprise.Scheduler.Business.Testing
{
	public class StmScheduleTaskRecipientLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBlankReportActivitiesList()
		{
			AssertEquals("BlankReportActivities.ContainsCode(SendEmailNotification)", true, Recipient.Lookups.BlankReportActivities.ContainsCode(EmptyReportContingencyList.Codes.SendEmailNotification));
			AssertEquals("BlankReportActivities.ContainsCode(SendNothing)", true, Recipient.Lookups.BlankReportActivities.ContainsCode(EmptyReportContingencyList.Codes.SendNothing));
			AssertEquals("BlankReportActivities.ContainsCode(SendReport)", true, Recipient.Lookups.BlankReportActivities.ContainsCode(EmptyReportContingencyList.Codes.SendReport));
		}

		public void TestAttachmentTypes()
		{
			TestAttachmentTypesCore();
		}

		protected virtual void TestAttachmentTypesCore()
		{
			AssertEquals("Count", 8, Recipient.Lookups.AttachmentTypes.Count);
			AssertEquals("[0].Code", OrgConstants.AttachmentType.XLS, Recipient.Lookups.AttachmentTypes[0].Code);
			AssertEquals("[1].Code", OrgConstants.AttachmentType.XLSX, Recipient.Lookups.AttachmentTypes[1].Code);
			AssertEquals("[2].Code", OrgConstants.AttachmentType.PDF, Recipient.Lookups.AttachmentTypes[2].Code);
			AssertEquals("[3].Code", OrgConstants.AttachmentType.PDFA, Recipient.Lookups.AttachmentTypes[3].Code);
			AssertEquals("[4].Code", OrgConstants.AttachmentType.PDFC, Recipient.Lookups.AttachmentTypes[4].Code);
			AssertEquals("[5].Code", OrgConstants.AttachmentType.TIF, Recipient.Lookups.AttachmentTypes[5].Code);
			AssertEquals("[6].Code", OrgConstants.AttachmentType.HTML, Recipient.Lookups.AttachmentTypes[6].Code);
			AssertEquals("[7].Code", OrgConstants.AttachmentType.HTMF, Recipient.Lookups.AttachmentTypes[7].Code);
		}

		public void TestOrganisations()
		{
			AssertEquals("Organisations.GetType()", typeof(OrganisationsFindBoxCollection), Recipient.Lookups.Organisations.GetType());
		}

		public void TestNotifyModes()
		{
			AssertEquals("Count", ExpectedNotifyModes, Recipient.Lookups.NotifyModes.Count);
			AssertEquals("ContainsCode(Core.Constants.ContactNotifyModes.Email)", true, Recipient.Lookups.NotifyModes.ContainsCode(Core.Constants.ContactNotifyModes.Email));
			AssertEquals("ContainsCode(Core.Constants.ContactNotifyModes.EPrint)", true, Recipient.Lookups.NotifyModes.ContainsCode(Core.Constants.ContactNotifyModes.EPrint));
			AssertEquals("ContainsCode(Core.Constants.ContactNotifyModes.Fax)", true, Recipient.Lookups.NotifyModes.ContainsCode(Core.Constants.ContactNotifyModes.Fax));
			AssertEquals("ContainsCode(Core.Constants.ContactNotifyModes.Print)", true, Recipient.Lookups.NotifyModes.ContainsCode(Core.Constants.ContactNotifyModes.Print));
		}

		protected virtual int ExpectedNotifyModes
		{
			get { return 4; }
		}

		public void TestContactNames()
		{
			AssertEquals("Count", 0, Recipient.Lookups.ContactNames.Count);

			OrgHeader organisation1 = Factory.New<OrgHeader>();
			organisation1.Contacts.AddNew().OC_ContactName = "Bob";

			Recipient.S6_OH = organisation1.PK;
			AssertEquals("Count", 1, Recipient.Lookups.ContactNames.Count);
			AssertEquals("[0].Code", "Bob", Recipient.Lookups.ContactNames[0].Code);

			OrgHeader organisation2 = Factory.New<OrgHeader>();
			organisation2.Contacts.AddNew().OC_ContactName = "Peter";
			organisation2.Contacts.AddNew().OC_ContactName = "Jane";

			Recipient.S6_OH = organisation2.PK;
			AssertEquals("Count", 2, Recipient.Lookups.ContactNames.Count);
			AssertEquals("[0].Code", "Peter", Recipient.Lookups.ContactNames[0].Code);
			AssertEquals("[1].Code", "Jane", Recipient.Lookups.ContactNames[1].Code);
		}

		public void TestOnlyActiveContactNames()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			Recipient.S6_OH = org.PK;

			var activeContact = org.Contacts.AddNew();
			activeContact.OC_ContactName = "Active Contact";
			activeContact.OC_IsActive = true;

			var inactiveContact = org.Contacts.AddNew();
			inactiveContact.OC_ContactName = "Inactive Contact";
			inactiveContact.OC_IsActive = false;

			AssertEquals("Should only be one contact", 1, Recipient.Lookups.ContactNames.Count);
			AssertEquals("Should only be active contact", activeContact.OC_ContactName, Recipient.Lookups.ContactNames[0].Code);
		}

		public void TestDeliveryRecipientTypes() => TestDeliveryRecipientTypesCore();
		protected virtual void TestDeliveryRecipientTypesCore()
		{
			AssertEquals("Count", 3, Recipient.Lookups.DeliveryRecipientTypes.Count);
			AssertEquals("GetCodeFromDescription(\"Contact\")", ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Recipient.Lookups.DeliveryRecipientTypes.GetCodeFromDescription("Contact"));
			AssertEquals("GetCodeFromDescription(\"Group\")", ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Recipient.Lookups.DeliveryRecipientTypes.GetCodeFromDescription("Group"));
			AssertEquals("GetCodeFromDescription(\"Staff\")", ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Recipient.Lookups.DeliveryRecipientTypes.GetCodeFromDescription("Staff"));
		}

		#region Implementation

		protected virtual StmScheduleTaskRecipient Recipient
		{
			get
			{
				if (recipient == null)
				{
					recipient = NewStmScheduleTaskRecipient();
				}
				return recipient;
			}
		}
		StmScheduleTaskRecipient recipient;

		protected virtual StmScheduleTaskRecipient NewStmScheduleTaskRecipient()
		{
			return Factory.New<StmScheduleTaskRecipient>();
		}

		#endregion
	}
}
