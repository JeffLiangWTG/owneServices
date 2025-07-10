using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.EdocSaver.Testing
{
	sealed class SADEDocsSaverTest : TestCaseWithFactory
	{
		public void TestSADIsRenderedAndSavedToEDocs()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_DeclarationReference = "B00001114";
			entry.CH_BGMReference = "BGMReference";
			Factory.Save();

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
			AssertNull("Prerequisite: No print jobs are created for entry.", printJob);

			new SADEDocsSaver(entry).RenderDocumentAndSaveInEDocs();
			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
			AssertEquals("There should be a print job created for entry.", 1, printJobs.Length);
			AssertEquals("There should be a print job with document type as EPR.", "EPR", printJobs[0].SP_DocumentType);
			AssertContains("There should be a print job with the expected document name", "SADH C88 - BGMReference", printJobs[0].SP_DocumentName);
			AssertEquals("There should be a print job with the expected attachment name", "SAD/H for BGMReference for B00001114 .XLSX", printJobs[0].SP_EmailAttachments);
		}
	}
}
