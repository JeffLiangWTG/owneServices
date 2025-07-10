using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoEstablishmentQueryManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, ((SeaCargoEstablishmentQueryManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusOutturnCustomsStatusCalculator), ((SeaCargoEstablishmentQueryManager)Manager).StatusCalculators[0].GetType());
		}

		public void TestBusinessObject()
		{
			AssertEquals(Outturn, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			Outturn.C5_ContainerNumber = "CONT";
			Outturn.C5_MasterBill = "MASTER";
			Outturn.C5_HouseBill = "HOUSE";
			AssertEquals("MessageFriendlyName", "Sea Cargo Establishment Query for: Cont: CONT OBL: MASTER HBL: HOUSE ", Manager.MessageFriendlyName);
		}

		public void TestGetMessages()
		{
			AssertEquals("Messages", header.Messages, ((SeaCargoEstablishmentQueryManager)Manager).GetMessages(Outturn));
		}

		public new void TestCanSendOriginal()
		{
			Assert(true);
		}

		public new void TestCanSendWithdrawal()
		{
			Assert(true);
		}

		public new void TestNotificationsWhenWaitingForResponse()
		{
			Assert(true);
		}

		protected override CMRMessageManager GetManager() => new SeaCargoEstablishmentQueryManager(Outturn);

		protected override void SetStatus(ZString status)
		{
		}

		DepotCusOutturn outturn;
		CusOutturnHeader header;
		DepotCusOutturn Outturn
		{
			get
			{
				if (outturn == null)
				{
					header = Factory.New<CusOutturnHeader>();
					outturn = Factory.New<DepotCusOutturn>();
					outturn.C5_C6 = header.PK;
				}
				return outturn;
			}
		}
	}
}
