using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBTriggerActionMessagingSupporterProviderTest : TriggerActionMessagingSupporterProviderTest<CusMAWB>
	{
		public override void TestTriggerActionMessaging()
		{
			base.TestTriggerActionMessaging();
			Assert(mawb.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoReportLogReference));
			Assert(mawb.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoOutturnLogReference));
		}

		protected override CusMAWB GetNewSupporter() => mawb;

		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
				yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage;
			}
		}

		protected override string ExpectedStringInNotifications => "";

		CusMAWB mawb;
		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();
		}
	}
}
