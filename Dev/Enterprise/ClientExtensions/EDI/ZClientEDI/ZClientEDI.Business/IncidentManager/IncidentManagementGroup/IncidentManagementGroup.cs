using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Workflow;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroupConstants;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeProperty(IncidentManagementGroupSchema.Constants.ING_IncidentGroupNumber), DescriptionProperty(IncidentManagementGroupSchema.Constants.ING_Description)]
	[UserDefinedValues]
	[UniversalCopyWithExtendedEntities]
	public class IncidentManagementGroup :
		AutoIncidentManagementGroup,
		IWorkTaskRelatedItemSource,
		IWorkTaskRelatedItem,
		IWorkTaskTreeNode,
		IWorkflowProvider,
		IIncidentDetailsSource,
		IConversationProvider,
		IWorkflowProviderCore,
		ICustomFieldProvider,
		IDocumentSupportable,
		IBusiness,
		IJobHeaderParent,
		IJobNumber,
		IDocManagerSupport,
		ITriageAssistParent
	{
		public IncidentManagementGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ING_Status), ConcurrencyPolicy.Strict);
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = this.Number;
				if (!this.ING_Description.IsEmpty)
				{
					result += $" - {ING_Description}";
				}
				return result;
			}
		}

		public bool BroadcastFromPopupForm { get; set; }

		public bool TriggerFromPopupForm { get; set; }

		#region Collection

		public IncidentManagementGroupMessageCollection IncidentManagementGroupMessages
		{
			get
			{
				if (incidentManagementGroupMessages == null)
				{
					var localIncidentManagementGroupMessages = new IncidentManagementGroupMessageCollection(Factory, PK);
					localIncidentManagementGroupMessages.Load();
					incidentManagementGroupMessages = localIncidentManagementGroupMessages;
					incidentManagementGroupMessages.HasChangesChanged += IncidentManagementGroupMessages_HasChangesChanged;

					if (incidentManagementGroupMessages.Count == 0)
					{
						CreateDefaultIncidentManagementMessages();
					}
				}
				return incidentManagementGroupMessages;
			}
		}
		IncidentManagementGroupMessageCollection incidentManagementGroupMessages;

		void CreateDefaultIncidentManagementMessages()
		{
			if (incidentManagementGroupMessages != null)
			{
				var messageAutoReply = incidentManagementGroupMessages.AddNew();
				messageAutoReply.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
				messageAutoReply.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage, false);

				var messageClosing = incidentManagementGroupMessages.AddNew();
				messageClosing.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
				messageClosing.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage, false);

				var messageInterim = incidentManagementGroupMessages.AddNew();
				messageInterim.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
				messageInterim.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage, false);

				var messageOpening = incidentManagementGroupMessages.AddNew();
				messageOpening.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
				messageOpening.SetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage, false);
			}
		}

		void IncidentManagementGroupMessages_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (!HasChanges && e.ObjectJustWasChanged)
			{
				HasChanges = true;
			}
		}

		[ChildEditable(false)]
		public IncidentManagementLinkCollection LinkedIncidents
		{
			get
			{
				if (linkedIncidents == null)
				{
					var query = new ZQuery(IncidentManagementLinkSchema.INL_ING_Group, PK);
					var localLinkedIncidents = new IncidentManagementLinkCollection(Factory, query);
					localLinkedIncidents.Load();
					linkedIncidents = localLinkedIncidents;
					RegisterEditableChildObject(linkedIncidents);
				}
				return linkedIncidents;
			}
		}
		IncidentManagementLinkCollection linkedIncidents;

		public WorkTaskRelatedItemCollection AutoCascadeRelatedItems
		{
			get
			{
				if (autoCascadeRelatedItems == null)
				{
					var localAutoCascadeRelatedItems = new IncidentGroupAutoCascadeWorkItemCollection(this);
					localAutoCascadeRelatedItems.Load();
					autoCascadeRelatedItems = localAutoCascadeRelatedItems;
				}
				return autoCascadeRelatedItems;
			}
		}

		WorkTaskRelatedItemCollection autoCascadeRelatedItems;

		#endregion

		#region Saving

		public override void OnSaving()
		{
			SetIncidentNumberIfRequired();
			SetWorkItemsCascadeToAllIncidents();
			CascadeProductCriticalityAndTriage();
			LogStatusChange();
			AddProductDetailsChangedWorkflowEvent();
			EnactControlOnLinkedIncidentsAfterStatusChange();

			if (HasTemplateSelectionCriteriaValuesChanged)
			{
				LastMatchingTemplates = MatchingTemplates;
			}

			CompleteIncidentsOnStageChange();
			base.OnSaving();
		}

		#region Broadcast Message

		bool ShouldBroadcastMessageOnStatusChange => TriggerFromPopupForm ? BroadcastFromPopupForm : ING_StatusInfo.HasChanges;

		void BroadcastMessageOnStatusChange()
		{
			if (ShouldBroadcastMessageOnStatusChange)
			{
				var exisitngIncidentLinksToBroadcast = LinkedIncidents.Where(link => link.IsInDatabase && link.CanReceiveBroadcastMessages);
				if (exisitngIncidentLinksToBroadcast.Any())
				{
					var message = GetBroadcastMessage(checkLastStatusSequence: true);
					if (message != null)
					{
						foreach (var existingLink in exisitngIncidentLinksToBroadcast)
						{
							SendBroadcastMessage(existingLink, message, false);
						}
					}
				}

				var newIncidentLinksToBroadcast = LinkedIncidents.Where(link => !link.IsInDatabase && link.CanReceiveBroadcastMessages);
				if (newIncidentLinksToBroadcast.Any())
				{
					var message = GetBroadcastMessage(checkLastStatusSequence: false);
					if (message != null)
					{
						foreach (var newLink in newIncidentLinksToBroadcast)
						{
							SendBroadcastMessage(newLink, message, false);
						}
					}
				}
			}
		}

		public void SendBroadcastMessage(IncidentManagementLink link, bool isSupportSender = true, bool ignoreInterimMessage = false)
		{
			if (ShouldBroadcastMessageOnStatusChange)
			{
				return;
			}

			var message = GetBroadcastMessage(checkLastStatusSequence: false, ignoreInterimMessage: ignoreInterimMessage);
			if (message == null)
			{
				return;
			}

			SendBroadcastMessage(link, message, isSupportSender);
		}

		public void SendBroadcastMessage(IncidentManagementLink link, IncidentManagementGroupMessage message, bool isSupportSender = true)
		{
			var incident = link?.SupportIncident;
			if (incident != null)
			{
				if (isSupportSender)
				{
					incident.AddMessageFromSupport(message.IGM_Message, shouldAddMessageSentEvent: false);
				}
				else
				{
					if (GroupOwner == null)
					{
						incident.AddStaffMessageToCustomer(message.IGM_Message, shouldAddMessageSentEvent: false);
					}
					else
					{
						using (Env.SetTemporaryUserContext(GroupOwner.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
						{
							incident.AddStaffMessageToCustomer(message.IGM_Message, shouldAddMessageSentEvent: false);
						}
					}
				}

				GenerateBroadcastMessageLog(link, message);
			}
		}

		public IncidentManagementGroupMessage GetBroadcastMessage(bool checkLastStatusSequence, bool ignoreInterimMessage = false)
		{
			var currentStatusSequence = Stages.GetSequenceByCode(ING_Status);

			if (currentStatusSequence.IsEmpty)
			{
				return null;
			}

			if (checkLastStatusSequence)
			{
				var lastStatusSequence = Stages.GetSequenceByCode((ZString)ING_StatusInfo.OriginalValue);
				if (!lastStatusSequence.IsEmpty)
				{
					if (currentStatusSequence >= IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence
						&& currentStatusSequence < IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence
						&& lastStatusSequence >= IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence)
					{
						return null;
					}
					else if (currentStatusSequence == IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence
						&& lastStatusSequence >= IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence)
					{
						return null;
					}
				}
			}

			IncidentManagementGroupMessage result = null;
			var publishedMessages = IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>().Where(message => message.IGM_IsPublished);

			if (currentStatusSequence >= IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence
				&& currentStatusSequence < IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence)
			{
				if (!ignoreInterimMessage)
				{
					result = publishedMessages.FirstOrDefault(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Interim);
				}

				if (result == null)
				{
					result = publishedMessages.FirstOrDefault(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Opening);
				}
			}
			else if (currentStatusSequence == IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence)
			{
				result = publishedMessages.FirstOrDefault(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Closing);
			}

			return result;
		}

		void GenerateBroadcastMessageLog(IncidentManagementLink link, IncidentManagementGroupMessage message)
		{
			var messageType = message.IGM_Type;
			var description = FormattableString.Invariant($"{messageType} Broadcast Message sent to {link.SupportIncident.Number}");

			var paramList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, messageType),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, link.SupportIncident.Number),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, description)
			};

			link.SupportIncident.Logs.AddNew(AutoEvents.BroadcastMessage, paramList.ToArray());
			Logs.AddNew(AutoEvents.BroadcastMessage, paramList.ToArray());

			var shouldPostUnflaggedEvent = false;
			switch (messageType)
			{
				case IncidentManagementGroupMessageTypePairList.Codes.Opening:
					shouldPostUnflaggedEvent = true;
					break;
				case IncidentManagementGroupMessageTypePairList.Codes.Interim:
					shouldPostUnflaggedEvent = ING_IsInterimBroadcastUnflagsCommunication;
					break;
				case IncidentManagementGroupMessageTypePairList.Codes.AutoReply:
					shouldPostUnflaggedEvent = ING_IsAutoReplyUnflagsCommunication;
					break;
				case IncidentManagementGroupMessageTypePairList.Codes.Closing:
					shouldPostUnflaggedEvent = true;
					break;
				default:
					shouldPostUnflaggedEvent = false;
					break;
			}

			if (shouldPostUnflaggedEvent)
			{
				link.CreateOrUpdateUnflaggedEvent();
			}
		}

		#endregion

		#region CascadeProductAndCriticalityAndTriage

		void CascadeProductCriticalityAndTriage()
		{
			LinkedIncidents.Reload(reLoadExistingRows: false, assumeRowsMissingFromQueryResultsAreDeleted: true);
			if (LinkedIncidents.Count != 0)
			{
				foreach (IncidentManagementLink link in LinkedIncidents)
				{
					if ((!originalCascadeProductDetails || HasProductDetailsChanged) && CascadeProductDetails)
					{
						link.CascadeGroupParameters();
					}

					link.CascadeTriage();
				}
			}
		}

		ZBool originalCascadeProductDetails;

		public ZBool HasProductDetailsChanged
		{
			get
			{
				return
				ING_PriorityInfo.HasChanges
				|| ING_ProductInfo.HasChanges
				|| ING_ProductAreaInfo.HasChanges
				|| ING_ModuleInfo.HasChanges
				|| ING_ServiceTypeInfo.HasChanges
				|| ING_SourceModuleIdInfo.HasChanges;
			}
		}

		#endregion

		protected ZBool HasProductDetailsChangedForPostWorkflow
		{
			get
			{
				return
				ING_ProductInfo.HasChanges
				|| ING_ProductAreaInfo.HasChanges
				|| ING_ModuleInfo.HasChanges
				|| ING_SourceModuleIdInfo.HasChanges;
			}
		}

		protected void SetIncidentNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(ING_IncidentGroupNumberInfo, Fountains.IncidentManagementGroupNumber);
			}
		}

		readonly ClientNumberFountainRegistration Fountains = ClientNumberFountainRegistration.GetInstance();

		void CompleteIncidentsOnStageChange()
		{
			if (previousStageIncidentCompletedValue != null && !previousStageIncidentCompletedValue.Value && NowStage != null && NowStage.IncidentCompleted)
			{
				foreach (var incidentManagementLink in LinkedIncidents.Where(l => l.INL_IsGroupControlled))
				{
					var incident = incidentManagementLink.SupportIncident;
					if (!incident.RelatedWorkItems.Any())
					{
						incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, string.Empty);
						incident.SetIncidentClosedDate();
					}

					var parameters = new List<KeyValuePair<string, string>>
						{
							new KeyValuePair<string, string>("DES", $"Incident {incident.IM_IncidentNumber} closed by Incident Management Group {ING_IncidentGroupNumber}"),
							new KeyValuePair<string, string>("INC", incident.IM_IncidentNumber),
							new KeyValuePair<string, string>("ING", ING_IncidentGroupNumber)
						};

					Logs.AddNew(Events.IncidentClosed, parameters.ToArray());
					var incidentClosedEvent = incident.Logs.AddNew(Events.IncidentClosed, parameters.ToArray());
					var incidentClosedCount = incident.Logs.Find(f => f.SL_SE_NKEvent == Events.IncidentClosedCode).Count();

					incident.MetricsCollection.RefreshFromDb();
					incident.UpdateSingleUseMetric(IncidentMetricConstants.TotalERequestAge, incidentClosedEvent.SL_PostedTimeUtc, incidentClosedCount);
				}

				previousStageIncidentCompletedValue = null;
			}
		}

		#endregion

		#region Saved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				originalCascadeProductDetails = CascadeProductDetails;
				EConversation.OnIncidentManagementGroupSaveSucceed();
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			IncidentManagementGroupMessages.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Overrides

		[MaxLength(Schema.ING_IncidentGroupNumberMaxLength)]
		public override ZString ING_IncidentGroupNumber
		{
			get => base.ING_IncidentGroupNumber;
			set => base.ING_IncidentGroupNumber = value;
		}

		[List("Lookups.Types")]
		public override ZString ING_Type
		{
			get => base.ING_Type;
			set
			{
				if (ING_Type != value)
				{
					base.ING_Type = value;
					RefreshStages();
					ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
				}
			}
		}

		public bool ING_Type_ReadOnly => IsInDatabase;

		[List("Lookups.StageList")]
		public override ZString ING_Status
		{
			get => base.ING_Status;
			set
			{
				if (NowStage != null && base.ING_Status != value)
				{
					previousStageIncidentCompletedValue = NowStage.IncidentCompleted;
				}
				base.ING_Status = value;
			}
		}

		ZBool? previousStageIncidentCompletedValue;

		void EnactControlOnLinkedIncidentsAfterStatusChange()
		{
			if (!ING_StatusInfo.HasChanges && IsInDatabase)
			{
				return;
			}

			var newStage = NowStage;
			var oldStage = Stages.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code.EqualsIgnoringCase((ZString)ING_StatusInfo.OriginalValue));

			if ((!IsInDatabase || oldStage == null || oldStage.ControlIncidents == false) && newStage != null && newStage.ControlIncidents == true)
			{
				var linkedIncidents = LinkedIncidents;
				linkedIncidents.Reload(reLoadExistingRows: false, assumeRowsMissingFromQueryResultsAreDeleted: true);
				foreach (var link in linkedIncidents.Cast<IncidentManagementLink>().Where(x => x.INL_IsGroupControlled))
				{
					link.CancelTasksAddLogsAndUpdateStatusOnIncident();
				}
			}
			else if (oldStage != null && oldStage.ControlIncidents == true && newStage != null && newStage.ControlIncidents == false)
			{
				var linkedIncidents = LinkedIncidents;
				linkedIncidents.Reload(reLoadExistingRows: false, assumeRowsMissingFromQueryResultsAreDeleted: true);

				foreach (var incidentLink in linkedIncidents.Where(x => x.INL_IsGroupControlled))
				{
					var groupLogBuilder = EventLogReferenceBuilder.New()
								.AddMandatory(EventReferenceParameters.Codes.ReferenceNumber, incidentLink.SupportIncident.IM_IncidentNumber)
								.AddMandatory(EventReferenceParameters.Codes.JobNumber, ING_IncidentGroupNumber)
								.AddMandatory(EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Control disabled for incident {incidentLink.SupportIncident.IM_IncidentNumber}"));
					var groupLog = groupLogBuilder.Build();
					Logs.AddNew(AutoEvents.UnlockForEdit, groupLog);
					incidentLink.SupportIncident.Logs.AddNew(AutoEvents.UnlockForEdit, groupLog);
				}
			}
			else
			{
				var linkedIncidents = LinkedIncidents;
				linkedIncidents.Reload(reLoadExistingRows: false, assumeRowsMissingFromQueryResultsAreDeleted: true);
				foreach (var link in linkedIncidents.Cast<IncidentManagementLink>().Where(x => x.IsControlled && x.SupportIncident.IM_PriorityInfo.HasChanges
										&& !IncidentConstants.IsDefect((ZString)x.SupportIncident.IM_PriorityInfo.OriginalValue) && x.SupportIncident.IsDefect))
				{
					link.UpdateStatusOnIncidentIfRequired();
				}
			}

			SetWorkItemsCascadeToAllIncidents();
		}

		[List("Lookups.ServiceOutageList")]
		public override ZString ING_ServiceOutage
		{
			get => base.ING_ServiceOutage;
			set => base.ING_ServiceOutage = value;
		}

		[List("Lookups.BusinessImpactList")]
		public override ZString ING_BusinessImpact
		{
			get => base.ING_BusinessImpact;
			set => base.ING_BusinessImpact = value;
		}

		[List("Lookups.UrgencyList")]
		public override ZString ING_Urgency
		{
			get => base.ING_Urgency;
			set => base.ING_Urgency = value;
		}

		[List("Lookups.CriticalityList")]
		public override ZString ING_Priority
		{
			get => base.ING_Priority;
			set => base.ING_Priority = value;
		}

		[List("Lookups.ProductList")]
		public override ZString ING_Product
		{
			get => base.ING_Product;
			set => base.ING_Product = value;
		}

		[List("Lookups.ProductAreaList")]
		public override ZString ING_ProductArea
		{
			get => base.ING_ProductArea;
			set => base.ING_ProductArea = value;
		}

		[List("Lookups.ModuleListEnabledModulesOnly")]
		public override ZString ING_Module
		{
			get => base.ING_Module;
			set
			{
				if (base.ING_Module != value)
				{
					base.ING_Module = value;

					if (!ING_ModuleInfo.HasErrors())
					{
						RecalculateProductArea();
						Validation.ValidateING_Module();
					}
				}
			}
		}

		public ZString ING_ModuleDescription
		{
			get { return Lookups.ProductAreaIndependentModuleList.GetDescriptionFromCode(ING_Module); }
		}

		[List("Lookups.ServiceTypeList")]
		public override ZString ING_ServiceType
		{
			get => base.ING_ServiceType;
			set => base.ING_ServiceType = value;
		}

		[List("Lookups.CountryList")]
		public override ZString ING_RN_NKCountry
		{
			get { return base.ING_RN_NKCountry; }
			set
			{
				if (base.ING_RN_NKCountry != value)
				{
					base.ING_RN_NKCountry = value;
				}
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ING_BusinessImpact = IncidentManagementGroupConstants.BusinessImpactCodes.HighImpact;
			ING_Urgency = IncidentManagementGroupConstants.UrgencyCodes.VeryHigh;
			ING_Product = ProductTypes.Codes.Enterprise;
			ING_GS_NKGroupOwner = "E";
			InitialSymptomsText = "test data";
			BusinessImpactDescriptionText = "test data";
			RootCauseText = "test data";
		}

#endif

		#region Logging

		protected override bool UpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		#endregion

		#endregion

		#region IWorkTaskRelatedItemSource

		public WorkTaskRelatedItemCollection RelatedItems
		{
			get
			{
				if (relatedItems == null)
				{
					var localRelatedItems = new IncidentManagementGroupRelatedItemGenPivotCollection(this);
					localRelatedItems.Load();
					relatedItems = localRelatedItems;
				}
				return relatedItems;
			}
		}
		public WorkTaskRelatedItemCollection relatedItems;

		public IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules => new[] { WorkTaskRelatedItemModuleInfo.Project(Factory, false), WorkTaskRelatedItemModuleInfo.WorkItem(Factory), EDIWorkTaskRelatedItemModuleInfo.GenericIncident(Factory, allowAttach: true, allowNew: true) };

		public ZBool ShowOnlyNonClosedItems { get; set; }

		public FilteredWorkTaskRelatedItemCollection FilteredRelatedItems
		{
			get
			{
				if (filteredRelatedItems == null)
				{
					filteredRelatedItems = new FilteredWorkTaskRelatedItemCollection(RelatedItems);
				}
				return filteredRelatedItems;
			}
		}
		FilteredWorkTaskRelatedItemCollection filteredRelatedItems;

		ZBool IWorkTaskRelatedItemSource.ShouldAddRelatedItemAsParent { get; set; }

		public void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
		{
		}
		 
		#endregion

		#region IWorkTaskRelatedItem

		public ModuleListType ModuleType
		{
			get { return IncidentApprovalLookups.GetModuleListType(ING_Priority); }
		}

		public ZString Type => ResString.GetMultilingualString("90ebe576-c726-46e1-9c35-f23be0f60238", "Incident Management Group");

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		public ZString ItemDescription => ING_Description;

		public ZPropertyInfo ItemDescriptionInfo => ING_DescriptionInfo;

		public ZString StatusDescription
		{
			get => Stages.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code.EqualsIgnoringCase(ING_Status))?.DescriptionOnGroup ?? string.Empty;
		}

		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(nameof(StatusDescription));

		public ZBool IsClosedOrCancelled => NowStage == null ? ZBool.False : NowStage.GroupCompleted;

		public ZBool IncidentCompleted => NowStage == null ? ZBool.False : NowStage.IncidentCompleted;

		public ZBool ControlIncidents => NowStage == null ? ZBool.False : NowStage.ControlIncidents;

		public ZBool CascadeProductDetails => NowStage == null ? ZBool.False : NowStage.CascadeProductDetails;

		public ZInt Sequence => NowStage == null ? ZInt.Zero : NowStage.Sequence;

		#region SelectionCriterion

		public ZString SelectionCriterion1 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.Types, ING_Type);
		public ZPropertyInfo SelectionCriterion1Info => GetZPropertyInfo(nameof(SelectionCriterion1));

		public ZString SelectionCriterion2 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.ProductAreaList, ING_ProductArea);
		public ZPropertyInfo SelectionCriterion2Info => GetZPropertyInfo(nameof(SelectionCriterion2));

		public ZString SelectionCriterion3 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.ProductList, ING_Product);
		public ZPropertyInfo SelectionCriterion3Info => GetZPropertyInfo(nameof(SelectionCriterion3));

		public ZString SelectionCriterion4 => ZString.Empty;
		public ZPropertyInfo SelectionCriterion4Info => GetZPropertyInfo(nameof(SelectionCriterion4));

		public ZString SelectionCriterion5 => ZString.Empty;
		public ZPropertyInfo SelectionCriterion5Info => GetZPropertyInfo(nameof(SelectionCriterion5));

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new IncidentManagementGroupFetchStrategy(this);
		}

		#endregion

		#region Service Type

		public ZString ING_ServiceTypeDescription
		{
			get { return Lookups.ServiceTypeList.GetDescriptionFromCode(ING_ServiceType); }
		}

		#endregion

		#region Product Area

		public void RecalculateProductArea()
		{
			var newProductArea = GetRecalculatedProductArea();
			if (!newProductArea.IsEmpty)
			{
				ING_ProductArea = newProductArea;
			}
		}

		public ZString GetRecalculatedProductArea()
		{
			return IncidentDetailsHelper.FindProductArea(this);
		}

		#endregion

		#region Source Module

		public override ZString ING_SourceModuleId
		{
			get => base.ING_SourceModuleId;
			set
			{
				if (base.ING_SourceModuleId != value)
				{
					base.ING_SourceModuleId = value;
					if (IsInDatabase)
					{
						isSourceModuleOverridenCache = true;
					}
				}
			}
		}

		ZBool? isSourceModuleOverridenCache;
		public ZBool IsSourceModuleOverriden
		{
			get
			{
				if (!isSourceModuleOverridenCache.HasValue)
				{
					var mostRecentLogSourceModuleChange = MostRecentLogStatusChange(SourceModuleIdFieldDescription);
					isSourceModuleOverridenCache = (mostRecentLogSourceModuleChange != null) && (mostRecentLogSourceModuleChange.SL_Reference != BuildLogStatusChangeReference(SourceModuleIdFieldDescription, "", ING_SourceModuleId));
				}
				return isSourceModuleOverridenCache.Value;
			}
		}

		const string SourceModuleIdFieldDescription = "Menu Item";

		protected StmALog MostRecentLogStatusChange(string description)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, description);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			return Factory.LoadTop1<StmALog>(query);
		}

		protected string BuildLogStatusChangeReference(string description, string originalValue, string newValue)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} - {1} to {2}", description, originalValue, newValue);
		}

		public ZString SourceModuleWithPath
		{
			get
			{
				if (string.IsNullOrEmpty(ING_SourceModuleId) || ING_SourceModuleId.EqualsIgnoringCase(IncidentApproval.NotAvailableActiveModuleID))
				{
					return "<Not Available>";
				}

				if (ING_SourceModuleId == ModuleListBuilder.Codes.All)
				{
					return ModuleListBuilder.Descriptions.All;
				}

				var sourceModule = EDIDataRegistry.Instance.SourceModules.Value.GetSourceModule(ING_SourceModuleId, ING_Product);
				if (sourceModule != null)
				{
					return sourceModule.Path + " " + sourceModule.Description;
				}

				return ING_SourceModuleId;
			}
		}

		#endregion

		#region Notes

		#region Initial Symptoms

		public virtual ZString InitialSymptomsText
		{
			get => InitialSymptoms.Text;
			set
			{
				CheckMaximumLength(InitialSymptomsTextInfo, value);
				InitialSymptoms.Text = value;
				InitialSymptomsTextInfo.RefreshBinding();
				HasChanges = true;
			}
		}

		public ZPropertyInfo InitialSymptomsTextInfo
		{
			get { return GetZPropertyInfo(nameof(InitialSymptomsText)); }
		}

		public int InitialSymptomsText_MaxLength
		{
			get { return EDIPredefinedNoteTypes.Instance.IncidentManagementGroupInitialSymptoms.TextOnlyMaxLength; }
		}

		UniqueNote InitialSymptoms
		{
			get
			{
				if (fInitialSymptoms == null)
				{
					fInitialSymptoms = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentManagementGroupInitialSymptoms);
				}
				return fInitialSymptoms;
			}
		}

		UniqueNote fInitialSymptoms;

		#endregion

		#region Client

		public ZString ClientName => ZString.Empty;

		public ZPropertyInfo ClientNameInfo => GetZPropertyInfo(nameof(ClientName));

		public ZString ClientCode => ZString.Empty;

		public ZPropertyInfo ClientCodeInfo => GetZPropertyInfo(nameof(ClientCode));

		#endregion

		public ZString Number => ING_IncidentGroupNumber;

		public ZPropertyInfo NumberInfo => ING_IncidentGroupNumberInfo;

		public ZString AssignedStaffCode => ZString.Empty;

		public ZPropertyInfo AssignedStaffCodeInfo => GetZPropertyInfo(nameof(AssignedStaffCode));

		public ZString Criticality => ING_Priority;

		public ZPropertyInfo CriticalityInfo => ING_PriorityInfo;

		public ZString Source => ZString.Empty;

		public ZPropertyInfo SourceInfo => GetZPropertyInfo(nameof(Source));

		public ControllerID ControllerID => ClientControllerRegistration.IncidentManagementGroup;

		public Type PivotCollectionType => typeof(GenPivotCollection);

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = new NoteTypeCollection();
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentManagementGroupInitialSymptoms);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentManagementGroupBusinessImpactDescription);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentManagementGroupRootCause);
				return types;
			}
		}

		#endregion

		#region IWorkTaskTreeNode

		public BusinessObjectCollection ChildrenOnlyRelatedItems
		{
			get
			{
				if (childrenOnlyRelatedItems == null)
				{
					childrenOnlyRelatedItems = new WorkTaskRelatedItemGenPivotCollection(this, RelatedLinkType.MasterAlwaysParent);
					childrenOnlyRelatedItems.Load();
				}
				return childrenOnlyRelatedItems;
			}
		}
		WorkTaskRelatedItemGenPivotCollection childrenOnlyRelatedItems;

		public BusinessObjectCollection ParentsOnlyRelatedItems
		{
			get
			{
				if (parentsOnlyRelatedItems == null)
				{
					parentsOnlyRelatedItems = new WorkTaskRelatedItemGenPivotCollection(this, RelatedLinkType.MasterAlwaysChild);
					parentsOnlyRelatedItems.Load();
				}
				return parentsOnlyRelatedItems;
			}
		}
		WorkTaskRelatedItemGenPivotCollection parentsOnlyRelatedItems;

		public ZDateTime AgreedDeliveryDate
		{
			get
			{
				if (jobHeaderDeliveryDate == null)
				{
					jobHeaderDeliveryDate = ProcessJobHeaderProvider.GetForParent(this, Factory, false);
				}
				return jobHeaderDeliveryDate?.AgreedDeliveryDateLocal ?? ZDateTime.Empty;
			}
		}
		IProcessJobHeader jobHeaderDeliveryDate;

		public ZString CurrentTaskStatus => CurrentTask?.P9_Status ?? string.Empty;

		public ZPropertyInfo CurrentTaskStatusInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentTaskStatus)); }
		}

		public ZString CurrentTaskDescription => CurrentTask?.P9_Description ?? string.Empty;

		public ZPropertyInfo CurrentTaskDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentTaskDescription)); }
		}

		public ZString CurrentTaskCapabilityCodeDescription => OverallAssignedToDescription;

		public ZString CurrentTaskAssigned => OverallAssignedToCode;

		public ZString SelectionCriterion1Code => ING_Type;

		public ZString SelectionCriterion2Code => ING_ProductArea;

		public ZString SelectionCriterion3Code => ING_Product;

		public ZString SelectionCriterion4Code => string.Empty;

		public ZString SelectionCriterion5Code => string.Empty;

		public void AddFetchHintsForOrgAddressIfRequired()
		{
		}

		public void AddFetchHintsForOrgHeaderIfRequired()
		{
		}

		#endregion

		#region IWorkflowProvider

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public IncidentManagementGroupProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new IncidentManagementGroupProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		IncidentManagementGroupProcessTaskCollection workflowItems;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		[ChildEditableTestExclude]
		public MilestoneCollectionView Milestones => WorkflowItems.Milestones;

		public ZString WorkflowType
		{
			get { return IncidentManagementGroupConstants.WorkflowDescriptorInformation.Code; }
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			// Must match IncidentManagementGroupFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, ING_Type, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, ING_Product, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, ING_ProductArea, ZString.Empty);
			return result;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			var isInitialSave = !IsInDatabase && WorkflowItems.Count == 0;

			if (isInitialSave)
			{
				AddWorkflowEvent(Res.GetString("IncidentManagementGroup.WorkflowEvent.Detail.Initially", "Product details initially set"), AutoEvents.QueueChanged);

				this.ApplyWorkflowTemplates();

				if (isInitialSave && WorkflowItems.Tasks.Count > 0)
				{
					var firstTask = WorkflowItems.Tasks.Cast<ProcessTask>().OrderBy(task => task.P9_Sequence).First();
					if (firstTask.P9_GS_NKAssignedStaffMember == User.ServiceUserCode || firstTask.P9_GS_NKAssignedStaffMember == User.WebUserCode)
					{
						firstTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
						if (firstTask.P9_G4_RequiredCapability.IsEmpty)
						{
							firstTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
						}
					}
				}
			}
			else
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#region Post workflow events

		void AddProductDetailsChangedWorkflowEvent()
		{
			if (IsInDatabase && HasProductDetailsChangedForPostWorkflow)
			{
				AddWorkflowEvent(Res.GetString("IncidentManagementGroup.WorkflowEvent.Detail.Changed", "Product details were changed by user"), AutoEvents.QueueChanged);

				if ((ING_Product != ZString.Empty && ING_ProductInfo.HasChanges) || (ING_ProductArea != ZString.Empty && ING_ProductAreaInfo.HasChanges))
				{
					if (IsProductDetailsChangedCauseWorkflowTemplateBeingApplied)
					{
						AddWorkflowEvent(Res.GetString("IncidentManagementGroup.WorkflowEvent.Detail.Cancelled", "Task cancel trigger event due to product details change"), AutoEvents.Cancelled);
					}
				}
			}
		}

		void AddWorkflowEvent(string description, Event eventCode)
		{
			var paramList = new List<KeyValuePair<string, string>>();

			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, description));
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Product, ING_Product));
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.ProductArea, ING_ProductArea));
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Module, ING_Module));
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.SourceModuleId, ING_SourceModuleId));

			Logs.AddNew(
				eventCode,
				paramList.ToArray()
			);
		}

		protected bool IsProductDetailsChangedCauseWorkflowTemplateBeingApplied
		{
			get
			{
				if (MatchingTemplates == null)
				{
					return false;
				}

				var currentNewTemplates = MatchingTemplates.Except(LastMatchingTemplates);

				if (!currentNewTemplates.IsNullOrEmpty())
				{
					foreach (MatchingTemplateViewLine newTemplateViewLine in currentNewTemplates)
					{
						var newTemplate = newTemplateViewLine.Template;

						if (ING_ProductInfo.HasChanges && ING_Product != ZString.Empty && newTemplate.P0_SubType2 == ING_Product)
						{
							return true;
						}

						if (ING_ProductAreaInfo.HasChanges && ING_ProductArea != ZString.Empty && newTemplate.P0_SubType3 == ING_ProductArea)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		bool HasTemplateSelectionCriteriaValuesChanged
		{
			get
			{
				return ING_TypeInfo.HasChanges || ING_ProductInfo.HasChanges || ING_ProductAreaInfo.HasChanges;
			}
		}

		#region Last Matching Templates

		IEnumerable<BusinessObject> LastMatchingTemplates
		{
			get
			{
				if (lastMatchingTemplates == null)
				{
					lastMatchingTemplates = MatchingTemplates;
				}

				return lastMatchingTemplates;
			}

			set
			{
				if (value != LastMatchingTemplates)
				{
					lastMatchingTemplates = value;
				}
			}
		}
		IEnumerable<BusinessObject> lastMatchingTemplates;

		IEnumerable<BusinessObject> MatchingTemplates
		{
			get
			{
				if (WorkflowItems != null)
				{
					var currentTasksTemplateView = new MatchingTemplateView(WorkflowItems.Tasks);
					var currentMilestonesTemplateView = new MatchingTemplateView(WorkflowItems.Milestones);
					var currentTriggersTemplateView = new MatchingTemplateView(WorkflowItems.Triggers);

					var totalCurrentTasksTemplates = (currentTasksTemplateView.MatchingTemplates.
													Union(currentMilestonesTemplateView.MatchingTemplates)).
													Union(currentTriggersTemplateView.MatchingTemplates);

					return totalCurrentTasksTemplates;
				}

				return Enumerable.Empty<BusinessObject>();
			}
		}

		#endregion

		#endregion

		protected override void OnFactorySaving()
		{
			SetWorkItemsCascadeToAllIncidents();
			BroadcastMessageOnStatusChange();
			base.OnFactorySaving();
		}

		public bool IsDefect => IncidentConstants.IsDefect(ING_Priority);

		#region Business Impact Description

		public virtual ZString BusinessImpactDescriptionText
		{
			get => BusinessImpactDescription.Text;
			set
			{
				CheckMaximumLength(BusinessImpactDescriptionTextInfo, value);
				BusinessImpactDescription.Text = value;
				BusinessImpactDescriptionTextInfo.RefreshBinding();
				HasChanges = true;
			}
		}

		public ZPropertyInfo BusinessImpactDescriptionTextInfo
		{
			get { return GetZPropertyInfo(nameof(BusinessImpactDescriptionText)); }
		}

		public int BusinessImpactDescriptionText_MaxLength
		{
			get { return EDIPredefinedNoteTypes.Instance.IncidentManagementGroupBusinessImpactDescription.TextOnlyMaxLength; }
		}

		UniqueNote BusinessImpactDescription
		{
			get
			{
				if (fBusinessImpactDescription == null)
				{
					fBusinessImpactDescription = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentManagementGroupBusinessImpactDescription);
				}
				return fBusinessImpactDescription;
			}
		}

		UniqueNote fBusinessImpactDescription;

		#endregion

		#region Root Cause

		public virtual ZString RootCauseText
		{
			get => RootCause.Text;
			set
			{
				CheckMaximumLength(RootCauseTextInfo, value);
				RootCause.Text = value;
				RootCauseTextInfo.RefreshBinding();
				HasChanges = true;
			}
		}

		public ZPropertyInfo RootCauseTextInfo
		{
			get { return GetZPropertyInfo(nameof(RootCauseText)); }
		}

		public int RootCauseText_MaxLength
		{
			get { return EDIPredefinedNoteTypes.Instance.IncidentManagementGroupBusinessImpactDescription.TextOnlyMaxLength; }
		}

		UniqueNote RootCause
		{
			get
			{
				if (fRootCause == null)
				{
					fRootCause = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentManagementGroupRootCause);
				}
				return fRootCause;
			}
		}

		UniqueNote fRootCause;

		#endregion

		#endregion

		public void SetWorkItemsCascadeToAllIncidents()
		{
			if (!IsDefect)
			{
				return;
			}

			foreach (IncidentManagementLink link in LinkedIncidents)
			{
				SetWorkItemsCascadeToIncident(link);
			}
		}

		public void SetWorkItemsCascadeToIncident(IncidentManagementLink link)
		{
			var incident = link.SupportIncident;
			foreach (NewWorkItem workItemCascade in AutoCascadeRelatedItems)
			{
				if (workItemCascade != null && incident != null && incident.IsDefect && !incident.RelatedWorkItems.Contains(workItemCascade.PK))
				{
					if (incident.IM_Category == SupportIncidentCategoriesList.Codes.Support)
					{
						incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, string.Empty);
					}

					incident.RelatedWorkItems.Add(workItemCascade);

					var preAtcLog = Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.IsCancelled && l.SL_Reference.Contains("Work Item " + workItemCascade.WKI_WorkItemNumber + " added to Incident"));
					preAtcLog?.Cancel();

					var parameters = new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>("DES", $"Work Item {workItemCascade.WKI_WorkItemNumber} added to Incident {incident.IM_IncidentNumber} by Incident Group {ING_IncidentGroupNumber}"),
						new KeyValuePair<string, string>("RFN", incident.IM_IncidentNumber),
						new KeyValuePair<string, string>("ARG", workItemCascade.WKI_WorkItemNumber),
						new KeyValuePair<string, string>("JOB", ING_IncidentGroupNumber)
					};

					Logs.AddNew(Events.Attached, parameters.ToArray());
					incident.Logs.AddNew(Events.Attached, parameters.ToArray());
				}
			}
		}

		#region Status

		public IncidentGroupStatusConfiguration NowStage
		{
			get
			{
				if (ING_Status.IsEmpty)
				{
					return null;
				}
				return Stages.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code.EqualsIgnoringCase(ING_Status));
			}
		}

		public IncidentGroupStatusConfigurationCollection Stages
		{
			get
			{
				if (stages == null)
				{
					stages = GetStages();
				}

				return stages;
			}
		}

		public IncidentGroupStatusConfigurationCollection LatestStages
		{
			get
			{
				RefreshStages();
				return Stages;
			}
		}

		IncidentGroupStatusConfigurationCollection stages;

		IncidentGroupStatusConfigurationCollection GetStages()
		{
			if (ING_Type.IsEmpty)
			{
				return new IncidentGroupStatusConfigurationCollection();
			}
			else
			{
				var registryStages = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value.GetActiveStages(ING_Type);
				return (IncidentGroupStatusConfigurationCollection)registryStages.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			}
		}

		public void RefreshStages()
		{
			var registryStages = GetStages();

			if (stages != null)
			{
				Stages.RemoveAll();

				foreach (var stage in registryStages)
				{
					Stages.Add(stage);
				}
			}
		}

		public IncidentGroupStatusConfigurationCollection AllStages
		{
			get
			{
				if (allStages == null)
				{
					allStages = GetAllStages();
				}

				return allStages;
			}
		}

		IncidentGroupStatusConfigurationCollection allStages;

		IncidentGroupStatusConfigurationCollection GetAllStages()
		{
			if (ING_Type.IsEmpty)
			{
				return new IncidentGroupStatusConfigurationCollection();
			}
			else
			{
				var registryStages = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value.GetAllThisTypeStages(ING_Type);
				return (IncidentGroupStatusConfigurationCollection)registryStages.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			}
		}

		public IncidentGroupStatusConfiguration GetPreviousStatus()
		{
			var sortedStages = Stages.ToList<IncidentGroupStatusConfiguration>().OrderBy(x => x.Sequence);
			var currentStageIndex = sortedStages.IndexOf((stage) => stage.Code.EqualsIgnoringCase(ING_Status));

			if (currentStageIndex <= 0)
			{
				return null;
			}

			return sortedStages.ElementAt(currentStageIndex - 1);
		}

		public IncidentGroupStatusConfiguration GetNextStatus()
		{
			var sortedStages = Stages.ToList<IncidentGroupStatusConfiguration>().OrderBy(x => x.Sequence);
			var currentStageIndex = sortedStages.IndexOf((stage) => stage.Code.EqualsIgnoringCase(ING_Status));

			if (currentStageIndex < 0 || currentStageIndex == sortedStages.Count() - 1)
			{
				return null;
			}

			return sortedStages.ElementAt(currentStageIndex + 1);
		}

		void LogStatusChange()
		{
			if (!ING_StatusInfo.HasChanges)
			{
				return;
			}

			var newINGStatus = NowStage != null ? NowStage.Code : (ZString)"No Status";
			var oldINGStatus = GetPreviousStatus() != null ? GetPreviousStatus().Code : (ZString)"No Status";
			var newStageIncidentCompleted = NowStage != null ? NowStage.IncidentCompleted.ToString() : string.Empty;
			var newStageGroupCompleted = NowStage != null ? NowStage.GroupCompleted.ToString() : string.Empty;

			var detail = FormattableString.Invariant(
				$"Group stage was changed from {oldINGStatus} to {newINGStatus}. Incidents completed {newStageIncidentCompleted}, Group Completed {newStageGroupCompleted}"
				);
			var paramList = new List<KeyValuePair<string, string>>();
			paramList.Add(new KeyValuePair<string, string>(StageChangeEventLogCodes.NewStatus, newINGStatus));
			paramList.Add(new KeyValuePair<string, string>(StageChangeEventLogCodes.OldStatus, oldINGStatus));
			paramList.Add(new KeyValuePair<string, string>(StageChangeEventLogCodes.NewStageIncidentCompleted, newStageIncidentCompleted));
			paramList.Add(new KeyValuePair<string, string>(StageChangeEventLogCodes.NewStageGroupCompleted, newStageGroupCompleted));
			paramList.Add(new KeyValuePair<string, string>(StageChangeEventLogCodes.Detail, detail));

			Logs.AddNew(
				AutoEvents.StageChange,
				paramList.ToArray()
				);
		}

		#endregion

		#region IWorkItemRelatedItem

		public bool OnRelatedWorkItemClosed(WorkItem workItem)
		{
			return true;
		}

		public void OnRelatedWorkItemReOpened(WorkItem workItem)
		{
		}

		public void OnWorkItemAdded(WorkItem workItem)
		{
		}

		public void OnWorkItemRemoved(WorkItem workItem)
		{
		}

		#endregion

		#region Current Task

		public ProcessTask CurrentTask
		{
			get
			{
				if (currentTask == null)
				{
					currentTask = new CachedProperty<ProcessTask>(Factory, delegate
					{
						return TaskFinder.FindCurrentStartableTask();
					}
					);
				}
				return currentTask.Value;
			}
		}
		CachedProperty<ProcessTask> currentTask;

		CurrentTaskFinder TaskFinder => taskFinder ?? (taskFinder = new CurrentTaskFinder(this));
		CurrentTaskFinder taskFinder;

		[ResourceStringData("e803e9a1-25d4-4eef-8d5e-c7849faa63df", Caption = "Code")]
		public ZString OverallAssignedToCode
		{
			get
			{
				ZString result = ZString.Empty;
				var task = CurrentTask;
				if (task != null)
				{
					if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
					{
						result = task.P9_GS_NKAssignedStaffMember;
					}
					else if (!task.P9_G4_RequiredCapability.IsEmpty && task.RequiredCapability != null)
					{
						result = task.RequiredCapability.G4_Code;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo OverallAssignedToCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OverallAssignedToCode)); }
		}

		[ResourceStringData("e91ad3cb-7382-41b1-a8ba-4d0e90d88972", Caption = "Description")]
		public ZString OverallAssignedToDescription
		{
			get
			{
				ZString result = ZString.Empty;
				var task = CurrentTask;
				if (task != null)
				{
					if (task.AssignedStaffMember != null)
					{
						result = task.AssignedStaffMember.GS_FullName;
					}
					else if (!task.P9_G4_RequiredCapability.IsEmpty)
					{
						result = task.RequiredCapability?.G4_Description ?? ZString.Empty;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo OverallAssignedToDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(OverallAssignedToDescription)); }
		}

		public ZString OverallAssignedToLabelText
		{
			get
			{
				ZString result = "Assigned To:";
				var task = CurrentTask;
				if (task != null)
				{
					if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
					{
						result = "Task Assigned:";
					}
					else if (!task.P9_G4_RequiredCapability.IsEmpty)
					{
						result = "Capability:";
					}
				}

				return result;
			}
		}

		public ZPropertyInfo OverallAssignedToLabelTextInfo
		{
			get { return GetZPropertyInfo(nameof(OverallAssignedToLabelText)); }
		}

		#endregion

		#region IIncidentDetailsSource

		ZString IIncidentDetailsSource.Product => ING_Product;

		ZString IIncidentDetailsSource.Criticality => ING_Priority;

		ZString IIncidentDetailsSource.ProductArea => ING_ProductArea;

		ZString IIncidentDetailsSource.Module => ING_Module;

		ZString IIncidentDetailsSource.SourceModuleId => ING_SourceModuleId;

		#endregion

		#region AutoReplyDescription

		[List("Lookups.AutoReplyDescriptionList")]
		public ZString AutoReplyDescription
		{
			get
			{
				return ING_IsAutoReply ? IncidentManagementGroupConstants.AutoReplyDescriptions.AutoReplyOnce : IncidentManagementGroupConstants.AutoReplyDescriptions.Manual;
			}

			set
			{
				ING_IsAutoReply = (value == IncidentManagementGroupConstants.AutoReplyDescriptions.AutoReplyOnce);
				ING_IsAutoReplyInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo AutoReplyTextInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AutoReplyDescription), x => ING_IsAutoReplyInfo); }
		}

		#endregion

		#region Grid Column Properties

		public ZString BusinessImpact => Lookups.BusinessImpactList.GetDescriptionFromCode(ING_BusinessImpact);

		public ZString ServiceOutageStatusDescription => Lookups.ServiceOutageList.GetDescriptionFromCode(ING_ServiceOutage);

		public ZString TaskStatusDescription => CurrentTask?.Lookups.Statuses.GetDescriptionFromCode(CurrentTask.P9_Status) ?? ZString.Empty;

		public ZString CapabilityCode => CurrentTask?.RequiredCapability?.G4_Code ?? ZString.Empty;

		public ZString CapabilityDescription => CurrentTask?.RequiredCapability?.G4_Description ?? ZString.Empty;

		public ZString AssignedToUserCode => CurrentTask?.P9_GS_NKAssignedStaffMember ?? ZString.Empty;

		public ZString AssignedToUserName => CurrentTask?.AssignedStaffMember?.GS_FullName ?? ZString.Empty;

		public ZString UrgencyDescription => Lookups.UrgencyList.GetDescriptionFromCode(ING_Urgency);

		public ZString ProductDescription => Lookups.ProductList.GetDescriptionFromCode(ING_Product);

		public ZString ProductAreaDescription => Lookups.ProductAreaList.GetDescriptionFromCode(ING_ProductArea);

		public ZBool HasAutoReply => IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>().Any(x => x.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply && x.IGM_IsPublished);

		#endregion

		public ZString OutageDuration
		{
			get
			{
				var milestones = Milestones.Cast<ProcessTask>();
				var serviceSuspendedMilestone = milestones.FirstOrDefault(x => x.P9_SE_NKMilestoneEvent == AutoEvents.ServiceSuspendedCode && !x.P9_ActualDate.IsEmpty);

				if (serviceSuspendedMilestone == null)
				{
					return string.Empty;
				}

				if (ING_ServiceOutage == ServiceOutageCodes.Restored)
				{
					var serviceRestoredMilestone = milestones.FirstOrDefault(x => x.P9_SE_NKMilestoneEvent == AutoEvents.ServiceCommencedCode && !x.P9_ActualDate.IsEmpty);

					if (serviceRestoredMilestone == null)
					{
						return "Milestone Setup Error";
					}

					var outageTime = serviceRestoredMilestone.P9_ActualDateUtc - serviceSuspendedMilestone.P9_ActualDateUtc;

					return GetFormattedDatetimeString(outageTime);
				}

				var currentOutageTime = ZDateTime.UtcNow - serviceSuspendedMilestone.P9_ActualDateUtc;

				return GetFormattedDatetimeString(currentOutageTime);
			}
		}

		string GetFormattedDatetimeString(TimeSpan timeSpan)
		{
			return FormattableString.Invariant($"{timeSpan.Days}Day(s) {timeSpan.Hours}Hour(s) {timeSpan.Minutes}Min");
}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			originalCascadeProductDetails = CascadeProductDetails;
			LastMatchingTemplates = MatchingTemplates;
		}

		#endregion

		public ZPropertyInfo OutageDurationInfo => GetZPropertyInfo(nameof(OutageDuration));

		public int IncidentsPendingResponseCount
		{
			get
			{
				return LinkedIncidents.Cast<IncidentManagementLink>().Count(x => x.IsCustomerWaitingForResponse);
			}
		}

		public ZString LongestWaitingTime
		{
			get
			{
				var incidentWithLongestWaitingTime = LinkedIncidents.Cast<IncidentManagementLink>()
					.Where(x => x.IsCustomerWaitingForResponse)
					.OrderBy(y => y.LastCustomerMessageReceived.SendLocalDateTime)
					.FirstOrDefault();
				return incidentWithLongestWaitingTime.CustomerWaitingTime;
			}
		}

		#region Add Event Logs

		public void ProcessIncidentMessageReceived(SupportIncident incident)
		{
			AddIncidentMessageReceivedEvent(incident);

			var link = incident.IncidentManagementLink;
			if (link != null && !link.StatusAndResolutionCodeChanged && link.IsControlled && NowStage != null && NowStage.GroupCompleted
						&& incident.RelatedWorkItems.Cast<WorkItem>().All(x => x.IsClosedOrCancelled))
			{
				incident.Logs.AddLog(AutoEvents.StatusChange, "previousStatus: " + incident.IM_Status);
				incident.Logs.AddLog(AutoEvents.StatusChange, "previousResolutionCode: " + incident.IM_ResolutionCode);
				incident.IM_Status = IncidentMainLookups.Status.Working;
				incident.IM_ResolutionCode = DispositionList.Constants.Open.AssignedAwaitingAction;
			}
		}

		void AddIncidentMessageReceivedEvent(SupportIncident incident)
		{
			var incidentGroup = incident.ManagementGroup;
			if (incidentGroup != null && incidentGroup.PK == PK)
			{
				var incidentLogBuilder = EventLogReferenceBuilder.New()
					.AddMandatory(EventReferenceParameters.Codes.ReferenceNumber, incident.IM_IncidentNumber)
					.AddMandatory(EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Message(s) received for {incident.IM_IncidentNumber}"));
				incidentGroup.Logs.AddNew(AutoEvents.MessageReceived, incidentLogBuilder.Build());
			}
		}

		public void AddIncidentMessageSentEvent(SupportIncident incident)
		{
			this.AddIncidentMessageSentEvent(incident, "Manual", FormattableString.Invariant($"Message(s) sent for {incident.IM_IncidentNumber}"));
		}

		public EventValue AddIncidentMessageSentEvent(SupportIncident incident, string messageType, string description)
		{
			var incidentGroup = incident.ManagementGroup;
			if (incidentGroup != null && incidentGroup.PK == PK)
			{
				var incidentLogBuilder = EventLogReferenceBuilder.New()
					.AddMandatory(EventReferenceParameters.Codes.MessageType, messageType)
					.AddMandatory(EventReferenceParameters.Codes.ReferenceNumber, incident.IM_IncidentNumber)
					.AddMandatory(EventReferenceParameters.Codes.Description, description);

				var eventValue = new EventValue(AutoEvents.MessageSent, reference: incidentLogBuilder.Build());

				incidentGroup.Logs.AddNew(eventValue);
				incident.Logs.AddNew(eventValue);

				if ((ING_IsAutoReplyUnflagsCommunication || messageType != IncidentManagementGroupMessageTypePairList.Codes.AutoReply) && incident != null && incident.IncidentManagementLink != null)
				{
					incident.IncidentManagementLink.CreateOrUpdateUnflaggedEvent();
					incident.IncidentManagementLink.UpdateStatusOnReopenedIncidentIfRequired();
				}

				return eventValue;
			}

			return null;
		}

		#endregion

		public IncidentManagementGroupEConversation EConversation
		{
			get
			{
				if (eConversation == null)
				{
					eConversation = new IncidentManagementGroupEConversation(this);
				}
				return eConversation;
			}
		}
		IncidentManagementGroupEConversation eConversation;

		#region IConversationProvider

		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
			return;
		}

		JobConversation IConversationProvider.eConversation => EConversation.Conversation;

		ModuleIdentifier IConversationProvider.ParentModule => ClientModuleRegistration.IncidentManagementGroup;

		ControllerID IConversationProvider.ParentController => ClientControllerRegistration.IncidentManagementGroup;

		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants
		{
			get { return Array.Empty<EConversation.Business.RelatedParty>(); }
		}

		bool IConversationProvider.SendEmailNotificationsOnSave => true;

		string IConversationProvider.EmailSubjectContentOverride => default;

		string IConversationProvider.FromAddressOverride => default;

		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}

		CustomBusinessObject customBusinessObject;

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new IncidentManagementGroupDocumentSupporter(this);
				}
				return fDocumentSupporter;
			}
		}

		#region IJobHeader

		public bool AllowInvoiceDeletion => true;

		public void SetJobNumberFieldOnSaving()
		{
		}

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobCreated(JobHeader job)
		{
		}

		public void OnJobDeleting(JobHeader job)
		{
		}

		public void OnJobDeleted(JobHeader job)
		{
		}

		public string JobNumber => this.ING_IncidentGroupNumber;

		#endregion

		IncidentManagementGroupDocumentSupporter fDocumentSupporter;

		#endregion

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, IncidentConstants.IncidentManagementGroupDocManagerCode));

		DocManagerInfo docManagerInfo;

		#endregion

		#region ITriageAssistParent

		ZString ITriageAssistParent.Priority { get => ING_Priority; set => ING_Priority = value; }

		ZString ITriageAssistParent.Product {  get => ING_Product; set => ING_Product = value; }

		ZString ITriageAssistParent.Module {  get => ING_Module; set => ING_Module = value; }

		ZString ITriageAssistParent.ProductArea { get => ING_ProductArea; set => ING_ProductArea = value; }

		ZString ITriageAssistParent.Category { get => ING_Category; set => ING_Category = value; }

		ZString ITriageAssistParent.SourceModuleId { get => ING_SourceModuleId; set => ING_SourceModuleId = value; }

		ZString ITriageAssistParent.SourceModuleWithPath { get => SourceModuleWithPath; }

		ZBool ITriageAssistParent.IsSourceModuleOverriden { get => IsSourceModuleOverriden; }

		HashSet<ZString> nonTriageOverridableCriticalities;

		HashSet<ZString> ITriageAssistParent.nonTriageOverridableCriticalities
		{
			get {
				if (nonTriageOverridableCriticalities == null)
				{
					nonTriageOverridableCriticalities = new HashSet<ZString>();
					nonTriageOverridableCriticalities.Add(Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
					nonTriageOverridableCriticalities.Add(Constants.CustomerService.CriticalityCodes.CR2_ModuleDown);
					nonTriageOverridableCriticalities.Add(Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround);
					nonTriageOverridableCriticalities.Add(Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
					nonTriageOverridableCriticalities.Add(Constants.CustomerService.CriticalityCodes.CR5_Training);
				}
				return nonTriageOverridableCriticalities;
			}
		}

		ZPropertyInfo ITriageAssistParent.TriagePKInfo => ING_IMT_TriageInfo;

		public IncidentTriage IncidentTriage => Factory.Load<IncidentTriage>(ING_IMT_Triage);

		public string TriageDescription => IncidentTriage != null ? IncidentTriage.IMT_TriageNumber + " - " + IncidentTriage.IMT_SupportDescription : "Not Applied";
		
		[RelatedBusinessObject("IncidentTriage")]
		[List("Lookups.TriageList")]
		public ZGuid TriagePK { get => ING_IMT_Triage; set => ING_IMT_Triage = value; }

		#endregion
	}
}
