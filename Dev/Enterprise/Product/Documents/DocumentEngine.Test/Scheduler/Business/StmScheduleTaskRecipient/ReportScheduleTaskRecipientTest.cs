using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Scheduler.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(ReportScheduleTaskRecipient))]
	sealed class ReportScheduleTaskRecipientTest : StmScheduleTaskRecipientTest
	{
		public void TestEDocDeliverMode()
		{
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;

			AssertEquals(string.Empty, Recipient.S6_EmptyReportDeliveryOptions);
			AssertEquals(string.Empty, Recipient.S6_DeliveryMethod);
			AssertEquals(true, Recipient.S6_DeliveryMethodInfo.ReadOnly);
			AssertEquals(true, Recipient.S6_EmptyReportDeliveryOptionsInfo.ReadOnly);
			AssertEquals(true, Recipient.ShouldHaveAttachmentTypeExposedForTests);
			AssertEquals(false, Recipient.S6_AttachmentTypeInfo.ReadOnly);
			AssertEquals(false, Recipient.S6_OHInfo.ReadOnly);
			AssertEquals(false, Recipient.ContactNameInfo.ReadOnly);
		}

		#region Populate from Contact

		public void TestPopulateFromContact()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = organisation1.Contacts.AddNew();
			contact1.OC_ContactName = "Zubin";

			var contact2 = organisation1.Contacts.AddNew();
			contact2.OC_ContactName = "Bob";

			var contact3 = organisation2.Contacts.AddNew();
			contact3.OC_ContactName = "Jane";

			Factory.Save();

			var printQueuePK = ZGuid.NewZGuid();
			var instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printQueuePK;

			var docContact = instructions.Recipients.AddNew();
			docContact.OrgHeaderPK = organisation1.PK;
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			docContact.AttachmentType = OrgConstants.AttachmentType.PDF;
			docContact.Name = "Zubin";
			docContact.EmailSubjectMacro = "Test - <OC_ContactName>";

			Recipient.PopulateFromContact(instructions, docContact);
			AssertEquals("S6_OH", organisation1.PK, Recipient.S6_OH);
			AssertEquals("S6_OC", contact1.PK, Recipient.S6_OC);
			AssertEquals("S6_AttachmentType", OrgConstants.AttachmentType.PDF, Recipient.S6_AttachmentType);
			AssertEquals("S6_DeliveryMethod", Core.Constants.ContactNotifyModes.Email, Recipient.S6_DeliveryMethod);
			AssertEquals("S6_SQ", ZGuid.Empty, Recipient.S6_SQ);
			AssertEquals("S6_EmailFromAddress", "", Recipient.S6_EmailFromAddress);
			AssertEquals("S6_EmailSubjectLineOverride", "Test - Zubin", Recipient.S6_EmailSubjectLineOverride);

			docContact.DefaultEmailFromAddress = "Default@test.com";
			Recipient.PopulateFromContact(instructions, docContact);

			AssertEquals("S6_EmailFromAddress", "Default@test.com", Recipient.S6_EmailFromAddress);

			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			docContact.AttachmentType = OrgConstants.AttachmentType.XLS;
			Recipient.PopulateFromContact(instructions, docContact);
			AssertEquals("S6_OH", organisation1.PK, Recipient.S6_OH);
			AssertEquals("S6_OC", contact1.PK, Recipient.S6_OC);
			AssertEquals("S6_AttachmentType", OrgConstants.AttachmentType.XLS, Recipient.S6_AttachmentType);
			AssertEquals("S6_DeliveryMethod", Core.Constants.ContactNotifyModes.EPrint, Recipient.S6_DeliveryMethod);
			AssertEquals("S6_SQ", ZGuid.Empty, Recipient.S6_SQ);
			AssertEquals("S6_EmailToRecipientsAsString", "", Recipient.S6_EmailToRecipientsAsString);
			AssertEquals("S6_EmailSubjectLineOverride", "Test - Zubin", Recipient.S6_EmailSubjectLineOverride);

			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			docContact.AttachmentType = "";
			docContact.Name = "Bob";
			docContact.DeliveryAddress = "11223344";

			Recipient.PopulateFromContact(instructions, docContact);
			AssertEquals("S6_OH", organisation1.PK, Recipient.S6_OH);
			AssertEquals("S6_OC", contact2.PK, Recipient.S6_OC);
			AssertEquals("S6_AttachmentType", "", Recipient.S6_AttachmentType);
			AssertEquals("S6_DeliveryMethod", Core.Constants.ContactNotifyModes.Fax, Recipient.S6_DeliveryMethod);
			AssertEquals("S6_SQ", ZGuid.Empty, Recipient.S6_SQ);
			AssertEquals("S6_FaxOverride", "11223344", Recipient.S6_FaxOverride);

			docContact.OrgHeaderPK = organisation2.PK;
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			docContact.Name = "Jane";

			Recipient.PopulateFromContact(instructions, docContact);
			AssertEquals("S6_OH", organisation2.PK, Recipient.S6_OH);
			AssertEquals("S6_OC", contact3.PK, Recipient.S6_OC);
			AssertEquals("S6_AttachmentType", "", Recipient.S6_AttachmentType);
			AssertEquals("S6_DeliveryMethod", Core.Constants.ContactNotifyModes.Print, Recipient.S6_DeliveryMethod);
			AssertEquals("S6_SQ", printQueuePK, Recipient.S6_SQ);

			docContact.OrgHeaderPK = ZGuid.Empty;
			docContact.StaffCode = (Environment.Env.CurrentUser as GlbStaff).GS_Code;
			Recipient.PopulateFromContact(instructions, docContact);
			AssertEquals("S6_OC", ZGuid.Empty, Recipient.S6_OC);
			AssertEquals("S6_GS_NKRecipient", docContact.StaffCode, Recipient.S6_GS_NKRecipient);
			AssertEquals("S6_DeliveryToType", ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Recipient.S6_DeliveryToType);
		}

		public void TestPopulateFromContactHasCCAndBCC()
		{
			var instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = ZGuid.NewZGuid();

			DocDeliveryContact docContact = instructions.Recipients.AddNew();
			docContact.OrgHeaderPK = ZGuid.NewZGuid();
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			docContact.AttachmentType = OrgConstants.AttachmentType.PDF;
			docContact.DeliveryAddress = "11@11.com";
			docContact.EmailCarbonCopyRecipientsAsString = "22@22.com";
			docContact.EmailBlindCarbonCopyRecipientsAsString = "33@33.com";

			Recipient.PopulateFromContact(instructions, docContact);

			AssertEquals("S6_EmailToRecipientsAsString", "11@11.com", Recipient.EmailToRecipients.Value);
			AssertEquals("S6_CarbonCopyRecipientsAsString", "22@22.com", Recipient.S6_CarbonCopyRecipientsAsString);
			AssertEquals("S6_BlindCarbonCopyRecipientsAsString", "33@33.com", Recipient.S6_BlindCarbonCopyRecipientsAsString);
		}

		#endregion

		#region Lookups / Validation

		public void TestLookups()
		{
			AssertEquals(typeof(ReportScheduleTaskRecipientLookups), Recipient.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(ReportScheduleTaskRecipientValidation), Recipient.Validation.GetType());
		}

		#endregion

		#region Overridden properties

		public void TestShouldHaveAttachmentType()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			Assert(!Recipient.ShouldHaveAttachmentTypeExposedForTests);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert("From base", Recipient.ShouldHaveAttachmentTypeExposedForTests);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			Assert("New Ftp mode", Recipient.ShouldHaveAttachmentTypeExposedForTests);
		}

		public void TestS6_OH()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			Recipient.S6_OH = orgHeader.PK;
			Recipient.ContactName = "233";
			Recipient.S6_OH = ZGuid.Empty;
			AssertEquals(ZString.Empty, Recipient.ContactName);
		}

		public void TestS6_GS_NKRecipient()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			Recipient.ContactName = "233";
			Recipient.S6_GS_NKRecipient = glbStaff.GS_Code;
			AssertEquals(ZString.Empty, Recipient.ContactName);
		}

		public void TestS6_GS_NKRecipient_ReadOnly()
		{
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			AssertEquals(false, Recipient.S6_GS_NKRecipientInfo.ReadOnly);
			Recipient.S6_OH = ZGuid.NewZGuid();
			AssertEquals(true, Recipient.S6_GS_NKRecipientInfo.ReadOnly);
		}

		public void TestS6_OH_ReadOnly()
		{
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			AssertEquals(false, Recipient.S6_OHInfo.ReadOnly);
			Recipient.S6_GS_NKRecipient = "aa";
			AssertEquals(true, Recipient.S6_OHInfo.ReadOnly);
		}

		#endregion

		#region Ftp details

		public void TestIsFtpDeliverMode()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.DoNotDeliver;
			Assert(!Recipient.IsFtpDeliverMode);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert(!Recipient.IsFtpDeliverMode);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Assert(!Recipient.IsFtpDeliverMode);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			Assert(!Recipient.IsFtpDeliverMode);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			Assert("It is Ftp mode", Recipient.IsFtpDeliverMode);
		}

		#endregion

		#region EmailFromAddress

		public void TestEmailFromAddressTypeList()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			Factory.Save();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");
			typeList.AddPair("EXP", "Export");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "main@test.com";
				var emailAddresses = staff.EmailAddresses;

				var emailAddress1 = emailAddresses.AddNew();
				emailAddress1.GSE_GC_Company = company.PK;
				emailAddress1.GSE_EmailAddress = "import@wtg.com";
				emailAddress1.GSE_Type = "IMP";

				var emailAddress2 = emailAddresses.AddNew();
				emailAddress2.GSE_GC_Company = company.PK;
				emailAddress2.GSE_EmailAddress = "export@wtg.com";
				emailAddress2.GSE_Type = "EXP";

				var emailAddress3 = emailAddresses.AddNew();
				emailAddress3.GSE_EmailAddress = "import@bar.com";
				emailAddress3.GSE_Type = "IMP";

				var emailAddress4 = emailAddresses.AddNew();
				emailAddress4.GSE_EmailAddress = "export@bar.com";
				emailAddress4.GSE_Type = "EXP";

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				reportScheduleTask.UserFK = staff.PK;
				Recipient.S6_S5 = reportScheduleTask.PK;
				Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

				AssertEquals(5, Recipient.EmailFromAddressList.Count);
				AssertEquals("Main - main@test.com", Recipient.EmailFromAddressList[0].Code);
				AssertEquals("main@test.com", Recipient.EmailFromAddressList[0].Description);

				AssertEquals("Export (WTG) - export@wtg.com", Recipient.EmailFromAddressList[1].Code);
				AssertEquals("export@wtg.com", Recipient.EmailFromAddressList[1].Description);

				AssertEquals("Export - export@bar.com", Recipient.EmailFromAddressList[2].Code);
				AssertEquals("export@bar.com", Recipient.EmailFromAddressList[2].Description);

				AssertEquals("Import (WTG) - import@wtg.com", Recipient.EmailFromAddressList[3].Code);
				AssertEquals("import@wtg.com", Recipient.EmailFromAddressList[3].Description);

				AssertEquals("Import - import@bar.com", Recipient.EmailFromAddressList[4].Code);
				AssertEquals("import@bar.com", Recipient.EmailFromAddressList[4].Description);

				Recipient.EmailFromAddress = "Import - import@bar.com";

				AssertEquals("import@bar.com", Recipient.S6_EmailFromAddress);

				Recipient.S6_EmailFromAddress = "export@bar.com";

				AssertEquals("Export - export@bar.com", Recipient.EmailFromAddress);

				Recipient.EmailFromAddress = "Import (WTG) - import@wtg.com";

				AssertEquals("import@wtg.com", Recipient.S6_EmailFromAddress);

				Recipient.S6_EmailFromAddress = "export@wtg.com";

				AssertEquals("Export (WTG) - export@wtg.com", Recipient.EmailFromAddress);
			}
		}

		public void TestEmailFromAddressCorrectlyUpdated()
		{
			const string printerMainEmail = "printer@main.com";
			const string printerMainEmail2 = "printer@main2.com";
			const string printerCustomizedEmail = "cus@wtg.com";
			const string printerCustomizedEmai2 = "dvt@wtg.com";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("REC", "Recive");

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();

			var contact = organisation1.Contacts.AddNew();
			contact.OC_ContactName = "Zubin";

			Factory.Save();

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				var printer = Factory.NewWithValidTestData<GlbStaff>();
				printer.GS_EmailAddress = printerMainEmail;
				var printercustomizedEmailAddress = AddEmailAddress(printer, printerCustomizedEmail, company.PK, typeList[0].Code);

				Factory.Save();

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				reportScheduleTask.UserFK = printer.PK;

				var staff1Recipient = AddStaffToRecipients(reportScheduleTask, printer);
				var staff2Recipient = AddStaffToRecipients(reportScheduleTask, printer);
				staff1Recipient.EmailFromAddress = printerMainEmail;
				staff2Recipient.EmailFromAddress = printerCustomizedEmail;
				Factory.Save();

				AssertEquals("EmailFromAddress should be printer email address", printer.GS_EmailAddress, staff1Recipient.S6_EmailFromAddress);
				AssertEquals("EmailFromAddress should be printer customized eamil address", printercustomizedEmailAddress.GSE_EmailAddress, staff2Recipient.S6_EmailFromAddress);

				printercustomizedEmailAddress.GSE_EmailAddress = printerCustomizedEmai2;
				printer.GS_EmailAddress = printerMainEmail2;
				Factory.Save();

				AssertEquals("EmailFromAddress should be printer main email address", printer.GS_EmailAddress, staff1Recipient.S6_EmailFromAddress);
				AssertEquals("EmailFromAddress should be printer customized email address", printercustomizedEmailAddress.GSE_EmailAddress, staff2Recipient.S6_EmailFromAddress);
			}
		}

		public void TestEmailFromAddressListAlwaysLoadFromPrinter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("SND", "Send");
			typeList.AddPair("RCV", "Receive");

			Factory.Save();

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "main@wtg.com";

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				reportScheduleTask.UserFK = staff.PK;
				Factory.Save();

				recipient = AddStaffToRecipients(reportScheduleTask, staff);
				var emailFromAddressList = recipient.EmailFromAddressList;
				AssertEquals("EmailFromAddressList only contain one address.", 1, emailFromAddressList.Count);
				AssertEquals("EmailFormAddressList only have printer main address.", emailFromAddressList[0].Description, staff.GS_EmailAddress);

				var sendAddress = AddEmailAddress(staff, "Send@wtg.com", company.PK, typeList[0].Code);
				var receiveAddress = AddEmailAddress(staff, "Receive@wtg.com", company.PK, typeList[1].Code);
				staff.GS_EmailAddress = "main@wisetech.com";
				Factory.Save();

				emailFromAddressList = recipient.EmailFromAddressList;
				AssertEquals("EmailFromAddressList contain three address.", 3, emailFromAddressList.Count);
				AssertEquals("The first element of EmailFormAddressList is printer main address.", emailFromAddressList[0].Description, staff.GS_EmailAddress);
				AssertEquals("The second element of EmailFormAddressList is printer Receive address.", emailFromAddressList[1].Description, receiveAddress.GSE_EmailAddress);
				AssertEquals("The third element of EmailFormAddressList is printer Send address.", emailFromAddressList[2].Description, sendAddress.GSE_EmailAddress);

				staff.GS_EmailAddress = "main@wtg.com";
				receiveAddress.GSE_EmailAddress = "Receive@wisetech.com";

				emailFromAddressList = recipient.EmailFromAddressList;
				AssertEquals("EmailFromAddressList contain three address.", 3, emailFromAddressList.Count);
				AssertEquals("The first element of EmailFormAddressList is printer main address.", emailFromAddressList[0].Description, staff.GS_EmailAddress);
				AssertEquals("The second element of EmailFormAddressList is printer Receive address.", emailFromAddressList[1].Description, receiveAddress.GSE_EmailAddress);
				AssertEquals("The third element of EmailFormAddressList is printer Send address.", emailFromAddressList[2].Description, sendAddress.GSE_EmailAddress);
			}
		}

		GlbStaffEmailAddress AddEmailAddress(GlbStaff staff, string emailAddress, ZGuid company, string emailType = null)
		{
			var glbEmailAddress = staff.EmailAddresses.AddNew();

			glbEmailAddress.GSE_GC_Company = company;
			glbEmailAddress.GSE_EmailAddress = emailAddress;
			glbEmailAddress.EmailType = emailType;
			return glbEmailAddress;
		}

		ReportScheduleTaskRecipient AddStaffToRecipients(ReportScheduleTask reportScheduleTask, GlbStaff staff)
		{
			var newRecipient = reportScheduleTask.Recipients.AddNew();
			newRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			newRecipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			newRecipient.S6_AttachmentType = OrgConstants.AttachmentType.XLSX;
			newRecipient.S6_GS_NKRecipient = staff.GS_Code;
			return newRecipient;
		}

		#endregion

		#region Implementation

		ReportScheduleTaskRecipient Recipient
		{
			get
			{
				if (recipient == null)
				{
					recipient = Factory.New<ReportScheduleTaskRecipient>();
				}
				return recipient;
			}
		}
		ReportScheduleTaskRecipient recipient;

		#endregion
	}
}
