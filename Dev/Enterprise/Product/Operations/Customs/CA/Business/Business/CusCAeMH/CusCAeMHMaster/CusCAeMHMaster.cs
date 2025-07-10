using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[CodeProperty(CusCAeMHMaster.Schema.BP_PrimaryCCN), DescriptionProperty(CusCAeMHMaster.Schema.BP_MasterBill)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusCAeMHMaster : AutoCusCAeMHMaster,
		Integration.Customs.CA.ICusCAeMHMaster,
		IDocAddresses,
		IEDIMessageCollectionProvider,
		IWorkflowProvider,
		IWorkflowTriggerEventSource,
		IJobInvoicingPlugIn,
		IEDocsProvider,
		IMessageManageableBizObj,
		ICAEDIFACTMessageAttachee,
		IControllerIDProvider,
		IACIForwarderMessageProvider,
		ITemplateCopyable,
		ISynchroniserReadOnlyMembersProvider,
		IDocumentSupportable
	{
		public CusCAeMHMaster(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoCusCAeMHMaster.Schema
		{
			public const string BP_RL_NKDiscPortName = "BP_RL_NKDiscPortName";
			public const string BP_CBSACarrierName = "BP_CBSACarrierName";
			public const string BP_CBSADischargePortName = "BP_CBSADischargePortName";
			public const string BP_CBSADischargeSubLocationName = "BP_CBSADischargeSubLocationName";
			public const string BP_MessageStatusDescription = "BP_MessageStatusDescription";
			public const string BP_CustomsStatusDescription = "BP_CustomsStatusDescription";
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region new properties

		public ForwardingConsol Consol
		{
			get { return Factory.Load<ForwardingConsol>(BP_ParentID); }
		}

		public JobDocAddress Consolidator
		{
			get
			{
				if (fConsolidator == null || fConsolidator.IsDeleted)
				{
					fConsolidator = DocAddresses.FindOrCreateWithRequirement(new CAeMHDocAddressRequirement(Factory, DocAddressType.Consolidator));
				}
				return fConsolidator;
			}
		}
		JobDocAddress fConsolidator;

		public JobDocAddress PlaceOfConsolidation
		{
			get
			{
				if (fPlaceOfConsolidation == null || fPlaceOfConsolidation.IsDeleted)
				{
					fPlaceOfConsolidation = DocAddresses.FindOrCreateWithRequirement(new CAeMHDocAddressRequirement(Factory, DocAddressType.PlaceOfConsolidation));
				}
				return fPlaceOfConsolidation;
			}
		}
		JobDocAddress fPlaceOfConsolidation;

		public ZBool ReadyToClose => HouseBills.All(houseBill => houseBill.IsAccepted);

		public ZString FormattedLatestD4MessageStatus
		{
			get
			{
				var result = ZString.Empty;
				var statusCode = BP_D4MessageStatus;
				if (!statusCode.IsEmpty)
				{
					result = ZString.Format("{0} - {1}", statusCode, BP_D4MessageStatusDescription);
				}
				return result;
			}
		}

		public ZString BP_D4MessageStatusDescription
		{
			get
			{
				return CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, BP_D4MessageStatus);
			}
		}

		public ZString HouseBillLatestD4MessageStatus => LatestD4MessageStatusAndDescriptions.LatestD4MessageStatus;

		public ZString HouseBillLatestD4MessageStatusDescription => LatestD4MessageStatusAndDescriptions.LatestD4MessageStatusDescription;

		(ZString LatestD4MessageStatus, ZString LatestD4MessageStatusDescription) LatestD4MessageStatusAndDescriptions
		{
			get
			{
				if (cachedLatestD4MessageStatusAndDescriptions == null)
				{
					cachedLatestD4MessageStatusAndDescriptions = new CachedProperty<(ZString LatestD4MessageStatus, ZString LatestD4MessageStatusDescription)>(Factory, () =>
					{
						var latestD4MessageStatusList = new List<ZString>();
						var latestD4MessageStatusDescriptionList = new List<ZString>();
						foreach (var bill in HouseBills)
						{
							if (!bill.BW_D4MessageStatus.IsEmpty)
							{
								latestD4MessageStatusList.Add(bill.BW_D4MessageStatus);
								latestD4MessageStatusDescriptionList.Add(bill.BW_D4MessageStatusDescription);
							}
						}

						var latestD4MessageStatus = ZString.Join("| ", latestD4MessageStatusList.Distinct().ToArray());
						var latestD4MessageStatusDescription = ZString.Join("| ", latestD4MessageStatusDescriptionList.Distinct().ToArray());
						return (latestD4MessageStatus, latestD4MessageStatusDescription);
					});
				}
				return cachedLatestD4MessageStatusAndDescriptions.Value;
			}
		}
		CachedProperty<(ZString LatestD4MessageStatus, ZString LatestD4MessageStatusDescription)> cachedLatestD4MessageStatusAndDescriptions;

		public bool IsValidatingAll => isValidatingAll;

		bool isValidatingAll;

		public IDisposable SetIsValidatingAll()
		{
			return new DisposableAction(
				() => CommenceMasterValidation(),
				() => FinishMasterValidation()
			);
		}

		void CommenceMasterValidation()
		{
			isValidatingAll = true;
			HouseBills.ForEach(house =>
			{
				if (!HouseValidationDictionary.TryGetValue(house.BW_HouseBill, out var alreadyDuplicated))
				{
					HouseValidationDictionary.Add(house.BW_HouseBill, false);
				}
				else if (!alreadyDuplicated)
				{
					HouseValidationDictionary[house.BW_HouseBill] = true;
				}
			});
		}

		void FinishMasterValidation()
		{
			isValidatingAll = false;
			HouseValidationDictionary.Clear();
		}

		public Dictionary<ZString, bool> HouseValidationDictionary => houseValidationDictionary ?? (houseValidationDictionary = new Dictionary<ZString, bool>());
		Dictionary<ZString, bool> houseValidationDictionary;

#if DEBUG
		internal int ValidationCallsOriginatingFromMaster { get; set; }
#endif

		#endregion
		#region overrides

		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.AmendmentCodes))]
		public override ZString BP_AmendReasonCode
		{
			get { return base.BP_AmendReasonCode; }
			set { base.BP_AmendReasonCode = value; }
		}

		#region BP_OverrideFreightDefaults

		public event CancelEventHandler OnOverrideFreightDefaultsChanging;

		public override ZBool BP_OverrideFreightDefaults
		{
			get => base.BP_OverrideFreightDefaults;
			set
			{
				var oldValue = BP_OverrideFreightDefaults;
				if (oldValue != value)
				{
					if (!value && Consol != null)
					{
						var args = new CancelEventArgs();
						if (OnOverrideFreightDefaultsChanging != null)
						{
							OnOverrideFreightDefaultsChanging(this, args);
						}

						if (!args.Cancel)
						{
							base.BP_OverrideFreightDefaults = value;
							ConsolSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						}
						else
						{
							BP_OverrideFreightDefaultsInfo.RefreshBinding();
						}
					}
					else
					{
						base.BP_OverrideFreightDefaults = value;
						DisposeConsolSynchroniser();
					}
					RefreshBindingIncludingChildren();
				}
			}
		}

		#endregion

		#region BP_RL_NKDiscPort

		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.DischargePorts))]
		public override ZString BP_RL_NKDiscPort
		{
			get { return base.BP_RL_NKDiscPort; }
			set
			{
				bool hasChanges = base.BP_RL_NKDiscPort != value;
				base.BP_RL_NKDiscPort = value;
				if (!IsCopying && hasChanges)
				{
					DefaultPortOfClearanceAndSubLocationCode();
				}
			}
		}

		void DefaultPortOfClearanceAndSubLocationCode()
		{
			var onlyIsEmpty = IsInDatabase;
			if (CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.Value)
			{
				UNLOCODefaulter.DefaultCustomsCode(() => BP_CBSADischargePortInfo, CACustomsCodeType.Office, onlyIsEmpty);
				UNLOCODefaulter.DefaultCustomsCode(() => BP_CBSADischargeSubLocationInfo, CACustomsCodeType.SubLocation, onlyIsEmpty);
			}
		}

		public RefUNLOCO DischargePort
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, this.BP_RL_NKDiscPort); }
		}

		public ZString BP_RL_NKDiscPortName
		{
			get
			{
				var port = DischargePort;
				return port != null ? port.RL_PortName : ZString.Empty;
			}
		}

		#endregion

		#region BP_ModeOfTransport

		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.TransportModeList))]
		public override ZString BP_ModeOfTransport
		{
			get { return base.BP_ModeOfTransport; }
			set { base.BP_ModeOfTransport = value; }
		}

		public bool IsSea
		{
			get { return BP_ModeOfTransport == TransportTypeList.Codes.Sea; }
		}

		public bool IsAir
		{
			get { return BP_ModeOfTransport == TransportTypeList.Codes.Air; }
		}

		#endregion

		#region BP_CBSACarrierCode

		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.Carriers))]
		public override ZString BP_CBSACarrierCode
		{
			get { return base.BP_CBSACarrierCode; }
			set { base.BP_CBSACarrierCode = value; }
		}

		public ZString BP_CBSACarrierName
		{
			get
			{
				var result = ZString.Empty;
				var cBSACarrierCode = BP_CBSACarrierCode;
				if (!cBSACarrierCode.IsEmpty)
				{
					if (zZCarrierLoader == null)
					{
						zZCarrierLoader = new ZZRefCarrierCombined.Loader(Factory);
					}

					result = zZCarrierLoader.LoadFromCode(Core.Constants.CountryCodes.Canada, cBSACarrierCode)?.ZZ4_Description ?? ZString.Empty;
				}

				return result;
			}
		}
		ZZRefCarrierCombined.Loader zZCarrierLoader;

		#endregion

		#region BP_CBSADischargePort

		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.DischargeOffices))]
		public override ZString BP_CBSADischargePort
		{
			get { return base.BP_CBSADischargePort; }
			set
			{
				var newValue = value.IsEmpty ? value : value.PadLeft(4, '0');
				bool hasChanges = base.BP_CBSADischargePort != newValue;
				base.BP_CBSADischargePort = newValue;
				if (BP_ParentID.IsEmpty && !IsCopying && hasChanges)
				{
					UNLOCODefaulter.DefaultUNLOCOCode(value, IsInDatabase);
				}
			}
		}

		public ZString BP_CBSADischargePortName
		{
			get
			{
				var dischargeOffices = Lookups.DischargeOffices;
				dischargeOffices.Load();
				return dischargeOffices.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == BP_CBSADischargePort)?.ZZD_Description ?? ZString.Empty;
			}
		}

		#endregion

		#region BP_CBSADischargeSubLocation

		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.DischargeSubLocations))]
		public override ZString BP_CBSADischargeSubLocation
		{
			get { return base.BP_CBSADischargeSubLocation; }
			set { base.BP_CBSADischargeSubLocation = value; }
		}

		public ZString BP_CBSADischargeSubLocationName
		{
			get { return Lookups.DischargeSubLocations.GetDescriptionFromCode(this.BP_CBSADischargeSubLocation); }
		}

		#endregion

		#region BP_MessageStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.MessageStatuses))]
		public override ZString BP_MessageStatus
		{
			get { return base.BP_MessageStatus; }
			set { base.BP_MessageStatus = value; }
		}

		public ZString BP_MessageStatusDescription
		{
			get { return Lookups.MessageStatuses.GetDescriptionFromCode(this.BP_MessageStatus); }
		}

		#endregion

		#region BP_CustomsStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusCAeMHMasterLookups.CustomsStatuses))]
		public override ZString BP_CustomsStatus
		{
			get { return base.BP_CustomsStatus; }
			set { base.BP_CustomsStatus = value; }
		}

		public ZString BP_CustomsStatusDescription
		{
			get { return Lookups.CustomsStatuses.GetDescriptionFromCode(this.BP_CustomsStatus); }
		}

		#endregion

		[ReadOnly(true)]
		public override ZDateTime BP_RNSProcessingDate
		{
			get { return base.BP_RNSProcessingDate; }
			set { base.BP_RNSProcessingDate = value; }
		}

		#endregion

		#region collections

		[ChildEditable(true)]
		public CusCAeMHContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new CusCAeMHContainerCollection(this);
					RegisterEditableChildObject(fContainers);
				}
				return fContainers;
			}
		}
		CusCAeMHContainerCollection fContainers;

		ICollection Integration.Customs.CA.ICusCAeMHMaster.Containers => Containers;

		[ChildEditable(true)]
		public CusCAeMHHouseCollection HouseBills
		{
			get
			{
				if (fHouseBills == null)
				{
					fHouseBills = new CusCAeMHHouseCollection(this);
					RegisterEditableChildObject(fHouseBills);
				}
				return fHouseBills;
			}
		}
		CusCAeMHHouseCollection fHouseBills;

		[ChildEditable(true)]
		CAeMHDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new CAeMHDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		CAeMHDocAddressDependentCollection fDocAddresses;

		#endregion

		#region UNLOCO defaulting

		internal UNLOCODefaulter UNLOCODefaulter
		{
			get
			{
				return unlocoDefaulter ?? (unlocoDefaulter = new UNLOCODefaulter(Factory, () => BP_RL_NKDiscPortInfo, () => BP_ModeOfTransport));
			}
		}
		UNLOCODefaulter unlocoDefaulter;
		#endregion

		#region Implementation

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);

			if (IsInDatabase)
			{
				if (BP_MessageStatusInfo.HasChanges)
				{
					Logs.AddNew(Events.MessageStatusChange, ZString.Format("{0} - {1} Close", BP_MessageStatus, new MessageStatusList().GetDescriptionFromCode(BP_MessageStatus)));
				}
				if (BP_CustomsStatusInfo.HasChanges)
				{
					Logs.AddNew(Events.StatusChange, ZString.Format("{0} - {1} Close", BP_CustomsStatus, new EManifestForwarderJobStatusList().GetDescriptionFromCode(BP_CustomsStatus)));
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BP_GB_Branch = GlbBranch.CurrentBranch.PK;
			BP_CBSACarrierCode = GetDefaultCarrierCode();
			SetupNCT();
			//CB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			BP_CBSADischargePort = CACustomsDataRegistry.Instance.DefaultPortOfClearance.GetFallBackValueAtAllLevels(Company.PK.ToGuid(), Branch.PK.ToGuid(), Guid.Empty);
		}

		ZString GetDefaultCarrierCode()
		{
			var carrierCode = GlbBranch.CurrentBranch.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada) ?? ZString.Empty;
			if (carrierCode.IsEmpty)
			{
				carrierCode = Factory.CanadianCarrierCode();
			}
			return carrierCode.SubstringSafe(0, 4);
		}

		void SetupNCT()
		{
			var container = Factory.New<CusCAeMHContainer>();
			using (container.SuspendSettingHasChanges())
			{
				container.BQ_BP_Master = this.PK;
				container.BQ_ContainerNumber = Core.Constants.ContainerModes.NonContainerised;
				container.IsNonContainerized = true;
				var customValue = container.GetUserDefinedProperty(CusCAeMHContainer.Schema.IsNonContainerized, AddOnColumnDataType.Codes.Boolean);
				customValue.HasChanges = false;
			}
		}

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			base.OnSaving();
			PopulateJobReferenceIfNeeded();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!saveSucceeded)
			{
				foreach (EDIMessage message in Messages.ToArray())
				{
					if (message.IsTransmitMessage && !message.IsInDatabase && !message.IsDeleted)
					{
						message.Delete();
					}
				}
			}
		}

		public void PopulateJobReferenceIfNeeded()
		{
			PopulateNumberPropertyIfRequired(BP_MessageReferenceInfo, x => GetNewMessageReference(x));
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				BP_MessageStatus = (ZString)BP_MessageStatusInfo.OriginalValue;
				if (!this.IsInDatabase)
				{
					BP_MessageReference = ZString.Empty;
				}
			}
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		ACIEManifestForwaderStatusCalculator StatusCalculator
		{
			get { return statusCalculator ?? (statusCalculator = new ACIEManifestForwaderStatusCalculator(MessageTypeList.Descriptions.ACIForwarderClose)); }
		}
		ACIEManifestForwaderStatusCalculator statusCalculator;

		public bool IsLodged
		{
			get { return StatusCalculator.IsLodged(this.BP_CustomsStatus); }
		}

		ZString GetNewMessageReference(BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			var consol = Consol;
			if (consol != null)
			{
				consol.PopulateJK_UniqueConsignRefIfNeeded();
				result = consol.JK_UniqueConsignRef.Right(CusCAeMHMaster.Schema.BP_MessageReferenceMaxLength);
			}
			else
			{
				result = Env.NumberFountains.CAMasterBilleManifest.GetNextFormatted(factory);
			}
			return result;
		}

		public new CusCAeMHMasterLookups Lookups
		{
			get { return base.Lookups; }
		}

		protected override CusCAeMHMasterLookups GetNewLookups()
		{
			return new CusCAeMHMasterLookups(this);
		}

		public new CusCAeMHMasterValidation Validation
		{
			get { return base.Validation; }
		}

		protected override CusCAeMHMasterValidation GetNewValidation()
		{
			return new CusCAeMHMasterValidation(this);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(HouseBills);
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}

		public override void Delete()
		{
			Containers.DeleteAll();
			HouseBills.DeleteAll();
			DocAddresses.RemoveAndDeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Empty;

				if (!BP_MessageReference.IsEmpty)
				{
					result = BP_MessageReference;
				}

				if (!BP_PrimaryCCN.IsEmpty)
				{
					result += Res.GetString("9e38fd53-d502-4afb-a9c3-6ffb4f7005d0", " - CCN: ") + BP_PrimaryCCN;
				}

				return result;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				string result = Res.GetString("81f0e44c-0c33-44f2-80c7-5c29a7304077", "eManifest");
				if (!BP_MessageReference.IsEmpty)
				{
					result += " " + BP_MessageReference;
				}

				return result;
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource

		internal GlbCompany Company
		{
			get
			{
				return Branch?.Company ?? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			}
		}

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var consol = Consol;
				return consol != null ? new IWorkflowProviderCore[] { consol } : Array.Empty<IWorkflowProviderCore>();
			}
		}
		#endregion // IWorkflowTriggerEventSource

		#region IDocAddresses

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new CusCAeMHMasterJobDocAddressValidation(addressToValidate);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new[] { DocAddressType.Consolidator, DocAddressType.PlaceOfConsolidation }; }
		}

		#endregion

		#region IEDIMessageCollectionProvider

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return Messages; }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		#endregion

		#region IWorkflowProvider

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get
			{
				return WorkflowDescriptors.CAeManifestWorkflowDescriptorCode;
			}
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
#if DEBUG
		internal
