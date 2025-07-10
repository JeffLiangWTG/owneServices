using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class BLCancelMessageSenderTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestSendMessage()
		{
			PopulateManifestHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
			Factory.Save();

			var messageSender = new BLOriginalMessageSender(bill, new BLArgentinaWrapperManifest(bill));
			var messageResult = messageSender.SendMessage();

			AssertEquals(messageResult, "Message sent successfully");

			AssertEquals(MessageStatusCodeList.Codes.Awaiting, bill.ABL_MessageStatus);

			var createdMessage = bill.Messages.FirstOrDefault() as ARMessage;
			AssertNotNull("Should have created a message.", createdMessage);

			AssertEquals("Message Sub Type should be Empty.", "", createdMessage.EM_MessageSubType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ARCustomsDataRegistry.Instance.ARTestingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
		}

		void PopulateManifestHeader()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_ManifestType = ARManifestTypes.Codes.MAN;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
		}
	}
}
