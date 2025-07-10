using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseSEACRManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestBusinessObject()
		{
			AssertEquals(House, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			House.CA_HouseBill = "123";
			AssertEquals("MessageFriendlyName", "Sea Cargo Report for housebill: 123", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(House);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEACRMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(House);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEACRMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(House);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRSEACRMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSCAHouseSEACRManager)Manager).GetStatus());
		}

		public void TestResetToOriginal()
		{
			House.Messages.AddNew(typeof(CMRSEACRMessage));
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusSCAHouseSEACRManager)Manager).GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, ((CusSCAHouseSEACRManager)Manager).GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, House.Messages[0].EM_Status);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 3, ((CusSCAHouseSEACRManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusSCAHouseMessageStatusCalculator), ((CusSCAHouseSEACRManager)Manager).StatusCalculators[0].GetType());
			AssertEquals("StatusCalculators", typeof(CusSCAPivotStatusCalculator), ((CusSCAHouseSEACRManager)Manager).StatusCalculators[1].GetType());
			AssertEquals("StatusCalculators", typeof(CusSCAPivotStatusCalculator), ((CusSCAHouseSEACRManager)Manager).StatusCalculators[1].GetType());
		}

		public void TestWeCantSendCargoReportsForMultiOBLUnpacks()
		{
			const string expectedError = "You can't send this cargo report because the ocean bill unpack checkbox has been selected";
			House.OceanBill.CB_MultiOBLUnpack = true;
			AssertEquals("Errors includes '" + expectedError + "'", true, Manager.GetNotificationsForSendingAnOriginal().ContainsError(expectedError));
		}

		public void TestNotifications()
		{
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11111111111111");
			AssertEquals(true, ManagerTest.GetNotificationsForSendingAnOriginal().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "");
			AssertEquals(true, ManagerTest.GetNotificationsForSendingAnOriginal().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered."));

			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "1234");
				AssertEquals(0, ManagerTest.GetNotificationsForSendingAnOriginal().WarningCount);
			}
		}

		public void TestOverdueCargoReportExceptionCore()
		{
			var testingDate = new ZDateTime(2008, 1, 15, 1, 2, 3);
			var craEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.CargoReportAccepted.Code);
			craEvent.SE_SeaExceptionSafetyMargin = 24;
			var manager = GetManager();
			AssertNull("Null returned when no shipment attached", manager.OverdueCargoReportException);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			House.CA_JS = shipment.PK;
			var mileStoneProcessTask = (ForwardingShipmentProcessTask)shipment.WorkflowItems.Milestones.AddNew();
			mileStoneProcessTask.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(testingDate));
			mileStoneProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			var exceptionProcessTask = (ForwardingShipmentProcessTask)mileStoneProcessTask.CreateMilestoneException();
			AssertEquals(mileStoneProcessTask.P9_SE_NKMilestoneEvent, exceptionProcessTask.TriggerConditions.TriggerEventCode);
			AssertEquals("Correct exception task", exceptionProcessTask, manager.OverdueCargoReportException);
		}

		protected override void SetStatus(ZString status)
		{
			House.CA_MessageStatus = status;
		}

		protected override CMRMessageManager GetManager() => new CusSCAHouseSEACRManager(House);

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}

		CusSCAHouseSEACRManager manger;
		CusSCAHouseSEACRManager ManagerTest => manger ?? (manger = new CusSCAHouseSEACRManager(House));

		CusSCAHouse house;
		CusSCAHouse House
		{
			get
			{
				if (house == null)
				{
					var oceanBill = Factory.New<CusSCAOceanBill>();
					house = oceanBill.HouseBills.AddNew();
					house.Pivot.AddNew();
					house.Pivot.AddNew();
				}
				return house;
			}
		}
	}
}
