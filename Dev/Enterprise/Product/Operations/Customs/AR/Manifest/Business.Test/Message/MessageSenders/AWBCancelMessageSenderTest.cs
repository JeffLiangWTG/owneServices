using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class AWBCancelMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendMessage()
		{
			var header = PopulateManifestHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
			Factory.Save();

			var messageSender = new AWBCancelMessageSender(bill, new HouseWaybillWrapper(bill, MessageSubTypeCodes.Codes.Cancellation));
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
		}

		AsycudaManifestHeader PopulateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_ManifestType = ARManifestTypes.Codes.MAN;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_MasterBillIssueDate = new ZDate(2023, 01, 12);
			header.AMA_E_ARV = new ZDate(2023, 01, 12);
			header.AMA_E_DEP = new ZDate(2023, 01, 11);

			return header;
		}
	}
}
