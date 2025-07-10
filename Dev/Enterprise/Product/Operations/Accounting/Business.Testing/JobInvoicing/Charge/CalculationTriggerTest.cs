using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class CalculationTriggerTest : TestCaseWithFactory
	{
		public void TestInfoIsCollectedWhenSellCurrencyIsSetToLocalCurrencyAndOSSellAmountAndLocalSellAmountIsNotEqual()
		{
			var creator = new TestObjectCreator(Factory);
			creator.USD.ExchangeRates.DeleteAll();
			creator.CreateExchangeRate(creator.USD, "BUY", 1.25m, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			var charge1 = CreateCharge(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 1000m);
			var charge2 = CreateCharge(Constants.CurrencyCodes.UnitedStates, 2000m);
			var charge3 = CreateCharge(Constants.CurrencyCodes.UnitedStates, 3000m);
			var charge4 = CreateCharge(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 4000m);
			var charge5 = CreateCharge(Constants.CurrencyCodes.UnitedStates, 5000m);
			var charge6 = CreateCharge(Constants.CurrencyCodes.UnitedStates, 6000m);
			var charge7 = CreateCharge(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 7000m);
			var charge8 = CreateCharge(Constants.CurrencyCodes.UnitedStates, 8000m);
			var charge9 = CreateCharge(Constants.CurrencyCodes.UnitedStates, 9000m);

			AssertChargeSellSide(charge1, true, 1000m, 1000m);
			AssertChargeSellSide(charge2, false, 2000m, 1600m);
			AssertChargeSellSide(charge3, false, 3000m, 2400m);
			AssertChargeSellSide(charge4, true, 4000m, 4000m);
			AssertChargeSellSide(charge5, false, 5000m, 4000m);
			AssertChargeSellSide(charge6, false, 6000m, 4800m);
			AssertChargeSellSide(charge7, true, 7000m, 7000m);
			AssertChargeSellSide(charge8, false, 8000m, 6400m);
			AssertChargeSellSide(charge9, false, 9000m, 7200m);

			var expectedMessageWhenInfoNotCollected = "JobChargeOSSellAmountNotRecalculatedWhenSellCurrencyIsSetToLocalCurrency: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			AssertContains("No Info is collected yet, because OS sell amount == local sell amount.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge1.PK));
			AssertContains("No Info is collected yet, because sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge2.PK));
			AssertContains("No Info is collected yet, because sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge3.PK));
			AssertContains("No Info is collected yet, because OS sell amount == local sell amount.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge4.PK));
			AssertContains("No Info is collected yet, because sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge5.PK));
			AssertContains("No Info is collected yet, because sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge6.PK));
			AssertContains("No Info is collected yet, because OS sell amount == local sell amount.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge7.PK));
			AssertContains("No Info is collected yet, because sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge8.PK));
			AssertContains("No Info is collected yet, because sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge9.PK));

			using (charge1.Calculations.SuspendCalculations())
			{
				charge1.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertChargeSellSide(charge1, false, 1000m, 1000m);
				AssertContains("Info should not be collected because new sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge1.PK));
			}

			using (charge2.Calculations.SuspendCalculations())
			{
				charge2.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				AssertChargeSellSide(charge2, true, 2000m, 1600m);
				var infoForCharge2 = GetCriticalInfo(charge2.PK);
				AssertContains("Info should be collected because OS sell amount != local sell amount.", "Stack Trace:    at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", infoForCharge2);
				AssertContains("Info should be collected because OS sell amount != local sell amount.", $"Charge: PK = {charge2.PK}", infoForCharge2);
				AssertContains("Info should be collected because OS sell amount != local sell amount.", "CalculationSuspender.IsSuspended :Yes", infoForCharge2);
			}

			charge3.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertChargeSellSide(charge3, true, 2400m, 2400m);
			AssertContains("Info should not be collected because OS sell amount == local sell amount.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge3.PK));

			using (charge4.Calculations.SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations())
			{
				charge4.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertChargeSellSide(charge4, false, 4000m, 4000m);
				AssertContains("Info should not be collected because new sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge4.PK));
			}

			using (charge5.Calculations.SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations())
			{
				charge5.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				AssertChargeSellSide(charge5, true, 5000m, 4000m);
				var infoForCharge5 = GetCriticalInfo(charge5.PK);
				AssertContains("Info should be collected because OS sell amount != local sell amount.", "Stack Trace:    at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", infoForCharge5);
				AssertContains("Info should be collected because OS sell amount != local sell amount.", $"Charge: PK = {charge5.PK}", infoForCharge5);
				AssertContains("Info should be collected because OS sell amount != local sell amount.", "LocalToForeignOrForeginToLocalSellAmountCalculationSuspender.IsSuspended :Yes", infoForCharge5);
			}

			charge6.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertChargeSellSide(charge6, true, 4800m, 4800m);
			AssertContains("Info should not be collected because OS sell amount == local sell amount.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge6.PK));

			charge7.Calculations.SwitchOffCalculations();
			charge7.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertChargeSellSide(charge7, false, 7000m, 7000m);
			AssertContains("Info should not be collected because new sell currency is foreign.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge7.PK));

			charge8.Calculations.SwitchOffCalculations();
			charge8.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertChargeSellSide(charge8, true, 8000m, 6400m);
			var infoForCharge8 = GetCriticalInfo(charge8.PK);
			AssertContains("Info should be collected because OS sell amount != local sell amount.", "Stack Trace:    at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", infoForCharge8);
			AssertContains("Info should be collected because OS sell amount != local sell amount.", $"Charge: PK = {charge8.PK}", infoForCharge8);
			AssertContains("Info should be collected because OS sell amount != local sell amount.", "isCaclualtionsSwitchedOff :Yes", infoForCharge8);

			charge9.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertChargeSellSide(charge9, true, 7200m, 7200m);
			AssertContains("Info should not be collected because OS sell amount == local sell amount.", expectedMessageWhenInfoNotCollected, GetCriticalInfo(charge9.PK));

			#region Local Functions

			string GetCriticalInfo(ZGuid chargePk) => infoCollector.GetInfo(chargePk, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmountNotRecalculatedWhenSellCurrencyIsSetToLocalCurrency);

			Charge CreateCharge(ZString currency, ZDecimal osSellAmount)
			{
				var charge = creator.Job1.Charges.AddNew();
				charge.JR_RX_NKSellCurrency = currency;
				charge.JR_OSSellAmt = osSellAmount;
				return charge;
			}

			void AssertChargeSellSide(Charge charge, bool isSellLocal, ZDecimal expectedOSSellAmount, ZDecimal expectedLocalSellAmount)
			{
				AssertEquals(isSellLocal, charge.IsSellLocal);
				AssertEquals(expectedOSSellAmount, charge.JR_OSSellAmt);
				AssertEquals(expectedLocalSellAmount, charge.JR_LocalSellAmt);
			}

			#endregion
		}

		public void TestSwitchOffCalculations()
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.Job1;
			var charge = job.Charges.AddNew();

			var calculationTrigger = new CalculationTrigger(charge);

			Assert(calculationTrigger.Enabled_ForTestOnly);

			using (calculationTrigger.SuspendCalculations())
			{
				Assert(!calculationTrigger.Enabled_ForTestOnly);
			}
			Assert(calculationTrigger.Enabled_ForTestOnly);

			calculationTrigger.SwitchOffCalculations();
			Assert(!calculationTrigger.Enabled_ForTestOnly);

			using (calculationTrigger.SuspendCalculations())
			{
				Assert(!calculationTrigger.Enabled_ForTestOnly);
			}
			Assert(!calculationTrigger.Enabled_ForTestOnly);
		}

		public void TestCalculationSuspender()
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.Job1;
			var charge = job.Charges.AddNew();

			var calculationTrigger = new CalculationTrigger(charge);
			using (calculationTrigger.SuspendCalculations())
			{
				Assert(charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
				Assert(charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
				using (calculationTrigger.SuspendCalculations())
				{
					Assert(charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
					Assert(charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
				}
				Assert(charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
				Assert(charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			}

			Assert(!charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			Assert(!charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
		}

		public void TestUpdateRevenueBasedOnCost_SellAmountNotChangedWhenChargePostedWithJobRevenueJournal()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(EnvProxy.Instance.CurrentCompany.PK);

			var gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
			gatewayConsol.JK_UniqueConsignRef = "C10011991";
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_RL_NKDischargePort = "CNSHA";
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			gatewayConsol.Shipments.AddNew();

			Assert(gatewayConsol.IsGateway());

			using (var gatewayJob = new Job.Loader(gatewayConsol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
				var gatewayCharge = gatewayJob.Charges.AddNew();
				gatewayCharge.JR_AC = chargeCode.PK;
				gatewayCharge.JR_OSSellAmt = 230m;
				gatewayCharge.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				gatewayCharge.JR_JH_InternalJob = gatewayJob.PK;
				gatewayCharge.JR_GE_InternalDept = gatewayCharge.JR_GE;
				gatewayCharge.JR_GB_InternalBranch = gatewayCharge.JR_GB;

				AssertEquals(230m, gatewayCharge.JR_OSCostAmt);

				Factory.Save();

				AssertEquals(1, gatewayConsol.GetApportionments(true).CostsCollection.Count);
				AssertEquals(1, gatewayJob.Charges.Count);
				AssertEquals(230m, gatewayCharge.JR_OSSellAmt);
				Assert(gatewayCharge.IsRevenuePosted);

				gatewayCharge.JR_OSCostAmt = 500m;
				AssertEquals(230m, gatewayCharge.JR_OSSellAmt);
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestNullCharge()
		{
			AssertNotNull(new CalculationTrigger(null));
			Charge aCharge = Factory.New<Charge>();
			AssertNotNull(aCharge);
		}

		public void TestChangeSellCurrencyWhenOppositeLocalAmountIsAlreadySet()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var origIsReciprocal = creator.SetCurrentCompanyReciprocal(true);

			try
			{
				creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
				Job job = creator.Job1;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				Charge charge = job.Charges.AddNew();

				charge.JR_AC = creator.CC1.PK;
				charge.JR_RX_NKCostCurrency = creator.GBP.RX_Code;
				charge.CostExchangeRate.SetBuyRate_ForTestOnly(12.5m);
				charge.JR_OSCostAmt = 100m;
				charge.RevenueExchangeRate?.SetBuyRate_ForTestOnly(12.5m);

				AssertEquals("Should have calculated local value", 1250.00m, charge.JR_LocalCostAmt);
				AssertEquals("Should have calculated correct sell currency", creator.GBP.RX_Code, charge.JR_RX_NKSellCurrency);
				AssertEquals("Should have calculated correct local sell amount", 1250.00m, charge.JR_LocalSellAmt);
				AssertEquals("Should have calculated correct OS sell amount", 100.00m, charge.JR_OSSellAmt);

				charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(12.5m);
				charge.JR_OSSellAmt = 100m;

				AssertEquals("Should keep local value as 1250.00", 1250.00m, charge.JR_LocalSellAmt);
				AssertEquals("Should not have calculated any CFX", 0m, charge.JR_CFXAmt);

				charge.JR_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
				charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
				charge.CostExchangeRate.SetBuyRate_ForTestOnly(8.9m);
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(8.9m);
				charge.JR_OSCostAmt = 140.45m;
				charge.JR_OSSellAmt = 140.45m;

				AssertEquals("Should have calculated correct cost currency", creator.USD.RX_Code, charge.JR_RX_NKCostCurrency);
				AssertEquals("Should have calculated correct sell currency", creator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
				AssertEquals("Should keep local cost amount as 1250.00", 1250.01m, charge.JR_LocalCostAmt);
				AssertEquals("Should keep local sell amount as 1250.00", 1250.01m, charge.JR_LocalSellAmt);
				AssertEquals("Should have calculated correct OS cost amount", 140.45m, charge.JR_OSCostAmt);
				AssertEquals("Should have calculated correct OS sell amount", 140.45m, charge.JR_OSSellAmt);

				charge.CostExchangeRate.SetBuyRate_ForTestOnly(20m);
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(20m);

				AssertEquals("Should have calculated local cost amount", 2809.00m, charge.JR_LocalCostAmt);
				AssertEquals("Should have calculated local sell amount", 2809.00m, charge.JR_LocalSellAmt);
				AssertEquals("Should keep OS cost amount as 140.45", 140.45m, charge.JR_OSCostAmt);
				AssertEquals("Should keep OS sell amount as 140.45", 140.45m, charge.JR_OSSellAmt);

				charge.JR_OSCostAmt = 200m;
				AssertEquals("Should have calculated OS sell amount", 200.00m, charge.JR_OSSellAmt);
				AssertEquals("Should have calculated local cost amount", 4000.00m, charge.JR_LocalCostAmt);
				AssertEquals("Should have calculated local sell amount", 4000.00m, charge.JR_LocalSellAmt);

				charge.JR_OSSellAmt = 150m;
				AssertEquals("Should have calculated OS cost amount", 150.00m, charge.JR_OSCostAmt);
				AssertEquals("Should have calculated local cost amount", 3000.00m, charge.JR_LocalCostAmt);
				AssertEquals("Should have calculated local sell amount", 3000.00m, charge.JR_LocalSellAmt);
			}
			finally
			{
				creator.SetCurrentCompanyReciprocal(origIsReciprocal);
			}
		}

		public void TestLocalSellAmountWhenReverseInvoiceRedoBillingWithDSBCharge()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Japan))
			{
				TestObjectCreator creator = new TestObjectCreator(Factory);
				var job = creator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var taxRate = creator.CreateTaxRate("rate", "test", 1);
				var charge = creator.CreateCharge(job, creator.CC1, "charge1", creator.AUD, 10m, creator.Creditor1, creator.AUD, 10m, creator.AALSHI);
				charge.JR_AT_SellGSTRate = creator.GST1.PK;
				charge.JR_AT_CostGSTRate = taxRate.PK;
				charge.JR_AT_SellGSTRate = taxRate.PK;
				charge.JR_ChargeType = Core.Constants.ChargeType.Disbursement;
				charge.JR_CostRatingOverride = false;
				charge.JR_OSSellAmt = 505.53m;
				charge.JR_OSCostAmt = 505.53m;
				charge.JR_RX_NKSellCurrency = "USD";
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(136.25m);
				charge.JR_LocalCostAmt = 68879m;
				charge.JR_LocalSellAmt = 68879m;
				Factory.Save();

				var originalInvoice = creator.CreateInvoice(typeof(ARInvoice), "AR001", creator.AUD, 1m, creator.AALSHI);
				originalInvoice.AH_PostDate = new ZDate(2022, 3, 31);
				originalInvoice.Lines.Add(creator.CreateRevenueLine(charge, originalInvoice.PK));
				Factory.Save();

				var reverser = new JobInvoicingReverser(job);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				AssertEquals(charge.JR_LocalSellAmt, 68879m);   // After reversal the LocalSellAmt is unchanged.
				AssertEquals(charge.JR_LocalCostAmt, 68879m);
			}
		}

		public void TestChangeSellCurrencyWhenCostCurrencyLocalAndSellCurrencyForeignWithoutCFX()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				var origIsReciprocal = creator.SetCurrentCompanyReciprocal(true);
				try
				{
					creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = "KRW";
					creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = "KRW";
					var job = creator.Job1;
					job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					var charge = job.Charges.AddNew();

					var exRate = creator.CreateExchangeRate(job, creator.USD, 1076.9m);
					exRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
					exRate.JF_CFXPercent = 0m;
					exRate.JF_CFXMinimum = 0m;
					charge.InvoicingJob.ExchangeRates.Add(exRate);
					charge.JR_ChargeType = Constants.ChargeType.Disbursement;
					charge.JR_AC = creator.CC1.PK;
					charge.JR_RX_NKCostCurrency = "KRW";

					charge.JR_OSCostAmt = 2003000m;
					charge.JR_RX_NKSellCurrency = creator.USD.Code;
					AssertEquals("Should not be changed", 2003000m, charge.JR_OSCostAmt);
					AssertEquals("Should keep local value as 2003000m", 2003000m, charge.JR_LocalCostAmt);
					AssertEquals("Should have calculated correct OS sell amount", 1859.97m, charge.JR_OSSellAmt);
					AssertEquals("Should keep local value as 2003000m", 2003000m, charge.JR_LocalSellAmt);
					AssertEquals("Should be 0", 0m, charge.JR_CFXAmt);
				}
				finally
				{
					creator.SetCurrentCompanyReciprocal(origIsReciprocal);
				}
			}
		}

		public void TestChangeSellCurrencyWhenCostCurrencyLocalAndSellCurrencyForeignWithCFX()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.AUD.Code;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = creator.AUD.Code;
			Job job = creator.Job1;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var charge = job.Charges.AddNew();

			var exRate = creator.CreateExchangeRate(job, creator.USD, 2m);
			exRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			exRate.JF_CFXPercent = 2m;
			charge.InvoicingJob.ExchangeRates.Add(exRate);
			charge.JR_ChargeType = Constants.ChargeType.Disbursement;
			charge.JR_AC = creator.CC1.PK;
			charge.JR_RX_NKCostCurrency = creator.AUD.RX_Code;

			charge.JR_OSCostAmt = 100m;
			charge.JR_RX_NKSellCurrency = creator.USD.Code;
			AssertEquals("Should have calculated local value", 100.00m, charge.JR_LocalCostAmt);
			AssertEquals("Should have calculated correct sell currency", creator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("Should have calculated correct OS sell amount", 200.00m, charge.JR_OSSellAmt);
			AssertEquals("Should have calculated correct local sell amount", 102.04m, charge.JR_LocalSellAmt);
			AssertEquals("Should have calculated correct CFX", 2.04m, charge.JR_CFXAmt);
		}

		public void TestChangeSellCurrencyFor2DiffForeignCurrenciesAndDisbursement()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var origIsReciprocal = creator.SetCurrentCompanyReciprocal(true);

			try
			{
				creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
				Job job = creator.Job1;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				var charge = job.Charges.AddNew();

				charge.JR_AC = creator.CC2.PK;
				charge.JR_RX_NKCostCurrency = creator.GBP.RX_Code;

				AssertNotNull(charge.CostExchangeRate);
				charge.CostExchangeRate.SetBuyRate_ForTestOnly(12.5m);

				charge.JR_OSCostAmt = 100m;

				AssertNotNull(charge.RevenueExchangeRate);
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(12.5m);

				AssertEquals("Should have calculated local value", 1250.00m, charge.JR_LocalCostAmt);
				AssertEquals("Should have calculated correct sell currency", creator.GBP.RX_Code, charge.JR_RX_NKSellCurrency);
				AssertEquals("Should have calculated correct local sell amount", 1250.00m, charge.JR_LocalSellAmt);
				AssertEquals("Should have calculated correct OS sell amount", 100.00m, charge.JR_OSSellAmt);

				charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;

				AssertNotNull(charge.RevenueExchangeRate);
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(8.9m);

				AssertEquals("Should keep local value as 1250.00", 1250.00m, charge.JR_LocalSellAmt);
				AssertEquals("Should not have calculated any CFX", 0m, charge.JR_CFXAmt);
			}
			finally
			{
				creator.SetCurrentCompanyReciprocal(origIsReciprocal);
			}
		}

		public void TestChangeSellCurrencyToDiggerentForeignCurrencyOnCostPostedDisbursementCharge()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var origIsReciprocal = creator.SetCurrentCompanyReciprocal(true);

			try
			{
				creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
				Job job = creator.Job1;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				var charge = job.Charges.AddNew();

				charge.JR_AC = creator.CC2.PK;
				charge.JR_RX_NKCostCurrency = creator.GBP.RX_Code;
				Assert(charge.IsDisbursementCharge);

				AssertNotNull(charge.CostExchangeRate);
				charge.CostExchangeRate.SetBuyRate_ForTestOnly(12.5m);

				charge.JR_OSCostAmt = 100m;

				AssertNotNull(charge.RevenueExchangeRate);
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(12.5m);

				AssertEquals("Should have calculated local value", 1250.00m, charge.JR_LocalCostAmt);
				AssertEquals("Should have calculated correct sell currency", creator.GBP.RX_Code, charge.JR_RX_NKSellCurrency);
				AssertEquals("Should have calculated correct local sell amount", 1250.00m, charge.JR_LocalSellAmt);
				AssertEquals("Should have calculated correct OS sell amount", 100.00m, charge.JR_OSSellAmt);

				var invoice = creator.CreateAPInvoice<APInvoice>("I0001", creator.GBP, 12.5m, 100m, 0m, 0m, 1250m, 0m, 0m, creator.LocalClient);
				charge.JR_AL_APLine = invoice.Lines[0].PK;
				AssertNull(charge.CostExchangeRate);

				charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;

				AssertNotNull(charge.RevenueExchangeRate);
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(8.9m);

				AssertEquals("Should keep local value as 1250.00", 1250.00m, charge.JR_LocalSellAmt);
				AssertEquals("Should calculate OS value as 140.45", 140.45m, charge.JR_OSSellAmt);
			}
			finally
			{
				creator.SetCurrentCompanyReciprocal(origIsReciprocal);
			}
		}

		public void TestChangeSellCurrencyWillAlsoChangeCostCurrencyForDSBCharge()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			Charge charge = job.Charges.AddNew();
			charge.JR_ChargeType = Constants.ChargeType.Disbursement;

			charge.JR_AC = creator.CC2.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.2m);
			charge.JR_OSSellAmt = 150m;
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(1.2m);
			charge.JR_OSSellAmt = 150m;

			AssertEquals("Sell currency should be USD", creator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("USD: JR_OSSellAmt", 150m, charge.JR_OSSellAmt);
			AssertEquals("AUD: JR_LocalSellAmt", 125m, charge.JR_LocalSellAmt);

			AssertEquals("Cost currency should be USD", creator.USD.RX_Code, charge.JR_RX_NKCostCurrency);
			AssertEquals("USD: JR_OSCostAmt", 150m, charge.JR_OSCostAmt);
			AssertEquals("AUD: JR_LocalCostAmt", 125m, charge.JR_LocalCostAmt);
		}

		public void TestChangeSellCurrencyFromForeignToLocalCurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			var charge = job.Charges.AddNew();
			charge.JR_ChargeType = Constants.ChargeType.Margin;

			charge.JR_AC = creator.CC2.PK;
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_LocalSellAmt = 1000m;
			charge.JR_OSSellAmt = 1000m;
			charge.JR_RX_NKCostCurrency = creator.GBP.RX_Code;
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5754m);

			charge.JR_OSCostAmt = 470.84m;
			charge.JR_LocalCostAmt = 818.28m;

			AssertEquals("GBP: JR_LocalCostAmt", 818.28m, charge.JR_LocalCostAmt);
			AssertEquals("GBP: JR_OSCostAmt", 470.84m, charge.JR_OSCostAmt);
			AssertEquals("AUD: JR_LocalSellAmt", 1000.00m, charge.JR_LocalSellAmt);
			AssertEquals("AUD: JR_OSSellAmt", 1000.00m, charge.JR_OSSellAmt);

			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;

			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.9318m);
			AssertEquals(0.9318m, charge.RevenueExchangeRate.SellRate);
			charge.JR_LocalSellAmt = 1000m;
			AssertEquals("USD: JR_OSSellAmt", 931.80m, charge.JR_OSSellAmt);

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_LocalSellAmt = 1000m;
			AssertEquals("Back to AUD: JR_OSSellAmt", 1000m, charge.JR_OSSellAmt);
		}

		public void TestChangeSellCurrencyFromLocalToForeignCurrencyWithRoundingDelta()
		{
			var creator = new TestObjectCreator(Factory);
			var reciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Thailand))
			using (new DisposableAction(() => creator.SetCurrentCompanyReciprocal(true), () => creator.SetCurrentCompanyReciprocal(reciprocal)))
			{
				var job = creator.Job1;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				job.ExchangeRates.AddRate(creator.USD, 32.82m, job.LocalCharges.PK, ExchangeRateOrgTypeEnum.Debtor);
				job.ExchangeRates.AddRate(creator.USD, 32.83m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor);

				var charge = job.Charges.AddNew();
				charge.JR_AC = creator.CC1.PK; // Margin 100% Charge Code
				charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;

				charge.JR_OSCostAmt = 13m;

				AssertEquals("USD: JR_LocalCostAmt", 426.79m, charge.JR_LocalCostAmt);
				AssertEquals("USD: JR_OSCostAmt", 13m, charge.JR_OSCostAmt);
				AssertEquals("THB: JR_OSSellAmt", 426.79m, charge.JR_OSSellAmt);
				AssertEquals("THB: JR_LocalSellAmt", 426.79m, charge.JR_LocalSellAmt);

				charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;

				AssertEquals("USD: JR_OSSellAmt", 13m, charge.JR_OSSellAmt);
				AssertEquals("USD: JR_LocalSellAmt is preserved", 426.79m, charge.JR_LocalSellAmt);
			}
		}

		public void TestSetFromForeignToLocalAmountWithCfxMin()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Belgium))
			{
				var creator = new TestObjectCreator(Factory);

				var job = creator.Job1;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5m, 5.68m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				var jpy = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Japan);
				job.ExchangeRates.AddRate(jpy, 128.18m, job.LocalCharges.PK, ExchangeRateOrgTypeEnum.Debtor);
				job.ExchangeRates.AddRate(jpy, 128.18m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor);

				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = creator.CC1.PK; // Margin 100% Charge Code
				charge1.JR_RX_NKSellCurrency = jpy.Code;
				charge1.JR_RX_NKCostCurrency = jpy.Code;

				charge1.JR_OSSellAmt = 2520m;

				AssertEquals("JPY: JR_OSCostAmt", 2520m, charge1.JR_OSCostAmt);
				AssertEquals("JPY: JR_LocalCostAmt", 19.66m, charge1.JR_LocalCostAmt);
				AssertEquals("JPY: JR_OSSellAmt", 2520m, charge1.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 25.34m, charge1.JR_LocalSellAmt);

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = creator.CC1.PK; // Margin 100% Charge Code

				charge2.JR_OSSellAmt = 2520m;

				AssertEquals("EUR: JR_OSCostAmt", 2520m, charge2.JR_OSCostAmt);
				AssertEquals("EUR: JR_LocalCostAmt", 2520m, charge2.JR_LocalCostAmt);
				AssertEquals("EUR: JR_OSSellAmt", 2520m, charge2.JR_OSSellAmt);
				AssertEquals("EUR: JR_LocalSellAmt", 2520m, charge2.JR_LocalSellAmt);

				charge2.JR_RX_NKSellCurrency = jpy.Code;
				charge2.JR_OSSellAmt = 2520m;

				AssertEquals("EUR: JR_OSCostAmt", 2520m, charge2.JR_OSCostAmt);
				AssertEquals("EUR: JR_LocalCostAmt", 2520m, charge2.JR_LocalCostAmt);

				AssertEquals("JPY: JR_OSSellExRate", 99.447514m, charge2.JR_OSSellExRate);
				AssertEquals("JPY: JR_OSSellAmt", 2520m, charge2.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 25.34m, charge2.JR_LocalSellAmt);

				charge2.JR_OSSellAmt = 13830m; // Magic OS Amount just before giving up CFX Min

				AssertEquals("JPY: JR_OSSellExRate", 121.764395m, charge2.JR_OSSellExRate);
				AssertEquals("JPY: JR_OSSellAmt", 13830m, charge2.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 113.58m, charge2.JR_LocalSellAmt);

				charge2.JR_OSSellAmt = 13831m; // Now we do not use CFX Min anymore, only CFX %

				AssertEquals("JPY: JR_OSSellExRate", 121.771m, charge2.JR_OSSellExRate);
				AssertEquals("JPY: JR_OSSellAmt", 13831m, charge2.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 113.58m, charge2.JR_LocalSellAmt);
			}
		}

		public void TestSetFromLocalToForeignAmountWithCfxMin()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Belgium))
			{
				var creator = new TestObjectCreator(Factory);

				var job = creator.Job1;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5m, 5.68m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				var jpy = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Japan);
				job.ExchangeRates.AddRate(jpy, 128.18m, job.LocalCharges.PK, ExchangeRateOrgTypeEnum.Debtor);
				job.ExchangeRates.AddRate(jpy, 128.18m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor);

				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = creator.CC1.PK; // Margin 100% Charge Code
				charge1.JR_RX_NKSellCurrency = jpy.Code;
				charge1.JR_RX_NKCostCurrency = jpy.Code;

				charge1.JR_OSSellAmt = 2520m;

				AssertEquals("JPY: JR_OSCostAmt", 2520m, charge1.JR_OSCostAmt);
				AssertEquals("JPY: JR_LocalCostAmt", 19.66m, charge1.JR_LocalCostAmt);
				AssertEquals("JPY: JR_OSSellAmt", 2520m, charge1.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 25.34m, charge1.JR_LocalSellAmt);

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = creator.CC1.PK; // Margin 100% Charge Code

				charge2.JR_OSSellAmt = 2000m;

				AssertEquals("EUR: JR_OSCostAmt", 2000m, charge2.JR_OSCostAmt);
				AssertEquals("EUR: JR_LocalCostAmt", 2000m, charge2.JR_LocalCostAmt);
				AssertEquals("EUR: JR_OSSellAmt", 2000m, charge2.JR_OSSellAmt);
				AssertEquals("EUR: JR_LocalSellAmt", 2000m, charge2.JR_LocalSellAmt);

				charge2.JR_RX_NKSellCurrency = jpy.Code;
				charge2.JR_LocalSellAmt = 25.34m;

				AssertEquals("EUR: JR_OSCostAmt", 2000m, charge2.JR_OSCostAmt);
				AssertEquals("EUR: JR_LocalCostAmt", 2000m, charge2.JR_LocalCostAmt);

				AssertEquals("JPY: JR_OSSellExRate", 99.447514m, charge2.JR_OSSellExRate);
				AssertEquals("JPY: JR_OSSellAmt", 2520m, charge2.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 25.34m, charge2.JR_LocalSellAmt);

				charge2.JR_LocalSellAmt = 113.57m; // Magic Local Amount just before giving up CFX Min

				AssertEquals("JPY: JR_OSSellExRate", 121.766312m, charge2.JR_OSSellExRate);
				AssertEquals("JPY: JR_OSSellAmt", 13829m, charge2.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 113.57m, charge2.JR_LocalSellAmt);

				charge2.JR_LocalSellAmt = 113.58m; // Now we do not use CFX Min anymore, only CFX %

				AssertEquals("JPY: JR_OSSellExRate", 121.771m, charge2.JR_OSSellExRate);
				AssertEquals("JPY: JR_OSSellAmt", 13831m, charge2.JR_OSSellAmt);
				AssertEquals("JPY: JR_LocalSellAmt", 113.58m, charge2.JR_LocalSellAmt);
			}
		}

		public void TestLocalSellCalculationWhenJobCFXChanges()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var origIsReciprocal = creator.SetCurrentCompanyReciprocal(true);

			try
			{
				creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
				creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				creator.Agent.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
				Job job = creator.Job1;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
				creator.CreateExchangeRate(job, creator.USD, 8.9m);
				creator.CreateExchangeRate(job, creator.GBP, 12.5m);
				Charge mRGCharge = job.Charges.AddNew();
				Charge dSBCharge = job.Charges.AddNew();

				mRGCharge.JR_AC = creator.CC1.PK; //CC1 is a MRG charge code
				mRGCharge.JR_RX_NKCostCurrency = creator.GBP.RX_Code;
				mRGCharge.JR_OH_SellAccount = job.AgentCollectPK;

				dSBCharge.JR_AC = creator.CC2.PK; //CC2 is a DSB charge code
				dSBCharge.JR_RX_NKCostCurrency = creator.GBP.RX_Code;
				dSBCharge.JR_OH_SellAccount = job.AgentCollectPK;

				mRGCharge.JR_OSCostAmt = 100m;
				AssertEquals("Should have calculated local value", 1250.00m, mRGCharge.JR_LocalCostAmt);
				AssertEquals("Should have calculated correct sell currency", creator.GBP.RX_Code, mRGCharge.JR_RX_NKSellCurrency);
				AssertEquals("Should have calculated correct local sell amount", 1250.00m, mRGCharge.JR_LocalSellAmt);
				AssertEquals("Should have calculated correct OS sell amount", 100.00m, mRGCharge.JR_OSSellAmt);

				dSBCharge.JR_OSCostAmt = 100m;
				AssertEquals("Should have calculated local value", 1250.00m, dSBCharge.JR_LocalCostAmt);
				AssertEquals("Should have calculated correct sell currency", creator.GBP.RX_Code, dSBCharge.JR_RX_NKSellCurrency);
				AssertEquals("Should have calculated correct local sell amount", 1250.00m, dSBCharge.JR_LocalSellAmt);
				AssertEquals("Should have calculated correct OS sell amount", 100.00m, dSBCharge.JR_OSSellAmt);

				mRGCharge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
				dSBCharge.JR_RX_NKSellCurrency = creator.USD.RX_Code;

				AssertEquals("OS Sell Amount", 140.45m, mRGCharge.JR_OSSellAmt);
				AssertEquals("Should keep local value as 1250.00", 1250.00m, mRGCharge.JR_LocalSellAmt);
				AssertEquals("Should not have calculated any CFX", 0m, mRGCharge.JR_CFXAmt);

				AssertEquals("OS Sell Amount", 140.45m, dSBCharge.JR_OSSellAmt);
				AssertEquals("Should keep local value as 1250.00", 1250.00m, dSBCharge.JR_LocalSellAmt);
				AssertEquals("Should not have calculated any CFX", 0m, dSBCharge.JR_CFXAmt);

				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 10m);

				//force refreshing on charge as it does not happen when configuration changes;
				//basically it triggers the same behaviour as when ex rate changes;
				mRGCharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(8.9m);

				AssertEquals("OS Sell Amount", 140.45m, mRGCharge.JR_OSSellAmt);
				AssertEquals("Should recalculate local sell value", 1375.01m, mRGCharge.JR_LocalSellAmt);

				dSBCharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(8.9m); //force refreshing on charge as it does not happen when configuration changes;

				AssertEquals("OS Sell Amount", 140.45m, dSBCharge.JR_OSSellAmt);
				AssertEquals("Should recalculate local sell value", 1375.01m, dSBCharge.JR_LocalSellAmt);

				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

				mRGCharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(8.9m); //force refreshing on charge as it does not happen when configuration changes;

				AssertEquals("OS Sell Amount", 140.45m, mRGCharge.JR_OSSellAmt);
				AssertEquals("Should recalculate local sell value", 1250.01m, mRGCharge.JR_LocalSellAmt);

				dSBCharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(8.9m); //force refreshing on charge as it does not happen when configuration changes;

				AssertEquals("OS Sell Amount", 140.45m, dSBCharge.JR_OSSellAmt);
				AssertEquals("Should recalculate local sell value", 1250.00m, dSBCharge.JR_LocalSellAmt);
			}
			finally
			{
				creator.SetCurrentCompanyReciprocal(origIsReciprocal);
			}
		}

		public void TestSetSellWhenCostIsZeroForDisbursements()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			creator.CreateExchangeRate(job, creator.USD, 0.65m);
			Charge charge = job.Charges.AddNew();

			charge.JR_AC = creator.CC2.PK;
			charge.JR_OSCostAmt = 200m;
			AssertEquals("Should have calculated Sell", 200m, charge.JR_OSSellAmt);

			charge.JR_OSCostAmt = 0m;

			AssertEquals("Should have changed sell", 0m, charge.JR_OSSellAmt);
		}

		public void TestSettingSellSetsCostForDisbursements()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			creator.Agent.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 2m);
			creator.CreateExchangeRate(job, creator.USD, 0.65m);
			Charge charge = job.Charges.AddNew();

			charge.JR_AC = creator.DSBChargeCode.PK;
			charge.JR_OSSellAmt = 150m;

			AssertEquals("Should have calculated local sell value in local curr.", 150m, charge.JR_LocalSellAmt);
			AssertEquals("Should have calculated cost amount", 150m, charge.JR_OSCostAmt);
			AssertEquals("Should have calculated local cost amount - charge is still local", 150m, charge.JR_LocalCostAmt);

			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_OH_SellAccount = creator.Agent.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;

			AssertEquals("Should recalculate the foreign amount from the local", 97.5m, charge.JR_OSCostAmt);

			charge.JR_OSCostAmt = 200m;

			AssertEquals("Should have recalculated the local cost amount", 307.69m, charge.JR_LocalCostAmt);
			AssertEquals("Should have recalculated the local sell amount with sell rate and CFX", 313.97m, charge.JR_LocalSellAmt);

			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine cSTLine = (APInvoiceLine)invoice.Lines.AddNew();
			charge.JR_AL_APLine = cSTLine.PK;

			charge.JR_OSSellAmt = 150m;

			AssertEquals("Should not have recalculated cost amounts", 200m, charge.JR_OSCostAmt);
			AssertEquals("Should not have recalculated cost amounts", 307.69m, charge.JR_LocalCostAmt);
		}

		public void TestDisbursementBehaviourForTwoDifferentForeignCurrencies()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			creator.Agent.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			Job job = creator.Job1;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.DSBChargeCode.PK;

			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_OH_SellAccount = creator.Agent.PK;
			charge.JR_OSCostAmt = 200m;

			AssertNotNull(charge.RevenueExchangeRate);
			AssertEquals(creator.USD.RX_Code, charge.RevenueExchangeRate.CurrencyCode);
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.745m);
			AssertEquals(0.7301m, charge.RevenueExchangeRate.SellRate);

			charge.JR_LocalSellAmt = 273.94m;

			AssertEquals("Sell Currency should be same as cost because no values have been entered", creator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("Local Sell Amount should be calculated correctly from new OS Sell Amt and sell ex rate", 273.94m, charge.JR_LocalSellAmt);
			AssertEquals("OS Sell Amount should be same as cost", 200m, charge.JR_OSSellAmt);

			charge.JR_RX_NKSellCurrency = creator.GBP.RX_Code;

			AssertNotNull(charge.RevenueExchangeRate);
			AssertEquals(creator.GBP.RX_Code, charge.RevenueExchangeRate.CurrencyCode);
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.423m);
			AssertEquals(0.41454m, charge.RevenueExchangeRate.SellRate);

			charge.JR_RX_NKCostCurrency = creator.GBP.RX_Code;

			AssertNotNull(charge.CostExchangeRate);
			AssertEquals(creator.GBP.RX_Code, charge.CostExchangeRate.CurrencyCode);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.423m);
			AssertEquals(0.423m, charge.CostExchangeRate.SellRate);

			charge.JR_OSCostAmt = 170.32;
			AssertEquals("Sell currency should stay the same as set above", creator.GBP.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("Local cost amount should be calculated correctly", 402.65m, charge.JR_LocalCostAmt);
			AssertEquals("Local Sell Amount should be calculated correctly from new OS Sell Amt and sell ex rate", 410.87m, charge.JR_LocalSellAmt);
			AssertEquals("OS Sell Amount should be calculated correctly", 170.32m, charge.JR_OSSellAmt);
		}

		public void TestDontSetCostFromSellWhenCostIsApportioned()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			creator.Agent.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			Job job = creator.Job1;
			var uSDRate = creator.CreateExchangeRate(job, creator.USD, 0.745m);
			var gBPRate = creator.CreateExchangeRate(job, creator.GBP, 0.423m);

			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 2);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.DSBChargeCode.PK;

			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_OH_SellAccount = creator.Agent.PK;
			charge.JR_OSCostAmt = 200m;
			charge.JR_E6 = ZGuid.NewZGuid();

			AssertEquals("Sell Currency should be same as cost because no values have been entered", creator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("Local Sell Amount should be calculated correctly from new OS Sell Amt and sell ex rate", 273.94m, charge.JR_LocalSellAmt);
			AssertEquals("OS Sell Amount should be same as cost", 200m, charge.JR_OSSellAmt);

			charge.JR_OSSellAmt = 500m;

			AssertEquals("Local cost amount should not be changed because cost is apportioned", 268.46m, charge.JR_LocalCostAmt);
			AssertEquals("Foreign cost amount should not be changed because cost is apportioned", 200m, charge.JR_OSCostAmt);
		}

		public void TestDontSetSellFromCostWhenChargeTypeIsManualAccrualJob()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.ManualJobAccrualChargeCode.PK;

			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.JR_OSCostAmt = 100m;
			charge.JR_E6 = ZGuid.NewZGuid();

			AssertEquals("Cost Currency is USD", creator.USD.RX_Code, charge.JR_RX_NKCostCurrency);
			AssertEquals("OS Cost Amount ", 100m, charge.JR_OSCostAmt);
			AssertEquals("Local Cost Amount ", 200m, charge.JR_LocalCostAmt);

			AssertEquals("Sell Currency should not be changed because charge type is Manual Job Accrual", creator.AUD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("Local Sell Amount should not be set because charge type is Manual Job Accrual", 0m, charge.JR_LocalSellAmt);
			AssertEquals("OS Sell Amount should not be set because charge type is Manual Job Accrual", 0m, charge.JR_OSSellAmt);
		}

		public void TestDontSetCostFromSellWhenChargeTypeIsManualAccrualJob()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.ManualJobAccrualChargeCode.PK;

			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.JR_OSSellAmt = 100m;
			charge.JR_E6 = ZGuid.NewZGuid();

			AssertEquals("Sell Currency is USD", creator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals("OS Sell Amount ", 100m, charge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount ", 200m, charge.JR_LocalSellAmt);

			AssertEquals("Cost Currency should not be changed because charge type is Manual Job Accrual", creator.AUD.RX_Code, charge.JR_RX_NKCostCurrency);
			AssertEquals("Local Cost Amount should not be set because charge type is Manual Job Accrual", 0m, charge.JR_LocalCostAmt);
			AssertEquals("OS Cost Amount should not be set because charge type is Manual Job Accrual", 0m, charge.JR_OSCostAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestDontSetSellWhenSellAmtIsNotEmpty()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			Job job = creator.Job1;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC8.PK;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_OSCostAmt = 180m;
			AssertEquals("Local Cost is zero (no exchange rate)", 0m, charge.JR_LocalCostAmt);
			AssertEquals("Local Sell is zero (no exchange rate)", 0m, charge.JR_LocalSellAmt);
			AssertEquals("Should have calculated Sell", 200m, charge.JR_OSSellAmt);
			AssertEquals("USD", charge.JR_OSSellCurrencyCode);

			charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC8.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_OSSellAmt = 500m;
			AssertEquals("Local Sell is zero (no exchange rate)", 0m, charge.JR_LocalSellAmt);
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_OSCostAmt = 180m;
			AssertEquals("Local Cost is zero (no exchange rate)", 0m, charge.JR_LocalCostAmt);
			AssertEquals("Sell is not set", 500m, charge.JR_OSSellAmt);
		}

		public void TestCostExRateChangedOnlyUpdateDSBChargeAmountsIfRevenueIsNotPosted()
		{
			var creator = new TestObjectCreator(Factory);
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			var job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			var charge = job.Charges.AddNew();
			charge.JR_AC = creator.DSBChargeCode.PK;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_OH_SellAccount = creator.Agent.PK;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.6m);

			var rate = charge.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.OrgType == ExchangeRateOrgTypeEnum.Debtor);
			AssertEquals(0.6m, rate.JF_BaseRate);
			rate.JF_IsTransformed = true;

			charge.JR_OSSellAmt = 150m;
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.6m);
			charge.JR_LocalCostAmt = 150m;
			AssertEquals("Should recalculate the foreign amount from the local", 90m, charge.JR_OSCostAmt);

			rate.JF_BaseRate = 2m;
			AssertEquals("Should recalculate the local sell amount because revenue is not posted yet", 45m, charge.JR_LocalSellAmt);
			AssertEquals("Cost part should not change", 90m, charge.JR_OSCostAmt);
			AssertEquals("Cost part should not change", 150m, charge.JR_LocalCostAmt);
			AssertEquals("OS Sell amount should be equal to OS Cost amount", 90m, charge.JR_OSSellAmt);

			rate.JF_CFXPercent = 10m;

			AssertEquals("Should recalculate the local sell amount taking CFX in consideration because revenue is not posted yet", 50m, charge.JR_LocalSellAmt);
			AssertEquals("Cost part should not change", 90m, charge.JR_OSCostAmt);
			AssertEquals("Cost part should not change", 150m, charge.JR_LocalCostAmt);
			AssertEquals("OS Sell amount should be equal to OS Cost amount", 90m, charge.JR_OSSellAmt);

			rate.JF_BaseRate = 1m;

			AssertEquals("Should recalculate the local sell amount taking CFX in consideration because revenue is not posted yet", 100m, charge.JR_LocalSellAmt);
			AssertEquals("Cost part should not change", 90m, charge.JR_OSCostAmt);
			AssertEquals("Cost part should not change", 150m, charge.JR_LocalCostAmt);
			AssertEquals("OS Sell amount should be equal to OS Cost amount", 90m, charge.JR_OSSellAmt);

			charge.JR_OSCostAmt = 120m;

			AssertEquals("Should recalculate the local sell amount taking CFX in consideration because revenue is not posted yet", 133.33m, charge.JR_LocalSellAmt);
			AssertEquals("Local cost should be re-calculated based on new OS Amount", 200m, charge.JR_LocalCostAmt);
			AssertEquals("OS Sell amount should be equal to OS Cost amount", 120m, charge.JR_OSSellAmt);

			ARInvoice invoice = Factory.New<ARInvoice>();
			ARInvoiceLine arLine = (ARInvoiceLine)invoice.Lines.AddNew();
			charge.JR_AL_ARLine = arLine.PK;

			rate.JF_BaseRate = 3m;
			AssertEquals("Should not recalculate local sell amount because revenue is already posted", 133.33m, charge.JR_LocalSellAmt);
		}

		public void TestUpdateDSBChargeAmountsOnApprtionmentCharge()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			var shipment = creator.CreateShipment("S0001", consol);

			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = creator.EUR.Code;
			var job = creator.CreateJob(shipment, creator.LocalClient, 0m, creator.Agent, 0m);
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			var usdRate = creator.USD.ExchangeRates.AddNew();
			usdRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			usdRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			usdRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			usdRate.RE_SellRate = 1.25m;

			var eurRate = creator.EUR.ExchangeRates.AddNew();
			eurRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			eurRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			eurRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			eurRate.RE_SellRate = 1.56m;

			Factory.Save();
			ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			var charge = creator.CreateCharge(job, creator.DSBChargeCode, 50m, 50m);
			charge.JR_OH_SellAccount = creator.Agent.PK;

			Factory.Save();

			AssertEquals(1, job.Charges.Count);
			AssertEquals(charge.PK, job.Charges[0].PK);

			Assert(charge.JR_OH_CostAccount.IsEmpty);
			AssertEquals(creator.AUD.Code, charge.JR_RX_NKCostCurrency);
			AssertNull("No CostExchangeRate!", charge.CostExchangeRate);
			AssertEquals(1m, charge.JR_OSCostExRate);
			AssertEquals(50m, charge.JR_OSCostAmt);
			AssertEquals(50m, charge.JR_LocalCostAmt);

			AssertEquals(creator.Agent.PK, charge.JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, charge.JR_RX_NKSellCurrency);
			AssertNotNull(charge.RevenueExchangeRate);
			AssertEquals(1.56m, charge.RevenueExchangeRate.Rate);
			AssertEquals(78m, charge.JR_OSSellAmt);
			AssertEquals(50m, charge.JR_LocalSellAmt);

			var cost = creator.CreateConsolCost(consol, creator.DSBChargeCode, creator.Creditor1);
			AssertEquals(Constants.ChargeType.Disbursement, cost.ChargeCode.AC_ChargeType);
			cost.E6_RX_NKCurrency = creator.USD.Code;
			cost.E6_ExchangeRate = 1.3555m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 50m;

			Factory.Save();

			AssertEquals(1, job.Charges.Count);
			AssertEquals(charge.PK, job.Charges[0].PK);

			AssertEquals(creator.Creditor1.PK, charge.JR_OH_CostAccount);
			AssertEquals(creator.USD.Code, charge.JR_RX_NKCostCurrency);
			AssertNull("No CostExchangeRate!", charge.CostExchangeRate);
			AssertEquals(1.3555m, charge.JR_OSCostExRate);
			AssertEquals(50m, charge.JR_OSCostAmt);
			AssertEquals(36.89m, charge.JR_LocalCostAmt);

			AssertEquals(creator.Agent.PK, charge.JR_OH_SellAccount);
			AssertEquals(creator.EUR.Code, charge.JR_RX_NKSellCurrency);
			AssertNotNull(charge.RevenueExchangeRate);
			AssertEquals(1.56m, charge.RevenueExchangeRate.Rate);
			AssertEquals(57.54m, charge.JR_OSSellAmt);
			AssertEquals(36.89m, charge.JR_LocalSellAmt);
		}

		public void TestUpdatingDSBSellFromCostDoesntSetOverridenFlag()
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.Job1;

			var charge = job.Charges.AddNew();
			charge.JR_AC = creator.DSBChargeCode.PK;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_LocalSellAmt = 10m;
			charge.JR_SellRatingOverride = false;

			Assert("Prerequisite", !charge.JR_SellRatingOverride);

			var calculationTrigger = new CalculationTrigger(charge);
			calculationTrigger.CostExRateChanged();

			Assert("Should not tick Override Rating flag", !charge.JR_SellRatingOverride);
		}

		public void TestCostCurrencyChangeOnSellAmtChanged()
		{
			ExceptionReporterTestListener.Instance.Clear();

			var creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCurrency(creator.CurrencyWithoutCents.Code);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			var shipment = creator.CreateShipment("Test1234", false);

			var job = creator.CreateJob(shipment, false);

			job.ExchangeRates.AddRate(creator.USD, 0M, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);

			GlbCompany.CurrentCompany.Factory.Save();

			var jobCharge = creator.CreateCharge(job, creator.CC1, "charge",
				creator.USD, 2.22M, creator.AALSHI, "charge",
				creator.CurrencyWithoutCents, 1M, creator.AALSHI);

			jobCharge.JR_OSCostExRate = 0;
			AssertEquals(creator.CC1.AC_ChargeType, Core.Constants.ChargeType.Margin);
			AssertEquals(jobCharge.JR_RX_NKCostCurrency, creator.USD.Code);
			AssertEquals(jobCharge.JR_OSCostAmt, 2.22M);
			AssertEquals(jobCharge.JR_LocalCostAmt, 0M);

			jobCharge.JR_OSSellAmt = 2000M;

			AssertEquals(jobCharge.JR_RX_NKCostCurrency, creator.CurrencyWithoutCents.Code);
			AssertEquals(jobCharge.JR_OSSellAmt.DecimalPlaces, creator.CurrencyWithoutCents.Decimals);
			var errorMessage = @"JR_OSCostAmt: 2.22
Currency USD was changed to KRW
Only 0 decimals are allowed.";
			if (ExceptionReporterTestListener.Instance.Count > 0)
			{
				AssertNotContains(ExceptionReporterTestListener.Instance[0].InnerException.Message, errorMessage, ExceptionReporterTestListener.Instance[0].InnerException.Message);
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSellCurrencyChange()
		{
			var creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCurrency(creator.CurrencyWithoutCents.Code);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			var shipment = creator.CreateShipment("Test1234", false);

			var job = creator.CreateJob(shipment, false);

			job.ExchangeRates.AddRate(creator.USD, 1000M, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Debtor);

			GlbCompany.CurrentCompany.Factory.Save();

			var jobCharge = creator.CreateCharge(job, creator.CC1, "charge",
				creator.CurrencyWithoutCents, 100M, creator.AALSHI, "charge",
				creator.USD, 100.01M, creator.AALSHI);

			var propertyInfo = typeof(ChargeWithCost).GetProperty("Calculations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var calculationTrigger = ((CalculationTrigger)propertyInfo.GetValue(jobCharge));
			calculationTrigger.IsProcessingForTest = true;

			jobCharge.JR_RX_NKSellCurrency = creator.CurrencyWithoutCents.Code;

			Assert("IsProcessing should not change.", calculationTrigger.IsProcessingForTest);
			Assert("The decimal places of OSSellAmt should be less than or equal to the decimal places of currency.", jobCharge.JR_OSSellAmt.DecimalPlaces <= creator.CurrencyWithoutCents.Decimals);
			AssertEquals("When IsProcessing is True and SellCurrency has been change, should be recalculate OSSellAmt.", jobCharge.JR_OSSellAmt, 100010M);
		}

		public void TestCostCurrencyChange()
		{
			var creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCurrency(creator.CurrencyWithoutCents.Code);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			var shipment = creator.CreateShipment("Test1234", false);

			var job = creator.CreateJob(shipment, false);

			job.ExchangeRates.AddRate(creator.USD, 5M, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);

			GlbCompany.CurrentCompany.Factory.Save();

			var jobCharge = creator.CreateCharge(job, creator.CC1, "charge",
				creator.USD, 2.22M, creator.AALSHI, "charge",
				creator.CurrencyWithoutCents, 1M, creator.AALSHI);

			var propertyInfo = typeof(ChargeWithCost).GetProperty("Calculations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var calculationTrigger = ((CalculationTrigger)propertyInfo.GetValue(jobCharge));
			calculationTrigger.IsProcessingForTest = true;

			jobCharge.JR_RX_NKCostCurrency = creator.CurrencyWithoutCents.Code;

			Assert("IsProcessing should not change.", calculationTrigger.IsProcessingForTest);
			Assert("The decimal places of OSCostAmt should be less than or equal to the decimal places of currency.", jobCharge.JR_OSCostAmt.DecimalPlaces <= creator.CurrencyWithoutCents.Decimals);
			AssertEquals("When IsProcessing is True and CostCurrency has been change, should be recalculate OSCostAmt.", jobCharge.JR_OSCostAmt, 11M);
		}

		public void TestSuspendLocalToForeignOrForeginToLocalCOSTAmountConversionCalculations_CostAndSellCurrenciesAreSame()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			#region Foreign To Local

			charge.JR_OSCostAmt = 100m;
			AssertEquals("Local Cost Amount", 200m, charge.JR_LocalCostAmt);
			AssertEquals("OS Sell Amount", 100m, charge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount", 200m, charge.JR_LocalSellAmt);

			charge.JR_OSSellAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations())
			{
				charge.JR_OSCostAmt = 500m;

				AssertEquals("Local Cost Amount should remain unchanged", 200m, charge.JR_LocalCostAmt);
				AssertEquals("OS Sell Amount changed, as OS Cost amount has changed", 500m, charge.JR_OSSellAmt);
				AssertEquals("Local Sell Amount, as OS Sell amount has changed", 1000m, charge.JR_LocalSellAmt);
			}

			#endregion

			#region Local To Foreign

			charge.JR_OSSellAmt = 0m;
			charge.JR_LocalCostAmt = 150m;
			AssertEquals("OS Cost Amount", 75m, charge.JR_OSCostAmt);
			AssertEquals("OS Sell Amount", 75m, charge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount", 150m, charge.JR_LocalSellAmt);

			charge.JR_OSSellAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations())
			{
				charge.JR_LocalCostAmt = 500m;

				AssertEquals("OS Cost Amount should remain unchanged", 75m, charge.JR_OSCostAmt);
				AssertEquals("OS Sell Amount remains same, as OS Cost Amount has not changed", 75m, charge.JR_OSSellAmt);
				AssertEquals("Local Sell Amount remains same, as OS Sell Amount has not changed", 150m, charge.JR_LocalSellAmt);
			}

			#endregion
		}

		public void TestSuspendLocalToForeignOrForeginToLocalCOSTAmountConversionCalculations_CostAndSellCurrenciesAreNOTSame()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_RX_NKSellCurrency = creator.EUR.RX_Code;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.75m);
			charge.SellAccount.CompanyData.OB_RX_NKARDDefltCurrency = creator.EUR.RX_Code;

			#region Foreign To Local

			charge.JR_OSCostAmt = 100m;
			AssertEquals("Local Cost Amount", 133.33m, charge.JR_LocalCostAmt);
			AssertEquals("OS Sell Amount", 66.67m, charge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount", 133.34m, charge.JR_LocalSellAmt);

			charge.JR_OSSellAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations())
			{
				charge.JR_OSCostAmt = 500m;

				AssertEquals("Local Cost Amount should remain unchanged", 133.33m, charge.JR_LocalCostAmt);
				AssertEquals("OS Sell Amount should remain unchanged, as local cost amount has not changed", 66.67m, charge.JR_OSSellAmt);
				AssertEquals("Local Sell Amount, as OS sales amount has not changed", 133.34m, charge.JR_LocalSellAmt);
			}

			#endregion

			#region Local To Foreign

			charge.JR_OSSellAmt = 0m;
			charge.JR_LocalCostAmt = 150m;
			AssertEquals("OS Cost Amount", 112.5m, charge.JR_OSCostAmt);
			AssertEquals("OS Sell Amount", 75m, charge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount", 150m, charge.JR_LocalSellAmt);

			charge.JR_OSSellAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations())
			{
				charge.JR_LocalCostAmt = 500m;

				AssertEquals("OS Cost Amount should remain unchanged", 112.5m, charge.JR_OSCostAmt);
				AssertEquals("OS Sell Amount changed, as Local Cost Amount has changed", 250m, charge.JR_OSSellAmt);
				AssertEquals("Local Sell Amount changed, as OS Sell Amount has changed", 500m, charge.JR_LocalSellAmt);
			}

			#endregion
		}

		public void TestSuspendLocalToForeignOrForeginToLocalSELLAmountConversionCalculations_CostAndSellCurrenciesAreSame()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			#region Foreign To Local

			charge.JR_OSSellAmt = 100m;
			AssertEquals("Local Sell Amount", 200m, charge.JR_LocalSellAmt);
			AssertEquals("OS Cost Amount", 100m, charge.JR_OSCostAmt);
			AssertEquals("Local Cost Amount", 200m, charge.JR_LocalCostAmt);

			charge.JR_OSCostAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations())
			{
				charge.JR_OSSellAmt = 500m;

				AssertEquals("Local Sell Amount should remain unchanged", 200m, charge.JR_LocalSellAmt);
				AssertEquals("OS Cost Amount changed, as OS Sell amount has changed", 500m, charge.JR_OSCostAmt);
				AssertEquals("Local Cost Amount, as OS Sell amount has changed", 1000m, charge.JR_LocalCostAmt);
			}

			#endregion

			#region Local To Foreign

			charge.JR_OSCostAmt = 0m;
			charge.JR_LocalSellAmt = 150m;
			AssertEquals("OS Sell Amount", 75m, charge.JR_OSSellAmt);
			AssertEquals("OS Cost Amount", 75m, charge.JR_OSCostAmt);
			AssertEquals("Local Cost Amount", 150m, charge.JR_LocalCostAmt);

			charge.JR_OSCostAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations())
			{
				charge.JR_LocalSellAmt = 500m;

				AssertEquals("OS Sell Amount should remain unchanged", 75m, charge.JR_OSSellAmt);
				AssertEquals("OS Cost Amount remains same, as OS Sell Amount has not changed", 75m, charge.JR_OSCostAmt);
				AssertEquals("Local Cost Amount remains same, as OS Cost Amount has not changed", 150m, charge.JR_LocalCostAmt);
			}

			#endregion
		}

		public void TestSuspendLocalToForeignOrForeginToLocalSELLAmountConversionCalculations_CostAndSellCurrenciesAreNOTSame()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_OH_CostAccount = job.LocalCharges.PK;
			charge.CostAccount.CompanyData.OB_RX_NKAPDefltCurrency = creator.EUR.RX_Code;
			charge.SellAccount.CompanyData.OB_RX_NKARDDefltCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKCostCurrency = creator.EUR.RX_Code;
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.75m);

			#region Foreign To Local

			charge.JR_OSSellAmt = 100m;
			AssertEquals("Local Sell Amount", 133.33m, charge.JR_LocalSellAmt);
			AssertEquals("OS Cost Amount", 66.67m, charge.JR_OSCostAmt);
			AssertEquals("Local Cost Amount", 133.34m, charge.JR_LocalCostAmt);

			charge.JR_OSCostAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations())
			{
				charge.JR_OSSellAmt = 500m;

				AssertEquals("Local Sell Amount should remain unchanged", 133.33m, charge.JR_LocalSellAmt);
				AssertEquals("OS Cost Amount changed, as OS Sell amount has changed", 66.67m, charge.JR_OSCostAmt);
				AssertEquals("Local Cost Amount, as OS Sell amount has changed", 133.34m, charge.JR_LocalCostAmt);
			}

			#endregion

			#region Local To Foreign

			charge.JR_OSCostAmt = 0m;
			charge.JR_LocalSellAmt = 150m;
			AssertEquals("OS Sell Amount", 112.5m, charge.JR_OSSellAmt);
			AssertEquals("OS Cost Amount", 75m, charge.JR_OSCostAmt);
			AssertEquals("Local Cost Amount", 150m, charge.JR_LocalCostAmt);

			charge.JR_OSCostAmt = 0m;
			using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations())
			{
				charge.JR_LocalSellAmt = 500m;

				AssertEquals("OS Sell Amount should remain unchanged", 112.5m, charge.JR_OSSellAmt);
				AssertEquals("OS Cost Amount remains same, as OS Sell Amount has not changed", 250m, charge.JR_OSCostAmt);
				AssertEquals("Local Cost Amount remains same, as OS Cost Amount has not changed", 500m, charge.JR_LocalCostAmt);
			}

			#endregion
		}

		public void TestNestedSuspender_AllCalculationAndCostAmountConversionTurnedOff()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			charge.JR_OSCostAmt = 100m;
			AssertEquals("Local Cost Amount", 200m, charge.JR_LocalCostAmt);
			AssertEquals("OS Sell Amount", 100m, charge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount", 200m, charge.JR_LocalSellAmt);

			charge.JR_OSSellAmt = 0m;

			using (charge.Calculations.SuspendCalculations())
			{
				using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations())
				{
					charge.JR_OSCostAmt = 500m;

					AssertEquals("Local Cost Amount should remain unchanged", 200m, charge.JR_LocalCostAmt);
					AssertEquals("OS Sell Amount changed, as OS Cost amount has changed", 0m, charge.JR_OSSellAmt);
					AssertEquals("Local Sell Amount, as OS Sell amount has changed", 0m, charge.JR_LocalSellAmt);
				}
				AssertEquals("CostRoundingErrorReproterFunctionalitySuspender should remain suspended", true, charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
				AssertEquals("SellRoundingErrorReproterFunctionalitySuspender should remain suspended", true, charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			}

			AssertEquals("CostRoundingErrorReproterFunctionalitySuspender should not remain suspended", false, charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			AssertEquals("SellRoundingErrorReproterFunctionalitySuspender should not remain suspended", false, charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
		}

		public void TestNestedSuspender_AllCalculationAndSellAmountConversionTurnedOff()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			charge.JR_OSSellAmt = 100m;
			AssertEquals("Local Sell Amount", 200m, charge.JR_LocalSellAmt);
			AssertEquals("OS Cost Amount", 100m, charge.JR_OSCostAmt);
			AssertEquals("Local Cost Amount", 200m, charge.JR_LocalCostAmt);

			charge.JR_OSCostAmt = 0m;
			using (charge.Calculations.SuspendCalculations())
			{
				using (charge.Calculations.SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations())
				{
					charge.JR_OSSellAmt = 500m;

					AssertEquals("Local Sell Amount should remain unchanged", 200m, charge.JR_LocalSellAmt);
					AssertEquals("OS Cost Amount has not changed, as all calculation has been suspended", 0m, charge.JR_OSCostAmt);
					AssertEquals("Local Cost Amount, as all calculation has been suspended", 0m, charge.JR_LocalCostAmt);
				}

				AssertEquals("CostRoundingErrorReproterFunctionalitySuspender should remain suspended", true, charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
				AssertEquals("SellRoundingErrorReproterFunctionalitySuspender should remain suspended", true, charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			}

			AssertEquals("CostRoundingErrorReproterFunctionalitySuspender should not remain suspended", false, charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			AssertEquals("SellRoundingErrorReproterFunctionalitySuspender should not remain suspended", false, charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
		}

		public void TestSellRatedAndCostRated_ForDisbursementCharge()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			charge.JR_OSCostAmt = 100m;

			AssertEquals("Local Cost Amount", 200m, charge.JR_LocalCostAmt);
			AssertEquals("Local Sell Amount", 200m, charge.JR_LocalSellAmt);
			AssertEquals("OS Cost Amount", 100m, charge.JR_OSCostAmt);
			AssertEquals("OS Sell Amount", 100m, charge.JR_OSSellAmt);
			AssertEquals("Is Cost Rated", false, charge.JR_CostRated);
			AssertEquals("Is Sell Rated", false, charge.JR_SellRated);

			charge.JR_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
			charge.JR_OSCostAmt = 500m;

			AssertEquals("Disbursement charge - Local Cost Amount", 1000m, charge.JR_LocalCostAmt);
			AssertEquals("Disbursement charge - Local Sell Amount", 1000m, charge.JR_LocalSellAmt);
			AssertEquals("Disbursement charge - OS Cost Amount", 500m, charge.JR_OSCostAmt);
			AssertEquals("Disbursement charge - OS Sell Amount", 500m, charge.JR_OSSellAmt);
			AssertEquals("Disbursement charge - Is Cost Rated", false, charge.JR_CostRated);
			AssertEquals("Disbursement charge - Is Sell Rated", false, charge.JR_SellRated);
		}

		public void TestLocalSellAmountFromForeignAmountNoCalculation_WhenSuspendJobChargeCalculationBusinessContextIsSet()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			var charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_OH_SellAccount = job.LocalCharges.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			AssertEquals("Precondition: JR_OSSellExRate", 0.5m, charge.JR_OSSellExRate);
			AssertEquals("Precondition: JR_OSSellAmt", 0m, charge.JR_OSSellAmt);
			AssertEquals("Precondition: JR_LocalSellAmt", 0m, charge.JR_LocalSellAmt);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			charge.JR_OSSellAmt = 100m;
			AssertEquals("Local Sell Amount", 200m, charge.JR_LocalSellAmt);

			charge.JR_OSSellExRate = 1.2m;
			AssertEquals("Local Sell Amount", 83.33m, charge.JR_LocalSellAmt);

			using (charge.SetTempContext(JobInvoicingBusinessContext.SuspendJobChargeCalculationTrigger))
			{
				charge.JR_OSSellExRate = 1.2m;
				AssertEquals("Local Sell Amount should remain unchanged", 83.33m, charge.JR_LocalSellAmt);

				charge.JR_OSSellAmt = 220m;
				AssertEquals("Local Sell Amount should remain unchanged", 83.33m, charge.JR_LocalSellAmt);
			}
		}

		public void TestForeignSellAmountFromLocalAmountNoCalculation_WhenSuspendJobChargeCalculationBusinessContextIsSet()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = creator.Job1;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 0m);
			var charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC6.PK;
			charge.JR_OH_SellAccount = job.LocalCharges.PK;
			charge.JR_RX_NKSellCurrency = creator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.2m);
			AssertEquals("Precondition: JR_OSSellExRate", 1.2m, charge.JR_OSSellExRate);
			AssertEquals("Precondition: JR_OSSellAmt", 0m, charge.JR_OSSellAmt);
			AssertEquals("Precondition: JR_LocalSellAmt", 0m, charge.JR_LocalSellAmt);

			charge.JR_LocalSellAmt = 100m;
			AssertEquals("OS Sell Amount", 120m, charge.JR_OSSellAmt);

			using (charge.SetTempContext(JobInvoicingBusinessContext.SuspendJobChargeCalculationTrigger))
			{
				charge.JR_LocalSellAmt = 220m;
				AssertEquals("OS Sell Amount should remain unchanged", 120m, charge.JR_OSSellAmt);
			}
		}
	}
}
