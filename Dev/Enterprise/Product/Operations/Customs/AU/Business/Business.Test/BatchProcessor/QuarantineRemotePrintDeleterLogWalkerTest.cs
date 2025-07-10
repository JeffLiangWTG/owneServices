using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineRemotePrintDeleterLogWalker))]
	sealed class QuarantineRemotePrintDeleterLogWalkerTest : LogSubscriberTest<QuarantineRemotePrintDeleterLogWalker>
	{
		public void TestQuarantinePrintsAreDeleted()
		{
			JobDeclaration dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.Logs.AddNew(Events.DocumentSent, "HP GayzerJet 123 - Quarantine Remote Print - B00000069|NAM=123456789.pcl");
			Factory.Save();
			dec.DocManagerInfo.AddFileOrDocument(new byte[] { 5, 6, 7 }, "123456789.pcl", "QRP");
			dec.DocManagerInfo.Save();
			AssertEquals("Pre req", 1, dec.DocManagerInfo.Files.Count);
			RunLogWalkerCycleForTest();

			AssertEquals(0, dec.DocManagerInfo.Files.Count);
		}

		public void TestOnlyPrintedDocsAreDeleted()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.Logs.AddNew(Events.DocumentSent, "MEL_OPS_KYOCERA_4200DN_03 ON CTFPRN03 - CAU - MEL - Quarantine Remote Print - SMEL4041253|NAM=ExDoc175.pcl");
			Factory.Save();

			dec.DocManagerInfo.AddFileOrDocument(new byte[] { 5, 6, 7 }, "123456789.pcl", "QRP");
			dec.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			dec.DocManagerInfo.AddFileOrDocument(new byte[] { 3, 6, 7 }, "ExDoc175.pcl", "QRP");
			dec.DocManagerInfo.AddFileOrDocument(new byte[] { 3, 4, 7 }, "ED48299.pcl", "QRP");
			dec.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }, "document.pdf", "PKL");
			dec.DocManagerInfo.Save();
			AssertEquals("Pre-requisite - all eDocs", 5, dec.DocManagerInfo.Files.Count);

			RunLogWalkerCycleForTest();
			AssertEquals("Un-printed QRP documents should still exist in Job eDocs", 4, dec.DocManagerInfo.Files.Count);
		}

		public void TestQuarantinePrintsAreNotDeletedForNexdocs()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = dec.Invoices.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = "DAI";
			dec.Logs.AddNew(Events.DocumentSent, "HP GayzerJet 123 - Quarantine Remote Print - B00000069|NAM=123456789.pcl");
			Factory.Save();
			dec.DocManagerInfo.AddFileOrDocument(new byte[] { 5, 6, 7 }, "123456789.pcl", "QRP");
			dec.DocManagerInfo.Save();
			AssertEquals("Pre req", 1, dec.DocManagerInfo.Files.Count);

			RunLogWalkerCycleForTest();

			AssertEquals("File is not deleted.", 1, dec.DocManagerInfo.Files.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "XXX";
			newBranch.GB_RL_NKHomePort = "AUXXX";

			Factory.Save();
		}
		GlbBranch newBranch;
	}
}
