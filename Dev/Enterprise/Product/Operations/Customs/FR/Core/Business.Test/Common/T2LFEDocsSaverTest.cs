using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.EdocSaver.Testing
{
	sealed class T2LFEDocsSaverTest : TestCaseWithFactory
	{
		public void TestT2LFIsRenderedAndSavedToEDocs()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_DeclarationReference = "B00001114";
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entry.CH_BGMReference = "BGMReference";
			Factory.Save();

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
			AssertNull("Prerequisite: No print jobs are created for entry.", printJob);

			new T2LFEDocsSaver(entry).RenderDocumentAndSaveInEDocs();
			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));

			CombineAssertions(() =>
			{
				AssertEquals("There should be a print job created for entry.", 1, printJobs.Length);
				AssertContains("T2LF - BGMReference", printJobs[0].SP_DocumentName);
				AssertEquals("T2LF - BGMReference - T2LF.XLSX", printJobs[0].SP_EmailAttachments);
			});
		}
	}
}
