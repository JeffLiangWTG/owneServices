using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManArrivalPortCARLSTManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestBusinessObject()
		{
			AssertEquals(Arrival, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			Arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("MessageFriendlyName", "Cargo List Report for Port - AUSYD", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(Arrival);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRCARLSTMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(Arrival);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRCARLSTMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(Arrival);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRCARLSTMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManArrivalPortCARLSTManager)Manager).GetStatus());
		}

		public void TestResetToOriginal()
		{
			Arrival.Messages.AddNew(typeof(CMRCARLSTMessage));
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManArrivalPortCARLSTManager)Manager).GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, ((CusSeaManArrivalPortCARLSTManager)Manager).GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, Arrival.Messages[0].EM_Status);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, ((CusSeaManArrivalPortCARLSTManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusSeaManArrivalPortCargoListStatusCalculator), ((CusSeaManArrivalPortCARLSTManager)Manager).StatusCalculators[0].GetType());
		}

		public void TestSendingWithNoOceanBills()
		{
			AssertEquals("Precondition", 0, Arrival.Header.OceanBills.Count);

			var manager = GetManager();
			Customs.Business.MessageSendingNotificationCollection result = manager.GetNotificationsForSendingAnOriginal();
			int previousErrorCount = result.ErrorCount;
			Assert("notifications ocean bill", result.ContainsError("At least one cargo list line is required."));

			Arrival.CargoLines.AddNew();
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("no notifications", (previousErrorCount - 1), result.ErrorCount);
		}

		protected override void SetStatus(ZString status)
		{
			Arrival.CargoListStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new CusSeaManArrivalPortCARLSTManager(Arrival);

		CusSeaManArrivalPort arrival;
		CusSeaManArrivalPort Arrival
		{
			get
			{
				if (arrival == null)
				{
					var header = Factory.New<CusSeaManTranHead>();
					arrival = header.Arrivals.AddNew();
				}
				return arrival;
			}
		}
	}
}
