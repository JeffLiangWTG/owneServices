using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.EdocSaver.Testing
{
	sealed class C88EDocsSaverTest : TestCaseWithFactory
	{
		public void TestC88IsRenderedAndSavedToEDocs()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_DeclarationReference = "B00001114";
			entry.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			entry.CH_BGMReference = "testBgmReference";
			Factory.Save();

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
			AssertNull("Should not have a C88 yet", printJob);

			new C88EDocsSaver(entry).RenderDocumentAndSaveInEDocs();
			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));

			AssertEquals("Should have a single C88 against the entry", 1, printJobs.Length);

			new C88EDocsSaver(entry).RenderDocumentAndSaveInEDocs();
			Factory.Save();

			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
			AssertEquals("Should now have a second C88 against the declaration - we should have made a second cos we're keeping versions", 2, printJobs.Length);
		}
	}
}
