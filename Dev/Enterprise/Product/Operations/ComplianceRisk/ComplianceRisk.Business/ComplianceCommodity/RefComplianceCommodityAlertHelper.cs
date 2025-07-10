using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using DirectionCodes = Enterprise.MasterFiles.Business.RefComplianceCommodityAlertDirectionList.Codes;
using RiskStatusCodes = Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Codes;

namespace Enterprise.ComplianceRisk.Business
{
	public class RefComplianceCommodityAlertHelper
	{
		readonly BusinessObjectFactory factory;
		public CommodityRiskCalculateFactor CalculateFactor { get; }

		public RefComplianceCommodityAlertHelper(BusinessObjectFactory factory, ComplianceCheckResponseModel complianceCheckResponse, CommodityRiskCalculateFactor calculateFactor = CommodityRiskCalculateFactor.All)
		{
			this.factory = factory;
			CalculateFactor = calculateFactor;

			InitializeData(complianceCheckResponse);
		}

		public void SetStatusBasedOnCommodityAlerts(ComplianceCommodityDetail commodityDetail, ComplianceCheckResponseCommodityModel[] responseCommodities)
		{
			commodityDetail.CCD_RiskStatus = GetRiskStatusByConditions(GetConditionsByCalculateFactor(responseCommodities));
		}

		public void SetImportAlertsForExportJobIfNeeded(ComplianceCommodityDetail commodityDetail, ComplianceCheckResponseCommodityModel[] responseCommodities)
		{
			if (CalculateFactor == CommodityRiskCalculateFactor.Export)
			{
				commodityDetail.ImportAlertsForExportJobDescription = GetRiskStatusByConditions(GetExportJobImportAlertsConditions(responseCommodities));
			}
			else
			{
				commodityDetail.ImportAlertsForExportJobDescription = string.Empty;
			}
		}

		IEnumerable<KeyInformation> GetExportJobImportAlertsConditions(ComplianceCheckResponseCommodityModel[] responseCommodities)
		{
			return responseCommodities
				.SelectMany(commodity => commodity.PointPairs ?? Enumerable.Empty<ComplianceCheckResponsePointPairModel>())
					.SelectMany(u => (u.DestinationPoint?.ComplianceCodes ?? Enumerable.Empty<string>())
						.Select(v => GetKeyAndAddEuropeanUnionIfNeeded(u.DestinationPoint.Country, DirectionCodes.Import, v)));
		}

		string GetRiskStatusByConditions(IEnumerable<KeyInformation> conditions)
		{
			var hasAlertAndSetToPRS = false;
			foreach (var condition in conditions)
			{
				if (PossibleRiskCommodityAlerts.TryGetValue(condition.OriginCountryRegionKey, out var commodityAlert) || PossibleRiskCommodityAlerts.TryGetValue(condition.EuropeanUnionKey, out commodityAlert))
				{
					hasAlertAndSetToPRS = true;
				}
				else
				{
					return RiskStatusCodes.HighRisk;
				}
			}

			return hasAlertAndSetToPRS ? RiskStatusCodes.PossibleRisk : RiskStatusCodes.Clear;
		}

