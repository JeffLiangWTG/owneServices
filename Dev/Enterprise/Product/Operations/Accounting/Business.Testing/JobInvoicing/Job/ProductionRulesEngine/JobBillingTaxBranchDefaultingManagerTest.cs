using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class JobBillingTaxBranchDefaultingManagerTest : JobBillingDefaultingManagerTest
	{
		protected override JobBillingDefaultingManager GetDefaultingManager(IFactLoaderProvider factLoaderProvider)
		{
			return new JobBillingTaxBranchDefaultingManager(factLoaderProvider);
		}

		protected override string Context => "JTB";

		public void TestDefaulting_SHP_EndToEnd_SendingAgent()
		{
			var sendingAgent = TestObjectCreator.ABIGAS;
			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet1 = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 1);
			TestObjectCreator.CreateProductionRules(ruleSet1, Context, 1, "ConsolSendingAgent", sendingAgent.PK, PK2);

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C00001000");
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment, false);

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);

			defaultingManager.SetDefaultValue(shipment, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(PK2, defaultingManager.DefaultValue);
		}

		public void TestDefaulting_SHP_EndToEnd_RecevingAgent()
		{
			var receivingAgent = TestObjectCreator.ABIGAS;
			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet1 = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 1);
			TestObjectCreator.CreateProductionRules(ruleSet1, Context, 1, "ConsolReceivingAgent", receivingAgent.PK, PK2);

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C00001000");
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment, false);

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);

			defaultingManager.SetDefaultValue(shipment, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(PK2, defaultingManager.DefaultValue);
		}

		protected override Guid PK1 => Branch1.PK.ToGuid();
		protected override Guid PK2 => Branch2.PK.ToGuid();

		public void TestJobTaxBranch_SHP_EndToEnd()
		{
			var jobBranch = GlbBranch.CurrentBranch;
			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet1 = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 1);
			TestObjectCreator.CreateProductionRules(ruleSet1, Context, 1, "JobBranch", jobBranch.PK, PK1);

			var ruleSet2 = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 2, GlbCompany.CurrentCompany.PK);
			TestObjectCreator.CreateProductionRules(ruleSet2, Context, 2, "JobBranch", jobBranch.PK, PK2);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_GB = jobBranch.PK;

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);

			defaultingManager.SetDefaultValue(shipment, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(PK2, defaultingManager.DefaultValue);
		}

		GlbBranch Branch1 => branch1 ?? (branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany));
		GlbBranch branch1;

		GlbBranch Branch2 => branch2 ?? (branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany));
		GlbBranch branch2;
	}
}
