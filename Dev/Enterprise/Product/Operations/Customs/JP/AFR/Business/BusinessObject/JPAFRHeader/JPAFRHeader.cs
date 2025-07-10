using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using ImportAction = Enterprise.Customs.Business.ImportAction;

namespace Enterprise.Customs.JP.AFR.Business
{
	[CodeProperty(JPAFRHeader.Schema.JPH_JobReference)]
	[SingleObjectAroundARow]
	[UniversalDataContext(DataContextType.AFRHeader)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class JPAFRHeader : AutoJPAFRHeader,
		IDocAddresses,
		IJobNumber,
		IControllerIDProvider,
		Integration.Customs.JP.AFR.IJPAFRHeader,
		IWorkflowProvider,
		IWorkflowTriggerEventSource,
		Integration.Customs.JP.AFR.IJPAFRHeaderWithConsolSynchonisation,
		IDocumentSupportable,
		ICusAddInfoTypeSupporter,
		IEDocsProvider,
		IUniversalXMLNoteParent,
		ISynchroniserReadOnlyMembersProvider,
		ISailingParentFindBox
	{
		public JPAFRHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJPAFRHeader.Schema
		{
			public const string JPH_BillRegistrationStatus = "JPH_BillRegistrationStatus";
			public const string JPH_BillMessageStatus = "JPH_BillMessageStatus";
			public const string JPH_BillMessageStatusDescription = "JPH_BillMessageStatusDescription";
			public const string JPH_BillReleaseStatus = "JPH_BillReleaseStatus";
			public const string JPH_BillReleaseStatusDescription = "JPH_BillReleaseStatusDescription";
			public const string JPH_MessageStatusDescription = "JPH_MessageStatusDescription";
			public const string JPH_NoOfAFRBills = "JPH_NoOfAFRBills";
			public const string JPH_NoOfSailingBills = "JPH_NoOfSailingBills";
			public const string JPH_NoOfAFRContainers = "JPH_NoOfAFRContainers";
			public const string JPH_NoOfSailingContainers = "JPH_NoOfSailingContainers";
		}

		#region CachedProperty

		public bool IsAnyBillAlreadyRegistered
		{
			get
			{
				if (isAnyBillAlreadyRegistered == null)
				{
					isAnyBillAlreadyRegistered = new CachedProperty<bool>(Factory, delegate
					{
						return Bills.Any(bill => bill.IsBillAlreadyRegistered);
					});
				}
				return isAnyBillAlreadyRegistered.Value;
			}
		}
		CachedProperty<bool> isAnyBillAlreadyRegistered;

		public bool IsAnyBillHaveMessagingInProgress
		{
			get
			{
				if (isAnyBillHaveMessagingInProgress == null)
				{
					isAnyBillHaveMessagingInProgress = new CachedProperty<bool>(Factory, delegate
					{
						return Bills.Any(bill => bill.IsMessagingInProgress);
					});
				}
				return isAnyBillHaveMessagingInProgress.Value;
			}
		}
		CachedProperty<bool> isAnyBillHaveMessagingInProgress;

		public bool AreAllBillsRegistered
		{
			get
			{
				if (areAllBillsRegistered == null)
				{
					areAllBillsRegistered = new CachedProperty<bool>(Factory, delegate
					{
						return Bills.Count > 0 && Bills.All(bill => bill.IsBillAlreadyRegistered);
					});
				}
				return areAllBillsRegistered.Value;
			}
		}
		CachedProperty<bool> areAllBillsRegistered;

		#region Completion Status

		public static ZQuery BillRegistrationCompletedQuery
		{
			get
			{
				var billCompletionQuery = new ZQuery(StmALogSchema.SL_Reference, BillRegistrationReference);
				billCompletionQuery.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
				billCompletionQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, User.ServiceUserCode);
				return billCompletionQuery;
			}
		}

		public bool IsBillRegistrationCompleted
		{
			get
			{
				if (isBillRegistrationCompleted == null)
				{
					isBillRegistrationCompleted = new CachedProperty<bool>(Factory, delegate
					{
						return Logs.MostRecentLogByEventTime(Events.TaskCompleted, BillRegistrationCompletedQuery) != null;
					});
				}
				return isBillRegistrationCompleted.Value;
			}
		}
		CachedProperty<bool> isBillRegistrationCompleted;

		#endregion

		#region ATD Status

		public static ZQuery DepartureTimeRegisteredQuery
		{
			get
			{
				var atdRegisteredQuery = new ZQuery(StmALogSchema.SL_Reference, DepartureTimeRegistrationReference);
				atdRegisteredQuery.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
				atdRegisteredQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, User.ServiceUserCode);
				return atdRegisteredQuery;
			}
		}

