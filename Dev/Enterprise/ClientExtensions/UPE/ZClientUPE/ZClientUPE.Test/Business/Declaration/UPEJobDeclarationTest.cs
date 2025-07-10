using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.GSSI;
using Enterprise.Client.UPE.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Declaration.Testing
{
	internal class UPEJobDeclarationTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
		}

		public void TestOnFactorySaved()
		{
			TestHelper.NewJobRelatedWayBillsWithValidTestData();

			Factory.Save();
			var messages = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" }).Select(x => x.EM_MessageText).ToArray();

			AssertEquals("files created", TestHelper.GssiExpectedMessages.Count, messages.Length);

			foreach (string s in messages)
			{
				string actualContents = s.Substring(0, 66);
				Assert("Actual (" + actualContents + ") message valid", TestHelper.GssiExpectedMessages.Contains(actualContents));
				TestHelper.GssiExpectedMessages.Remove(actualContents);
			}
			Assert("All expected messages found", TestHelper.GssiExpectedMessages.Count == 0);

			TestHelper.AssertContainsHoldExportLog(TestHelper.HouseBill.Declaration.PK, Factory, "E8", TestHelper.HouseBill.CS_HAWB);

			var anotherHouseBillQuery = new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, TestHelper.JobDeclaration.PK);
			anotherHouseBillQuery.AddToFilter(CusHAWBSchema.CS_HAWB, "AnotherHouseBill");
			var anotherHouseBill = Factory.LoadTop1<UPECusHAWB>(anotherHouseBillQuery);
			AssertNotNull(anotherHouseBill);

			TestHelper.AssertContainsHoldExportLog(anotherHouseBill.Declaration.PK, Factory, "E8", anotherHouseBill.CS_HAWB);
			TestHelper.AssertContainsHoldExportLog(anotherHouseBill.Declaration.PK, Factory, "E8", "VirtualChild");
			TestHelper.AssertContainsHoldExportLog(anotherHouseBill.Declaration.PK, Factory, "E8", "VirtualChild2");

			TestHelper.JobDeclaration.CurrentQueue.P4_QueueName = "CPL";
			TestHelper.JobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			Factory.Save();
			messages = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" }).Select(x => x.EM_MessageText).ToArray();
			AssertEquals("no new messages should be created if Dec is completed", 4, messages.Length);
		}

		[SnailTest]
		public void TestOnFactorySaved_LargerDataSet()
		{
			TestHelper.NewJobRelatedWayBillsWithValidTestData();

			int sample = 100 + 1;
			for (int index = 1; index < sample; index++)
			{
				UPECusHAWB anotherChildHouseBill = Factory.NewWithValidTestData<UPECusHAWB>();
				anotherChildHouseBill.CS_HAWB = "AnotherHouseBill" + index;
				anotherChildHouseBill.CS_JE_CustomsFormalEntry = TestHelper.JobDeclaration.PK;
				anotherChildHouseBill.CurrentQueue.P4_QueueName = "AAA";
				anotherChildHouseBill.CurrentQueue.P4_Status = CommercialQueueCodeDescriptionPairList.Codes.Hold;
				anotherChildHouseBill.CurrentQueue.P4_SubStatus = "BBB";
				anotherChildHouseBill.CurrentQueue.P4_CustomsStatus = "CCC";
			}

			for (int index = 1; index < sample; index++)
			{
				TestHelper.GssiExpectedMessages.Add("01ERR       7340      N7340AnotherHouseBill" + index.ToString().PadRight(3) + "                03E8");  // Child house bill.
			}

			Factory.Save();
			var messages = Factory.Load<GSSMessage>(new ZQuery { OrderBy = "EM_MessageNum" }).Select(x => x.EM_MessageText).ToArray();
			AssertEquals("All files (" + TestHelper.GssiExpectedMessages.Count + ") created", TestHelper.GssiExpectedMessages.Count, messages.Length);

			foreach (string s in messages)
			{
				string actualContents = s.Substring(0, 66);
				Assert("Actual (" + actualContents + ") message valid", TestHelper.GssiExpectedMessages.Contains(actualContents));
				TestHelper.GssiExpectedMessages.Remove(actualContents);
			}
			Assert("All expected messages found", TestHelper.GssiExpectedMessages.Count == 0);
		}

		public void TestMissingControlNumer()
		{
			UPEJobDeclaration jobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			IRefundEnquiry owner = jobDec;
			AssertNull("Missing refund", owner.Refund);

			ZString noteText = ZString.Format(refundNoteFormat, "0223573", GlbStaff.CurrentUser.GS_FullName, ZDateTime.UtcNow.ToLongTimeString(),
				"TICKED");
			jobDec.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.RefundNote.Description, noteText);
			Assert("And now it magically reappears...", !owner.Refund.T10_ControlNumber.IsEmpty);
		}
		const string refundNoteFormat = "CONTROL NUMBER        : {0}\r\nUSER NAME             : {1}\r\nDATE                  : {2}\r\nREMARKS               : {3}";

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;
	}
}

