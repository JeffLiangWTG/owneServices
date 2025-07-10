using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	abstract class OnlineDeliveryBaseTest : TestCaseWithFactory
	{
		public void TestAttachmentType()
		{
			AssertAttachmentType(true);
			AssertAttachmentType(false);
		}

		public void TestSetAdditionalPropertiesWithFileAttachment()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			info.SetFileContents(SimpleTestXls, "ABC");
			var contact = GetContact();
			var deliveryMethod = GetNewDeliveryMethod(contact);
			deliveryMethod.AddFile(info);
			deliveryMethod.Deliver();

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			AssertEquals("StmPrintJob Count", 1, printJobs.Count);
			AssertEquals("BlobType", "ABC", printJobs[0].BlobType);
			AssertEquals("SP_EmailAttachmentFormat", OrgConstants.AttachmentType.FIL, printJobs[0].SP_EmailAttachmentFormat);
		}

		public void TestSetAdditionalPropertiesWithXLSfile()
		{
			var xLSInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo.SetFileContents(NewStyleTemplateXls, "xls");

			var contact = GetContact();
			var deliveryMethod = GetNewDeliveryMethod(contact);
			deliveryMethod.AddFile(xLSInfo);

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			deliveryMethod.Deliver();

			var printJobsAfter = new StmPrintJobCollection(Factory);
			printJobsAfter.Load();

			// remove all the print jobs from the after collection so we're left only with the new print job
			foreach (StmPrintJob printJob in printJobs)
			{
				printJobsAfter.Remove(printJob);
			}

			AssertEquals("We should have one print job after email.deliver()", 1, printJobsAfter.Count);
			AssertEquals("Blob type should be XLS", AttachmentTypeList.Codes.Xls, printJobsAfter[0].BlobType);
			AssertEquals("Email delivery type should be XLS", AttachmentTypeList.Codes.Xls, printJobsAfter[0].SP_EmailAttachmentFormat.ToUpper());
		}

		public void TestSetAdditionalPropertiesWithPDFCFormat()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			info.SetFileContents(SimpleTestXls, "ABC");

			var contact = GetContact();
			contact.AttachmentType = AttachmentTypeList.Codes.Pdfc;
			var deliveryMethod = GetNewDeliveryMethod(contact);
			deliveryMethod.AddFile(info);
			deliveryMethod.Deliver();

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			AssertEquals("StmPrintJob Count", 1, printJobs.Count);
			AssertEquals("BlobType", "ABC", printJobs[0].BlobType);
			AssertEquals("SP_EmailAttachmentFormat", deliveryMethod is Email ? OrgConstants.AttachmentType.PDFC : OrgConstants.AttachmentType.FIL, printJobs[0].SP_EmailAttachmentFormat);
		}

		public void TestFromDeliveryContact()
		{
			var contact = GetContact();

			var deliveryMethod = (OnlineDeliveryBase)GetNewDeliveryMethod(contact);
			AssertEquals("Parsed AttachmentType", AttachmentTypeList.Codes.Xls, deliveryMethod.AttachmentType);

			contact.AttachmentType = "PDF";
			deliveryMethod = (OnlineDeliveryBase)GetNewDeliveryMethod(contact);
			AssertEquals("Parsed AttachmentType", AttachmentTypeList.Codes.Pdf, deliveryMethod.AttachmentType);

			contact.AttachmentType = "TIF";
			deliveryMethod = (OnlineDeliveryBase)GetNewDeliveryMethod(contact);
			AssertEquals("Parsed AttachmentType", AttachmentTypeList.Codes.Tif, deliveryMethod.AttachmentType);
		}

		public void TestFromInvalidDeliveryAttachmentType()
		{
			var contact = new DocDeliveryContact(Factory);
			contact.AttachmentType = "XL";
			contact.Email = "test@edi.com.au";

			var email = new Email(contact);
			AssertEquals("Parsed AttachmentType Defaults to Xls", AttachmentTypeList.Codes.Xls, email.AttachmentType);
		}

		protected abstract DeliveryMethod GetNewDeliveryMethod(DocDeliveryContact contact);

		protected DocDeliveryContact GetContact()
		{
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = AttachmentTypeList.Codes.Xls;
			contact.Email = "test@edi.com.au";
			return contact;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		void AssertAttachmentType(bool isFormatSwitchingRequired)
		{
			DocDeliveryContact contact = GetContact();
			contact.IsFormatSwitchingRequired = isFormatSwitchingRequired;
			var deliveryMethod = (OnlineDeliveryBase)GetNewDeliveryMethod(contact);

			foreach (CodeDescriptionPair attachmentType in new AttachmentTypeList())
			{
				contact.AttachmentType = attachmentType.Code;

				AssertEquals(contact.AttachmentTypeWithFormatSwitching, deliveryMethod.AttachmentType);

				if (isFormatSwitchingRequired && attachmentType.Code == AttachmentTypeList.Codes.Xls)
				{
					AssertEquals(AttachmentTypeList.Codes.Xlsx, deliveryMethod.AttachmentType);
				}
				else
				{
					AssertEquals(attachmentType.Code, deliveryMethod.AttachmentType);
				}
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		Stream NewStyleTemplateXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls");

		Stream SimpleTestXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest.xls");
	}
}