		public bool IsDepartureTimeRegistered
		{
			get
			{
				if (isDepartureRegistered == null)
				{
					isDepartureRegistered = new CachedProperty<bool>(Factory, delegate
					{
						return Logs.MostRecentLogByEventTime(Events.TaskCompleted, DepartureTimeRegisteredQuery) != null;
					});
				}
				return isDepartureRegistered.Value;
			}
		}
		CachedProperty<bool> isDepartureRegistered;

		#endregion

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRHeader|JPH_BillRegistrationStatus", Caption = "Completion Status")]
		public ZString JPH_BillRegistrationStatus
		{
			get
			{
				var isHeaderShippingLineEntry = this.JPH_IsShippingLineEntry;
				var eventCheckerResult = isHeaderShippingLineEntry ? IsDepartureTimeRegistered : IsBillRegistrationCompleted;
				var pendingMessageStatus = isHeaderShippingLineEntry ? MessageStatusList.Codes.AwaitingDepartureTimeRegistration : MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion;
				return eventCheckerResult ? ResString.GetMultilingualString("99699748-33D6-4203-9568-BD135DAE5FBF", "Completed") : JPH_MessageStatus == pendingMessageStatus ? ResString.GetMultilingualString("670BA0CB-243E-4F07-8316-4E1C482878E5", "Pending") : "";
			}
		}

