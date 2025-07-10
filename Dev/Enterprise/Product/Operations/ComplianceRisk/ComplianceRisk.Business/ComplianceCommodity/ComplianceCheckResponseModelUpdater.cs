using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using static Enterprise.ComplianceRisk.Business.BorderWiseApiHelper;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceCheckResponseModelUpdater
	{
		public static ComplianceCheckResponseCommodityModel[] UpdateCommoditiesRiskStatus(ComplianceCommodityDetail commodityDetail, ComplianceCheckResponseModel responseModel, RefComplianceCommodityAlertHelper alertHelper)
		{
			var responseCommodities = responseModel.Commodities
				.Where(c => c.HsCode == commodityDetail.CCD_HarmonizedCode
					&& commodityDetail.CCD_RN_NKOrigin == (c.Origin?.FirstOrDefault() ?? ZString.Empty)
					&& commodityDetail.CCD_Description.EqualsIgnoringCase(c.GoodsDescription ?? ZString.Empty)).ToArray();

			if (responseCommodities.Length > 0)
			{
				var nomenclatureCondition = false;
				var specificCondition = false;
				var matchedHsCode = string.Empty;
				var commodityCountryCodes = new List<string>();
				var isValidHsCode = false;
				var unsupportedCountryCodes = responseModel.Locations?.Where(u => !u.IsSupportedCountry).Select(u => u.Country).ToHashSet();

				foreach (var responseCommodity in responseCommodities)
				{
					if (alertHelper.CalculateFactor == CommodityRiskCalculateFactor.All)
					{
						nomenclatureCondition = nomenclatureCondition || responseCommodity.NomenclatureWideConditionsApply == ConditionApply;
						specificCondition = specificCondition || responseCommodity.CommoditySpecificConditionsApply == ConditionApply;
					}

					if (responseCommodity.PointPairs != null)
					{
						foreach (var pointPair in responseCommodity.PointPairs)
						{
							if (alertHelper.CalculateFactor == CommodityRiskCalculateFactor.Import)
							{
								nomenclatureCondition = nomenclatureCondition
													|| pointPair.DestinationPoint?.NomenclatureWideConditionsApply == ConditionApply
													|| alertHelper.IsGoodsOriginPoint(pointPair.OriginPoint) && pointPair.OriginPoint.NomenclatureWideConditionsApply == ConditionApply;

								specificCondition = specificCondition
													|| pointPair.DestinationPoint?.CommoditySpecificConditionsApply == ConditionApply
													|| alertHelper.IsGoodsOriginPoint(pointPair.OriginPoint) && pointPair.OriginPoint.CommoditySpecificConditionsApply == ConditionApply;
							}
							else if (alertHelper.CalculateFactor == CommodityRiskCalculateFactor.Export)
							{
								nomenclatureCondition = nomenclatureCondition ||
														pointPair.OriginPoint?.NomenclatureWideConditionsApply == ConditionApply;

								specificCondition = specificCondition ||
													pointPair.OriginPoint?.CommoditySpecificConditionsApply == ConditionApply;
							}

							isValidHsCode = isValidHsCode
								|| pointPair.OriginPoint != null && pointPair.OriginPoint.IsValidHsCode
								|| pointPair.DestinationPoint != null && pointPair.DestinationPoint.IsValidHsCode;

							commodityCountryCodes.AddRange(new[] { pointPair.OriginPoint?.Country, pointPair.DestinationPoint?.Country }
								.Where(u => !string.IsNullOrEmpty(u)));

							if (string.IsNullOrEmpty(matchedHsCode))
							{
								var firstMatchedHsCode = new[] { pointPair.OriginPoint?.MatchedHsCode, pointPair.DestinationPoint?.MatchedHsCode }
								.FirstOrDefault(v => !string.IsNullOrEmpty(v));

								if (!string.IsNullOrEmpty(firstMatchedHsCode))
								{
									matchedHsCode = firstMatchedHsCode;
								}
							}
						}
					}
				}

				commodityDetail.CCD_NomenclatureCondition = nomenclatureCondition;
				commodityDetail.CCD_SpecificCondition = specificCondition;
				commodityDetail.Countries = commodityCountryCodes.Distinct().Select(c => (c, !unsupportedCountryCodes.Contains(c))).ToArray();
				commodityDetail.MatchedHsCode = matchedHsCode;
				commodityDetail.IsValidHsCode = isValidHsCode;

				if (!commodityDetail.CCD_RiskStatus.HasBlockedOrReleased())
				{
					if (commodityDetail.HasMatchedHsCode)
					{
						alertHelper.SetStatusBasedOnCommodityAlerts(commodityDetail, responseCommodities);
					}
					else if (commodityDetail.AllCountriesUnsupported && commodityDetail.IsValidHsCode)
					{
						commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PossibleRisk;
					}
					else
					{
						commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;
					}
				}

				if (commodityDetail.HasMatchedHsCode)
				{
					alertHelper.SetImportAlertsForExportJobIfNeeded(commodityDetail, responseCommodities);
				}
			}

			return responseCommodities;
		}

		public static void UpdateCommodityDetailExtension(ComplianceCommodityDetail commodity)
		{
			commodity.BorderWiseCheckStatus = commodity.HasMatchedHsCode ? BorderWiseCheckStatus.Viewable : BorderWiseCheckStatus.NotViewable;

			commodity.BorderWiseCheckInProgress = false;
		}
	}
}
