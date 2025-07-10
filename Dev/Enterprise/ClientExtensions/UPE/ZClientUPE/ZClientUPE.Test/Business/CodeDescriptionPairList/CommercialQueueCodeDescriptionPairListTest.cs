using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class CommercialQueueCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestOrder()
		{
			CommercialQueueCodeDescriptionPairList list = new CommercialQueueCodeDescriptionPairList();
			AssertEquals(9, list.Count);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Finance, list[0].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Hold, list[1].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.EIR, list[2].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.AR, list[3].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Chase, list[4].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.OnFile, list[5].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Rebill, list[6].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.AlternateBroker, list[7].Code);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Completed, list[8].Code);
		}

		public void TestCompletedQueueNames()
		{
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.CompletedQueueNames[0]);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.OnFile, CommercialQueueCodeDescriptionPairList.CompletedQueueNames[1]);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Chase, CommercialQueueCodeDescriptionPairList.CompletedQueueNames[2]);
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Rebill, CommercialQueueCodeDescriptionPairList.CompletedQueueNames[3]);
		}
	}
}
