using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillTriggerActionMessagingSupporterProviderTest : TriggerActionMessagingSupporterProviderTest<CusSCAOceanBill>
	{
		public override void TestTriggerActionMessaging()
		{
			base.TestTriggerActionMessaging();
			Assert("oceanBill HasDeferredScheduledMessageLog", oceanBill.HasDeferredScheduledMessageLog);
		}

		protected override CusSCAOceanBill GetNewSupporter()
		{
			return oceanBill;
		}

		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage; }
		}

		protected override string ExpectedStringInNotifications
		{
			get { return "Generating Cargo messages for Job"; }
		}

		CusSCAOceanBill oceanBill;
		protected override void SetUp()
		{
			base.SetUp();
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff.AddNew().GS_EmailAddress = "abc@abc.com";
			oceanBill = Factory.New<CusSCAOceanBill>();
		}
	}
}
