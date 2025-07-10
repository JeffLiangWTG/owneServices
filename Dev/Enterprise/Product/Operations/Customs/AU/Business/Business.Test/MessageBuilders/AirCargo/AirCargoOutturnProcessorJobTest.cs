using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoOutturnProcessorJobTest : TestCaseWithFactory
	{
		public void TestSendChildren()
		{
			var masterBill = Factory.New<CusMAWB>();
			Assert(new AirCargoProcessorJob(masterBill).SendChildren);
		}

		public void TestProcessAcceptableStandAloneCusMAWB()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "1234D";
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			masterBill.CM_ArrivalDate = DateTime.Today;
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_MAWB = "081-1324 5676";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_ResponsiblePartyID = "87003014042";
			var underbond1 = masterBill.Underbonds.AddNew();
			var underbond2 = masterBill.Underbonds.AddNew();
			underbond1.C4_OriginPremiseID = "1234D";
			underbond1.C4_DestinationPremiseID = "1234D";
			underbond2.C4_OriginPremiseID = "1234D";
			underbond2.C4_DestinationPremiseID = "1234D";

			masterBill.RunPreSaveValidation();
			Assert("Pre-condition", !masterBill.HasErrors && !masterBill.HasMessageErrorsNotIncludingChildren);
			Assert("Pre-condition", !underbond1.HasErrors && !underbond1.HasMessageErrors);
			Assert("Pre-condition", !underbond2.HasErrors && !underbond2.HasMessageErrors);

			var notify = new NotificationBuffer();
			new HouseBillsCargoMessageProcessor(notify).Process(new AirCargoOutturnProcessorJob(masterBill));

			AssertEquals("1 message is sent", 1, underbond1.Messages.Count);
			AssertEquals("1 message is sent", 1, underbond2.Messages.Count);
		}

		public void TestProcessNonAcceptableStandAloneCusMAWB()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "1234D";
			var masterBill = Factory.New<CusMAWB>();
			var underbond1 = masterBill.Underbonds.AddNew();
			var underbond2 = masterBill.Underbonds.AddNew();
			underbond1.C4_OriginPremiseID = "1234D";
			underbond1.C4_DestinationPremiseID = "1234D";
			underbond2.C4_OriginPremiseID = "1234D";
			underbond2.C4_DestinationPremiseID = "1234D";

			masterBill.RunPreSaveValidation();
			Assert("Pre-condition", masterBill.HasErrors && masterBill.HasMessageErrorsNotIncludingChildren);
			Assert("Pre-condition", !underbond1.HasErrors && !underbond1.HasMessageErrors);
			Assert("Pre-condition", !underbond2.HasErrors && !underbond2.HasMessageErrors);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var notify = new NotificationBuffer();
			new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.AirCargoSendErrorsToGroup).Process(new AirCargoOutturnProcessorJob(masterBill), notify, "TST", new LoggerForTest());

			AssertEquals("No message is sent", 0, underbond1.Messages.Count);
			AssertEquals("No message is sent", 0, underbond2.Messages.Count);
			AssertNotNull("Email is sent for notifying errors", Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == "AU Cargo Send errors or warnings"));
		}

		public void TestProcessWhenThereIsErrorWithUnderbond()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "1234D";
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			masterBill.CM_ArrivalDate = DateTime.Today;
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_MAWB = "081-1324 5676";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_ResponsiblePartyID = "87003014042";
			var underbond1 = masterBill.Underbonds.AddNew();
			var underbond2 = masterBill.Underbonds.AddNew();
			underbond1.C4_OriginPremiseID = "1234D";
			underbond1.C4_DestinationPremiseID = "1234D";
			underbond2.C4_DestinationPremiseID = "1234D";

			masterBill.RunPreSaveValidation();
			Assert("Pre-condition", !masterBill.HasErrors && !masterBill.HasMessageErrorsNotIncludingChildren);
			Assert("Pre-condition", !underbond1.HasErrors && !underbond1.HasMessageErrors);
			Assert("Pre-condition", !underbond2.HasErrors && underbond2.HasMessageErrors);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var notify = new NotificationBuffer();
			new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.AirCargoSendErrorsToGroup).Process(new AirCargoOutturnProcessorJob(masterBill), notify, "TST", new LoggerForTest());

			AssertEquals("1 message is sent", 1, underbond1.Messages.Count);
			AssertEquals("No message is sent", 0, underbond2.Messages.Count);
			AssertNotNull("Email is sent for notifying errors", Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == "AU Cargo Send errors or warnings"));
		}

		public void TestProcessWhenThereIsErrorWithOutturn()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "1234D";
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			masterBill.CM_ArrivalDate = DateTime.Today;
			masterBill.CM_FlightNo = "QF2";
			masterBill.CM_MAWB = "081-1324 5676";
			masterBill.CM_RL_NKDischargePort = "AUSYD";
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_ResponsiblePartyID = "87003014042";
			var underbond1 = masterBill.Underbonds.AddNew();
			var underbond2 = masterBill.Underbonds.AddNew();
			underbond1.C4_OriginPremiseID = "1234D";
			underbond1.C4_DestinationPremiseID = "1234D";
			underbond1.C4_Outurned = ZDateTime.Now;
			underbond2.C4_OriginPremiseID = "1234D";
			underbond2.C4_DestinationPremiseID = "1234D";
			underbond2.C4_Outurned = ZDateTime.Now;
			var outturn1 = underbond1.Outturns.AddNew();
			outturn1.C5_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			outturn1.C5_ParentID = masterBill.PK;
			var outturn2 = underbond2.Outturns.AddNew();
			outturn2.C5_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			outturn2.C5_ParentID = masterBill.PK;
			outturn2.C5_PackagesOutturned = -1;

			masterBill.RunPreSaveValidation();
			Assert("Pre-condition", !masterBill.HasErrors && !masterBill.HasMessageErrorsNotIncludingChildren);
			Assert("Pre-condition", !underbond1.HasErrors && !underbond1.HasMessageErrors);
			Assert("Pre-condition", !underbond2.HasErrors && underbond2.HasMessageErrors);
			Assert("Pre-condition", !outturn1.HasErrors && !outturn1.HasMessageErrors);
			Assert("Pre-condition", !outturn2.HasErrors && outturn2.HasMessageErrors);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var notify = new NotificationBuffer();
			new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.AirCargoSendErrorsToGroup).Process(new AirCargoOutturnProcessorJob(masterBill), notify, "TST", new LoggerForTest());

			AssertEquals("1 message is sent", 1, underbond1.Messages.Count);
			AssertEquals("No message is sent", 0, underbond2.Messages.Count);
			AssertNotNull("Email is sent for notifying errors", Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == "AU Cargo Send errors or warnings"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";
		}
	}
}