#endif
		ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (fWorkflowItems == null)
				{
					fWorkflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<CusCAeMHMasterProcessTask, CusCAeMHMaster>(this));
					RegisterEditableChildObject(fWorkflowItems);
				}
				return fWorkflowItems;
			}
		}
		ProcessTaskCollection<CusCAeMHMasterProcessTask, CusCAeMHMaster> fWorkflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}
		#endregion

		#region IJobInvoicingPlugIn

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CusCAeMHJobInvoicingSupporter(this)); }
		}
		IJobInvoicingSupporter fInvoicingSupporter;

		#endregion

		#region IJobHeaderParent

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateJobReferenceIfNeeded();
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return BP_MessageReference; }
		}

		#endregion

		#region IEDocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new CusCAeMHMasterDocumentSupporter(this); }
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return fDocManagerInfo ?? (fDocManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CAeManifest)); }
		}
		DocManagerInfo fDocManagerInfo;

		#endregion

		#region IMessageManageableBizObj

		public bool IsInAStatusAmendmentSendable
		{
			get { return false; }
		}

		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotSupportedException("The property is not supported.");
		}

		public IMessageManager GetMessageManagerForAmendmentDetection()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		#endregion

		public const string JobIdentificationPrefix = "CLS-";

		#region ICAEDIFACTMessageAttachee

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get { return HasChanges; }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return JobIdentificationPrefix + BP_MessageReference; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return BP_CustomsStatus; }
			set { BP_CustomsStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return this.BP_MessageStatus; }
			set { BP_MessageStatus = value; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return this; }
		}

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			Messages.Add(message);
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return true; }
		}

		#endregion

		#region IACIForwarderCloseProvider

		public bool IsPostArrival
		{
			get
			{
				if (isPostArrival == null)
				{
					isPostArrival = new CachedProperty<ZBool>(Factory, () =>
					{
						return MessagesForDisplay.Cast<EDIMessage>().Any(m => m.IsPostArrivalMessage());
					});
				}
				return isPostArrival.Value;
			}
		}
		CachedProperty<ZBool> isPostArrival;

		ZString IACIForwarderMessageProvider.AmendmentReason
		{
			get { return BP_AmendReasonCode; }
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.CA.CAHouseBilleManifest; }
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			return Clone();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(CusCAeMHMasterSchema.Constants.BP_MasterBill);
			result.Add(CusCAeMHMasterSchema.Constants.BP_MasterHouseBill);
			result.Add(CusCAeMHMasterSchema.Constants.BP_MasterHouseCCN);
			result.Add(CusCAeMHMasterSchema.Constants.BP_PrimaryCCN);
			result.Add(CusCAeMHMasterSchema.Constants.BP_ParentID);
			result.Add(CusCAeMHMasterSchema.Constants.BP_ParentTableCode);
			result.Add(CusCAeMHMasterSchema.Constants.BP_CustomsStatus);
			result.Add(CusCAeMHMasterSchema.Constants.BP_MessageStatus);
			result.Add(CusCAeMHMasterSchema.Constants.BP_MessageReference);
			result.Add(CusCAeMHMasterSchema.Constants.BP_GB_Branch);
			result.Add(CusCAeMHMasterSchema.Constants.BP_ATA);
			result.Add(CusCAeMHMasterSchema.Constants.BP_AmendReasonCode);
			result.Add(CusCAeMHMasterSchema.Constants.BP_SystemCreateTimeUtc);
			result.Add(CusCAeMHMasterSchema.Constants.BP_SystemCreateUser);
			return result;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (CusCAeMHMaster)base.CloneInternal(args);
			foreach (var address in DocAddresses)
			{
				templateCopy.DocAddresses.Add(address.Clone());
			}
			foreach (var house in HouseBills)
			{
				var houseCopy = (CusCAeMHHouse)house.Clone();
				houseCopy.BW_BP_Master = templateCopy.PK;
			}
			return templateCopy;
		}

		#endregion

		#region Consol synchronising

		internal CusCAeMHMasterSynchroniser ConsolSynchroniser
		{
			get { return fConsolSynchroniser ?? (fConsolSynchroniser = new CusCAeMHMasterSynchroniser(this, Consol)); }
		}
		CusCAeMHMasterSynchroniser fConsolSynchroniser;

		internal bool ShouldSynchroniseWithConsol
		{
			get
			{
				if (shouldSynchroniseWithConsolCache == null)
				{
					shouldSynchroniseWithConsolCache = new CachedProperty<bool>(Factory, delegate
					{
						return BP_CustomsStatus.IsEmpty && Consol != null && this.HouseBills.All(x => x.ShouldSynchroniseWithShipment) && !BP_OverrideFreightDefaults;
					});
				}
				return shouldSynchroniseWithConsolCache.Value;
			}
		}
		CachedProperty<bool> shouldSynchroniseWithConsolCache;

		public void EnableAndSynchronise(bool forceSync = false)
		{
			if (Consol != null)
			{
				using (GetValidationSuspender())
				{
					ConsolSynchroniser.SetEnabled(ShouldSynchroniseWithConsol, ConsolSynchroniser.DetectEnabled);
					if (forceSync)
					{
						ConsolSynchroniser.Synchronise(forceSync);
					}
					else
					{
						ConsolSynchroniser.Synchronise();
					}
				}
			}
		}

		protected void DisposeConsolSynchroniser()
		{
			if (fConsolSynchroniser != null)
			{
				fConsolSynchroniser.SetEnabled(false, fConsolSynchroniser.DetectEnabled);
				fConsolSynchroniser.Dispose();
				fConsolSynchroniser = null;
			}
		}

		#endregion

		#region Notice Messages

		public EDIMessageForDisplayCollection<EDIMessage> MessagesForDisplay
		{
			get
			{
				if (messagesForDisplay == null)
				{
					var query = new ZQuery();
					query.AddToFilter(EDIMessageQueryHelper.GetEDIMessageGenPivotQuery(new[] { PK }, new ZString[] { EDIMessageSubTypeList.Codes.XmlUniversalEvent, UniversalEventMessageTypes.Codes.D4Notices }));
					query.AddToFilter(Messages.CompleteFilter, JoinCondition.Or);

					messagesForDisplay = new EDIMessageForDisplayCollection<EDIMessage>(Factory, query);
					messagesForDisplay.Load();
					Messages.CountChanged += Messages_CountChanged;
				}

				return messagesForDisplay;
			}
		}
		EDIMessageForDisplayCollection<EDIMessage> messagesForDisplay;

		void Messages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && e.BizObject != null)
			{
				MessagesForDisplay.Add(e.BizObject);
				MessagesForDisplay.RefreshBinding();
			}
		}

		#endregion

		public IEnumerable<UniversalEventMessage> D4MessagesOnMasterBillAndHouseBills
		{
			get
			{
				var list = new List<UniversalEventMessage>();
				list.AddRange(MessagesForDisplay.Where(x => x is UniversalEventMessage && x.EM_MessageSubType == UniversalEventMessageTypes.Codes.D4Notices).Select(y => (UniversalEventMessage)y));

				foreach (var bill in HouseBills)
				{
					list.AddRange(bill.MessagesForDisplay.Where(x => x is UniversalEventMessage && x.EM_MessageSubType == UniversalEventMessageTypes.Codes.D4Notices).Select(y => (UniversalEventMessage)y));
				}

				return list;
			}
		}
	}
}
