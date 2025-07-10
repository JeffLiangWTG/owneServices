using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class StmScheduleTaskRecipientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestS6_EmailFaxOverrideErrorMessageTellsYouIfFaxNumberRequired()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertNoErrors(Recipient.S6_FaxOverrideInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertHasError(Recipient.S6_FaxOverrideInfo, StmScheduleTaskRecipientValidation.ErrorMessageEnterFaxNumber);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors(Recipient.S6_FaxOverrideInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			AssertNoErrors(Recipient.S6_FaxOverrideInfo);
		}

		public void TestIfSendEmailNotificationSelectedMakeSureThereIsAnEmailAddressEntered()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			Recipient.S6_EmailToRecipientsAsString = "";
			AssertHasError(Recipient.S6_EmailToRecipientsAsStringInfo, StmScheduleTaskRecipientValidation.ErrorMessageMustHaveEmailAddressForNotification);

			Recipient.S6_EmailToRecipientsAsString = "ben@sampol.org";
			AssertNoErrors(Recipient.S6_EmailToRecipientsAsStringInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors(Recipient.S6_EmailToRecipientsAsStringInfo);
			Recipient.S6_EmailToRecipientsAsString = "";
			AssertHasErrors(Recipient.S6_EmailToRecipientsAsStringInfo);
		}

		public void TestValidateS6_EmptyReportDeliveryOptions()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendReport;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = "";
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = "ABC";
			AssertHasError(Recipient.S6_EmptyReportDeliveryOptionsInfo, "Enter a valid Empty Report Contingency.");

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendReport;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = "";
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = "ABC";
			AssertHasError(Recipient.S6_EmptyReportDeliveryOptionsInfo, "Enter a valid Empty Report Contingency.");
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertHasError(Recipient.S6_EmptyReportDeliveryOptionsInfo, StmScheduleTaskRecipientValidation.ErrorMessageCannotUseEmailNotificationOnFaxDelivery);
			Recipient.S6_EmptyReportDeliveryOptions = "";
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendReport;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
			Recipient.S6_EmptyReportDeliveryOptions = "ABC";
			AssertHasError(Recipient.S6_EmptyReportDeliveryOptionsInfo, "Enter a valid Empty Report Contingency.");
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			AssertHasError(Recipient.S6_EmptyReportDeliveryOptionsInfo, StmScheduleTaskRecipientValidation.ErrorMessageCannotUseEmailNotificationOnFaxDelivery);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors(Recipient.S6_EmptyReportDeliveryOptionsInfo);
		}

		public void TestValidateFaxOverride()
		{
			StmScheduleTaskRecipient recipient = Factory.New<StmScheduleTaskRecipient>();
			ZString currentHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact contact = CreateContact(organisation, "contact", "test@test.com", "80973181193");

			recipient.S6_OC = contact.PK;
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient.Validation.ValidateS6_FaxOverride();
			AssertNoErrors(recipient.S6_FaxOverrideInfo);

			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			try
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
				recipient.S6_FaxOverride = "+380973181193,+380444147305,+380672203601";
				AssertNoErrors("fax has right syntax", recipient.S6_FaxOverrideInfo);

				recipient.S6_FaxOverride = "+38o973I8II93,+380444147305,+380672203601";
				AssertHasError(recipient.S6_FaxOverrideInfo, "Phone/Fax/Mobile numbers can only contain number characters, +, -, (, ) and spaces.");
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentHomePort;
			}

			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			recipient.S6_FaxOverride = ZString.Empty;
			AssertNoErrors(recipient.S6_FaxOverrideInfo);
		}

		public void TestValidateEmailToWithMultipleEmailAddressConcatenated()
		{
			var recipient = Factory.New<StmScheduleTaskRecipient>();

			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_EmailToRecipientsAsString = "test2@test.com, test3@test.com";
			AssertNoErrors(recipient.S6_EmailToRecipientsAsStringInfo);

			recipient.S6_EmailToRecipientsAsString = "test2@test.com,test3@test.com";
			AssertNoErrors(recipient.S6_EmailToRecipientsAsStringInfo);

			recipient.S6_EmailToRecipientsAsString = " test2@test.com , test3@test.com ";
			AssertNoErrors(recipient.S6_EmailToRecipientsAsStringInfo);
		}

		public void TestValidateAll()
		{
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			Recipient.DeliveryRecipientType = "x";
			using (Recipient.SuspendValidationTesting())
			{
				Recipient.ClearAllNotifications();
			}

			Recipient.Validation.ValidateAll();
			AssertHasErrors(Recipient.S6_DeliveryMethodInfo);
			AssertHasErrors(Recipient.DeliveryRecipientTypeInfo);
		}

		public void TestValidateS6_AttachmentType()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Recipient.S6_AttachmentType = "____";
			AssertHasError(Recipient.S6_AttachmentTypeInfo, "Enter a valid " + Recipient.S6_AttachmentTypeInfo.Description + ".");

			Recipient.S6_AttachmentType = "";
			AssertHasError(Recipient.S6_AttachmentTypeInfo, "Please enter an " + Recipient.S6_AttachmentTypeInfo.Description + ".");

			Recipient.S6_AttachmentType = OrgCodeLists.AttachmentType_List[0].Code;
			AssertNoErrors(Recipient.S6_AttachmentTypeInfo);
			Recipient.S6_AttachmentType = "HTML";
			AssertNoErrors(Recipient.S6_AttachmentTypeInfo);
			Recipient.S6_AttachmentType = "HTMF";
			AssertNoErrors(Recipient.S6_AttachmentTypeInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;

			Recipient.S6_AttachmentType = OrgCodeLists.AttachmentType_List[0].Code;
			AssertNoErrors(Recipient.S6_AttachmentTypeInfo);
			Recipient.S6_AttachmentType = "HTML";
			AssertHasErrors(Recipient.S6_AttachmentTypeInfo);
			Recipient.S6_AttachmentType = "HTMF";
			AssertHasErrors(Recipient.S6_AttachmentTypeInfo);

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			Recipient.S6_AttachmentType = OrgCodeLists.AttachmentType_List[0].Code;
			AssertNoErrors(Recipient.S6_AttachmentTypeInfo);
			Recipient.S6_AttachmentType = "HTML";
			AssertNoErrors(Recipient.S6_AttachmentTypeInfo);
			Recipient.S6_AttachmentType = "HTMF";
			AssertNoErrors(Recipient.S6_AttachmentTypeInfo);
		}

		public void TestValidateS6_DeliveryMethod()
		{
			Recipient.S6_DeliveryMethod = "!";
			AssertHasError(Recipient.S6_DeliveryMethodInfo, "Enter a valid " + Recipient.S6_DeliveryMethodInfo.Description + ".");

			Recipient.S6_DeliveryMethod = "";
			AssertHasError(Recipient.S6_DeliveryMethodInfo, "Please enter a " + Recipient.S6_DeliveryMethodInfo.Description + ".");

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors(Recipient.S6_DeliveryMethodInfo);

			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@printer.com");
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			Recipient.Validation.ValidateS6_DeliveryMethod();
			AssertNoErrors(Recipient.S6_DeliveryMethodInfo);

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			Recipient.Validation.ValidateS6_DeliveryMethod();
			AssertNoErrors(Recipient.S6_DeliveryMethodInfo);

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			Recipient.Validation.ValidateS6_DeliveryMethod();
			AssertNoErrors(Recipient.S6_DeliveryMethodInfo);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "KLL";
			Recipient.S6_GS_NKRecipient = staff.GS_Code;

			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			Recipient.Validation.ValidateS6_DeliveryMethod();
			AssertHasError(Recipient.S6_DeliveryMethodInfo, "ePrint email address is not defined. This can be set in the registry setting Documents > ePrint Email Address.");

			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@printer.com");
			Recipient.Validation.ValidateS6_DeliveryMethod();
			AssertNoErrors(Recipient.S6_DeliveryMethodInfo);
		}

		public void TestValidateDeliveryRecipientType()
		{
			Recipient.DeliveryRecipientType = "x";
			AssertHasError(Recipient.DeliveryRecipientTypeInfo, "Enter a valid selection.");

			Recipient.DeliveryRecipientType = "";
			AssertHasError(Recipient.DeliveryRecipientTypeInfo, "Please enter a value.");

			Recipient.DeliveryRecipientType = Recipient.Lookups.DeliveryRecipientTypes[0].Description;
			AssertNoErrors(Recipient.DeliveryRecipientTypeInfo);
		}

		public void TestValidateS6_GG()
		{
			GlbGroup group = Factory.New<GlbGroup>();

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;

			Recipient.S6_GG = ZGuid.Empty;
			AssertHasError(Recipient.S6_GGInfo, "Please enter a " + Recipient.S6_GGInfo.Description + ".");

			Recipient.S6_GG = ZGuid.Invalid;
			AssertHasError(Recipient.S6_GGInfo, "Enter a valid " + Recipient.S6_GGInfo.Description + ".");

			Recipient.S6_GG = group.PK;
			AssertNoErrors(Recipient.S6_GGInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Recipient.Validation.ValidateS6_GG();
			AssertHasError(Recipient.S6_GGInfo, "Please select a group with at least one staff that has an email address.");

			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "x@x.com";
			Recipient.Validation.ValidateS6_GG();
			AssertNoErrors(Recipient.S6_GGInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Recipient.Validation.ValidateS6_GG();
			AssertHasError(Recipient.S6_GGInfo, "Please select a group with at least one staff that has a fax number.");

			staff.GS_FaxNum = "xxx";
			Recipient.Validation.ValidateS6_GG();
			AssertNoErrors(Recipient.S6_GGInfo);

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			Recipient.S6_GG = ZGuid.Empty;
			AssertNoErrors(Recipient.S6_GGInfo);
		}

		public void TestValidateS6_GS_NKRecipient()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;

			Recipient.S6_GS_NKRecipient = "";
			AssertHasError(Recipient.S6_GS_NKRecipientInfo, "Please enter a " + Recipient.S6_GS_NKRecipientInfo.Description + ".");

			Recipient.S6_GS_NKRecipient = staff.GS_Code;
			AssertNoErrors(Recipient.S6_GS_NKRecipientInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Recipient.Validation.ValidateS6_GS_NKRecipient();
			AssertHasError(Recipient.S6_GS_NKRecipientInfo, "Please select a staff that has an email address.");

			staff.GS_EmailAddress = "x@x.com";
			Recipient.Validation.ValidateS6_GS_NKRecipient();
			AssertNoErrors(Recipient.S6_GS_NKRecipientInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Recipient.Validation.ValidateS6_GS_NKRecipient();
			AssertHasError(Recipient.S6_GS_NKRecipientInfo, "Please select a staff that has a fax number.");

			staff.GS_FaxNum = "xxx";
			Recipient.Validation.ValidateS6_GS_NKRecipient();
			AssertNoErrors(Recipient.S6_GGInfo);

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			Recipient.S6_GS_NKRecipient = ZString.Empty;
			AssertNoErrors(Recipient.S6_GS_NKRecipientInfo);
		}

		#region Implementation

		OrgContact CreateContact(OrgHeader organisation, string name, string email, string fax)
		{
			OrgContact result = organisation.Contacts.AddNew();
			result.OC_ContactName = name;
			result.OC_Email = email;
			result.OC_Fax = fax;
			return result;
		}

		StmScheduleTaskRecipient Recipient
		{
			get
			{
				if (recipient == null)
				{
					recipient = Factory.New<StmScheduleTaskRecipient>();
				}
				return recipient;
			}
		}
		StmScheduleTaskRecipient recipient;

		#endregion
	}
}
