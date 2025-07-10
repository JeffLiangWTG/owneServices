using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBAIRCRMessageManagerTest : CusHAWBBaseAIRCRManagerAbstractTest
	{
		public override void TestMessageFriendlyName()
		{
			HAWB.CS_HAWB = "08133333333";
			AssertEquals("MessageFriendlyName", "Air Cargo Report for HAWB: 08133333333", Manager.MessageFriendlyName);
		}

		public void TestOverdueCargoReportExceptionCore()
		{
			var testingDate = new ZDateTime(2008, 1, 15, 1, 2, 3);
			var craEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.CargoReportAccepted.Code);
			craEvent.SE_AirExceptionSafetyMargin = 24;
			var manager = GetManager();
			AssertNull("Null returned when no shipment attached", manager.OverdueCargoReportException);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			HAWB.CS_JS = shipment.PK;
			var mileStoneProcessTask = (ForwardingShipmentProcessTask)shipment.WorkflowItems.Milestones.AddNew();
			mileStoneProcessTask.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(testingDate));
			mileStoneProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			var exceptionProcessTask = (ForwardingShipmentProcessTask)mileStoneProcessTask.CreateMilestoneException();
			AssertEquals(mileStoneProcessTask.P9_SE_NKMilestoneEvent, exceptionProcessTask.TriggerConditions.TriggerEventCode);
			AssertEquals("Correct exception task", exceptionProcessTask, manager.OverdueCargoReportException);
		}

		protected override CMRMessageManager GetManager() => GetManager(HAWB);

		protected override CMRMessageManager GetManager(CusHAWBBase houseBill) => new CusHAWBAIRCRMessageManager(houseBill as CusHAWB);

		CusHAWB hawb;
		protected override CusHAWBBase HAWB
		{
			get
			{
				if (hawb == null)
				{
					var mawb = Factory.New<CusMAWB>();
					hawb = mawb.ChildBills.AddNew();
				}
				return hawb;
			}
		}
	}
}