		IEnumerable<KeyInformation> GetConditionsByCalculateFactor(ComplianceCheckResponseCommodityModel[] responseCommodities)
		{
			var conditions = Enumerable.Empty<KeyInformation>();

			switch (CalculateFactor)
			{
				case CommodityRiskCalculateFactor.All:
					conditions = responseCommodities
						.SelectMany(commodity => commodity.PointPairs ?? Enumerable.Empty<ComplianceCheckResponsePointPairModel>())
							.SelectMany(u => (u.OriginPoint?.ComplianceCodes ?? Enumerable.Empty<string>())
								.Select(v => GetKeyAndAddEuropeanUnionIfNeeded(u.OriginPoint.Country, DirectionCodes.Export, v))
							.Concat((u.DestinationPoint?.ComplianceCodes ?? Enumerable.Empty<string>())
								.Select(v => GetKeyAndAddEuropeanUnionIfNeeded(u.DestinationPoint.Country, DirectionCodes.Import, v))));
					break;
				case CommodityRiskCalculateFactor.Import:
					conditions = responseCommodities
						.SelectMany(commodity => commodity.PointPairs ?? Enumerable.Empty<ComplianceCheckResponsePointPairModel>())
							.SelectMany(u => (IsGoodsOriginPoint(u.OriginPoint) ? (u.OriginPoint.ComplianceCodes ?? Enumerable.Empty<string>()) : Enumerable.Empty<string>())
								.Select(v => GetKeyAndAddEuropeanUnionIfNeeded(u.OriginPoint.Country, DirectionCodes.Export, v))
							.Concat((u.DestinationPoint?.ComplianceCodes ?? Enumerable.Empty<string>())
								.Select(v => GetKeyAndAddEuropeanUnionIfNeeded(u.DestinationPoint.Country, DirectionCodes.Import, v))));
					break;
				case CommodityRiskCalculateFactor.Export:
					conditions = responseCommodities
						.SelectMany(commodity => commodity.PointPairs ?? Enumerable.Empty<ComplianceCheckResponsePointPairModel>())
							.SelectMany(u => (u.OriginPoint?.ComplianceCodes ?? Enumerable.Empty<string>())
								.Select(v => GetKeyAndAddEuropeanUnionIfNeeded(u.OriginPoint.Country, DirectionCodes.Export, v)));
					break;
			}

			return conditions;
		}

		internal bool IsGoodsOriginPoint(ComplianceCheckResponsePointPairLocationModel point)
		{
			if (point == null || point.MovementType == null)
			{
				return false;
			}

			return !point.MovementType.Equals(ComplianceCheckRequestModelBuilder.Export, StringComparison.OrdinalIgnoreCase)
					&& !point.MovementType.Equals(ComplianceCheckRequestModelBuilder.Import, StringComparison.OrdinalIgnoreCase);
		}

		void InitializeData(ComplianceCheckResponseModel complianceCheckResponse)
		{
			var countryRegionCodes = complianceCheckResponse
				.Commodities
					.SelectMany(u => (u.PointPairs ?? Enumerable.Empty<ComplianceCheckResponsePointPairModel>())
					.Select(v => v.OriginPoint?.Country)
					.Concat(u.PointPairs.Select(v => v.DestinationPoint?.Country))
					.Concat(u.Origin ?? Enumerable.Empty<string>()))
					.Where(u => !string.IsNullOrEmpty(u))
					.ToHashSet();

			Countries = factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, countryRegionCodes)).ToDictionary(u => u.RN_Code.ToString());

			if (Countries.Any(u => u.Value.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion))
			{
				countryRegionCodes.Add(Core.Constants.CountryCodes.EuropeanUnion);
			}

			var allRelatedRefComplianceCommodityAlerts = factory.Load<RefComplianceCommodityAlert>(new ZQuery(RefComplianceCommodityAlertSchema.RCR_CountryRegion, countryRegionCodes)
				.AddToFilter(new ZQuery(RefComplianceCommodityAlertSchema.RCR_IsActive, true))
				.AddToFilter(new ZQuery(RefComplianceCommodityAlertSchema.RCR_CommodityRiskStatus, RiskStatusCodes.PossibleRisk)));

			PossibleRiskCommodityAlerts = allRelatedRefComplianceCommodityAlerts.ToDictionary(u => $"{u.RCR_CountryRegion}|{u.RCR_TradeDirection}|{u.RCR_AlertCode}");
		}

		KeyInformation GetKeyAndAddEuropeanUnionIfNeeded(string countryCode, string direction, string condition)
		{
			var countryRegionKey = $"{countryCode}|{direction}|{condition}";

			if (Countries.TryGetValue(countryCode, out var country) && country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion)
			{
				return new KeyInformation(countryRegionKey, $"{Core.Constants.CountryCodes.EuropeanUnion}|{direction}|{condition}");
			}

			return new KeyInformation(countryRegionKey, string.Empty);
		}

		class KeyInformation
		{
			public KeyInformation(string originCountryRegionKey, string europeanUnionKey)
			{
				OriginCountryRegionKey = originCountryRegionKey;
				EuropeanUnionKey = europeanUnionKey;
			}

			public string OriginCountryRegionKey { get; set; }
			public string EuropeanUnionKey { get; set; }
		}

		Dictionary<string, RefComplianceCommodityAlert> PossibleRiskCommodityAlerts { get; set; }
		Dictionary<string, RefCountry> Countries { get; set; }
	}
}
