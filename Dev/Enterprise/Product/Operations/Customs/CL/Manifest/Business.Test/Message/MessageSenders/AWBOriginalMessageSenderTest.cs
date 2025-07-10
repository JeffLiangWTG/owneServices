using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBOriginalMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";

			var messageSender = new AWBOriginalMessageSender(bill, new AWBSendChileWrapper(bill, WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Mot));
			var messageResult = messageSender.SendMessage();

			AssertEquals(messageResult, "Message sent successfully");

			AssertEquals(MessageTypes.Codes.CHE, bill.Messages[0].EM_MessageType);
			AssertEquals("Message Sub Type should be Empty", "", bill.Messages[0].EM_MessageSubType);
		}
	}
}
