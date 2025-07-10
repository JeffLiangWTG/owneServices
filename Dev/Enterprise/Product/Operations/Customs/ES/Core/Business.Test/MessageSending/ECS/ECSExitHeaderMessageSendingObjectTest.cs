using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(MessageSending.ECSExitHeaderMessageSendingObject))]
	public class ECSExitHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageSending.ECSExitHeaderMessageSendingObject(exitDetail);
		}

		public void TestSendColumn()
		{
			CombineAssertions(() =>
			{
				exitDetail.CED_Status = MessageStatusList.Codes.SentAndRejected;
				testItem = new MessageSending.ECSExitHeaderMessageSendingObject(exitDetail);
				AssertEquals("Detail Status is ERR so should send is not readonly", false, testItem.ShouldSendInfo.ReadOnly);

				exitDetail.CED_Status = MessageStatusList.Codes.AwaitingResponse;
				testItem = new MessageSending.ECSExitHeaderMessageSendingObject(exitDetail);
				AssertEquals("Detail Status is not empty or ERR so should send is readonly", true, testItem.ShouldSendInfo.ReadOnly);
			});
		}

		public void TestProperties()
		{
			var arrivalDate = new ZDate(2020, 11, 13);
			exitDetail.CED_MovementReferenceNumber = "ES12345678912345";
			exitDetail.CED_ArrivalNotificationDate = arrivalDate;
			exitDetail.CED_Status = "AAA";

			CombineAssertions(() =>
			{
				AssertEquals("MRN is correct", "ES12345678912345", testItem.MRN);
				AssertEquals("Arrival Date is correct", arrivalDate, testItem.ArrivalNotificationDate);
				AssertEquals("Status is correct", "AAA", testItem.Status);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_Status = "XXX";

			testItem = new MessageSending.ECSExitHeaderMessageSendingObject(exitDetail);
		}
		MessageSending.ECSExitHeaderMessageSendingObject testItem;
		CusExitDetail exitDetail;
		CusExitControlHeader exitHeader;
	}
}
