using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class RefComplianceCommodityAlertHelperTest : TestCaseWithFactory
	{
		public void TestSetStatusBasedOnCommodityAlerts()
		{
			UpdateRefComplianceCommodityAlert();
			SetStatusBasedOnCommodityAlerts("AU", "", "AU", "", "US", "TEST1", "PRS");
			SetStatusBasedOnCommodityAlerts("CA", "TEST4", "CA", "TEST4", "AU", "", "PRS");
			SetStatusBasedOnCommodityAlerts("CA", "TEST4", "AU", "", "US", "", "PRS");
			SetStatusBasedOnCommodityAlerts("AU", "", "AU", "", "AU", "", "CLR");
			SetStatusBasedOnCommodityAlerts("AU", "", "AU", "", "FR", "TEST3", "HSK");
			SetStatusBasedOnCommodityAlerts("CA", "", "CA", "TEST4", "FR", "TEST3", "HSK");
		}

		#region Implementation

		void SetStatusBasedOnCommodityAlerts(string origin, string goodsComplianceCode, string originPointCountry, string originPointComplianceCodes, string destinationPointCountry, string destinationPointComplianceCodes, ZString expectRiskStatus)
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RN_NKOrigin = origin;
			commodityDetail.CCD_Description = "Test";
			var alertHelper = new RefComplianceCommodityAlertHelper(Factory, GetComplianceCheckResponseModelForTest(origin, goodsComplianceCode, originPointCountry, originPointComplianceCodes, destinationPointCountry, destinationPointComplianceCodes));
			alertHelper.SetStatusBasedOnCommodityAlerts(commodityDetail, [.. GetComplianceCheckResponseModelForTest(origin, goodsComplianceCode, originPointCountry, originPointComplianceCodes, destinationPointCountry, destinationPointComplianceCodes).Commodities]);

			AssertEquals(expectRiskStatus, commodityDetail.CCD_RiskStatus);
		}

		public void TestSetImportAlertsForExportJobIfNeeded()
		{
			UpdateRefComplianceCommodityAlert();
			AssertSetImportAlertsForExportJobIfNeeded(true, CommodityRiskCalculateFactor.All, string.Empty);
			AssertSetImportAlertsForExportJobIfNeeded(true, CommodityRiskCalculateFactor.Import, string.Empty);
			AssertSetImportAlertsForExportJobIfNeeded(true, CommodityRiskCalculateFactor.Export, ComplianceRiskStatusCodeList.Descriptions.HighRisk.ToString());
			AssertSetImportAlertsForExportJobIfNeeded(false, CommodityRiskCalculateFactor.Export, ComplianceRiskStatusCodeList.Descriptions.AssessmentNotInitialized.ToString());

			void AssertSetImportAlertsForExportJobIfNeeded(bool initialized, CommodityRiskCalculateFactor factor, ZString expectRiskStatus)
			{
				var declaration = Factory.New<ComplianceCommodityDetailCollectionTest.DeclarationWithProvider>();
				var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = declaration.PK;
				complianceRiskStatus.COR_ParentTableCode = declaration.TablePrefix;
				if (initialized)
				{
					StmComplianceEventHelperTest.CreateAssessmentEvent(declaration, ComplianceEventList.Codes.AssessmentInitialized);
				}

				var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
				commodityDetail.CCD_HarmonizedCode = "123456";
				commodityDetail.CCD_Description = "Test";
				commodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;

				var alertHelper = new RefComplianceCommodityAlertHelper(Factory, GetComplianceCheckResponseModelForTest("", "", "CA", "TEST4", "FR", "TEST3"), factor);
				alertHelper.SetImportAlertsForExportJobIfNeeded(commodityDetail, [.. GetComplianceCheckResponseModelForTest("", "", "CA", "TEST4", "FR", "TEST3").Commodities]);

				AssertEquals(expectRiskStatus, commodityDetail.ImportAlertsForExportJobDescription);
			}
		}

		ComplianceCheckResponseModel GetComplianceCheckResponseModelForTest(string origin, string goodsComplianceCode, string originPointCountry, string originPointComplianceCodes, string destinationPointCountry, string destinationPointComplianceCodes)
		{
			return new ComplianceCheckResponseModel
			{
				Commodities = new ComplianceCheckResponseCommodityModel[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						Origin = new string[] { origin },
						PointPairs = new ComplianceCheckResponsePointPairModel[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									Country = originPointCountry,
									ComplianceCodes = string.IsNullOrEmpty(originPointComplianceCodes) ? Array.Empty<string>() : new string[] { originPointComplianceCodes }
								},
								DestinationPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									Country = destinationPointCountry,
									ComplianceCodes = string.IsNullOrEmpty(destinationPointComplianceCodes) ? Array.Empty<string>() : new string[] { destinationPointComplianceCodes }
								}
							},
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									Country = origin,
									ComplianceCodes = string.IsNullOrEmpty(goodsComplianceCode) ? Array.Empty<string>() : new string[] { goodsComplianceCode }
								}
							}
						}
					}
				}
			};
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
			  (NEWID(), 1, 'TEST2', 'Description for alert 2', 'Alert Name 2', 'COM', 'HSK', 'CN', 2022, 'http://example.com/2', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'TEST3', 'Description for alert 3', 'Alert Name 3', 'COM', 'HSK', 'EU', 2021, 'http://example.com/3', 'IMP', GETDATE(), 'USR', GETDATE(), 'USR'),
			  (NEWID(), 1, 'TEST4', 'Description for alert 4', 'Alert Name 2', 'NOM', 'PRS', 'CA', 2022, 'http://example.com/4', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR');";

			using (var connection = Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
