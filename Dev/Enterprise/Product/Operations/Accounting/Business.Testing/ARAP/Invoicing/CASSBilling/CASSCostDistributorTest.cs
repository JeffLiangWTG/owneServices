using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Testing
{
	public class CASSCostDistributorTest : TestCaseWithFactory
	{
		public void TestGetCost_CostAndTaxSameSigned_NoAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine
				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 0M, 50.00M, 0M);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);

				var amounts = distributor.DistributeCostAndTax_ForTestOnly();

				AssertEquals("FRT [NO ACR]", 650.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [NO ACR]", 1250.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC2 [NO ACR]", 110.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost);
				AssertEquals("CC3 [NO ACR]", 35.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);

				AssertEquals("FRT TAX [NO ACR]", 31.7855M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC1 TAX [NO ACR]", 61.1245M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
				AssertEquals("CC2 TAX [NO ACR]", 5.3790M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax);
				AssertEquals("CC3 TAX [NO ACR]", 1.7110M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetCost_CostAndTaxSameSigned_NoAccrual_BothVATComponentHaveValue()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine
				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 1500.00M, 300.00M, 200.00M, 150.00M, 100.00M, 50.00M, 200.00M, 30M, 10M, 1.5M);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);

				var amounts = distributor.DistributeCostAndTax_ForTestOnly();

				AssertEquals("FRT [NO ACR]", 450.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [NO ACR]", 900.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC2 [NO ACR]", 150.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost);
				AssertEquals("CC3 [NO ACR]", 200.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);

				AssertEquals("FRT TAX [NO ACR]", 41.7950M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC1 TAX [NO ACR]", 92.3075M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
				AssertEquals("CC2 TAX [NO ACR]", 15.3845M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax);
				AssertEquals("CC3 TAX [NO ACR]", 20.5130M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);

				amounts = distributor.DistributeAdjustedCostAndTax_ForTestOnly();

				AssertEquals("FRT [NO ACR]", -22.5M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [NO ACR]", -45M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC2 [NO ACR]", -7.50M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost);
				AssertEquals("CC3 [NO ACR]", -10M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);

				AssertEquals("FRT TAX [NO ACR]", -2.09M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC1 TAX [NO ACR]", -4.615M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
				AssertEquals("CC2 TAX [NO ACR]", -.7690M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax);
				AssertEquals("CC3 TAX [NO ACR]", -1.0260M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetCost_CostAndTaxSameSigned_NoAccrual_NegativeDiscount()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine
				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, -25.00M, 100.00M, 0M, 50.00M, 0M);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);

				var amounts = distributor.DistributeCostAndTax_ForTestOnly();

				AssertEquals("FRT [NO ACR]", 700.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [NO ACR]", 1250.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC2 [NO ACR]", 110.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost);
				AssertEquals("CC3 [NO ACR]", 35.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);

				AssertEquals("FRT TAX [NO ACR]", 33.4125M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC1 TAX [NO ACR]", 59.6655M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
				AssertEquals("CC2 TAX [NO ACR]", 5.2510M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax);
				AssertEquals("CC3 TAX [NO ACR]", 1.6710M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetCost_CostAndTaxSameSigned_NoAccrual_NegativeDiscount_BothVATComponentHaveValue()
		{
			TestCASSChargeCodeCollection.RemoveAll();
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.CC4.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.CC5.PK);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

			//Create CASSBillingLine
			var line = new CASSBillingLine(Factory);
			CASSTestHelper.SetupExportCASSBillingLine(line, 48M, 0M, 122.08M, 0M, 2.40M, -6.40M, 33.53M, 0.46M, 0M, 0M, setupAdjustmentLine: false);

			var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);

			distributor.GetInvoiceLineInfoWithDistribution(true);

			var amounts = distributor.DistributeCostAndTax_ForTestOnly();

			AssertEquals("FRT [NO ACR]", 24M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
			AssertEquals("CC1 [NO ACR]", 24M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
			AssertEquals("CC3 [NO ACR]", 122.08M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);
			AssertEquals("CC4 [NO ACR]", -2.4M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC4.PK).Cost);
			AssertEquals("CC5 [NO ACR]", 6.4M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC5.PK).Cost);

			AssertEquals("FRT TAX [NO ACR]", 4.56M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
			AssertEquals("CC1 TAX [NO ACR]", 4.56M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
			AssertEquals("CC3 TAX [NO ACR]", 23.194M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);
			AssertEquals("CC4 TAX [NO ACR]", -0.46M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC4.PK).Tax);
			AssertEquals("CC5 TAX [NO ACR]", 1.216M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC5.PK).Tax);
		}

		public void TestGetCost_CostAndTaxSameSigned_HasAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine and associated ACR
				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 0M, 50.00M, 0M);

				var jobChargeInfo = new List<Tuple<AccChargeCode, ZDecimal, ZDecimal>>();
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.FRT, 900M, 900M));
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.CC1, 100M, 100M));
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.CC3, 15M, 15M));

				SetupAssociatedBizos(line.MAWBNumber, line.CASSCostCurrency, line.LoadPortIATA, line.DischargePortIATA, jobChargeInfo);
				line.ForceRecalculateData();
				Factory.Save();

				AssertEquals(2030M, line.SystemCostAccrualValue);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);
				var amounts = distributor.DistributeCostAndTax_ForTestOnly();

				AssertEquals("FRT [HAS ACR]", 1650M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [HAS ACR]", 250.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC2 [HAS ACR]", 110.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost);
				AssertEquals("CC3 [HAS ACR]", 35.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);

				AssertEquals("FRT TAX [HAS ACR]", 80.6851M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC1 TAX [HAS ACR]", 12.2249M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
				AssertEquals("CC2 TAX [HAS ACR]", 5.3790M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax);
				AssertEquals("CC3 TAX [HAS ACR]", 1.711M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetCost_CostAndTaxSameSigned_HasAccrual_NotAllChargeCode()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var jobChargeInfo = new List<Tuple<AccChargeCode, ZDecimal, ZDecimal>>();
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.FRT, 900M, 900M));
				SetupAssociatedBizos("17267828073", ObjectCreator.AUD, "LEJ", "MEX", jobChargeInfo);
				Factory.Save();

				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine and associated ACR

				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 0M, 50.00M, 0M);
				Factory.Save();

				AssertEquals(1800M, line.SystemCostAccrualValue);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);
				var amounts = distributor.DistributeCostAndTax_ForTestOnly();

				AssertEquals("Count", 2, amounts.Count());
				AssertEquals("FRT [HAS ACR]", 1935M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC2 [HAS ACR]", 110.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost);

				AssertEquals("FRT TAX [HAS ACR]", 94.621M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC2 TAX [HAS ACR]", 5.3790M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetCost_CostAndTaxSameSigned_HasAccrual_BothVATComponentHaveValue()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine
				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 1500.00M, 300.00M, 200.00M, 150.00M, 100.00M, 50.00M, 200.00M, 30M, 10M, 1.5M);

				var jobChargeInfo = new List<Tuple<AccChargeCode, ZDecimal, ZDecimal>>();
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.FRT, 900M, 900M));
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.CC1, 100M, 100M));
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.CC3, 15M, 15M));

				SetupAssociatedBizos(line.MAWBNumber, line.CASSCostCurrency, line.LoadPortIATA, line.DischargePortIATA, jobChargeInfo);

				line.ForceRecalculateData();

				Factory.Save();

				AssertEquals(2030M, line.SystemCostAccrualValue);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);

				var amounts = distributor.DistributeCostAndTax_ForTestOnly();

				AssertEquals("FRT [HAS ACR]", 1050.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [HAS ACR]", 450.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC3 [HAS ACR]", 200.0M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);

				AssertEquals("FRT TAX [HAS ACR]", 103.3334M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC1 TAX [HAS ACR]", 46.1536M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
				AssertEquals("CC3 TAX [HAS ACR]", 20.513M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);

				amounts = distributor.DistributeAdjustedCostAndTax_ForTestOnly();

				AssertEquals("FRT [HAS ACR]", -52.5M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [HAS ACR]", -22.5M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC3 [HAS ACR]", -10M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);

				AssertEquals("FRT TAX [HAS ACR]", -5.1668M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax);
				AssertEquals("CC1 TAX [HAS ACR]", -2.3072M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax);
				AssertEquals("CC3 TAX [HAS ACR]", -1.026M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetCost_CostAndTaxOppositeSigned_NoAccrual()
		{
			TestCASSChargeCodeCollection.RemoveAll();
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

			//Create CASSBillingLine
			var line = new CASSBillingLine(Factory);
			CASSTestHelper.SetupExportCASSBillingLine(line, 2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 200M, 50.00M, 100M);

			var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);
			var amountCosts = distributor.DistributeCostsWhenCostAndTaxOppositeSigned_ForTestOnly(1, -1);
			var amountCostsRev = distributor.DistributeTaxWhenCostAndTaxOppositeSigned_ForTestOnly();

			var sumOfDistributedCASSCostAmount = ZDecimal.Zero;
			var sumOfDistributedTaxAmount = ZDecimal.Zero;

			var amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost;
			var amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost;
			AssertEquals("FRT [NO ACR]", 650.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost;
			AssertEquals("CC1 [NO ACR]", 1250.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost;
			AssertEquals("CC2 [NO ACR]", 110.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost;
			AssertEquals("CC3 [NO ACR]", 35.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax;
			AssertEquals("FRT TAX [NO ACR]", -153.2440M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax;
			AssertEquals("CC1 TAX [NO ACR]", 47.7100M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax;
			AssertEquals("CC2 TAX [NO ACR]", 4.1980M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax;
			AssertEquals("CC3 TAX [NO ACR]", 1.3360M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			AssertEquals("CASS Cost Amount", line.CASSCostValue, sumOfDistributedCASSCostAmount);
			AssertEquals("CASS TAX Amount", line.CASSCostTaxValue, sumOfDistributedTaxAmount);
		}

		public void TestGetCost_CostAndTaxOppositeSigned_NoAccrual_NegativeDiscount()
		{
			TestCASSChargeCodeCollection.RemoveAll();
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

			//Create CASSBillingLine
			var line = new CASSBillingLine(Factory);
			CASSTestHelper.SetupExportCASSBillingLine(line, 2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, -25.00M, 100.00M, 200M, 50.00M, 100M);

			var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);
			var amountCosts = distributor.DistributeCostsWhenCostAndTaxOppositeSigned_ForTestOnly(1, -1);
			var amountCostsRev = distributor.DistributeTaxWhenCostAndTaxOppositeSigned_ForTestOnly();

			var sumOfDistributedCASSCostAmount = ZDecimal.Zero;
			var sumOfDistributedTaxAmount = ZDecimal.Zero;

			var amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost;
			var amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost;
			AssertEquals("FRT [NO ACR]", 700.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost;
			AssertEquals("CC1 [NO ACR]", 1250.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost;
			AssertEquals("CC2 [NO ACR]", 110.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost;
			AssertEquals("CC3 [NO ACR]", 35.0M, amount1 + amount2);
			sumOfDistributedCASSCostAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax;
			AssertEquals("FRT TAX [NO ACR]", -152.2475M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax;
			AssertEquals("CC1 TAX [NO ACR]", 46.8165M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax;
			AssertEquals("CC2 TAX [NO ACR]", 4.12M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax;
			amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax;
			AssertEquals("CC3 TAX [NO ACR]", 1.3110M, amount1 + amount2);
			sumOfDistributedTaxAmount += (amount1 + amount2);

			AssertEquals("CASS Cost Amount", line.CASSCostValue, sumOfDistributedCASSCostAmount);
			AssertEquals("CASS TAX Amount", line.CASSCostTaxValue, sumOfDistributedTaxAmount);
		}

		public void TestGetCost_CostAndTaxOppositeSigned_HasAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine and associated ACR
				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 200M, 50.00M, 100M);

				var jobChargeInfo = new List<Tuple<AccChargeCode, ZDecimal, ZDecimal>>();
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.FRT, 900M, 900M));
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.CC1, 100M, 100M));
				jobChargeInfo.Add(Tuple.Create<AccChargeCode, ZDecimal, ZDecimal>(ObjectCreator.CC3, 15M, 15M));

				SetupAssociatedBizos(line.MAWBNumber, line.CASSCostCurrency, line.LoadPortIATA, line.DischargePortIATA, jobChargeInfo);
				line.ForceRecalculateData();
				Factory.Save();

				AssertEquals(2030M, line.SystemCostAccrualValue);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);

				var amountCosts = distributor.DistributeCostsWhenCostAndTaxOppositeSigned_ForTestOnly(1, -1);
				var amountCostsRev = distributor.DistributeTaxWhenCostAndTaxOppositeSigned_ForTestOnly();

				var sumOfDistributedCASSCostAmount = ZDecimal.Zero;
				var sumOfDistributedTaxAmount = ZDecimal.Zero;

				var amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost;
				var amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost;
				AssertEquals("FRT [HAS ACR]", 1650M, amount1 + amount2);
				sumOfDistributedCASSCostAmount += (amount1 + amount2);

				amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost;
				amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost;
				AssertEquals("CC1 [HAS ACR]", 250.0M, amount1 + amount2);
				sumOfDistributedCASSCostAmount += (amount1 + amount2);

				amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost;
				amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost;
				AssertEquals("CC2 [HAS ACR]", 110.0M, amount1 + amount2);
				sumOfDistributedCASSCostAmount += (amount1 + amount2);

				amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost;
				amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost;
				AssertEquals("CC3 [HAS ACR]", 35.0M, amount1 + amount2);
				sumOfDistributedCASSCostAmount += (amount1 + amount2);

				amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax;
				amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Tax;
				AssertEquals("FRT TAX [HAS ACR]", -115.0760M, amount1 + amount2);
				sumOfDistributedTaxAmount += (amount1 + amount2);

				amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax;
				amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Tax;
				AssertEquals("CC1 TAX [HAS ACR]", 9.5420M, amount1 + amount2);
				sumOfDistributedTaxAmount += (amount1 + amount2);

				amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax;
				amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Tax;
				AssertEquals("CC2 TAX [HAS ACR]", 4.1980M, amount1 + amount2);
				sumOfDistributedTaxAmount += (amount1 + amount2);

				amount1 = amountCosts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax;
				amount2 = amountCostsRev.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Tax;
				AssertEquals("CC3 TAX [HAS ACR]", 1.336M, amount1 + amount2);
				sumOfDistributedTaxAmount += (amount1 + amount2);

				AssertEquals("CASS Cost Amount", line.CASSCostValue, sumOfDistributedCASSCostAmount);
				AssertEquals("CASS TAX Amount", line.CASSCostTaxValue, sumOfDistributedTaxAmount);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestDistributedAmountWithTooSmallAmount()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				//Setup Registry

				TestCASSChargeCodeCollection.RemoveAll();
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC4.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC5.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC6.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC7.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC8.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC9.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC10.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, ObjectCreator.CC11.PK);

				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, ObjectCreator.CC4.PK);

				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, ObjectCreator.CC4.PK);

				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, ObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, ObjectCreator.FRT.PK);

				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCASSChargeCodeCollection);

				//Create CASSBillingLine
				var line = new CASSBillingLine(Factory);
				CASSTestHelper.SetupExportCASSBillingLine(line, 0.026M, 0.024M, 0.03M, 0M, 0.0M, 0.00M, 0M, 0M, 0M, 0M, false);

				var distributor = new CASSCostDistributor(line, GSTTaxRate, ObjectCreator.FREECAPGST);

				var amounts = distributor.DistributeCostAndTax_ForTestOnly();

				AssertEquals("Count", 5, amounts.Count());
				AssertEquals("FRT [NO ACR]", 0.01M, amounts.First(x => x.ChargeCodePK == ObjectCreator.FRT.PK).Cost);
				AssertEquals("CC1 [NO ACR]", 0.01M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC1.PK).Cost);
				AssertEquals("CC2 [NO ACR]", 0.02M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC2.PK).Cost);
				AssertEquals("CC3 [NO ACR]", 0.025M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC3.PK).Cost);
				AssertEquals("CC4 [NO ACR]", 0.015M, amounts.First(x => x.ChargeCodePK == ObjectCreator.CC4.PK).Cost);
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		void SetupAssociatedBizos(ZString mAWBNumber, RefCurrency cASSCostCurrency, ZString loadPortIATA, ZString dischargePortIATA, List<Tuple<AccChargeCode, ZDecimal, ZDecimal>> jobChargeInfo)
		{
			string origin = RefUNLOCO.LoadFromIATA(Factory, loadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, dischargePortIATA).RL_Code;

			var consol = ObjectCreator.CreateConsol(origin, destination, "C0001");
			consol.JK_MasterBillNum = mAWBNumber;

			var shipment1 = ObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
			var shipment2 = ObjectCreator.CreateShipment("SHIP2", origin, destination, consol);
			var job1 = ObjectCreator.CreateJob(shipment1, false);
			var job2 = ObjectCreator.CreateJob(shipment2, false);

			foreach (Tuple<AccChargeCode, ZDecimal, ZDecimal> infoItem in jobChargeInfo)
			{
				ObjectCreator.CreateCharge(job1, infoItem.Item1, infoItem.Item1.AC_Desc, cASSCostCurrency, infoItem.Item2, null, cASSCostCurrency, infoItem.Item3, null);
				ObjectCreator.CreateCharge(job2, infoItem.Item1, infoItem.Item1.AC_Desc, cASSCostCurrency, infoItem.Item2, null, cASSCostCurrency, infoItem.Item3, null);
			}

			ObjectCreator.SetExchangeRate(job1, cASSCostCurrency, 1M);
			ObjectCreator.SetExchangeRate(job2, cASSCostCurrency, 1M);
			ObjectCreator.CreateTestPeriods(ZDateTime.Today);

			ObjectCreator.GLHeader1.AG_AccountType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;
			ObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
		}

		void AddCASSChargeCodeLine(ZString cASSType, ZString component, ZGuid chargeCodePK)
		{
			var cassChargeCode = TestCASSChargeCodeCollection.AddNew();
			cassChargeCode.CASSType = cASSType;
			cassChargeCode.ChargeCodePK = chargeCodePK;
			cassChargeCode.CASSComponentCode = component;
		}

		CASSChargeCodeCollection TestCASSChargeCodeCollection
		{
			get
			{
				if (testCASSChargeCodeCollection == null)
				{
					testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				}
				return testCASSChargeCodeCollection;
			}
		}

		CASSChargeCodeCollection testCASSChargeCodeCollection;

		public AccTaxRate GSTTaxRate
		{
			get { return objectCreator.GST1; }
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (objectCreator == null)
				{
					objectCreator = new TestObjectCreator(Factory);
				}
				return objectCreator;
			}
		}
		TestObjectCreator objectCreator;
	}
}