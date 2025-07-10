using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.ComplianceRisk.Business
{
	static class ComplianceMaterialChangesDetector
	{
		internal static void ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges(this ComplianceRiskBusinessObject complianceRiskBizO)
		{
			if ((complianceRiskBizO.ComplianceCommodityRiskStatusProvider != null)
				&& complianceRiskBizO.IsCommodityRiskAssessable())
			{
				var pointPairInfo = complianceRiskBizO.ComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo;
				if (pointPairInfo.SupportAssessmentByBorderWise)
				{
					var (requestModel, commodities) = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskBizO, pointPairInfo);

					if (complianceRiskBizO.ComplianceRiskStatus.IsAssessmentInitialized
						&& complianceRiskBizO.ComplianceMaterialChangesSnapshot != null)
					{
						if (JobHasMaterialChanges(requestModel, complianceRiskBizO.ComplianceMaterialChangesSnapshot))
						{
							commodities.ForEach(UpdateCommodityRiskStatusToNotChecked);
						}
						else
						{
							requestModel.Commodities.ForEach(commodityModel =>
							{
								var commodity = commodities.FirstOrDefault(u => u.CCD_HarmonizedCode == commodityModel.HsCode
									&& u.CCD_RN_NKOrigin.Equals(commodityModel.Origin.FirstOrDefault() ?? ZString.Empty)
									&& u.CCD_Description.EqualsIgnoringCase(commodityModel.GoodsDescription));

								if (commodity == null)
								{
									return;
								}

								var commoditiesWithSameHsCodeAndOrigin = GetCommoditiesWithSameHsCodeAndOrigin(commodityModel, complianceRiskBizO.ComplianceMaterialChangesSnapshot.Commodities);
								// Material Changed
								if (commoditiesWithSameHsCodeAndOrigin.Length == 0)
								{
									UpdateCommodityRiskStatusToNotChecked(commodity);
								}
								// Current Status Is BLK or REL and Goods Description changed
								else if (commodity.CCD_RiskStatus.HasBlockedOrReleased() && !commoditiesWithSameHsCodeAndOrigin.Any(u => commodity.CCD_Description.EqualsIgnoringCase(u.GoodsDescription)))
								{
									UpdateCommodityRiskStatusToNotChecked(commodity);
								}
							});
						}
					}

					complianceRiskBizO.ComplianceMaterialChangesSnapshot = requestModel;
				}
			}
		}

		public static void UpdateComplianceMaterialChangeSnapshot(this ComplianceRiskBusinessObject complianceRiskBizO, ComplianceCheckRequestModel requestModel, bool updateSnapshot)
		{
			if (updateSnapshot)
			{
				// Directly update the snapshot
				complianceRiskBizO.ComplianceMaterialChangesSnapshot = requestModel;
			}
			else if (complianceRiskBizO.ComplianceMaterialChangesSnapshot != null)
			{
				requestModel.Commodities.ForEach(commodity =>
				{
					var existingCommodity = complianceRiskBizO
					.ComplianceMaterialChangesSnapshot
					.Commodities
					.FirstOrDefault(u => u.HsCode == commodity.HsCode
										&& u.Origin.OriginIsTheSame(commodity.Origin)
										&& u.GoodsDescription.EqualIgnoreCase(commodity.GoodsDescription));
					if (existingCommodity == null)
					{
						complianceRiskBizO.ComplianceMaterialChangesSnapshot.Commodities = complianceRiskBizO.ComplianceMaterialChangesSnapshot.Commodities.Concat(new[] { commodity }).ToArray();
					}
				});
			}
		}

		public static void InitializeComplianceMaterialChangesSnapshotForUniversalDataTransfer(this ComplianceRiskBusinessObject complianceRiskBizO)
		{
			if (complianceRiskBizO.ComplianceCommodityRiskStatusProvider != null)
			{
				var pointPairInfo = complianceRiskBizO.ComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo;
				if (pointPairInfo != null)
				{
					complianceRiskBizO.ComplianceMaterialChangesSnapshot = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskBizO, pointPairInfo).RequestModel;
				}
			}
		}

		static void UpdateCommodityRiskStatusToNotChecked(ComplianceCommodityDetail commodity)
		{
			commodity.NeedResetStatus = false;
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
		}

		static ComplianceCheckRequestCommodityModel[] GetCommoditiesWithSameHsCodeAndOrigin(ComplianceCheckRequestCommodityModel currentCommodity, ICollection<ComplianceCheckRequestCommodityModel> snapshotCommodities)
		{
			var snapshotCommoditiesWithSameCodeAndOrigin = snapshotCommodities
				.Where(u => u.HsCode == currentCommodity.HsCode
							&& u.Origin.OriginIsTheSame(currentCommodity.Origin));

			return snapshotCommoditiesWithSameCodeAndOrigin.ToArray();
		}

		static bool JobHasMaterialChanges(ComplianceCheckRequestModel currentRequestModel, ComplianceCheckRequestModel snapshotRequestModel)
		{
			return snapshotRequestModel.PointPairs != null
				&& currentRequestModel.PointPairs != null
				&& (currentRequestModel.PointPairs.Count != snapshotRequestModel.PointPairs.Count
				|| !currentRequestModel.PointPairs
					.All(u => snapshotRequestModel.PointPairs
					.Any(v => v.OriginPoint?.Country == u.OriginPoint?.Country
							&& v.DestinationPoint?.Country == u.DestinationPoint?.Country)));
		}

		static bool EqualIgnoreCase(this string strA, string strB)
		{
			return strA.Equals(strB, StringComparison.OrdinalIgnoreCase);
		}

		static bool OriginIsTheSame(this ICollection<string> originA, ICollection<string> originB)
		{
			return originA == null && originB == null
				|| originA != null && originB != null
					&& originA.Count == originB.Count
					&& !originA.Except(originB).Any();
		}
	}
}
