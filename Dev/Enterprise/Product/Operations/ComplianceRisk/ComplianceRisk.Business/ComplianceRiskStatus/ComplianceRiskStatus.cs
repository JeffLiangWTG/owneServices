using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using static Enterprise.ComplianceRisk.Integration.ComplianceEventList;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskStatus : AutoComplianceRiskStatus
	{
		public ComplianceRiskStatus(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			CommodityEventHelper = new(this);
		}

		public ComplianceRiskPlugInBusinessObject PlugInParent { get; internal set; }

		public ComplianceRiskBusinessObject ParentJob { get; internal set; }

		public BusinessObject Parent => Factory.Load(COR_ParentTableCode, COR_ParentID);

		public bool IsProceedWithoutCommodityRiskAssessmentCheck => !ComplianceRiskHelper.IsComplianceCommodityRiskAssessmentEnabled && !IsAssessmentInitialized && !IsAssessmentDeclined && !IsAllCommoditiesAssessesmentInitialized && (SupportedCountriesCheckResponseModel == null || IsCountriesSupportedOrHasRelatedJobCommodity());

		bool IsCountriesSupportedOrHasRelatedJobCommodity()
		{
			var commodities = CommodityDetailCollection.Cast<ComplianceCommodityDetail>();
			if (!IsCountriesSupported(commodities))
			{
				return commodities.Any(u =>
					u.CommodityType == CommodityType.RelatedJobLink &&
					u.CCD_RiskStatus != ComplianceRiskStatusCodeList.Codes.PossibleRisk);
			}

			return true;
		}

		bool IsAllCommoditiesAssessesmentInitialized => CommodityDetailCollection.Count > 0 && CommodityDetailCollection.Cast<ComplianceCommodityDetail>().All(u => u.AssessmentInitialized);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
			COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Unknown;
		}

		[ChildEditable(true)]
		public ComplianceCommodityDetailCollection CommodityDetailCollection
		{
			get
			{
				if (commodityDetailCollection is null)
				{
					commodityDetailCollection = new(this, ParentJob?.ComplianceCommodityRiskStatusProvider);
					commodityDetailCollection.Load();

					RegisterEditableChildObject(commodityDetailCollection);
				}

				return commodityDetailCollection;
			}
		}
		ComplianceCommodityDetailCollection commodityDetailCollection;

		public ComplianceCommodityDetailFilteredCollectionView CommodityDetailCollectionView
		{
			get { return commodityDetailCollectionView ??= new(CommodityDetailCollection); }
		}

		ComplianceCommodityDetailFilteredCollectionView commodityDetailCollectionView;

		public override ZString COR_OverallRisk
		{
			get { return base.COR_OverallRisk; }
			set
			{
				base.COR_OverallRisk = value;
				PlugInParent?.OverallRiskDescriptionInfo.RefreshBinding();
			}
		}

		public override ZString COR_PartyRisk
		{
			get => base.COR_PartyRisk;
			set
			{
				base.COR_PartyRisk = value;
				PlugInParent?.PartyRiskDescriptionInfo.RefreshBinding();
			}
		}

		public override ZString COR_LocationRisk
		{
			get { return base.COR_LocationRisk; }
			set
			{
				base.COR_LocationRisk = value;
				PlugInParent?.LocationRiskDescriptionInfo.RefreshBinding();
			}
		}

		public override ZString COR_CommodityRisk
		{
			get { return base.COR_CommodityRisk; }
			set
			{
				base.COR_CommodityRisk = value;
				PlugInParent?.CommodityRiskDescriptionInfo.RefreshBinding();
			}
		}

		public override ZDateTimeOffset COR_JobEndDate
		{
			get { return base.COR_JobEndDate; }
			set
			{
				if (base.COR_JobEndDate != value)
				{
					base.COR_JobEndDate = value;
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			AddStatusUpdatedEventsIfNeeded();
		}

		#region Add Status Updated Events

		void AddStatusUpdatedEventsIfNeeded()
		{
			if (Parent is IStmALogParent logParent)
			{
				AddStatusUpdatedEventIfNeeded(COR_PartyRiskInfo, (NoResString)"Party risk status", logParent, needToTakeSnapshot: false);
				AddStatusUpdatedEventIfNeeded(COR_LocationRiskInfo, (NoResString)"Location risk status", logParent, needToTakeSnapshot: false);
				AddStatusUpdatedEventIfNeeded(COR_CommodityRiskInfo, (NoResString)"Commodity risk status", logParent, needToTakeSnapshot: false);
				AddStatusUpdatedEventIfNeeded(COR_OverallRiskInfo, (NoResString)"Job compliance status", logParent, needToTakeSnapshot: true);
			}
		}

		void AddStatusUpdatedEventIfNeeded(ZPropertyInfo propertyInfo, ZString field, IStmALogParent logParent, bool needToTakeSnapshot)
		{
			if (!IsInDatabase)
			{
				AddComplianceStatusUpdateEventLog(propertyInfo, false);
			}
			else if (propertyInfo.HasChanges)
			{
				var originalValue = (ZString)propertyInfo.OriginalValue;
				var currentValue = (ZString)propertyInfo.Value;
				if (originalValue != currentValue)
				{
					logParent.Logs.AddNew(AutoEvents.StatusUpdated, GetUpdatedStatusEventParameters(originalValue, currentValue, field, needToTakeSnapshot));
					AddComplianceStatusUpdateEventLog(propertyInfo, true);
				}
			}
		}

		KeyValuePair<string, string>[] GetUpdatedStatusEventParameters(ZString? oldStatus, string newStatus, ZString field, bool needToTakeSnapshot)
		{
			var parameters = new List<KeyValuePair<string, string>>
			{
				new(ParameterCodes.New, ((CodeDescriptionPair)Lookups.AllRiskStatusCodes[newStatus]).MultilingualDescription.GetUnresolvedString()),
				new(ParameterCodes.MessageType, (NoResString)"Compliance Risk"),
				new(ParameterCodes.Type, field),
			};

			if (oldStatus.HasValue)
			{
				parameters.Add(new KeyValuePair<string, string>(ParameterCodes.Old, ((CodeDescriptionPair)Lookups.AllRiskStatusCodes[oldStatus]).MultilingualDescription.GetUnresolvedString()));
			}

			return parameters.ToArray();
		}

		internal void TakeSnapshotWhenComplianceDecisionChanged()
		{
			if (PlugInParent != null)
			{
				DecisionChangedSnapshot = new ComplianceAuditSnapshot();
				TakeSnapshot(PlugInParent, DecisionChangedSnapshot);
			}
		}

		public void TakeSnapshotAndSetStatusToOverrideClear(ComplianceRiskPlugInBusinessObject plugInBizO, (string Code, string Description, string Reason) reasonInfo)
		{
			ComplianceRiskStatusSynchronizer.Synchronize(plugInBizO);

			COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;

			OverrideSnapshot = new ComplianceAuditSnapshot();
			TakeSnapshot(plugInBizO, OverrideSnapshot);

			OverrideSnapshot.OverrideDecision.Code = reasonInfo.Code;
			OverrideSnapshot.OverrideDecision.Description = reasonInfo.Description;
			OverrideSnapshot.OverrideDecision.Reason = reasonInfo.Reason;

			ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null);
		}

		void TakeSnapshot(ComplianceRiskPlugInBusinessObject plugInBizO, ComplianceAuditSnapshot snapshot)
		{
			var allPartyAndCountryPKs = new List<ZGuid>();
			var plugInBizOAllParties = plugInBizO.Parties.Cast<ComplianceRiskPartyWrapper>().ToList();
			var plugInBizOAllLocations = plugInBizO.Locations.Cast<ComplianceRiskLocationWrapper>().ToList();
			plugInBizOAllParties.ForEach(p => allPartyAndCountryPKs.Add(p.Key));
			plugInBizOAllLocations.ForEach(l => allPartyAndCountryPKs.Add(l.Key));

			var mostRecentScreeningStatusRelatedLogForAllPartiesAndCountries = GetAllSourcesAndLogs(allPartyAndCountryPKs);
			snapshot.ComplianceJobDirection.IsInternational = (plugInBizO.HostBusinessEntity as IComplianceJobDirectionProvider)?.IsInternational ?? false;
			snapshot.ComplianceJobDirection.Direction = (plugInBizO.HostBusinessEntity as IComplianceJobDirectionProvider).JobDirection.ToString();
			snapshot.IsComplianceCommodityRiskProvider = plugInBizO.ComplianceCommodityRiskStatusProvider != null;
			snapshot.IsCompliancePartyRiskProvider = plugInBizO.CompliancePartyRiskStatusProvider != null;
			snapshot.IsComplianceLocationRiskProvider = plugInBizO.ComplianceLocationRiskStatusProvider != null;

			snapshot.LocationRisk = COR_LocationRisk;
			snapshot.PartyRisk = COR_PartyRisk;
			snapshot.CommodityRisk = COR_CommodityRisk;

			plugInBizOAllParties.ForEach(p => snapshot.Parties.Add(
				new Party()
				{
					PK = p.Key.ToGuid(),
					Code = p.Code,
					Description = p.ParentsDescription,
					TableCode = p.WrappedScreeningParty.ScreeningEntity.TablePrefix,
					Status = p.ScreeningStatus,
					LogPK = mostRecentScreeningStatusRelatedLogForAllPartiesAndCountries.TryGetValue(p.Key.ToGuid(), out var logPK) ? logPK : Guid.Empty
				}
			));

			plugInBizOAllLocations.ForEach(l => snapshot.Countries.Add(
				new Country()
				{
					Code = l.Location,
					Name = l.LocationDescription,
					Description = l.Description,
					IsSanctioned = l.IsSanctioned,
					LogPK = mostRecentScreeningStatusRelatedLogForAllPartiesAndCountries.TryGetValue(l.Key.ToGuid(), out var logPK) ? logPK : Guid.Empty
				}));

			plugInBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ForEach(c => snapshot.Commodities.Add(
				Commodity.GetCommodity
				(
					code: c.CCD_HarmonizedCode,
					conditions: c.Conditions,
					hsCodeDescription: c.Description,
					riskStatus: c.CCD_RiskStatus,
					nomenclatureCondition: c.NomenclatureCondition,
					specificCondition: c.SpecificCondition,
					source: c.Source,
					commoditySource: c.CommoditySource,
					notes: c.CCD_AssessmentNotes,
					originOfGoods: c.CCD_RN_NKOrigin,
					goodsDescription: c.CCD_Description,
					isAssessmentInitiated: c.AssessmentInitialized,
					dateAddedUtc: SetEmptyDateTimeToCurrentUtc(c)
				)));
		}

		DateTime SetEmptyDateTimeToCurrentUtc(ComplianceCommodityDetail complianceCommodityDetail)
		{
			if (complianceCommodityDetail.CCD_SystemCreateTimeUtc.IsEmpty)
			{
				using (complianceCommodityDetail.SuspendSettingHasChanges())
				{
					complianceCommodityDetail.CCD_SystemCreateTimeUtc = ZDateTime.UtcNow;
				}
			}

			return complianceCommodityDetail.CCD_SystemCreateTimeUtc.ToDateTime();
		}

		ComplianceAuditSnapshot OverrideSnapshot { get; set; }

		ComplianceAuditSnapshot DecisionChangedSnapshot { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		Dictionary<Guid, Guid> GetAllSourcesAndLogs(List<ZGuid> sourceIDs)
		{
			var result = new Dictionary<Guid, Guid>();
			if (sourceIDs.Any())
			{
				var sql = string.Format(CultureInfo.InvariantCulture,
					@"WITH CTE AS
(
	SELECT ROW_NUMBER() OVER(PARTITION BY PJ_ParentID ORDER BY PJ_Sequence DESC) AS RN, PJ_PK, PJ_ParentID
	FROM dbo.StmEntityScreeningLog
	JOIN @PKs ON PJ_ParentID = VALUE
	WHERE PJ_Status <> @CancelScreen

)
SELECT PJ_ParentID, PJ_PK FROM CTE WHERE RN = 1");
				using var cmd = Db.Connection.Command(sql);
				cmd.AddTableValuedParameter("@PKs", "dbo.TVP_uniqueidentifier", sourceIDs.Distinct().Select(s => s.ToGuid()));
				cmd.AddParameter("@CancelScreen", SqlDbType.Char, DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled);

				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					if (!result.ContainsKey(reader.GetGuid(0)))
					{
						result.Add(reader.GetGuid(0), reader.GetGuid(1));
					}
				}
			}

			return result;
		}

		void AddComplianceStatusUpdateEventLog(ZPropertyInfo propertyInfo, bool hasOldValue)
		{
			var eventLog = AddNewComplianceEventLog(AutoEvents.StatusUpdatedCode);
			eventLog.SCE_NewValue = (ZString)propertyInfo.Value;
			eventLog.SCE_OldValue = hasOldValue ? (ZString)propertyInfo.OriginalValue : ZString.Empty;

			if (propertyInfo == COR_PartyRiskInfo)
			{
				eventLog.SCE_EventSubType = Codes.PartyRisk;
				eventLog.SCE_EventReference = Descriptions.PartyRisk;
			}
			else if (propertyInfo == COR_LocationRiskInfo)
			{
				eventLog.SCE_EventSubType = Codes.LocationRisk;
				eventLog.SCE_EventReference = Descriptions.LocationRisk;
			}
			else if (propertyInfo == COR_CommodityRiskInfo)
			{
				eventLog.SCE_EventSubType = Codes.CommodityRisk;
				eventLog.SCE_EventReference = Descriptions.CommodityRisk;
			}
			else if (propertyInfo == COR_OverallRiskInfo)
			{
				eventLog.SCE_EventSubType = Codes.OverallRisk;
				eventLog.SCE_EventReference = Descriptions.OverallRisk;

				if (OverrideSnapshot != null)
				{
					eventLog.SCE_Snapshot = (ZString)JsonConvert.SerializeObject(OverrideSnapshot);
					OverrideSnapshot = null;
				}
			}
		}

		public IEnumerable<StmComplianceEvent> GetEventLogs()
		{
			return Factory.Load<StmComplianceEvent>(new ZQuery(StmComplianceEventSchema.SCE_ParentID, COR_ParentID));
		}

		IEnumerable<StmComplianceEvent> GetRiskInteractionEventLogs()
		{
			var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, COR_ParentID);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.ComplianceRiskInteractionCode);
			return Factory.Load<StmComplianceEvent>(query);
		}

		internal StmComplianceEvent AddNewComplianceEventLog(ZString eventType)
		{
			var eventLog = Factory.New<StmComplianceEvent>();
			eventLog.SCE_ParentID = COR_ParentID;
			eventLog.SCE_ParentTableCode = COR_ParentTableCode;
			eventLog.SCE_EventTimeOffset = ZDateTimeOffset.Now;
			eventLog.SCE_EventType = eventType;
			return eventLog;
		}

		internal void AddNewComplianceEventLogIfComplianceDecisionChanged()
		{
			if (DecisionChangedSnapshot != null)
			{
				var eventLog = Factory.New<StmComplianceEvent>();
				eventLog.SCE_ParentID = COR_ParentID;
				eventLog.SCE_ParentTableCode = COR_ParentTableCode;
				eventLog.SCE_EventTimeOffset = ZDateTimeOffset.Now;
				eventLog.SCE_EventType = AutoEvents.StatusUpdatedCode;
				eventLog.SCE_EventSubType = Codes.ComplianceDecisionChanged;
				eventLog.SCE_NewValue = eventLog.SCE_OldValue = COR_OverallRisk;
				eventLog.SCE_Snapshot = (ZString)JsonConvert.SerializeObject(DecisionChangedSnapshot);

				DecisionChangedSnapshot = null;
			}
		}

		#endregion

		public async Task ViewBorderWisePortalIfAvailable(ComplianceCommodityDetail commodityDetail)
		{
			if (commodityDetail.ShowLegalBookLink && PlugInParent?.CommodityRiskStatusChecker != null)
			{
				await PlugInParent.CommodityRiskStatusChecker.ViewBorderWisePortal(commodityDetail);
			}
		}

		internal async Task CheckCommoditiesRiskStatusIfAvailable(ComplianceCommodityDetail changedCommodityDetail)
		{
			if (PlugInParent?.CommodityRiskStatusChecker != null)
			{
				await PlugInParent.CommodityRiskStatusChecker.CheckCommoditiesRiskStatus(changedCommodityDetail);
			}
		}

		public bool IsAssessmentInitialized => GetRiskInteractionEventLogs().Any(u => u.SCE_EventSubType == Codes.AssessmentInitialized);

		public bool IsAssessmentDeclined => !IsAssessmentInitialized && GetRiskInteractionEventLogs().Any(u => u.SCE_EventSubType == Codes.AssessmentDeclined);

		internal CommodityEventHelper CommodityEventHelper { get; }
		public bool AssessmentInitializedByRule { get; set; }

		public bool CopyFromBooking { get; set; }

		public bool CopyFromBookingFirstLoaded { get; set; }

		public bool ShouldDoAssessmentByBorderWise
		{
			get => IsAssessmentInitialized
				|| SupportedCountriesCheckResponseModel != null && IsCountriesSupported(CommodityDetailCollection.Cast<ComplianceCommodityDetail>());
		}

		public bool CommodityDetailsShouldBeNotChecked
		{
			get => IsAssessmentInitialized
				|| SupportedCountriesCheckResponseModel == null
				|| SupportedCountriesCheckResponseModel != null && IsCountriesSupported(CommodityDetailCollection.Cast<ComplianceCommodityDetail>());
		}

		public bool SupportedCountriesCheckResponseModelExists => SupportedCountriesCheckResponseModel != null;

		bool modelTryLoad;

		SupportedCountriesCheckResponseModel _supportedCountriesCheckResponseModel;

		public SupportedCountriesCheckResponseModel SupportedCountriesCheckResponseModel
		{
			get
			{
				if (!modelTryLoad && _supportedCountriesCheckResponseModel == null)
				{
					_supportedCountriesCheckResponseModel = SupportedCountriesHelper.GetSupportedCountriesStmData(Factory).SupportedCountries;
					modelTryLoad = true;
				}

				return _supportedCountriesCheckResponseModel;
			}
			set
			{
				_supportedCountriesCheckResponseModel = value;
			}
		}

		internal bool UpdateCommodityRiskStatusBySupportedCountriesModelOrDeclined()
		{
			var hasChanges = false;
			if (SupportedCountriesCheckResponseModel != null && !IsAssessmentInitialized || IsAssessmentDeclined)
			{
				var commodities = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(CommodityDetailCollection.Cast<ComplianceCommodityDetail>()).ToArray();
				if (!IsAssessmentDeclined && IsCountriesSupported(commodities))
				{
					commodities.ForEach(u =>
					{
						if (u.CCD_RiskStatus != ComplianceRiskStatusCodeList.Codes.NotChecked)
						{
							hasChanges = true;
							u.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
						}
					});
				}
				else
				{
					commodities.ForEach(u =>
					{
						if (u.CCD_RiskStatus != ComplianceRiskStatusCodeList.Codes.PossibleRisk)
						{
							hasChanges = true;
							u.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PossibleRisk;
						}
					});
				}
			}

			return hasChanges;
		}

		bool IsCountriesSupported(IEnumerable<ComplianceCommodityDetail> commodities)
		{
			if (SupportedCountriesCheckResponseModel != null && ParentJob?.ComplianceCommodityRiskStatusProvider != null)
			{
				var complianceCommodityDetails = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(commodities).ToArray();
				var requestModel = ComplianceCheckRequestModelBuilder.GetRequestModel(ParentJob.ComplianceItemRiskStatusProvider as BusinessObject, ParentJob.ComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo?.PointPairs, complianceCommodityDetails);
				return (requestModel?.Commodities?.Any(u => u.Origin.Any(origin => SupportedCountriesCheckResponseModel.CommodityLevel.OriginOfGoods.Contains(origin))) ?? false)
					|| (requestModel?.PointPairs?.Any(u => SupportedCountriesCheckResponseModel.CommodityLevel.Export.Contains(u.OriginPoint?.Country ?? string.Empty)) ?? false)
					|| (requestModel?.PointPairs?.Any(u => SupportedCountriesCheckResponseModel.CommodityLevel.Import.Contains(u.DestinationPoint?.Country ?? string.Empty)) ?? false);
			}

			return false;
		}

		internal ZString GetCommodityRiskStatusWhenNoCommodities()
		{
			if (IsAssessmentInitialized)
			{
				return ComplianceRiskStatusCodeList.Codes.Incomplete;
			}
			else if (IsAssessmentDeclined)
			{
				return ComplianceRiskStatusCodeList.Codes.PossibleRisk;
			}
			else if (SupportedCountriesCheckResponseModel != null)
			{
				if (IsCountriesSupported(Enumerable.Empty<ComplianceCommodityDetail>()))
				{
					return ComplianceRiskStatusCodeList.Codes.Unknown;
				}
				else
				{
					return ComplianceRiskStatusCodeList.Codes.PossibleRisk;
				}
			}
			else
			{
				return COR_CommodityRisk.ToString() is ComplianceRiskStatusCodeList.Codes.Unknown or ComplianceRiskStatusCodeList.Codes.PossibleRisk
					? COR_CommodityRisk : ComplianceRiskStatusCodeList.Codes.Unknown;
			}
		}

		void CopyComplianceRiskStatusCoreInfo(ComplianceRiskStatus oldRiskStatus)
		{
			OverrideSnapshot = oldRiskStatus.OverrideSnapshot;
			DecisionChangedSnapshot = oldRiskStatus.DecisionChangedSnapshot;
			AssessmentInitializedByRule = oldRiskStatus.AssessmentInitializedByRule;
			CopyFromBooking = oldRiskStatus.CopyFromBooking;
			CopyFromBookingFirstLoaded = oldRiskStatus.CopyFromBookingFirstLoaded;
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandlers == null)
				{
					uniqueIndexFailureHandlers = new List<IUniqueIndexFailureHandler>();
					uniqueIndexFailureHandlers.Add(new ComplianceRiskStatusUniqueIndexFailureHandler(this));
				}

				return uniqueIndexFailureHandlers;
			}
		}

		List<IUniqueIndexFailureHandler> uniqueIndexFailureHandlers;

		class ComplianceRiskStatusUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ComplianceRiskStatusUniqueIndexFailureHandler(ComplianceRiskStatus riskStatus)
			{
				this.RiskStatus = riskStatus;
			}
			readonly ComplianceRiskStatus RiskStatus;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return ComplianceRiskStatusSchema.Constants.Indexes.NR_UC__COR_ParentID; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier?.ReportInformation(Res.GetString("EC80063A-F1C0-45FF-9C55-6B92FBED582A", "While you have been working with this form, another user has made changes.\r\n\r\nThe system will now try to combine your changes with those of the other user.\r\nAfter you click 'OK', the form will merge your changes with changes made by other user.\r\n\r\nHowever, the fields will have warning messages explaining the other user's changes.\r\nPlease review the form carefully before clicking the 'Save' button again.\r\n\r\nThe following objects have changes and will be merged:\r\nCompliance Risk Status"), Res.GetString("0FEF93D4-A139-435A-B393-F34C1FCB1D78", "Duplicate Compliance Risk"));
				AttemptToResolveDuplicateComplianceRisk();
			}

			void AttemptToResolveDuplicateComplianceRisk()
			{
				var parentId = RiskStatus.COR_ParentID;
				var complianceBusinessObject = RiskStatus.ParentJob;
				if (complianceBusinessObject == null)
				{
					return;
				}

				var query = new ZDBOnlyQuery(typeof(ComplianceRiskStatus));
				query.IgnoreDbQueryCache = true;
				query.AddToFilter(ComplianceRiskStatusSchema.COR_ParentID, parentId);
				var riskStatusInDb = RiskStatus.Factory.LoadTop1<ComplianceRiskStatus>(query);
				if (riskStatusInDb == null)
				{
					return;
				}

				riskStatusInDb.CopyComplianceRiskStatusCoreInfo(RiskStatus);
				RiskStatus.CommodityDetailCollection.RemoveAndDeleteAll();
				complianceBusinessObject.ReplaceComplianceRiskStatus(riskStatusInDb);
				RiskStatus.Delete();
			}
		}
	}
}
