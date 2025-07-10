using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class BLCancelMessageSenderTest : TestCaseWithFactory
	{
		public void TestCancelManifest()
		{
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "92048000-4";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "SMS-301";
			bill.CustomsEntryNumber = "15716610";

			var messageSender = new BLCancelMessageSender(bill, new BLCancelChileWrapper(bill, "Reason"));
			var messageResult = messageSender.SendMessage();

			AssertEquals(messageResult, "Message sent successfully");

			AssertEquals(MessageTypes.Codes.CHC, bill.Messages[0].EM_MessageType);
			AssertEquals("Message Sub Type should be Empty", "", bill.Messages[0].EM_MessageSubType);
		}
	}
}
