using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class ComplianceCheckResponseModelUpdaterTest : TestCaseWithFactory
	{
		public void TestIsInValidHsCode()
		{
			var pointPairs = CommodityRiskStatusBorderWiseCheckerTest.GetPointPairs(string.Empty, string.Empty, "NZ", "NZAKL");
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AU";
			shipmentProvider.JS_RL_NKDestination = "US";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var collection = new ComplianceRiskPlugInBusinessObject(shipmentProvider).ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";
			commodity.CCD_Description = "TestGood";

			var complianceCheckResponseModel = new ComplianceCheckResponseModel
			{
				Commodities = new[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "123456",
						GoodsDescription = "TestGood",
						Origin = new[] { "AU" },
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
									CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
									IsValidHsCode = false
								},
								DestinationPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
									CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
									IsValidHsCode = false
								}
							}
						}
					}
				}
			};

			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodity, complianceCheckResponseModel, GetRefComplianceCommodityAlertHelperForTest());

			AssertEquals(false, commodity.IsValidHsCode);
		}

		public void TestIsValidHsCode()
		{
			var pointPairs = CommodityRiskStatusBorderWiseCheckerTest.GetPointPairs(string.Empty, string.Empty, "NZ", "NZAKL");
			var shipmentProvider = Factory.New<ShipmentWithProvider>();
			shipmentProvider.JS_RL_NKOrigin = "AU";
			shipmentProvider.JS_RL_NKDestination = "US";
			shipmentProvider.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);

			var collection = new ComplianceRiskPlugInBusinessObject(shipmentProvider).ComplianceRiskStatus.CommodityDetailCollection;
			var commodity = collection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";
			commodity.CCD_Description = "TestGood";

			var complianceCheckResponseModel = new ComplianceCheckResponseModel
			{
				Commodities = new[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "123456",
						GoodsDescription = "TestGood",
						Origin = new[] { "AU" },
						CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
						NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
									CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
									IsValidHsCode = true
								},
								DestinationPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									NomenclatureWideConditionsApply = BorderWiseApiHelper.ConditionApply,
									CommoditySpecificConditionsApply = BorderWiseApiHelper.ConditionApply,
									IsValidHsCode = true
								}
							}
						}
					}
				}
			};

			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodity, complianceCheckResponseModel, GetRefComplianceCommodityAlertHelperForTest());

			AssertEquals(true, commodity.IsValidHsCode);
		}

		public void TestUpdateCommoditiesRiskStatus()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			var emptyResponse = new ComplianceCheckResponseModel { Commodities = Array.Empty<ComplianceCheckResponseCommodityModel>() };
			var result = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, emptyResponse, GetRefComplianceCommodityAlertHelperForTest());

			AssertEquals(true, result.Length == 0);
		}

		public void TestUpdateCommoditiesRiskStatus_DetailsMismatch()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";

			var hsCodeMismatch = new ComplianceCheckResponseModel { Commodities = new[] { new ComplianceCheckResponseCommodityModel { HsCode = "654321", Origin = new[] { "AU" }, GoodsDescription = "TestGood" } } };
			var originMismatch = new ComplianceCheckResponseModel { Commodities = new[] { new ComplianceCheckResponseCommodityModel { HsCode = "123456", Origin = new[] { "EU" }, GoodsDescription = "TestGood" } } };
			var descriptionMismatch = new ComplianceCheckResponseModel { Commodities = new[] { new ComplianceCheckResponseCommodityModel { HsCode = "123456", Origin = new[] { "AU" }, GoodsDescription = "TestGood2" } } };

			AssertCollectionNotContains("No items should be returned (HsCode mismatch).", ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, hsCodeMismatch, GetRefComplianceCommodityAlertHelperForTest()));
			AssertCollectionNotContains("No items should be returned (Origin mismatch).", ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, originMismatch, GetRefComplianceCommodityAlertHelperForTest()));
			AssertCollectionNotContains("No items should be returned (GoodsDescription mismatch).", ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, descriptionMismatch, GetRefComplianceCommodityAlertHelperForTest()));
		}

		public void TestUpdateCommoditiesRiskStatus_RiskStatusHighRisk()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";
			commodityDetail.CCD_NomenclatureCondition = false;
			commodityDetail.CCD_SpecificCondition = false;
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;

			var nomenclatureUpdated = CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", commoditySpecificConditionsApply: false, nomenclatureWideConditionsApply: true, Factory, "AU", "TestGood");

			var result = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, nomenclatureUpdated, GetRefComplianceCommodityAlertHelperForTest());
			Assert(result.Length == 1);
			Assert(commodityDetail.CCD_NomenclatureCondition);

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			var specificConditionUpdated = CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", commoditySpecificConditionsApply: true, nomenclatureWideConditionsApply: false, Factory, "AU", "TestGood");
			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, specificConditionUpdated, GetRefComplianceCommodityAlertHelperForTest());
			Assert(commodityDetail.CCD_SpecificCondition);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, specificConditionUpdated, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodityDetail.CCD_RiskStatus);

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, specificConditionUpdated, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Released, commodityDetail.CCD_RiskStatus);
		}

		public void TestUpdateCommoditiesRiskStatus_ConditionChanges_RiskStatusChanges()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";
			commodityDetail.CCD_NomenclatureCondition = false;
			commodityDetail.CCD_SpecificCondition = false;
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

			var specificConditionUpdated = CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", commoditySpecificConditionsApply: true, nomenclatureWideConditionsApply: false, Factory, "AU", "TestGood");
			var result = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, specificConditionUpdated, GetRefComplianceCommodityAlertHelperForTest());

			Assert(result.Length == 1);
			Assert(commodityDetail.CCD_SpecificCondition);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, specificConditionUpdated, GetRefComplianceCommodityAlertHelperForTest());

			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);
		}

		public void TestUpdateCommoditiesRiskStatus_RiskStatusClears()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";
			commodityDetail.CCD_NomenclatureCondition = false;
			commodityDetail.CCD_SpecificCondition = false;

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotApplicable;

			var response = GetResponse("123456", true, "AU", "US", "123456", false, false);
			var result = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response, GetRefComplianceCommodityAlertHelperForTest());

			Assert(result.Length == 1);
			Assert(commodityDetail.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Clear);

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response, GetRefComplianceCommodityAlertHelperForTest());

			Assert(commodityDetail.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Clear);
		}

		public void TestUpdateCommoditiesRiskStatus_CountriesSupported()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

			var response1 = GetResponse("123456", true, "CN", "IR", "");
			var result1 = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response1, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodityDetail.CCD_RiskStatus);
			AssertContainsExactElementsInAnyOrder(new[] { ("CN", false), ("IR", false) }, commodityDetail.Countries);
			AssertNullOrEmpty(commodityDetail.MatchedHsCode);

			ComplianceCheckResponseModelUpdater.UpdateCommodityDetailExtension(commodityDetail);
			AssertEquals(BorderWiseCheckStatus.NotViewable, commodityDetail.BorderWiseCheckStatus);

			var response2 = GetResponse("123456", true, "CN", "AU", "1234", specificConditions: false);
			var result2 = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response2, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals("When there are supported countries, matched hs code is not null, no specific conditions apply", ComplianceRiskStatusCodeList.Codes.Clear, commodityDetail.CCD_RiskStatus);
			AssertContainsExactElementsInAnyOrder(new[] { ("CN", false), ("AU", true) }, commodityDetail.Countries);
			AssertEquals("1234", commodityDetail.MatchedHsCode);

			ComplianceCheckResponseModelUpdater.UpdateCommodityDetailExtension(commodityDetail);
			AssertEquals(BorderWiseCheckStatus.Viewable, commodityDetail.BorderWiseCheckStatus);

			var response3 = GetResponse("123456", true, "CN", "AU", "1234", specificConditions: true, false, "Test");
			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response3, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals("When there are supported countries, matched hs code is not null, specific conditions apply", ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);

			var response4 = GetResponse("123456", false, "CN", "IR", null);
			var result4 = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response4, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals("When countries are unsupported, isvalidhscode is false", ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);
			AssertContainsExactElementsInAnyOrder(new[] { ("CN", false), ("IR", false) }, commodityDetail.Countries);
			AssertNullOrEmpty(commodityDetail.MatchedHsCode);

			ComplianceCheckResponseModelUpdater.UpdateCommodityDetailExtension(commodityDetail);
			AssertEquals(BorderWiseCheckStatus.NotViewable, commodityDetail.BorderWiseCheckStatus);
		}

		public void TestUpdateCommoditiesRiskStatus_CommodityNomenclatureConditionEnabled()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotApplicable;

			var response1 = GetResponse("123456", true, "CN", "IR", "");
			var response2 = GetResponse("123456", true, "AU", "NZ", "123456", true, true, "TEST");
			var response3 = GetResponse("123456", true, "AU", "NZ", "123456", false, false);
			var response4 = GetResponse("123456", true, "AU", "US", "123456", false, true, "TEST");

			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response1, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodityDetail.CCD_RiskStatus);

			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response2, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);

			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response3, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodityDetail.CCD_RiskStatus);

			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_AlertCode = "TEST";
			alert.RCR_AlertType = "NOM";
			alert.RCR_AlertName = "DUMMY NAME";
			alert.RCR_TradeDirection = "IMP";
			alert.RCR_CountryRegion = "US";
			alert.RCR_CommodityRiskStatus = "PRS";

			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response4, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals("Only nomenclature conditions apply", ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodityDetail.CCD_RiskStatus);
		}

		public void TestComplianceRiskStatusPossibleRisk_WhenAllCountryUnsupportedAndHsCodeIsValid()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var response = GetResponse("123456", true, "CN", "IR", "");

			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response, GetRefComplianceCommodityAlertHelperForTest());
			AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodityDetail.CCD_RiskStatus);
		}

		static ComplianceCheckResponseModel GetResponse(string hsCode, bool isValidHsCode, string originCountry, string destCountry, string matchedHsCode, bool specificConditions = true, bool nomenclatureWideConditions = false, string complianceCodes = "")
		{
			return GetResponseForMultipleHsCodes([(hsCode, isValidHsCode, originCountry, destCountry, matchedHsCode, specificConditions, nomenclatureWideConditions, complianceCodes)]);
		}

		static ComplianceCheckResponseModel GetResponseForMultipleHsCodes((string hsCode, bool isValidHsCode, string originCountry, string destCountry, string matchedHsCode, bool specificConditions, bool nomenclatureWideConditions, string complianceCodes)[] responseInfo)
		{
			return new ComplianceCheckResponseModel
			{
				RequestId = Guid.NewGuid(),

				Commodities = responseInfo.Select(commodity => new ComplianceCheckResponseCommodityModel
				{
					HsCode = commodity.hsCode,
					Origin = ["AU"],
					GoodsDescription = "TestGood",
					CommoditySpecificConditionsApply = commodity.specificConditions ? BorderWiseApiHelper.ConditionApply : BorderWiseApiHelper.ConditionNotApply,
					NomenclatureWideConditionsApply = commodity.nomenclatureWideConditions ? BorderWiseApiHelper.ConditionApply : BorderWiseApiHelper.ConditionNotApply,
					PointPairs = new[]
					{
						new ComplianceCheckResponsePointPairModel
						{
							OriginPoint = new ComplianceCheckResponsePointPairLocationModel
							{
								Country = commodity.originCountry,
								MatchedHsCode = commodity.matchedHsCode,
								IsValidHsCode = commodity.isValidHsCode
							},
							DestinationPoint = new ComplianceCheckResponsePointPairLocationModel
							{
								Country = commodity.destCountry,
								MatchedHsCode = commodity.matchedHsCode,
								IsValidHsCode = commodity.isValidHsCode,
								ComplianceCodes = string.IsNullOrEmpty(commodity.complianceCodes) ? Array.Empty<string>() : new[] { commodity.complianceCodes }
							}
						}
					}
				}).ToArray(),
				Locations = new[]
				{
					new ComplianceCheckResponseLocationModel
					{
						Country = "AU",
						IsSupportedCountry = true
					},
					new ComplianceCheckResponseLocationModel
					{
						Country = "NZ",
						IsSupportedCountry = true
					},
					new ComplianceCheckResponseLocationModel
					{
						Country = "CN",
						IsSupportedCountry = false
					},
					new ComplianceCheckResponseLocationModel
					{
						Country = "IR",
						IsSupportedCountry = false
					},
				}
			};
		}

		RefComplianceCommodityAlertHelper GetRefComplianceCommodityAlertHelperForTest()
		{
			return new RefComplianceCommodityAlertHelper(Factory, GetResponse("123456", true, "AU", "US", "123456"));
		}

		public void TestComplianceRiskStatus_WhenCommodityAlertEnabled()
		{
			UpdateRefComplianceCommodityAlert();

			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = "AU";
			commodityDetail.CCD_Description = "TestGood";
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotApplicable;

			var response = GetResponseForMultipleHsCodes([("123456", true, "AU", "FR", "123456", false, true, "Test")]);
			var alertHelper = new RefComplianceCommodityAlertHelper(Factory, response);
			ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response, alertHelper);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, commodityDetail.CCD_RiskStatus);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateRefComplianceCommodityAlert()
		{
			var sql = @"-- UpdateComplianceCommodityAlert
			DELETE FROM dbo.RefComplianceCommodityAlert
			INSERT INTO dbo.RefComplianceCommodityAlert (
			  RCR_PK, RCR_IsActive, RCR_AlertCode, RCR_AlertDescription, RCR_AlertName, 
			  RCR_AlertType, RCR_CommodityRiskStatus, RCR_CountryRegion, RCR_PublishYear, 
			  RCR_SourceURL, RCR_TradeDirection, RCR_SystemCreateTimeUtc, RCR_SystemCreateUser, 
			  RCR_SystemLastEditTimeUtc, RCR_SystemLastEditUser
			) VALUES
			  (NEWID(), 1, 'TEST1', 'Description for alert 1', 'Alert Name 1', 'NOM', 'PRS', 'US', 2023, 'http://example.com/1', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'TEST2', 'Description for alert 2', 'Alert Name 2', 'COM', 'HSK', 'AU', 2022, 'http://example.com/2', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'TEST3', 'Description for alert 3', 'Alert Name 3', 'COM', 'HSK', 'EU', 2021, 'http://example.com/3', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR');";

			using (var connection = Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void TestUpdateCommoditiesRiskStatusWithDirection()
		{
			InsertRefComplianceCommodityAlert();

			AssertCommodityRisk(CommodityRiskCalculateFactor.All, "", [], [], [], ComplianceRiskStatusCodeList.Codes.Clear);
			AssertCommodityRisk(CommodityRiskCalculateFactor.All, "", ["AUPRSEXP"], ["NZPRSIMP"], [], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.All, "", ["AUHSKEXP"], ["NZPRSIMP"], [], ComplianceRiskStatusCodeList.Codes.HighRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.All, "", ["AUPRSEXP"], ["NZHSKIMP"], [], ComplianceRiskStatusCodeList.Codes.HighRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.All, "US", [], [], ["USPRSEXP"], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.All, "US", ["AUPRSEXP"], ["NZPRSIMP"], ["USHSKEXP"], ComplianceRiskStatusCodeList.Codes.HighRisk);

			AssertCommodityRisk(CommodityRiskCalculateFactor.Import, "", [], [], [], ComplianceRiskStatusCodeList.Codes.Clear);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Import, "", ["AUPRSEXP"], ["NZPRSIMP"], [], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Import, "", ["AUHSKEXP"], ["NZPRSIMP"], [], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Import, "", ["AUPRSEXP"], ["NZHSKIMP"], [], ComplianceRiskStatusCodeList.Codes.HighRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Import, "US", [], [], ["USPRSEXP"], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Import, "US", ["AUPRSEXP"], ["NZPRSIMP"], ["USHSKEXP"], ComplianceRiskStatusCodeList.Codes.HighRisk);

			AssertCommodityRisk(CommodityRiskCalculateFactor.Export, "", [], [], [], ComplianceRiskStatusCodeList.Codes.Clear);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Export, "", ["AUPRSEXP"], ["NZPRSIMP"], [], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Export, "", ["AUPRSEXP"], ["NZHSKIMP"], [], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Export, "", ["AUHSKEXP"], ["NZPRSIMP"], [], ComplianceRiskStatusCodeList.Codes.HighRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Export, "US", [], [], ["USPRSEXP"], ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			AssertCommodityRisk(CommodityRiskCalculateFactor.Export, "US", ["AUPRSEXP"], ["NZPRSIMP"], ["USHSKEXP"], ComplianceRiskStatusCodeList.Codes.HighRisk);

			AssertConditionsApply(CommodityRiskCalculateFactor.All, "", [], [], [], string.Empty, string.Empty);
			AssertConditionsApply(CommodityRiskCalculateFactor.All, "", ["AUPRSEXP"], [], [], "Yes", string.Empty);
			AssertConditionsApply(CommodityRiskCalculateFactor.All, "", [], ["NZPRSIMP"], [], "Yes", string.Empty);
			AssertConditionsApply(CommodityRiskCalculateFactor.All, "", ["AUHSKEXP"], [], [], string.Empty, "Yes");
			AssertConditionsApply(CommodityRiskCalculateFactor.All, "", [], ["NZHSKIMP"], [], string.Empty, "Yes");
			AssertConditionsApply(CommodityRiskCalculateFactor.All, "", ["AUPRSEXP"], ["NZHSKIMP"], [], "Yes", "Yes");
			AssertConditionsApply(CommodityRiskCalculateFactor.All, "US", [], [], ["USPRSEXP", "USHSKEXP"], "Yes", "Yes");

			AssertConditionsApply(CommodityRiskCalculateFactor.Import, "", [], [], [], string.Empty, string.Empty);
			AssertConditionsApply(CommodityRiskCalculateFactor.Import, "", ["AUPRSEXP", "AUHSKEXP"], [], [], string.Empty, string.Empty);
			AssertConditionsApply(CommodityRiskCalculateFactor.Import, "", [], ["NZPRSIMP", "NZHSKIMP"], [], "Yes", "Yes");
			AssertConditionsApply(CommodityRiskCalculateFactor.Import, "US", [], [], ["USPRSEXP", "USHSKEXP"], "Yes", "Yes");

			AssertConditionsApply(CommodityRiskCalculateFactor.Export, "", [], [], [], string.Empty, string.Empty);
			AssertConditionsApply(CommodityRiskCalculateFactor.Export, "", ["AUPRSEXP", "AUHSKEXP"], [], [], "Yes","Yes");
			AssertConditionsApply(CommodityRiskCalculateFactor.Export, "", [], ["NZPRSIMP", "NZHSKIMP"], [], string.Empty, string.Empty);
			AssertConditionsApply(CommodityRiskCalculateFactor.Export, "US", [], [], ["USPRSEXP", "USHSKEXP"], "Yes", "Yes");

			void AssertCommodityRisk(CommodityRiskCalculateFactor factor, string goodsOrigin, List<string> originCodes, List<string> destinationCodes, List<string> goodsOriginCodes, string expected)
			{
				var commodityDetail = GetCommodityDetail(goodsOrigin);
				var response = GetResponseWithComplianceCodes(goodsOrigin, originCodes, destinationCodes, goodsOriginCodes);
				var alertHelper = new RefComplianceCommodityAlertHelper(Factory, response, factor);

				ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response, alertHelper);
				AssertEquals(expected, commodityDetail.CCD_RiskStatus);
			}

			void AssertConditionsApply(CommodityRiskCalculateFactor factor, string goodsOrigin, List<string> originCodes, List<string> destinationCodes, List<string> goodsOriginCodes, string expectedNomenclatureCondition, string expectedSpecificCondition)
			{
				var commodityDetail = GetCommodityDetail(goodsOrigin);
				var response = GetResponseWithComplianceCodes(goodsOrigin, originCodes, destinationCodes, goodsOriginCodes);
				var alertHelper = new RefComplianceCommodityAlertHelper(Factory, response, factor);

				ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response, alertHelper);
				AssertEquals(expectedNomenclatureCondition, commodityDetail.NomenclatureCondition);
				AssertEquals(expectedSpecificCondition, commodityDetail.SpecificCondition);
			}

			ComplianceCommodityDetail GetCommodityDetail(string goodsOrigin)
			{
				var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
				commodityDetail.CCD_HarmonizedCode = "123456";
				commodityDetail.CCD_RN_NKOrigin = goodsOrigin;
				commodityDetail.CCD_Description = "TestGood";
				commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotApplicable;
				return commodityDetail;
			}
		}

		public void TestUpdateCommoditiesRiskStatus_ForExportJob()
		{
			InsertRefComplianceCommodityAlert();

			AssertUpdateCommoditiesRiskStatus_ForExportJob(CommodityRiskCalculateFactor.Export, ["AUPRSEXP"], ["NZPRSIMP"], ComplianceRiskStatusCodeList.Descriptions.PossibleRisk.ToString());
			AssertUpdateCommoditiesRiskStatus_ForExportJob(CommodityRiskCalculateFactor.Export, ["AUPRSEXP"], ["NZHSKIMP"], ComplianceRiskStatusCodeList.Descriptions.HighRisk.ToString());
			AssertUpdateCommoditiesRiskStatus_ForExportJob(CommodityRiskCalculateFactor.Export, ["AUHSKEXP"], ["NZPRSIMP"], ComplianceRiskStatusCodeList.Descriptions.PossibleRisk.ToString());
			AssertUpdateCommoditiesRiskStatus_ForExportJob(CommodityRiskCalculateFactor.Export, ["AUPRSEXP"], [], ComplianceRiskStatusCodeList.Descriptions.Clear.ToString());
			AssertUpdateCommoditiesRiskStatus_ForExportJob(CommodityRiskCalculateFactor.All, ["AUPRSEXP"], ["NZHSKIMP"], string.Empty);
			AssertUpdateCommoditiesRiskStatus_ForExportJob(CommodityRiskCalculateFactor.Import, ["AUPRSEXP"], ["NZHSKIMP"], string.Empty);

			void AssertUpdateCommoditiesRiskStatus_ForExportJob(CommodityRiskCalculateFactor factor, List<string> originCodes, List<string> destinationCodes, string expected)
			{
				var declarationProvider = Factory.New<DeclarationWithProvider>();
				var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentTableCode = declarationProvider.TablePrefix;
				complianceRiskStatus.COR_ParentID = declarationProvider.PK;

				StmComplianceEventHelperTest.CreateAssessmentEvent(declarationProvider, ComplianceEventList.Codes.AssessmentInitialized);

				var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
				commodityDetail.CCD_HarmonizedCode = "123456";
				commodityDetail.CCD_Description = "TestGood";
				commodityDetail.CCD_RN_NKOrigin = "";
				commodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;

				var response = GetResponseWithComplianceCodes("", originCodes, destinationCodes, []);
				var alertHelper = new RefComplianceCommodityAlertHelper(Factory, response, factor);

				ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, response, alertHelper);
				AssertEquals(expected, commodityDetail.ImportAlertsForExportJobDescription);
			}
		}

		static ComplianceCheckResponseModel GetResponseWithComplianceCodes(string goodsOrigin, List<string> originCodes, List<string> destinationCodes, List<string> goodsOriginCodes)
		{
			var originNomenclatureWideApply = originCodes.Any(u => u.Contains("PRS"))
				? BorderWiseApiHelper.ConditionApply
				: BorderWiseApiHelper.ConditionNotApply;
			var originCommoditySpecificApply = originCodes.Any(u => u.Contains("HSK"))
				? BorderWiseApiHelper.ConditionApply
				: BorderWiseApiHelper.ConditionNotApply;

			var destinationNomenclatureWideApply = destinationCodes.Any(u => u.Contains("PRS"))
				? BorderWiseApiHelper.ConditionApply
				: BorderWiseApiHelper.ConditionNotApply;
			var destinationCommoditySpecificApply = destinationCodes.Any(u => u.Contains("HSK"))
				? BorderWiseApiHelper.ConditionApply
				: BorderWiseApiHelper.ConditionNotApply;

			var goodsOriginNomenclatureWideApply = goodsOriginCodes.Any(u => u.Contains("PRS"))
				? BorderWiseApiHelper.ConditionApply
				: BorderWiseApiHelper.ConditionNotApply;
			var goodsOriginCommoditySpecificApply = goodsOriginCodes.Any(u => u.Contains("HSK"))
				? BorderWiseApiHelper.ConditionApply
				: BorderWiseApiHelper.ConditionNotApply;

			var pointPairs = new List<ComplianceCheckResponsePointPairModel>();
			pointPairs.Add(new ComplianceCheckResponsePointPairModel
			{
				OriginPoint = new ComplianceCheckResponsePointPairLocationModel
				{
					Country = "AU",
					MatchedHsCode = "123456",
					IsValidHsCode = true,
					ComplianceCodes = originCodes,
					MovementType = "export",
					NomenclatureWideConditionsApply = originNomenclatureWideApply,
					CommoditySpecificConditionsApply = originCommoditySpecificApply
				},
				DestinationPoint = new ComplianceCheckResponsePointPairLocationModel
				{
					Country = "NZ",
					MatchedHsCode = "123456",
					IsValidHsCode = true,
					ComplianceCodes = destinationCodes,
					MovementType = "import",
					NomenclatureWideConditionsApply = destinationNomenclatureWideApply,
					CommoditySpecificConditionsApply = destinationCommoditySpecificApply
				}
			});

			if (!goodsOrigin.IsNullOrEmpty())
			{
				pointPairs.Add(new ComplianceCheckResponsePointPairModel
				{
					OriginPoint = new ComplianceCheckResponsePointPairLocationModel
					{
						Country = goodsOrigin,
						MatchedHsCode = "123456",
						IsValidHsCode = true,
						ComplianceCodes = goodsOriginCodes,
						MovementType = "UNKNOWN",
						NomenclatureWideConditionsApply = goodsOriginNomenclatureWideApply,
						CommoditySpecificConditionsApply = goodsOriginCommoditySpecificApply
					}
				});
			}

			var complianceCheckResponseModel = new ComplianceCheckResponseModel
			{
				Commodities = new[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = "123456",
						GoodsDescription = "TestGood",
						Origin = new[] { goodsOrigin },
						NomenclatureWideConditionsApply = (originNomenclatureWideApply == BorderWiseApiHelper.ConditionApply || destinationNomenclatureWideApply == BorderWiseApiHelper.ConditionApply || goodsOriginNomenclatureWideApply == BorderWiseApiHelper.ConditionApply) ? BorderWiseApiHelper.ConditionApply : BorderWiseApiHelper.ConditionNotApply,
						CommoditySpecificConditionsApply = (originCommoditySpecificApply == BorderWiseApiHelper.ConditionApply || destinationCommoditySpecificApply == BorderWiseApiHelper.ConditionApply || goodsOriginCommoditySpecificApply == BorderWiseApiHelper.ConditionApply) ? BorderWiseApiHelper.ConditionApply : BorderWiseApiHelper.ConditionNotApply,
						PointPairs = pointPairs
					}
				},
				Locations = new[]
				{
					new ComplianceCheckResponseLocationModel
					{
						Country = "AU",
						IsSupportedCountry = true
					},
					new ComplianceCheckResponseLocationModel
					{
						Country = "NZ",
						IsSupportedCountry = true
					}
				}
			};
			return complianceCheckResponseModel;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void InsertRefComplianceCommodityAlert()
		{
			var sql = @"-- InsertComplianceCommodityAlert
			INSERT INTO dbo.RefComplianceCommodityAlert (
			  RCR_PK, RCR_IsActive, RCR_AlertCode, RCR_AlertDescription, RCR_AlertName, 
			  RCR_AlertType, RCR_CommodityRiskStatus, RCR_CountryRegion, RCR_PublishYear, 
			  RCR_SourceURL, RCR_TradeDirection, RCR_SystemCreateTimeUtc, RCR_SystemCreateUser, 
			  RCR_SystemLastEditTimeUtc, RCR_SystemLastEditUser
			) VALUES
			  (NEWID(), 1, 'AUPRSEXP', 'Description for alert 1', 'Alert Name 1', 'COM', 'PRS', 'AU', 2023, 'http://example.com/1', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'AUHSKEXP', 'Description for alert 2', 'Alert Name 2', 'COM', 'HSK', 'AU', 2023, 'http://example.com/2', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'NZPRSIMP', 'Description for alert 3', 'Alert Name 3', 'NOM', 'PRS', 'NZ', 2023, 'http://example.com/3', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'NZHSKIMP', 'Description for alert 4', 'Alert Name 4', 'COM', 'HSK', 'NZ', 2023, 'http://example.com/4', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'USPRSIMP', 'Description for alert 5', 'Alert Name 5', 'NOM', 'PRS', 'US', 2023, 'http://example.com/5', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'USPRSEXP', 'Description for alert 6', 'Alert Name 6', 'COM', 'PRS', 'US', 2023, 'http://example.com/6', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'USHSKIMP', 'Description for alert 7', 'Alert Name 7', 'COM', 'HSK', 'US', 2023, 'http://example.com/7', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'USHSKEXP', 'Description for alert 8', 'Alert Name 8', 'COM', 'HSK', 'US', 2023, 'http://example.com/8', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR');";

			using (var connection = Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
