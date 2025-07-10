using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManArrivalPortSEAAARManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestCannotSendImpendingArrivalReportUnlessBA_ArrivalPortATAIsSet()
		{
			var port = Factory.New<CusSeaManArrivalPort>();
			var manager = new CusSeaManArrivalPortSEAAARManager(port);
			var notifications = manager.GetNotificationsForSendingAnOriginal();
			Assert("ata port info", notifications.ContainsError("You cannot send unless ATA Port Information has been set."));

			port.BA_ArrivalPortATA = new ZDateTime(2005, 02, 03);
			notifications = manager.GetNotificationsForSendingAnOriginal();
			Assert("ata port info", !notifications.ContainsError("You cannot send unless ATA Port Information has been set."));

			port.BA_ArrivalPortATA = ZDateTime.Invalid;
			notifications = manager.GetNotificationsForSendingAnOriginal();
			Assert("ata port info", notifications.ContainsError("You cannot send unless ATA Port Information has been set."));
		}

		public void TestBusinessObject()
		{
			AssertEquals(Arrival, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			Arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("MessageFriendlyName", "Actual Arrival Report (AUSYD)", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(Arrival);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEAAARMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(Arrival);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEAAARMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(Arrival);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEAAARMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManArrivalPortSEAAARManager)Manager).GetStatus());
		}

		public void TestResetToOriginal()
		{
			Arrival.Messages.AddNew(typeof(CMRSEAAARMessage));
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManArrivalPortSEAAARManager)Manager).GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, ((CusSeaManArrivalPortSEAAARManager)Manager).GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, Arrival.Messages[0].EM_Status);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, ((CusSeaManArrivalPortSEAAARManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusSeaManArrivalPortStatusCalculator), ((CusSeaManArrivalPortSEAAARManager)Manager).StatusCalculators[0].GetType());
		}

		protected override void SetStatus(ZString status)
		{
			Arrival.ActualArrivalResponseStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new CusSeaManArrivalPortSEAAARManager(Arrival);

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
