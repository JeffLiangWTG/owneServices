using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityDetailCollection : DependentBusinessObjectCollection<ComplianceCommodityDetail, ComplianceRiskStatus>
	{
		readonly IComplianceCommodityRiskStatusProvider complianceCommodityRisk;
		readonly ISupportInteractionWithComplianceWiseCommodities supportInteractionWithComplianceWiseCommodities;
		string jobSourceNumber;

		static string UserInputCommoditySource => Res.GetString("63EF7974-6CD7-4CF0-B9AE-166125DCF02A", "Compliance");

		public ComplianceCommodityDetailCollection(ComplianceRiskStatus master, IComplianceCommodityRiskStatusProvider complianceRisk) : base(master)
		{
			complianceCommodityRisk = complianceRisk;
			if (complianceRisk is ISupportInteractionWithComplianceWiseCommodities supportInteraction)
			{
				supportInteractionWithComplianceWiseCommodities = supportInteraction;
			}

			SetJobSourceNumber();
		}

		protected override string FkColumnName => ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk.Name;

		public void LoadWithSuspendChanges()
		{
			using (Master.SuspendSettingHasChanges())
			using (SuspendSettingHasChanges())
			{
				Load();
			}
		}

		internal void LoadWithSuspendChanges(IComplianceCommodity[] changedCommodities)
		{
			using (Master.SuspendSettingHasChanges())
			using (SuspendSettingHasChanges())
			{
				Load(changedCommodities);
			}
		}

		public void UpdateJobSourceNumberIfNeeded()
		{
			if (string.IsNullOrEmpty(jobSourceNumber))
			{
				SetJobSourceNumber();

				if (!string.IsNullOrEmpty(jobSourceNumber))
				{
					LoadWithSuspendChanges();
				}
			}
		}

		public override void Load()
		{
			Load(null);
		}

		void Load(IComplianceCommodity[] changedCommodities)
		{
			SuspendValidation();

			Where(o => o.CommodityType == CommodityType.RelatedJobLink && !o.IsInDatabase).ToList().ForEach(RemoveAndDelete);

			base.Load();

			var commodityDetails = this.Cast<ComplianceCommodityDetail>();
			commodityDetails.ForEach(o =>
			{
				o.Source = jobSourceNumber;
				o.Description = ZString.Empty;
				o.Conditions = ZString.Empty;
				o.HasChangesChanged -= ComplianceCommodityDetail_HasChangesChanged;
			});

			if (complianceCommodityRisk?.Commodities != null)
			{
				var commoditiesFromFetchDataEntry = new List<ComplianceCommodity>();
				var commoditiesFromRelatedJobLink = new List<ComplianceCommodity>();

				// To upper for HarmonizedCode && Origin && GoodsDescription since in the database, the fields are case insensitive and there is a unique index on it.
				var commoditiesGroupByHsCodeAndParentJobIDAndOriginAndDescription = complianceCommodityRisk.Commodities.Concat(changedCommodities ?? Enumerable.Empty<IComplianceCommodity>())
					.GroupBy(o => (HarmonizedCode: ComplianceRiskHelper.ExtractAllAlphanumericFromHsCode(o.HarmonizedCode).ToUpperInvariant(), o.ParentJobID, Origin: o.Origin.ToUpperInvariant(), GoodsDescription: o.GoodsDescription.ToUpperInvariant()))
					.Where(o => !o.Key.HarmonizedCode.IsEmpty);
				commoditiesGroupByHsCodeAndParentJobIDAndOriginAndDescription.ForEach(o =>
				{
					var commodityHasRiskStatus = o.FirstOrDefault(u => u.RiskStatus.HasValue);
					ComplianceCommodity complianceCommodity;

					if (commodityHasRiskStatus != null)
					{
						complianceCommodity = new ComplianceCommodity
							(o.Key.HarmonizedCode,
							commodityHasRiskStatus.GroupingOrCountry,
							ZString.Join(", ", o.Select(c => c.Source).Distinct().ToArray()),
							o.Key.ParentJobID,
							o.Key.Origin,
							ZString.Join(", ", o.Select(c => c.CommoditySource).Where(u => !u.IsEmpty).Distinct().OrderBy(c => c).ToArray()),
							commodityHasRiskStatus.GoodsDescription,
							commodityHasRiskStatus.RiskStatus.Value,
							commodityHasRiskStatus.AssessmentNotes,
							commodityHasRiskStatus.DateAddedUtc);
					}
					else
					{
						complianceCommodity = new ComplianceCommodity
							(o.Key.HarmonizedCode,
							o.Select(c => c.GroupingOrCountry).FirstOrDefault(),
							ZString.Join(", ", o.Select(c => c.Source).Distinct().ToArray()),
							o.Key.ParentJobID,
							o.Key.Origin,
							ZString.Join(", ", o.Select(c => c.CommoditySource).Where(u => !u.IsEmpty).Distinct().OrderBy(c => c).ToArray()),
							o.First().GoodsDescription);
					}

					if (complianceCommodity.ParentJobID == complianceCommodityRisk.ParentID)
					{
						commoditiesFromFetchDataEntry.Add(complianceCommodity);
					}
					else
					{
						commoditiesFromRelatedJobLink.Add(complianceCommodity);
					}
				});

				// Create lookup tables to reduce repetitive enumeration
				var commoditiesLookup = commodityDetails
					.Where(u => u.CommodityType != CommodityType.RelatedJobLink)
					.GroupBy(u => GetKey(u.CCD_HarmonizedCode, u.CCD_CountryOrGrouping, u.CCD_RN_NKOrigin))
					.ToDictionary(g => g.Key, g => g.ToList());

				if (commoditiesFromFetchDataEntry.Count > 0)
				{
					commoditiesFromFetchDataEntry.ForEach(commodityFromFetchDataEntry => MaintainFetchDataEntryCommodity(commoditiesLookup, commodityFromFetchDataEntry));
				}

				SetCommodityTypeToUserDataEntryAndDeleteNotExistingCommodityFromCurrentJob(commoditiesFromFetchDataEntry, commodityDetails.ToList());

				Master.UpdateCommodityRiskStatusBySupportedCountriesModelOrDeclined();

				// Create the dictionary when changedCommodities not null
				if (changedCommodities != null)
				{
					CommoditiesLinkedToCurrentJob ??= new Dictionary<string, ComplianceCommodityDetail>();
					CommoditiesLinkedToCurrentJob.Clear();
				}

				// Hook has changes events for each commodity belong to current job.
				commodityDetails.ForEach(o =>
				{
					HookHasChangesEventsForCommodityDetail(o);

					// Maintain the dictionary every time the collection is loaded
					if (CommoditiesLinkedToCurrentJob != null && o.CommodityType == CommodityType.FetchDataEntry)
					{
						CommoditiesLinkedToCurrentJob[ComplianceRiskHelper.GetUnionKeyFromCommodity(o.CCD_HarmonizedCode, o.CCD_CountryOrGrouping, o.CCD_RN_NKOrigin, o.CCD_Description)] = o;
					}
				});

				commoditiesFromRelatedJobLink.Reverse();
				if (commoditiesFromRelatedJobLink.Count > 0 && this is IList currentList)
				{
					// We can refactor this part to FetchCommodityDetailsInDB later.
					var jobIDs = commoditiesFromRelatedJobLink.Select(a => a.ParentJobID).Distinct();
					var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, jobIDs);
					query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.ComplianceRiskInteractionCode);
					query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.AssessmentInitialized);

					var jobInitializedComplianceIDs = Factory.Load<StmComplianceEvent>(query).Select(complianceEvent => complianceEvent.SCE_ParentID).ToHashSet();

					commoditiesFromRelatedJobLink.ForEach(o => currentList.Insert(0, InitReadOnlyCommodityDetailElement(o, jobInitializedComplianceIDs)));
				}
			}

			if (!complianceCommodityRisk?.IsEditingCommoditySupported ?? false)
			{
				SetReadOnlyIncludingChildren(true);
			}

			ResumeValidation();
		}

		public Dictionary<string, ComplianceCommodityDetail> CommoditiesLinkedToCurrentJob { get; private set; }

		public void InitializeCommoditiesLinkedToCurrentJobIfNeeded()
		{
			if (CommoditiesLinkedToCurrentJob == null)
			{
				CommoditiesLinkedToCurrentJob = new Dictionary<string, ComplianceCommodityDetail>();

				foreach (var detail in ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(this.Cast<ComplianceCommodityDetail>()))
				{
					CommoditiesLinkedToCurrentJob[ComplianceRiskHelper.GetUnionKeyFromCommodity(detail.CCD_HarmonizedCode, detail.CCD_CountryOrGrouping, detail.CCD_RN_NKOrigin, detail.CCD_Description)] = detail;
				}
			}
		}

		void SetCommodityTypeToUserDataEntryAndDeleteNotExistingCommodityFromCurrentJob(List<ComplianceCommodity> commoditiesFromFetchDataEntry, List<ComplianceCommodityDetail> commodityDetails)
		{
			var fetchDataEntrySet = new HashSet<string>(
				commoditiesFromFetchDataEntry.Select(u => ComplianceRiskHelper.GetUnionKeyFromCommodity(u.HarmonizedCode, u.GroupingOrCountry, u.Origin, u.GoodsDescription))
			);

			commodityDetails
				.Where(existingCommodityDetail => existingCommodityDetail.CommodityType == CommodityType.Unknown || existingCommodityDetail.CommodityType == CommodityType.FetchDataEntry)
				.ForEach(existingCommodityDetail =>
				{
					var detailKey = ComplianceRiskHelper.GetUnionKeyFromCommodity(existingCommodityDetail.CCD_HarmonizedCode, existingCommodityDetail.CCD_CountryOrGrouping, existingCommodityDetail.CCD_RN_NKOrigin, existingCommodityDetail.CCD_Description);

					if (!fetchDataEntrySet.Contains(detailKey))
					{
						if (existingCommodityDetail.CommodityType == CommodityType.Unknown)
						{
							existingCommodityDetail.CommodityType = CommodityType.UserDataEntry;
							existingCommodityDetail.CommoditySource = UserInputCommoditySource;
						}
						else
						{
							RemoveAndDelete(existingCommodityDetail);
						}
					}
					else if (existingCommodityDetail.CommodityType == CommodityType.Unknown)
					{
						existingCommodityDetail.CommodityType = CommodityType.UserDataEntry;
						existingCommodityDetail.CommoditySource = UserInputCommoditySource;
					}
				});
		}

		void MaintainFetchDataEntryCommodity(Dictionary<string, List<ComplianceCommodityDetail>> commoditiesLookup, ComplianceCommodity commodityFromFetchDataEntry)
		{
			var key = GetKey(commodityFromFetchDataEntry.HarmonizedCode, commodityFromFetchDataEntry.GroupingOrCountry, commodityFromFetchDataEntry.Origin);

			if (commoditiesLookup.TryGetValue(key, out var existingCommodityDetails))
			{
				var commodityWithSameDescriptions = existingCommodityDetails.Where(u =>
					u.CCD_Description.EqualsIgnoringCase(commodityFromFetchDataEntry.GoodsDescription)).ToList();

				if (commodityWithSameDescriptions.Count >= 1)
				{
					var fetchDataEntryCommodity = commodityWithSameDescriptions
						.FirstOrDefault(u => u.CommodityType == CommodityType.FetchDataEntry) ?? commodityWithSameDescriptions[0];

					fetchDataEntryCommodity.CommodityType = CommodityType.FetchDataEntry;
					fetchDataEntryCommodity.OriginOfGoods = commodityFromFetchDataEntry.Origin;
					fetchDataEntryCommodity.CommoditySource = commodityFromFetchDataEntry.CommoditySource;
				}
				else
				{
					var commodityWithDiffDescriptionWhenPSKOrCLR = existingCommodityDetails
						.FirstOrDefault(u => !u.CCD_RiskStatus.HasBlockedOrReleased() && u.CCD_RiskStatus != Codes.NotChecked);

					if (commodityWithDiffDescriptionWhenPSKOrCLR != null)
					{
						AddNewFetchDataEntryCommodity(existingCommodityDetails, commodityWithDiffDescriptionWhenPSKOrCLR.CCD_RiskStatus);
					}
					else
					{
						AddNewFetchDataEntryCommodity(existingCommodityDetails);
					}
				}
			}
			else
			{
				AddNewFetchDataEntryCommodity();
			}

			void AddNewFetchDataEntryCommodity(List<ComplianceCommodityDetail> details = null, string riskStatus = null)
			{
				var newFetchDataEntryCommodity = Master.Factory.New<ComplianceCommodityDetail>();
				newFetchDataEntryCommodity.CommodityType = CommodityType.FetchDataEntry;
				newFetchDataEntryCommodity.CCD_COR_ComplianceRisk = Master.PK;
				newFetchDataEntryCommodity.CCD_CountryOrGrouping = commodityFromFetchDataEntry.GroupingOrCountry;
				newFetchDataEntryCommodity.CCD_HarmonizedCode = commodityFromFetchDataEntry.HarmonizedCode;
				newFetchDataEntryCommodity.Source = commodityFromFetchDataEntry.Source;
				newFetchDataEntryCommodity.CommoditySource = commodityFromFetchDataEntry.CommoditySource;
				newFetchDataEntryCommodity.OriginOfGoods = commodityFromFetchDataEntry.Origin;

				newFetchDataEntryCommodity.CCD_RN_NKOrigin = commodityFromFetchDataEntry.Origin;
				newFetchDataEntryCommodity.CCD_Description = commodityFromFetchDataEntry.GoodsDescription;

				if (riskStatus != null)
				{
					newFetchDataEntryCommodity.CCD_RiskStatus = riskStatus;
					newFetchDataEntryCommodity.NeedResetStatus = false;
				}

				Add(newFetchDataEntryCommodity);

				if (details == null)
				{
					commoditiesLookup.Add(key, new List<ComplianceCommodityDetail> { newFetchDataEntryCommodity });
				}
				else
				{
					details.Add(newFetchDataEntryCommodity);
				}
			}
		}

		static string GetKey(ZString harmonizedCode, ZString groupingOrCountry, ZString origin)
		{
			return $"{harmonizedCode.ToUpperInvariant()}$${groupingOrCountry}$${origin.ToUpperInvariant()}";
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is ComplianceCommodityDetail complianceCommodityDetail)
			{
				if (complianceCommodityDetail.ComplianceRiskStatus?.IsAssessmentDeclined ?? false)
				{
					complianceCommodityDetail.CCD_RiskStatus = Codes.PossibleRisk;
				}
				else
				{
					complianceCommodityDetail.CCD_RiskStatus = Master.CommodityDetailsShouldBeNotChecked ? Codes.NotChecked : Codes.PossibleRisk;
				}

				HookHasChangesEventsForCommodityDetail(complianceCommodityDetail);
			}
		}

		void HookHasChangesEventsForCommodityDetail(ComplianceCommodityDetail complianceCommodityDetail)
		{
			complianceCommodityDetail.HasChangesChanged -= ComplianceCommodityDetail_HasChangesChanged;
			complianceCommodityDetail.HasChangesChanged += ComplianceCommodityDetail_HasChangesChanged;
		}

		internal static ZString ExtractAllNumbersFromTariffCode(ZString tariffCode)
		{
			return Regex.Replace(tariffCode, @"[^0-9]+", string.Empty);
		}

		ComplianceCommodityDetail InitReadOnlyCommodityDetailElement(ComplianceCommodity commodity, HashSet<ZGuid> assessmentInitializedJobIDs)
		{
			var result = Master.Factory.New<ComplianceCommodityDetail>();
			if (assessmentInitializedJobIDs.Contains(commodity.ParentJobID))
			{
				result.RelatedJobIsAssessmentInitialized = true;
			}

			using (result.GetValidationSuspender())
			{
				result.ReadOnly = true;
				result.CommodityType = CommodityType.RelatedJobLink;
				result.CCD_COR_ComplianceRisk = Master.PK;
				result.CCD_CountryOrGrouping = commodity.GroupingOrCountry;
				result.CCD_HarmonizedCode = commodity.HarmonizedCode;
				result.Source = commodity.Source;
				result.CommoditySource = commodity.CommoditySource.IsEmpty ? UserInputCommoditySource : commodity.CommoditySource;
				result.CCD_RiskStatus = commodity?.RiskStatus ?? result.CCD_RiskStatus;
				result.CCD_AssessmentNotes = commodity.AssessmentNotes;
				result.OriginOfGoods = commodity.Origin;

				result.CCD_Description = commodity.GoodsDescription;
				result.CCD_RN_NKOrigin = commodity.Origin;
				result.CCD_SystemCreateTimeUtc = commodity.DateAddedUtc;
			}
			result.HasChanges = false;
			return result;
		}

		async void ComplianceCommodityDetail_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			var changedCommodityDetail = e.ObjectThatWasChanged as ComplianceCommodityDetail;
			var commodityNeedToNotify = false;
			var needToTriggerValidation = false;
			var needRecalculateCommodityRiskStatus = changedCommodityDetail != null
				  && (changedCommodityDetail.IsDeleted
				  || changedCommodityDetail.IsDeleting
				  || changedCommodityDetail.HasChanges);

			if (changedCommodityDetail != null
				  && !changedCommodityDetail.ReadOnly
				  && !changedCommodityDetail.IsDeleted
				  && !changedCommodityDetail.IsDeleting)
			{
				commodityNeedToNotify = true;

				needToTriggerValidation = changedCommodityDetail.NeedResetStatus
					&& changedCommodityDetail.HasChanges;
				if (changedCommodityDetail.Source == ZString.Empty)
				{
					changedCommodityDetail.Source = jobSourceNumber;
				}

				if (changedCommodityDetail.CommoditySource == ZString.Empty)
				{
					changedCommodityDetail.CommoditySource = UserInputCommoditySource;
				}

				if (changedCommodityDetail.CommodityType == CommodityType.Unknown)
				{
					changedCommodityDetail.CommodityType = CommodityType.UserDataEntry;
				}
			}

			if (needRecalculateCommodityRiskStatus)
			{
				SetCommodityRiskStatus_AndOverallComplianceRiskStatus(changedCommodityDetail);
			}

			if (needToTriggerValidation)
			{
				changedCommodityDetail.NeedResetStatus = false;

				if (Master.IsAssessmentInitialized)
				{
					changedCommodityDetail.CCD_RiskStatus = Codes.NotChecked;
					changedCommodityDetail.ImportAlertsForExportJobDescription = ZString.Empty;
					await Master.CheckCommoditiesRiskStatusIfAvailable(changedCommodityDetail);
				}
				else if (Master.IsAssessmentDeclined)
				{
					changedCommodityDetail.CCD_RiskStatus = Codes.PossibleRisk;
				}
				else
				{
					changedCommodityDetail.CCD_RiskStatus = Master.CommodityDetailsShouldBeNotChecked ? Codes.NotChecked : Codes.PossibleRisk;
				}
			}

			if (commodityNeedToNotify)
			{
				NotifyChangesFromCpwSide(changedCommodityDetail);
			}
		}

		public void NotifyChangesFromCpwSide(params ComplianceCommodityDetail[] changedCommodityDetails)
		{
			changedCommodityDetails = changedCommodityDetails.Where(u => u.CommodityType == CommodityType.FetchDataEntry).ToArray();
			if (changedCommodityDetails.Length > 0)
			{
				if (supportInteractionWithComplianceWiseCommodities?.Helper?.CpwSideCommodities?.CommoditiesChanged != null)
				{
					supportInteractionWithComplianceWiseCommodities.Helper.CpwSideCommodities.CommoditiesChanged.Invoke(changedCommodityDetails.Select(
						changedCommodityDetail => new ComplianceResultFromCpw
						{
							HarmonizedCode = changedCommodityDetail.CCD_HarmonizedCode,
							GroupingOrCountry = changedCommodityDetail.CCD_CountryOrGrouping,
							GoodsDescription = changedCommodityDetail.CCD_Description,
							OriginOfGoods = changedCommodityDetail.CCD_RN_NKOrigin,
							LinkVisible = changedCommodityDetail.ShowLegalBookLink,
							HarmonizedBorderWiseTextual = changedCommodityDetail.HarmonizedBorderWiseTextual,
							RiskStatus = changedCommodityDetail.CCD_RiskStatus,
							RiskNotes = changedCommodityDetail.CCD_AssessmentNotes,
							AssessmentInitialized = changedCommodityDetail.AssessmentInitialized,
							ImportAlertStatus = changedCommodityDetail.ImportAlertsForExportJobDescription,
						}).ToArray());
				}
			}
		}

		void SetJobSourceNumber()
		{
			if (complianceCommodityRisk != null)
			{
				if (complianceCommodityRisk is BusinessObject hostedCommodityBusinessObject)
				{
					jobSourceNumber = CodePropertyAttribute.CodeFromBusinessObject(hostedCommodityBusinessObject);
				}
				else
				{
					throw new ArgumentNullException(nameof(complianceCommodityRisk));
				}
			}
			else
			{
				jobSourceNumber = string.Empty;
			}
		}

		void SetCommodityRiskStatus_AndOverallComplianceRiskStatus(ComplianceCommodityDetail changedCommodityDetail)
		{
			// In case changedCommodityDetail hasn't committed to this collection, we need to append it
			var hasCommodities = (changedCommodityDetail != null ? this.Append(changedCommodityDetail) : this).Where(o => !(o.IsDeleting || o.IsDeleted))
				.Cast<ComplianceCommodityDetail>();
			Master.SetCommodityRiskStatus(hasCommodities);

			var ignoreOverrideClear = changedCommodityDetail != null
										&& !changedCommodityDetail.IsDeleting
										&& !changedCommodityDetail.IsDeleted
										&& !changedCommodityDetail.CCD_COR_ComplianceRisk.IsEmpty
									|| changedCommodityDetail == null;

			Master.SetOverallRiskStatus(ignoreOverrideClear);
		}
	}
}
