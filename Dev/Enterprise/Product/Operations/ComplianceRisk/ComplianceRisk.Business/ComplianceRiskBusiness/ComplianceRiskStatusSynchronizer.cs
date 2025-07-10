using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business.Messaging;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceRiskStatusSynchronizer
	{
		public static bool Synchronize(ComplianceRiskPlugInBusinessObject complianceRisk) => RecalculateComplianceRiskStatus(complianceRisk, synchronizeEnforceOnFormLoad: false);

		public static bool SynchronizeOnUserControlShownAndSaveIfNeeded(ComplianceRiskPlugInBusinessObject complianceRisk)
		{
			var hostBusinessEntityHasChanges = complianceRisk.HostBusinessEntity.HasChanges;

			var riskStatusChanged = RecalculateComplianceRiskStatus(complianceRisk, synchronizeEnforceOnFormLoad: false, shouldRunAdditionalComplianceProcess: false);

			if (!hostBusinessEntityHasChanges && complianceRisk.ComplianceRiskStatus.HasChanges)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
				{
					complianceRisk.ComplianceRiskStatus.Factory.Save();
				}, null);
			}

			return riskStatusChanged;
		}

		/// <summary>
		/// Synchronize all compliance risk status created/updated by the UXML Data Object Reader or Compliance Risk Assessment (CRA) Service Task"
		/// </summary>
		public static void Synchronize(ComplianceRiskBusinessObject complianceRisk, bool shouldRunAdditionalComplianceProcess = true)
		{
			RecalculateComplianceRiskStatus(complianceRisk, (_) => complianceRisk.ComplianceRiskStatus.COR_OverallRisk != Codes.OverrideClear, shouldRunAdditionalComplianceProcess);
		}

		public static bool SynchronizeAndSaveIfNeeded(ComplianceRiskPlugInBusinessObject pluginBizO, bool synchronizeEnforceOnFormLoad)
		{
			var result = RecalculateComplianceRiskStatus(pluginBizO, synchronizeEnforceOnFormLoad);
			if (pluginBizO.ComplianceRiskStatus.HasChanges)
			{
				if (synchronizeEnforceOnFormLoad)
				{
					try
					{
						pluginBizO.ComplianceRiskStatus.Factory.Save();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						// Ignore the save exception on form load
					}
				}
				else
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(pluginBizO.ComplianceRiskStatus.Factory.Save, null);
				}
			}

			return result;
		}

		public static void SynchronizeForConvertShipmentFirstLoadIfNeeded(ComplianceRiskPlugInBusinessObject pluginBizO, bool synchronizeEnforceOnFormLoad)
		{
			if (pluginBizO.ComplianceRiskStatus.CopyFromBooking && pluginBizO.ComplianceRiskStatus.CopyFromBookingFirstLoaded && pluginBizO.ComplianceRiskStatus.COR_OverallRisk == Codes.OverrideClear)
			{
				RecalculateComplianceRiskStatus(pluginBizO, synchronizeEnforceOnFormLoad);
				pluginBizO.ComplianceRiskStatus.COR_OverallRisk = Codes.OverrideClear;
				pluginBizO.ComplianceRiskStatus.CopyFromBookingFirstLoaded = false;
			}
		}

		static ComplianceSnapShot RecalculateComplianceRiskStatus(ComplianceRiskBusinessObject complianceRiskBizO, Func<ComplianceSnapShot, bool> shouldUpdateOverallRiskStatus, bool shouldRunAdditionalComplianceProcess = true)
		{
			complianceRiskBizO.RefreshData();

			complianceRiskBizO.ResetCommodityRiskStatusHasJobOrCommodityMaterialChanges();

			if (shouldRunAdditionalComplianceProcess && complianceRiskBizO.HostBusinessEntity.ComplianceRiskAssessmentFeatureEnabledForJob())
			{
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskBizO);

				// publish EDI message if any commodity is not checked
				if (complianceRiskBizO.ComplianceRiskStatus.IsAssessmentInitialized
					&& complianceRiskBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Any(commodity => commodity.CommodityType != CommodityType.RelatedJobLink
					&& !commodity.CCD_HarmonizedCode.IsEmpty
					&& commodity.CCD_RiskStatus.ToString() == Codes.NotChecked))
				{
					ComplianceRiskAssessmentMessagePublisher.Publish((BusinessObject)complianceRiskBizO.HostBusinessEntity);
				}
				CommodityEventHelper.AddCommodityAdditionAndDeletionLogIfNeeded(complianceRiskBizO.ComplianceRiskStatus);
			}

			var complianceStatus = complianceRiskBizO.ComplianceRiskStatus;
			complianceStatus.COR_PartyRisk = GetPartyRisk(complianceRiskBizO.Parties, out var snapShotParties);
			complianceStatus.COR_LocationRisk = GetLocationRisk(complianceRiskBizO.Locations, out var snapShotLocations);
			complianceStatus.COR_CommodityRisk = GetCommodityRisk(
				complianceRiskBizO,
				out var snapShotCommodities,
				out var snapShotIsInternational,
				out var snapShotSubProvidersCommodityRiskStatus);

			var currentSnapShot = new ComplianceSnapShot(snapShotParties, snapShotLocations, snapShotCommodities, snapShotIsInternational, snapShotSubProvidersCommodityRiskStatus);

			if (shouldUpdateOverallRiskStatus(currentSnapShot))
			{
				complianceStatus.COR_OverallRisk = complianceStatus.GetOverallRiskStatus();
			}

			complianceRiskBizO.ComplianceRiskStatus.COR_JobEndDate = complianceRiskBizO.ComplianceItemRiskStatusProvider.JobTime.JobEndDate.UtcToDateTimeOffset();

			complianceRiskBizO.JobEntityCacheSyncCore();

			return currentSnapShot;
		}

		static bool RecalculateComplianceRiskStatus(ComplianceRiskPlugInBusinessObject compliancePluginBizObject, bool synchronizeEnforceOnFormLoad, bool shouldRunAdditionalComplianceProcess = true)
		{
			if (!synchronizeEnforceOnFormLoad)
			{
				compliancePluginBizObject.ReloadSafeComplianceRiskStatusWithRelatedParties();
			}

			var complianceStatus = compliancePluginBizObject.ComplianceRiskStatus;
			var rawComplianceStatuses = (complianceStatus.COR_OverallRisk, complianceStatus.COR_PartyRisk, complianceStatus.COR_LocationRisk, complianceStatus.COR_CommodityRisk);

			var beforeSnapShot = compliancePluginBizObject.SnapShot;

			compliancePluginBizObject.SnapShot = RecalculateComplianceRiskStatus(
				compliancePluginBizObject,
				(snapShot) =>
					(compliancePluginBizObject.HostBusinessEntity.HasChanges || complianceStatus.HasChanges || synchronizeEnforceOnFormLoad)
					&& (!synchronizeEnforceOnFormLoad || complianceStatus.COR_OverallRisk != Codes.OverrideClear)
					&& snapShot != compliancePluginBizObject.SnapShot
					&& !(complianceStatus.COR_OverallRisk == Codes.OverrideClear && (compliancePluginBizObject.SnapShot?.Contains(snapShot) ?? false)),
					shouldRunAdditionalComplianceProcess);

			var afterSnapShot = compliancePluginBizObject.SnapShot;

			if (beforeSnapShot != null && afterSnapShot != null && beforeSnapShot != afterSnapShot && !beforeSnapShot.Contains(afterSnapShot))
			{
				var parentProviders = GetAllParentComplianceItemRiskStatusProvider(compliancePluginBizObject.ComplianceItemRiskStatusProvider);
				if (parentProviders.Count > 0)
				{
					var providerParentIds = parentProviders.Select((provider) => provider.ParentID).ToArray();
					var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, providerParentIds);
					query.AddToFilter(new ZQuery(ComplianceRiskStatusSchema.COR_OverallRisk, Codes.OverrideClear), JoinCondition.And);
					var providerStatuses = complianceStatus.Factory.Load<ComplianceRiskStatus>(query);

					foreach (var providerStatus in providerStatuses)
					{
						providerStatus.ParentJob ??= new ComplianceRiskBusinessObject(providerStatus.Parent);
						RecalculateComplianceRiskStatus(providerStatus.ParentJob, (_) => true, false);
					}
				}
			}

			return rawComplianceStatuses != (complianceStatus.COR_OverallRisk, complianceStatus.COR_PartyRisk, complianceStatus.COR_LocationRisk, complianceStatus.COR_CommodityRisk);
		}

		static List<IComplianceItemRiskStatusProvider> GetAllParentComplianceItemRiskStatusProvider(IComplianceItemRiskStatusProvider provider)
		{
			List<IComplianceItemRiskStatusProvider> complianceItemRiskStatusProviders = new List<IComplianceItemRiskStatusProvider>();

			AddParentProviders(provider);

			return complianceItemRiskStatusProviders;

			void AddParentProviders(IComplianceItemRiskStatusProvider currentProvider)
			{
				if (currentProvider.ParentComplianceRiskStatusProviders != null)
				{
					foreach (var parentProvider in currentProvider.ParentComplianceRiskStatusProviders)
					{
						if (!complianceItemRiskStatusProviders.Contains(parentProvider))
						{
							complianceItemRiskStatusProviders.Add(parentProvider);
							AddParentProviders(parentProvider);
						}
					}
				}
			}
		}

		static ZString GetPartyRisk(ComplianceRiskPartyWrapperCollection parties, out List<(ZGuid PK, ZString ScreeningStatus)> snapShotParties)
		{
			snapShotParties = parties.Cast<ComplianceRiskPartyWrapper>().Select(u => (u.Key, u.ScreeningStatus)).ToList();
			return RiskStatusHelper.GetPartyRiskStatusBasedOnPartiesSnapshot(snapShotParties);
		}

		static ZString GetLocationRisk(ComplianceRiskLocationWrapperCollection locations, out List<(ZGuid PK, bool IsSanctioned)> snapShotLocations)
		{
			snapShotLocations = locations.Cast<ComplianceRiskLocationWrapper>().Select(u => (u.Key, u.IsSanctioned)).ToList();
			return snapShotLocations.Any(u => u.IsSanctioned)
				? Codes.Blocked : Codes.Clear;
		}

		static ZString GetCommodityRisk(
			ComplianceRiskBusinessObject complianceRiskBizO,
			out List<(ZString HarmonizedCode, ZString RiskStatus, CommodityType CommodityType)> snapShotCommodities,
			out bool snapShotIsInternational,
			out ZString snapShotSubProvidersCommodityRiskStatus)
		{
			snapShotSubProvidersCommodityRiskStatus = ZString.Empty;
			snapShotCommodities = [];
			if (!complianceRiskBizO.CommodityScreeningEnabled() || complianceRiskBizO.ComplianceRiskStatus.IsProceedWithoutCommodityRiskAssessmentCheck)
			{
				snapShotIsInternational = false;
				return complianceRiskBizO.HostBusinessEntity.IsCommodityRiskAssessableWithoutScreeningEnabledCheck() ?
					Codes.NotAssessed :
					Codes.NotApplicable;
			}

			if (complianceRiskBizO.ComplianceCommodityRiskStatusProvider is not null && complianceRiskBizO.HostBusinessEntity.IsCommodityRiskAssessableWithoutScreeningEnabledCheck())
			{
				snapShotCommodities = complianceRiskBizO.ComplianceRiskStatus.CommodityDetailCollection
					.Cast<ComplianceCommodityDetail>()
					.Select(u => (u.CCD_HarmonizedCode, u.CCD_RiskStatus, u.CommodityType))
					.ToList();

				snapShotIsInternational = true;

				if (snapShotCommodities.Count == 0
					&& !complianceRiskBizO.ComplianceCommodityRiskStatusProvider.ComplianceRiskSupport.IsSupportInitialization()
					&& complianceRiskBizO.ComplianceCommodityRiskStatusProvider.ComplianceRiskSupport.IsSupportSubCompliances()
					&& complianceRiskBizO.TryGetSubProvidersCommodityRiskStatus(out var subProvidersStatus))
				{
					snapShotSubProvidersCommodityRiskStatus = subProvidersStatus;
					return subProvidersStatus;
				}

				return complianceRiskBizO.ComplianceRiskStatus.GetCommodityRiskStatus(snapShotCommodities);
			}

			snapShotIsInternational = false;

			return Codes.NotApplicable;
		}
	}
}
