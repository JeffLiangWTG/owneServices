using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoProcessorJobTest : TestCaseWithFactory
	{
		public void TestSendChildren()
		{
			var masterBill = Factory.New<CusMAWB>();
			Assert(new AirCargoProcessorJob(masterBill).SendChildren);
		}

		public void TestProcessAcceptableStandAloneCusMAWB()
		{
			var helper = new ZTestHelper(Factory);
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			masterBill.CM_ArrivalDate = DateTime.Today;
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_MAWB = "081-1324 5676";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_ResponsiblePartyID = "87003014042";
			masterBill.RunPreSaveValidation();

			Assert(!masterBill.HasErrors);
			Assert(!masterBill.HasMessageErrors);

			var houseBill = helper.CreateTestHouseBill(masterBill);
			houseBill.RunPreSaveValidation();
			Assert(!houseBill.HasErrors);
			Assert(!houseBill.HasMessageErrors);

			var houseBill2 = helper.CreateTestHouseBill(masterBill);
			houseBill2.RunPreSaveValidation();
			Assert(!houseBill2.HasErrors);
			Assert(!houseBill2.HasMessageErrors);

			var notify = new NotificationBuffer();
			new HouseBillsCargoMessageProcessor(notify).Process(new AirCargoProcessorJob(masterBill));

			AssertEquals("one message is sent", 1, houseBill.Messages.Count);
			AssertEquals("one message is sent", 1, houseBill2.Messages.Count);
		}

		public void TestProcessNonAcceptableStandAloneCusMAWB()
		{
			var helper = new ZTestHelper(Factory);
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			masterBill.CM_ArrivalDate = DateTime.Today;
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_MAWB = "";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_ResponsiblePartyID = "87003014042";
			masterBill.RunPreSaveValidation();

			Assert(masterBill.HasErrors);
			Assert(!masterBill.HasMessageErrors);

			var houseBill = helper.CreateTestHouseBill(masterBill);
			houseBill.RunPreSaveValidation();
			Assert(!houseBill.HasErrors);
			Assert(!houseBill.HasMessageErrors);

			var houseBill2 = helper.CreateTestHouseBill(masterBill);
			houseBill2.RunPreSaveValidation();
			Assert(!houseBill2.HasErrors);
			Assert(!houseBill2.HasMessageErrors);

			var notify = new NotificationBuffer();
			new HouseBillsCargoMessageProcessor(notify).Process(new AirCargoProcessorJob(masterBill));

			AssertEquals("no message is sent", 0, houseBill.Messages.Count);
			AssertEquals("no message is sent", 0, houseBill2.Messages.Count);
		}

		public void TestProcessWhenThereIsOneHouseBillWithMessageError()
		{
			var helper = new ZTestHelper(Factory);
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			masterBill.CM_ArrivalDate = DateTime.Today;
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_MAWB = "081-1324 5676";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_ResponsiblePartyID = "87003014042";
			masterBill.RunPreSaveValidation();

			Assert(!masterBill.HasErrors);
			Assert(!masterBill.HasMessageErrors);

			var houseBill = helper.CreateTestHouseBill(masterBill);
			houseBill.RunPreSaveValidation();
			Assert(!houseBill.HasErrors);
			Assert(!houseBill.HasMessageErrors);

			var houseBill2 = helper.CreateTestHouseBill(masterBill);
			houseBill2.CS_GoodsDescription = "";
			houseBill2.RunPreSaveValidation();
			Assert(!houseBill2.HasErrors);
			Assert(houseBill2.HasMessageErrors);

			var notify = new NotificationBuffer();
			new HouseBillsCargoMessageProcessor(notify).Process(new AirCargoProcessorJob(masterBill));

			AssertEquals("one message is sent", 1, houseBill.Messages.Count);
			AssertEquals("No message is sent", 0, houseBill2.Messages.Count);
		}
	}
}
