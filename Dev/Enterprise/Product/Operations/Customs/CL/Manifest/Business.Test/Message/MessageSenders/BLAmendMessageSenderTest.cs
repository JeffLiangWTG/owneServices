using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLAmendMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendManifest()
		{
			var header = PopulateManifestHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";

			var messageSender = new BLAmendMessageSender(bill, new BLSendChileWrapper(bill, WrappersConstants.ActionType.M, nameof(WrappersConstants.ObservationName.Mot), "ACL: SE MODIFICA TIPO SERVICIO DE FCL/LCL A FCL/FCL"));
			var messageResult = messageSender.SendMessage();

			AssertEquals(messageResult, "Message sent successfully");

			AssertEquals(MessageTypes.Codes.CHA, bill.Messages[0].EM_MessageType);
			AssertEquals("Message Sub Type should be Empty", "", bill.Messages[0].EM_MessageSubType);
		}

		AsycudaManifestHeader PopulateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CNT-001";
			container.ACN_EmptyFullIndicator = "FCL";

			return header;
		}
	}
}
