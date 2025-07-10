using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoForwarderUMBREQRMessageInterest : MessageFactoryInterestTest
	{
		public override void TestIsInterested()
		{
			SeaCargoForwarderUnderbondApprovalFactory creator = new SeaCargoForwarderUnderbondApprovalFactory();
			AssertEquals("HAWB Interest", false, creator.IsInterestedInUBMREQRInternal(HAWBUnderbondApproval));
			AssertEquals("SEA Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
			AssertEquals("SEA Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910));

			CreateOBL250805001ForwardingConsol(Factory);
			var synchroniser = new CMRSeaCargoSynchroniser(Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, OBL250805001_OceanBillNum)));
			synchroniser.LoadHouseBills();
			AssertEquals("Sea Forwarder Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
			var container = synchroniser.OceanBill.Containers.Find(OBL250805001_Container3);
			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_SendersMessageReference = "L5040040C0003/3";
			AssertEquals("Sea Forwarder Interest", true, creator.IsInterestedInUBMREQRInternal(OBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910));
		}
	}
}
