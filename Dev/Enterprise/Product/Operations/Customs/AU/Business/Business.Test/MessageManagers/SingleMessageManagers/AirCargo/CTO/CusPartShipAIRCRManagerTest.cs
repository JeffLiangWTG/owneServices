using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusPartShipAIRCRManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestBusinessObject()
		{
			AssertEquals(PartShip, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", $"Air Cargo Report for MAWB:  (part shipment for flight: QF123, 4/07/2005)", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(PartShip);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRCRMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(PartShip);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRCRMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(PartShip);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRCRMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", Manager.GetStatus());
		}

		public void TestResetToOriginal()
		{
			PartShip.Messages.AddNew(typeof(CMRAIRCRMessage));
			SetStatus("123");
			AssertEquals("GetStatus()", "123", Manager.GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, Manager.GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, PartShip.Messages[0].EM_Status);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, Manager.StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusPartShipStatusCalculator), Manager.StatusCalculators[0].GetType());
		}

		protected override void SetStatus(ZString status)
		{
			PartShip.CG_CustomsStatus = status;
		}

		protected override CMRMessageManager GetManager() => new CusPartShipAIRCRManager(PartShip);

		new CusPartShipAIRCRManager Manager => (CusPartShipAIRCRManager)base.Manager;

		CusPartShip partShip;
		CusPartShip PartShip
		{
			get
			{
				if (partShip == null)
				{
					var mawb = Factory.New<CTOCusMAWB>();
					var hawb = mawb.ChildBills.AddNew();
					partShip = hawb.PartShips.AddNew();
					partShip.CG_FlightNo = "QF123";
					partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 4);
				}
				return partShip;
			}
		}
	}
}
