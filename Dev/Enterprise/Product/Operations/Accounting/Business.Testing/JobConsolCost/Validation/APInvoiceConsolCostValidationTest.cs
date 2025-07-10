using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class APInvoiceConsolCostValidationTest : CommonConsolCostValidationTest
	{
		public void TestCheckE6_ParentIDGatewayConsolError_GatewayAgent()
		{
			TestCheckE6_ParentIDGatewayConsolError(Constants.AgentType.Agent);
		}

		public void TestCheckE6_ParentIDGatewayConsolError_GatewayCoLoad()
		{
			TestCheckE6_ParentIDGatewayConsolError(Constants.AgentType.CoLoad);
		}

		void TestCheckE6_ParentIDGatewayConsolError(string gatewayType)
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0001";
			Factory.Save();
			APInvoiceConsolCosting consolCosting = new APInvoiceConsolCosting(Factory, invoice);
			JobConsolCost cost = consolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			consol.JK_AgentType = gatewayType;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			var gatewayJob = Factory.NewJobForTesting<Job>();
			gatewayJob.Parent = consol;
			Assert(consol.IsGateway());

			cost.Validation.ValidateE6_ParentID();
			AssertNoErrors(cost.E6_ParentIDInfo);
		}

		public void TestCheckE6_ParentIDNotEmpty()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			APInvoiceConsolCosting consolCosting = new APInvoiceConsolCosting(Factory, invoice);
			JobConsolCost cost = consolCosting.ConsolCosts.AddNew();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.E6_ParentID = Guid.Empty;
			}

			AssertHasErrorContaining(cost.E6_ParentIDInfo, "Please enter a Consol");
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.E6_ParentID = Guid.NewGuid();
			}

			AssertHasErrorContaining(cost.E6_ParentIDInfo, "Enter a valid Consol");
		}

		public void TestCheckE6_IsTaxAmountOverridden()
		{
			Env.Security.MaintainConsolJobInvoicingAllowTickOverrideTaxAmountCheckbox.IsAllowed = false;

			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			APInvoiceConsolCosting consolCosting = new APInvoiceConsolCosting(Factory, invoice);
			JobConsolCost cost = consolCosting.ConsolCosts.AddNew();
			cost.E6_IsTaxAmountOverridden = true;
			AssertNoErrors(cost.E6_IsTaxAmountOverriddenInfo);
		}

		public void TestCheckE6_GS_NKConsolCostOwner()
		{
			Env.Security.MaintainConsolJobInvoicingAllowOverrideConsolCostOwner.IsAllowed = false;

			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();
			APInvoiceConsolCosting consolCosting = new APInvoiceConsolCosting(Factory, invoice);
			JobConsolCost cost = consolCosting.ConsolCosts.AddNew();
			cost.E6_GS_NKConsolCostOwner = "ZEN";
			AssertNoErrors(cost.E6_GS_NKConsolCostOwnerInfo);
		}
	}
}