using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECusHAWBTypeDeciderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertEquals(typeof(UPECusHAWB), Factory.New(typeof(CusHAWB)).GetType());
		}

		public void TestLoad()
		{
			var hawb = Factory.New<CusHAWB>();
			Factory.Save();
			AssertEquals(typeof(UPECusHAWB), new BusinessObjectFactory().Load(typeof(CusHAWB), hawb.PK).GetType());
			UPECusHAWB uPECusHAWB = hawb as UPECusHAWB;
			uPECusHAWB.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			uPECusHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;
			Factory.Save();
			AssertEquals(typeof(Callout), new BusinessObjectFactory().Load(typeof(CusHAWB), hawb.PK).GetType());
			uPECusHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			Factory.Save();
			AssertEquals(typeof(UPECusHAWB), new BusinessObjectFactory().Load(typeof(CusHAWB), hawb.PK).GetType());
		}
	}
}
