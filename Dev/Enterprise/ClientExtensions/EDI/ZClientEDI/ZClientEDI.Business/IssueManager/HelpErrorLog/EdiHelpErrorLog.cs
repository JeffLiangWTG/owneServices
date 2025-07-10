using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	[CodeProperty(HelpErrorLogSchema.Constants.HE_IssueNumber)]
	[DescriptionProperty(HelpErrorLogSchema.Constants.HE_IssueNumber)]
	public class EdiHelpErrorLog : HelpErrorLog, IWorkItemRelatedItem, IWorkItemSource, IWorkTaskRelatedItemSource, IWorkTaskTreeNode
	{
		#region Schema

		public new abstract class Schema : AutoHelpErrorLog.Schema
		{
			public const string Lookups_StaffCodes = "Lookups+StaffCodes";
		}

		#endregion

		public EdiHelpErrorLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ErrorLogFetchStrategy(this);
		}

		class ErrorLogFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public ErrorLogFetchStrategy(EdiHelpErrorLog parent)
				: base(parent)
			{
			}

			ZQuery PivotQuery
			{
				get
				{
					return GenPivotCollectionRelationship.GetRelatedActivitiesQuery(BusinessObject, Core.Constants.GenPivotTypes.ProcessManagement, includeChildren: false, includeParents: true);
				}
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				foreach (var column in columns)
				{
					if (column.ColumnName == "HE_IssueNumber")
					{
						Factory.AddFetchHint(typeof(GenPivot), PivotQuery);
					}
					else if (column.ColumnName == "HasWorkItems")
					{
						var rows = Factory.Load(typeof(GenPivot), PivotQuery);
						var workItemIds = new List<ZGuid>();
						var incidentIds = new List<ZGuid>();
						foreach (var row in rows)
						{
							if (row is GenPivot pivot)
							{
								if (pivot.XX_Relation1TableCode == WorkItemSchema.Constants.Prefix)
								{
									var workItemId = pivot.XX_Relation1ID;
									if (!workItemIds.Contains(workItemId))
									{
										workItemIds.Add(workItemId);
										Factory.AddFetchHint(typeof(WorkItem), new ZQuery(WorkItemSchema.PK, workItemId));
									}
								}
								else if (pivot.XX_Relation1TableCode == IncidentMainSchema.Constants.Prefix)
								{
									var incidentId = pivot.XX_Relation1ID;
									if (!incidentIds.Contains(incidentId))
									{
										incidentIds.Add(incidentId);
										Factory.AddFetchHint(typeof(IncidentMainBase), new ZQuery(IncidentMainSchema.PK, incidentId));
									}
								}
							}
						}
					}
					else if (column.ColumnName == "FirstKeyString")
					{
						var query = new ZQuery(HelpErrorLogKeySchema.HK_HE, BusinessObject.PK);
						Factory.AddFetchHint(typeof(HelpErrorLogKey), query);
					}
				}
			}

			protected override void FetchForFactorySaveCore()
			{
				if (BusinessObject.HasChanges)
				{
					Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.PK);
				}
			}
		}

		#endregion

		#region Lookups

		public new EdiHelpErrorLogLookups Lookups => (EdiHelpErrorLogLookups)base.Lookups;

		protected override HelpErrorLogLookups GetNewLookups() => new EdiHelpErrorLogLookups(this);

		#endregion

		#region Validation

		protected override HelpErrorLogValidation GetNewValidation() => new EdiHelpErrorLogValidation(this);

		#endregion

		#region Related Business Objects

		#region Incidents

		public SupportIncidentCollection RelatedIncidents
		{
			get
			{
				if (fRelatedIncidents == null)
				{
					var localRelatedIncidents = new SupportIncidentCollection(Factory, GetRelatedIncidentsFilter());
					localRelatedIncidents.Load();
					fRelatedIncidents = localRelatedIncidents;
					fRelatedIncidents.SetReadOnlyIncludingChildren(true);
				}
				return fRelatedIncidents;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Field set in RelatedIncidents property getter")]
		public void ReloadRelatedIncidents()
		{
			if (fRelatedIncidents != null)
			{
				fRelatedIncidents.Load();
			}
		}

		ZQuery GetRelatedIncidentsFilter()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(SupportIncident));

			var logQuery = new ZQuery(HelpErrorLogSchema.PK, PK);

			var incidentLogQuery = GenPivot.Query.GetPivotQuery(false, IncidentMainSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
				typeof(EdiHelpErrorLog), HelpErrorLogSchema.Constants.Prefix, logQuery);

			var workItemLogQuery = GenPivot.Query.GetPivotQuery(false, WorkItemSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
				typeof(EdiHelpErrorLog), HelpErrorLogSchema.Constants.Prefix, logQuery);
			var incidentWorkItemQuery1 = GenPivot.Query.GetPivotQuery(false, IncidentMainSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
				typeof(NewWorkItem), WorkItemSchema.Constants.Prefix, null, workItemLogQuery);
			var incidentWorkItemQuery2 = GenPivot.Query.GetPivotQuery(true, IncidentMainSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
				typeof(NewWorkItem), WorkItemSchema.Constants.Prefix, null, workItemLogQuery);

			result.AddSubQuery(incidentLogQuery, JoinCondition.And);
			result.AddSubQuery(incidentWorkItemQuery1, JoinCondition.Or);
			result.AddSubQuery(incidentWorkItemQuery2, JoinCondition.Or);

			return result;
		}

		SupportIncidentCollection fRelatedIncidents;

		#endregion

		#region Keys

		public HelpErrorLogKeyCollection Keys
		{
			get
			{
				if (keys == null)
				{
					keys = new HelpErrorLogKeyCollection(this, Factory);
					keys.Load();
				}

				return keys;
			}
		}

		HelpErrorLogKeyCollection keys;

		public ZInt KeyCount
		{
			get { return Keys.Count; }
		}

		public HelpErrorLogKey FirstKey
		{
			get
			{
				if (firstKey == null)
				{
					ZQuery filter = new ZQuery(HelpErrorLogKeySchema.HK_HE, PK);
					filter.IncludeBlob(HelpErrorLogKeySchema.HK_Key);
					firstKey = Factory.LoadTop1<HelpErrorLogKey>(filter);
				}

				return firstKey;
			}
		}

		HelpErrorLogKey firstKey;

		public ZString FirstKeyString
		{
			get { return FirstKey != null ? FirstKey.HK_Key : ZString.Empty; }
		}

		public ZInt FirstKeyHashCode
		{
			get { return FirstKey != null ? FirstKey.HK_HashCode : ZInt.Zero; }
		}

		#endregion

		#region Occurences

		public HelpErrorLogOccurrenceCollection Occurrences
		{
			get
			{
				if (occurrences == null)
				{
					occurrences = new HelpErrorLogOccurrenceCollection(this);
					occurrences.Load();
				}

				return occurrences;
			}
		}

		HelpErrorLogOccurrenceCollection occurrences;

		public HelpErrorLogOccurrence FirstOccurrence
		{
			get
			{
				if (firstOccurrence == null || firstOccurrence.IsDeleted)
				{
					ZQuery filter = new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, PK);
					filter.IncludeBlob(HelpErrorLogOccurrenceSchema.HO_CompressedXmlData);
					firstOccurrence = Factory.LoadTop1<HelpErrorLogOccurrence>(filter);
				}

				return firstOccurrence;
			}
		}

		HelpErrorLogOccurrence firstOccurrence;

		#endregion

		#region Work Items

		public NewWorkItemRelatedCollection RelatedWorkItems
		{
			get
			{
				if (relatedWorkItems == null)
				{
					relatedWorkItems = new NewWorkItemRelatedCollection(this, RelatedLinkType.MasterAlwaysChild);
					relatedWorkItems.Load();
					relatedWorkItems.CountChanged += delegate
					{ ReloadRelatedIncidents(); };
					relatedWorkItems.RelatedItemAdded += delegate
					{ CalculateFixedDateFromRelatedWorkItems(); };
					relatedWorkItems.RelatedItemRemoved += delegate
					{ CalculateFixedDateFromRelatedWorkItems(); };
				}
				return relatedWorkItems;
			}
		}

		NewWorkItemRelatedCollection relatedWorkItems;

		#endregion

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "Issue " + HE_IssueNumber; }
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = "Issue " + HE_IssueNumber;

				if (!string.IsNullOrEmpty(ExceptionMessageFirstLine))
				{
					result += " - " + ExceptionMessageFirstLine;
				}

				return result;
			}
		}

		public override string ToString()
		{
			string result = OccurrencesText;
			if (!HE_ExceptionMessage.IsEmpty)
			{
				result = HE_ExceptionMessage + ", " + result;
			}
			return result;
		}

		#endregion

		#region Properties

		public ZDateTime HE_FirstProcessedLocal
		{
			get { return HE_FirstProcessed.ToLocalBranchTime(); }
		}

		public ZDateTime HE_FirstReportedLocal
		{
			get { return HE_FirstReported.ToLocalBranchTime(); }
		}

		public ZDateTime HE_LastReportedLocal
		{
			get { return HE_LastReported.ToLocalBranchTime(); }
		}

		public ZDateTime HE_FirstEXEVersionDateLocal
		{
			get { return HE_FirstEXEVersionDate.ToLocalBranchTime(); }
		}

		public ZDateTime HE_LastEXEVersionDateLocal
		{
			get { return HE_LastEXEVersionDate.ToLocalBranchTime(); }
		}

		public ZDateTime HE_FixedDateLocal
		{
			get { return HE_FixedDate.ToLocalBranchTime(); }
			set { HE_FixedDate = value.ToUniversalBranchTime(); }
		}

		public ZWrappedPropertyInfo HE_FixedDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.HE_FixedDate, x => HE_FixedDateInfo); }
		}

		#region ReadOnly

		public bool HE_ExceptionMessage_ReadOnly
		{
			get { return true; }
		}

		public bool HE_ExceptionSource_ReadOnly
		{
			get { return true; }
		}

		public bool HE_ExceptionType_ReadOnly
		{
			get { return true; }
		}

		public bool HE_FailCount_ReadOnly
		{
			get { return true; }
		}

		public bool HE_FirstReported_ReadOnly
		{
			get { return true; }
		}

		public bool HE_LastReported_ReadOnly
		{
			get { return true; }
		}

		#endregion

		public string ExceptionMessageFirstLine
		{
			get { return HE_ExceptionMessage.Split('\n')[0].Trim(); }
		}

		public bool HasAutomaticallyCreatedIncidents
		{
			get
			{
				var autoCreateLog = StmALogEntryLocator.Instance.GetLastPostEventOfType(this, AutoEvents.AutoMatchDone);
				return autoCreateLog != null && !autoCreateLog.IsCancelled;
			}
		}

		public ZBool HasWorkItems
		{
			get { return RelatedWorkItems.Count > 0; }
		}

		public ZPropertyInfo HasWorkItemsInfo
		{
			get { return GetZPropertyInfo(nameof(HasWorkItems)); }
		}

		public virtual ZString HE_ExceptionMessageWithoutLineBreaks
		{
			get { return new ZString(GetValueFromRowSafely(HelpErrorLogSchema.HE_ExceptionMessage)).Replace("\r", "").Replace("\n", ""); }
		}

		public ZString OccurrencesText
		{
			get { return Occurrences.ToString() + " / " + Keys.Count.ToString(CultureInfo.InvariantCulture) + " Key" + (Keys.Count == 1 ? "" : "s"); }
		}

		public ZPropertyInfo OccurrencesTextInfo
		{
			get { return GetZPropertyInfo(nameof(OccurrencesText)); }
		}

		[BusinessObjectTestExclude]
		public ZBool LoadFinalKeys
		{
			get
			{
				return loadFinalKeys || Occurrences.Count < 51;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(LoadFinalKeysInfo, ref loadFinalKeys, value);
				}
				Occurrences.RefreshBinding();
			}
		}

		ZBool loadFinalKeys;

		public ZPropertyInfo LoadFinalKeysInfo
		{
			get { return GetZPropertyInfo(nameof(LoadFinalKeys)); }
		}

		public ZString UserEnteredBodyTextForAssignToEmail
		{
			get;
			set;
		}

		#endregion

		#region Create Incidents

		public bool ShouldCreateIncidentsForNewOccurrences
		{
			get { return !HE_FixedDate.IsEmpty && HasAutomaticallyCreatedIncidents; }
		}

		public void CreateIncident(HelpErrorLogOccurrence occurrence)
		{
			if (HasWorkItems)
			{
				CreateIncidentsCore(new HelpErrorLogOccurrence[] { occurrence });
			}
		}

		public SupportIncident[] CreateIncidents(bool save)
		{
			SortInfo sortInfo = RelatedIncidents.SortInformation;
			SupportIncident[] result = Array.Empty<SupportIncident>();
			if (HasWorkItems)
			{
				ZQuery query = new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, PK);
				query.FetchOnlyFromLocalCache = true;
				HelpErrorLogOccurrence[] newOccurrences = Factory.Load<HelpErrorLogOccurrence>(query);
				result = CreateIncidentsCore(newOccurrences);
			}
			if (!HasAutomaticallyCreatedIncidents)
			{
				Logs.AddNew(Events.AutoMatchDone, "Automatically created incidents");
			}
			if (save)
			{
				Factory.Save();
			}
			if (sortInfo != null)
			{
				RelatedIncidents.Sort(sortInfo);
			}
			return result;
		}

		SupportIncident[] CreateIncidentsCore(HelpErrorLogOccurrence[] occurrenceList)
		{
			List<SupportIncident> result = new List<SupportIncident>();

			HashSet<ZGuid> relatedIncidentClientCompanyPKs = new HashSet<ZGuid>();

			foreach (SupportIncident incident in RelatedIncidents)
			{
				var clientCompany = incident.ClientCompany;
				if (clientCompany != null)
				{
					relatedIncidentClientCompanyPKs.Add(clientCompany.PK);
				}
			}

			foreach (HelpErrorLogOccurrence occurrence in occurrenceList)
			{
				ClientCompany clientCompany = occurrence.ClientCompany;
				if (clientCompany == null)
				{
					var db = occurrence.Database;
					if (db != null)
					{
						var companiesWithActiveOrg = db.ClientCompanies
							.Where(x => x.Org?.OH_IsActive ?? false)
							.ToList();
						if (companiesWithActiveOrg.Count == 1)
						{
							clientCompany = companiesWithActiveOrg[0];
						}
					}
				}

				if (clientCompany != null
					&& !relatedIncidentClientCompanyPKs.Contains(clientCompany.PK)
					&& (clientCompany.Org == null || clientCompany.Org.OH_IsActive))
				{
					relatedIncidentClientCompanyPKs.Add(clientCompany.PK);
					SupportIncident incident = Factory.New<SupportIncident>();
					incident.PopulateFromIssueOccurrence(occurrence, clientCompany);
					foreach (NewWorkItem workItem in RelatedWorkItems)
					{
						incident.RelatedItems.Add(workItem);
					}
					result.Add(incident);
					RelatedIncidents.Add(incident);
				}
			}

			return result.ToArray();
		}

		public static string GetIncidentCreatedMessage(List<SupportIncident> incidents)
		{
			incidents.Sort((x, y) => { return x.IM_IncidentNumber.CompareTo(y.IM_IncidentNumber); });

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("The following incidents have been created:");

			foreach (SupportIncident incident in incidents)
			{
				builder.AppendLine();
				EDIOrgHeader client = (EDIOrgHeader)incident.Client;
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0} for {1} - {2}", incident.IM_IncidentNumber, client.OH_Code, client.OH_FullName);
			}

			return builder.ToString();
		}

		#endregion

		#region Save, Load & Delete

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			autoClosed = false;
		}

		public override void Delete()
		{
			Occurrences.RemoveAndDeleteAll();
			Keys.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			SetIssueNumber();
			UpdateFixedCount();

			if (ShouldCreateIncidentsForNewOccurrences)
			{
				CreateIncidents(false);
			}
		}

		protected override void ReloadCore()
		{
			base.ReloadCore();
			if (relatedWorkItems != null)
			{
				relatedWorkItems.Load();
			}
			ReloadRelatedIncidents();
		}

		#endregion

		#region Implementation

		public void AutoClose(ZDateTime closedTime)
		{
			HE_FixedDate = closedTime;
			Logs.AddNew(Events.IncidentClosed, "Auto-Closed due to Exe Date before " + closedTime.ToShortDateString());
			autoClosed = true;
		}

		public void AutoReOpen()
		{
			Logs.AddNew(Events.IncidentReopened, "Auto-ReOpened due to Exe Date after " + HE_FixedDate.ToShortDateString());
			HE_FixedDate = ZDateTime.Empty;
		}

		public void SetIssueNumber()
		{
			if (HE_IssueNumber.IsEmpty)
			{
				HE_IssueNumber = Modules.ClientNumberFountainRegistration.GetInstance().IssueNo.GetNextFormatted(Factory);
				AfterIssueNumberSetAction?.Invoke();
			}
		}

		public void AfterIssueNumberSet(Action action)
		{
			if (HE_IssueNumber.IsEmpty)
			{
				if (AfterIssueNumberSetAction == null)
				{
					AfterIssueNumberSetAction = action;
				}
				else
				{
					var oldAction = AfterIssueNumberSetAction;
					AfterIssueNumberSetAction = new Action(() =>
					{
						oldAction();
						action();
					});
				}
			}
			else
			{
				action();
			}
		}
		Action AfterIssueNumberSetAction;

		void UpdateFixedCount()
		{
			if (!autoClosed && !HE_FixedDate.IsEmpty && (HE_FixedDate != (ZDateTime)HE_FixedDateInfo.OriginalValue || !IsInDatabase))
			{
				HE_FixedCount++;
			}
		}

		public void UpdateFirstAndLastReported(HelpErrorLogOccurrence occurrence)
		{
			if (occurrence.HO_ExceptionDateTime > HE_LastReported || HE_LastReported.IsEmpty)
			{
				HE_LastReported = occurrence.HO_ExceptionDateTime;
			}

			if (occurrence.HO_ExceptionDateTime < HE_FirstReported || HE_FirstReported.IsEmpty)
			{
				HE_FirstReported = occurrence.HO_ExceptionDateTime;
			}

			if (HE_FirstProcessed.IsEmpty)
			{
				HE_FirstProcessed = ZDateTime.Now;
			}
		}

		public void UpdateFirstAndLastExeVersionDate(HelpErrorLogOccurrence occurrence)
		{
			if (occurrence.HO_EXEDateTime > HE_LastEXEVersionDate || HE_LastEXEVersionDate.IsEmpty)
			{
				HE_LastEXEVersionDate = occurrence.HO_EXEDateTime;
			}

			if (occurrence.HO_EXEDateTime < HE_FirstEXEVersionDate || HE_FirstEXEVersionDate.IsEmpty)
			{
				HE_FirstEXEVersionDate = occurrence.HO_EXEDateTime;
			}
		}

		public void UpdateFirstAndLastVersionNumber(HelpErrorLogOccurrence occurrence)
		{
			if (!string.IsNullOrEmpty(occurrence.HO_VersionNumber))
			{
				var occurrenceVersion = new Version(occurrence.HO_VersionNumber);
				if (HE_LastVersionNumber.IsEmpty || occurrenceVersion > new Version(HE_LastVersionNumber))
				{
					HE_LastVersionNumber = occurrence.HO_VersionNumber;
				}

				if (HE_FirstVersionNumber.IsEmpty || occurrenceVersion < new Version(HE_FirstVersionNumber))
				{
					HE_FirstVersionNumber = occurrence.HO_VersionNumber;
				}
			}
		}

		bool autoClosed;

		#endregion

		#region IWorkItemRelatedItem Members

		public ZString Type
		{
			get { return EDIWorkTaskRelatedItemTypes.Issue; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.TypeInfo
		{
			get { return GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.Type"); }
		}

		public ZString ClientName
		{
			get { return FirstOccurrence != null ? FirstOccurrence.HO_Company : ZString.Empty; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.ClientNameInfo
		{
			get { return GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.ClientName"); }
		}

		public ZString ClientCode
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.ClientCodeInfo
		{
			get { return GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.ClientCode"); }
		}

		public ZString Number
		{
			get { return HE_IssueNumber; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.NumberInfo
		{
			get { return HE_IssueNumberInfo; }
		}

		public ZString StatusDescription
		{
			get { return HE_FixedDate.IsEmpty ? "Not Fixed : " + HE_FailCount.ToString() : "Fixed : " + HE_FixedDate.ToShortDateString(); }
		}

		ZPropertyInfo IWorkTaskRelatedItem.StatusDescriptionInfo
		{
			get { return GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.StatusDescription"); }
		}

		public ZString AssignedStaffCode
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.AssignedStaffCodeInfo
		{
			get { return GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.AssignedStaffCode"); }
		}

		public ZString ItemDescription
		{
			get { return HE_ExceptionMessage; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.ItemDescriptionInfo
		{
			get { return HE_ExceptionMessageInfo; }
		}

		bool IWorkItemRelatedItem.OnRelatedWorkItemClosed(WorkItem workItem)
		{
			if (RelatedWorkItems.Cast<WorkItem>().All(wi => wi.IsClosedOrCancelled))
			{
				HE_FixedDate = ZDateTime.UtcNow;
			}
			Logs.AddNew(Events.IncidentClosed, workItem.WKI_WorkItemNumber + " closed " + HE_FixedDate.ToShortDateString());
			return true;
		}

		void IWorkItemRelatedItem.OnRelatedWorkItemReOpened(WorkItem workItem)
		{
			HE_FixedDate = ZDateTime.Empty;
			Logs.AddNew(Events.IncidentReopened, workItem.WKI_WorkItemNumber);
		}

		void IWorkItemRelatedItem.OnWorkItemAdded(WorkItem workItem)
		{
			CalculateFixedDateFromRelatedWorkItems();
		}

		void IWorkItemRelatedItem.OnWorkItemRemoved(WorkItem workItem)
		{
			CalculateFixedDateFromRelatedWorkItems();
		}

		void CalculateFixedDateFromRelatedWorkItems()
		{
			ZDateTime fixedDate = ZDateTime.Empty;
			NewWorkItem closedWorkItem = null;

			foreach (NewWorkItem workItem in RelatedWorkItems)
			{
				if (!workItem.IsClosedOrCancelled)
				{
					fixedDate = ZDateTime.Empty;
					break;
				}

				ZDateTime jobCloseDateUtc = workItem.JobCloseDateUtc;
				if (jobCloseDateUtc.IsEmpty)
				{
					fixedDate = ZDateTime.UtcNow;
					closedWorkItem = workItem;
					break;
				}

				if (fixedDate.IsEmpty)
				{
					fixedDate = jobCloseDateUtc;
					closedWorkItem = workItem;
				}
				else if (jobCloseDateUtc > fixedDate)
				{
					fixedDate = jobCloseDateUtc;
					closedWorkItem = workItem;
				}
			}

			HE_FixedDate = fixedDate;
			if (closedWorkItem != null && fixedDate != ZDateTime.Empty)
			{
				Logs.AddNew(Events.IncidentClosed, closedWorkItem.WKI_WorkItemNumber + " closed " + fixedDate.ToShortDateString());
			}
		}

		ControllerID IWorkTaskRelatedItem.ControllerID => ClientControllerRegistration.IssueManager;

		public ZString Criticality => ZString.Empty;

		ZPropertyInfo IWorkTaskRelatedItem.CriticalityInfo => GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.Criticality");

		public ZString Source => ZString.Empty;

		ZPropertyInfo IWorkTaskRelatedItem.SourceInfo => GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.Source");

		public ZBool IsClosedOrCancelled => !HE_FixedDate.IsEmpty && HE_FixedDate.IsValid;

		public ZString SelectionCriterion1 => string.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.SelectionCriterion1Info => GetZPropertyInfo(nameof(IWorkTaskRelatedItem.SelectionCriterion1));
		public ZString SelectionCriterion2 => string.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.SelectionCriterion2Info => GetZPropertyInfo(nameof(IWorkTaskRelatedItem.SelectionCriterion2));
		public ZString SelectionCriterion3 => string.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.SelectionCriterion3Info => GetZPropertyInfo(nameof(IWorkTaskRelatedItem.SelectionCriterion3));
		public ZString SelectionCriterion4 => string.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.SelectionCriterion4Info => GetZPropertyInfo(nameof(IWorkTaskRelatedItem.SelectionCriterion4));
		public ZString SelectionCriterion5 => string.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.SelectionCriterion5Info => GetZPropertyInfo(nameof(IWorkTaskRelatedItem.SelectionCriterion5));

		public Type PivotCollectionType => typeof(GenPivotCollection);

		#endregion

		#region IWorkItemSource Members

		public void PopulateWorkItem(NewWorkItem workItem)
		{
			Argument.NotNull(workItem, "workItem");

			workItem.WKI_WorkItemType = ProductTypes.Codes.Enterprise;
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;
			ZString description = ExceptionMessageFirstLine;
			workItem.WKI_Summary = description.SubstringSafe(0, IncidentMainSchema.IM_Description.MaxLength);
			workItem.WKI_Details = ZBlob.FromUTF8(HE_ExceptionMessage);
			workItem.Logs.AddNew(Events.AutoMatchDone, "Created from issue " + HE_IssueNumber);
			RelatedWorkItems.Add(workItem);
		}

		public void CreateWorkItem()
		{
			ZString product;
			ZString productArea;
			ZString productModule;
			if (FindWorkItemInfo(this.HE_ExceptionSource, out product, out productArea, out productModule))
			{
				NewWorkItem workItem = Factory.New<NewWorkItem>();
				PopulateWorkItem(workItem);
				workItem.WKI_WorkItemType = product;
				workItem.WKI_WorkItemArea = productArea;
				workItem.WKI_ActivityType = productModule;
				workItem.WKI_Priority = "GPR";
				RelatedWorkItems.Add(workItem);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static bool FindWorkItemInfo(string assemblyName, out ZString productArea, out ZString product, out ZString productModule)
		{
			var productAreaFound = false;
			productArea = ZString.Empty;
			product = ZString.Empty;
			productModule = ZString.Empty;

			using (var connection = Db.NewExtraConnection("syddat.db.wtg.zone", "AutoTester_UserTests", "AutoTester", "builder"))
			using (var command = connection.Command(string.Format(CultureInfo.InvariantCulture, AssembliesSql, assemblyName)))
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					product = reader[0].ToString();
					productArea = reader[1].ToString();
					productModule = reader[2].ToString();
					productAreaFound = true;
				}
			}
			return productAreaFound;
		}

		const string AssembliesSql = @"
			SELECT E8_Product, E8_ProductArea , E8_Module
			FROM Assembly
			WHERE E8_AssemblyName like '{0}'";

		#endregion

		#region IWorkTaskRelatedItemSource Members

		WorkTaskRelatedItemCollection IWorkTaskRelatedItemSource.RelatedItems => null;

		ZBool IWorkTaskRelatedItemSource.ShowOnlyNonClosedItems { get; set; }

		FilteredWorkTaskRelatedItemCollection IWorkTaskRelatedItemSource.FilteredRelatedItems => null;

		IEnumerable<WorkTaskRelatedItemModuleInfo> IWorkTaskRelatedItemSource.SupportedRelatedItemModules => Array.Empty<WorkTaskRelatedItemModuleInfo>();

		void IWorkTaskRelatedItemSource.PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
		{
		}

		public ZBool ShouldAddRelatedItemAsParent { get; set; }

		#endregion

		#region IWorkTaskTreeNode Members

		public BusinessObjectCollection ChildrenOnlyRelatedItems => null; // Doesn't implement TreeView on Related Items
		public BusinessObjectCollection ParentsOnlyRelatedItems => null; // Doesn't implement TreeView on Related Items

		public ZDateTime AgreedDeliveryDate => ZDateTime.Empty;

		public ZString CurrentTaskStatus => ZString.Empty;

		public ZString CurrentTaskDescription => ZString.Empty;

		public ZString CurrentTaskCapabilityCodeDescription => ZString.Empty;

		public ZString CurrentTaskAssigned => ZString.Empty;

		public ZString SelectionCriterion1Code => ZString.Empty;

		public ZString SelectionCriterion2Code => ZString.Empty;

		public ZString SelectionCriterion3Code => ZString.Empty;

		public ZString SelectionCriterion4Code => ZString.Empty;

		public ZString SelectionCriterion5Code => ZString.Empty;

		public void AddFetchHintsForOrgAddressIfRequired()
		{
		}

		public void AddFetchHintsForOrgHeaderIfRequired()
		{
		}

		#endregion

		#region IAuditDetails

		ZDateTime IAuditDetails.SystemCreateTimeUtc => ZDateTime.Empty;

		ZString IAuditDetails.SystemCreateUser => string.Empty;

		ZDateTime IAuditDetails.SystemLastEditTimeUtc => ZDateTime.Empty;

		ZString IAuditDetails.SystemLastEditUser => string.Empty;

		#endregion
	}
}

