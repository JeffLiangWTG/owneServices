using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class QueueCodeSetTest : TestCaseWithFactory
	{
		public void TestGetReasonCodeSet()
		{
			QueueCodeSet queueCodeSet = new QueueCodeSet();
			QueueReasonCodeSet reasonCodeSet1 = queueCodeSet.GetReasonCodeSet("Q1");
			QueueReasonCodeSet reasonCodeSet2 = queueCodeSet.GetReasonCodeSet("Q2");
			QueueReasonCodeSet retrievedReasonCodeSet1 = queueCodeSet.GetReasonCodeSet("Q1");
			QueueReasonCodeSet retrievedReasonCodeSet2 = queueCodeSet.GetReasonCodeSet("Q2");
			AssertEquals("The same QueueReasonCodeSet should be returned each time", reasonCodeSet1, retrievedReasonCodeSet1);
			reasonCodeSet1.Add("RE1");
			reasonCodeSet2.Add("RE2");
			AssertEquals("Reason code set 1 should contain RE1", true, reasonCodeSet1.Contains("RE1"));
			AssertEquals("Reason code set 2 should contain RE2", true, reasonCodeSet2.Contains("RE2"));
			AssertEquals("Reason code set 1 should NOT contain RE1", false, reasonCodeSet1.Contains("RE2"));
			AssertEquals("Reason code set 2 should NOT contain RE2", false, reasonCodeSet2.Contains("RE1"));
		}

		public void TestGetMultipleCodeFilter()
		{
			CusHAWB hAWB1 = Factory.New<CusHAWB>();
			hAWB1.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			hAWB1.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.AN_RefusedDuplicateOrder;
			CusHAWB hAWB2 = Factory.New<CusHAWB>();
			hAWB2.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
			hAWB2.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			CusHAWB decoyHAWB = Factory.New<CusHAWB>();
			decoyHAWB.CurrentQueue.P4_QueueName = "QX";
			QueueCodeSet queueCodeSet = new QueueCodeSet();
			queueCodeSet.Add(DefaultQueueCodeDescriptionPairList.Codes.EIR);
			queueCodeSet.Add(DefaultQueueCodeDescriptionPairList.Codes.Hold);
			ZQuery query = queueCodeSet.GetMultipleCodeFilter(ProcessQueueSchema.P4_QueueName, ProcessQueueSchema.P4_Status, ProcessQueueSchema.P4_SubStatus);
			ProcessQueue[] filterMatches = (UPECargoReportQueue[])Factory.Load(typeof(UPECargoReportQueue), query);
			AssertEquals("2 HAWBs should match", 2, filterMatches.Length);
			AssertCollectionContains("Correct HAWB should match", hAWB1.CurrentQueue, filterMatches);
			AssertCollectionContains("Correct HAWB should match", hAWB2.CurrentQueue, filterMatches);
		}

		public void TestClearQueueClearsReasons()
		{
			QueueCodeSet queueCodeSet = new QueueCodeSet();
			QueueReasonCodeSet reasonCodeSet = queueCodeSet.GetReasonCodeSet("QUE");
			reasonCodeSet.Add("QUE");
			queueCodeSet.Clear();
			AssertEquals("When the selected reasons are cleared, the selected reasons should also be cascade cleared", false, reasonCodeSet.Contains("REA"));
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
