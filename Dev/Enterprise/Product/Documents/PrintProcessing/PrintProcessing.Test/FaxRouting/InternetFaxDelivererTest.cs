using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.PrintProcessing.FaxRouting.Testing
{
	sealed class InternetFaxDelivererTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliver()
		{
			string faxNumber = "+61290251198";
			StmPrintJob faxJob = Factory.New<StmPrintJob>();
			faxJob.SP_JobType = "FAX";
			faxJob.SP_EmailAttachmentFormat = "XLS";
			faxJob.SP_FaxDestination = faxNumber;
			faxJob.SP_DocumentName = PrintProcessingConstants.TestGeneratedReport;
			faxJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			faxJob.SP_EmailSubjectLine = PrintProcessingConstants.TestGeneratedReport.Substring(0, PrintProcessingConstants.TestGeneratedReport.IndexOf("."));
			faxJob.SP_ParentTableName = "JobShipment";
			faxJob.SP_RelatedBusinessContext = "SHP";
			faxJob.SP_DocumentType = "CIV";
			faxJob.SP_RunDateTime = ZDateTime.Now;
			faxJob.SP_SB_DeliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory).PK;
			faxJob.StoredAttachmentFilename = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport;

			new InternetFaxDeliverer().Deliver(faxJob);
			AssertEquals("FAA", faxJob.SP_JobType);
		}
	}
}
