using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManTranHeadSEAIARManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestBusinessObject()
		{
			AssertEquals(TransportHeader, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Impending Arrival Report", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(TransportHeader);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEAIARMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(TransportHeader);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEAIARMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(TransportHeader);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEAIARMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManTranHeadSEAIARManager)Manager).GetStatus());
		}

		public void TestResetToOriginal()
		{
			TransportHeader.Messages.AddNew(typeof(CMRSEAIARMessage));
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManTranHeadSEAIARManager)Manager).GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, ((CusSeaManTranHeadSEAIARManager)Manager).GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, TransportHeader.Messages[0].EM_Status);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, ((CusSeaManTranHeadSEAIARManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusSeaManTranHeadStatusCalculator), ((CusSeaManTranHeadSEAIARManager)Manager).StatusCalculators[0].GetType());
		}

		public void TestSendingWithNoArrivalPorts()
		{
			AssertEquals("precondition arrivals", 0, TransportHeader.Arrivals.Count);
			var manager = GetManager();

			var result = manager.GetNotificationsForSendingAnOriginal();
			var previousErrorCount = result.ErrorCount;
			Assert("notification", result.ContainsError("At least one port of arrival is required."));

			TransportHeader.Arrivals.AddNew();
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("no notification", (previousErrorCount - 1), result.ErrorCount);
		}

		protected override void SetStatus(ZString status)
		{
			TransportHeader.ImpendingArrivalResponseStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new CusSeaManTranHeadSEAIARManager(TransportHeader);

		CusSeaManTranHead transportHeader;
		CusSeaManTranHead TransportHeader => transportHeader ?? (transportHeader = Factory.New<CusSeaManTranHead>());
	}
}
