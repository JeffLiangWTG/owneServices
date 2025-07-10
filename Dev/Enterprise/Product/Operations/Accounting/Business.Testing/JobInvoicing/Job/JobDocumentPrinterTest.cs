using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobDocumentPrinter))]
	public class JobDocumentPrinterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPrintJobProfit()
		{
			DocumentCommand jobDocumentCommand = JobDocPrinter.FindJobDocumentCommand_ForTestOnly();
			AssertNotNull("Job Document Command should not be null", jobDocumentCommand);
			AssertEquals("Job Document Command Template Name", JobDocPrinter.JobProfitDocumentMenuName_ForTestOnly, jobDocumentCommand.SU_MenuName);
		}

		public void TestGetDocumentPack()
		{
			DocumentPack docPack = JobDocPrinter.GetDocumentPack_ForTestOnly();
			AssertEquals("DocPack Count", 0, docPack.Count);
		}

		public void TestFindJobDocumentCommand()
		{
			DocumentCommand jobDocumentCommand = JobDocPrinter.FindJobDocumentCommand_ForTestOnly();

			AssertNotNull("Job Document Command should not be null", jobDocumentCommand);
			AssertEquals("Job Document Command: Menu Name", JobDocPrinter.JobProfitDocumentMenuName_ForTestOnly, jobDocumentCommand.SU_MenuName);
			AssertEquals("Job Document Command: Is Published", true, jobDocumentCommand.SU_IsPublished);
			AssertEquals("Job Document Command: Is System Defined", true, jobDocumentCommand.SU_IsSystemDefined);

			jobDocumentCommand.SU_IsPublished = false;
			jobDocumentCommand = JobDocPrinter.FindJobDocumentCommand_ForTestOnly();

			AssertNotNull("Job Document Command should not be null", jobDocumentCommand);
			AssertEquals("Job Document Command: Menu Name", JobDocPrinter.JobProfitDocumentMenuName_ForTestOnly, jobDocumentCommand.SU_MenuName);
			AssertEquals("Job Document Command: Is Published", false, jobDocumentCommand.SU_IsPublished);
			AssertEquals("Job Document Command: Is System Defined", true, jobDocumentCommand.SU_IsSystemDefined);
		}

		public void TestGetPrintDeliveryInstructions()
		{
			DeliveryInstructions instructions = JobDocPrinter.GetPrintDeliveryInstructions_ForTestOnly((JobDocPrinter.GetDocumentPack_ForTestOnly()));
			AssertNotNull("Delivery Instructions should not be null", instructions);
			AssertEquals("Delivery Instructions should have one Recipient", 1, instructions.Recipients.Count);

			DocDeliveryContact contact = instructions.Recipients[0];
			AssertNotNull("Delivery Instructions Contact should not be null", contact);
			AssertEquals("Delivery Instructions Contact: OrgHeaderPK ", ZGuid.Empty, contact.OrgHeaderPK);
			AssertEquals("Delivery Instructions Contact: Name", ZString.Empty, contact.Name);
			AssertEquals("Delivery Instructions Contact: DeliveryMethod", Core.Constants.ContactNotifyModes.Print, contact.DeliveryMethod);
		}

		public void TestPrintTaskDeliveryInstructionsPK()
		{
			var jobDocumentCommand = JobDocPrinter.FindJobDocumentCommand_ForTestOnly();
			var task = JobDocPrinter.GetPrintTask_ForTestOnly(jobDocumentCommand.PK);
			AssertEquals("Task delivery instructions PK", jobDocumentCommand.PK, task.DeliveryInstructionsDefaultPK);
		}

		public void TestHasChanges()
		{
			AssertEquals("HasChanges of JobDocPrinter must be false", false, JobDocPrinter.HasChanges);
			AssertEquals("Initial value of property PrintAPInvoiceAnalysis is false ", ZBool.False, JobDocPrinter.PrintAPInvoiceAnalysis);
			JobDocPrinter.PrintAPInvoiceAnalysis = ZBool.True;
			AssertEquals("HasChanges of JobDocPrinter after property PrintAPInvoiceAnalysis was changed must be false", false, JobDocPrinter.HasChanges);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDocPrinter = new JobDocumentPrinter(Factory);
			return JobDocPrinter;
		}

		protected JobDocumentPrinter JobDocPrinter;

		#endregion
	}
}
