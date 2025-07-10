using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManOBLHeaderSEACRManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestRequiresAmendmentOnlyWhenHasChanges()
		{
			var oceanBill2 = OceanBill.TransportHeader.OceanBills.AddNew();
			SetStatus("ACO");

			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var oceanBillOnFactory2 = factory2.Load<CusSeaManOBLHeader>(OceanBill.PK);
			oceanBillOnFactory2.BO_OceanBill = "foo";
			factory2.Save();

			AssertEquals(false, OceanBill.HasChanges);
			AssertEquals(false, OceanBill.TransportHeader.HasChanges);
			AssertEquals(false, Manager.RequiresAmendment());

			OceanBill.HasChanges = true;
			AssertEquals(true, Manager.RequiresAmendment());

			OceanBill.HasChanges = false;
			AssertEquals(false, Manager.RequiresAmendment());

			OceanBill.TransportHeader.HasChanges = true;
			AssertEquals(true, Manager.RequiresAmendment());

			OceanBill.TransportHeader.HasChanges = false;
			AssertEquals(false, Manager.RequiresAmendment());

			oceanBill2.HasChanges = true;
			AssertEquals(true, OceanBill.TransportHeader.HasChanges);
			AssertEquals(false, ((IBusinessObjectState)OceanBill.TransportHeader).HasChangesNotIncludingChildren);
			AssertEquals(false, Manager.RequiresAmendment());
		}

		public void TestBusinessObject()
		{
			AssertEquals(OceanBill, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Sea Cargo Report for: ", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(OceanBill);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEACRMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(OceanBill);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEACRMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(OceanBill);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEACRMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManOBLHeaderSEACRManager)Manager).GetStatus());
		}

		public void TestResetToOriginal()
		{
			OceanBill.Messages.AddNew(typeof(CMRSEACRMessage));
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSeaManOBLHeaderSEACRManager)Manager).GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, ((CusSeaManOBLHeaderSEACRManager)Manager).GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, OceanBill.Messages[0].EM_Status);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 2, ((CusSeaManOBLHeaderSEACRManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusSeaManOBLHeaderStatusCalculator), ((CusSeaManOBLHeaderSEACRManager)Manager).StatusCalculators[0].GetType());
			AssertEquals("StatusCalculators", typeof(CusSeaManOBLHeaderShipmentStatusCalculator), ((CusSeaManOBLHeaderSEACRManager)Manager).StatusCalculators[1].GetType());
		}

		public void TestSendingWithNoContainers()
		{
			AssertEquals("precondition", 0, OceanBill.Details.Count);

			var manager = GetManager();
			var result = manager.GetNotificationsForSendingAnOriginal();
			var previousErrorCount = result.ErrorCount;
			Assert("notifications container", result.ContainsError("Ocean bill must have container or bulk/breakbulk details entered."));

			OceanBill.Details.AddNew();
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("no notifications", previousErrorCount - 1, result.ErrorCount);
		}

		protected override void SetStatus(ZString status)
		{
			OceanBill.CargoReportStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new CusSeaManOBLHeaderSEACRManager(OceanBill);

		CusSeaManOBLHeader oceanBill;
		CusSeaManOBLHeader OceanBill
		{
			get
			{
				if (oceanBill == null)
				{
					var header = Factory.New<CusSeaManTranHead>();
					oceanBill = header.OceanBills.AddNew();
				}
				return oceanBill;
			}
		}
	}
}
