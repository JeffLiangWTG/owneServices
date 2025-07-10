using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class BuildXmlBizOEntryTest : TestCase
	{
		public void TestLivesInZArchitecture()
		{
			AssertEquals("BizO in Freight, Entry.LivesInZArchitecture should be false.", false, EntryInFreight.LivesInZArchitecture);
			AssertEquals("BizO in MasterFiles, Entry.LivesInZArchitecture should be false.", false, EntryInMasterFiles.LivesInZArchitecture);
			AssertEquals("BizO in ZArchitecture, Entry.LivesInZArchitecture should be true.", true, EntryInZ.LivesInZArchitecture);
			AssertEquals("BizO in HRM, Entry.LivesInZArchitecture should be false.", false, EntryInHRM.LivesInZArchitecture);
		}

		public void TestLivesMasterFiles()
		{
			AssertEquals("BizO in Freight, Entry.LivesInMasterFiles should be false.", false, EntryInFreight.LivesInMasterFiles);
			AssertEquals("BizO in MasterFiles, Entry.LivesInMasterFiles should be true.", true, EntryInMasterFiles.LivesInMasterFiles);
			AssertEquals("BizO in ZArchitecture, Entry.LivesInMasterFiles should be false.", false, EntryInZ.LivesInMasterFiles);
			AssertEquals("BizO in HRM, Entry.LivesInMasterFiles should be false.", false, EntryInHRM.LivesInMasterFiles);
		}

		public void TestLivesInHRMFiles()
		{
			AssertEquals("BizO in Freight, Entry.LivesInHRMFiles should be false.", false, EntryInFreight.LivesInHRMFiles);
			AssertEquals("BizO in MasterFiles, Entry.LivesInHRMFiles should be false.", false, EntryInMasterFiles.LivesInHRMFiles);
			AssertEquals("BizO in ZArchitecture, Entry.LivesInHRMFiles should be false.", false, EntryInZ.LivesInHRMFiles);
			AssertEquals("BizO in HRM, Entry.LivesInHRMFiles should be true.", true, EntryInHRM.LivesInHRMFiles);
		}

		public void TestMasterFileReference()
		{
			AssertEquals("BizO in Freight, Entry.MasterFileReference should be false.", false, EntryInFreight.MasterFileReference);
			AssertEquals("BizO in MasterFiles, Entry.MasterFileReference should be false.", false, EntryInMasterFiles.MasterFileReference);
			AssertEquals("BizO in ZArchitecture, Entry.MasterFileReference should be true.", true, EntryInZ.MasterFileReference);
			AssertEquals("BizO in HRM, Entry.MasterFileReference should be false.", false, EntryInHRM.MasterFileReference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			EntryInFreight = new BuildXmlBizOEntryOnMainDbForTesting("Dummy", "Freight", false, true, false);
			EntryInMasterFiles = new BuildXmlBizOEntryOnMainDbForTesting("OrgHeader", "MasterFiles", false, true, false);
			EntryInZ = new BuildXmlBizOEntryOnMainDbForTesting("StmALog", "Enterprise.ZArchitecture.Business", true, true, false);
			EntryInHRM = new BuildXmlBizOEntryOnMainDbForTesting("HrlPolicy", "HRM", false, true, false);
		}

		BuildXmlBizOEntry EntryInFreight;
		BuildXmlBizOEntry EntryInMasterFiles;
		BuildXmlBizOEntry EntryInZ;
		BuildXmlBizOEntry EntryInHRM;
	}
}
