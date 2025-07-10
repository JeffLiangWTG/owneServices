using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class QueueStatusCodeSetTest : TestCaseWithFactory
	{
		public void TestGetMultipleCodeFilter()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			CusHAWB hAWB = Factory.New<CusHAWB>();
			hAWB.CurrentQueue.P4_Status = AutoReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient;
			hAWB.CurrentQueue.P4_SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			CusHAWB decoyHAWB = Factory.New<CusHAWB>();
			decoyHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			decoyHAWB.CurrentQueue.P4_SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			QueueStatusCodeSet statusCodeSet = new QueueStatusCodeSet(ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient);
			statusCodeSet.Add(StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted);
			ZQuery query = statusCodeSet.GetMultipleCodeFilter(ProcessQueueSchema.P4_Status, ProcessQueueSchema.P4_SubStatus);
			ProcessQueue[] filterMatches = (UPECargoReportQueue[])Factory.Load(typeof(UPECargoReportQueue), query);
			AssertEquals("Only 1 HAWB should match", 1, filterMatches.Length);
			AssertCollectionContains("Correct HAWB should match", hAWB.CurrentQueue, filterMatches);
		}
	}
}
