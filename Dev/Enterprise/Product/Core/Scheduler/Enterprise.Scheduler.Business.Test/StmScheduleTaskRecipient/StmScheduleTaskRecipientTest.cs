using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmScheduleTaskRecipient))]
	public class StmScheduleTaskRecipientTest : EnterpriseBusinessObjectTestCase
	{
		public void TestS6_FaxOverride_ReadOnlyWhenDeliveryMethodIsPrint()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			AssertEquals("S6_FaxOverride.ReadOnly for SendNothing", true, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			AssertEquals("S6_FaxOverride.ReadOnly for SendEmailNotification", true, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendReport;
			AssertEquals("S6_FaxOverride.ReadOnly for SendReport", true, Recipient.S6_FaxOverrideInfo.ReadOnly);
		}

		public void TestS6_FaxOverride_ReadOnlyWhenDeliveryMethodIsEmail()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			AssertEquals("S6_FaxOverride.ReadOnly for SendNothing", true, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			AssertEquals("S6_FaxOverride.ReadOnly for SendEmailNotification", true, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendReport;
			AssertEquals("S6_FaxOverride.ReadOnly for SendReport", true, Recipient.S6_FaxOverrideInfo.ReadOnly);
		}

		public void TestDeliveryAddressWithEmailNotificationPicksUpTheEmailAddress()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_Email = "butter@cookies.com";
			contact.OC_Fax = "xxx";

			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff staff1 = group.Staff.AddNew();
			GlbStaff staff2 = group.Staff.AddNew();
			GlbStaff staff3 = group.Staff.AddNew();

			staff1.GS_Code = "XXX";
			staff1.GS_EmailAddress = "tim@tam.com";
			staff1.GS_FaxNum = "yyy";
			staff3.GS_Code = "YYY";
			staff2.GS_EmailAddress = "danish@cookies.com";
			staff2.GS_FaxNum = "zzz";
			staff3.GS_Code = "ZZZ";

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "", "", "");

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "butter@cookies.com", "tim@tam.com, danish@cookies.com", "tim@tam.com");

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "", "", "");

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendReport;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "", "", "");
		}

		public void TestAttachmentType()
		{
			Recipient.S6_DeliveryMethod = "";
			AssertEquals("S6_AttachmentTypeInfo.ReadOnly", true, Recipient.S6_AttachmentTypeInfo.ReadOnly);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("S6_AttachmentTypeInfo.ReadOnly", false, Recipient.S6_AttachmentTypeInfo.ReadOnly);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			AssertEquals("S6_AttachmentTypeInfo.ReadOnly", false, Recipient.S6_AttachmentTypeInfo.ReadOnly);

			Recipient.S6_AttachmentType = "XYZ";
			Recipient.S6_DeliveryMethod = "";
			AssertEquals("S6_AttachmentType", "", Recipient.S6_AttachmentType);
		}

		public void TestRecipientFieldsReadOnly()
		{
			AssertEquals("S6_GGInfo.ReadOnly", true, Recipient.S6_GGInfo.ReadOnly);
			AssertEquals("S6_GS_NKRecipientInfo.ReadOnly", true, Recipient.S6_GS_NKRecipientInfo.ReadOnly);
			AssertEquals("S6_OHInfo.ReadOnly", true, Recipient.S6_OHInfo.ReadOnly);
			AssertEquals("ContactNameInfo.ReadOnly", true, Recipient.ContactNameInfo.ReadOnly);

			Recipient.DeliveryRecipientType = "Contact";
			AssertEquals("S6_GGInfo.ReadOnly", true, Recipient.S6_GGInfo.ReadOnly);
			AssertEquals("S6_GS_NKRecipientInfo.ReadOnly", true, Recipient.S6_GS_NKRecipientInfo.ReadOnly);
			AssertEquals("S6_OHInfo.ReadOnly", false, Recipient.S6_OHInfo.ReadOnly);
			AssertEquals("ContactNameInfo.ReadOnly", false, Recipient.ContactNameInfo.ReadOnly);

			Recipient.DeliveryRecipientType = "Staff";
			AssertEquals("S6_GGInfo.ReadOnly", true, Recipient.S6_GGInfo.ReadOnly);
			AssertEquals("S6_GS_NKRecipientInfo.ReadOnly", false, Recipient.S6_GS_NKRecipientInfo.ReadOnly);
			AssertEquals("S6_OHInfo.ReadOnly", true, Recipient.S6_OHInfo.ReadOnly);
			AssertEquals("ContactNameInfo.ReadOnly", true, Recipient.ContactNameInfo.ReadOnly);

			Recipient.DeliveryRecipientType = "Group";
			AssertEquals("S6_GGInfo.ReadOnly", false, Recipient.S6_GGInfo.ReadOnly);
			AssertEquals("CheckS6_GS_NKRecipientInfo.ReadOnly", true, Recipient.S6_GS_NKRecipientInfo.ReadOnly);
			AssertEquals("S6_OHInfo.ReadOnly", true, Recipient.S6_OHInfo.ReadOnly);
			AssertEquals("ContactNameInfo.ReadOnly", true, Recipient.ContactNameInfo.ReadOnly);
		}

		public void TestContactName()
		{
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;

			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Some Org";

			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Zubin";

			Recipient.ContactName = "Hello";
			AssertEquals("S6_OC", ZGuid.Empty, Recipient.S6_OC);

			Recipient.S6_OH = organisation.PK;
			Recipient.ContactName = "Mary";
			AssertEquals("S6_OC", ZGuid.Empty, Recipient.S6_OC);

			Recipient.ContactName = "Zubin";
			AssertEquals("S6_OC", contact.PK, Recipient.S6_OC);
		}

		public void TestOrganisationName()
		{
			AssertEquals("OrganisationName", "", Recipient.OrganisationName);

			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "RRRRRRR";

			Recipient.S6_OH = organisation.PK;
			AssertEquals("OrganisationName", "RRRRRRR", Recipient.OrganisationName);
		}

		public void TestStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "S1";
			staff.GS_FullName = "Staff1";

			Factory.Save();

			ZGuid orgPk = ZGuid.NewZGuid();
			Recipient.DeliveryRecipientType = "x";
			Recipient.S6_OH = orgPk;
			Recipient.S6_GS_NKRecipient = staff.GS_Code;
			Recipient.ContactName = "Bob";

			AssertEquals("Staff1", Recipient.Staff.GS_FullName);
		}

		public void TestDeliveryRecipientType()
		{
			ZGuid oh = ZGuid.NewZGuid();
			ZString gs = "XXX";
			ZGuid gg = ZGuid.NewZGuid();

			Recipient.DeliveryRecipientType = "x";
			Recipient.S6_OH = oh;
			Recipient.S6_GS_NKRecipient = gs;
			Recipient.S6_GG = gg;
			Recipient.ContactName = "Bob";

			AssertEquals("DeliveryRecipientType", "x", Recipient.DeliveryRecipientType);
			AssertEquals("S6_DeliveryToType", "", Recipient.S6_DeliveryToType);
			AssertEquals("S6_OH", oh, Recipient.S6_OH);
			AssertEquals("S6_GS_NKRecipient", gs, Recipient.S6_GS_NKRecipient);
			AssertEquals("S6_GG", gg, Recipient.S6_GG);
			AssertEquals("ContactName", "Bob", Recipient.ContactName);

			Recipient.DeliveryRecipientType = "Contact";
			AssertEquals("DeliveryRecipientType", "Contact", Recipient.DeliveryRecipientType);
			AssertEquals("S6_DeliveryToType", ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Recipient.S6_DeliveryToType);
			AssertEquals("S6_OH", oh, Recipient.S6_OH);
			AssertEquals("S6_GS_NKRecipient", ZString.Empty, Recipient.S6_GS_NKRecipient);
			AssertEquals("S6_GG", ZGuid.Empty, Recipient.S6_GG);
			AssertEquals("ContactName", "Bob", Recipient.ContactName);

			Recipient.S6_GS_NKRecipient = gs;
			Recipient.S6_GG = gg;
			Recipient.DeliveryRecipientType = "Staff";
			AssertEquals("DeliveryRecipientType", "Staff", Recipient.DeliveryRecipientType);
			AssertEquals("S6_DeliveryToType", ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Recipient.S6_DeliveryToType);
			AssertEquals("S6_OH", ZGuid.Empty, Recipient.S6_OH);
			AssertEquals("S6_GS_NKRecipient", gs, Recipient.S6_GS_NKRecipient);
			AssertEquals("S6_GG", ZGuid.Empty, Recipient.S6_GG);
			AssertEquals("ContactName", "", Recipient.ContactName);

			Recipient.S6_OH = oh;
			Recipient.S6_GG = gg;
			Recipient.ContactName = "Bob";
			Recipient.DeliveryRecipientType = "Group";
			AssertEquals("DeliveryRecipientType", "Group", Recipient.DeliveryRecipientType);
			AssertEquals("S6_DeliveryToType", ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Recipient.S6_DeliveryToType);
			AssertEquals("S6_OH", ZGuid.Empty, Recipient.S6_OH);
			AssertEquals("S6_GS_NKRecipient", ZString.Empty, Recipient.S6_GS_NKRecipient);
			AssertEquals("S6_GG", gg, Recipient.S6_GG);
			AssertEquals("ContactName", "", Recipient.ContactName);
		}

		public void TestS6_SQ()
		{
			ZGuid sq = ZGuid.NewZGuid();
			Recipient.S6_SQ = sq;
			AssertEquals("S6_SQInfo.ReadOnly", true, Recipient.S6_SQInfo.ReadOnly);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("S6_SQInfo.ReadOnly", false, Recipient.S6_SQInfo.ReadOnly);
			AssertEquals("S6_SQ", sq, Recipient.S6_SQ);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("S6_SQInfo.ReadOnly", true, Recipient.S6_SQInfo.ReadOnly);
			AssertEquals("S6_SQ", ZGuid.Empty, Recipient.S6_SQ);
		}

		public void TestS6_FaxOverride()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("S6_EmailFaxOverrideInfo.ReadOnly when Fax", false, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("S6_EmailFaxOverrideInfo.ReadOnly when Print", true, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("S6_EmailFaxOverrideInfo.ReadOnly when Email", true, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_DeliveryMethod = "";
			AssertEquals("S6_EmailFaxOverrideInfo.ReadOnly when (empty)", true, Recipient.S6_FaxOverrideInfo.ReadOnly);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("Cleared", ZString.Empty, Recipient.S6_FaxOverride);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Recipient.S6_FaxOverride = "23235";

			Recipient.S6_DeliveryMethod = ZString.Empty;
			AssertEquals("Cleared", ZString.Empty, Recipient.S6_FaxOverride);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Recipient.S6_FaxOverride = "23235";

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Cleared", ZString.Empty, Recipient.S6_FaxOverride);
		}

		public void TestDeliveryAddress()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_Email = "butter@cookies.com";
			contact.OC_Fax = "xxx";

			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff staff1 = group.Staff.AddNew();
			GlbStaff staff2 = group.Staff.AddNew();
			GlbStaff staff3 = group.Staff.AddNew();

			staff1.GS_Code = "XXX";
			staff1.GS_EmailAddress = "tim@tam.com";
			staff1.GS_FaxNum = "yyy";
			staff3.GS_Code = "YYY";
			staff2.GS_EmailAddress = "danish@cookies.com";
			staff2.GS_FaxNum = "zzz";
			staff3.GS_Code = "ZZZ";

			var ePrinterAddress = "email@printer.com";
			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrinterAddress);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "butter@cookies.com", "tim@tam.com, danish@cookies.com", "tim@tam.com");

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, ePrinterAddress, ePrinterAddress, ePrinterAddress);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "xxx", "yyy, zzz", "yyy");

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "", "", "");

			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "butter@cookies.com", "tim@tam.com, danish@cookies.com", "tim@tam.com");

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			TestDeliveryAddress(organisation.PK, contact.PK, group.PK, staff1.GS_Code, "butter@cookies.com", "tim@tam.com, danish@cookies.com", "tim@tam.com");
		}

		void TestDeliveryAddress(ZGuid orgPK, ZGuid contactPK, ZGuid groupPK, ZString staffNK, string contactValue, string groupValue, string staffValue)
		{
			var isEPrint = (Recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.EPrint);

			var expectedDeliveryAddressWhenNoContactSelected = (isEPrint ? contactValue : "");
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			Recipient.S6_OH = orgPK;
			AssertEquals("DeliveryAddress", expectedDeliveryAddressWhenNoContactSelected, Recipient.DeliveryAddress);

			Recipient.S6_OC = contactPK;
			AssertEquals("DeliveryAddress", contactValue, Recipient.DeliveryAddress);

			var expectedDeliveryAddressWhenNoGroupSelected = (isEPrint ? groupValue : "");
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			AssertEquals("DeliveryAddress", expectedDeliveryAddressWhenNoGroupSelected, Recipient.DeliveryAddress);

			Recipient.S6_GG = groupPK;
			AssertEquals("DeliveryAddress", groupValue, Recipient.DeliveryAddress);

			var expectedDeliveryAddressWhenNoStaffSelected = (isEPrint ? staffValue : "");
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			AssertEquals("DeliveryAddress", expectedDeliveryAddressWhenNoStaffSelected, Recipient.DeliveryAddress);

			recipient.S6_GS_NKRecipient = staffNK;
			AssertEquals("DeliveryAddress", staffValue, Recipient.DeliveryAddress);
		}

		public void TestContacts()
		{
			AssertEquals("Count", 0, Recipient.Contacts.Count);

			OrgHeader organisation1 = Factory.New<OrgHeader>();
			OrgContact contact1 = organisation1.Contacts.AddNew();

			Recipient.S6_OH = organisation1.PK;
			AssertEquals("Count", 1, Recipient.Contacts.Count);
			AssertEquals("[0].PK", contact1.PK, Recipient.Contacts[0].PK);

			OrgHeader organisation2 = Factory.New<OrgHeader>();
			OrgContact contact2 = organisation2.Contacts.AddNew();
			OrgContact contact3 = organisation2.Contacts.AddNew();

			Recipient.S6_OH = organisation2.PK;
			AssertEquals("Count", 2, Recipient.Lookups.ContactNames.Count);
			AssertEquals("[0].PK", contact2.PK, Recipient.Contacts[0].PK);
			AssertEquals("[1].PK", contact3.PK, Recipient.Contacts[1].PK);
		}

		public void TestS6_EmailToRecipientsAsString_IsReadOnlyForFax()
		{
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Fax;
			Recipient.S6_EmailToRecipientsAsString = "test1@test.com, test2@test.com";
			Assert(Recipient.S6_EmailToRecipientsAsStringInfo.ReadOnly);
			AssertEquals(string.Empty, Recipient.S6_EmailToRecipientsAsString);
		}

		public void TestS6_EmailToRecipientsAsString_NotReadOnlyForEmail()
		{
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			Recipient.S6_EmailToRecipientsAsString = "test1@test.com, test2@test.com";

			var emailToRecipients = Recipient.EmailToRecipients;

			Assert("When the delivermethod is EML we should be allowed to enter emails", !Recipient.S6_EmailToRecipientsAsStringInfo.ReadOnly);
			AssertEquals("test1@test.com, test2@test.com", Recipient.S6_EmailToRecipientsAsString);
			AssertEquals(2, emailToRecipients.Count);
			AssertCollectionContains(emailToRecipients, ccr => ccr.SCR_EmailAddress == "test1@test.com");
			AssertCollectionContains(emailToRecipients, ccr => ccr.SCR_EmailAddress == "test2@test.com");
		}

		public void TestS6_CarbonCopyRecipientsAsString()
		{
			// Arrange
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Fax;
			// Act
			var isReadOnly = Recipient.S6_CarbonCopyRecipientsAsStringInfo.ReadOnly;
			Recipient.S6_CarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			// Assert
			Assert(isReadOnly);
			AssertEquals(string.Empty, Recipient.S6_CarbonCopyRecipientsAsString);

			// Arrange
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			// Act
			isReadOnly = Recipient.S6_CarbonCopyRecipientsAsStringInfo.ReadOnly;
			var carbonCopyRecipients = Recipient.CarbonCopyRecipients;
			// Assert
			Assert(!isReadOnly);
			AssertEquals("test1@test.com, test2@test.com", Recipient.S6_CarbonCopyRecipientsAsString);
			AssertEquals(2, carbonCopyRecipients.Count);
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.SCR_EmailAddress == "test1@test.com");
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.SCR_EmailAddress == "test2@test.com");
		}

		public void TestS6_BlindCarbonCopyRecipientsAsString()
		{
			// Arrange
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Fax;
			// Act
			var isReadOnly = Recipient.S6_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly;
			Recipient.S6_BlindCarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			// Assert
			Assert(isReadOnly);
			AssertEquals(string.Empty, Recipient.S6_BlindCarbonCopyRecipientsAsString);

			// Arrange
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			// Act
			isReadOnly = Recipient.S6_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly;
			var carbonCopyRecipients = Recipient.BlindCarbonCopyRecipients;
			// Assert
			Assert(!isReadOnly);
			AssertEquals("test1@test.com, test2@test.com", Recipient.S6_BlindCarbonCopyRecipientsAsString);
			AssertEquals(2, carbonCopyRecipients.Count);
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.SCR_EmailAddress == "test1@test.com");
			AssertCollectionContains(carbonCopyRecipients, ccr => ccr.SCR_EmailAddress == "test2@test.com");
		}

		public void TestEmailToRecipientsAreDeletedUponSavingWhenDisabled()
		{
			// Arrange
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			Recipient.S6_EmailToRecipientsAsString = "test1@test.com, test2@test.com";

			Factory.Save();

			Assert(!Recipient.S6_EmailToRecipientsAsStringInfo.ReadOnly);
			AssertEquals(2, Recipient.EmailToRecipients.Count);

			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Fax;
			Assert("Email should be readonly when DeliveryMode is Fax", Recipient.S6_EmailToRecipientsAsStringInfo.ReadOnly);
			Factory.Save();
			AssertEquals(0, Recipient.EmailToRecipients.Count);
		}

		public void TestCarbonCopyRecipientsAreDeletedUponSavingWhenDisabled()
		{
			// Arrange
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			Recipient.S6_CarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			Factory.Save();
			Assert(!Recipient.S6_CarbonCopyRecipientsAsStringInfo.ReadOnly);
			AssertEquals(2, Recipient.CarbonCopyRecipients.Count);
			// Act
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Fax;
			Assert(Recipient.S6_CarbonCopyRecipientsAsStringInfo.ReadOnly);
			Factory.Save();
			// Assert
			AssertEquals(0, Recipient.CarbonCopyRecipients.Count);
		}

		public void TestBlindCarbonCopyRecipientsAreDeletedUponSavingWhenDisabled()
		{
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			Recipient.S6_BlindCarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			Factory.Save();
			Assert(!Recipient.S6_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly);
			AssertEquals(2, Recipient.BlindCarbonCopyRecipients.Count);
			// Act
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Fax;
			Assert(Recipient.S6_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly);
			Factory.Save();
			// Assert
			AssertEquals(0, Recipient.BlindCarbonCopyRecipients.Count);
		}

		public void TestInternalCloneIncludesToAndCCAndBCC()
		{
			Recipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			Recipient.S6_EmailToRecipientsAsString = "00@00.com";
			Recipient.S6_CarbonCopyRecipientsAsString = "11@11.com";
			Recipient.S6_BlindCarbonCopyRecipientsAsString = "22@22.com";

			var cloneRecipient = (StmScheduleTaskRecipient)Recipient.Clone();

			AssertEquals("S6_EmailToRecipientsAsString", "00@00.com", cloneRecipient.S6_EmailToRecipientsAsString);
			AssertEquals("S6_CarbonCopyRecipientsAsString", "11@11.com", cloneRecipient.S6_CarbonCopyRecipientsAsString);
			AssertEquals("S6_BlindCarbonCopyRecipientsAsString", "22@22.com", cloneRecipient.S6_BlindCarbonCopyRecipientsAsString);
		}

		#region Implementation

		StmScheduleTaskRecipient Recipient
		{
			get { return recipient ?? (recipient = Factory.NewWithValidTestData<StmScheduleTaskRecipient>()); }
		}
		StmScheduleTaskRecipient recipient;

		#endregion
	}
}
