using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.GatewayBilling.Testing
{
	public class GatewaySellToCostSynchroniserTest : TestCaseWithFactory
	{
		public void TestReverseSellApportionmentDeletesChargeAndConsolCostTogether()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				var consol = GatewayConsol;
				Assert("Pre-condition", consol.IsGateway());
				try
				{
					using (gatewayJob = TestObjectCreator.CreateJob(consol))
					{
						gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
						gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;
						var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m, true);
						charge.JR_OSCostAmt = 0m;
						charge.JR_OH_SellAccount = GatewayAgent.PK;
						Factory.Save();

						var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
						AssertEquals(1, consolCosts.Length);
						var consolCost = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC1.PK);
						AssertNotNull(consolCost);
						AssertEquals(250m, consolCost.E6_OSCostAmount);
						AssertEquals(charge.PK, consolCost.E6_GatewaySellChargeID);

						AssertEquals(1, gatewayJob.Charges.Count);
						AssertEquals(consolCost.PK, charge.JR_E6_GatewaySellHeader);
						Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);

						var newFactory = new BusinessObjectFactory();
						consol = newFactory.Load<ForwardingConsol>(consol.PK);
						gatewayJob = newFactory.Load<Job>(gatewayJob.PK);
						charge = newFactory.Load<Charge>(charge.PK);
						consolCost = newFactory.Load<JobConsolCost>(consolCost.PK);
						var journal = newFactory.Load<JobRevenueJournal>(charge.ARLine.TransactionHeader.PK);
						journal.GenerateReverseTransaction(true);
						Assert("charge should be deleted after reverse JRJ", charge.IsDeleted);
						Assert("Pre conditon - consol cost should not be deleted yet", !consolCost.IsDeleted);
						GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
						Assert("Post condition = consol cost should be deleted after synchronisation", consolCost.IsDeleted);
					}
				}
				finally
				{
					ReleaseMutexes(consol);
				}
			}
		}

		public void TestIsRecommendToEnableAutoJRJForGatewaySellApportionment()
		{
			var consol = GatewayConsol;
			Assert("Pre-condition", consol.IsGateway());

			try
			{
				using (var gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					using (var shipmentJob = TestObjectCreator.CreateJob(consol.Shipments[0]))
					{
						var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
						Assert("Expect recommendation as all conditions are met", GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));

						using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
						{
							Assert("Enable the registry should disable the recommendation", !GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));
						}

						charge.JR_Calc_RelatedJobNumber = consol.Shipments[0].JobNumber;
						charge.JR_OH_SellAccount = GatewayAgent.PK;
						Assert("Assign charge a related job number should disable the recommendation", !GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));
						charge.JR_Calc_RelatedJobNumber = ZString.Empty;

						charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
						Assert("Make the debtor a non gateway agent should disable the recommendation", !GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));
						charge.JR_OH_SellAccount = GatewayAgent.PK;

						TestObjectCreator.CC1.AC_MarginPercentage = 0;
						charge.JR_OH_SellAccount = GatewayAgent.PK;
						Assert("Assign charge code 0 margin should disable the recommendation", !GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));
						TestObjectCreator.CC1.AC_MarginPercentage = 100;

						TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
						charge.JR_OH_SellAccount = GatewayAgent.PK;
						Assert("Make charge code type non-margin should disable the recommendation", !GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));
						TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Margin;
						charge.JR_OH_SellAccount = GatewayAgent.PK;

						Assert("Expect recommendation as all conditions are met", GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));

						var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.EUR, 1M, GatewayAgent);
						var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 28.50M, 0M, 0M, 28.50M, 0M, 0M, TestObjectCreator.CC1.PK);
						charge.JR_AL_ARLine = line.PK;
						Assert("Charge posted should disable the recommendation", !GatewaySellToCostSynchroniser.IsRecommendToEnableAutoJRJForGatewaySellApportionment(gatewayJob));
					}
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchroniseWillCleanUpUnusedShipmentJobsOnFactorySavingBeforeTransaction()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				AssertUnusedJobCleanup(false, ApportionmentListingStates.Cleaned);
				AssertUnusedJobCleanup(false, ApportionmentListingStates.Loaded);
				AssertUnusedJobCleanup(false, ApportionmentListingStates.NotLoaded);

				AssertUnusedJobCleanup(true, ApportionmentListingStates.Cleaned);
				AssertUnusedJobCleanup(true, ApportionmentListingStates.Loaded);
				AssertUnusedJobCleanup(true, ApportionmentListingStates.NotLoaded);
			}

			void AssertUnusedJobCleanup(bool isOnFactorySavingBeforeTransaction, ApportionmentListingStates initialAppListingState)
			{
				var newFactory = new BusinessObjectFactory();
				var gatewaConsolInNewFactory = newFactory.Load<ForwardingConsol>(GatewayConsol.PK);
				using (var gatewayJobInNewFactory = TestObjectCreator.CreateJob(gatewaConsolInNewFactory, newFactory: newFactory))
				{
					gatewayJobInNewFactory.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJobInNewFactory.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJobInNewFactory, TestObjectCreator.CC1.PK, 250m, jrjFieldsMatch: true);
					charge.JR_OSCostAmt = 0m;
					charge.JR_OH_SellAccount = GatewayAgent.PK;
					AssertEquals(true, GatewaySellToCostSynchroniser.IsGatewaySynchronizable(charge));

					var apportionmentListing = gatewaConsolInNewFactory.GetApportionments(true);

					try
					{
						if (initialAppListingState == ApportionmentListingStates.Loaded || initialAppListingState == ApportionmentListingStates.Cleaned)
						{
							apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();
							if (initialAppListingState == ApportionmentListingStates.Cleaned)
							{
								apportionmentListing.ReleaseMutexesOnUnusedJobs();
							}
						}
						AssertEquals("Precondition", initialAppListingState, apportionmentListing.State);

						GatewaySellToCostSynchroniser.simulateIsProcessingOnFactorySavingBeforeTransaction_ForTestOnly = isOnFactorySavingBeforeTransaction;
						GatewaySellToCostSynchroniser.Synchronise(gatewayJobInNewFactory);

						if (isOnFactorySavingBeforeTransaction)
						{
							var expectedAppListingState = initialAppListingState == ApportionmentListingStates.Loaded ? ApportionmentListingStates.Loaded : ApportionmentListingStates.Cleaned;
							AssertEquals(expectedAppListingState, apportionmentListing.State);
						}
						else
						{
							AssertEquals(ApportionmentListingStates.Loaded, apportionmentListing.State);
						}
					}
					finally
					{
						apportionmentListing.ReleaseMutexes();
					}
				}
			}
		}

		public void TestSynchronisationIsDoneOnlyForJobsBelongingToLoggedInCompany()
		{
			var loadPort = "USLAX";
			var dischargePort = "AUSYD";

			var sendingAgent = TestObjectCreator.CreateOrgHeader("SENORG", false, false);
			var sendingAgentGatewayPort = sendingAgent.AppointedGatewayAgentPorts.AddNew();
			sendingAgentGatewayPort.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			sendingAgentGatewayPort.O5_PortOrCountry = loadPort;
			sendingAgentGatewayPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendingAgentGatewayPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var sendingCompany = TestObjectCreator.CreateNewCompany("SEN", Constants.CountryCodes.UnitedStates, orgProxy: sendingAgent);
			var sendingBranch = TestObjectCreator.CreateBranch("SSS", sendingCompany, sendingAgent);

			var receivingAgent = TestObjectCreator.CreateOrgHeader("RECORG", false, false);
			var receivingAgentGatewayPort = receivingAgent.AppointedGatewayAgentPorts.AddNew();
			receivingAgentGatewayPort.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			receivingAgentGatewayPort.O5_PortOrCountry = dischargePort;
			receivingAgentGatewayPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receivingAgentGatewayPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var receivingCompany = TestObjectCreator.CreateNewCompany("REC", Constants.CountryCodes.Australia, orgProxy: receivingAgent);
			var receivingBranch = TestObjectCreator.CreateBranch("RRR", receivingCompany, receivingAgent);

			var consol = TestObjectCreator.CreateConsol(loadPort, dischargePort, "CS0001");
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			TestObjectCreator.CreateShipment("S0001", consol);
			TestObjectCreator.CreateShipment("S0002", consol);
			Factory.Save();

			Assert(consol.IsGatewayBillingEnabled(sendingCompany));
			Assert(consol.IsGatewayBillingEnabled(receivingCompany));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sendingCompanyGatewayJob = TestObjectCreator.CreateJob(consol);
				var charge = TestObjectCreator.CreateCharge(sendingCompanyGatewayJob, TestObjectCreator.CC1, "", TestObjectCreator.USD, 10m, null, TestObjectCreator.USD, 10m, sendingAgent);
				charge.JR_JH_InternalJob = sendingCompanyGatewayJob.PK;
				Factory.Save();
				AssertSellApportionment(sendingCompany.PK, consol.PK, true, 10m);
				AssertSellApportionment(receivingCompany.PK, consol.PK, false, 0m);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, receivingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				var newObjectCreator = new TestObjectCreator(newFactory);

				var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
				var receivingCompanyGatewayJob = newObjectCreator.CreateJob(consolInNewFactory);
				var charge = newObjectCreator.CreateCharge(receivingCompanyGatewayJob, newObjectCreator.CC1, "", newObjectCreator.AUD, 20m, null, newObjectCreator.AUD, 20m, receivingAgent);
				charge.JR_JH_InternalJob = receivingCompanyGatewayJob.PK;
				var sendingCompanyGatewayJob = new JobHeader.Loader(consolInNewFactory).Load(true, sendingCompany, false);
				AssertNotNull("Precondition : sendingCompanyGatewayJob is loaded inside receiving company.", sendingCompanyGatewayJob);
				newFactory.Save();

				AssertSellApportionment(sendingCompany.PK, consol.PK, true, 10m);
				AssertSellApportionment(receivingCompany.PK, consol.PK, true, 20m);
			}
		}

		void AssertSellApportionment(ZGuid companyPK, ZGuid consolPK, ZBool sellApportionmentShouldBeCreated, ZDecimal expectedSellApportionmentAmount)
		{
			var consolCosts = new BusinessObjectFactory().Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consolPK).AddToFilter(new ZQuery(JobConsolCostSchema.E6_GC, companyPK)));
			AssertEquals(sellApportionmentShouldBeCreated ? 1 : 0, consolCosts.Length);
			if (sellApportionmentShouldBeCreated)
			{
				AssertEquals(expectedSellApportionmentAmount, consolCosts.First().E6_OSCostAmount);
			}
		}

		public void TestSynchronise_NoNewChargesAppear()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var consol = GatewayConsol;
			Assert("Pre-condition", consol.IsGateway());

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

					Factory.Save();

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals(1, consolCosts.Length);

					var consolCost = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC1.PK);
					AssertNotNull(consolCost);
					AssertEquals(250m, consolCost.E6_OSCostAmount);

					AssertEquals(1, gatewayJob.Charges.Count);
					AssertEquals(250m, charge.JR_OSSellAmt);
					AssertEquals(250m, charge.JR_LocalSellAmt);

					charge.JR_OSCostAmt = 100m;
					charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
					AssertNotEquals("PreCondition", charge.JR_GB, charge.JR_GB_InternalBranch);
					Factory.Save();

					AssertEquals(1, gatewayJob.Charges.Count);
					AssertEquals(250m, charge.JR_OSSellAmt);
					AssertEquals(250m, charge.JR_LocalSellAmt);
					AssertEquals(100m, charge.JR_OSCostAmt);
					AssertEquals(charge.JR_GB, TestObjectCreator.NonCurrentBranch.PK);
					Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
					Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronise_WhenAutoJRJFieldsMatch_CreatesConsolCostsFromGatewayCharges() => TestSynchronise_CreatesConsolCostsFromGatewayCharges(true);

		public void TestSynchronise_WhenAutoJRJFieldsDiffer_DoesNotCreateConsolCostsFromGatewayCharges() => TestSynchronise_CreatesConsolCostsFromGatewayCharges(false);

		void TestSynchronise_CreatesConsolCostsFromGatewayCharges(bool jrjFieldsMatch)
		{
			var consol = GatewayConsol;
			Assert("Pre-condition", consol.IsGateway());

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m, jrjFieldsMatch);

					var charge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC2.PK, 80m, jrjFieldsMatch);
					charge2.JR_RX_NKSellCurrency = "CNY";
					charge2.RevenueExchangeRate.SetBuyRate_ForTestOnly(5.36m);
					charge2.JR_LocalSellAmt = 80m;

					var charge3 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC3.PK, 270m, jrjFieldsMatch);
					charge3.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;

					var charge4 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC4.PK, -50m, jrjFieldsMatch);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					if (jrjFieldsMatch)
					{
						AssertEquals("Expected all consol costs except for charge3, as its not valid for gateway", 3, consolCosts.Length);

						var costCharge1 = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC1.PK);
						AssertNotNull(costCharge1);
						AssertEquals(GatewayAgent.PK, costCharge1.E6_OH_Creditor);
						AssertEquals(charge1.JR_RX_NKSellCurrency, costCharge1.E6_RX_NKCurrency);
						AssertEquals(charge1.JR_OSSellExRate, costCharge1.E6_ExchangeRate);
						AssertEquals(250m, costCharge1.E6_OSCostAmount);
						AssertEquals(ZGuid.Empty, costCharge1.E6_AT_TaxRate);
						AssertEquals(charge1.JR_E6_GatewaySellHeader, costCharge1.PK);
						AssertEquals(charge1.PK, costCharge1.E6_GatewaySellChargeID);

						var costCharge2 = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC2.PK);
						AssertNotNull(costCharge2);
						AssertEquals(GatewayAgent.PK, costCharge2.E6_OH_Creditor);
						AssertEquals(charge2.JR_RX_NKSellCurrency, costCharge2.E6_RX_NKCurrency);
						AssertEquals(charge2.JR_OSSellExRate, costCharge2.E6_ExchangeRate);
						AssertEquals(charge2.JR_OSSellAmt, costCharge2.E6_OSCostAmount);
						AssertEquals(charge2.JR_E6_GatewaySellHeader, costCharge2.PK);
						AssertEquals(charge2.PK, costCharge2.E6_GatewaySellChargeID);

						var costCharge4 = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC4.PK);
						AssertNotNull(costCharge4);
						AssertEquals(GatewayAgent.PK, costCharge4.E6_OH_Creditor);
						AssertEquals("Expected negative value to be copied", charge4.JR_LocalSellAmt, costCharge4.E6_OSCostAmount);
						AssertEquals(charge4.JR_E6_GatewaySellHeader, costCharge4.PK);
						AssertEquals(charge4.PK, costCharge4.E6_GatewaySellChargeID);
					}
					else
					{
						AssertEquals("Expected NO consol costs synchronized", 0, consolCosts.Length);
					}
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronise_WhenAutoJRJFieldsMatch_RecreatesConsolCostsFromGatewayCharges() => TestSynchronise_RecreatesOrDeleteConsolCostsFromGatewayCharges(true);

		public void TestSynchronise_WhenAutoJRJFieldsDiffer_DeletesConsolCostsFromGatewayCharges() => TestSynchronise_RecreatesOrDeleteConsolCostsFromGatewayCharges(false);

		public void TestSynchronise_RecreatesOrDeleteConsolCostsFromGatewayCharges(bool jrjFieldsMatch)
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					var charge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC3.PK, 80m);
					charge2.JR_RX_NKSellCurrency = "CNY";
					charge2.RevenueExchangeRate.SetBuyRate_ForTestOnly(5.36m);
					charge2.JR_LocalSellAmt = 80m;

					var charge3 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC5.PK, 70m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Expected 3 job consol costs to have been created as there are 3 valid gate way charges", 3, consolCosts.Length);

					charge1.JR_RX_NKSellCurrency = "USD";
					charge1.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.15m);
					charge1.JR_LocalSellAmt = 60m;

					charge2.JR_LocalSellAmt = 90m;

					charge3.JR_GE = TestObjectCreator.FIADepartment.PK;
					charge3.JR_OH_SellAccount = Factory.New<OrgHeader>().PK;

					if (!jrjFieldsMatch)
					{
						charge1.JR_GE_InternalDept = ZGuid.Empty;
						charge2.JR_GE_InternalDept = ZGuid.Empty;
						charge3.JR_GE_InternalDept = ZGuid.Empty;
					}

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					if (jrjFieldsMatch)
					{
						AssertEquals("Expect only 2 charges as charge3 is no longer valid for gateway", 2, consolCosts.Length);

						var costCharge1 = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC1.PK);
						AssertNotNull(costCharge1);
						AssertEquals(GatewayAgent.PK, costCharge1.E6_OH_Creditor);
						AssertEquals("Expected to have updated currency", "USD", costCharge1.E6_RX_NKCurrency);
						AssertEquals("Expected to have updated exchange rate", 1.15m, costCharge1.E6_ExchangeRate);
						AssertEquals("Overseas cost amount should have changed now there is a foreign currency", charge1.JR_LocalSellAmt, costCharge1.E6_LocalCostAmount);
						AssertEquals(charge1.JR_E6_GatewaySellHeader, costCharge1.PK);
						AssertEquals(charge1.PK, costCharge1.E6_GatewaySellChargeID);

						var costCharge2 = consolCosts.FirstOrDefault(c => c.E6_AC_ChargeCode == TestObjectCreator.CC3.PK);
						AssertNotNull(costCharge2);
						AssertEquals(charge2.JR_OSSellAmt, costCharge2.E6_OSCostAmount);
						AssertEquals(charge2.JR_E6_GatewaySellHeader, costCharge2.PK);
						AssertEquals(charge2.PK, costCharge2.E6_GatewaySellChargeID);
					}
					else
					{
						AssertEquals("Expect all costs deleted", 0, consolCosts.Length);
					}
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronise_DeletedGatewayChargesDoNotCreateConsolCosts()
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					var charge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC3.PK, 80m);

					charge1.Delete();

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Expected only one as charge1 was deleted", 1, consolCosts.Length);

					charge2.Delete();

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Expected no job consol costs as both gateway charges are deleted", 0, consolCosts.Length);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronise_DeletesConsolCostsNotFromGatewayCharges()
		{
			var consol = GatewayConsol;

			consol.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					Assert("Pre-condition", consol.IsGateway());

					var cost1 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.FRT, GatewayAgent);
					var cost2 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.CC1);
					var cost3 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.CC2, GatewayAgent);

					var relatedCharge = gatewayJob.Charges.AddNew();
					relatedCharge.JR_E6_GatewaySellHeader = cost1.PK;

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					Assert("Expected cost to be deleted as there are no corresponding gateway charges", cost1.IsDeleted);
					Assert(cost2.IsDeleted);
					Assert(cost3.IsDeleted);

					AssertEquals("should be unlinked", ZGuid.Empty, relatedCharge.JR_E6_GatewaySellHeader);
					AssertEquals(false, relatedCharge.IsDeleted || relatedCharge.IsDeleting);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
				consol.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		public void TestSynchronise_DoesNotDeleteChargeWithPostedCost()
		{
			var consol = GatewayConsol;
			consol.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					Assert("Pre-condition", consol.IsGateway());

					var cost1 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.FRT, GatewayAgent);
					var cost2 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.CC1);
					var apportionCharge1 = cost2.ApportionmentCharges[0];
					var apportionCharge2 = cost2.ApportionmentCharges[0];

					var apInvoice = Factory.NewWithValidTestData<APInvoice>();
					var apInvoiceLine1 = apInvoice.Lines.AddNew();
					var apInvoiceLine2 = apInvoice.Lines.AddNew();

					cost2.E6_AH_APInvoice = apInvoice.PK;
					apportionCharge1.JR_AL_APLine = apInvoiceLine1.PK;
					apportionCharge2.JR_AL_APLine = apInvoiceLine2.PK;

					var relatedCharge = gatewayJob.Charges.AddNew();
					relatedCharge.JR_E6_GatewaySellHeader = cost1.PK;

					Assert("Cost Posted", apportionCharge1.IsCostPosted);
					Assert("Cost Posted", apportionCharge2.IsCostPosted);
					Assert("IsApportioned", apportionCharge2.JR_IsApportioned);
					Assert("IsApportioned", apportionCharge2.JR_IsApportioned);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					Assert("Cost Posted", apportionCharge1.IsCostPosted);
					Assert("Cost Posted", apportionCharge2.IsCostPosted);
					Assert("IsApportioned", !apportionCharge2.JR_IsApportioned);
					Assert("IsApportioned", !apportionCharge2.JR_IsApportioned);

					Assert("Expected cost to be deleted as there are no corresponding gateway charges", cost1.IsDeleted);
					Assert("Expected cost to be deleted as there are no corresponding gateway charges", cost2.IsDeleted);
					AssertEquals("should be unlinked", ZGuid.Empty, relatedCharge.JR_E6_GatewaySellHeader);
					AssertEquals(false, relatedCharge.IsDeleted || relatedCharge.IsDeleting);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
				consol.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		public void TestSynchronise_ErrorReportedWhenCalculationStartegyIsNotOfExpectedType()
		{
			ErrorReporter.Clear();
			var consolcost = Factory.NewWithValidTestData<JobConsolCost>();
			GatewaySellToCostSynchroniser.DeleteConsolCosts_TestOnly(new[] { consolcost });
			AssertEquals("Error Should be reported", "CalculationStrategy is of type: Enterprise.Accounting.Business.ConsolCosting.JobConsolCost+ConsolCostCalculationStrategyWithoutCalculations. Expected type: Enterprise.Accounting.Business.ConsolCosting.JobConsolCost+ConsolCostCalculationStrategyWithCalculations", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSynchronise_DeletesChargesOnConsolAndShipment()
		{
			var consol = GatewayConsol;

			consol.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					Assert("Pre-condition", consol.IsGateway());

					var cost1 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.FRT, GatewayAgent);
					var cost2 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.CC1);
					var cost3 = TestObjectCreator.CreateGatewayConsolCost(consol, TestObjectCreator.CC2, GatewayAgent);

					AssertEquals(2, cost2.ApportionmentCharges.Count);
					var charge21 = cost2.ApportionmentCharges[0];
					var charge22 = cost2.ApportionmentCharges[1];

					var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("123", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);

					AssertEquals(2, cost3.ApportionmentCharges.Count);
					var charge31 = cost3.ApportionmentCharges[0];
					var charge32 = cost3.ApportionmentCharges[1];

					TestObjectCreator.CreateRevenueLine(charge31, arInvoice.PK);
					TestObjectCreator.CreateRevenueLine(charge32, arInvoice.PK);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					Assert("Expected cost to be deleted as there are no corresponding gateway charges", cost1.IsDeleted);
					Assert(cost2.IsDeleted);
					Assert("Charges on the shipment should be deleted when consol cost is deleted", charge21.IsDeleted);
					Assert(charge22.IsDeleted);

					Assert("Cost3 has been deleted", cost3.IsDeleted);
					Assert("Charges in Cost3 which have been revenue posted should not be deleted", !charge31.IsDeleted);
					Assert("Charges in Cost3 which have been revenue posted should not be deleted", !charge32.IsDeleted);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
				consol.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		public void TestSynchronise_ChargesWhereTheInternalJobIsNotTheConsolIsIgnored()
		{
			var consol = GatewayConsol;
			Assert("Pre-condition", consol.IsGateway());

			try
			{
				using (var gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					using (var shipmentJob = TestObjectCreator.CreateJob(consol.Shipments[0]))
					{
						GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
						GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC2.PK, 80m);
						var shipmentCharge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 70m);
						shipmentCharge.JR_JH_InternalJob = shipmentJob.PK;

						GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

						var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
						AssertEquals("Shipment charge should not have restriced charge 1 from being synchronised with gateway", 2, consolCosts.Length);
					}
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronise_UpdatesInternalValues()
		{
			var consol1 = GatewayConsol;
			var consol2 = TestObjectCreator.CreateGatewayConsol(consolNum: "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);

			try
			{
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), TestObjectCreator.GEADepartment.PK.ToGuid()))
				using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
				using (var gatewayJob1 = TestObjectCreator.CreateJob(consol1))
				using (var gatewayJob2 = TestObjectCreator.CreateJob(consol2))
				{
					gatewayJob1.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;

					var branch1Pk = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany).PK;
					var branch2Pk = TestObjectCreator.CreateBranch("BR2", GlbCompany.CurrentCompany).PK;
					var department1Pk = TestObjectCreator.CreateDepartment("DP1").PK;
					var department2Pk = TestObjectCreator.CreateDepartment("DP2").PK;

					var charge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob1, TestObjectCreator.CC1.PK, 100);
					charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					charge1.JR_JH_InternalJob = gatewayJob2.PK;
					var charge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob1, TestObjectCreator.CC3.PK, 100);
					charge2.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					charge2.JR_GB = branch1Pk;
					charge2.JR_GB_InternalBranch = branch1Pk;
					var charge3 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob1, TestObjectCreator.CC5.PK, 100);
					charge3.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					charge3.JR_GE = department1Pk;
					charge3.JR_GE_InternalDept = department1Pk;
					GatewaySellToCostSynchroniser.Synchronise(gatewayJob1);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol1.PK));
					Assert(consolCosts.All(x => x.SellChargeFromSellToCostSynchronisation != charge1));
					AssertCostMatches(consolCosts, charge2, gatewayJob1.PK, branch1Pk, GlbDepartment.CurrentDepartment.PK);
					AssertCostMatches(consolCosts, charge3, gatewayJob1.PK, GlbBranch.CurrentBranch.PK, department1Pk);

					charge1.JR_JH_InternalJob = gatewayJob1.PK;
					charge2.JR_GB_InternalBranch = branch2Pk;
					charge3.JR_GE_InternalDept = department2Pk;
					GatewaySellToCostSynchroniser.Synchronise(gatewayJob1);

					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol1.PK));
					AssertCostMatches(consolCosts, charge1, gatewayJob1.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
					Assert(consolCosts.All(x => x.SellChargeFromSellToCostSynchronisation != charge2));
					Assert(consolCosts.All(x => x.SellChargeFromSellToCostSynchronisation != charge3));

					charge1.JR_JH = gatewayJob2.PK;
					charge1.JR_JH_InternalJob = gatewayJob2.PK;
					charge1.JR_GB = branch2Pk;
					charge1.JR_GB_InternalBranch = branch2Pk;
					charge1.JR_GE = department2Pk;
					charge1.JR_GE_InternalDept = department2Pk;
					charge2.JR_GB = branch2Pk;
					charge3.JR_GE = department2Pk;
					GatewaySellToCostSynchroniser.Synchronise(gatewayJob1);

					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol1.PK));
					AssertCostMatches(consolCosts, charge1, gatewayJob2.PK, branch2Pk, department2Pk);
					AssertCostMatches(consolCosts, charge2, gatewayJob1.PK, branch2Pk, GlbDepartment.CurrentDepartment.PK);
					AssertCostMatches(consolCosts, charge3, gatewayJob1.PK, GlbBranch.CurrentBranch.PK, department2Pk);

					void AssertCostMatches(JobConsolCost[] costs, Charge parentCharge, ZGuid job, ZGuid branch, ZGuid department)
					{
						var cost = costs.FirstOrDefault(x => x.SellChargeFromSellToCostSynchronisation == parentCharge);
						AssertNotNull(cost);
						var costCharges = cost.ApportionmentCharges.Cast<ApportionSplitCharge>();

						AssertEquals(job, parentCharge.JR_JH_InternalJob);
						costCharges.ForEach(x => AssertEquals(job, x.JR_JH_InternalJob));
						AssertEquals(branch, parentCharge.JR_GB_InternalBranch);
						costCharges.ForEach(x => AssertEquals(branch, x.JR_GB_InternalBranch));
						AssertEquals(department, parentCharge.JR_GE_InternalDept);
						costCharges.ForEach(x => AssertEquals(department, x.JR_GE_InternalDept));
					}
				}
			}
			finally
			{
				ReleaseMutexes(consol1);
				ReleaseMutexes(consol2);
			}
		}

		public void TestConsolRequirementsToSynchronise_MustBeAGatewayConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Assert("Pre-condition", !consol.IsGateway());
			var gateway = (IGateway)consol;
			AssertEquals("Pre-condition: has no agent so is not valid for synchronisation", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			var cost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, GatewayAgent);
			var cost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			var cost3 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, GatewayAgent);

			var consolJob = Factory.NewJobForTesting<Job>();
			consolJob.Parent = consol;

			Assert("IsGateway", consol.IsGateway());
			AssertEquals("Has no agent so is not valid for synchronisation", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			GatewaySellToCostSynchroniser.Synchronise(consolJob);

			Assert("Consol is not valid for synchronisation so Consol Costs should remain untouched", !cost1.IsDeleted);
			Assert(!cost2.IsDeleted);
			Assert(!cost3.IsDeleted);
		}

		public void TestConsolRequirementsToSynchronise_MustHaveAShipment_AllShipmentsValidForApportionment()
		{
			var consol = GatewayConsol;
			consol.Shipments.RemoveAll();
			Factory.Save();

			Assert("Pre-condition", consol.IsGateway());

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 240m);

					var shipment1 = consol.Shipments.AddNew();
					shipment1.JS_TransportMode = Constants.TransportModes.Sea;
					shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
					shipment1.JS_ActualWeight = 500m;
					var shipment2 = consol.Shipments.AddNew();
					shipment2.JS_TransportMode = Constants.TransportModes.Sea;
					shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
					shipment2.JS_ActualWeight = 500m;

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertNotNull("Now that we have shipments synchronisation should succeed", consolCosts);
					AssertEquals(2, consolCosts.ApportionmentCharges.Count);
					AssertEquals(true, consolCosts.ApportionmentCharges[0].JR_IsUsedForApportionment);
					AssertEquals(true, consolCosts.ApportionmentCharges[1].JR_IsUsedForApportionment);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestConsolRequirementsToSynchronise_MustHaveAShipment_NoShipments()
		{
			var consol = GatewayConsol;
			consol.Shipments.RemoveAll();
			Factory.Save();

			using (gatewayJob = TestObjectCreator.CreateJob(consol))
			{
				gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
				gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

				GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 240m);

				GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

				var consolCosts = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
				AssertNull("Should not have a consol cost if there are no shipments", consolCosts);
			}
		}

		public void TestConsolRequirementsToSynchronise_MustHaveAShipment_SomeShipmentsValidForApportionment()
		{
			var consol = GatewayConsol;
			consol.Shipments.RemoveAll();
			Factory.Save();

			Assert("Pre-condition", consol.IsGateway());

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 240m);

					var shipmentWithChargeable = consol.Shipments.AddNew();
					shipmentWithChargeable.JS_TransportMode = Constants.TransportModes.Sea;
					shipmentWithChargeable.JS_UnitOfWeight = Constants.Weight.Kilograms;
					shipmentWithChargeable.JS_ActualWeight = 500m;

					consol.Shipments.AddNew();

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertNotNull("Now that we have shipments synchronisation should succeed", consolCosts);
					AssertEquals("Expected only one charge as only one shipment is valid for apportionment", 1, consolCosts.ApportionmentCharges.Count);
					AssertEquals(true, consolCosts.ApportionmentCharges[0].JR_IsUsedForApportionment);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronise_SharedCompanyConsolCostMustNotBeDeleted()
		{
			var sharedCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C10011991";
			consol.JK_RL_NKLoadPort = "DEFRA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = sharedCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = GatewayAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "DEFRA";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			gatewayAgentPort = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var shipment = consol.Shipments.AddNew();

			try
			{
				JobConsolCost consolCost;

				using (new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly())
				{
					ApportionmentListing listing = new ApportionmentListing(Factory, consol);
					listing.PrepareForConsolCosting();

					consol.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
					try
					{
						consolCost = listing.CostsCollection.AddNew();
					}
					finally
					{
						consol.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
					}
					consolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
					consolCost.E6_OSCostAmount = 235;
					consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
					consolCost.E6_ExchangeRate = 1m;
					consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.Australia;

					Factory.Save();
				}

				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				Factory.Save();

				Assert("Pre-condition", consol.IsGateway());
				AssertNoErrors(consol.JK_SendingForwarderHandlingTypeInfo);

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sharedCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var newFactory = new BusinessObjectFactory();
					consol = newFactory.Load<ForwardingConsol>(consol.PK);
					var helper = new TestObjectCreator(newFactory);

					Assert("Postcondition: IsGateway", consol.IsGateway());

					var costs = newFactory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals(1, costs.Length);
					AssertEquals(consolCost.PK, costs[0].PK);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestGetGatewayApportionmentListingLockedChildShipmentsJobsErrorMessageWhenJobIsLocked()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateGatewayConsol("AUSYD", "NZAKL", "C001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = creator.CreateShipment("S001", consol);
			var shipment2 = creator.CreateShipment("S002", consol);
			Factory.Save();

			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment1).Load());
			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment2).Load());

			using (var shipmentJob1 = new JobHeader.Loader(new BusinessObjectFactory(), shipment1).TryCreateWithMutex())
			using (var shipmentJob2 = new JobHeader.Loader(new BusinessObjectFactory(), shipment2).TryCreateWithMutex())
			using (var consolJob = new JobHeader.Loader(Factory, consol).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var errorMessage = GatewaySellToCostSynchroniser.GetGatewayApportionmentListingLockedChildShipmentsJobsErrorMessage((Job)consolJob);
				Assert(!errorMessage.IsEmpty);
				AssertContains("You have created the job S001 on another form, but haven't saved it yet", errorMessage);
				AssertContains("You have created the job S002 on another form, but haven't saved it yet", errorMessage);
			}
		}

		public void TestGetGatewayApportionmentListingLockedChildShipmentsJobsErrorMessageWhenJobIsNotLocked()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateGatewayConsol("AUSYD", "NZAKL", "C001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = creator.CreateShipment("S001", consol);
			var shipment2 = creator.CreateShipment("S002", consol);
			Factory.Save();

			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment1).Load());
			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment2).Load());

			using (var consolJob = new JobHeader.Loader(Factory, consol).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var errorMessage = GatewaySellToCostSynchroniser.GetGatewayApportionmentListingLockedChildShipmentsJobsErrorMessage((Job)consolJob);
				Assert(errorMessage.IsEmpty);
				AssertNull("shipment job is not created", new JobHeader.Loader(shipment1).Load());
				AssertNull("shipment job is not created", new JobHeader.Loader(shipment2).Load());
			}
		}

		#region No Gateway Consol Costs are created when multiple Charges have the same charge code

		public void TestNoSynchronisation_WhenGatewaySellChargesHaveIdenticalChargeCodes()
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					var charge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 100m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					var errorMessage = @"Should not synchronise when there are gateway sell charges with the same charge code.
This is because currently we add errors to consol costs if we have more than one using a charge code.
If we synchronise, we create consol costs with errors on a field that users can't edit, which is very bad.
So for now we should disallow synchronising if it will create multi consol costs with the same charge codes";

					AssertEquals(errorMessage, 0, consolCosts.Length);

					charge2.JR_AC = TestObjectCreator.CC2.PK;
					charge2.JR_LocalSellAmt = 80m;      //reset amounts after changing the charge code

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals("Now charge codes are both unique it should synchronise", 2, consolCosts.Length);

					GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC2.PK, 50m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals("Expected not to synchronise the new charge it has the same charge code as charge2", 1, consolCosts.Length);
					AssertEquals(charge1.JR_LocalSellAmt, consolCosts[0].E6_OSCostAmount);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronisation_MutliplePostedCostsNotSynchronised()
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var postedCharge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					var postedCharge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 80m);

					var arLine1 = Factory.New<AccTransactionLines>();
					arLine1.AL_LineType = TransactionLineTypes.Revenue;
					postedCharge1.JR_AL_ARLine = arLine1.PK;

					var arLine2 = Factory.New<AccTransactionLines>();
					arLine2.AL_LineType = TransactionLineTypes.Revenue;
					postedCharge2.JR_AL_ARLine = arLine2.PK;

					Assert("Pre-condition", postedCharge1.JR_IsRevenuePosted);
					Assert("Pre-condition", postedCharge2.JR_IsRevenuePosted);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Expected no consol costs as the charge codes are the same", 0, consolCosts.Length);

					var unpostedCharge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 150m);

					Assert("Pre-condition", !unpostedCharge.JR_IsRevenuePosted);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Expected only the unposted charge to have been pulled through", 1, consolCosts.Length);
					AssertEquals(TestObjectCreator.CC1.PK, consolCosts[0].E6_AC_ChargeCode);
					AssertEquals(unpostedCharge.JR_LocalSellAmt, consolCosts[0].E6_OSCostAmount);

					GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 50m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Now there are two unposted charges and two posted, we have no charge we can synchronise", 0, consolCosts.Length);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronisation_OnlyUniqueByChargeCodeAndStatusCostsSynced_OneGWAgent()
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var postedCharge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					var postedCharge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 200m);

					var arLine1 = Factory.New<AccTransactionLines>();
					arLine1.AL_LineType = TransactionLineTypes.Revenue;
					postedCharge1.JR_AL_ARLine = arLine1.PK;

					var arLine2 = Factory.New<AccTransactionLines>();
					arLine2.AL_LineType = TransactionLineTypes.Revenue;
					postedCharge2.JR_AL_ARLine = arLine2.PK;

					var unpostedCharge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 150m);

					Assert("Pre-condition", !unpostedCharge.JR_IsRevenuePosted);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Expected only the unposted charge to have been pulled through", 1, consolCosts.Length);
					AssertEquals(TestObjectCreator.CC1.PK, consolCosts[0].E6_AC_ChargeCode);
					AssertEquals(unpostedCharge.JR_LocalSellAmt, consolCosts[0].E6_OSCostAmount);

					var unrelatedCharge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC2.PK, 250m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals(2, consolCosts.Length);
					AssertEquals(unpostedCharge.JR_LocalSellAmt, consolCosts[0].E6_OSCostAmount);
					AssertEquals(unrelatedCharge.JR_LocalSellAmt, consolCosts[1].E6_OSCostAmount);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronisation_OnlyUniqueByChargeCodeAndStatusCostsSynced_TwoGWAgents()
		{
			var consol = GatewayConsolWithTwoGWAgents;
			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					var postedCharge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					var postedCharge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 200m);

					var arLine1 = Factory.New<AccTransactionLines>();
					arLine1.AL_LineType = TransactionLineTypes.Revenue;
					postedCharge1.JR_AL_ARLine = arLine1.PK;

					var arLine2 = Factory.New<AccTransactionLines>();
					arLine2.AL_LineType = TransactionLineTypes.Revenue;
					postedCharge2.JR_AL_ARLine = arLine2.PK;

					var unpostedCharge1 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 150m);
					unpostedCharge1.JR_OH_SellAccount = GatewaySendingAgent.PK;
					Assert("Pre-condition", !unpostedCharge1.JR_IsRevenuePosted);

					var unpostedCharge2 = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 260m);
					unpostedCharge2.JR_OH_SellAccount = GatewayReceivingAgent.PK;
					Assert("Pre-condition", !unpostedCharge2.JR_IsRevenuePosted);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals("Expected 0 charge to have been pulled through because they all have gateway agents as debtor.", 0, consolCosts.Length);

					unpostedCharge2.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);
					consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals("Expected only the unposted charge to have been pulled through", 1, consolCosts.Length);
					AssertEquals(TestObjectCreator.CC1.PK, consolCosts[0].E6_AC_ChargeCode);
					AssertEquals(unpostedCharge1.JR_LocalSellAmt, consolCosts[0].E6_OSCostAmount);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		#endregion

		public void TestNoSynchronisation_SkipChargeHasNoCostAmount()
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var chargeToSync = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC2.PK, 0m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					AssertEquals("Expected to only sychronise to gateway apportion only the charge that has a cost", 1, consolCosts.Length);

					AssertEquals(chargeToSync.JR_LocalSellAmt, consolCosts[0].E6_OSCostAmount);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestAppendGatewayLinkToRevenueCalcDesc()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
			var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(job, Env.Registry.FreightChargeCode, 100m);

			var expectedGatewayMessage = string.Format("Gateway Sell is apportioned.");

			Assert("Pre-condition", charge.CostCalculationDescription.IsEmpty);

			GatewaySellToCostSynchroniser.AppendGatewayLinkToRevenueCalcDesc(charge);

			Assert("Should have added to Revenue Calculation", !charge.RevenueCalculationDescription.IsEmpty);
			AssertEquals(expectedGatewayMessage, charge.RevenueCalculationDescription.ToAscii());

			var mockText = "Some Auto-rating has been done for this charge";
			var mockAutoratingNote = ZBlob.FromAscii(mockText);

			charge.RevenueCalculationDescription = mockAutoratingNote;
			AssertEquals("Expected RevCalc to have been overriden", mockAutoratingNote, charge.RevenueCalculationDescription);

			var revenueCalcDesc = GatewaySellToCostSynchroniser.AppendGatewayLinkToRevenueCalcDesc(charge);

			AssertContains("Expected to have included both texts", mockText, charge.RevenueCalculationDescription.ToAscii());
			AssertContains("Expected to have included both texts", expectedGatewayMessage, charge.RevenueCalculationDescription.ToAscii());

			GatewaySellToCostSynchroniser.AppendGatewayLinkToRevenueCalcDesc(charge);

			AssertEquals("Running append method again should not change the RevCalc note as we already have the Gateway sell message",
				revenueCalcDesc, charge.RevenueCalculationDescription);
		}

		public void TestRevenueCalcDescToApportionedCharges()
		{
			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(GatewayConsol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var shipment1 = GatewayConsol.Shipments[0];
					shipment1.JS_TransportMode = Constants.TransportModes.Sea;
					shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
					shipment1.JS_ActualWeight = 500m;

					var shipment2 = GatewayConsol.Shipments[1];
					shipment2.JS_TransportMode = Constants.TransportModes.Sea;
					shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
					shipment2.JS_ActualWeight = 500m;

					GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 240m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCost = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, TestObjectCreator.CC1.PK));

					AssertNotNull("Pre-condition", consolCost);
					AssertEquals("Pre-condition:", 2, consolCost.ApportionmentCharges.Count);
					AssertEquals(consolCost.CostCalculationDescription, consolCost.ApportionmentCharges[0].CostCalculationDescription);
					AssertEquals(consolCost.CostCalculationDescription, consolCost.ApportionmentCharges[1].CostCalculationDescription);
				}
			}
			finally
			{
				ReleaseMutexes(GatewayConsol);
			}
		}

		public void TestApportionedChargesAreLinkedToConsolCost()
		{
			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(GatewayConsol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var shipment1 = GatewayConsol.Shipments[0];
					shipment1.JS_TransportMode = Constants.TransportModes.Sea;
					shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
					shipment1.JS_ActualWeight = 500m;

					var shipment2 = GatewayConsol.Shipments[1];
					shipment2.JS_TransportMode = Constants.TransportModes.Sea;
					shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
					shipment2.JS_ActualWeight = 500m;

					var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 240m);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCost = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, TestObjectCreator.CC1.PK));

					AssertNotNull("Pre-condition", consolCost);
					AssertEquals("Pre-condition:", 2, consolCost.ApportionmentCharges.Count);
					AssertEquals(consolCost.PK, consolCost.ApportionmentCharges[0].JR_E6_GatewaySellHeader);
					AssertEquals(consolCost.PK, consolCost.ApportionmentCharges[1].JR_E6_GatewaySellHeader);
					AssertEquals(charge.PK, consolCost.E6_GatewaySellChargeID);
				}
			}
			finally
			{
				ReleaseMutexes(GatewayConsol);
			}
		}

		public void TestSave_GatewayConsolWithChargeInternalDeptAsConsolNumber()
		{
			var newOrgProxy = TestObjectCreator.CreateOrgHeader("TSTORG", false, false);
			var newCompany = TestObjectCreator.CreateNewCompany("ZZZ", orgProxy: newOrgProxy);
			var newBranch = TestObjectCreator.CreateBranch("ZZZ", "Branch ZZZ", newCompany);
			var newBranch1 = TestObjectCreator.CreateBranch("ZZ1", "Branch ZZ1", newCompany, newOrgProxy);

			using (newBranch.SetAsTemporaryContext())
			{
				var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
				Job job1 = TestObjectCreator.CreateJob(consol);

				var shipment = TestObjectCreator.CreateShipment("S0001", consol: consol, saveIt: false);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var job = new Job.Loader(newFactory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, creditor: newBranch1.OrgProxy);
				charge.JR_GB_InternalBranch = newBranch1.PK;
				var consolFromNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
				var consolJobFromNewFactory = new Job.Loader(consolFromNewFactory).Load();

				charge.JR_JH_InternalJob = consolJobFromNewFactory.PK;

				AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

				AssertNoErrors(charge);
				AssertNoErrors(job);

				newFactory.Save();

				ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
				filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

				var jobHeader = newFactory.Load<JobHeader>(filter).FirstOrDefault();
				AssertNotEquals("Job Header for shipment should be created", jobHeader, null);
				var chargeFromDB = newFactory.Load<JobCharge>(charge.PK);
				AssertEquals("Job charge should have consol as Internal Job", chargeFromDB.JR_JH_InternalJob, job1.PK);
			}
		}

		public void TestSynchronise_CostCreatedWithChargeCurrencyWhenDebtorDefaultCurrencyIsDifferent()
		{
			var companyData =
				Factory.Load<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_OH, GatewayAgent.PK)).FirstOrDefault();

			AssertNotNull(companyData);

			companyData.OB_RX_NKAPDefltCurrency = "AUD";
			companyData.OB_RX_NKARDDefltCurrency = "AUD";

			Factory.Save();

			var consol = GatewayConsol;
			Assert("Pre-condition", consol.IsGateway());

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var usdExchangeRate = gatewayJob.ExchangeRates.AddNew();
					usdExchangeRate.JF_RX_NKRateCurrency = "USD";
					usdExchangeRate.JF_BaseRate = 2;

					var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					charge.JR_RX_NKSellCurrency = "USD";

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals(1, consolCosts.Length);

					var consolCost = consolCosts[0];
					AssertEquals(GatewayAgent.PK, consolCost.E6_OH_Creditor);
					AssertEquals(charge.JR_RX_NKSellCurrency, consolCost.E6_RX_NKCurrency);
					AssertEquals(charge.JR_OSSellExRate, consolCost.E6_ExchangeRate);
					AssertEquals(500m, consolCost.E6_OSCostAmount);
					AssertEquals(250m, consolCost.E6_LocalCostAmount);
					AssertEquals(ZGuid.Empty, consolCost.E6_AT_TaxRate);
					AssertEquals(charge.JR_E6_GatewaySellHeader, consolCost.PK);
					AssertEquals(charge.PK, consolCost.E6_GatewaySellChargeID);
					AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronise_MustNotDeleteShipmentJob()
		{
			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var gatewayConsol = creator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob = creator.CreateJob(gatewayConsol);
			var shipment = creator.CreateShipment("S0001");
			Factory.Save();

			shipment.CreateJobHeaderWithMutex();    // simulate the creation of shipment job on Shipment form
			var job = shipment.Job;
			gatewayConsol.Shipments.Add(shipment);  // simulate attach shipment to gateway consol on Shipment form

			GatewaySellToCostSynchroniser.Synchronise(consolJob);
			AssertNotNull("shipment job should not be deleted", shipment.Job);
			AssertEquals("still the same job", job.PK, shipment.Job.PK);
			Factory.Save(); // added just to release the mutex
		}

		public void TestCopyConsolCostFromGatewaySell_E6_TaxDate()
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var postedCharge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					var arLine = Factory.New<AccTransactionLines>();
					arLine.AL_LineType = TransactionLineTypes.Revenue;
					postedCharge.JR_AL_ARLine = arLine.PK;
					postedCharge.JR_AT_SellGSTRate = TestObjectCreator.VATSPV.PK;
					postedCharge.JR_SellTaxDate = ZDate.Today.AddDays(1);
					Assert("Pre-condition", postedCharge.JR_IsRevenuePosted);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

					Assert("Posted charges don't synchronize", !consolCosts.Any());
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestSynchronizeShouldFixInvalidChargeWithOrgProxyDebtorAndSellTaxID()
		{
			var consol = GatewayConsol;

			try
			{
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
					taxRate.SetRate_ForTestOnly(6, 10, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));

					var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m);
					charge.JR_AT_SellGSTRate = taxRate.PK;

					Assert(charge.SellAccount.IsProxyOrg(charge.Company));
					AssertEquals("Wrong Data: charge Sell Tax ID should be null if Sell Account is Org Proxy", taxRate.PK, charge.JR_AT_SellGSTRate);

					GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

					var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
					AssertEquals(1, consolCosts.Length);

					var consolCost = consolCosts[0];
					AssertEquals(GatewayAgent.PK, consolCost.E6_OH_Creditor);
					AssertEquals(charge.JR_RX_NKSellCurrency, consolCost.E6_RX_NKCurrency);
					AssertEquals(charge.JR_OSSellExRate, consolCost.E6_ExchangeRate);
					AssertEquals(250m, consolCost.E6_OSCostAmount);
					AssertEquals(250m, consolCost.E6_LocalCostAmount);
					AssertEquals(charge.JR_E6_GatewaySellHeader, consolCost.PK);
					AssertEquals(charge.PK, consolCost.E6_GatewaySellChargeID);
					AssertEquals("The wrong TaxID was not copied to the cost", ZGuid.Empty, consolCost.E6_AT_TaxRate);
					AssertEquals("The wrong TaxID was removed", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestMutexIssueWhenSynchronizingEligibleChargeToConsolCost()
		{
			var consol = GatewayConsol;
			var shipment = consol.Shipments[0];

			Factory.Save();
			AssertNull("PreCondition", shipment.Job);

			try
			{
				using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				using (CreateShipmentJobAndDoMutexLockInAnotherCW1(shipment))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 250m, jrjFieldsMatch: true);
					charge.JR_OSCostAmt = 0m;
					charge.JR_OH_SellAccount = GatewayAgent.PK;
					AssertEquals(true, GatewaySellToCostSynchroniser.IsGatewaySynchronizable(charge));

					var exp = AssertExceptionThrown<JobCreationException>("Should create job for shipments and have mutex error.", () => GatewaySellToCostSynchroniser.Synchronise(gatewayJob));
					AssertEquals("User GS1 is in the process of creating the Job S00001000. You cannot work on the job until he/she saves it or cancels the changes.", exp.Message);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		public void TestMutexIssueWhenNoEligibleChargeToConsolCost()
		{
			var consol = GatewayConsol;
			var shipment = consol.Shipments[0];

			Factory.Save();
			AssertNull("PreCondition", shipment.Job);

			try
			{
				using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
				using (gatewayJob = TestObjectCreator.CreateJob(consol))
				using (CreateShipmentJobAndDoMutexLockInAnotherCW1(shipment))
				{
					gatewayJob.JH_OA_LocalChargesAddr = GatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var apportionmentsListing = consol.GetApportionments(true);
					AssertEquals("PreCondition", false, apportionmentsListing.CostsCollectionLoaded);
					AssertNoExceptionThrown("Should not create job for shipments when no revenue JRJ charge. So we have no locking error."
						, () => GatewaySellToCostSynchroniser.Synchronise(gatewayJob)
					);

					AssertEquals("PreCondition", true, apportionmentsListing.CostsCollectionLoaded);
					AssertNoExceptionThrown("In second run, even CostsCollectionLoaded is loaded, we still not create shipment job. So we have no locking error."
						, () => GatewaySellToCostSynchroniser.Synchronise(gatewayJob)
					);
				}
			}
			finally
			{
				ReleaseMutexes(consol);
			}
		}

		JobHeader CreateShipmentJobAndDoMutexLockInAnotherCW1(ForwardingShipment shipment)
		{
			JobHeader shipmentJobInAnotherCW1;
			using (Env.SetTemporaryUserContext(TestObjectCreator.GS1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var tempSemaphoreProvide = new SemaphoreProviderWithCurrentUserForTesting())
			{
				TestSemaphoreProviderAttribute.TestProvider = tempSemaphoreProvide;
				var factoryInAnotherCW1 = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};

				var shipmentInAnotherCW1 = factoryInAnotherCW1.Load<ForwardingShipment>(shipment.PK);
				var loaderInAnotherCW1_shipment2 = new JobHeader.Loader(factoryInAnotherCW1, shipmentInAnotherCW1);
				shipmentJobInAnotherCW1 = loaderInAnotherCW1_shipment2.TryCreateWithMutex();
			}
			Assert("Another CW1 is locking the shipment.", JobHeader.GetCreateOrActivateJobMutex(shipment.PK).IsLocked);

			return shipmentJobInAnotherCW1;
		}

		void ReleaseMutexes(ForwardingConsol consol)
		{
			var apportionmentList = consol.GetApportionments(true);
			AssertNotNull(apportionmentList);
			apportionmentList.ReleaseMutexes();
		}

		#region Set up

		protected override void TearDown()
		{
			if (gatewayJob != null)
			{
				gatewayJob.Dispose();
			}

			base.TearDown();
		}

		Job gatewayJob;

		ForwardingConsol GatewayConsol
		{
			get
			{
				if (gatewayConsol == null)
				{
					gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
					gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
					gatewayConsol.JK_UniqueConsignRef = "C10011991";
					gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
					gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
					gatewayConsol.JK_RL_NKDischargePort = "CNSHA";
					gatewayConsol.JK_OA_SendingForwarderAddress = GatewayAgent.MainAddress.PK;
					gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

					var gatewayAgentPort = gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
					gatewayAgentPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
					gatewayAgentPort.O5_PortOrCountry = "AUSYD";
					gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
					gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

					gatewayConsol.Shipments.AddNew();
					gatewayConsol.Shipments.AddNew();
					Factory.Save();
				}

				return gatewayConsol;
			}
		}

		ForwardingConsol gatewayConsol;

		static OrgHeader GatewayAgent => GlbCompany.CurrentCompany.OrgProxy;

		ForwardingConsol GatewayConsolWithTwoGWAgents
		{
			get
			{
				if (gatewayConsol == null)
				{
					var branch1 = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.GB_Code == "SYD");
					branch1.GB_OH_OrgProxy = GatewaySendingAgent.PK;
					var branch2 = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.GB_Code == "BNE");
					branch2.GB_OH_OrgProxy = GatewayReceivingAgent.PK;
					GlbCompany.CurrentCompany.Factory.Save();

					gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
					gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
					gatewayConsol.JK_UniqueConsignRef = "C10011991";
					gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
					gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
					gatewayConsol.JK_RL_NKDischargePort = "CNSHA";
					gatewayConsol.JK_OA_SendingForwarderAddress = GatewaySendingAgent.MainAddress.PK;
					gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
					gatewayConsol.JK_OA_ReceivingForwarderAddress = GatewayReceivingAgent.MainAddress.PK;
					gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

					var gatewaySendingAgentPort = gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
					gatewaySendingAgentPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
					gatewaySendingAgentPort.O5_PortOrCountry = "AUSYD";
					gatewaySendingAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
					gatewaySendingAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

					var gatewayReceivingAgentPort = gatewayConsol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
					gatewayReceivingAgentPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_ReceivingForwarderAddress;
					gatewayReceivingAgentPort.O5_PortOrCountry = "CNSHA";
					gatewayReceivingAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
					gatewayReceivingAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

					gatewayConsol.Shipments.AddNew();
					gatewayConsol.Shipments.AddNew();
					Factory.Save();
				}

				return gatewayConsol;
			}
		}

		OrgHeader GatewaySendingAgent
		{
			get { return gatewaySendingAgent ?? (gatewaySendingAgent = GlbCompany.CurrentCompany.Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader gatewaySendingAgent;

		OrgHeader GatewayReceivingAgent
		{
			get { return gatewayReceivingAgent ?? (gatewayReceivingAgent = GlbCompany.CurrentCompany.Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader gatewayReceivingAgent;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator testObjectCreator;

		#endregion
	}
}

