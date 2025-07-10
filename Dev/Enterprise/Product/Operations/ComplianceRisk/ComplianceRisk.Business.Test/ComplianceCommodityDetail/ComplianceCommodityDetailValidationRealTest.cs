using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceCommodityDetailValidationRealTest : TestCaseWithFactory
	{
		public void TestTariffView_WhenConditionsIsEmpty_ShouldNotShowWarning()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "123456");

			CombineAssertions("Pre-conditions:", () =>
			{
				AssertEquals("123456", tariffView.ZZ1_TariffCode);
				Assert(tariffView.Conditions.IsNullOrEmpty());
			});

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = tariffView.ZZ1_TariffCode;
			Assert(!commodityDetail.ConditionsInfo.HasWarnings());
		}

		public void TestTariffView_WhenConditionsIsNotEmpty_WithBorderWiseAPIIntegration_ShouldShowWarning()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(Factory, "123456", "Condition Value");

			CombineAssertions("Pre-conditions:", () =>
			{
				AssertEquals("123456", tariffView.ZZ1_TariffCode);
				AssertEquals("Condition Value", tariffView.Conditions[0].ConditionValues[0].ZX3_Value);
			});

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = tariffView.ZZ1_TariffCode;
			commodityDetail.Conditions = tariffView.Conditions[0].ConditionValues[0].ZX3_Value;
			commodityDetail.RelatedJobIsAssessmentInitialized = true;
			commodityDetail.CommodityType = CommodityType.RelatedJobLink;
			AssertNoWarnings(commodityDetail.ConditionsInfo);
		}

		public void TestTariffView_WhenTariffCodeNotUnique_ShouldShowError()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var tariffView1 = Factory.NewWithValidTestData<TariffView>();
			tariffView1.ZZ1_TariffCode = "123456";

			var commodityDetail1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail1.CCD_HarmonizedCode = tariffView1.ZZ1_TariffCode;

			var tariffView2 = Factory.NewWithValidTestData<TariffView>();
			tariffView2.ZZ1_TariffCode = "123456";

			var commodityDetail2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail2.CCD_HarmonizedCode = tariffView2.ZZ1_TariffCode;

			commodityDetail2.CommodityType = CommodityType.RelatedJobLink;
			commodityDetail2.Validation.ValidateCCD_HarmonizedCode();
			AssertEquals(true, commodityDetail2.CCD_HarmonizedCodeInfo.ReadOnly);
			AssertNoRowErrors(commodityDetail2);
		}

		public void TestTariffView_WhenTariffCodeGoodsDescriptionOriginOfGoodsNotUnique_ShouldShowError()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

				var duplicatedError = "Harmonized Code, Goods Description and Origin of Goods must be unique.";

				var tariffView1 = Factory.NewWithValidTestData<TariffView>();
				tariffView1.ZZ1_TariffCode = "123456";

				var commodityDetail1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodityDetail1.CCD_HarmonizedCode = tariffView1.ZZ1_TariffCode;
				commodityDetail1.CCD_Description = "TEST";
				commodityDetail1.CCD_RN_NKOrigin = "AU";

				var tariffView2 = Factory.NewWithValidTestData<TariffView>();
				tariffView2.ZZ1_TariffCode = "123456";

				var commodityDetail2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodityDetail2.CCD_HarmonizedCode = tariffView2.ZZ1_TariffCode;
				commodityDetail2.CCD_Description = "test";
				commodityDetail2.CCD_RN_NKOrigin = "au";
				AssertHasRowError(commodityDetail2, duplicatedError);

				commodityDetail2.CommodityType = CommodityType.RelatedJobLink;
				commodityDetail2.Validation.ValidateCCD_HarmonizedCode();
				AssertEquals(true, commodityDetail2.CCD_HarmonizedCodeInfo.ReadOnly);
				AssertNoRowErrors(commodityDetail2);

				commodityDetail2.CommodityType = CommodityType.FetchDataEntry;
				commodityDetail2.CCD_RN_NKOrigin = "US";
				AssertNoRowError(commodityDetail2, duplicatedError);

				commodityDetail2.CCD_RN_NKOrigin = "AU";
				commodityDetail2.CCD_Description = "another";
				AssertNoRowError(commodityDetail2, duplicatedError);
			}
		}

		public void TestTariffView_WhenTariffCodeWithAlphaNumericCharacters_ShouldShowError()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var tariffView = Factory.NewWithValidTestData<TariffView>();
			tariffView.ZZ1_TariffCode = "123eqdb";

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = tariffView.ZZ1_TariffCode;

			CombineAssertions(() =>
			{
				Assert(!commodityDetail.CCD_HarmonizedCodeInfo.HasWarnings());
				Assert(commodityDetail.CCD_HarmonizedCodeInfo.HasError("Invalid Harmonized Code. Only numeric characters are allowed."));
			});
		}

		public void TestTariffView_WhenTariffCodeWithAlphaNumericCharactersAndNotUniqueCode_ShouldShowErrors()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var tariffView1 = Factory.NewWithValidTestData<TariffView>();
			tariffView1.ZZ1_TariffCode = "123WTG";

			var commodityDetail1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail1.CCD_HarmonizedCode = tariffView1.ZZ1_TariffCode;

			CombineAssertions(() =>
			{
				var expectedErrorMessage = "Invalid Harmonized Code. Only numeric characters are allowed.";
				Assert(commodityDetail1.CCD_HarmonizedCodeInfo.HasError(expectedErrorMessage));
			});
		}

		public void TestHarmonizedCodeValidation()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityDetail1 = GetCommodityDetail("123456");
			var commodityDetail2 = GetCommodityDetail("1234.56");
			var commodityDetail3 = GetCommodityDetail("1234AB");
			var commodityDetail4 = GetCommodityDetail("1234 AB");
			var commodityDetail5 = GetCommodityDetail("1234ABCD", CommodityType.FetchDataEntry);

			var errorMessage = "Invalid Harmonized Code. Only numeric characters are allowed.";

			CombineAssertions(() =>
			{
				AssertNoError(commodityDetail1.CCD_HarmonizedCodeInfo, errorMessage);
				AssertHasError(commodityDetail2.CCD_HarmonizedCodeInfo, errorMessage);
				AssertHasError(commodityDetail3.CCD_HarmonizedCodeInfo, errorMessage);
				AssertHasError(commodityDetail4.CCD_HarmonizedCodeInfo, errorMessage);
				AssertNoError(commodityDetail5.CCD_HarmonizedCodeInfo, errorMessage);
			});

			Factory.Save();
			commodityDetail1.CCD_HarmonizedCode = "123456CD";
			AssertHasError(commodityDetail1.CCD_HarmonizedCodeInfo, errorMessage);

			Factory.Save();
			commodityDetail1.Validation.ValidateCCD_HarmonizedCode();
			AssertHasWarning(commodityDetail1.CCD_HarmonizedCodeInfo, errorMessage);
			AssertNoError(commodityDetail1.CCD_HarmonizedCodeInfo, errorMessage);

			ComplianceCommodityDetail GetCommodityDetail(string code, CommodityType commodityType = CommodityType.UserDataEntry)
			{
				var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodityDetail.CommodityType = commodityType;
				commodityDetail.CCD_HarmonizedCode = code;

				return commodityDetail;
			}
		}

		public void TestWhenJobIsNotInitiated_ShouldNotShowWarnings()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

				var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodityDetail.CCD_HarmonizedCode = "123456";
				commodityDetail.RelatedJobIsAssessmentInitialized = true;
				commodityDetail.CommodityType = CommodityType.RelatedJobLink;

				commodityDetail.IsValidHsCode = false;
				commodityDetail.Validation.CheckCommodityStatus();
				AssertHasRowWarning(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
				commodityDetail.RelatedJobIsAssessmentInitialized = false;
				commodityDetail.Validation.CheckCommodityStatus();
				AssertNoRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);

				commodityDetail.CCD_NomenclatureCondition = false;
				commodityDetail.CCD_SpecificCondition = true;
				commodityDetail.Validation.CheckCommodityStatus();
				AssertNoRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);
				commodityDetail.RelatedJobIsAssessmentInitialized = true;
				commodityDetail.Validation.CheckCommodityStatus();
				AssertHasRowWarning(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);

				commodityDetail.BlockedByComplianceRule = true;
				commodityDetail.Validation.CheckCommodityStatus();
				AssertHasRowWarning(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.ComplianceRuleAdministratorMessage);
				commodityDetail.RelatedJobIsAssessmentInitialized = false;
				commodityDetail.Validation.CheckCommodityStatus();
				AssertNoRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.ComplianceRuleAdministratorMessage);
			}
		}

		public void TestCheckCommodityStatus_ShouldShowWarnings()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			var complianceEvent = Factory.NewWithValidTestData<StmComplianceEvent>();
			complianceEvent.SCE_ParentID = shipment.PK;
			complianceEvent.SCE_ParentTableCode = shipment.TablePrefix;
			complianceEvent.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			complianceEvent.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";

			AssertNoRowWarnings(commodityDetail);

			commodityDetail.BorderWiseCheckStatus = BorderWiseCheckStatus.NotViewable;
			commodityDetail.Validation.CheckCommodityStatus();
			AssertHasRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);

			commodityDetail.BorderWiseCheckInProgress = true;
			commodityDetail.Validation.CheckCommodityStatus();
			AssertHasRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.RiskCheckInProgressMessage);

			commodityDetail.CCD_NomenclatureCondition = false;
			commodityDetail.CCD_SpecificCondition = true;
			commodityDetail.Validation.CheckCommodityStatus();
			AssertHasRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);

			commodityDetail.CCD_NomenclatureCondition = true;
			commodityDetail.CCD_SpecificCondition = false;
			commodityDetail.Validation.CheckCommodityStatus();
			AssertNoRowWarningContaining("Ignore Nomenclature Condition", commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);

			var complianceRuleApply = ComplianceCommodityDetailValidationReal.GetMessages.ComplianceRuleAdministratorMessage;
			commodityDetail.BlockedByComplianceRule = true;
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			AssertEquals(true, commodityDetail.BlockedByComplianceRule);

			commodityDetail.Validation.CheckCommodityStatus();
			AssertHasRowWarningContaining(commodityDetail, complianceRuleApply);

			commodityDetail.CCD_HarmonizedCode = "654321";
			AssertEquals("Set to false when harmonized code change", false, commodityDetail.BlockedByComplianceRule);

			commodityDetail.Validation.CheckCommodityStatus();
			AssertNoRowWarningContaining(commodityDetail, complianceRuleApply);

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			commodityDetail.BlockedByComplianceRule = true;
			commodityDetail.Validation.CheckCommodityStatus();
			AssertNoRowWarningContaining(commodityDetail, complianceRuleApply);
		}

		public void TestUnsupportedCountryMessage_ShouldShowWarnings()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var complianceEvent = Factory.NewWithValidTestData<StmComplianceEvent>();
			complianceEvent.SCE_ParentID = shipment.PK;
			complianceEvent.SCE_ParentTableCode = shipment.TablePrefix;
			complianceEvent.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			complianceEvent.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_SpecificCondition = true;
			commodityDetail.Countries = new[]
			{
				("AU", true),
				("US", false),
				("CA", false)
			};

			commodityDetail.Validation.CheckCommodityStatus();
			AssertHasRowWarning(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedCountryMessage + string.Join(", ", commodityDetail.Countries
						.Where(u => !u.Supported)
						.Select(u => Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, u.CountryCode)?.Description)));
		}

		public void TestHarmonizedCodeNotEqualToMatchedHsCode_ShouldShowWarnings()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var complianceEvent = Factory.NewWithValidTestData<StmComplianceEvent>();
			complianceEvent.SCE_ParentID = shipment.PK;
			complianceEvent.SCE_ParentTableCode = shipment.TablePrefix;
			complianceEvent.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			complianceEvent.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_SpecificCondition = true;

			CombineAssertions("The Harmonized Matched Not Complete Message should be displayed when the Border Wise Check Status is only Viewable.", () =>
			{
				commodityDetail.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;
				commodityDetail.MatchedHsCode = "1234";
				commodityDetail.IsValidHsCode = true;
				commodityDetail.Validation.CheckCommodityStatus();

				AssertHasRowWarning(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.HarmonizedMatchedNotCompleteMessage + commodityDetail.MatchedHsCode);
				AssertNoRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
			});

			CombineAssertions("The Unsupported Harmonized Message should be displayed when the Border Wise Check Status is only NotViewable.", () =>
			{
				commodityDetail.BorderWiseCheckStatus = BorderWiseCheckStatus.NotViewable;
				commodityDetail.IsValidHsCode = false;
				commodityDetail.MatchedHsCode = null;
				commodityDetail.Validation.CheckCommodityStatus();

				AssertHasRowWarning(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
				AssertNoRowWarningContaining(commodityDetail, ComplianceCommodityDetailValidationReal.GetMessages.HarmonizedMatchedNotCompleteMessage);
			});
		}

		public void TestValidateAll_ShouldValidateAllCommodityDetails()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_RiskStatus = "BLK";

			commodityDetail.Validation.ValidateAll();
			AssertHasErrors("Please enter a value", commodityDetail.CCD_HarmonizedCodeInfo);
		}

		public void TestValidateAll_ShouldSkipCheckUniqueOfHsCodeAndDescriptionAndOrigin()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityDetail1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail1.CCD_HarmonizedCode = "123456";
			commodityDetail1.CCD_Description = "TEST";
			commodityDetail1.CCD_RN_NKOrigin = "AU";

			var commodityDetail2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail2.CCD_HarmonizedCode = "123456";
			commodityDetail2.CCD_Description = "test";
			commodityDetail2.CCD_RN_NKOrigin = "au";

			var duplicatedError = "Harmonized Code, Goods Description and Origin of Goods must be unique.";

			CombineAssertions(() =>
			{
				AssertHasRowError(commodityDetail2, duplicatedError);

				commodityDetail2.CommodityType = CommodityType.RelatedJobLink;
				commodityDetail2.Validation.ValidateAll();
				AssertHasRowError(commodityDetail2, duplicatedError);

				commodityDetail2.CommodityType = CommodityType.FetchDataEntry;
				commodityDetail2.CCD_RN_NKOrigin = "US";
				commodityDetail2.Validation.ValidateAll();
				AssertNoRowError(commodityDetail2, duplicatedError);

				commodityDetail2.CCD_RN_NKOrigin = "AU";
				commodityDetail2.CCD_Description = "another";
				commodityDetail2.Validation.ValidateAll();
				AssertNoRowError(commodityDetail2, duplicatedError);
			});
		}
	}
}
