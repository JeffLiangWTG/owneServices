using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class ReportScheduleTaskRecipientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateS6_DeliveryMethodWhenDeliveryToTypeIsEDoc()
		{
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			Recipient.S6_DeliveryMethod = string.Empty;
			AssertHasError(Recipient.S6_DeliveryMethodInfo, "Please enter a " + Recipient.S6_DeliveryMethodInfo.Description + ".");

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			AssertNoErrors(Recipient.S6_DeliveryMethodInfo);
		}

		public void TestValidateS6_SQ()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			Recipient.S6_SQ = ZGuid.Empty;
			AssertHasError(Recipient.S6_SQInfo, "Please enter a " + Recipient.S6_SQInfo.Description + ".");

			Recipient.S6_SQ = ZGuid.Invalid;
			AssertHasError(Recipient.S6_SQInfo, "Enter a valid " + Recipient.S6_SQInfo.Description + ".");

			Recipient.S6_SQ = ZGuid.NewZGuid();
			AssertHasError(Recipient.S6_SQInfo, "Enter a valid " + Recipient.S6_SQInfo.Description + ".");

			Recipient.S6_SQ = printer.PK;
			AssertNoErrors(Recipient.S6_SQInfo);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Recipient.S6_SQ = ZGuid.Empty;
			AssertNoErrors(Recipient.S6_SQInfo);
		}

		public void TestCheckS6_DeliveryMethodWithBigReportTemplate()
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest with big size.xls", "SimpleTest with big size.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest with big size.xls", Path.GetFullPath(tempFileName));
				var command = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Empty Report", excelTemplate,
					Factory);
				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();

				var docPack = new DocumentPack(command);
				using (var report = (Report)docPack[0])
				{
					report.PrepareForRender();
					reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(docPack.DeliveryInstructions, report);
				}

				reportScheduleTask.S5_ParentID = command.PK;
				reportScheduleTask.S5_EndDate = reportScheduleTask.S5_StartDate.AddDays(10);
				reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);

				var taskRecipient = reportScheduleTask.Recipients.AddNew();
				taskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
				taskRecipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				taskRecipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
				taskRecipient.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
				taskRecipient.S6_EmailToRecipientsAsString = "test@test.com";

				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
				{
					taskRecipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					AssertHasError(
						reportScheduleTask.Recipients.OfType<ReportScheduleTaskRecipient>().FirstOrDefault().S6_DeliveryMethodInfo,
						$"The report exceeds the {1}MB attachment limit and cannot be sent. The limit is defined in the Registry at {((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location}.");

					taskRecipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
					AssertNoErrors(reportScheduleTask.Recipients.OfType<ReportScheduleTaskRecipient>().FirstOrDefault().S6_DeliveryMethodInfo);
				}

				//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
				{
					taskRecipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					AssertNoErrors(reportScheduleTask.Recipients.OfType<ReportScheduleTaskRecipient>().FirstOrDefault().S6_DeliveryMethodInfo);
				}
			}
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

		public void TestCheckS6_EmailFromAddress_SelectedEmailAddressIsNotValid()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@email.com";

			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			task.UserFK = staff.PK;
			Recipient.S6_S5 = task.PK;
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Recipient.S6_EmailFromAddress = "wrong@email.com";

			AssertHasError(Recipient.S6_EmailFromAddressInfo, "This email address is not on the selected Print User's staff record.");

			Recipient.S6_EmailFromAddress = "test@email.com";
			AssertNoErrors(Recipient.S6_EmailFromAddressInfo);
		}

		public void TestCheckS6_EmailFromAddressIsEmptyWhenS6_DeliveryMethodIsNotEmailAndS6_EmailFromAddressIsInvalid()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@test.test";

			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			task.UserFK = staff.PK;
			Recipient.S6_S5 = task.PK;

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Recipient.S6_EmailFromAddress = "test@test.test";
			AssertEquals(Recipient.S6_EmailFromAddress, "test@test.test");

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			AssertEquals(Recipient.S6_EmailFromAddress, "test@test.test");

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals(Recipient.S6_EmailFromAddress, "test@test.test");

			Recipient.S6_EmailFromAddress = "test2@test2.test";
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			AssertEquals(Recipient.S6_EmailFromAddress, ZString.Empty);

			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals(Recipient.S6_EmailFromAddress, ZString.Empty);
		}

		public void TestValidateToFaxOrEmailWhenDeliveryMethodIsFax()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			Recipient.S6_GG = group.PK;
			Recipient.ToFaxOrEmail = "+49 4961 1448577";
			AssertNoErrors(Recipient.ToFaxOrEmailInfo);
			Recipient.ToFaxOrEmail = "test@test.test";
			AssertHasError(Recipient.ToFaxOrEmailInfo, "The phone number as entered has a high probability of being incorrect.\r\n\r\nNo country/region identified to format the number. Please enter the number in an international format. See below example:\r\n+61 2 1234 5678");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AA";
			staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			Recipient.S6_GS_NKRecipient = staff.GS_Code;
			Recipient.ToFaxOrEmail = "(02) 1234 5678";
			AssertNoErrors(Recipient.ToFaxOrEmailInfo);
			Recipient.ToFaxOrEmail = "123";
			AssertHasError(Recipient.ToFaxOrEmailInfo, "The phone number as entered has a high probability of being incorrect.\r\n\r\nPlease check the format and see below examples:\r\n(02) 1234 5678 (local format)\r\n+61 2 1234 5678 (international format)");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			Recipient.S6_OH = org.PK;
			Recipient.ToFaxOrEmail = "(201) 555-0123";
			AssertNoErrors(Recipient.ToFaxOrEmailInfo);
			Recipient.ToFaxOrEmail = "123";
			AssertHasError(Recipient.ToFaxOrEmailInfo, "The phone number as entered has a high probability of being incorrect.\r\n\r\nPlease check the format and see below examples:\r\n(201) 555-0123 (local format)\r\n+1 201-555-0123 (international format)");

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			AssertNullOrEmpty(Recipient.ToFaxOrEmail);
		}

		public void TestValidateToFaxOrEmailWhenDeliveryMethodIsEmail()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			Recipient.S6_GG = group.PK;
			Recipient.ToFaxOrEmail = "test@test.test";
			AssertNoErrors(Recipient.ToFaxOrEmailInfo);
			Recipient.ToFaxOrEmail = "+49 4961 1448577";
			AssertHasError(Recipient.ToFaxOrEmailInfo, "The email address \"+49 4961 1448577\" is invalid. You can separate multiple email addresses with a comma (,).");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			Recipient.S6_GS_NKRecipient = staff.GS_Code;
			Recipient.ToFaxOrEmail = "test@test.test";
			AssertNoErrors(Recipient.ToFaxOrEmailInfo);
			Recipient.ToFaxOrEmail = "+49 4961 1448577";
			AssertHasError(Recipient.ToFaxOrEmailInfo, "The email address \"+49 4961 1448577\" is invalid. You can separate multiple email addresses with a comma (,).");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			Recipient.S6_OH = org.PK;
			Recipient.ToFaxOrEmail = "test@test.test";
			AssertNoErrors(Recipient.ToFaxOrEmailInfo);
			Recipient.ToFaxOrEmail = "+49 4961 1448577";
			AssertHasError(Recipient.ToFaxOrEmailInfo, "The email address \"+49 4961 1448577\" is invalid. You can separate multiple email addresses with a comma (,).");

			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			AssertNullOrEmpty(Recipient.ToFaxOrEmail);
		}

		public void TestValidateToFaxOrEmailWhenEmptyReportDeliveryOptionsIsEmail()
		{
			Recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;

			var printer = Factory.New<StmPrintQueue>();
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			Recipient.S6_SQ = printer.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			Recipient.S6_OH = org.PK;

			Recipient.ToFaxOrEmail = "test";
			AssertHasError(Recipient.ToFaxOrEmailInfo, "The email address \"test\" is invalid. You can separate multiple email addresses with a comma (,).");

			Recipient.ToFaxOrEmail = "test@test.com";
			AssertNoErrors(Recipient.ToFaxOrEmailInfo);
		}

		public void TestValidateToFaxOrEmail_WhenDeliveryMethodIsFax_ShouldDetectCountryCode()
		{
			Recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("5aa5c86e-6efe-4602-b2d1-2992d6742bd7", new ResourceStringData("5aa5c86e-6efe-4602-b2d1-2992d6742bd7", "Other Language Contact"));
				mockRes.Put("2f3c2f2d-6971-4407-9ecc-b906d413de9f", new ResourceStringData("2f3c2f2d-6971-4407-9ecc-b906d413de9f", "Other Language Group"));
				mockRes.Put("d3cce607-a735-4aec-80fa-6b25c82c4886", new ResourceStringData("d3cce607-a735-4aec-80fa-6b25c82c4886", "Other Language Staff"));

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
				Recipient.S6_GS_NKRecipient = staff.GS_Code;
				Recipient.ToFaxOrEmail = "+61 2 12345678";
				AssertNoErrors(Recipient.ToFaxOrEmailInfo);
				Recipient.ToFaxOrEmail = "12345678";
				AssertHasError(Recipient.ToFaxOrEmailInfo, "The phone number as entered has a high probability of being incorrect.\r\n\r\nPlease check the format and see below examples:\r\n(02) 1234 5678 (local format)\r\n+61 2 1234 5678 (international format)");

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
				Recipient.S6_OH = org.PK;
				Recipient.ToFaxOrEmail = "+61 2 87654321";
				AssertNoErrors(Recipient.ToFaxOrEmailInfo);
				Recipient.ToFaxOrEmail = "87654321";
				AssertHasError(Recipient.ToFaxOrEmailInfo, "The phone number as entered has a high probability of being incorrect.\r\n\r\nPlease check the format and see below examples:\r\n(02) 1234 5678 (local format)\r\n+61 2 1234 5678 (international format)");

				Recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
				AssertNullOrEmpty(Recipient.ToFaxOrEmail);
			}
		}

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
	}
}
