using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CASSCostImportLine))]
	public class CASSCostImportLineTest : CASSCostLineTest
	{
		public override void TestVATAmount()
		{
			AssertEquals("VATAmount", 0m, (CASSDataForTest as CASSCostImportLine).VATAmount);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesCASSCostImportLine()
		{
			var line = CASSDataForTest as CASSCostImportLine;

			var currencyList = new List<string>
			{
				nameof(line.WeightCharges),
				nameof(line.ChargesDueAgentCC),
				nameof(line.ChargesDueCarrierCC),
				nameof(line.FeeAmount),
				nameof(line.HandlingCharges),
				nameof(line.StorageCharges),
				nameof(line.OtherCharge1Amount),
				nameof(line.OtherCharge2Amount),
				nameof(line.MiscellaneousChargesAmount)
			};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckNonLocalCurrency(currencyList, nameof(line.CurrencyISODecimalPlaces), nameof(line.CurrencyCode), line);
		}

		public override void TestAdjustedCASSCost()
		{
			SetAmountFields(20m);

			var cassImpLine = CASSDataForTest as CASSCostImportLine;
			cassImpLine.RecordType = CASSHOTFileFormat.RecortID.IBO;
			AssertEquals("WeightCharges", 20m, cassImpLine.WeightCharges);
			AssertEquals("ChargesDueAgentCC", 35m, cassImpLine.ChargesDueAgentCC);
			AssertEquals("ChargesDueCarrierCC", 30m, cassImpLine.ChargesDueCarrierCC);
			AssertEquals("FeeCharged", true, cassImpLine.FeeCharged);
			AssertEquals("FeeAmount", 1m, cassImpLine.FeeAmount);
			AssertEquals("HandlingCharges", 2m, cassImpLine.HandlingCharges);
			AssertEquals("StorageCharges", 25m, cassImpLine.StorageCharges);
			AssertEquals("OtherCharge1Amount", 22m, cassImpLine.OtherCharge1Amount);
			AssertEquals("OtherCharge2Amount", 20m, cassImpLine.OtherCharge2Amount);
			AssertEquals("MiscellaneousChargesAmount", 10m, cassImpLine.MiscellaneousChargesAmount);

			AssertEquals("AdjustedCASSCost", 95m, cassImpLine.CASSCost);
		}

		public override void TestUpdateOriginalAmounts()
		{
			var cassImpLine = CASSDataForTest as CASSCostImportLine;

			SetAmountFields(25);
			AssertEquals("HasChanges", true, CASSDataForTest.HasChanges);
			AssertEquals("WeightChargePP", 0m, cassImpLine.WeightCharges_Original);
			AssertEquals("ValuationChargePP", 0m, cassImpLine.ChargesDueAgentCC_Original);
			AssertEquals("ChargesDueCarrierPP", 0m, cassImpLine.ChargesDueCarrierCC_Original);
			AssertEquals("ChargesDueAgentCC", false, cassImpLine.FeeCharged_Original);
			AssertEquals("FeeAmount", 0m, cassImpLine.FeeAmount_Original);
			AssertEquals("HandlingCharges", 0m, cassImpLine.HandlingCharges_Original);
			AssertEquals("StorageCharges", 0m, cassImpLine.StorageCharges_Original);
			AssertEquals("OtherCharge1Amount", 0m, cassImpLine.OtherCharge1Amount_Original);
			AssertEquals("OtherCharge2Amount", 0m, cassImpLine.OtherCharge2Amount_Original);
			AssertEquals("MiscellaneousChargesAmount", 0m, cassImpLine.MiscellaneousChargesAmount_Original);

			cassImpLine.UpdateOriginalAmounts();

			AssertEquals("HasChanges", false, CASSDataForTest.HasChanges);
			AssertEquals("WeightChargePP", cassImpLine.WeightCharges, cassImpLine.WeightCharges_Original);
			AssertEquals("ValuationChargePP", cassImpLine.ChargesDueAgentCC, cassImpLine.ChargesDueAgentCC_Original);
			AssertEquals("ChargesDueCarrierPP", cassImpLine.ChargesDueCarrierCC, cassImpLine.ChargesDueCarrierCC_Original);
			AssertEquals("ChargesDueAgentCC", cassImpLine.FeeCharged, cassImpLine.FeeCharged_Original);
			AssertEquals("FeeAmount", cassImpLine.FeeAmount, cassImpLine.FeeAmount_Original);
			AssertEquals("HandlingCharges", cassImpLine.HandlingCharges, cassImpLine.HandlingCharges_Original);
			AssertEquals("StorageCharges", cassImpLine.StorageCharges, cassImpLine.StorageCharges_Original);
			AssertEquals("OtherCharge1Amount", cassImpLine.OtherCharge1Amount, cassImpLine.OtherCharge1Amount_Original);
			AssertEquals("OtherCharge2Amount", cassImpLine.OtherCharge2Amount, cassImpLine.OtherCharge2Amount_Original);
			AssertEquals("MiscellaneousChargesAmount", cassImpLine.MiscellaneousChargesAmount, cassImpLine.MiscellaneousChargesAmount_Original);
		}

		public override void TestReadonlyProperties()
		{
			var cassImpLine = CASSDataForTest as CASSCostImportLine;

			AssertCommonReadonlyProperties();

			Env.Security.APCASSCostFileModification.IsAllowed = true;
			AssertEquals("WeightChargePP", false, cassImpLine.WeightChargesInfo.ReadOnly);
			AssertEquals("ValuationChargePP", false, cassImpLine.ChargesDueAgentCCInfo.ReadOnly);
			AssertEquals("ChargesDueCarrierPP", false, cassImpLine.ChargesDueCarrierCCInfo.ReadOnly);
			AssertEquals("ChargesDueAgentCC", false, cassImpLine.FeeChargedInfo.ReadOnly);
			AssertEquals("FeeAmount", false, cassImpLine.FeeAmountInfo.ReadOnly);
			AssertEquals("HandlingCharges", false, cassImpLine.HandlingChargesInfo.ReadOnly);
			AssertEquals("StorageCharges", false, cassImpLine.StorageChargesInfo.ReadOnly);
			AssertEquals("OtherCharge1Amount", false, cassImpLine.OtherCharge1AmountInfo.ReadOnly);
			AssertEquals("OtherCharge2Amount", false, cassImpLine.OtherCharge2AmountInfo.ReadOnly);
			AssertEquals("MiscellaneousChargesAmount", false, cassImpLine.MiscellaneousChargesAmountInfo.ReadOnly);

			Env.Security.APCASSCostFileModification.IsAllowed = false;
			AssertEquals("WeightChargePP", true, cassImpLine.WeightChargesInfo.ReadOnly);
			AssertEquals("ValuationChargePP", true, cassImpLine.ChargesDueAgentCCInfo.ReadOnly);
			AssertEquals("ChargesDueCarrierPP", true, cassImpLine.ChargesDueCarrierCCInfo.ReadOnly);
			AssertEquals("ChargesDueAgentCC", true, cassImpLine.FeeChargedInfo.ReadOnly);
			AssertEquals("FeeAmount", true, cassImpLine.FeeAmountInfo.ReadOnly);
			AssertEquals("HandlingCharges", true, cassImpLine.HandlingChargesInfo.ReadOnly);
			AssertEquals("StorageCharges", true, cassImpLine.StorageChargesInfo.ReadOnly);
			AssertEquals("OtherCharge1Amount", true, cassImpLine.OtherCharge1AmountInfo.ReadOnly);
			AssertEquals("OtherCharge2Amount", true, cassImpLine.OtherCharge2AmountInfo.ReadOnly);
			AssertEquals("MiscellaneousChargesAmount", true, cassImpLine.MiscellaneousChargesAmountInfo.ReadOnly);
		}

		protected override CASSData GetCASSData()
		{
			return new CASSCostImportLine(Factory, CASSCostLineType.Default);
		}

		protected override void SetAmountFields(decimal seedValue)
		{
			var cassImpLine = CASSDataForTest as CASSCostImportLine;
			cassImpLine.CurrencyCode = "AUD";
			cassImpLine.WeightCharges = seedValue;
			cassImpLine.ChargesDueAgentCC = seedValue + 15m;
			cassImpLine.ChargesDueCarrierCC = seedValue + 10m;
			cassImpLine.FeeCharged = true;
			cassImpLine.FeeAmount = seedValue * 0.05m;
			cassImpLine.HandlingCharges = seedValue * 0.1m;
			cassImpLine.StorageCharges = seedValue + 5m;
			cassImpLine.OtherCharge1Amount = seedValue + 2m;
			cassImpLine.OtherCharge2Amount = seedValue;
			cassImpLine.MiscellaneousChargesAmount = seedValue * 0.5m;
		}
	}
}
