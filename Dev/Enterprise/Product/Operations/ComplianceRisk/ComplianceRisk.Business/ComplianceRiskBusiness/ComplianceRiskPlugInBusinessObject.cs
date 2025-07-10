using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskPlugInBusinessObject : ComplianceRiskBusinessObject
	{
		public static ComplianceRiskPlugInBusinessObject[] GetComplianceRiskPlugInBusinessObjects(IComplianceItemRiskStatusProvider[] itemProviders)
		{
			if (itemProviders.IsNullOrEmpty() || itemProviders.Any(x => x is null))
			{
				throw new ArgumentException("Invalid item providers", nameof(itemProviders));
			}

			var factory = itemProviders[0].Factory;
			var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, itemProviders.Select(x => x.ParentID));
			var complianceRiskStatuses = factory.Load<ComplianceRiskStatus>(query).ToDictionary(x => x.COR_ParentID);

			return itemProviders.Select(provider =>
			{
				if (!complianceRiskStatuses.TryGetValue(provider.ParentID, out var complianceRiskStatus))
				{
					complianceRiskStatus = CreateNewComplianceRiskStatus(provider);
				}

				return new ComplianceRiskPlugInBusinessObject((IBusiness)provider, complianceRiskStatus);
			}).ToArray();
		}

		public ComplianceRiskPlugInBusinessObject(IBusiness hostBusinessEntity, ComplianceRiskStatus complianceRiskStatus)
			: base(hostBusinessEntity, complianceRiskStatus) => ComplianceRiskStatus.PlugInParent = this;

		public ComplianceRiskPlugInBusinessObject(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity) => ComplianceRiskStatus.PlugInParent = this;

		#region Create Helper and Register Interact Event for ISupportInteractionWithComplianceWiseCommodities

		ISupportInteractionWithComplianceWiseCommodities supportInteractionWithCommodities;

		public void CreateHelperRegisterInteractEventIfNeeded(IAssessmentHelper assessmentHelper)
		{
			if (HostBusinessEntity is ISupportInteractionWithComplianceWiseCommodities supportInteraction
				&& supportInteraction.Enabled)
			{
				supportInteractionWithCommodities = supportInteraction;
				supportInteractionWithCommodities.Helper = new InteractionWithComplianceWiseCommoditiesHelper(HostBusinessEntity);
				AssessmentHelper = assessmentHelper;

				var helper = supportInteractionWithCommodities.Helper;
				helper.SourceSideCommodities.GetCommoditiesStatusFromCpw = GetCommoditiesStatusFromCpw;
				helper.SourceSideCommodities.GetCommodityStatusFromCpw = GetCommodityStatusFromCpw;
				helper.SourceSideCommodities.CommoditiesChanged = CommoditiesChanged;
				helper.SourceSideCommodities.CommoditiesAssessmentChanged = CommoditiesAssessmentChanged;
				helper.SourceSideCommodities.InitializeAssessment = InitializeAssessment;
				helper.SourceSideCommodities.AssessmentInitialized = AssessmentInitialized;
				helper.SourceSideCommodities.ViewBorderWisePortalIfAvailable = ViewBorderWisePortalIfAvailable;
			}
		}

		async Task ViewBorderWisePortalIfAvailable(ComplianceCommodityFromSource line)
		{
			var commodityDetail = GetCommoditityStatus(line);
			if (commodityDetail != null)
			{
				await commodityDetail.ComplianceRiskStatus.ViewBorderWisePortalIfAvailable(commodityDetail);
			}
		}

		ComplianceResultFromCpw[] GetCommoditiesStatusFromCpw()
		{
			return ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>())
				.Select(detail =>
					new ComplianceResultFromCpw
					{
						HarmonizedCode = detail.CCD_HarmonizedCode,
						GroupingOrCountry = detail.CCD_CountryOrGrouping,
						GoodsDescription = detail.CCD_Description,
						OriginOfGoods = detail.CCD_RN_NKOrigin,
						LinkVisible = detail.ShowLegalBookLink,
						RiskStatus = detail.CCD_RiskStatus,
						RiskNotes = detail.CCD_AssessmentNotes,
						AssessmentInitialized = detail.AssessmentInitialized,
						HarmonizedBorderWiseTextual = detail.HarmonizedBorderWiseTextual,
						ImportAlertStatus = detail.ImportAlertsForExportJobDescription,
					})
				.ToArray();
		}

		ComplianceResultFromCpw? GetCommodityStatusFromCpw(ComplianceCommodityFromSource searchCandidate)
		{
			var detail = GetCommoditityStatus(searchCandidate);

			if (detail != null)
			{
				return new ComplianceResultFromCpw
				{
					HarmonizedCode = detail.CCD_HarmonizedCode,
					GroupingOrCountry = detail.CCD_CountryOrGrouping,
					GoodsDescription = detail.CCD_Description,
					OriginOfGoods = detail.CCD_RN_NKOrigin,
					LinkVisible = detail.ShowLegalBookLink,
					RiskStatus = detail.CCD_RiskStatus,
					RiskNotes = detail.CCD_AssessmentNotes,
					AssessmentInitialized = detail.AssessmentInitialized,
					HarmonizedBorderWiseTextual = detail.HarmonizedBorderWiseTextual,
					ImportAlertStatus = detail.ImportAlertsForExportJobDescription,
				};
			}

			return default;
		}

		ComplianceCommodityDetail GetCommoditityStatus(ComplianceCommodityFromSource searchCandidate)
		{
			var hsCode = ComplianceRiskHelper.ExtractAllAlphanumericFromHsCode(searchCandidate.HarmonizedCode).ToUpperInvariant();

			if (hsCode.IsEmpty)
			{
				return default;
			}

			var key = ComplianceRiskHelper.GetUnionKeyFromCommodity(hsCode, searchCandidate.GroupingOrCountry, searchCandidate.OriginOfGoods, GetDescriptionFromCommodity(searchCandidate));
			ComplianceRiskStatus.CommodityDetailCollection.InitializeCommoditiesLinkedToCurrentJobIfNeeded();
			ComplianceRiskStatus.CommodityDetailCollection.CommoditiesLinkedToCurrentJob.TryGetValue(key, out var detail);

			return detail;
		}

		async void CommoditiesChanged(IComplianceCommodity[] changedCommodities)
		{
			if (CommodityRiskStatusChecker != null && changedCommodities?.Length > 0)
			{
				// TODO: get changed commodities need to validate
				ComplianceRiskStatus.CommodityDetailCollection.LoadWithSuspendChanges(changedCommodities);

				var changed = changedCommodities
					.Select(u => (commodity: u, hsCode: ComplianceRiskHelper.ExtractAllAlphanumericFromHsCode(u.HarmonizedCode).ToUpperInvariant()))
					.Where(u => !u.hsCode.IsEmpty)
					.ToList();
				if (changed.Count > 0)
				{
					var commoditiesNeedToCheck = new List<ComplianceCommodityDetail>();

					supportInteractionWithCommodities.Helper.CpwSideCommodities.CommoditiesChanged?
						.Invoke(GetChangedCommoditiesInfo());

					//Trigger validation for changed commodities
					await CommodityRiskStatusChecker.CheckCommoditiesRiskStatus(commoditiesNeedToCheck.ToArray());

					ComplianceResultFromCpw[] GetChangedCommoditiesInfo()
					{
						foreach (var searchCandidate in changed)
						{
							var key = ComplianceRiskHelper.GetUnionKeyFromCommodity(searchCandidate.hsCode, searchCandidate.commodity.GroupingOrCountry, searchCandidate.commodity.Origin, searchCandidate.commodity.GoodsDescription);

							if (ComplianceRiskStatus.CommodityDetailCollection.CommoditiesLinkedToCurrentJob != null && ComplianceRiskStatus.CommodityDetailCollection.CommoditiesLinkedToCurrentJob.TryGetValue(key, out var detail))
							{
								commoditiesNeedToCheck.Add(detail);
							}
						}

						return commoditiesNeedToCheck.Select(detail =>
						new ComplianceResultFromCpw
						{
							HarmonizedCode = detail.CCD_HarmonizedCode,
							GroupingOrCountry = detail.CCD_CountryOrGrouping,
							GoodsDescription = detail.CCD_Description,
							OriginOfGoods = detail.CCD_RN_NKOrigin,
							LinkVisible = detail.ShowLegalBookLink,
							RiskStatus = detail.CCD_RiskStatus,
							RiskNotes = detail.CCD_AssessmentNotes,
							HarmonizedBorderWiseTextual = detail.HarmonizedBorderWiseTextual,
							AssessmentInitialized = detail.AssessmentInitialized,
							ImportAlertStatus = detail.ImportAlertsForExportJobDescription,
						}).ToArray();
					}
				}
			}
		}

		void CommoditiesAssessmentChanged(ComplianceCommodityFromSource[] changedCommodities)
		{
			if (changedCommodities?.Length > 0)
			{
				var changed = changedCommodities
					.Select(u => (commodity: u, hsCode: ComplianceRiskHelper.ExtractAllAlphanumericFromHsCode(u.HarmonizedCode).ToUpperInvariant()))
					.Where(u => !u.hsCode.IsEmpty)
					.ToList();

				if (changed.Count > 0)
				{
					ComplianceRiskStatus.CommodityDetailCollection.InitializeCommoditiesLinkedToCurrentJobIfNeeded();

					foreach (var searchCandidate in changed)
					{
						var description = GetDescriptionFromCommodity(searchCandidate.commodity);
						var key = ComplianceRiskHelper.GetUnionKeyFromCommodity(searchCandidate.hsCode, searchCandidate.commodity.GroupingOrCountry, searchCandidate.commodity.OriginOfGoods, description);

						if (ComplianceRiskStatus.CommodityDetailCollection.CommoditiesLinkedToCurrentJob.TryGetValue(key, out var detail))
						{
							detail.CCD_RiskStatus = searchCandidate.commodity.RiskStatus;
							detail.CCD_AssessmentNotes = searchCandidate.commodity.RiskNotes;
						}
					}
				}
			}
		}

		ZString GetDescriptionFromCommodity(ComplianceCommodityFromSource commodity)
		{
			return commodity.GoodsDescription.Length > ComplianceCommodityDetailSchema.CCD_Description.MaxLength
				? commodity.GoodsDescription.Substring(0, ComplianceCommodityDetailSchema.CCD_Description.MaxLength)
				: commodity.GoodsDescription;
		}

		async void InitializeAssessment()
		{
			if (AssessmentHelper != null && ComplianceItemRiskStatusProvider.ComplianceRiskSupport.IsSupportInitialization())
			{
				await AssessmentHelper.InitializeAssessment();
			}
		}

		bool AssessmentInitialized()
		{
			if (AssessmentHelper != null && ComplianceRiskStatus != null)
			{
				return ComplianceRiskStatus.IsAssessmentInitialized;
			}

			return false;
		}

		public IAssessmentHelper AssessmentHelper { get; set; }

		#endregion

		internal override void ReplaceComplianceRiskStatus(ComplianceRiskStatus riskStatus)
		{
			base.ReplaceComplianceRiskStatus(riskStatus);
			ComplianceRiskStatus.PlugInParent = this;
		}

		public ComplianceSnapShot SnapShot { get; set; }

		public Action<bool> ComplianceRiskSpinnerIndicatorVisibility { get; set; }

		#region Risks Status Properties

		public ZString OverallRiskDescription => ComplianceRiskStatus.Lookups.OverallRiskStatusCodes[ComplianceRiskStatus.COR_OverallRisk].Description;

		public ZPropertyInfo OverallRiskDescriptionInfo => GetZPropertyInfo(nameof(OverallRiskDescription));

		public ZString PartyRiskDescription => ComplianceRiskStatus.Lookups.PartyRiskStatusCodes[ComplianceRiskStatus.COR_PartyRisk].Description;

		public ZPropertyInfo PartyRiskDescriptionInfo => GetZPropertyInfo(nameof(PartyRiskDescription));

		public ZString LocationRiskDescription => ComplianceRiskStatus.Lookups.LocationRiskStatusCodes[ComplianceRiskStatus.COR_LocationRisk].Description;

		public ZPropertyInfo LocationRiskDescriptionInfo => GetZPropertyInfo(nameof(LocationRiskDescription));

		public ZString CommodityRiskDescription => ComplianceRiskStatus.Lookups.CommodityRiskStatusCodes[ComplianceRiskStatus.COR_CommodityRisk].Description;

		public ZPropertyInfo CommodityRiskDescriptionInfo => GetZPropertyInfo(nameof(CommodityRiskDescription));

		#endregion

		#region Freight Movement Restriction

		public string OverallRiskRegistryInfo => ComplianceRiskStatus.COR_OverallRisk == Codes.OverrideClear ||
			(ComplianceRiskStatus.COR_OverallRisk.HasOverallRisk() &&
			HostBusinessEntity is ICreditControlledDocumentDelivery documentDelivery &&
			!documentDelivery.IsDPSFreightMovementRestricted)
			? Res.GetString("2ab23e08-c1a3-480f-ac84-03d8502bc7c5", "Document delivery is not blocked") : string.Empty;

		#endregion

#if DEBUG
		public ISupportInteractionWithComplianceWiseCommodities GetSupportInteractionWithComplianceWiseCommodities => supportInteractionWithCommodities;
#endif
	}
}
