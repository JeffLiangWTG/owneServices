using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Module.Testing
{
	sealed class MiscRequestMessagesFilterLookupsTest : TestCaseWithFactory
	{
		public void TestBranchList()
		{
			AssertEquals("BranchLis", typeof(GlbBranchCollection), lookups.BranchList.GetType());
		}

		public void TestStaffList()
		{
			AssertEquals("StaffList", typeof(GlbStaffCollection), lookups.StaffList.GetType());
		}

		public void TestRequestEntryTypeList()
		{
			AssertEquals(2, lookups.RequestEntryTypeList.Count);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, lookups.RequestEntryTypeList[0].Code);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, lookups.RequestEntryTypeList[1].Code);
		}

		public void TestRequestMessageTypeList()
		{
			AssertEquals(2, lookups.RequestMessageTypeList.Count);
			AssertEquals(ElectronicDocumentTypeList.Codes._5AC, lookups.RequestMessageTypeList[0].Code);
			AssertEquals(ElectronicDocumentTypeList.Codes._5GW, lookups.RequestMessageTypeList[1].Code);
		}

		public void TestRequesStatusList()
		{
			AssertEquals(4, lookups.StatusList.Count);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, lookups.StatusList[0].Code);
			AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, lookups.StatusList[1].Code);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, lookups.StatusList[2].Code);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, lookups.StatusList[3].Code);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new MiscRequestMessagesFilterLookups(new MiscRequestMessagesFilterStripBusinessObject());
		}
		MiscRequestMessagesFilterLookups lookups;
	}
}