		public ZPropertyInfo JPH_BillRegistrationStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_BillRegistrationStatus); }
		}

		#region Bill Message Status
		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRHeader|JPH_BillMessageStatus", Caption = "Bill Message Status", ShortCaption = "Bill Msg. Stat.")]
		public ZString JPH_BillMessageStatus
		{
			get
			{
				if (billMessageStatusCached == null)
				{
					billMessageStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;
						var hasBlank = false;
						foreach (JPAFRBills bill in Bills)
						{
							var billStatus = bill.JPB_MessageStatus;
							if (billStatus.IsEmpty)
							{
								hasBlank = true;
							}
							else
							{
								if (result.IsEmpty)
								{
									result = billStatus;
								}
								else if (result != billStatus)
								{
									result = AFRStatusHelper.MultipleStatusCode;
									break;
								}
							}
						}
						if (!result.IsEmpty && hasBlank)
						{
							result += ",No Status";
						}
						return result;
					});
				}
				return billMessageStatusCached.Value;
			}
		}
		CachedProperty<ZString> billMessageStatusCached;

		public ZPropertyInfo JPH_BillMessageStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_BillMessageStatus); }
		}

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRHeader|JPH_BillMessageStatusDescription", Caption = "Bill Message Status Description", ShortCaption = "Bill Msg. Stat. Desc.")]
		public ZString JPH_BillMessageStatusDescription
		{
			get
			{
				if (billMessageStatusDescriptionCached == null)
				{
					billMessageStatusDescriptionCached = new CachedProperty<ZString>(Factory, delegate
					{
						ZString result;
						var status = JPH_BillMessageStatus;
						var statuses = status.Split(',');
						var extraMessage = "";
						if (statuses.Length > 1 && statuses[1].EqualsIgnoringCase("No Status"))
						{
							status = statuses[0];
							extraMessage = ResString.GetMultilingualString("8578409E-C47A-4E28-A33C-B44D36D80AA9", "; also there is a bill without any status.");
						}
						if (status == AFRStatusHelper.MultipleStatusCode)
						{
							result = MultipleBillsWithDifferentStatuses;
						}
						else
						{
							result = Factory.GetCachedValue<MessageStatusList>().GetDescriptionFromCode(status);
						}
						return result + extraMessage;
					});
				}
				return billMessageStatusDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> billMessageStatusDescriptionCached;

		public ZPropertyInfo JPH_BillMessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_BillMessageStatusDescription); }
		}

		Integration.Customs.JP.AFR.IAFRStatusHelper AfrStatusHelper
		{
			get { return afrStatusHelper ?? (afrStatusHelper = new AFRStatusHelper()); }
		}
		Integration.Customs.JP.AFR.IAFRStatusHelper afrStatusHelper;

		#endregion

		#region Bill Release Status
		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRHeader|JPH_BillReleaseStatus", Caption = "Bill Release Status", ShortCaption = "Bill Rel. Stat.")]
		public ZString JPH_BillReleaseStatus
		{
			get
			{
				if (billReleaseStatusCached == null)
				{
					billReleaseStatusCached = new CachedProperty<ZString>(Factory, () => { return AfrStatusHelper.GetAFRBillStatus(this); });
				}
				return billReleaseStatusCached.Value;
			}
		}
		CachedProperty<ZString> billReleaseStatusCached;

		public ZPropertyInfo JPH_BillReleaseStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_BillReleaseStatus); }
		}

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRHeader|JPH_BillReleaseStatusDescription", Caption = "Bill Release Status Description", ShortCaption = "Bill Rel. Status Desc.")]
		public ZString JPH_BillReleaseStatusDescription
		{
			get
			{
				if (billReleaseStatusDescriptionCached == null)
				{
					billReleaseStatusDescriptionCached = new CachedProperty<ZString>(Factory, delegate
					{ return AfrStatusHelper.GetAFRBillStatusDescription(Factory, JPH_BillReleaseStatus); });
				}
				return billReleaseStatusDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> billReleaseStatusDescriptionCached;

		public ZPropertyInfo JPH_BillReleaseStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_BillReleaseStatusDescription); }
		}

		#endregion

		public static string MultipleBillsWithDifferentStatuses
		{
			get { return ResString.GetMultilingualString("2C9C4427-7515-4FEA-A712-BFC86068F051", "There are multiple bills with different statuses"); }
		}

		#endregion

		#region RegistryHelper
		public Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						var result = Guid.Empty;
						var branch = Branch;
						if (branch == null)
						{
							result = GlbCompany.CurrentCompany.PK.ToGuid();
						}
						else if (!branch.GB_GC.IsEmpty)
						{
							result = branch.GB_GC.ToGuid();
						}
						return result;
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		public Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						var branch = Branch;
						return branch == null ? GlbBranch.CurrentBranch.PK.ToGuid() : branch.PK.ToGuid();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;
		#endregion

		#region Related Business Objects

		public JobDocAddress Carrier
		{
			get
			{
				if (carrier == null || carrier.IsDeleted)
				{
					carrier = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.Carrier));
					carrier.DocAddressChanged += delegate
					{ MarkAsNeedingValidation(); };
					carrier.OnRelationshipFieldsChanged += delegate
					{ MarkAsNeedingValidation(); };
				}

				return carrier;
			}
		}
		JobDocAddress carrier;

		public ForwardingConsol Consol
		{
			get { return JPH_ParentTableCode == JobConsolSchema.Constants.Prefix ? Factory.Load<ForwardingConsol>(JPH_ParentId) : null; }
		}

		public JobSailing Sailing
		{
			get
			{
				JobSailing result = null;
				if (this.JPH_IsShippingLineEntry && Sailings.Count == 1)
				{
					result = Sailings[0];
				}
				else if (JPH_ParentTableCode == JobSailingSchema.Constants.Prefix)
				{
					result = Factory.Load<JobSailing>(JPH_ParentId);
				}
				return result;
			}
		}

		public JobSailingCollection Sailings
		{
			get
			{
				if (sailings == null)
				{
					sailings = new JobSailingCollection(Factory);
					if (JPH_ParentTableCode == JobSailingSchema.Constants.Prefix && !JPH_ParentId.IsEmpty)
					{
						sailings.AddFromDatabase(JPH_ParentId);
					}
				}
				return sailings;
			}
		}
		JobSailingCollection sailings;

		public GlbCompany Company
		{
			get
			{
				GlbBranch branch = Branch;
				return branch == null ? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK) : branch.Company;
			}
		}

		[ChildEditable]
		public JPAFRBillsCollection Bills
		{
			get
			{
				if (bills == null)
				{
					bills = new JPAFRBillsCollection(this);
					RegisterEditableChildObject(bills);
				}
				return bills;
			}
		}
		JPAFRBillsCollection bills;

		public EDIMessageForDisplayCollection<JPAFRMessage> AFRMessages
		{
			get
			{
				if (afrMessages == null)
				{
					var receiveMessageFilter = new ZDBOnlyQuery(typeof(EDIMessage));
					receiveMessageFilter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging);
					receiveMessageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					receiveMessageFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
					var genPivotFilter = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID);
					var logFilter = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.PK);
					logFilter.AddToFilter(StmALogSchema.SL_Parent, PK);
					genPivotFilter.AddSubQuery(GenPivotSchema.XX_Relation1ID, logFilter, JoinCondition.And);
					receiveMessageFilter.AddSubQuery(genPivotFilter, JoinCondition.And);
					var messageCombineFilter = new ZQuery();
					var transmitMessageQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);
					transmitMessageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging);
					transmitMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					transmitMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
					messageCombineFilter.AddToFilter(transmitMessageQuery, JoinCondition.Or);
					messageCombineFilter.AddToFilter(receiveMessageFilter, JoinCondition.Or);
					afrMessages = new EDIMessageForDisplayCollection<JPAFRMessage>(Factory, messageCombineFilter);
					afrMessages.Load();
				}
				return afrMessages;
			}
		}
		EDIMessageForDisplayCollection<JPAFRMessage> afrMessages;

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging);
					messages.Load(query);
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		public VesselVoyage NewVesselVoyage
		{
			get
			{
				var query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.JPAFRNewVesselVoyage);
				query.AddToFilter(CusAddInfoSchema.B7_ParentID, PK);
				return Factory.LoadTop1<VesselVoyage>(query);
			}
		}

		#endregion

		#region Override Methods

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (Consol == null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			if (IsInDatabase && JPH_MessageStatusInfo.HasChanges)
			{
				Logs.AddNew(Events.MessageStatusChange, ZString.Format("{0} - {1}", JPH_MessageStatus, new MessageStatusList().GetDescriptionFromCode(JPH_MessageStatus)));
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Bills.DeleteAll();
				Messages.RemoveAndDeleteAll();
				((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			}
			base.Delete();
			InBondDetailInitiator = null;
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(this.Bills);
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateJobReferenceIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				JPH_JobReference = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		public void PopulateJobReferenceIfNeeded()
		{
			var consol = Consol;
			if (consol != null)
			{
				consol.PopulateJK_UniqueConsignRefIfNeeded();
				JPH_JobReference = consol.JK_UniqueConsignRef;
			}
			else
			{
				PopulateNumberPropertyIfRequired<ZString>(JPH_JobReferenceInfo, factory => Env.NumberFountains.JPAFRJobReference.GetNextFormatted(factory));
			}
		}

		#endregion

		#region Override Properties

		#region JPH_IsShippingLineEntry

		[ReadOnly(true)]
		public override ZBool JPH_IsShippingLineEntry
		{
			get { return base.JPH_IsShippingLineEntry; }
			set
			{
				bool hasChanged = base.JPH_IsShippingLineEntry != value;
				base.JPH_IsShippingLineEntry = value;
				if (hasChanged && this.Bills != null)
				{
					Bills.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JPH_MessageStatus

		[ReadOnly(true)]
		public override ZString JPH_MessageStatus
		{
			get { return base.JPH_MessageStatus; }
			set { base.JPH_MessageStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRHeader|JPH_MessageStatusDescription", Caption = "Message Status Description", ShortCaption = "Msg. Status Desc.")]
		public ZString JPH_MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(JPH_MessageStatus); }
		}

		public ZPropertyInfo JPH_MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_MessageStatusDescription); }
		}

		#endregion

		#region JPH_OverrideFreightDefaults

		public event CancelEventHandler OnOverrideFreightDefaultsChanging;

		public override ZBool JPH_OverrideFreightDefaults
		{
			get { return base.JPH_OverrideFreightDefaults; }
			set { SetOverrideDefaults(value); }
		}

		#endregion

		#region JPH_RL_NKDischarge

		public override ZString JPH_RL_NKDischarge
		{
			get { return base.JPH_RL_NKDischarge; }
			set
			{
				bool hasChanged = base.JPH_RL_NKDischarge != value;
				base.JPH_RL_NKDischarge = value;
				if (hasChanged && this.Bills != null)
				{
					Bills.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JPH_VesselName

		[List(nameof(Lookups) + "." + nameof(JPAFRHeaderLookups.Vessels))]
		public override ZString JPH_VesselName
		{
			get { return base.JPH_VesselName; }
			set
			{
				if (base.JPH_VesselName != value)
				{
					if (!IsCopying)
					{
						VesselCombination.DefaultCallSignAndNationalityIfNeeded(value);
					}
					base.JPH_VesselName = value;
				}
			}
		}

		public RefVessel Vessel => VesselCombination.Vessel;

		public JPAFRVesselCombination VesselCombination => vesselCombination ?? (vesselCombination = new JPAFRVesselCombination(Factory, JPH_VesselNameInfo, JPH_RadioCallSignInfo, JPH_RN_NKCountryOfRegInfo));
		JPAFRVesselCombination vesselCombination;

		#endregion

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		public static string DeactiveIsNotAllowedMessage
		{
			get { return ResString.GetMultilingualString("Enterprise.Customs.JP.AFR.Business.JPAFRHeader|DeactiveNotAllow", "This job may not be deactivated because status indicates that the Advance Filing Rules portion of the job is active with Japan Customs. The bills would need to be deleted from Japan Customs before the job is deactivated."); }
		}

		public override string CanCancel()
		{
			var result = base.CanCancel();
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}
			if (IsAnyBillAlreadyRegistered || IsAnyBillHaveMessagingInProgress || IsBillRegistrationCompleted || MessageStatusList.IsMessagingInProgressType(JPH_MessageStatus))
			{
				return DeactiveIsNotAllowedMessage;
			}
			if (!checkingCanCancelRelatedObjects)
			{
				checkingCanCancelRelatedObjects = true;
				try
				{
					var consol = Consol;
					if (consol != null)
					{
						var canCancelConsol = consol.CanCancel();
						if (!string.IsNullOrEmpty(canCancelConsol))
						{
							return Res.GetString(
								"7AB0CA7F-DCD0-45FE-8631-9C6223AFC667",
								"This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.") +
								System.Environment.NewLine + consol.HumanReadableName + ": " + canCancelConsol;
						}
					}
				}
				finally
				{
					checkingCanCancelRelatedObjects = false;
				}
			}

			return null;
		}
		bool checkingCanCancelRelatedObjects;

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);
			if (!IsDeleted && !JPH_IsActive)
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		const string BillRegistrationReference = "Bill Registration";
		const string DepartureTimeRegistrationReference = "Departure Time Registration";

		public IEnumerable<JPAFRContainer> GetMatchingContainers(ZGuid containerPKToExclude, ZString containerNumber)
		{
			foreach (var otherBill in Bills)
			{
				foreach (var container in otherBill.Containers.Where(c => c.PK != containerPKToExclude && c.JPC_ContainerNum == containerNumber))
				{
					yield return container;
				}
			}
		}

		public void LogBillRegistrationCompletion()
		{
			var log = Logs.AddNew(Events.TaskCompleted, BillRegistrationReference);
			log.SL_GS_NKUser = User.ServiceUserCode;
		}

		public void CancelBillRegistrationCompletionLog()
		{
			foreach (var log in Logs.Find(BillRegistrationCompletedQuery))
			{
				log.Cancel();
			}
		}

		public void LogDepartureTimeRegistration()
		{
			var log = Logs.AddNew(Events.TaskCompleted, DepartureTimeRegistrationReference);
			log.SL_GS_NKUser = User.ServiceUserCode;
		}

		public void CancelDepartureTimeRegistrationLog()
		{
			foreach (var log in Logs.Find(DepartureTimeRegisteredQuery))
			{
				log.Cancel();
			}
		}

		public bool ShouldSynchroniseWithConsol
		{
			get { return !JPH_OverrideFreightDefaults && Consol != null && !HasAFRMessages; }
		}

		public bool ShouldSynchroniseWithSailing
		{
			get { return !JPH_OverrideFreightDefaults && Sailing != null && !HasAFRMessages; }
		}

		bool HasAFRMessages
		{
			get
			{
				if (hasAFRMessagesCached == null)
				{
					hasAFRMessagesCached = new CachedProperty<bool>(Factory, () =>
					{
						return Messages.OfType<XmlEDIMessage>().Any(x => x.IsAFRTransmitMessage());
					});
				}
				return hasAFRMessagesCached.Value;
			}
		}
		CachedProperty<bool> hasAFRMessagesCached;

		public BusinessObjectSynchroniser Synchroniser
		{
			get
			{
				if (fSynchroniser == null)
				{
					var isShippingLineEntry = JPH_IsShippingLineEntry;
					IBusinessObjectState consol = Consol;
					var sailing = Sailing;
					if (!isShippingLineEntry && consol != null)
					{
						fSynchroniser = new JPAFRHeaderConsolSynchroniser(this);
#pragma warning disable
						consol.UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
						consol.UpdatedByDataRefreshIncludingChildren += OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
					}
					else if (isShippingLineEntry && sailing != null)
					{
						fSynchroniser = new JPAFRHeaderSailingSynchroniser(this);
					}
					else
					{
						throw new NotSupportedException(string.Format("You can't synchronise when you don't have a {0} for this {1} job.",
							isShippingLineEntry ? "sailing" : "consol",
							isShippingLineEntry ? "VOCC" : "NVOCC"));
					}
				}
				return fSynchroniser;
			}
		}
		BusinessObjectSynchroniser fSynchroniser;

		#region InBond Detail Initiator

		public IInBondDetailInitiator InBondDetailInitiator
		{
			get { return inBondDetailInitiator; }
			set { inBondDetailInitiator = value; }
		}
		IInBondDetailInitiator inBondDetailInitiator;

		public bool HasInBondDetailInitiator
		{
			get { return inBondDetailInitiator != null; }
		}

		#endregion

		#region Implementation

		#region Suspend Update Matching Containers

		internal IDisposable SuspendUpdateMatchingContainers()
		{
			return new UpdateMatchingContainersSuspender(this);
		}

		bool IsUpdateMatchingContainersSuspended
		{
			get { return updateMatchingContainersSuspenderIndex > 0; }
		}

		byte updateMatchingContainersSuspenderIndex;

		class UpdateMatchingContainersSuspender : IDisposable
		{
			public UpdateMatchingContainersSuspender(JPAFRHeader header)
			{
				this.header = header;
				header.updateMatchingContainersSuspenderIndex++;
			}

			readonly JPAFRHeader header;

			#region IDisposable Members

			public void Dispose()
			{
				header.updateMatchingContainersSuspenderIndex--;
			}

			#endregion
		}

		#endregion

		internal void UpdateMatchingContainers(ZGuid containerPK, ZString containerNum, ZPropertyInfo info)
		{
			if (!IsUpdateMatchingContainersSuspended)
			{
				var fieldName = info.Name;
				if (!MatchingContainerUpdateInProgressFieldNames.Contains(fieldName))
				{
					try
					{
						MatchingContainerUpdateInProgressFieldNames.Add(fieldName);
						var newValue = info.Value;
						foreach (var container in GetMatchingContainers(containerPK, containerNum))
						{
							container[fieldName] = newValue;
						}
					}
					finally
					{
						MatchingContainerUpdateInProgressFieldNames.Remove(fieldName);
					}
				}
			}
		}

		List<string> MatchingContainerUpdateInProgressFieldNames
		{
			get { return matchingContainerUpdateInProgressFieldNames ?? (matchingContainerUpdateInProgressFieldNames = new List<string>()); }
		}
		List<string> matchingContainerUpdateInProgressFieldNames;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		void OnConsolWasUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			if (fSynchroniser != null)
			{
				fSynchroniser.Synchronise();
			}
		}

		void SetOverrideDefaults(ZBool value)
		{
			var oldValue = JPH_OverrideFreightDefaults;
			if (oldValue != value)
			{
				var syncSource = JPH_IsShippingLineEntry ? Sailing : (IBusinessObjectState)Consol;
				if (!value && syncSource != null)
				{
					var args = new CancelEventArgs(false);
					if (OnOverrideFreightDefaultsChanging != null)
					{
						OnOverrideFreightDefaultsChanging(this, args);
					}

					if (!args.Cancel)
					{
						base.JPH_OverrideFreightDefaults = value;
						Synchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
					}
					else
					{
						JPH_OverrideFreightDefaultsInfo.RefreshBinding();
					}
				}
				else
				{
					base.JPH_OverrideFreightDefaults = value;
					RemoveSynchroniser();
				}

				RefreshBindingIncludingChildren();
			}
		}

		void RemoveSynchroniser()
		{
			if (fSynchroniser != null)
			{
				IBusinessObjectState syncSource = this.JPH_IsShippingLineEntry ? Sailing : Consol;
				if (syncSource != null)
				{
#pragma warning disable
					syncSource.UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
				}
				fSynchroniser.SetEnabled(false, fSynchroniser.DetectEnabled);
				fSynchroniser.Dispose();
				fSynchroniser = null;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("98482894-155a-44b5-9616-9da40855187b", "Advance Filing Rules");

				var parameters = new ZStringBuilder();
				parameters.AppendIfNotEmpty("JOB: ", JPH_JobReference);
				parameters.AppendIfNotEmpty("MBOL: ", JPH_MasterBillNumber);
				if (!parameters.IsEmpty)
				{
					result += " (" + parameters.ToStringWithDelimiterBetweenAppends(" ") + ")";
				}

				return result;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(JPAFRHeader header)
				: base(header)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(JPAFRBillsSchema.JPB_JPH_Header, BusinessObject.PK);
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(
					tableSchema: JobDocAddressSchema.Instance,
					mainQuery: new ZQuery(JobDocAddressSchema.E2_ParentTableCode, BusinessObject.TablePrefix),
					secondQuery: new ZQuery(JobDocAddressSchema.E2_ParentID, BusinessObject.PK));
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JPH_GB_Branch = GlbBranch.CurrentBranch.PK;
			if (ValidationUtils.GetCurrentJPDate >= JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.Value && !JPH_IsShippingLineEntry)
			{
				JPH_VesselDetailsChanged = ZBool.True;
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (!JPH_IsActive && !IsDeleted)
			{
				this.SetCountedReadOnlyIncludingChildren(!JPH_IsActive);
			}
		}

		#endregion

		#region IDocAddresses Members

		[ChildEditable]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					fDocAddresses.Sort(JobDocAddress.Schema.E2_AddressSequence);
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		internal JobDocAddressDependentCollection fDocAddresses;

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Carrier:
					return GetJobDocAddressRequirement(addressType);
			}

			return null;
		}

		JobDocAddressRequirement GetJobDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result;
			if (!PartiesJobDocAddressRequirements.TryGetValue(addressType, out result))
			{
				result = new JobDocAddressRequirement(addressType);
				result.DefaultMax = 1;
				PartiesJobDocAddressRequirements.Add(addressType, result);
			}
			return result;
		}

		Dictionary<DocAddressType, JobDocAddressRequirement> PartiesJobDocAddressRequirements
		{
			get { return fPartiesJobDocAddressRequirements ?? (fPartiesJobDocAddressRequirements = new Dictionary<DocAddressType, JobDocAddressRequirement>()); }
		}
		Dictionary<DocAddressType, JobDocAddressRequirement> fPartiesJobDocAddressRequirements;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.Carrier
				};
			}
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JPAFRHeaderCarrierValidation(addressToValidate, this);
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			if (docAddress != null && docAddress.DocAddressType == DocAddressType.Carrier)
			{
				this.JPH_CarrierCode = ConsolDataCalculator.GetSCAC(docAddress);
			}
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return docAddress != carrier;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return JPH_JobReference; }
		}

		#endregion

		#region IJPAFRHeaderWithConsolSynchonisation Members

		void Integration.Customs.JP.AFR.IJPAFRHeaderWithConsolSynchonisation.SynchroniseWithConsolIfNeeded()
		{
			SynchroniseIfNeeded();
		}

		public void SynchroniseIfNeeded()
		{
			if (ShouldSynchroniseWithConsol || ShouldSynchroniseWithSailing)
			{
				try
				{
					using (SuspendSettingHasChanges())
					using (GetValidationSuspender())
					{
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
						Messages.CountChanged -= StopSynchronisationOnMessages_CountChanged;
						Messages.CountChanged += StopSynchronisationOnMessages_CountChanged;
					}
				}
				finally
				{
					if (IsInDatabase)
					{
						ClearSynchronisationHasChanges();
					}
				}
			}
		}

		void ClearSynchronisationHasChanges()
		{
			((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
		}

		void StopSynchronisationOnMessages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (HasAFRMessages)
			{
				Messages.CountChanged -= new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
				Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));
			}
		}

		#endregion

		#region IControllerIDProvider Members
		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var consol = Consol;
				return consol == null ? ControllerIDs.Customs.JP.AFR : ControllerIDs.JobConsol;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var consol = Consol;
				return consol == null ? PK.ToGuid() : consol.PK.ToGuid();
			}
		}
		#endregion

		#region IWorkflowProvider Members

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

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
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<JPAFRHeaderProcessTask, JPAFRHeader>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection<JPAFRHeaderProcessTask, JPAFRHeader> workflowItems;

		#endregion

		#region IWorkflowProviderCore Members

		public CargoWise.Integration.IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_GB, JPH_GB_Branch, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, Carrier.OrganisationPK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, JPH_RL_NKLoading, JPH_RL_NKLoading.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, JPH_RL_NKDischarge, JPH_RL_NKDischarge.Substring(0, 2), ZString.Empty);

			return result;
		}

		public ZString WorkflowType
		{
			get { return JPAFRWorkflowDescriptor.Constants.Code; }
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		public IGlbCompany JobHeaderCompany
		{
			get { return this.Company; }
		}

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var consol = this.Consol;
				if (consol != null)
				{
					list.Add(consol);
				}
				return list;
			}
		}

		#endregion

		#region Implementation for Information Change

		public bool HasVesselInformationChanged
		{
			get
			{
				return HasChanges && this.IsInDatabase &&
					!(
					JPH_CarrierCodeInfo.OriginalValue.Equals(JPH_CarrierCode) &&
					JPH_VesselNameInfo.OriginalValue.Equals(JPH_VesselName) &&
					JPH_VoyageInfo.OriginalValue.Equals(JPH_Voyage) &&
					JPH_RL_NKLoadingInfo.OriginalValue.Equals(JPH_RL_NKLoading) &&
					JPH_LoadingPortSuffixInfo.OriginalValue.Equals(JPH_LoadingPortSuffix)
					);
			}
		}

		public bool HasATDInformationChanged
		{
			get
			{
				return HasChanges && this.IsInDatabase &&
					!(
					JPH_RelaxedAppIdInfo.OriginalValue.Equals(JPH_RelaxedAppId) &&
					JPH_ETDInfo.OriginalValue.Equals(JPH_ETD)
					);
			}
		}

		public bool HasMasterInformationChanged
		{
			get
			{
				return HasChanges && this.IsInDatabase &&
					!(
					JPH_RelaxedAppIdInfo.OriginalValue.Equals(JPH_RelaxedAppId) &&
					JPH_RL_NKDischargeInfo.OriginalValue.Equals(JPH_RL_NKDischarge) &&
					JPH_ETDInfo.OriginalValue.Equals(JPH_ETD) &&
					JPH_ETAInfo.OriginalValue.Equals(JPH_ETA)
					);
			}
		}

		public delegate bool ShouldValidateVesselInformationChangeDelegate();
		public ShouldValidateVesselInformationChangeDelegate ShouldValidateVesselInformationChange;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new JPAFRHeaderDocumentSupporter(this); }
		}

		#endregion

		#region IEDocsProvider Members

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new JPAFRHeaderDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region VOCC Section

		public void ChangeSailing(ZGuid sailingPK)
		{
			Sailings.Remove(JPH_ParentId);
			RemoveSynchroniser();
			if (!sailingPK.IsEmpty)
			{
				JPH_ParentTableCode = JobSailingSchema.Constants.Prefix;
				var sailing = Factory.Load<JobSailing>(sailingPK);
				if (sailing != null)
				{
					Sailings.Add(sailing);
					SynchroniseIfNeeded();
					Carrier.HasChanges = true;
				}
			}
			else
			{
				JPH_ParentTableCode = ZString.Empty;
			}
			JPH_ParentId = sailingPK;
			RefreshSailingStatistics();
		}

		#region ISailingParentFindBox Members

		ZString ISailingParentFindBox.Destination
		{
			get { return ZString.Empty; }
		}

		ZString ISailingParentFindBox.DischargePort
		{
			get { return this.JPH_RL_NKDischarge; }
		}

		ZString ISailingParentFindBox.LoadPort
		{
			get { return this.JPH_RL_NKLoading; }
		}

		ZString ISailingParentFindBox.Origin
		{
			get { return ZString.Empty; }
		}

		ZGuid ISailingParentFindBox.SailingPK
		{
			get { return JPH_ParentTableCode == JobSailingSchema.Constants.Prefix ? JPH_ParentId : Guid.Empty; }
			set { ChangeSailing(value); }
		}

		ZString ISailingParentFindBox.TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		#endregion

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>(8);
			result.Add(CusAddInfoTypeAttribute.Codes.JPAFRNewVesselVoyage, typeof(VesselVoyage));
			return result;
		}

		#endregion

		#region Section for ImportFromSailing

		internal IEnumerable<BillOfLading> BillsOfLading
		{
			get
			{
				if (this.JPH_IsShippingLineEntry && fBillsOfLading == null && Sailing != null)
				{
					this.fBillsOfLading = new List<BillOfLading>(new BillsOfLadingAtPortStrategy(Sailing.Voyage).GetBillsOnVesselAt(Sailing.JX_JB_E_ARV, true));
				}
				return fBillsOfLading ?? Enumerable.Empty<BillOfLading>();
			}
		}
		List<BillOfLading> fBillsOfLading;

		public void RefreshSailingStatistics()
		{
			foreach (var bill in Bills)
			{
				bill.RefreshSailingStatistics();
			}
			fBillsOfLading = null;
			fJPH_NoOfSailingContainers = null;
			JPH_NoOfSailingBillsInfo.RefreshBinding();
			JPH_NoOfSailingContainersInfo.RefreshBinding();
		}

		public void ImportBillsOfLadingLinkedToTheSameSailing(BillImportActionCollection importActions)
		{
			var sourceBills = new List<BillOfLading>(BillsOfLading);

			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				foreach (BillImportAction importAction in importActions)
				{
					if (importAction.Action == ImportAction.Replace && !importAction.IsSelected)
					{
						sourceBills.Remove(((ISailingSynchronisationTarget<BillOfLading>)importAction.Bill).Source);
					}
					else if (importAction.Action == ImportAction.Delete && importAction.IsSelected)
					{
						this.Bills.Delete(importAction.Bill);
					}
				}
			}

			this.Bills.Synchronise(sourceBills, false);
			RefreshSailingStatistics();
		}

		#region JPH_NoOfAFRBills

		public ZInt JPH_NoOfAFRBills
		{
			get { return Bills.Count; }
		}

		public ZPropertyInfo JPH_NoOfAFRBillsInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_NoOfAFRBills); }
		}

		#endregion

		#region JPH_NoOfSailingBills

		public ZInt JPH_NoOfSailingBills
		{
			get { return BillsOfLading.Count(); }
		}

		public ZPropertyInfo JPH_NoOfSailingBillsInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_NoOfSailingBills); }
		}

		#endregion

		#region JPH_NoOfAFRContainers

		public ZInt JPH_NoOfAFRContainers
		{
			get { return Bills.Sum(x => x.JPB_NoOfAFRContainers); }
		}

		public ZPropertyInfo JPH_NoOfAFRContainersInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_NoOfAFRContainers); }
		}

		#endregion

		#region JPH_NoOfSailingContainers

		public ZInt JPH_NoOfSailingContainers
		{
			get
			{
				if (!fJPH_NoOfSailingContainers.HasValue)
				{
					fJPH_NoOfSailingContainers = BillsOfLading.Sum(x => x.RealContainers.Count);
				}
				return fJPH_NoOfSailingContainers.Value;
			}
		}
		ZInt? fJPH_NoOfSailingContainers;

		public ZPropertyInfo JPH_NoOfSailingContainersInfo
		{
			get { return GetZPropertyInfo(Schema.JPH_NoOfSailingContainers); }
		}

		#endregion

		#endregion
	}
}
