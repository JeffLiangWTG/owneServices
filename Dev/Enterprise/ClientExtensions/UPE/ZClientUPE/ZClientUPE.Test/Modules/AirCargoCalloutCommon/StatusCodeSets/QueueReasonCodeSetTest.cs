using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class QueueReasonCodeSetTest : TestCaseWithFactory
	{
		public void TestGetStatusCodeSet()
		{
			QueueReasonCodeSet reasonCodeSet = new QueueReasonCodeSet("QUE");
			QueueStatusCodeSet statusCodeSet1 = reasonCodeSet.GetStatusCodeSet("RE1");
			QueueStatusCodeSet statusCodeSet2 = reasonCodeSet.GetStatusCodeSet("RE2");
			QueueStatusCodeSet retrievedStatusCodeSet1 = reasonCodeSet.GetStatusCodeSet("RE1");
			QueueStatusCodeSet retrievedStatusCodeSet2 = reasonCodeSet.GetStatusCodeSet("RE2");
			AssertEquals("The same QueueStatusCodeSet should be returned each time", statusCodeSet1, retrievedStatusCodeSet1);
			statusCodeSet1.Add("ST1");
			statusCodeSet2.Add("ST2");
			AssertEquals("Status code set 1 should contain ST1", true, statusCodeSet1.Contains("ST1"));
			AssertEquals("Status code set 2 should contain ST2", true, statusCodeSet2.Contains("ST2"));
			AssertEquals("Status code set 1 should NOT contain ST2", false, statusCodeSet1.Contains("ST2"));
			AssertEquals("Status code set 2 should NOT contain ST1", false, statusCodeSet2.Contains("ST1"));
		}

		public void TestGetMultipleCodeFilter()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			CusHAWB hAWB1 = Factory.New<CusHAWB>();
			hAWB1.CurrentQueue.P4_QueueName = "QUE";
			hAWB1.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.AN_RefusedDuplicateOrder;
			CusHAWB hAWB2 = Factory.New<CusHAWB>();
			hAWB2.CurrentQueue.P4_QueueName = "QUE";
			hAWB2.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			hAWB2.CurrentQueue.P4_SubStatus = StatusCodeDescriptionPairList.Codes.DT_FaxEmailSent;
			CusHAWB hAWB3 = Factory.New<CusHAWB>();
			hAWB3.CurrentQueue.P4_QueueName = "QUE";
			hAWB3.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient;
			hAWB3.CurrentQueue.P4_SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			CusHAWB decoyHAWB = Factory.New<CusHAWB>();
			decoyHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			decoyHAWB.CurrentQueue.P4_SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			CusHAWB decoyHAWB2 = Factory.New<CusHAWB>();
			decoyHAWB2.CurrentQueue.P4_QueueName = "X";
			decoyHAWB2.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			decoyHAWB2.CurrentQueue.P4_SubStatus = StatusCodeDescriptionPairList.Codes.DT_FaxEmailSent;
			QueueReasonCodeSet reasonCodeSet = new QueueReasonCodeSet("QUE");
			reasonCodeSet.Add(ReasonCodeDescriptionPairList.Codes.AN_RefusedDuplicateOrder);
			reasonCodeSet.Add(ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription);
			QueueStatusCodeSet bA_StatusCodeSet = reasonCodeSet.GetStatusCodeSet(ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription);
			bA_StatusCodeSet.Add(StatusCodeDescriptionPairList.Codes.DT_FaxEmailSent);
			reasonCodeSet.Add(ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient);
			QueueStatusCodeSet s1_StatusCodeSet = reasonCodeSet.GetStatusCodeSet(ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient);
			s1_StatusCodeSet.Add(StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted);
			ZQuery query = reasonCodeSet.GetMultipleCodeFilter(ProcessQueueSchema.P4_QueueName, ProcessQueueSchema.P4_Status, ProcessQueueSchema.P4_SubStatus);
			ProcessQueue[] filterMatches = (UPECargoReportQueue[])Factory.Load(typeof(UPECargoReportQueue), query);
			AssertEquals("Only 3 HAWBs should match", 3, filterMatches.Length);
			AssertCollectionContains("Correct HAWB should match", hAWB1.CurrentQueue, filterMatches);
			AssertCollectionContains("Correct HAWB should match", hAWB2.CurrentQueue, filterMatches);
			AssertCollectionContains("Correct HAWB should match", hAWB3.CurrentQueue, filterMatches);
		}

		public void TestClearReasonClearsStatuses()
		{
			QueueReasonCodeSet reasonCodeSet = new QueueReasonCodeSet("QUE");
			QueueStatusCodeSet statusCodeSet = reasonCodeSet.GetStatusCodeSet("ST");
			statusCodeSet.Add("ST");
			reasonCodeSet.Clear();
			AssertEquals("When the selected reasons are cleared, the selected statuses should also be cascade cleared", false, statusCodeSet.Contains("ST"));
		}
	}
}
