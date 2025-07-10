using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	public class ApportionmentForGatewayPluginTest : ApportionmentPlugInTestBase
	{
		public override void TestName()
		{
			var consol = CreateConsol();
			using (var plugIn = GetTestApportionmentPluginObject(consol))
			{
				AssertEquals("Gateway Sell Apportionment", plugIn.Name);
			}
		}

		public override void TestRefreshGatewayElements()
		{
			var consol = CreateConsol();
			using (var plugIn = GetTestApportionmentPluginObject(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();
				AssertNull(plugIn.TopLevelMenu);

				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(false);
				AssertEquals(false, plugIn.Enabled);
				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(true);
				AssertEquals(true, plugIn.Enabled);
			}
		}

		public override void TestCheckIsUsedForGatewayApportionments()
		{
			var nonGatewayConsol = CreateConsol();
			var sisterCompanyOrgProxy1 = TestObjectCreator.CreateOrgHeader("SISOR1", true, false);
			var sisterCompany1 = TestObjectCreator.CreateNewCompany("SI1", orgProxy: sisterCompanyOrgProxy1);
			var sisterBranch1 = TestObjectCreator.CreateNewBranch(sisterCompany1, "SI1");
			Factory.Save();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany1);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertCheckIsUsedForGatewayApportionments(nonGatewayConsol, true);
				AssertCheckIsUsedForGatewayApportionments(gatewayConsol, true);
			}
		}

		public void TestAutoJRJGatewayApportionmentForClosedJob()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var proxyOrg = TestObjectCreator.CreateOrgHeader("RA_BNE", true, true, "AUBNE");
			var agentPorts = proxyOrg.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = proxyOrg.MainAddress.PK;
			agentPorts.O5_PortOrCountry = "AUBNE";
			agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			agentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var bneBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "BNE");
			bneBranch.GB_OH_OrgProxy = proxyOrg.PK;
			Factory.Save();

			var consol = TestObjectCreator.CreateGatewayConsol("AUSYD", "AUBNE", receivingGatewayAgent: proxyOrg);
			var shipment = TestObjectCreator.CreateShipment("S0001", "AUSYD", "SGSIN", consol, incoTerm: "CFR", housebill: "S0001");
			Factory.Save();

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			using (var gatewayJob = TestObjectCreator.CreateJob(consol))
			{
				shipmentJob.JH_Status = JobHeaderStatus.Codes.Closed;
				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.FRT, 100m, 100m);
				charge.JR_OH_SellAccount = proxyOrg.PK;
				charge.JR_OH_CostAccount = TestObjectCreator.Debtor1.PK;
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				using (var plugIn = new ApportionmentForGatewayPlugin(consol))
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					AssertEquals(ContinueWithSave.Yes, plugIn.ShowPreSaveDialogsCore());
					AssertEquals(@"Closed Job(s) :S0001
You are about to reopen these closed jobs. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

					form.FireSaveButton();
					AssertNoExceptionThrown(() => Factory.Save());

					AssertEquals(JobHeaderStatus.Codes.Working, shipmentJob.JH_Status);
				}
			}
		}

		protected override ApportionmentPlugin GetTestApportionmentPluginObject(IBusiness consol)
		{
			return new ApportionmentForGatewayPlugin(consol);
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}
	}
}
