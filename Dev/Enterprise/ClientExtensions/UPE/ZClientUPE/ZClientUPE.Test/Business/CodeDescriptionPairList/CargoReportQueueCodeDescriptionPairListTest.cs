using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class CargoReportQueueCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestOrder()
		{
			CargoReportQueueCodeDescriptionPairList list = new CargoReportQueueCodeDescriptionPairList();
			AssertEquals(9, list.Count);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Intervention, list[0].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Pending, list[1].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Unknown, list[2].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Hold, list[3].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.EIR, list[4].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.AwaitingDeclaration, list[5].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, list[6].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, list[7].Code);
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Completed, list[8].Code);
		}
	}
}
