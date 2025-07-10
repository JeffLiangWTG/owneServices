using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CASSCostExportLine))]
	public class CASSCostExportLineTest : CASSCostLineTest
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesCASSCostExportLine()
		{
			var line = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);

			var currencyList = new List<string>
			{
				nameof(line.WeightChargePP),
				nameof(line.ValuationChargePP),
				nameof(line.ChargesDueCarrierPP),
				nameof(line.ChargesDueAgentCC),
				nameof(line.Commission),
				nameof(line.Discount),
				nameof(line.VATDueAirline),
				nameof(line.VATDueAgent)
			};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckNonLocalCurrency(currencyList, nameof(line.CurrencyISODecimalPlaces), nameof(line.CurrencyCode), line);
		}

		public override void TestVATAmount()
		{
			SetAmountFields(25);
			var cassExpLine = CASSDataForTest as CASSCostExportLine;
			cassExpLine.VATDueAirline = 5m;
			cassExpLine.VATDueAgent = 2m;
			AssertEquals("VATAmount", 3m, cassExpLine.VATAmount);
		}

		public override void TestAdjustedCASSCost()
		{
			var cassExpLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			cassExpLine.RecordType = CASSHOTFileFormat.RecortID.DCO;
			cassExpLine.CurrencyCode = "AUD";
			cassExpLine.WeightChargePP = 20;
			cassExpLine.ValuationChargePP = 30;
			cassExpLine.ChargesDueCarrierPP = 25;
			cassExpLine.ChargesDueAgentCC = 15;
			cassExpLine.Commission = 1;
			cassExpLine.Discount = 0.4;

			AssertEquals("WeightChargePP", 20m, cassExpLine.WeightChargePP);
			AssertEquals("ValuationChargePP", 30m, cassExpLine.ValuationChargePP);
			AssertEquals("ChargesDueCarrierPP", 25m, cassExpLine.ChargesDueCarrierPP);
			AssertEquals("ChargesDueAgentCC", 15m, cassExpLine.ChargesDueAgentCC);
			AssertEquals("Commission", 1m, cassExpLine.Commission);
			AssertEquals("Discount", 0.4m, cassExpLine.Discount);

			AssertEquals("AdjustedCASSCost", 58.6m, cassExpLine.CASSCost);
		}

		public override void TestUpdateOriginalAmounts()
		{
			var cassExpLine = CASSDataForTest as CASSCostExportLine;
			SetAmountFields(25);
			AssertEquals("HasChanges", true, CASSDataForTest.HasChanges);
			AssertEquals("WeightChargePP", 0m, cassExpLine.WeightChargePP_Original);
			AssertEquals("ValuationChargePP", 0m, cassExpLine.ValuationChargePP_Original);
			AssertEquals("ChargesDueCarrierPP", 0m, cassExpLine.ChargesDueCarrierPP_Original);
			AssertEquals("ChargesDueAgentCC", 0m, cassExpLine.ChargesDueAgentCC_Original);
			AssertEquals("Commission", 0m, cassExpLine.Commission_Original);
			AssertEquals("Discount", 0m, cassExpLine.Discount_Original);

			cassExpLine.UpdateOriginalAmounts();

			AssertEquals("WeightChargePP", cassExpLine.WeightChargePP, cassExpLine.WeightChargePP_Original);
			AssertEquals("ValuationChargePP", cassExpLine.ValuationChargePP, cassExpLine.ValuationChargePP_Original);
			AssertEquals("ChargesDueCarrierPP", cassExpLine.ChargesDueCarrierPP, cassExpLine.ChargesDueCarrierPP_Original);
			AssertEquals("ChargesDueAgentCC", cassExpLine.ChargesDueAgentCC, cassExpLine.ChargesDueAgentCC_Original);
			AssertEquals("Commission", cassExpLine.Commission, cassExpLine.Commission_Original);
			AssertEquals("Discount", cassExpLine.Discount, cassExpLine.Discount_Original);
		}

		public override void TestReadonlyProperties()
		{
			Env.Security.APCASSCostFileModification.IsAllowed = true;

			base.AssertCommonReadonlyProperties();
			Assert("WeightChargePP", !(CASSDataForTest as CASSCostExportLine).WeightChargePPInfo.ReadOnly);
			Assert("ValuationChargePP", !(CASSDataForTest as CASSCostExportLine).ValuationChargePPInfo.ReadOnly);
			Assert("ChargesDueCarrierPP", !(CASSDataForTest as CASSCostExportLine).ChargesDueCarrierPPInfo.ReadOnly);
			Assert("ChargesDueAgentCC", !(CASSDataForTest as CASSCostExportLine).ChargesDueAgentCCInfo.ReadOnly);
			Assert("Commission", !(CASSDataForTest as CASSCostExportLine).CommissionInfo.ReadOnly);
			Assert("Discount", !(CASSDataForTest as CASSCostExportLine).DiscountInfo.ReadOnly);
			Assert("Commission", !(CASSDataForTest as CASSCostExportLine).CommissionInfo.ReadOnly);
			Assert("AdjustmentReasonType", !(CASSDataForTest as CASSCostExportLine).AdjustmentReasonTypeInfo.ReadOnly);
			Assert("AdjustmentReason", !(CASSDataForTest as CASSCostExportLine).AdjustmentReasonInfo.ReadOnly);
			Assert("AdjustmentReasonComment", !(CASSDataForTest as CASSCostExportLine).AdjustmentReasonCommentInfo.ReadOnly);
			Assert("VATDueAirline", (CASSDataForTest as CASSCostExportLine).VATDueAirlineInfo.ReadOnly);
			Assert("VATDueAgent", (CASSDataForTest as CASSCostExportLine).VATDueAgentInfo.ReadOnly);

			Env.Security.APCASSCostFileModification.IsAllowed = false;
			Assert("WeightChargePP", (CASSDataForTest as CASSCostExportLine).WeightChargePPInfo.ReadOnly);
			Assert("ValuationChargePP", (CASSDataForTest as CASSCostExportLine).ValuationChargePPInfo.ReadOnly);
			Assert("ChargesDueCarrierPP", (CASSDataForTest as CASSCostExportLine).ChargesDueCarrierPPInfo.ReadOnly);
			Assert("ChargesDueAgentCC", (CASSDataForTest as CASSCostExportLine).ChargesDueAgentCCInfo.ReadOnly);
			Assert("Commission", (CASSDataForTest as CASSCostExportLine).CommissionInfo.ReadOnly);
			Assert("Discount", (CASSDataForTest as CASSCostExportLine).DiscountInfo.ReadOnly);
			Assert("Commission", (CASSDataForTest as CASSCostExportLine).CommissionInfo.ReadOnly);
			Assert("AdjustmentReasonType", (CASSDataForTest as CASSCostExportLine).AdjustmentReasonTypeInfo.ReadOnly);
			Assert("AdjustmentReason", (CASSDataForTest as CASSCostExportLine).AdjustmentReasonInfo.ReadOnly);
			Assert("AdjustmentReasonComment", (CASSDataForTest as CASSCostExportLine).AdjustmentReasonCommentInfo.ReadOnly);
		}

		public void TestValidateReasonCode()
		{
			SetAmountFields(25);
			var cassExpLine = CASSDataForTest as CASSCostExportLine;
			cassExpLine.UpdateOriginalAmounts();
			AssertNoErrors("should not have any Error", cassExpLine.AdjustmentReasonInfo);

			cassExpLine.WeightChargePP = 250m;
			cassExpLine.AdjustmentReason = "49";
			AssertHasErrors("should have Error", cassExpLine.AdjustmentReasonInfo);

			cassExpLine.AdjustmentReasonType = cassExpLine.AdjustmentReasonTypeList[0].Code;
			cassExpLine.AdjustmentReason = cassExpLine.AdjustmentReasonList[0].Code;
			AssertNoErrors("should not have any Error", cassExpLine.AdjustmentReasonInfo);
		}

		public void TestValidateAdjustmentComment()
		{
			SetAmountFields(25);
			var cassExpLine = CASSDataForTest as CASSCostExportLine;
			cassExpLine.UpdateOriginalAmounts();

			cassExpLine.WeightChargePP = 250m;
			cassExpLine.AdjustmentReasonComment = "Test Comment";
			AssertNoErrors("Should not have Error", cassExpLine.AdjustmentReasonCommentInfo);

			cassExpLine.AdjustmentReasonComment = "";
			AssertHasErrors("Should have Error", cassExpLine.AdjustmentReasonCommentInfo);

			cassExpLine.AdjustmentReasonComment = "Test Comment 2";
			AssertNoErrors("Should not have Error", cassExpLine.AdjustmentReasonCommentInfo);
		}

		public void TestRunPreSaveValidation()
		{
			SetAmountFields(25);
			var cassExpLine = CASSDataForTest as CASSCostExportLine;
			cassExpLine.UpdateOriginalAmounts();
			AssertNoErrors("should not have any Error", cassExpLine.AdjustmentReasonInfo);
			AssertNoErrors("should not have any Error", cassExpLine.AdjustmentReasonCommentInfo);

			cassExpLine.WeightChargePP = 250m;
			cassExpLine.RunPreSaveValidation();
			AssertHasErrors("should have Error", cassExpLine.AdjustmentReasonInfo);
			AssertHasErrors("should have Error", cassExpLine.AdjustmentReasonCommentInfo);

			cassExpLine.AdjustmentReasonType = cassExpLine.AdjustmentReasonTypeList[0].Code;
			cassExpLine.AdjustmentReason = cassExpLine.AdjustmentReasonList[0].Code;
			cassExpLine.AdjustmentReasonComment = "Test Comment";
			AssertNoErrors("should not have any Error", cassExpLine.AdjustmentReasonInfo);
			AssertNoErrors("should not have any Error", cassExpLine.AdjustmentReasonCommentInfo);
		}

		public void TestGetCompoentListByVATComponent()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.AWM, false, 2000, 1000, 500, 500, 75, 250, 370.50, 57.50M);

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.AWM, false, 2000, 1000, 500, 500, 75, -250, 370.50, 57.50M);

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());
		}

		public void TestApportionedVAT_AWM_PositiveDiscount()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.AWM, false, 2000, 1000, 500, 500, 75, 250, 370.50, 57.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", 228.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", 114.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", 57.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", -28.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", -50.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", -7.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_AWM_NegativeDiscount()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.AWM, false, 2000, 1000, 500, 500, 75, -250, 370.50, 57.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", 197.600M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", 98.800M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", 49.400M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", 24.700M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", -50.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", -7.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_AWM_NegativeDiscount_NegativeVAT()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.AWM, false, 2000, 1000, 500, 500, 75, -250, 75M, 287.5M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", 40M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", 20M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", 10M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", 5M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", -250.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", -37.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_DCR_NegativeDiscount()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCR, false, 2000, 1000, 500, 500, 75, -250, 370.50, 57.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", 197.600M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", 98.800M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", 49.400M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", 24.700M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", -50.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", -7.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_DCR_PositiveDiscount()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCR, false, 2000, 1000, 500, 500, 75, 250, 370.50, 57.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", 228.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", 114.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", 57.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", -28.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", -50.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", -7.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_DCR_NegativeDiscount_NegativeVAT()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCR, false, 2000, 1000, 500, 500, 75, -250, 75M, 287.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", 40M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", 20M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", 10M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", 5M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", -250.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", -37.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(false, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_DCO_NegativeDiscount()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCO, true, -2000, -1000, -500, -500, -75, -250, 370.50, 57.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", -228.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", -114.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", -57.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", 28.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", 50.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", 7.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_DCO_PositiveDiscount()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCO, true, -2000, -1000, -500, -500, -75, 250, 370.50, 57.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", -197.600M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", -98.800M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", -49.400M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", -24.700M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", 50.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", 7.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_DCO_PositiveDiscount_PositiveVAT()
		{
			var cassExportLine = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCO, true, -2000, -1000, -500, -500, -75, 250, 75M, 287.50M);

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", -40M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", -20M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", -10M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", -5M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);
			AssertEquals("Sign of VATDueAirline amount apportioned to PWC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PVC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to PCC", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]));
			AssertEquals("Sign of VATDueAirline amount apportioned to DOI", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.Discount.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]));

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", 250.000M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", 37.500M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
			AssertEquals("Sign of VATDueAgent amount apportioned to COA", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]));
			AssertEquals("Sign of VATDueAgent amount apportioned to COM", GetSignAsString(cassExportLine.GetComponentAmount(true, CASSChargeCodeLookups.CASSComponents.Commission.Code)), GetSignAsString(vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]));
		}

		public void TestApportionedVAT_Combined()
		{
			var cassExportLineDCO = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCO, true, -500, -250, -175, -175, -18, -62, 92, 14);
			var cassExportLineDCR = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.DCR, false, 500, 250, 175, 175, 18, 62, 92, 14);
			var cassExportLineAWM = CASSTestHelper.CreateExportCASSCostLine(CASSHOTFileFormat.RecortID.AWM, false, 2000, 1000, 500, 500, 75, 250, 370.50, 57.50M);

			var cassBillingline = new CASSBillingLine(Factory);
			cassBillingline.AddCostLine(cassExportLineAWM, cassExportLineAWM.CurrencyCode);
			cassBillingline.AddCostLine(cassExportLineDCR, cassExportLineDCR.CurrencyCode);
			cassBillingline.AddCostLine(cassExportLineDCO, cassExportLineDCO.CurrencyCode);

			var cassExportLine = cassBillingline.AggregatedCostLine as CASSCostExportLine;

			var vatComponents = cassExportLine.VATComponentList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSCostExportLine.VATAgent, CASSCostExportLine.VATAirline }, vatComponents.GetAllCodes());

			var costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAirline);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			costComponents = cassExportLine.GetCostComponentListByVATComponent(CASSCostExportLine.VATAgent);
			AssertContainsExactElementsInAnyOrder(new ZString[] { CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code }, costComponents.GetAllCodes());

			var vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", 281.303M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", 140.651M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", 75.656M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", -35.110M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(false, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", -62.694M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", -8.806M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAirline);
			AssertEquals("VATDueAirline apportioned to PWC", -53.303M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PVC", -26.651M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code]);
			AssertEquals("VATDueAirline apportioned to PCC", -18.656M, vatDistribution[CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code]);
			AssertEquals("VATDueAirline apportioned to DOI", 6.610M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Discount.Code]);

			vatDistribution = cassExportLine.GetVATApportionedToCostComponents(true, CASSCostExportLine.VATAgent);
			AssertEquals("VATDueAgent apportioned to COA", 12.694M, vatDistribution[CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code]);
			AssertEquals("VATDueAgent apportioned to COM", 1.306M, vatDistribution[CASSChargeCodeLookups.CASSComponents.Commission.Code]);
		}

		protected override CASSData GetCASSData()
		{
			return new CASSCostExportLine(Factory, CASSCostLineType.Billing);
		}

		protected override void SetAmountFields(decimal seedValue)
		{
			var cassExpLine = CASSDataForTest as CASSCostExportLine;
			cassExpLine.RecordType = CASSHOTFileFormat.RecortID.AWM;
			cassExpLine.CurrencyCode = "AUD";
			cassExpLine.WeightChargePP = seedValue;
			cassExpLine.ValuationChargePP = seedValue + 10;
			cassExpLine.ChargesDueCarrierPP = seedValue + 5;
			cassExpLine.ChargesDueAgentCC = seedValue - 5;
			cassExpLine.Commission = seedValue * 0.05m;
			cassExpLine.Discount = seedValue * 0.02m;
		}

		string GetSignAsString(ZDecimal val)
		{
			return Math.Sign(val) < 0 ? "-" : "+";
		}
	}
}
