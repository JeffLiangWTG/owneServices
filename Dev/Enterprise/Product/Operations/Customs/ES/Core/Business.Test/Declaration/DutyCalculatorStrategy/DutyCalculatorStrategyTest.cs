using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee;
using DutyCalculatorStrategy = Enterprise.Customs.ES.Business.Declaration.DutyCalculatorStrategy;
using JobComInvoiceLine = Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	sealed class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		protected override ZString VATableAdditionChargeCode => UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge;
		protected override ZString NonVATableDeductionChargeCode => ZString.Empty;

		protected override ZInt ExpectedChargeAmountDecimalPlaces => 2;

		void AssertEntryLineFees(CusEntryLine entryLine, params FeeAssertionObject[] expectedFees) => LineMergerTestHelper.AssertEntryLineFees(entryLine, expectedFees, ExpectedChargeAmountDecimalPlaces);

		public void TestCalculateRetailerFees()
		{
			SetUpTariffAndAIEMReferenceData(TariffCode, ExciseCode, true);

			var importer = Factory.New<OrgHeader>();
			var addInfoCusImp = ESOrgImpAddInfo.Get(importer);
			addInfoCusImp.ZO_Retailer = true;

			var declaration = SetUpDeclaration(TariffCode);
			declaration.ZG_DestinationState = "1";
			declaration.JE_OH_Importer = importer.PK;

			CombineAssertions(() =>
			{
				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var fee = entryLine.Fees.AddNew();
				fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
				fee.G4_BaseAmount = 30m;
				invoiceLine.JI_ZZF_NKTaxType = "XXX";
				declaration.DoMerge();
				AssertEquals("B01 Fee not expected when invalid excise code", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));
				AssertEquals("1PL Fee not expected when ZG_HasNonRecycledPlastics is false", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.NonRecycledPlasticFee));

				invoiceLine.JI_ZZF_NKTaxType = IV1VatType;
				declaration.DoMerge();
				AssertEquals("B01 Fee expected when valid excise code", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));
				AssertEquals("3RM Fee not expected for in land", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));

				var feeCI = entryLine.Fees.AddNew();
				feeCI.G4_Type = "3IG";
				feeCI.G4_BaseAmount = 30m;
				declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
				invoiceLine.ZG_AIEMType = "";
				declaration.DoMerge();
				AssertEquals("3RM Fee expected when valid excise code", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));
				AssertEquals("B01 Fee not expected for canary island", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));

				invoiceLine.JI_ZZF_NKTaxType = "XXX";
				declaration.DoMerge();
				AssertEquals("3RM Fee not expected when invalid excise code", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));

				invoiceLine.JI_ZZF_NKTaxType = IV1VatType;
				invoiceLine.ZG_AIEMType = "0A7";
				declaration.DoMerge();
				AssertEquals("Prereq 3AI Fee expected", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));
				AssertEquals("3RM Fee expected when valid excise code", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));
				AssertEquals("B01 Fee not expected for canary island", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));

				var aiemFee = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM);
				var retailerFee = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge);
				AssertEquals("Base amount (aiemBase + aiemAmount)", aiemFee.G4_BaseAmount + new ZDecimal(aiemFee.G4_Amount), retailerFee.G4_BaseAmount);

				addInfoCusImp.ZO_Retailer = false;
				declaration.DoMerge();
				AssertEquals("3RM Fee not expected when importer is not retailer", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));
				AssertEquals("B01 Fee not expected when importer is not retailer", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));

				addInfoCusImp.ZO_Retailer = true;
				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.DoMerge();
				AssertEquals("3RM Fee not expected when no importer", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));
				AssertEquals("B01 Fee not expected when no importer", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));

				invoiceLine.ZG_HasNonRecycledPlastics = ZBool.True;
				declaration.DoMerge();
				AssertEquals("1PL Fee expected when ZG_HasNonRecycledPlastics is True", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.NonRecycledPlasticFee));

				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.DoMerge();
				AssertEquals("3RM Fee not expected when no import declaration", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));
				AssertEquals("B01 Fee not expected when no import declaration", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));
				AssertEquals("1PL Fee not expected when no import declaration", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.NonRecycledPlasticFee));
			});
		}

		public void TestCalculateRetailerFees_WithEGVCharges()
		{
			AssertCalculateRetailerFees(AddEGVCharges);
		}

		public void TestCalculateRetailerFees_WithEGPCharges()
		{
			AssertCalculateRetailerFees(AddEGPCharges);
		}

		public void TestCalculateRetailerFees_WithEGVAndEGPCharges()
		{
			AssertCalculateRetailerFees(AddEGVAndEGPCharges);
		}

		public void TestCalculateRetailerFees_WithoutESCustomsValue()
		{
			AssertCalculateRetailerFees(setLinePriceEmpty: true);
		}

		public void TestCalculateRetailerFees_WithoutEUCustomsValue()
		{
			AssertCalculateRetailerFees(AddEGVCharges, true);
		}

		void AssertCalculateRetailerFees(Action addCharges = null, bool setLinePriceEmpty = false)
		{
			SetUpTariffAndAIEMReferenceData(TariffCode, ExciseCode, true);

			var importer = Factory.New<OrgHeader>();
			var addInfoCusImp = ESOrgImpAddInfo.Get(importer);
			addInfoCusImp.ZO_Retailer = true;

			var declaration = SetUpDeclaration(TariffCode, vatType: IV1VatType);
			declaration.ZG_DestinationState = "1";
			declaration.JE_OH_Importer = importer.PK;

			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			if (addCharges != null)
			{
				addCharges.Invoke();
			}
			if (setLinePriceEmpty)
			{
				invoiceLine.JI_LinePrice = 0;
			}

			var fee = entryLine.Fees.AddNew();
			fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			fee.G4_BaseAmount = 30m;
			declaration.DoMerge();
			AssertEquals("B01 Fee expected when valid excise code", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));
			AssertEquals("3RM Fee not expected for in land", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));
			var expectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "B01", ChargeAmount = 15m, BaseValue = 30m, Rate = 50m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "", OverrideReason = "ADD" },
			};
			AssertEntryLineFees(entryLine, expectedFees);

			var feeCI = entryLine.Fees.AddNew();
			feeCI.G4_Type = "3IG";
			feeCI.G4_BaseAmount = 30m;
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			invoiceLine.ZG_AIEMType = "0A7";
			declaration.DoMerge();
			AssertEquals("Prereq 3AI Fee expected", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));
			AssertEquals("3RM Fee expected when valid excise code", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge));
			AssertEquals("B01 Fee not expected for canary island", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge));
			expectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "3AI", ChargeAmount = 6m, BaseValue = 30m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "3RM", ChargeAmount = 18m, BaseValue = 36m, Rate = 50m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "", OverrideReason = "ADD" },
				new FeeAssertionObject() { ChargeType = "3IG", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "", OverrideReason = "ADD" },
			};
			AssertEntryLineFees(entryLine, expectedFees);
		}

		public void TestAIEMTaxesCalculation()
		{
			SetUpTariffAndAIEMReferenceData(TariffCode, ExciseCode);

			CombineAssertions(() =>
			{
				var declaration = SetUpDeclaration(TariffCode);
				declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

				var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var aiemIntermediateResults = new AIEMTaxCalculator(entryLine).CalculateExtraFees();
				declaration.DoMerge();
				AssertEquals("Not 3AI Fee expected", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));

				var fee = entryLine.Fees.AddNew();
				fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
				fee.G4_BaseAmount = 30m;
				invoiceLine.ZG_AIEMType = "0A7";

				declaration.DoMerge();
				AssertEquals("3AI Fee expected", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));

				declaration.ZG_DestinationState = "11";
				declaration.DoMerge();
				AssertEquals("Not 3AI Fee expected for IMP declaration and not Canary Island destination", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));

				declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.DoMerge();
				AssertEquals("Not 3AI Fee expected for EXP declaration and Canary Island destination", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));
			});
		}

		public void TestAIEMTaxesCalculation_WithEGVCharges()
		{
			AssertAIEMTaxesCalculation(AddEGVCharges);
		}

		public void TestAIEMTaxesCalculation_WithEGPCharges()
		{
			AssertAIEMTaxesCalculation(AddEGPCharges);
		}

		public void TestAIEMTaxesCalculation_WithEGVAndEGPCharges()
		{
			AssertAIEMTaxesCalculation(AddEGVAndEGPCharges);
		}

		public void TestAIEMTaxesCalculation_WithoutESCustomsValue()
		{
			AssertAIEMTaxesCalculation(setLinePriceEmpty: true);
		}

		public void TestAIEMTaxesCalculation_WithoutEUCustomsValue()
		{
			AssertAIEMTaxesCalculation(AddEGVCharges, true);
		}

		void AssertAIEMTaxesCalculation(Action addCharges = null, bool setLinePriceEmpty = false)
		{
			SetUpTariffAndAIEMReferenceData(TariffCode, ExciseCode);

			var declaration = SetUpDeclaration(TariffCode);
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			if (addCharges != null)
			{
				addCharges.Invoke();
			}
			if (setLinePriceEmpty)
			{
				invoiceLine.JI_LinePrice = 0;
			}

			var aiemIntermediateResults = new AIEMTaxCalculator(entryLine).CalculateExtraFees();
			declaration.DoMerge();
			AssertEquals("Not 3AI Fee expected", false, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));
			AssertEquals("No Fees expected", false, entryLine.Fees.Any());

			var fee = entryLine.Fees.AddNew();
			fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			fee.G4_BaseAmount = 30m;
			invoiceLine.ZG_AIEMType = "0A7";

			declaration.DoMerge();
			AssertEquals("3AI Fee expected", true, entryLine.Fees.Any(x => ((CusEntryLineFee)x).G4_Type == UniversalReferenceConstants.RefCusRateCode.AIEM));
			AssertEquals("Fees expected", true, entryLine.Fees.Any());
			var expectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "3AI", ChargeAmount = 6m, BaseValue = 30m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "", OverrideReason = "ADD" },
			};
			AssertEntryLineFees(entryLine, expectedFees);
		}

		public void TestCalculateExtraSystemFeesIfApplicable_WithExciseExcemption_EOrBOrN()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 24.2m, BaseValue = 110m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, exciseExemption: "E");
		}

		public void TestCalculateExtraSystemFeesIfApplicable_WithExciseExcemption_EOrBOrN_WithRateCode1PL()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "1PL", ChargeAmount = 0m, BaseValue = 0m, Rate = 0.45m, MethodOfCalculation = "PK", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 24.2m, BaseValue = 110m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, exciseExemption: "B", hasNonRecycledPlastics: true);
		}

		public void TestCalculateExtraSystemFeesIfApplicable_WithExciseExcemption_DifferentOfEOrBOrN()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 24.2m, BaseValue = 110m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, exciseExemption: "D");
		}

		public void TestCalculateExtraSystemFeesIfApplicable_WithExciseExcemption_DifferentOfEOrBOrN_WithRateCode1PL()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "1PL", ChargeAmount = 0m, BaseValue = 0m, Rate = 0.45m, MethodOfCalculation = "PK", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 24.2m, BaseValue = 110m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, exciseExemption: "D", hasNonRecycledPlastics: true);
		}

		public void TestExciseFeesConsideredForVatCalculation()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 6m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 25.52m, BaseValue = 116m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees);
		}

		public void TestExciseFeesConsideredForVatCalculation_ExciseExcemption()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 24.2m, BaseValue = 110m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, exciseExemption: "S");
		}

		public void TestExciseFeesConsideredForVatCalculation_WithEGVCharges()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 3m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 24.86m, BaseValue = 113m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, AddEGVCharges);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithEGPCharges()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 1.2m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 2m, BaseValue = 20m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 5.1m, BaseValue = 23.2m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, AddEGPCharges);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithEGVAndEGPCharges()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0.6m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 2m, BaseValue = 20m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 4.97m, BaseValue = 22.6m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, AddEGVAndEGPCharges);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithoutESCustomsValue()
		{
			AssertExciseFeesConsideredForVatCalculation_WithoutCustomsValue();
		}

		public void TestExciseFeesConsideredForVatCalculation_WithoutEUCustomsValue()
		{
			AssertExciseFeesConsideredForVatCalculation_WithoutCustomsValue(true);
		}

		void AssertExciseFeesConsideredForVatCalculation_WithoutCustomsValue(bool shouldAddEGVCharges = false)
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 0m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 0m, BaseValue = 0m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, shouldAddEGVCharges ? AddEGVCharges : null, true);
		}

		void AssertExciseFeesConsideredForVatCalculation(FeeAssertionObject[] entryLineAExpectedFees, Action onAfterDeclarationSetup = null, bool setLinePriceEmpty = false, string exciseExemption = "", bool hasNonRecycledPlastics = false)
		{
			SetUpVatDutyAndExciseReferenceData(STDPreferenceCode, TariffCode, ORDVatType, ExciseCode);
			var declaration = SetUpDeclaration(TariffCode, stdPreferenceCode: STDPreferenceCode, vatType: ORDVatType, exciseCode: ExciseCode, exciseExemption: exciseExemption, hasNonRecycledPlastics: hasNonRecycledPlastics);

			onAfterDeclarationSetup?.Invoke();

			if (setLinePriceEmpty)
			{
				invoiceLine.JI_LinePrice = 0;
			}

			declaration.DoMerge();

			var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
			var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

			AssertEntryLineFees(entryLine, entryLineAExpectedFees);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithSpecialExciseFee()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 6m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "5A7", ChargeAmount = 30m, BaseValue = 30m, Rate = 1m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 32.12m, BaseValue = 146m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee(entryLineAExpectedFees);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_ExciseExcemption()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "5A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 0m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 24.2m, BaseValue = 110m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee(entryLineAExpectedFees, exciseExemption: "S");
		}

		public void TestExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithEGVCharges()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 3m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "5A7", ChargeAmount = 15m, BaseValue = 30m, Rate = 1m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 28.16m, BaseValue = 128m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee(entryLineAExpectedFees, AddEGVCharges);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithEGPCharges()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 1.2m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "5A7", ChargeAmount = 6m, BaseValue = 30m, Rate = 1m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 2m, BaseValue = 20m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 6.42m, BaseValue = 29.2m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee(entryLineAExpectedFees, AddEGPCharges);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithEGVAndEGPCharges()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0.6m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "5A7", ChargeAmount = 3m, BaseValue = 30m, Rate = 1m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 2m, BaseValue = 20m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 5.63m, BaseValue = 25.6m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee(entryLineAExpectedFees, AddEGVAndEGPCharges);
		}

		public void TestExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithoutESCustomsValue()
		{
			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithoutCustomsValue();
		}

		public void TestExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithoutEUCustomsValue()
		{
			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithoutCustomsValue(true);
		}

		void AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee_WithoutCustomsValue(bool shouldAddEGVCharges = false)
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "5A7", ChargeAmount = 0m, BaseValue = 30m, Rate = 1m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 0m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 0m, BaseValue = 0m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

			AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee(entryLineAExpectedFees, shouldAddEGVCharges ? AddEGVCharges : null, true);
		}

		void AssertExciseFeesConsideredForVatCalculation_WithSpecialExciseFee(FeeAssertionObject[] entryLineAExpectedFees, Action addCharges = null, bool setLinePriceEmpty = false, string exciseExemption = "")
		{
			SetUpVatDutyAndExciseReferenceData(STDPreferenceCode, TariffCode, ORDVatType, ExciseCode, ExciseCodeSpecial);
			var declaration = SetUpDeclaration(TariffCode, stdPreferenceCode: STDPreferenceCode, vatType: ORDVatType, exciseCode: ExciseCode, exciseExemption: exciseExemption);
			if (addCharges != null)
			{
				addCharges.Invoke();
			}
			if (setLinePriceEmpty)
			{
				invoiceLine.JI_LinePrice = 0;
			}

			declaration.DoMerge();

			var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
			var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

			AssertEntryLineFees(entryLine, entryLineAExpectedFees);
		}

		public void TestMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults()
		{
			var normalAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4600m, BaseValue = 25000m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var minAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 25000m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4620m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var maxAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 5040m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 28000m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults(normalAppliesExpectedFees, minAppliesExpectedFees, maxAppliesExpectedFees);
		}

		public void TestMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithEGVCharges()
		{
			var normalAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4600m, BaseValue = 25000m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var minAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 25000m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4601.59m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var maxAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 5022.06m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 28000m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults(normalAppliesExpectedFees, minAppliesExpectedFees, maxAppliesExpectedFees, AddEGVCharges);
		}

		public void TestMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithEGPCharges()
		{
			var normalAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4585.28m, BaseValue = 24920m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var minAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 24920m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4605.22m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var maxAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 5025.6m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 27920m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults(normalAppliesExpectedFees, minAppliesExpectedFees, maxAppliesExpectedFees, AddEGPCharges);
		}

		public void TestMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithEGVAndEGPCharges()
		{
			var normalAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4585.28m, BaseValue = 24920m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var minAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 24920m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 4586.87m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var maxAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 5007.72m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 27920m, Rate = 18.4m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults(normalAppliesExpectedFees, minAppliesExpectedFees, maxAppliesExpectedFees, AddEGVAndEGPCharges);
		}

		public void TestMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithoutESCustomsValue()
		{
			AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithoutCustomsValue();
		}

		public void TestMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithoutEUCustomsValue()
		{
			AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithoutCustomsValue(true);
		}

		public void TestNonRecycledPlasticFee()
		{
			var entryLineAExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "0A7", ChargeAmount = 6m, BaseValue = 30m, Rate = 0.2m, MethodOfCalculation = "PVP", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 10m, BaseValue = 100m, Rate = 10m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 27.75m, BaseValue = 126.15m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "1PL", ChargeAmount = 10.1475m, BaseValue = 22.55m, Rate = 0.45m, MethodOfCalculation = "PK", OverrideReason = "" },
			};

			Action setupNonRecycledPlasticData = () =>
			{
				invoiceLine.ZG_HasNonRecycledPlastics = ZBool.True;
				invoiceLine.JI_CustomsFourthQuantity = 22.55m;
				invoiceLine.JI_CustomsFourthUnitQty = ESConstants.UOM.PK;
			};

			AssertExciseFeesConsideredForVatCalculation(entryLineAExpectedFees, onAfterDeclarationSetup: setupNonRecycledPlasticData);
		}

		public void TestGetExtraFeeCalculatorCollection_ForNonRecycledPlasticsFeeCalculator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dutyCalculationStrategy = new DutyCalculatorStrategyForTest(declaration);
			var calculators = dutyCalculationStrategy.GetExtraFeeCalculatorCollectionExposed(entryLine).ToArray();
			Assert("NonRecycledPlasticsFeeCalculator is returned", calculators.Any(c => c is NonRecycledPlasticTaxCalculator));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			calculators = dutyCalculationStrategy.GetExtraFeeCalculatorCollectionExposed(entryLine).ToArray();
			Assert("NonRecycledPlasticsFeeCalculator is not returned", calculators.All(c => c.GetType() != typeof(NonRecycledPlasticTaxCalculator)));
		}

		void AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults_WithoutCustomsValue(bool shouldAddEGVCharges = false)
		{
			var normalAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 0m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 200m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var minAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 0m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			var maxAppliesExpectedFees = new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 24m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 0m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "A00", ChargeAmount = 0m, BaseValue = 210m, Rate = 22m, MethodOfCalculation = "DTN", OverrideReason = "" },
			};

			AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults(normalAppliesExpectedFees, minAppliesExpectedFees, maxAppliesExpectedFees, shouldAddEGVCharges ? AddEGVCharges : null, true);
		}

		void AssertMergeCreatesDutyFeesForFormulaNonParticipatingMinMaxResults(FeeAssertionObject[] normalAppliesExpectedFees, FeeAssertionObject[] minAppliesExpectedFees, FeeAssertionObject[] maxAppliesExpectedFees, Action addCharges = null, bool setLinePriceEmpty = false)
		{
			var rateFormula = "MIN(IF([DTN] = 0, 100, 24 * [DTN]), MAX(IF(VFD < 1000, 0.2 * VFD, 0.184 * VFD), 22 * [DTN]))";

			var configurationBuilder = DutyReferenceDataConfigurationBuilder.New(Factory)
				.AddPreferences(STDPreferenceCode)
				.AddTariffType("IMP")
				.AddTariff(TariffCode, taxOrFeeCode: "DTY")
				.AddRateCode(RateTypeEnum.Duty, rateCode: "A00", rateFormula, preference: "100");
			configurationBuilder.Configure();
			Factory.Save();

			var declaration = SetUpDeclaration(TariffCode, stdPreferenceCode: STDPreferenceCode);
			invoiceLine.JI_Procedure = "A";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "TNE";
			if (addCharges != null)
			{
				addCharges.Invoke();
			}
			if (setLinePriceEmpty)
			{
				invoiceLine.JI_LinePrice = 0;
			}
			else
			{
				invoiceLine.JI_LinePrice = 25000;
			}

			declaration.DoMerge();

			var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
			AssertEquals("Entry Header count", 1, entryHeaders.Count());
			var entryHeader = entryHeaders.Single();
			AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines.Single();

			// Normal Fee of 18.4% VFD applies
			AssertEntryLineFees(entryLine, normalAppliesExpectedFees);

			// Increase quantity to change formula expression results.
			invoiceLine.JI_CustomsQuantity = 21;
			declaration.DoMerge();

			// Minimum Fee of 22 * DTN applies
			AssertEntryLineFees(entryLine, minAppliesExpectedFees);

			if (!setLinePriceEmpty)
			{
				// Increase invoice line price to change formula expression results.
				invoiceLine.JI_LinePrice = 28000;
			}
			declaration.DoMerge();

			// Maximum Fee of 24 * DTN applies
			AssertEntryLineFees(entryLine, maxAppliesExpectedFees);
		}

		RefCusTariffType SetUpTariffReferenceData(UniversalReferenceTestDataHelper helper, string countryCode, string tariffCode, bool setUpDuty = false, CusRefTradeGroupView tradeGroup = null)
		{
			var importTariffType = helper.CreateTariffType(countryCode, "IMP");
			var importTariff = helper.LoadOrCreateNewTariff(countryCode, importTariffType.PK, tariffCode, startDate, endDate);

			if (setUpDuty)
			{
				const string dtyCode = "A00";

				var dtyRateType = helper.CreateCusRateType(countryCode, "DTY");
				var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, dtyCode, dtyRateType.PK);
				var dtyRate = helper.CreateRate(importTariff, dtyRateCode.PK, startDate, endDate, rateFormula: "0.1*VFD");
				if (tradeGroup != null)
				{
					helper.CreateCusApplicability(dtyRate.PK, tradeGroup, startDate, endDate);
				}
			}
			Factory.Save();
			return importTariffType;
		}

		void SetUpTariffAndAIEMReferenceData(string tariffCode, string aiemCode, bool setUpRetailerFee = false)
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);

			#region CanaryIslands Setup
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
			#endregion

			#region Fees Setup
			if (setUpRetailerFee)
			{
				var mapType = helper.CreateCusMapType(UniversalReferenceConstants.RefCusMapType.RetailerFee, Universal.MapDirectionList.Codes.BTH, "Retailer fee Spain", true);
				helper.CreateCusMap(mapType.ZZP_MapType, IV1VatType, "RQ1", startDate, endDate, countryCode);

				var taxOrFeeType = helper.CreateRefCusTaxOrFeeType("VAT");
				var taxOrFee = helper.CreateTaxOrFee("RQ1", 0.5000, countryCode, startDate, endDate);
				taxOrFee.ZZF_ZX0_NKTaxOrFeeType = taxOrFeeType.ZX0_TaxOrFeeType;
			}
			#endregion

			var importTariffType = SetUpTariffReferenceData(helper, countryCode, tariffCode);

			var aiemTariffType = helper.CreateTariffType(countryCode, UniversalReferenceConstants.RefCusTariffType.AIEM);
			var aiemRateType = helper.CreateCusRateType(countryCode, "EXC");
			var aiemTariff = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, aiemCode, startDate, endDate);
			var aiemRateCode = helper.LoadOrCreateNewCusRateCode(Factory, aiemCode, aiemRateType.PK);
			helper.CreateRate(aiemTariff, aiemRateCode.PK, startDate, endDate, rateFormula: "0.2*VFD");
			helper.CreateTariffRelationship(aiemTariff.PK, importTariffType.PK, tariffCode);
			Factory.Save();
		}

		void SetUpVatDutyAndExciseReferenceData(string stdPreferenceCode, string tariffCode, string vatType, string exciseCode, string exciseCodeSpecial = "")
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry(stdPreferenceCode, "STANDARD", countryCode);
			helper.CreateTaxOrFee(vatType, 0.22, countryCode, startDate, endDate);
			var tradeGroup = helper.CreateTradeGroup(countryCode, "STANDARD", startDate, endDate);

			var dtyTariffType = SetUpTariffReferenceData(helper, countryCode, tariffCode, true, tradeGroup);

			var exciseTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var exciseRateType = helper.CreateCusRateType(countryCode, "EXC");
			var exciseTariff = helper.LoadOrCreateNewTariff(countryCode, exciseTariffType.PK, exciseCode, startDate, endDate);
			var exciseRateCode = helper.LoadOrCreateNewCusRateCode(Factory, exciseCode, exciseRateType.PK);
			helper.CreateRate(exciseTariff, exciseRateCode.PK, startDate, endDate, rateFormula: "0.2*PVP");
			helper.CreateTariffRelationship(exciseTariff.PK, dtyTariffType.PK, TariffCode);

			if (!string.IsNullOrEmpty(exciseCodeSpecial))
			{
				exciseTariff = helper.LoadOrCreateNewTariff(countryCode, exciseTariffType.PK, exciseCodeSpecial, startDate, endDate);
				exciseRateCode = helper.LoadOrCreateNewCusRateCode(Factory, exciseCodeSpecial, exciseRateType.PK);
				helper.CreateRate(exciseTariff, exciseRateCode.PK, startDate, endDate, rateFormula: "1*PVP");
				helper.CreateTariffRelationship(exciseTariff.PK, dtyTariffType.PK, TariffCode);
			}
			Factory.Save();
		}

		void AddEGVCharges()
		{
			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 40m, Declaration.LocalCurrencyCode);
			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 60m, Declaration.LocalCurrencyCode);
		}

		void AddEGPCharges()
		{
			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 30m, Declaration.LocalCurrencyCode);
			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 50m, Declaration.LocalCurrencyCode);
		}

		void AddEGVAndEGPCharges()
		{
			AddEGVCharges();
			AddEGPCharges();
		}

		JobDeclaration SetUpDeclaration(string tariffCode, string stdPreferenceCode = "", string vatType = "", string exciseCode = "", string exciseExemption = "", bool hasNonRecycledPlastics = false)
		{
			var newDec = Factory.New<JobDeclaration>();
			newDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			newDec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			newDec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryInstruction = newDec.CustomsEntryInstructions.AddNew();
			var invoice = newDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_PrimaryPreference = stdPreferenceCode;
			invoiceLine.JI_Tariff = tariffCode;
			invoiceLine.ZG_ExciseCode = exciseCode;
			invoiceLine.ZG_TotalRetailPrice = 30;
			invoiceLine.JI_LinePrice = 100;
			invoiceLine.JI_ZZF_NKTaxType = vatType;
			invoiceLine.ZG_ExciseExemption = exciseExemption;
			invoiceLine.ZG_HasNonRecycledPlastics = hasNonRecycledPlastics;

			return newDec;
		}
		JobComInvoiceLine invoiceLine;

		readonly ZString countryCode = Core.Constants.CountryCodes.Spain;
		readonly ZDateTime startDate = ZDateTime.Today.AddYears(-1);
		readonly ZDateTime endDate = ZDateTime.Today.AddYears(1);

		const string ExciseCode = "0A7";
		const string TariffCode = "11112222";
		const string IV1VatType = "IV1";
		const string STDPreferenceCode = "STD";
		const string ORDVatType = "ORD";
		const string ExciseCodeSpecial = "5A7";
	}

	internal sealed class DutyCalculatorStrategyForTest : DutyCalculatorStrategy
	{
		public DutyCalculatorStrategyForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		internal IEnumerable<IExtraFeeCalculator> GetExtraFeeCalculatorCollectionExposed(CusEntryLine entryLine) => GetExtraFeeCalculatorCollectionCore(entryLine);
	}
}
