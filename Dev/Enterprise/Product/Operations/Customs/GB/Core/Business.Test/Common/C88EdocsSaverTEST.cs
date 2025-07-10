using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Testing
{
	class C88EdocsSaverTEST : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			var comp = Factory.NewWithValidTestData<GlbCompany>();
			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_BranchName = "Testy Test";
			comp.Branches.Add(branch);
			Factory.Save();
		}

		void RunC88Test(string expectedPartialDocName)
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_GB = branch.PK;
			CusEntryHeader entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_DeclarationReference = "B00001114";
			entry.CH_BGMReference = "B00001116";
			entry.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			Factory.Save();

			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, dec.PK));
			AssertNull("Should not have a C88 yet", printJob);

			new C88EdocsSaver(dec).RenderC88AndStoreInEdocs();
			Factory.Save();

			StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, dec.PK));
			if (!string.IsNullOrEmpty(expectedPartialDocName))
			{
				AssertEquals("Should have a single C88 against the declaration", 1, printJobs.Length);
				AssertContains("C88", printJobs[0].SP_DocumentName);
				AssertContains(expectedPartialDocName, printJobs[0].SP_EmailSubjectLine);
				Assert(printJobs[0].SP_EmailAttachments.Contains("B00001114"));
				Assert("Should have JOB's branch name in filename, not current processing/testing branch name", printJobs[0].SP_EmailSubjectLine.Contains("Testy Test"));

				new C88EdocsSaver(dec).RenderC88AndStoreInEdocs();
				Factory.Save();

				printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, dec.PK));
				AssertEquals("Should now have a second C88 against the declaration - we should have made a second cos we're keeping versions", 2, printJobs.Length);
			}
			else
			{
				AssertEquals("Should have no C88 against the declaration", 0, printJobs.Length);
			}
		}

		public void TestC88IsRenderedAndSavedToEdocs()
		{
			RunC88Test("SAD/H for B00001116");
		}

		public void TestC88IsRenderedAndSavedToEdocsPlain()
		{
			GBCustomsDataRegistry.Instance.ChiefC88EntryPrintPreference.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, Core.Constants.ChiefC88Options.Code.Plain);
			RunC88Test("C88 (plain) for");
		}

		public void TestC88IsRenderedAndSavedToEdocsNone()
		{
			GBCustomsDataRegistry.Instance.ChiefC88EntryPrintPreference.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, Core.Constants.ChiefC88Options.Code.None);
			RunC88Test("");
		}

		GlbBranch branch;
	}
}
