using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.EU.NCTS.Business.ResultOfCOntrol;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.EU.NCTS;
using static Enterprise.MasterFiles.Business.OrgCusCode;
using static Enterprise.MasterFiles.Business.WorkflowDescriptor;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[CodeProperty(NctsHeader.Schema.BH_JobReference)]  // For eDocs when child - Don't use MRN because not all jobs have an MRN
	[DescriptionProperty(nameof(HumanReadableNameConcrete))]  // For eDocs when child
	[UniversalDataContext(DataContextType.NctsHeader)]
	[SystemDefinedValues]
	[UserDefinedValues]
	[VisualizableDocumentsSupportable(nameof(Documents.DocDataObjects.NctsHeaderVisualizableDocumentSupporter))]

	public partial class NctsHeader : CusInBondHeader
		, INctsMovement
		, IDocAddresses
		, ICanBeImportOrExport
		, IEuOfficeCodeProvider
		, IDocManagerSupport
		, ITemplateCopyable
		, ICusAddInfoTypeSupporter
		, IWorkflowProvider
		, IWorkflowTriggerEventSource
		, Integration.Customs.EU.NCTS.ICusInBondHeader
		, IDisposable
		, IJobNumber
		, ICusCodeDataTypeSupporter
		, IAllowPermitProcessing
		, IUniversalXMLNoteParent
		, IDocumentSupportable
		, IEDocsProvider
		, ISupportMultipleResourceStringData
		, INctsHeader
		, ICusInBondPersonTypeProvider
		, IHaveServices
		, INCTSAutoSendingMessageSupporter
		, ICusSupportingInfoTypeSupporter
		, ICommonGoodsItemsIntegratorProvider
		, IRelatedJob
		, INCTSCusAuthorizationUsageMaster
		, ISequenceNumberHeader
		, ICanSupportPhase5
		, INctsAdditionalInfoSequenceHeader
		, IHaveRequiredDocuments
		, ICustomsFileParent
		, ICusInBondContainerTypeSupporter
		, ICusSealTypeSupporter
		, Shared.ICountryCodeProvider
		, IWarehouseIntegrationSupporter
		, ITypeDeciderContext
		, ICusReferenceTypeSupporter
		, IValidateForCustomsMessagingSupporter
		, ICustomFieldProvider
		, IWorkflowAffectedPropertyProvider
	{
		public NctsHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondHeader.Schema
		{
			public const string ContactEmail = nameof(NctsHeader.ContactEmail);
			public const string ContactFullName = nameof(NctsHeader.ContactFullName);
			public const string ContactPhone = nameof(NctsHeader.ContactPhone);
			public const string CountryOfDispatch = nameof(NctsHeader.CountryOfDispatch);
			public const string DeclarationPlace = nameof(NctsHeader.DeclarationPlace);
			public const string DepartureCustomsOfficeCode = nameof(NctsHeader.DepartureCustomsOfficeCode);
			public const string DestinationCustomsOfficeCode = nameof(NctsHeader.DestinationCustomsOfficeCode);
			public const string DestinationCustomsOfficeCodeForArrival = nameof(NctsHeader.DestinationCustomsOfficeCodeForArrival);
			public const string DestinationCustomsOfficeCodeForDeparture = nameof(NctsHeader.DestinationCustomsOfficeCodeForDeparture);
			public const string EnquiryCustomsOfficeCode = nameof(NctsHeader.EnquiryCustomsOfficeCode);
			public const string Explanation = nameof(NctsHeader.Explanation);
			public const string HeaderUnloadingNotes = nameof(NctsHeader.HeaderUnloadingNotes);
			public const string JobReferenceNumber = nameof(NctsHeader.JobReferenceNumber);
			public const string LocalReferenceNumber = nameof(NctsHeader.LocalReferenceNumber);
			public const string LocalReferenceNumberForDisplay = nameof(NctsHeader.LocalReferenceNumberForDisplay);
			public const string MovementReferenceNumber = nameof(NctsHeader.MovementReferenceNumber);
			public const string MovementReferenceIssueDate = nameof(NctsHeader.MovementReferenceIssueDate);
			public const string PlaceOfUnloading = nameof(NctsHeader.PlaceOfUnloading);
			public const string PortOfDispatch = nameof(NctsHeader.PortOfDispatch);
			public const string TotalGrossMassInKilograms = nameof(NctsHeader.TotalGrossMassInKilograms);
			public const string TotalNumberOfItems = nameof(NctsHeader.TotalNumberOfItems);
			public const string TotalNumberOfPackages = nameof(NctsHeader.TotalNumberOfPackages);
			public const string UnloadedMeansOfTransportAtDepartureIdentity = nameof(NctsHeader.UnloadedMeansOfTransportAtDepartureIdentity);
			public const string UnloadedMeansOfTransportAtDepartureNationality = nameof(NctsHeader.UnloadedMeansOfTransportAtDepartureNationality);
			public const string MovementReferenceExpiryDate = nameof(NctsHeader.MovementReferenceExpiryDate);
			public const string Principal = nameof(NctsHeader.Principal);

			public const int LocalReferenceNumberMaxLength = 22;
		}

		public new static readonly NctsHeaderTypeDecider TypeDecider = new NctsHeaderTypeDecider();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => IsPhase5 ? new NctsHeaderPhase5FetchStrategy(this) : new NctsHeaderFetchStrategy(this);

		public INctsHeaderValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, () => Configuration.GetValidationDecider(this));
		CachedValue<INctsHeaderValidationDecider> validationDeciderCached;

		#region Related Objects

		[ChildEditable]
		public SealCollection Seals
		{
			get
			{
				if (seals == null)
				{
					seals = GetNewSealCollection();
					seals.Load();
					RegisterEditableChildObject(seals);
				}

				return seals;
			}
		}
		SealCollection seals;

		protected virtual SealCollection GetNewSealCollection() => new SealCollection<Seal>(this);

		[ChildEditable]
		public CusSealCollection CusSeals
		{
			get
			{
				if (cusSeals == null)
				{
					cusSeals = GetCusSealsCore();
					cusSeals.Load();
					RegisterEditableChildObject(cusSeals);
				}

				return cusSeals;
			}
		}
		CusSealCollection cusSeals;

		protected virtual CusSealCollection GetCusSealsCore() => new CusSealCollection(this);

		Type ICusSealTypeSupporter.CusSealType => typeof(CusSeal);

		[ChildEditable]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActors
		{
			get
			{
				if (cusSupplyChainActor is null)
				{
					cusSupplyChainActor = GetCusSupplyChainActorsCore();
					cusSupplyChainActor.Load();
					cusSupplyChainActor.ListChanged += CusSupplyChainActors_ListChanged;
					RegisterEditableChildObject(cusSupplyChainActor);
				}

				return cusSupplyChainActor;
			}
		}

		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActor;

		protected virtual ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> GetCusSupplyChainActorsCore()
			=> new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

		protected virtual void CusSupplyChainActors_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		/// <summary>
		/// One (dummy) bill per header
		/// </summary>
		[ChildEditable]
		public new INctsBillCollection<NctsBill> Bills
		{
			get { return (INctsBillCollection<NctsBill>)base.Bills; }
		}

		protected sealed override ICusInBondBillCollection GetNewBillsCollection()
		{
			var collection = GetNewBillCollection();
			collection.CollectionCountChange += NctsBillCollectionSequenceNumberGeneratorHandler.OnCollectionCountChange;
			collection.ApplySort(AutoCusInBondBill.Schema.B0_SystemCreateTimeUtc, ListSortDirection.Ascending);
			collection.SequenceGenerator.ReCalculateAll();

			return collection;
		}

		protected virtual INctsBillCollection<NctsBill> GetNewBillCollection() => new NctsBillCollection<NctsBill>(this);

		protected override Type BillTypeCore
		{
			get { return typeof(NctsBill); }
		}

		[ResourceStringData("NctsHeader.MovementType", ShortCaption = "Type", Caption = "Movement Type", FullDescription = "Movement Type (Departure and/or Arrival)")]
		public override ZString BH_HeaderType
		{
			get { return base.BH_HeaderType; }
			set
			{
				var oldValue = BH_HeaderType;
				base.BH_HeaderType = value;
				if (!IsCopying && oldValue != BH_HeaderType)
				{
					switch (BH_HeaderType)
					{
						case NctsMovementType.Codes.Departure:
							_ = MovementHeader;
							break;
						case NctsMovementType.Codes.Arrival:
							ArrivalMovementHeader.GoodsItems.SetArrivalGoodsItemsReadOnly();
							break;
						case NctsMovementType.Codes.DepartureAndArrival:
							_ = MovementHeader;
							ArrivalMovementHeader.GoodsItems.SetArrivalGoodsItemsReadOnly();
							break;
					}
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						MovementHeaders.MarkAsNeedingValidationIncludingChildren();
						Bills.MarkAsNeedingValidation();
						DepartureHeaderContainers.MarkAsNeedingValidation();
						CountriesOfRouting.MarkAsNeedingValidation();
						if (IsPhase5)
						{
							Bills.ForEach(b => b.GoodsItems.MarkAsNeedingValidation());
							if (IsArrivalMovement)
							{
								ArrivalHeaderContainers.MarkAsNeedingValidation();
								ArrivalMovementHeader.CustomsOffices.MarkAsNeedingValidation();
							}
						}
						else
						{
							CustomsOffices.MarkAsNeedingValidation();
						}
						Bills.RefreshMaxCountValidation();
					}
				}
			}
		}

		public override ZString BH_UniqueVoyageIdentifier
		{
			get { return base.BH_UniqueVoyageIdentifier; }
			set
			{
				if (base.BH_UniqueVoyageIdentifier != value)
				{
					base.BH_UniqueVoyageIdentifier = value;
					if (!isSuspendPopulatingItinerary)
					{
						Itinerary.HasChanges = false;
						using (Itinerary.SuspendSettingHasChanges())
						{
							Itinerary.PopulateItineraryCountryCollection();
						}
					}
				}
			}
		}

		bool isSuspendPopulatingItinerary;

		/// <summary>
		/// MovementHeader pointing to departure declaration items.
		/// </summary>
		///
		public new NctsDepartureMovementHeader MovementHeader => IsDepartureMovement ? (NctsDepartureMovementHeader)base.MovementHeader : null;

		protected sealed override CusInBondMoveHeader GetNewMovementHeader()
		{
			NctsDepartureMovementHeader departureMovementHeader = null;
			if (IsDepartureMovement)
			{
				departureMovementHeader = GetNewDepartureMovementHeader();
				departureMovementHeader.ForbidDeletion();
				RegisterEditableChildObject(departureMovementHeader);
			}
			return departureMovementHeader;
		}

		protected virtual NctsDepartureMovementHeader GetNewDepartureMovementHeader() => NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeader>(this, NctsMoveHeaderType.Codes.Departure);

		/// <summary>
		/// MovementHeader pointing to arrived items.
		/// </summary>
		public NctsArrivalMovementHeader ArrivalMovementHeader
		{
			get
			{
				if (IsArrivalMovement && (arrivalMovementHeader == null || arrivalMovementHeader.IsDeleted || arrivalMovementHeader.BM_BH != PK))
				{
					arrivalMovementHeader = NctsCommonMovementHeader.LoadOrCreate<NctsArrivalMovementHeader>(this, NctsMoveHeaderType.Codes.Arrival);
					RegisterEditableChildObject(arrivalMovementHeader);
				}
				return arrivalMovementHeader;
			}
		}
		NctsArrivalMovementHeader arrivalMovementHeader;

		/// <summary>
		/// MovementHeader pointing to unloaded items.
		/// </summary>
		public NctsUnloadingMovementHeader UnloadingMovementHeader
		{
			get
			{
				if (unloadingMovementHeader == null || unloadingMovementHeader.IsDeleted || unloadingMovementHeader.BM_BH != PK)
				{
					unloadingMovementHeader = NctsCommonMovementHeader.LoadOrCreate<NctsUnloadingMovementHeader>(this, NctsMoveHeaderType.Codes.Unloading);
					RegisterEditableChildObject(unloadingMovementHeader);
				}
				return unloadingMovementHeader;
			}
		}
		NctsUnloadingMovementHeader unloadingMovementHeader;

		public NctsCommonMovementHeader CommonMovementHeader => (NctsCommonMovementHeader)MovementHeader ?? (NctsCommonMovementHeader)ArrivalMovementHeader ?? UnloadingMovementHeader;

		protected sealed override Type MovementHeaderTypeCore => typeof(NctsCommonMovementHeader);

		[ChildEditable]
		public new NctsCommonMovementHeaderCollection MovementHeaders => (NctsCommonMovementHeaderCollection)base.MovementHeaders;

		protected override CusInBondMoveHeaderCollection GetMovementHeaders() => new NctsCommonMovementHeaderCollection(this);

		[ChildEditable]
		public INctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader> DepartureMovementHeaders
		{
			get
			{
				if (departureMovementHeaders == null)
				{
					departureMovementHeaders = GetNewDepartureMovementHeaders();
					RegisterEditableChildObject(departureMovementHeaders);
				}
				return departureMovementHeaders;
			}
		}
		INctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader> departureMovementHeaders;

		protected virtual INctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader> GetNewDepartureMovementHeaders() => new NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>(this);

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore => IsDeleted ? BusinessObjectsWithRelatedEvents : new BusinessObject[] { MovementHeader, ArrivalMovementHeader };

		#region Lookups

		public new NctsHeaderLookups Lookups
		{
			get { return (NctsHeaderLookups)base.Lookups; }
		}

		protected override CusInBondHeaderLookups GetNewLookups()
		{
			return new NctsHeaderLookups(this);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		protected sealed override CusInBondHeaderValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusInBondHeaderValidation GetNewPhase4Validation() => new NctsHeaderValidation(this);

		protected virtual CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderPhase5Validation(this);

		public new NctsHeaderValidation Validation => (NctsHeaderValidation)base.Validation;

		public bool IsPhase4 => BH_ApplicationCode == CusInBondApplicationCodeList.Codes.NCTS4;

		public bool IsPhase5 => BH_ApplicationCode == CusInBondApplicationCodeList.Codes.NCTS5;

		public bool IsPhase5Arrival => IsPhase5 && IsArrivalMovement;

		public bool IsPhase5Departure => IsPhase5 && IsDepartureMovement;

		public bool IsNctsPhase5ArrivalCustomsStatusARTPhaseFRC => IsPhase5Arrival && ArrivalMovementHeader.IsCustomsStatusARTAndPhaseFRC;

		public virtual bool MessageHasBeenSent => EffectiveMessageStatus.In<ZString>(LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent);

		public IReadOnlyList<NctsDepartureCargoDesc> DepartureGoodsItems => Factory.GetCached(ref departureGoodsItemsCached, GetDepartureGoodsItems);
		CachedProperty<NctsDepartureCargoDesc[]> departureGoodsItemsCached;

		public bool Has30600AdditionalInformation => Factory.GetValue(ref has30600AdditionalInformation, AdditionalDocuments.Has30600AdditionalInformation);
		CachedProperty<bool> has30600AdditionalInformation;

		[ResourceStringData("91E95317-CDC1-4869-89A9-22E6AAB1AB5F", Caption = "[6] Total Packages", ShortCaption = "Packs")]
		public virtual ZLong TotalNumberOfPackages => IsPhase5 ?
			Bills.SelectMany(bill => bill.GoodsItems).SelectMany(item => item.Packages.Cast<NctsPackage>()).Aggregate<NctsPackage, ZLong>(0, (current, package) => current + (package.IsBulk && package.B5_UnitCount == 0 ? 1 : package.B5_UnitCount)) :
			MovementHeader?.TotalNumberOfPackages ?? 0;

		[ResourceStringData("C96C0982-2A1B-4232-A255-B62EF8984F80", Caption = "[5] No. of Items", ShortCaption = "Items")]
		public virtual ZInt TotalNumberOfItems => IsPhase5 ? (ZInt)Bills.Sum(bill => bill.GoodsItems.Count) : MovementHeader?.TotalNumberOfItems ?? 0;

		[ResourceStringData("24A4C969-951D-445B-97B9-B5C88A597CA2", Caption = "[35] Total Gross Weight (kg)", ShortCaption = "Total Gross")]
		public virtual ZDecimal TotalGrossMassInKilograms
		{
			get
			{
				if (MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					return IsPhase5 ? movementHeader.BM_GrossWeight : movementHeader.TotalGrossMassInKilograms;
				}
				return ZDecimal.Zero;
			}
		}

		public ZDecimal TotalInvoiceValue
		{
			get
			{
				if (MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					return IsPhase5 ? Bills.SelectMany(x => x.GoodsItems).Sum(x => x.BY_MonetaryValue) : movementHeader.GoodsItems.Sum(x => x.BY_MonetaryValue);
				}
				return ZDecimal.Zero;
			}
		}

		public ZDecimal TotalNettMassInKilograms
		{
			get
			{
				if (MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					return IsPhase5 ? Bills.SelectMany(x => x.GoodsItems).Sum(x => x.NetMassInKilograms) : movementHeader.TotalNettMassInKilograms;
				}
				return ZDecimal.Zero;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_ApplicationCode = DefaultApplicationCode;
		}

		protected virtual ZString DefaultApplicationCode
		{
			get
			{
				var nctsSettings = ObjectFactory.Get<Shared.INctsSettings>();
				return nctsSettings.IsUsingPhase5(DefaultDataGroupingCode) ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;
			}
		}

		public override void OnSaving()
		{
			SetJobNumberFieldOnSaving();
			base.OnSaving();
		}

		protected override void OnFactorySaving()
		{
			SaveItinerary();
			ApportionedAmountToGuaranteesLiabilityAmount();
			base.OnFactorySaving();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			LogLogicalStatusIfRequired();
			base.OnFactorySavingBeforeTransactionCore();
		}

		void LogLogicalStatusIfRequired()
		{
			if (!IsPhase5 && !PK.IsEmpty && (ZString)EffectiveMessageStatusInfo.OriginalValue != EffectiveMessageStatus)
			{
				Logs.AddNew(Events.MessageStatusChange, EffectiveMessageStatus, ZDateTimeOffset.Now);
			}
		}

		#region Saving LRN - Local Reference Number

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				BH_JobReference = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				IntegrateWithAccountingIfRequired();
			}
		}

		protected ZString LocalReferenceNumberAmendment
		{
			get
			{
				ZString newLrn;
				var lrn = LocalReferenceNumber;
				if (lrn.Contains("/", StringComparison.OrdinalIgnoreCase))
				{
					int slashPosition = lrn.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
					var suffix = lrn.SubstringSafe(slashPosition + 1, lrn.Length - slashPosition + 1);
					int amendmentNumber = ZInt.ParseSafe(suffix, ZInt.Zero);
					amendmentNumber++;
					newLrn = lrn.SubstringSafe(0, slashPosition) + "/" + amendmentNumber.ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					newLrn = lrn + "/1";
				}
				return newLrn;
			}
		}

		#endregion

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = GetNewMessageCollection();
					ediMessages.Load();
					ediMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessages);
				}
				return ediMessages;
			}
		}
		EDIMessageCollection ediMessages;

		protected virtual EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this, Factory);

		[ChildEditable(true)]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = GetNewServiceCollection();
					services.Load(new ZQuery(JobServiceSchema.ES_ParentID, PK));
					RegisterEditableChildObject(services);
				}
				return services;
			}
		}
		JobServiceDependentCollection services;

		protected virtual JobServiceDependentCollection GetNewServiceCollection()
		{
			return new JobServiceDependentCollection(this, Factory);
		}

		#endregion

		[LightValidationTestExempt]
		public override ZGuid BH_GB
		{
			get => base.BH_GB;
			set => base.BH_GB = value;
		}

		[LightValidationTestExempt]
		[ResourceStringData("NctsHeader.BH_ApplicationCode", Caption = "Application Code", ShortCaption = "App Code")]
		public override ZString BH_ApplicationCode
		{
			get => base.BH_ApplicationCode;
			set => base.BH_ApplicationCode = value;
		}

		public ZString[] GetC0009CountryCodes()
		{
			var date = ZDateTime.Today;
			return ZZRefCusCodeListCombined.GetUniqueCodes(Factory, DefaultDataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, date, includeParentDataGroupings: true);
		}

		#region INctsMovement Implementation

		public override ZString BH_MessageStatus
		{
			get => base.BH_MessageStatus;
			set
			{
				var oldValue = BH_MessageStatus;
				base.BH_MessageStatus = value;
				BH_MessageStatusInfo.RefreshBinding(oldValue);
				if (!IsCopying && oldValue != BH_MessageStatus && !IsMarkingAsNeedingValidationSuspended)
				{
					if (IsArrivalMovement)
					{
						ArrivalMovementHeader.MarkAsNeedingValidationIncludingChildren();
					}
					if (IsDepartureMovement)
					{
						MovementHeader.MarkAsNeedingValidationIncludingChildren();
					}
				}
			}
		}

		[ResourceStringData("50B8E511-1F3F-483E-96D2-5CC7C1E8DD07", Caption = "MRN", FullDescription = "Movement Reference Number", MultipleKey = Phase4CaptionKey)]
		[ResourceStringData("639C4C12-6873-49D8-9F7E-68C2D7E5CB42", Caption = "Movement Reference Number", MediumCaption = "Movement Reference", ShortCaption = "MRN", MultipleKey = Phase5CaptionKey)]
		public ZString MovementReferenceNumber => GetMovementReferenceEntryNumberCore(false)?.CE_EntryNum ?? ZString.Empty;

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(nameof(MovementReferenceNumber));

		[ResourceStringData("51D21A87-E050-4D24-A9C4-9796A446C7E0", Caption = "Release Date")]
		public ZDateTime MovementReferenceIssueDate => MovementReferenceIssueDateCore;

		protected virtual ZDateTime MovementReferenceIssueDateCore => GetMovementReferenceEntryNumberCore(false)?.CE_IssueDate ?? ZDateTime.Empty;

		public ZPropertyInfo MovementReferenceIssueDateInfo => GetZPropertyInfo(nameof(MovementReferenceIssueDate));

		public ZDateTime MovementReferenceExpiryDate => GetMovementReferenceEntryNumberCore(false)?.CE_ExpiryDate ?? ZDateTime.Empty;

		public ZPropertyInfo MovementReferenceExpiryDateInfo => GetZPropertyInfo(nameof(MovementReferenceExpiryDate));

		public ZString PresentationCustomsOffice => IsPhase5Arrival ? ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival : DestinationCustomsOfficeCodeForArrival;

		/// <summary>
		/// An MRN typed-in by hand by user (c.f. the linked departure's MRN)
		/// </summary>
		[ReadOnlyMember(nameof(ArrivalMrnFromUserReadOnly))]
		[ResourceStringData("NctsArrival.ArrivalMrnFromUser", Caption = "MRN", FullDescription = "Movement Reference Number", MediumCaption = "MRN", ShortCaption = "MRN")]
		[MaxLength(nameof(ArrivalMrnFromUser_MaxLength))]
		public virtual ZString ArrivalMrnFromUser
		{
			get => MovementReferenceNumber;
			set
			{
				var oldValue = ArrivalMrnFromUser;
				CheckMaximumLength(ArrivalMrnFromUserInfo, value);
				MovementReferenceEntryNumber.CE_EntryNum = value;
				ArrivalMrnFromUserInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateArrivalMrnFromUser();
				}
			}
		}

		protected int ArrivalMrnFromUser_MaxLength => IsPhase5 ? 18 : 21;

		public ZPropertyInfo ArrivalMrnFromUserInfo => GetZPropertyInfo(nameof(ArrivalMrnFromUser));

		public bool ArrivalMrnFromUserReadOnly => ArrivalMrnFromUserReadOnlyCore;

		protected virtual bool ArrivalMrnFromUserReadOnlyCore
		{
			get
			{
				var nctsArrivalMovementHeader = ArrivalMovementHeader;
				return nctsArrivalMovementHeader != null && (HasArrivalGoodItemsForMRNReadOnly || IsArrivalDetailsReadOnly || IsArrivalNotificationDisabled);
			}
		}

		protected virtual bool HasArrivalGoodItemsForMRNReadOnly => IsPhase5 ? Bills.SelectMany(x => x.ArrivalGoodsItems).Any() : ArrivalMovementHeader.GoodsItems.Count > 0;

		public CusEntryNumber MovementReferenceEntryNumber => GetMovementReferenceEntryNumberCore(true);

		internal CusEntryNumber GetMovementReferenceEntryNumberCore(bool createIfNotExists)
		{
			if (mrnEntryNumber == null || mrnEntryNumber.IsDeleted)
			{
				mrnEntryNumber = createIfNotExists
					? CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode)
					: CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
				if (mrnEntryNumber != null)
				{
					RegisterEditableChildObject(mrnEntryNumber);
				}
			}
			return mrnEntryNumber;
		}

		CusEntryNumber mrnEntryNumber;

		[ChildEditable(true)]
		public CusInBondEventCollection<EnRouteSeal> EnRouteSeals
		{
			get
			{
				if (enRouteSeals == null)
				{
					enRouteSeals = new CusInBondEventCollection<EnRouteSeal>(this, CusInBondEventTypes.Codes.Seal);
					RegisterEditableChildObject(enRouteSeals);
					enRouteSeals.SetReadOnlyIncludingChildren(!IsArrivalEventAvailable);
				}
				return enRouteSeals;
			}
		}
		CusInBondEventCollection<EnRouteSeal> enRouteSeals;

		[ChildEditable(true)]
		public CusInBondEventCollection<EnRouteTransshipment> EnRouteTransshipments
		{
			get
			{
				if (enRouteTransshipments == null)
				{
					enRouteTransshipments = new CusInBondEventCollection<EnRouteTransshipment>(this, CusInBondEventTypes.Codes.Transshipment);
					RegisterEditableChildObject(enRouteTransshipments);
					enRouteTransshipments.SetReadOnlyIncludingChildren(!IsArrivalEventAvailable);
				}
				return enRouteTransshipments;
			}
		}
		CusInBondEventCollection<EnRouteTransshipment> enRouteTransshipments;

		[ChildEditable(true)]
		public EnRouteIncidentCollection EnRouteIncidents
		{
			get
			{
				if (enRouteIncidents == null)
				{
					enRouteIncidents = GetEnRouteIncidents();
					RegisterEditableChildObject(enRouteIncidents);
					enRouteIncidents.SetReadOnlyIncludingChildren(IsIncidentsReadOnly);
				}
				return enRouteIncidents;
			}
		}
		EnRouteIncidentCollection enRouteIncidents;

		public bool IsIncidentsReadOnly => !IsArrivalEventAvailable || IsArrivalDetailsReadOnly;

		protected virtual EnRouteIncidentCollection GetEnRouteIncidents() => new EnRouteIncidentCollection(this);

		public virtual ZBool IsArrivalEventAvailable => BH_ExportFlag == EventFlagList.Codes.Yes;

		[ResourceStringData("2378dd94-a0de-486d-aed6-92866a250b49|BH_ExportFlag", Caption = "Event Flag", MultipleKey = Phase4CaptionKey)]
		[ResourceStringData("38943AF0-1449-40E0-B7FA-9FB80E249D7F|BH_ExportFlag", Caption = "Incident Flag", MediumCaption = "Incident", MultipleKey = Phase5CaptionKey)]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.EventFlagList))]
		[MaxLength(1)]
		[ReadOnlyMember(nameof(BH_ExportFlagReadOnly))]
		public override ZString BH_ExportFlag
		{
			get => base.BH_ExportFlag;
			set
			{
				var oldValue = BH_ExportFlag;
				base.BH_ExportFlag = value;
				if (!IsCopying && oldValue != BH_ExportFlag)
				{
					UpdateIncidentFlagAndIncidents(oldValue);
				}
			}
		}

		public bool BH_ExportFlagReadOnly => BH_ExportFlagReadOnlyCore;

		protected virtual bool BH_ExportFlagReadOnlyCore => IsArrivalDetailsReadOnly || IsArrivalNotificationDisabled;

		public event EventHandler<CancelEventArgs> RemoveIncidentsWhenIncidentFlagChanged;

		void UpdateIncidentFlagAndIncidents(ZString prevIncidentFlagValue)
		{
			if (BH_ExportFlag == EventFlagList.Codes.No)
			{
				if (RemoveIncidentsWhenIncidentFlagDisabled())
				{
					RemoveIncidents();
					return;
				}
				base.BH_ExportFlag = prevIncidentFlagValue;
				return;
			}
			EnRouteIncidents.SetReadOnlyIncludingChildren(IsIncidentsReadOnly);
			foreach (EnRouteIncident incident in EnRouteIncidents)
			{
				incident.SetGoodsLocationReadOnly();
			}
		}

		void RemoveIncidents()
		{
			EnRouteIncidents.DeleteAll();
			EnRouteIncidents.RefreshBinding();
		}

		bool RemoveIncidentsWhenIncidentFlagDisabled()
		{
			var cancelEventArgs = new CancelEventArgs();
			if (RemoveIncidentsWhenIncidentFlagChanged != null)
			{
				RemoveIncidentsWhenIncidentFlagChanged(this, cancelEventArgs);
				return !cancelEventArgs.Cancel;
			}
			return true;
		}

		[ChildEditable(true)]
		Customs.Business.MultiLineAddInfos.CusAddInfoCollection<UnloadingRemarkAddInfo> UnloadingRemarkCollectionOfOne
		{
			get { return unloadingRemarkAddInfoCollectionOfOne ?? (unloadingRemarkAddInfoCollectionOfOne = GetUnloadingRemarkAddInfoCollection()); }
		}
		Customs.Business.MultiLineAddInfos.CusAddInfoCollection<UnloadingRemarkAddInfo> unloadingRemarkAddInfoCollectionOfOne;

		Customs.Business.MultiLineAddInfos.CusAddInfoCollection<UnloadingRemarkAddInfo> GetUnloadingRemarkAddInfoCollection()
		{
			var cusAddInfoCollection = GetUnloadingRemarkAddInfoCollectionCore();
			cusAddInfoCollection.Load();
			RegisterEditableChildObject(cusAddInfoCollection);
			return cusAddInfoCollection;
		}

		protected virtual Customs.Business.MultiLineAddInfos.CusAddInfoCollection<UnloadingRemarkAddInfo> GetUnloadingRemarkAddInfoCollectionCore()
		{
			return new Customs.Business.MultiLineAddInfos.CusAddInfoCollection<UnloadingRemarkAddInfo>(this);
		}

		public UnloadingRemarkAddInfo UnloadingRemark
		{
			get
			{
				if (UnloadingRemarkCollectionOfOne.Count == 0)
				{
					var unloadingRemark = UnloadingRemarkCollectionOfOne.AddNew();
					unloadingRemark.HasChanges = false;
				}
				return UnloadingRemarkCollectionOfOne[0].Data;
			}
		}

		// 9 occurrences
		[ChildEditable(true)]
		public Customs.Business.MultiLineAddInfos.CusAddInfoCollection<ResultsOfControlAddInfo> ResultsOfControlCollection
		{
			get { return resultsOfControlCollection ?? (resultsOfControlCollection = GetResultsOfControlCollection()); }
		}
		Customs.Business.MultiLineAddInfos.CusAddInfoCollection<ResultsOfControlAddInfo> resultsOfControlCollection;

		Customs.Business.MultiLineAddInfos.CusAddInfoCollection<ResultsOfControlAddInfo> GetResultsOfControlCollection()
		{
			var cusAddInfoCollection = new Customs.Business.MultiLineAddInfos.CusAddInfoCollection<ResultsOfControlAddInfo>(this);
			cusAddInfoCollection.Load();
			RegisterEditableChildObject(cusAddInfoCollection);
			return cusAddInfoCollection;
		}

		public void ResetUnloadedValues()
		{
			ResultsOfControlCollection.DeleteAll();
			UnloadedMeansOfTransportAtDepartureNationality = ArrivalMovementHeader?.BM_RN_NKTransportAtDepartureCountry ?? ZString.Empty; // copy expected into actual

			UnloadingRemark.G9_StateOfSealsOk = ZString.Empty;
			UnloadingRemark.G9_UnloadingRemark = ZString.Empty;
			UnloadingRemark.G9_Conform = ZString.Empty;
			UnloadingRemark.G9_UnloadingCompletion = ZString.Empty;
			UnloadingRemark.G9_UnloadingDate = ZDate.Today;
			UnloadingRemark.G9_NoOfSeals = ArrivalMovementHeader?.Seals.Count ?? 0;
			UnloadingMovementHeader?.ResetUnloadedGoodsItem(ArrivalMovementHeader.GoodsItems);
			UnloadingMovementHeader?.ResetUnloadedTotalGrossMassInKilograms(ArrivalMovementHeader.BM_GrossWeight);
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes() => GetCusAddInfoTypes();

		protected virtual Dictionary<ZString, Type> GetCusAddInfoTypes() => new Dictionary<ZString, Type>
		{
			{ Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.EuNctsResultsOfControl, typeof(Customs.Business.MultiLineAddInfos.CusAddInfo<ResultsOfControlAddInfo>) },
			{ Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.EuNctsUnloadingRemark, typeof(Customs.Business.MultiLineAddInfos.CusAddInfo<UnloadingRemarkAddInfo>) }
		};

		const string MeansOfTransportAtDepartureIdentityPointer = "18";
		public const string MeansOfTransportAtDepartureNationalityPointer = "18#1";
		const string HeaderUnloadingNotesPointer = "0";
		const string ExplanationPointer = "1";

		ZString GetResultsOfControlValue(string pointerToTheAttribute, string defaultValueForNewItem)
		{
			var value = ZString.Empty;
			var resultsOfControl = ResultsOfControlCollection.OfType<Customs.Business.MultiLineAddInfos.CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute);
			if (resultsOfControl != null)
			{
				value = resultsOfControl.Data.G9_CorrectedValue;
			}
			else
			{
				value = defaultValueForNewItem;
			}
			return value;
		}

		void SetResultsOfControlValue(string pointerToTheAttribute, ZString controlIndicator, ZString value)
		{
			var resultsOfControl = ResultsOfControlCollection.OfType<Customs.Business.MultiLineAddInfos.CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute) ?? ResultsOfControlCollection.AddNew();
			if (resultsOfControl.Data.G9_CorrectedValue != value)
			{
				resultsOfControl.Data.G9_ControlIndicator = controlIndicator;
				resultsOfControl.Data.G9_PointerToTheAttribute = pointerToTheAttribute;
				resultsOfControl.Data.G9_CorrectedValue = value.SubstringSafe(0, ResultsOfControlAddInfoSchema.G9_CorrectedValue.MaxLength);
			}
		}

		ZString GetResultsOfControlDescription(string pointerToTheAttribute, string defaultValueForNewItem)
		{
			var value = ZString.Empty;
			var resultsOfControl = ResultsOfControlCollection.OfType<Customs.Business.MultiLineAddInfos.CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute);
			if (resultsOfControl != null)
			{
				value = resultsOfControl.Data.G9_Description;
			}
			else
			{
				value = defaultValueForNewItem;
			}
			return value;
		}

		void SetResultsOfControlDescription(string pointerToTheAttribute, ZString controlIndicator, ZString value)
		{
			var resultsOfControl = ResultsOfControlCollection.OfType<Customs.Business.MultiLineAddInfos.CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute) ?? ResultsOfControlCollection.AddNew();
			if (resultsOfControl.Data.G9_Description != value)
			{
				resultsOfControl.Data.G9_ControlIndicator = controlIndicator;
				resultsOfControl.Data.G9_PointerToTheAttribute = pointerToTheAttribute;
				resultsOfControl.Data.G9_Description = value.SubstringSafe(0, ResultsOfControlAddInfoSchema.G9_Description.MaxLength);
			}
		}

		[MaxLength(27)]
		public ZString UnloadedMeansOfTransportAtDepartureIdentity
		{
			get { return GetResultsOfControlValue(MeansOfTransportAtDepartureIdentityPointer, ArrivalMovementHeader?.BM_TransportAtDeparture); }
			set
			{
				SetResultsOfControlValue(MeansOfTransportAtDepartureIdentityPointer, ResultOfControlCodes.Codes.Different, value);
				UnloadedMeansOfTransportAtDepartureIdentityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedMeansOfTransportAtDepartureIdentityInfo => GetZPropertyInfo(nameof(UnloadedMeansOfTransportAtDepartureIdentity));

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.UnloadedMeansOfTransportAtDepartureNationalityList))]
		public ZString UnloadedMeansOfTransportAtDepartureNationality
		{
			get { return GetResultsOfControlValue(MeansOfTransportAtDepartureNationalityPointer, ArrivalMovementHeader?.BM_RN_NKTransportAtDepartureCountry); }
			set
			{
				var indicatorType = value == UnloadedMeansOfTransportAtDepartureNationality ? "" : ResultOfControlCodes.Codes.Different;
				CheckMaximumLength(UnloadedMeansOfTransportAtDepartureNationalityInfo, value);
				SetResultsOfControlValue(MeansOfTransportAtDepartureNationalityPointer, indicatorType, value.SubstringSafe(0, 2));
				if (!IsValidationSuspended)
				{
					Validation.ValidateUnloadedMeansOfTransportAtDepartureNationality();
				}
				UnloadedMeansOfTransportAtDepartureNationalityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedMeansOfTransportAtDepartureNationalityInfo => GetZPropertyInfo(nameof(UnloadedMeansOfTransportAtDepartureNationality));

		[MaxLength(140)]
		public ZString HeaderUnloadingNotes
		{
			get { return GetResultsOfControlDescription(HeaderUnloadingNotesPointer, ""); }
			set
			{
				CheckMaximumLength(HeaderUnloadingNotesInfo, value);
				SetResultsOfControlDescription(HeaderUnloadingNotesPointer, ResultOfControlCodes.Codes.Other, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateHeaderUnloadingNotes();
				}
				HeaderUnloadingNotesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HeaderUnloadingNotesInfo => GetZPropertyInfo(nameof(HeaderUnloadingNotes));

		public ForwardingShipment Shipment => Factory.Load<ForwardingShipment>(BH_ParentID);

		public JobHeader Job => job ?? (job = GetJobHeader());
		JobHeader job;

		JobHeader GetJobHeader()
		{
			var shipment = Shipment;
			return shipment != null ? shipment.Job : new JobHeader.Loader(this).Load();
		}

		[MaxLength(350)]
		public ZString Explanation
		{
			get { return GetResultsOfControlDescription(ExplanationPointer, ""); }
			set
			{
				CheckMaximumLength(ExplanationInfo, value);
				SetResultsOfControlDescription(ExplanationPointer, ResultOfControlCodes.Codes.Other, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateExplanation();
				}
				ExplanationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExplanationInfo => GetZPropertyInfo(nameof(Explanation));

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				CusAuthorizationUsages.RemoveAndDeleteAll();
				EnRouteSeals.DeleteAll();
			}
			base.Delete();
		}

		public (DepartureRecordFindResult result, NctsHeader departureHeaderFound) FindRelevantDepartureRecordForCombinedDepartureAndArrival()
		{
			if (IsDepartureAndArrivalMovement)
			{
				return (DepartureRecordFindResult.AlreadyCombinedDepartureAndArrival, this);
			}
			else if (IsArrivalMovement)
			{
				NctsHeader departureHeader = null;
				if (!MovementReferenceNumber.IsEmpty)
				{
					var departureQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
					departureQuery.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, NctsMovementType.Codes.Departure);

					var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusInBondHeader.Schema.TableName);
					entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
					entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, MovementReferenceNumber);
					departureQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

					departureHeader = Factory.LoadTop1<NctsHeader>(departureQuery); //MRN should be unique so should find one r none
				}

				if (departureHeader != null)
				{
					return (DepartureRecordFindResult.FoundByMatchingMrn, departureHeader);
				}
				else
				{
					return (DepartureRecordFindResult.NothingFound, null);
				}
			}
			else
			{
				return (DepartureRecordFindResult.Unknown, null);
			}
		}

		public void TurnArrivalIntoDepartureAndArrival()
		{
			BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			if (!IsPhase5)
			{
				EffectiveMessageStatus = GetDefaultDepartureAndArrivalMessageStatus;
			}
		}

		protected virtual ZString GetDefaultDepartureAndArrivalMessageStatus => NctsMessageStatusList.Codes.DepartureDeclarationNotSent;

		public enum DepartureRecordFindResult
		{
			Unknown,
			AlreadyCombinedDepartureAndArrival,
			FoundByMatchingMrn,
			NothingFound
		}

		[ReadOnlyMember(nameof(LocalReferenceNumberReadOnly))]
		[MaxLength(Schema.LocalReferenceNumberMaxLength)]
		[ResourceStringData("1DDB28FC-27EE-4328-9BDB-F511C4BFA4C0", Caption = "[7] Customer Reference Number", MediumCaption = "Customer Ref. Num.", ShortCaption = "CRN", MultipleKey = Phase4CaptionKey)]
		[ResourceStringData("243E529B-606C-4595-B6B4-82A27F52DA47", Caption = "Customer Reference", MediumCaption = "Customer Ref.", ShortCaption = "LRN", MultipleKey = Phase5CaptionKey)]
		public ZString LocalReferenceNumber
		{
			get
			{
				var lrn = ZString.Empty;
				if (IsPhase5)
				{
					if (MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
					{
						lrn = departureMovementHeader.BM_PaperlessInbondNum;
					}
					else if (ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
					{
						lrn = arrivalMovementHeader.BM_PaperlessInbondNum;
					}
				}
				else
				{
					lrn = this.GetSystemDefinedValue<ZString>(Schema.LocalReferenceNumber);
				}
				return lrn.IsEmpty ? base.BH_JobReference : lrn;
			}
			set
			{
				var oldValue = LocalReferenceNumber;
				CheckMaximumLength(LocalReferenceNumberInfo, value);
				if (IsPhase5)
				{
					if (MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
					{
						departureMovementHeader.BM_PaperlessInbondNum = value;
					}
					else if (ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
					{
						arrivalMovementHeader.BM_PaperlessInbondNum = value;
					}
				}
				else
				{
					this.SetSystemDefinedValue(Schema.LocalReferenceNumber, value);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateLocalReferenceNumber();
				}
				LocalReferenceNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(Schema.LocalReferenceNumber);

		public bool LocalReferenceNumberReadOnly => LocalReferenceNumberReadOnlyCore;
		protected virtual bool LocalReferenceNumberReadOnlyCore => IsDepartureAndArrivalMovement || Messages.Count > 0 || (IsDepartureMovement && !MovementReferenceNumber.IsEmpty) || IsArrivalDetailsReadOnly || IsArrivalNotificationDisabled;

		[ResourceStringData("1DDB28FC-27EE-4328-9BDB-F511C4BFA4C0", Caption = "[7] Customer Reference Number", MediumCaption = "Customer Ref. Num.", ShortCaption = "CRN", MultipleKey = Phase4CaptionKey)]
		[ResourceStringData("243E529B-606C-4595-B6B4-82A27F52DA47", Caption = "Customer Reference", MediumCaption = "Customer Ref.", ShortCaption = "LRN", MultipleKey = Phase5CaptionKey)]
		[ReadOnlyMember(nameof(LocalReferenceNumberReadOnly))]
		public ZString LocalReferenceNumberForDisplay => LocalReferenceNumber;

		[MaxLength(22)]
		[ResourceStringData("NctsHeader.BH_JobReference", Caption = "Job Number")]
		public override ZString BH_JobReference
		{
			get { return base.BH_JobReference; }
			set
			{
				base.BH_JobReference = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateLocalReferenceNumber();
				}
			}
		}

		public JobDocAddress Declarant => GetDeclarantCore();

		public virtual JobDocAddress GetDeclarantCore()
		{
			// Assumption is that when the type is D, we cannot move to DA unless the departure is released for transit (i.e. IsMakeArrivalFromDepartureAllowed), so we cannot be DA without having already sent the IE15.
			return (IsArrivalMovement || IsDepartureAndArrivalMovement) ? DestinationTrader : Principal;
		}

		public ZString DeclarantId
		{
			get { return GetTraderIdentification(Declarant); }
		}

		internal ZString GetTraderIdentification(JobDocAddress trader)
		{
			var traderId = ZString.Empty;
			if (trader != null && trader.Organisation != null)
			{
				traderId = trader.Address.GetEuIdentificationNumber();
			}
			return traderId;
		}

		[ReadOnlyMember(nameof(BH_RL_NKImportLoadPortReadOnly))]
		[MaxLength(nameof(BH_RL_NKImportLoadPortMaxLength))]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.CountryOfDispatchList))]
		[ResourceStringData("08F81B8A-4821-46AE-BCC7-021FF0724623", Caption = "[15A] Country of Dispatch", ShortCaption = "Disp.", FullDescription = "Country of Dispatch")]
		public override ZString BH_RL_NKImportLoadPort
		{
			get { return base.BH_RL_NKImportLoadPort; }
			set
			{
				bool hasChanged = base.BH_RL_NKImportLoadPort != value;
				base.BH_RL_NKImportLoadPort = value;
				if (hasChanged && !IsCopying && MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					movementHeader.GoodsItems.ForEach(x => x.BY_RN_NKCountryOfDispatchInfo.RefreshBinding());

					CreateTransitCustomsOfficeDependingOnImportLoadPort(movementHeader, value);

					AddValueToTransitCustomsOfficeDependingOnDestinationPortCountry(movementHeader, value);
					var customsOfficesForDeparture = IsPhase5 ? movementHeader.CustomsOfficesForDeparture : CustomsOfficesForDeparture;
					customsOfficesForDeparture.Reload(false);

					if (!IsMarkingAsNeedingValidationSuspended)
					{
						movementHeader.MarkAsNeedingValidation();
						movementHeader.GoodsItems.MarkAsNeedingValidation();
					}
				}
			}
		}

		void CreateTransitCustomsOfficeDependingOnImportLoadPort(NctsDepartureMovementHeader movementHeader, ZString importLoadPort)
		{
			var country = importLoadPort.Left(2).ToUpperInvariant();
			if (!country.IsEmpty && !Factory.IsMemberOfEU(country) && Factory.IsCountryEuOrCtCountry(country))
			{
				var isPhase5 = IsPhase5;
				var customsOffices = isPhase5 ? movementHeader.CustomsOffices : CustomsOffices;
				var customsOfficeRequirementHelper = isPhase5 ? movementHeader.CustomsOfficeRequirementHelper : CustomsOfficeRequirementHelper;
				if (customsOfficeRequirementHelper.GetOtherRequirementByRole(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit) != null
					&& !customsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Any())
				{
					customsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				}
			}
		}

		public void AddValueToTransitCustomsOfficeDependingOnDestinationPortCountry(NctsDepartureMovementHeader movementHeader, ZString countryCodeThatAllowedToAddValueInTransitCustomsOffice)
		{
			if (movementHeader.BM_RL_NKDestinationPort.EqualsIgnoringCase(countryCodeThatAllowedToAddValueInTransitCustomsOffice.Left(2)))
			{
				var customsOffices = IsPhase5 ? movementHeader.CustomsOffices : CustomsOffices;
				if (customsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit) is NctsEuOfficeCode traOffice
						&& customsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination) is NctsEuOfficeCode desOffice)
				{
					traOffice.CY_Data = desOffice.CY_Data;
				}
			}
		}

		public int BH_RL_NKImportLoadPortMaxLength => (Configuration != null && Configuration.FullLoadPortSupport) ? 5 : 2;

		[ReadOnlyMember(nameof(BH_RL_NKImportLoadPortReadOnly))]
		[MaxLength(nameof(BH_RL_NKImportLoadPortMaxLength))]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.PortOfDispatchList))]
		[ResourceStringData("E7AB17DF-6B15-49D1-8541-10D97DA760DA", Caption = "[15A] Port of Dispatch", ShortCaption = "[15A] Disp.", FullDescription = "Port of Dispatch", MultipleKey = Phase4CaptionKey)]//shi gu ding de ma?
		[ResourceStringData("3DD4EE6E-8ED0-4A66-995A-70C515C78967", Caption = "Port of Dispatch", ShortCaption = "Port Disp.", MultipleKey = Phase5CaptionKey)]
		public ZString PortOfDispatch
		{
			get { return base.BH_RL_NKImportLoadPort; }
			set
			{
				bool hasChanged = base.BH_RL_NKImportLoadPort != value;
				base.BH_RL_NKImportLoadPort = value;
				if (hasChanged && !IsCopying && MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					CreateTransitCustomsOfficeDependingOnImportLoadPort(movementHeader, BH_RL_NKImportLoadPort);

					AddValueToTransitCustomsOfficeDependingOnDestinationPortCountry(movementHeader, value);
					var customsOfficesForDeparture = IsPhase5 ? movementHeader.CustomsOfficesForDeparture : CustomsOfficesForDeparture;
					customsOfficesForDeparture.Reload(false);

					movementHeader.GoodsItems.ForEach(x => x.BY_RN_NKCountryOfDispatchInfo.RefreshBinding());

					if (!IsMarkingAsNeedingValidationSuspended)
					{
						movementHeader.MarkAsNeedingValidation();
						movementHeader.GoodsItems.MarkAsNeedingValidation();
					}
				}
			}
		}
		public ZPropertyInfo PortOfDispatchInfo => GetWrappedZPropertyInfo(nameof(PortOfDispatch), x => BH_RL_NKImportLoadPortInfo);

		protected bool BH_RL_NKImportLoadPortReadOnly => BH_RL_NKImportLoadPort.IsEmpty && IsDepartureMovement && (MovementHeader?.HasGoodsItemsWithCountryOfDispatch ?? false);

		[ResourceStringData("NctsHeader.DeclarationPlace", Caption = "Declaration Place", ShortCaption = "Dec. Place")]
		public ZString DeclarationPlace
		{
			get { return Branch.GB_City; }
		}

		public ZString PlaceOfUnloadingCode
		{
			get { return (IsDepartureMovement ? MovementHeader?.BM_PlaceOfUnloading : ArrivalMovementHeader?.BM_PlaceOfUnloading) ?? ZString.Empty; }
			set
			{
				if (BH_HeaderType.IsEmpty)
				{
					throw new InvalidOperationException(FormattableString.Invariant($"Attempt to change the value of {nameof(PlaceOfUnloadingCode)} when {nameof(BH_HeaderType)} is empty"));
				}

				var oldValue = PlaceOfUnloadingCode;
				if (IsDepartureMovement)
				{
					MovementHeader.BM_PlaceOfUnloading = value;
				}
				else
				{
					ArrivalMovementHeader.BM_PlaceOfUnloading = value;
				}
				PlaceOfUnloadingCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PlaceOfUnloadingCodeInfo
		{
			get
			{
				if (IsDepartureMovement)
				{
					return GetWrappedZPropertyInfo(nameof(PlaceOfUnloadingCode), x => MovementHeader?.BM_PlaceOfUnloadingInfo);
				}
				else
				{
					return GetWrappedZPropertyInfo(nameof(PlaceOfUnloadingCode), x => ArrivalMovementHeader?.BM_PlaceOfUnloadingInfo);
				}
			}
		}

		[ResourceStringData("NctsHeader.PlaceOfUnloading", Caption = "Unloading", FullDescription = "Place of Unloading")]
		public virtual ZString PlaceOfUnloading
		{
			get
			{
				var unloco = new RefUNLOCO.Loader(Factory).Load(PlaceOfUnloadingCode);
				return unloco == null ? ZString.Empty : unloco.Description.Left(35);
			}
		}

		[ResourceStringData("NctsHeader.Principal", Caption = "Principal", FullDescription = "Principal Name & Address")]
		public JobDocAddress Principal
		{
			get
			{
				if (principalJobDocAddress == null || principalJobDocAddress.IsDeleted)
				{
					if (principalJobDocAddress != null)
					{
						principalJobDocAddress.DocAddressChanged -= new EventHandler(PrincipalJobDocAddressChanged);
					}
					principalJobDocAddress = DocAddresses.FindOrCreateWithRequirement(PrincipalJobDocAddressRequirement);
					principalJobDocAddress.DocAddressChanged += new EventHandler(PrincipalJobDocAddressChanged);
				}
				principalJobDocAddress.SetReadOnlyIncludingChildren(IsPrincipalReadOnly);
				return principalJobDocAddress;
			}
		}

		JobDocAddress principalJobDocAddress;

		public ZPropertyInfo PrincipalInfo => GetWrappedZPropertyInfo(nameof(Principal), x => Principal.E2_OA_AddressInfo);

		protected virtual ZBool IsPrincipalReadOnly => false;

		void PrincipalJobDocAddressChanged(object sender, EventArgs e)
		{
			GuaranteeRefresher.PopulateGuaranteeWithFallbacks();

			if (IsPhase5 && MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				movementHeader.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, movementHeader.IsSimplifiedNctsProcedure);
				movementHeader.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.SpecialSeals, movementHeader.IsSimplifiedNctsProcedure);
				movementHeader.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, movementHeader.BM_ReducedDatasetIndicator);
			}

			MarkAsNeedingValidation();
		}

		[ResourceStringData("F8B3CCDC-8058-47CE-BB4B-04339D25D2BC", Caption = "Contact")]
		[MaxLength(70)]
		public ZString ContactFullName
		{
			get => LocationContact.CP_FullName;
			set => LocationContact.CP_FullName = value;
		}

		public ZPropertyInfo ContactFullNameInfo => GetWrappedZPropertyInfo(nameof(ContactFullName), x => LocationContact.CP_FullNameInfo);

		[ResourceStringData("C356D5AE-CF03-4A0C-8CC9-551B41F283C1", Caption = "Telephone", MediumCaption = "Phone", ShortCaption = "TEL")]
		[MaxLength(35)]
		public ZString ContactPhone
		{
			get => LocationContact.CP_Phone;
			set => LocationContact.CP_Phone = value;
		}

		public ZPropertyInfo ContactPhoneInfo => GetWrappedZPropertyInfo(nameof(ContactPhone), x => LocationContact.CP_PhoneInfo);

		[ResourceStringData("627B1EEC-E8E7-4B8E-B023-F1B252F0003C", Caption = "Email", ShortCaption = "EML")]
		[MaxLength(256)]
		public ZString ContactEmail
		{
			get => LocationContact.CP_Email;
			set => LocationContact.CP_Email = value;
		}

		public ZPropertyInfo ContactEmailInfo => GetWrappedZPropertyInfo(nameof(ContactEmail), x => LocationContact.CP_EmailInfo);

		[ChildEditable]
		internal CusInBondPerson LocationContact
		{
			get
			{
				if (locationContact == null || locationContact.IsDeleted)
				{
					locationContact = Customs.Business.CusInBondPerson.LoadOrCreate<CusInBondPerson>(this, CusInBondPerson.LocationContactType);
					RegisterEditableChildObject(locationContact);
				}
				return locationContact;
			}
		}
		CusInBondPerson locationContact;

		public NctsGuaranteeRefresher GuaranteeRefresher => guaranteeRefresher ?? (guaranteeRefresher = GetNewGuaranteeRefresher());
		NctsGuaranteeRefresher guaranteeRefresher;

		protected virtual NctsGuaranteeRefresher GetNewGuaranteeRefresher() => new NctsGuaranteeRefresher(this);

		[ResourceStringData("NctsHeader.Consignor", Caption = "Consignor", FullDescription = "Consignor Name & Address")]
		public JobDocAddress Consignor
		{
			get
			{
				if (consignorJobDocAddress == null || consignorJobDocAddress.IsDeleted)
				{
					if (consignorJobDocAddress != null)
					{
						consignorJobDocAddress.DocAddressChanged -= ConsignorDocumentaryAddressChanged;
					}
					consignorJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsignorJobDocAddressRequirement);
					consignorJobDocAddress.DocAddressChanged += ConsignorDocumentaryAddressChanged;
				}

				return consignorJobDocAddress;
			}
		}
		JobDocAddress consignorJobDocAddress;

		void ConsignorDocumentaryAddressChanged(object sender, EventArgs e)
		{
			DefaultCountryOfDispatchIfApplicable();
			DefaultIsSimplifiedNctsProcedureIfApplicable();
			DefaultDepartureCustomsOfficeIfApplicable();

			OnChangedConsignorDocumentaryAddress();
		}

		void DefaultCountryOfDispatchIfApplicable()
		{
			var fullPort = Configuration?.FullLoadPortSupport ?? false;
			if (fullPort)
			{
				var relatedPortCode = Consignor.Address?.OA_RL_NKRelatedPortCode ?? ZString.Empty;
				if (!relatedPortCode.IsEmpty && BH_RL_NKImportLoadPort != relatedPortCode && !BH_RL_NKImportLoadPortReadOnly)
				{
					BH_RL_NKImportLoadPort = relatedPortCode;
				}
			}
			else
			{
				var consignorCountryCode = Consignor?.E2_RN_NKCountryCode ?? ZString.Empty;
				if (!consignorCountryCode.IsEmpty && BH_RL_NKImportLoadPort != consignorCountryCode && !BH_RL_NKImportLoadPortReadOnly)
				{
					BH_RL_NKImportLoadPort = consignorCountryCode;
				}
			}
		}

		void DefaultIsSimplifiedNctsProcedureIfApplicable()
		{
			if (MovementHeader != null && Consignor != null && Consignor.Address != null && Consignor.Organisation != null)
			{
				var authorisations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(Factory, CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit }, ZDate.Today, new[] { Consignor.Organisation.PK }, new[] { Consignor.Address.PK });
				if (authorisations != null && authorisations.Any())
				{
					MovementHeader.IsSimplifiedNctsProcedure = true;
				}
			}
		}

		protected virtual void DefaultDepartureCustomsOfficeIfApplicable()
		{
			var officeOfDeparture = NctsEuOfficeCode.Load<NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			if (officeOfDeparture != null)
			{
				var customsRegsNoForCTR = Consignor?.Organisation?.CustomsCodes?.GetCustomsRegNo(CodeTypes.CustomsOfficeForTransit, GlbCompany.CurrentCompany.Country.Code, Consignor.E2_OA_Address) ?? ZString.Empty;
				if (!customsRegsNoForCTR.IsEmpty)
				{
					officeOfDeparture.CY_Data = customsRegsNoForCTR;
				}
			}
		}

		protected virtual void OnChangedConsignorDocumentaryAddress() { }

		protected virtual void OnChangedDestinationTraderDocumentaryAddress() { }

		[ResourceStringData("NctsHeader.Consignee", Caption = "Consignee", FullDescription = "Consignee Name & Address")]
		public JobDocAddress Consignee
		{
			get
			{
				if (consigneeJobDocAddress == null || consigneeJobDocAddress.IsDeleted)
				{
					if (consigneeJobDocAddress != null)
					{
						consigneeJobDocAddress.DocAddressChanged -= ConsigneeDocumentaryAddressChanged;
					}
					consigneeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeJobDocAddressRequirement);
					consigneeJobDocAddress.DocAddressChanged += ConsigneeDocumentaryAddressChanged;
					consigneeJobDocAddress.AdditionalValidation = GetConsigneeJobDocAddressAdditionalValidation(consigneeJobDocAddress);
				}
				consigneeJobDocAddress.SetReadOnlyIncludingChildren(IsConsigneeReadOnly);
				return consigneeJobDocAddress;
			}
		}
		JobDocAddress consigneeJobDocAddress;

		protected virtual ZBool IsConsigneeReadOnly => false;

		protected virtual ZValidation GetConsigneeJobDocAddressAdditionalValidation(JobDocAddress consigneeJobDocAddress) => new NctsHeaderConsigneeJobDocAddressValidation(consigneeJobDocAddress, this);

		void ConsigneeDocumentaryAddressChanged(object sender, EventArgs e)
		{
			DefaultCountryOfDestinationIfApplicable();
			DefaultDestinationCustomsOfficeIfApplicable();

			if (this.IsPhase5Departure)
			{
				this.Bills.ForEach(x => x.Consignee.Validation.ValidateOrganisationPK());
			}

			OnChangedConsigneeDocumentaryAddress();
		}

		void DefaultCountryOfDestinationIfApplicable()
		{
			var consigneeCountryCode = Consignee?.Country?.RN_Code ?? ZString.Empty;
			if (IsDepartureMovement && !consigneeCountryCode.IsEmpty && MovementHeader.BM_RL_NKDestinationPort != consigneeCountryCode && !MovementHeader.BM_RL_NKDestinationPortReadOnly)
			{
				MovementHeader.BM_RL_NKDestinationPort = consigneeCountryCode;
			}
		}

		void DefaultDestinationCustomsOfficeIfApplicable()
		{
			var officeOfDestination = NctsEuOfficeCode.Load<NctsEuOfficeCode>(IsPhase5 ? CommonMovementHeader : this, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			if (officeOfDestination != null)
			{
				var customsRegsNoForCTR = Consignee?.Organisation?.CustomsCodes?.GetCustomsRegNo(CodeTypes.CustomsOfficeForTransit, GlbCompany.CurrentCompany.Country.Code, Consignee.E2_OA_Address) ?? ZString.Empty;
				if (!customsRegsNoForCTR.IsEmpty)
				{
					officeOfDestination.CY_Data = customsRegsNoForCTR;
				}
			}
		}

		protected virtual void OnChangedConsigneeDocumentaryAddress() { }

		/// <summary>
		/// The Destination Trader is the person taking responsibility for
		/// carrying out CT formalities at the Office of Destination. E.g. the
		/// Principal, the Carrier or the Recipient
		/// </summary>
		public JobDocAddress DestinationTrader
		{
			get
			{
				if (destinationTrader == null || destinationTrader.IsDeleted)
				{
					if (destinationTrader != null)
					{
						destinationTrader.DocAddressChanged -= UpdateDestinationOffice;
						destinationTrader.DocAddressChanged -= UpdateAuthorizationData;
					}
					destinationTrader = DocAddresses.FindOrCreateWithRequirement(DestinationTraderJobDocAddressRequirement);
					destinationTrader.DefaultContactAllocationType = OrgConstants.ContactAllocationType.CUS;
					destinationTrader.DocAddressChanged += UpdateDestinationOffice;
					destinationTrader.DocAddressChanged += UpdateAuthorizationData;
				}
				destinationTrader.SetReadOnlyIncludingChildren(DestinationTraderReadOnlyCore);
				return destinationTrader;
			}
		}

		protected virtual bool DestinationTraderReadOnlyCore => IsArrivalDetailsReadOnly || IsArrivalNotificationDisabled;

		protected virtual void UpdateDestinationOffice(object sender, EventArgs e)
		{
			if (IsPhase5Arrival)
			{
				OnChangedDestinationTraderDocumentaryAddress();
				var customsCodes = DestinationTrader.Organisation?.CustomsCodes;
				var result = customsCodes?.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsOfficeForTransit, ZString.Empty, DestinationTrader.E2_OA_Address) ?? ZString.Empty;
				if (!result.IsEmpty)
				{
					ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = result;
					return;
				}

				result = customsCodes?.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.CustomsOfficeForTransit, ZString.Empty, ZGuid.Empty) ?? ZString.Empty;
				if (!result.IsEmpty)
				{
					ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = result;
				}
			}
		}

		void UpdateAuthorizationData(object sender, EventArgs e)
		{
			if (IsPhase5Arrival)
			{
				var arrivalMoveHeader = ArrivalMovementHeader;
				if (arrivalMoveHeader.ShouldSyncDestinationTraderWithAuthorization && (!arrivalMoveHeader.AuthorizationCode.IsEmpty || !arrivalMoveHeader.AuthorizationNumber.IsEmpty))
				{
					arrivalMoveHeader.AuthorizationOwner = DestinationTrader.OrganisationPK;
				}
			}
		}

		public override ZGuid BH_ParentID
		{
			get => base.BH_ParentID;
			set
			{
				var oldValue = BH_ParentID;
				if (!IsCopying && oldValue != value)
				{
					base.BH_ParentID = value;
					var movementHeader = MovementHeader;
					if (IsPhase5)
					{
						if (movementHeader != null)
						{
							movementHeader.CustomsOffices.MarkAsNeedingValidation();
						}
						else
						{
							ArrivalMovementHeader?.CustomsOffices?.MarkAsNeedingValidation();
						}
					}
					else
					{
						CustomsOffices.MarkAsNeedingValidation();
					}
					movementHeader?.GoodsItems.MarkAsNeedingValidation();
					if (IsPluggedIn)
					{
						if (movementHeader != null)
						{
							Principal.MakePersistentEvenIfEmpty();
							Consignor.MakePersistentEvenIfEmpty();
							Consignee.MakePersistentEvenIfEmpty();
						}
						if (IsArrivalMovement)
						{
							DestinationTrader.MakePersistentEvenIfEmpty();
						}
					}
				}
			}
		}

		JobDocAddress destinationTrader;

		void ValidateDestinationTrader(JobDocAddressValidation validation)
		{
			if ((IsArrivalMovement && !IsArrivalDetailsReadOnly) || IsDepartureAndArrivalMovement)
			{
				CheckMandatoryMRN(DestinationTrader.OrganisationPKInfo);
				this.CheckMandatoryArrivalDestinationTrader(DestinationTrader.OrganisationPKInfo);
				this.CheckMandatoryArrivalOffice(DestinationTrader.OrganisationPKInfo);
				ValidateDestinationTraderForSpecificCountry();
			}
		}

		void CheckMandatoryMRN(ZPropertyInfo info)
		{
			if (MovementReferenceNumber.IsEmpty)
			{
				if (IsPhase5)
				{
					if (Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration && configuration.IsRuleTR0073Active)
					{
						info.AddMessageError(configuration.Messages.TR0073Message);
					}
				}
				else
				{
					info.AddMessageError(ValidationRuleMessages.TR0073MessageText);
				}
			}
		}

		void ValidateContact(JobDocAddressValidation validation)
		{
			ValidateContactForSpecificCountry();
		}

		protected virtual ZBool CanOverrideDestinationTraderAddress => true;

		protected virtual void ValidateContactForSpecificCountry()
		{
		}

		protected virtual void ValidateDestinationTraderForSpecificCountry()
		{
			if (!IsPhase5)
			{
				this.CheckConditionC112(DestinationTrader.OrganisationPKInfo, DestinationTrader, Res.GetString("b9b4ba41-f547-43b8-9aab-c4bfb8a7c9e9", "simplified arrival or Normal arrival"));
			}
		}

		[ResourceStringData("42C8E647-512B-B57C-17AD-8378178D0F41", Caption = "Safety and Security:", MultipleKey = Phase4CaptionKey)]
		[ResourceStringData("94809F28-B4DA-4AF0-8340-0DB730B1A211", Caption = "Safety and Security", MediumCaption = "Safety/Security", ShortCaption = "Safety", MultipleKey = Phase5CaptionKey)]
		public override ZBool BH_FTZMove
		{
			get => base.BH_FTZMove;
			set
			{
				var oldValue = BH_FTZMove;
				if (oldValue != value)
				{
					base.BH_FTZMove = value;
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						MovementHeader?.MarkAsNeedingValidation();
						var customsOffices = IsPhase5 ? CommonMovementHeader.CustomsOffices : CustomsOffices;
						customsOffices.MarkAsNeedingValidation();
					}
					if (!value && ShouldWipeRelatedFields)
					{
						WipeRelatedFields();
					}
				}
			}
		}
		ZBool ShouldWipeRelatedFields => ShouldWipeRelatedFieldsCore;

		protected virtual ZBool ShouldWipeRelatedFieldsCore => ZBool.True;

		void WipeRelatedFields()
		{
			MovementHeader?.WipeAdditionalText();
			MovementHeader?.GoodsItems.ForEach(g => g.WipeCommercialReferenceNumber());
			WipeSecurityTraders();
		}

		void WipeSecurityTraders()
		{
			SecurityConsignee.OrganisationPK = ZGuid.Empty;
			SecurityConsignor.OrganisationPK = ZGuid.Empty;
		}

		public event EventHandler OnIsSafetyAndSecurityChanged
		{
			add
			{
				BH_FTZMoveInfo.ValueChanged += value;
			}
			remove
			{
				BH_FTZMoveInfo.ValueChanged -= value;
			}
		}

		#region Customs Offices - Departure, Transit, Destination, Enquiry

		public ICustomsOffice DepartureCustomsOffice => GetDepartureCustomsOffice();

		protected virtual ICustomsOffice GetDepartureCustomsOffice() => NctsEuOfficeCode.Load<NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		[ResourceStringData("NctsHeader.DepartureCustomsOfficeCode", Caption = "Departure Office", FullDescription = "Customs Office of Departure")]
		public ZString DepartureCustomsOfficeCode => DepartureCustomsOffice?.OfficeCode ?? ZString.Empty;

		public ZPropertyInfo DepartureCustomsOfficeCodeInfo => GetZPropertyInfo(Schema.DepartureCustomsOfficeCode);

		[ResourceStringData("NctsHeader.DepartureCustomsOfficeCodeCountry", Caption = "Departure Office Country", FullDescription = "Country of Customs Office of Departure")]
		public ZString DepartureCustomsOfficeCodeCountry => DepartureCustomsOfficeCode.Left(2);

		[MaxLength(8)]
		public List<ICustomsOffice> TransitCustomsOfficeCodeList => IsPhase5 ? null : GetTransitCustomsOfficeCollection().ToList();

		protected virtual IEnumerable<ICustomsOffice> GetTransitCustomsOfficeCollection()
		{
			return CustomsOffices
				.Cast<NctsEuOfficeCode>()
				.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		}

		public bool HasExitForTransitOffice(ZString exitForTransitOfficeCountry) => exitForTransitOfficeCountry.IsEmpty
			? ExitForTransitCustomsOfficeCodeList.Count > 0
			: ExitForTransitCustomsOfficeCodeList.Any(x => x.OfficeCode.SubstringSafe(0, 2).EqualsIgnoringCase(exitForTransitOfficeCountry));

		public bool HasTransitOffice(string transitOfficeCountry = null) => string.IsNullOrEmpty(transitOfficeCountry)
			? TransitCustomsOfficeCodeList.Count > 0
			: TransitCustomsOfficeCodeList.Any(x => x.OfficeCode.SubstringSafe(0, 2).EqualsIgnoringCase(transitOfficeCountry));

		public List<ICustomsOffice> ExitForTransitCustomsOfficeCodeList => GetExitForTransitCustomsOfficeCollection().ToList();

		protected virtual IEnumerable<ICustomsOffice> GetExitForTransitCustomsOfficeCollection()
		{
			return CustomsOffices
				.Cast<NctsEuOfficeCode>()
				.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
		}

		public ICustomsOffice DestinationCustomsOffice => GetDestinationCustomsOffice();

		protected virtual ICustomsOffice GetDestinationCustomsOffice() => IsArrivalMovement ? DestinationCustomsOfficeForArrival : DestinationCustomsOfficeForDeparture;

		public ZString DestinationCustomsOfficeCode => IsArrivalMovement ? DestinationCustomsOfficeCodeForArrival : DestinationCustomsOfficeCodeForDeparture;

		public ICustomsOffice DestinationCustomsOfficeForDeparture => GetDestinationCustomsOfficeForDeparture();

		protected virtual ICustomsOffice GetDestinationCustomsOfficeForDeparture() => NctsEuOfficeCode.Load<NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

		[ResourceStringData("NctsHeader.DestinationCustomsOfficeCodeForDeparture", Caption = "Actual Office of Destination", MediumCaption = "Destination Office", ShortCaption = "Dest. Office", FullDescription = "Customs Office of Destination")]
		[ReadOnlyMember(nameof(DestinationCustomsOfficeCodeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.DestinationCustomsOfficeCodeList))]
		[MaxLength(10)]
		public ZString DestinationCustomsOfficeCodeForDeparture
		{
			get => DestinationCustomsOfficeForDeparture?.OfficeCode ?? ZString.Empty;
			set
			{
				var oldValue = DestinationCustomsOfficeCodeForDeparture;
				SetOfficeData(DestinationCustomsOfficeForDeparture ?? AddNewCustomsOfficeCodeForDeparture(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination), value);
				DestinationCustomsOfficeCodeForDepartureInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDestinationCustomsOfficeCodeForDeparture();
				}
			}
		}

		public ICustomsOffice DestinationCustomsOfficeForArrival => GetDestinationCustomsOfficeForArrival();

		protected virtual ICustomsOffice GetDestinationCustomsOfficeForArrival()
		{
			return IsPhase5 ? null : CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
		}

		[ResourceStringData("NctsHeader.DestinationCustomsOfficeCodeForArrival", Caption = "Actual Office of Destination for Arrival", MediumCaption = "Destination Office for Arrival", ShortCaption = "Dest. Office (Arrival)", FullDescription = "Customs Office of Destination for Arrival")]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.DestinationCustomsOfficeCodeList))]
		[MaxLength(10)]
		[ReadOnlyMember(nameof(DestinationCustomsOfficeCodeForArrivalReadOnly))]
		public virtual ZString DestinationCustomsOfficeCodeForArrival
		{
			get => DestinationCustomsOfficeForArrival?.OfficeCode ?? ZString.Empty;
			set
			{
				var oldValue = DestinationCustomsOfficeCodeForArrival;
				SetOfficeData(DestinationCustomsOfficeForArrival ?? AddNewCustomsOfficeCodeForDeparture(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival), value);
				DestinationCustomsOfficeCodeForArrivalInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDestinationCustomsOfficeCodeForArrival();
				}
			}
		}

		public bool DestinationCustomsOfficeCodeForArrivalReadOnly => DestinationCustomsOfficeCodeForArrivalReadOnlyCore;

		protected virtual bool DestinationCustomsOfficeCodeForArrivalReadOnlyCore => IsArrivalNotificationDisabled || IsArrivalDetailsReadOnly;

		protected ICustomsOffice AddNewCustomsOfficeCodeForDeparture(string officeCode)
		{
			if (!IsPhase5)
			{
				var office = CustomsOffices.AddNew();
				office.CY_Code = officeCode;
				CustomsOfficesForDeparture.Reload(false);
				return office;
			}
			else
			{
				return null;
			}
		}

		protected void SetOfficeData(ICustomsOffice customsOffice, ZString value)
		{
			if (customsOffice is CusCodeData customsOfficeData)
			{
				customsOfficeData.CY_Data = value;
			}
		}

		public ZPropertyInfo DestinationCustomsOfficeCodeInfo => GetZPropertyInfo(Schema.DestinationCustomsOfficeCode);

		public ZPropertyInfo DestinationCustomsOfficeCodeForDepartureInfo => GetZPropertyInfo(Schema.DestinationCustomsOfficeCodeForDeparture);

		public ZPropertyInfo DestinationCustomsOfficeCodeForArrivalInfo => GetZPropertyInfo(Schema.DestinationCustomsOfficeCodeForArrival);

		[ResourceStringData("NctsHeader.DestinationCustomsOfficeCodeCountry", Caption = "Actual Office of Destination", MediumCaption = "Destination Office", ShortCaption = "Dest. Office", FullDescription = "Country of Customs Office of Destination")]
		public ZString DestinationCustomsOfficeCodeCountry => DestinationCustomsOfficeCode.Left(2);

		[ResourceStringData("NctsHeader.DestinationCustomsOfficeCodeCountryForDeparture", Caption = "Actual Office of Destination", MediumCaption = "Destination Office", ShortCaption = "Dest. Office", FullDescription = "Country of Customs Office of Destination")]
		public ZString DestinationCustomsOfficeCodeCountryForDeparture => DestinationCustomsOfficeCodeForDeparture.Left(2);

		public ZString DestinationCustomsOfficeCodeCountryForArrival => DestinationCustomsOfficeCodeForArrival.Left(2);

		public bool IsNctsCountryImplementedNatively(ZString country)
		{
			return GetNativelyImplementedNctsCountries().Contains(country);
		}

		static List<string> GetNativelyImplementedNctsCountries()
		{
			if (implementedNctsCountries == null)
			{
				implementedNctsCountries = new List<string>();
				implementedNctsCountries.Add(Core.Constants.CountryCodes.UnitedKingdom);
				implementedNctsCountries.Add(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes);
				implementedNctsCountries.Add(Core.Constants.CountryCodes.France);
			}
			return implementedNctsCountries;
		}
		[ThreadStatic]
		static List<string> implementedNctsCountries;

		public bool IsDepartureDestinationOfficeInNctsContractingCountry
		{
			get { return NctsHeaderValidationHelper.IsNctsContractingParty(DestinationCustomsOfficeCodeCountry); }
		}

		public bool IsTargetNctsSystemUsingNativeComms
		{
			get { return IsNctsCountryImplementedNatively(TargetNctsSystemCountryCode); }
		}

		public bool IsUniversalXmlSupportedByTargetNctsCountry
		{
			get { return NctsHeaderValidationHelper.IsNctsContractingParty(TargetNctsSystemCountryCode); }
		}

		public ZString TargetNctsSystemCountryCode
		{
			get
			{
				var countryCodeFromOfficeCode = IsPhase5
					? IsDepartureMovement ? MovementHeader.DepartureCustomsOfficeCodeCountry : ArrivalMovementHeader.DestinationCustomsOfficeCodeCountryForArrival
					: IsDepartureMovement ? DepartureCustomsOfficeCodeCountry : DestinationCustomsOfficeCodeCountryForArrival;
				return countryCodeFromOfficeCode == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes ? Core.Constants.CountryCodes.UnitedKingdom : countryCodeFromOfficeCode.ToString();
			}
		}

		public ZString MessageFunctionCode
		{
			get; set;
		}

		protected virtual bool DestinationCustomsOfficeCodeReadOnly => !IsArrivalMovement;

		public NctsEuOfficeCode EnquiryCustomsOffice => NctsEuOfficeCode.Load<NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

		[ResourceStringData("NctsHeader.EnquiryCustomsOfficeCode", Caption = "Enquiry Office", FullDescription = "Customs Office of Enquiry")]
		public ZString EnquiryCustomsOfficeCode => EnquiryCustomsOffice?.CY_Data ?? ZString.Empty;

		public ZPropertyInfo EnquiryCustomsOfficeCodeInfo => GetZPropertyInfo(Schema.EnquiryCustomsOfficeCode);

		[ResourceStringData("NctsHeader.EnquiryCustomsOfficeCodeCountry", Caption = "Enquiry Office Country", FullDescription = "Country of Customs Office of Enquiry")]
		public ZString EnquiryCustomsOfficeCodeCountry
		{
			get
			{
				var enquiryCustomsOfficeCode = EnquiryCustomsOfficeCode;
				return enquiryCustomsOfficeCode.IsEmpty ? ZString.Empty : enquiryCustomsOfficeCode.Left(2);
			}
		}

		#endregion

		public bool HasMultipleContainerisedContainers => Factory.GetValue(ref hasMultipleContainerisedContainersCached,
			() =>
			{
				var containers = IsPhase5Arrival
					? ArrivalHeaderContainers
					: (BusinessObjectCollection)DepartureHeaderContainers;
				return containers.Cast<NctsCusInBondContainer>().Where(x =>
						x.BC_Mode.EqualsIgnoringCase(Enterprise.Core.Constants.ContainerModes.Containerised)).Take(2)
					.Count() > 1;
			});
		CachedProperty<bool> hasMultipleContainerisedContainersCached;

		/// <summary>
		/// Active business object collection - used only for synching
		/// </summary>
		public ICusInBondContainerCollection ContainersAsICusInBondContainerCollectionForSynching
		{
			get { return new NctsDepartureHeaderContainersActiveCollection(this); }
		}

		[ChildEditable]
		public INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> DepartureHeaderContainers
		{
			get
			{
				if (departureHeaderContainers == null)
				{
					departureHeaderContainers = GetDepartureHeaderContainersCore();
					departureHeaderContainers.Load();
					RegisterEditableChildObject(departureHeaderContainers);
				}
				return departureHeaderContainers;
			}
		}
		INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> departureHeaderContainers;

		protected virtual INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> GetDepartureHeaderContainersCore() => new NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>(this);

		public IEnumerable<IShortSequenceNumberLine> HeaderContainersLines => new TypedEnumerable<IShortSequenceNumberLine>(DepartureHeaderContainers);

		public ShortSequenceNumberGenerator HeaderContainersLineNumberGenerator => headerContainersLineNumberGenerator ?? (headerContainersLineNumberGenerator = HeaderContainersLineNumberGeneratorCore);
		protected virtual ShortSequenceNumberGenerator HeaderContainersLineNumberGeneratorCore => new ShortSequenceNumberGenerator(() => HeaderContainersLines);
		ShortSequenceNumberGenerator headerContainersLineNumberGenerator;

		public IReadOnlyList<ZString> HeaderContainersSeals => DepartureHeaderContainers.SelectMany(x => x.AllSeals).ToList();

		[ChildEditable]
		public NctsArrivalHeaderContainerCollection ArrivalHeaderContainers
		{
			get
			{
				if (arrivalHeaderContainers == null)
				{
					arrivalHeaderContainers = GetArrivalHeaderContainersCore();
					arrivalHeaderContainers.Load();
					RegisterEditableChildObject(arrivalHeaderContainers);
				}
				return arrivalHeaderContainers;
			}
		}
		NctsArrivalHeaderContainerCollection arrivalHeaderContainers;

		protected virtual NctsArrivalHeaderContainerCollection GetArrivalHeaderContainersCore()
		{
			return new NctsArrivalHeaderContainerCollection(this);
		}

		public bool AllArrivalSealStateAreDEC => AllArrivalSealStateAreDECCore;

		protected virtual bool AllArrivalSealStateAreDECCore => !ArrivalHeaderContainers.Where(x => NctsHelper.IsUnloadedStateAccepted(x.BC_UnloadedState)).Any(y => y.Seals.Cast<CusSeal>().Any(seal => seal.BK_UnloadingState != NctsUnloadedStateList.Codes.DEC));

		public void SetUpUnloadingStateOfTargetArrivalSeals()
		{
			var targetContainers = ArrivalHeaderContainers.Where(x => NctsHelper.IsUnloadedStateAccepted(x.BC_UnloadedState));
			foreach (var container in targetContainers)
			{
				container.Seals.Where(seal => seal.BK_UnloadingState == NctsUnloadedStateList.Codes.MIS || seal.BK_UnloadingState == NctsUnloadedStateList.Codes.DAM).ForEach(x => x.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC);
			}
		}

		[ChildEditable(true)]
		public INctsGuaranteeCollection<NctsGuarantee> Guarantees
		{
			get
			{
				if (guarantees == null)
				{
					guarantees = GetGuaranteesForNonPhase5Departure();
					guarantees.Load();
					guarantees.ListChanged += Guarantees_ListChanged;
					RegisterEditableChildObject(guarantees);
				}
				return guarantees;
			}
		}
		INctsGuaranteeCollection<NctsGuarantee> guarantees;

		protected virtual void Guarantees_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		protected virtual INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new NctsGuaranteeCollection<NctsGuarantee>(this);

		// TODO eventually use transport routing for itinerary
		// The specs allow 99 countries but restrict to 15 including dispatch and destination because BH_UniqueVoyageIdentifier is a varchar(30).
		[ResourceStringData("NctsHeader.Itinerary", Caption = "Itinerary", FullDescription = "Itinerary Routing")]
		[ChildEditable(true)]
		public NonPersistentItineraryCountryCollection Itinerary
		{
			get
			{
				if (itineraryCountryCollection == null)
				{
					itineraryCountryCollection = GetItineraryCore();
					RegisterEditableChildObject(itineraryCountryCollection);
				}
				return itineraryCountryCollection;
			}
		}
		NonPersistentItineraryCountryCollection itineraryCountryCollection;

		protected virtual NonPersistentItineraryCountryCollection GetItineraryCore()
		{
			return new NonPersistentItineraryCountryCollection(this);
		}

		void SaveItinerary()
		{
			if (itineraryCountryCollection != null && itineraryCountryCollection.HasChanges)
			{
				var itineraryCountries = string.Empty;
				Itinerary.Sort("Sequence");
				foreach (NonPersistentItineraryCountry itineraryCountry in Itinerary)
				{
					itineraryCountries += itineraryCountry.CountryCode;
					itineraryCountry.HasChanges = false; // Need to force this as it doesn't seem to work automatically
				}

				using (new DisposableAction(() => isSuspendPopulatingItinerary = true, () => isSuspendPopulatingItinerary = false))
				{
					BH_UniqueVoyageIdentifier = itineraryCountries;
				}
			}
		}

		public ZString ItineraryCountries
		{
			get
			{
				var itineraryCountries = string.Empty;
				if (Itinerary != null)
				{
					foreach (NonPersistentItineraryCountry itineraryCountry in Itinerary)
					{
						itineraryCountries += itineraryCountry.CountryCode + " ";
					}
				}
				return itineraryCountries.TrimEnd();
			}
		}

		public OrgAddress CarrierOrgAddress
		{
			get
			{
				if (Carrier != null)
				{
					return Carrier.MainAddress;
				}
				return null;
			}
		}

		[ResourceStringData("NctsHeader.SecurityConsignor", Caption = "Security Consignor", FullDescription = "Security Consignor Name & Address")]
		public JobDocAddress SecurityConsignor
		{
			get
			{
				if (securityConsignorJobDocAddress == null || securityConsignorJobDocAddress.IsDeleted)
				{
					securityConsignorJobDocAddress = DocAddresses.FindOrCreateWithRequirement(SecurityConsignorJobDocAddressRequirement);
				}
				return securityConsignorJobDocAddress;
			}
		}
		JobDocAddress securityConsignorJobDocAddress;

		[ResourceStringData("NctsHeader.SecurityConsignee", Caption = "Security Consignee", FullDescription = "Security Consignee Name & Address")]
		public JobDocAddress SecurityConsignee
		{
			get
			{
				if (securityConsigneeJobDocAddress == null || securityConsigneeJobDocAddress.IsDeleted)
				{
					securityConsigneeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(SecurityConsigneeJobDocAddressRequirement);
				}
				return securityConsigneeJobDocAddress;
			}
		}
		JobDocAddress securityConsigneeJobDocAddress;

		#endregion

		#region IDocAddresses Implementation

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

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (jobDocAddressDependentCollection == null)
				{
					jobDocAddressDependentCollection = GetNewDocAddresses();
					jobDocAddressDependentCollection.Load();
					RegisterEditableChildObject(jobDocAddressDependentCollection);
				}
				return jobDocAddressDependentCollection;
			}
		}

		protected virtual JobDocAddressDependentCollection GetNewDocAddresses()
		{
			return new JobDocAddressDependentCollection(this);
		}

		JobDocAddressDependentCollection jobDocAddressDependentCollection;

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Environment.Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Principal:
					return PrincipalJobDocAddressRequirement;
				case DocAddressType.ConsignorDocumentaryAddress:
					return ConsignorJobDocAddressRequirement;
				case DocAddressType.ConsigneeAddress:
					return ConsigneeJobDocAddressRequirement;
				case DocAddressType.NotifyParty2:
					return SecurityConsignorJobDocAddressRequirement;
				case DocAddressType.NotifyParty3:
					return SecurityConsigneeJobDocAddressRequirement;
				case DocAddressType.ImporterDocumentaryAddress:
					return DestinationTraderJobDocAddressRequirement;
				default:
					return GetAdditionalDocAddressRequirement(addressType);
			}
		}

		protected virtual JobDocAddressRequirement GetAdditionalDocAddressRequirement(DocAddressType addressType) => null;

		public JobDocAddressRequirement DestinationTraderJobDocAddressRequirement
		{
			get
			{
				if (destinationTraderJobDocAddressRequirement == null)
				{
					destinationTraderJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ImporterDocumentaryAddress, ContactType.Consignee);
					destinationTraderJobDocAddressRequirement.ValidateOrganisationPK = ValidateDestinationTrader;
					destinationTraderJobDocAddressRequirement.ValidateContact += ValidateContact;
					destinationTraderJobDocAddressRequirement.CanOverride = CanOverrideDestinationTraderAddress;
					JobDocAddressManager.AddRequirement(destinationTraderJobDocAddressRequirement);
				}
				return destinationTraderJobDocAddressRequirement;
			}
		}

		JobDocAddressRequirement destinationTraderJobDocAddressRequirement;

		public JobDocAddressRequirement PrincipalJobDocAddressRequirement
		{
			get
			{
				if (principalJobDocAddressRequirement == null)
				{
					principalJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Principal, ContactType.NoContactType);
					JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(principalJobDocAddressRequirement);
					JobDocAddressValidationHelper.ApplyRequirementForContactWorkPhoneValidation_TR0079(principalJobDocAddressRequirement);
					principalJobDocAddressRequirement.ValidateOrganisationPK = ValidatePrincipalTrader;
					principalJobDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidatePrincipalAddress;
					principalJobDocAddressRequirement.ValidateContact += ValidatePrincipalContact;
					principalJobDocAddressRequirement.CanOverride = PrincipalJobDocAddressCanOverride;
					JobDocAddressManager.AddRequirement(principalJobDocAddressRequirement);
				}
				return principalJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement principalJobDocAddressRequirement;

		protected virtual void ValidatePrincipalTrader(JobDocAddressValidation validation)
		{
			if (IsDepartureMovement)
			{
				this.CheckConditionC050(Principal.OrganisationPKInfo, Principal);
				this.CheckConditionC111(Principal.OrganisationPKInfo, Principal);
				this.CheckConditionC236(Principal.OrganisationPKInfo);
				this.CheckConditionC0904(Principal);
				this.CheckConditionC0505(Principal.OrganisationPKInfo, Principal);
				this.CheckConditionR0520(Principal.E2_OA_AddressInfo, Principal.OrganisationPKInfo);
				this.CheckConditionTR0087(Principal);
				this.CheckNoOrMultipleGuarantees(Principal.OrganisationPKInfo);
				CheckPrincipal_NR0071(Principal);
				CheckPrincipal_NR0074(Principal);
			}
		}

		protected virtual void ValidatePrincipalAddress(JobDocAddressValidation validation)
		{
			validation.ValidateOrganisationPK();
			this.CheckConditionR0520(Principal.E2_OA_AddressInfo);
			this.CheckConditionE1102(Principal.E2_OA_AddressInfo, Principal, NctsConstants.Traders.PrincipalCaption);
			new NctsTraderAddressPhase5DepartureValidator(Principal, NctsConstants.Traders.PrincipalCaption)
				.CheckRuleE1104_1(this, Principal.E2_OA_AddressInfo);
		}

		void CheckPrincipal_NR0071(JobDocAddress principal)
		{
			if (!principal.IsEmpty && ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleNR0071Active && principal.Organisation.GetEORI().IsEmpty)
			{
				var hasAdditionalReferenceWithCodeY026AtHeaderLevel = AdditionalDocuments.Any(a => a.IsAnAdditionalReference && a.CSI_Code == NctsConstants.AdditionalInfoCodes.PrincipalAEOCertificateNumber);

				if (hasAdditionalReferenceWithCodeY026AtHeaderLevel)
				{
					principal.OrganisationPKInfo.AddMessageError(Configuration.ValidationRuleConfiguration.Messages.NR0071Message);
				}
			}
		}

		void CheckPrincipal_NR0074(JobDocAddress principal)
		{
			if (!principal.IsEmpty && ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleNR0074Active && principal.Organisation.GetEORI().IsEmpty)
			{
				var hasACROrSSEAuthorisation = MovementHeader.CusAuthorizationUsages.Any(c => c.AGC_Code == CusAuthorizationHeaderTypeList.Codes.SpecialSeals || c.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);

				if (hasACROrSSEAuthorisation)
				{
					principal.OrganisationPKInfo.AddMessageError(Configuration.ValidationRuleConfiguration.Messages.NR0074Message);
				}
			}
		}

		protected virtual void ValidatePrincipalContact(JobDocAddressValidation validation)
		{
		}

		protected virtual bool PrincipalJobDocAddressCanOverride => false;

		public JobDocAddressRequirement ConsignorJobDocAddressRequirement
		{
			get
			{
				if (consignorJobDocAddressRequirement == null)
				{
					consignorJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.NoContactType);
					JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(consignorJobDocAddressRequirement);
					JobDocAddressValidationHelper.ApplyRequirementForContactWorkPhoneValidation_TR0079(consignorJobDocAddressRequirement);
					consignorJobDocAddressRequirement.ValidateOrganisationPK = ValidateConsignor;
					consignorJobDocAddressRequirement.CanOverride = ConsignorJobDocAddressCanOverride;
					consignorJobDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateConsignorAddress;
					JobDocAddressManager.AddRequirement(consignorJobDocAddressRequirement);
				}
				return consignorJobDocAddressRequirement;
			}
		}

		protected virtual void ValidateConsignor(JobDocAddressValidation validation)
		{
			var consignor = Consignor;
			var targetInfo = consignor.OrganisationPKInfo;
			var headerResString = Res.GetString("0CD29B93-273A-49EB-AF48-50738D168AE3", "Header");

			this.CheckConditionC0505(targetInfo, consignor);
			new NctsRuleC0542_1Validation(this).ValidateConsignor(targetInfo, consignor, headerResString);
			new NctsRuleG0123_1Validation(this).ValidateConsignor(targetInfo, consignor, headerResString);

			CheckConsigneeNR0068(consignor);
		}

		JobDocAddressRequirement consignorJobDocAddressRequirement;

		protected virtual void ValidateConsignorAddress(JobDocAddressValidation validation)
		{
			this.CheckConditionE1102(Consignor.E2_OA_AddressInfo, Consignor, NctsConstants.Traders.ConsignorCaption);

			new NctsTraderAddressPhase5DepartureValidator(Consignor, NctsConstants.Traders.ConsignorCaption)
				.CheckRuleE1104_1(this, Consignor.E2_OA_AddressInfo);
		}

		void CheckConsigneeNR0068(JobDocAddress consignee)
		{
			if (!consignee.IsEmpty && ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleNR0068Active && consignee.Organisation.GetEORI().IsEmpty)
			{
				var hasAdditionalReferenceWithCodeY023AtHeaderLevel = AdditionalDocuments.Any(a => a.IsAnAdditionalReference && a.CSI_Code == NctsConstants.AdditionalInfoCodes.ConsignorAEOCertificateNumber);

				if (hasAdditionalReferenceWithCodeY023AtHeaderLevel)
				{
					consignee.OrganisationPKInfo.AddMessageError(Configuration.ValidationRuleConfiguration.Messages.NR0068Message);
				}
			}
		}

		protected virtual bool ConsignorJobDocAddressCanOverride => false;

		public JobDocAddressRequirement ConsigneeJobDocAddressRequirement
		{
			get
			{
				if (consigneeJobDocAddressRequirement == null)
				{
					consigneeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.NoContactType);
					JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(consigneeJobDocAddressRequirement);
					JobDocAddressValidationHelper.ApplyRequirementForContactWorkPhoneValidation_TR0079(consigneeJobDocAddressRequirement);
					consigneeJobDocAddressRequirement.ValidateOrganisationPK = ValidateConsignee;
					consigneeJobDocAddressRequirement.CanOverride = ConsigneeJobDocAddressCanOverride;
					consigneeJobDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateConsigneeAddress;
					JobDocAddressManager.AddRequirement(consigneeJobDocAddressRequirement);
				}
				return consigneeJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement consigneeJobDocAddressRequirement;

		protected virtual bool ConsigneeJobDocAddressCanOverride => false;

		protected virtual void ValidateConsigneeAddress(JobDocAddressValidation validation)
		{
			this.CheckConditionE1102(Consignee.E2_OA_AddressInfo, Consignee, NctsConstants.Traders.ConsigneeCaption);
			new NctsTraderAddressPhase5DepartureValidator(Consignee, NctsConstants.Traders.ConsigneeCaption)
				.CheckRuleE1104_1(this, Consignee.E2_OA_AddressInfo);
		}

		protected virtual void ValidateConsignee(JobDocAddressValidation validation)
		{
			if (!IsPhase5Departure)
			{
				CheckConditionC001();
			}
			else
			{
				CheckConsignee_NR0069(Consignee);
			}
			this.CheckConditionC0505(Consignee.OrganisationPKInfo, Consignee);
		}

		void CheckConsignee_NR0069(JobDocAddress consignee)
		{
			if (!consignee.IsEmpty && ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleNR0069Active && consignee.Organisation.GetEORI().IsEmpty)
			{
				var hasAdditionalReferenceWithCodeY023AtHeaderLevel = AdditionalDocuments.Any(a => a.IsAnAdditionalReference && a.CSI_Code == NctsConstants.AdditionalInfoCodes.ConsigneeAEOCertificateNumber);

				if (hasAdditionalReferenceWithCodeY023AtHeaderLevel)
				{
					consignee.OrganisationPKInfo.AddMessageError(Configuration.ValidationRuleConfiguration.Messages.NR0069Message);
				}
			}
		}

		protected virtual void CheckConditionC001() => this.CheckConditionC001(Consignee.OrganisationPKInfo);

		public JobDocAddressRequirement SecurityConsignorJobDocAddressRequirement
		{
			get
			{
				if (securityConsignorJobDocAddressRequirement == null)
				{
					securityConsignorJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty2, ContactType.Consignor);
					securityConsignorJobDocAddressRequirement.ValidateOrganisationPK = ValidateSecurityConsignorTrader;
					securityConsignorJobDocAddressRequirement.CanOverride = SecurityConsignorJobDocAddressCanOverride;
					JobDocAddressManager.AddRequirement(securityConsignorJobDocAddressRequirement);
				}
				return securityConsignorJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement securityConsignorJobDocAddressRequirement;

		protected virtual bool SecurityConsignorJobDocAddressCanOverride => false;

		void ValidateSecurityConsignorTrader(JobDocAddressValidation validation)
		{
			ValidateSecurityConsignorC187();
			this.CheckConditionC572(SecurityConsignor.OrganisationPKInfo, SecurityConsignor);
		}

		protected virtual void ValidateSecurityConsignorC187()
		{
			this.CheckConditionC187(SecurityConsignor.OrganisationPKInfo);
		}

		public JobDocAddressRequirement SecurityConsigneeJobDocAddressRequirement
		{
			get
			{
				if (securityConsigneeJobDocAddressRequirement == null)
				{
					securityConsigneeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty3, ContactType.Consignee);
					securityConsigneeJobDocAddressRequirement.ValidateOrganisationPK = ValidateSecurityConsigneeTrader;
					securityConsigneeJobDocAddressRequirement.CanOverride = SecurityConsigneeJobDocAddressCanOverride;
					JobDocAddressManager.AddRequirement(securityConsigneeJobDocAddressRequirement);
				}
				return securityConsigneeJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement securityConsigneeJobDocAddressRequirement;

		protected virtual bool SecurityConsigneeJobDocAddressCanOverride => false;

		void ValidateSecurityConsigneeTrader(JobDocAddressValidation validation)
		{
			ValidateSecurityConsigneeC188();
		}

		protected virtual void ValidateSecurityConsigneeC188()
		{
			this.CheckConditionC188(SecurityConsignee.OrganisationPKInfo);
		}

		public JobDocAddressManager JobDocAddressManager
		{
			get { return jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager()); }
		}
		JobDocAddressManager jobDocAddressManager;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return new OrgHeaderCollection(Factory);
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return HumanReadableNameCore; }
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

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => GetJobDocAddressValidation(addressToValidate);

		protected virtual ZValidation GetJobDocAddressValidation(JobDocAddress addressToValidate) => new NctsJobDocAddressValidation(addressToValidate, this);

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new DocAddressType[] { DocAddressType.Principal, DocAddressType.ConsignorDocumentaryAddress, DocAddressType.ConsigneeAddress, DocAddressType.NotifyParty2, DocAddressType.NotifyParty3, DocAddressType.ImporterDocumentaryAddress }; }
		}

		#endregion

		#region IEuOfficeCodeProvider Members

		public ZString CountryCode => Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		public ZString CompanyCode => Company.GC_Code;

		public ZBool IsExport => false;

		public ZBool IsImport => false;

		public bool IsNCTS => true;

		public bool IsEMCS => false;

		IEnumerable<EuOfficeCode> IEuOfficeCodeProvider.CustomsOffices
		{
			get
			{
				if (IsPhase5)
				{
					ErrorReporter.ReportOnce("Access to IEuOfficeCodeProvider.CustomsOffices for Phase5", $"CustomsOffices collection on NctsHeader can only be used for Phase 4 (BH_ApplicationCode = 'NCT')'. For Phase 5 should be used customs offices collection on MovementHeader.");
					return Enumerable.Empty<NctsEuOfficeCode>();
				}

				return CustomsOffices;
			}
		}

		[BusinessObjectTestExclude]
		[ChildEditable(false)]
		public NctsEuOfficeCodeCollection CustomsOffices
		{
			get
			{
				if (BH_ApplicationCode == CusInBondApplicationCodeList.Codes.NCTS4 || BH_ApplicationCode != CusInBondApplicationCodeList.Codes.NCTS5)
				{
					if (customsOffices == null)
					{
						customsOffices = GetNewCustomsOffices();
						customsOffices.Load();
						((IBindingList)customsOffices).ListChanged += CustomsOffices_ListChanged;
						RegisterEditableChildObject(customsOffices);
					}
					else if (customsOfficesNeedingReload)
					{
						customsOffices.Reload(false);
						customsOfficesNeedingReload = false;
					}
					return customsOffices;
				}

				throw new DeveloperNotificationException(
					$"CustomsOffices collection on NctsHeader can only be used for Phase 4 (BH_ApplicationCode = 'NCT'). Current application code is '{BH_ApplicationCode}'. For Phase 5 should be used customs offices collection on MovementHeader.");
			}
		}
		NctsEuOfficeCodeCollection customsOffices;

		protected virtual NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsEuOfficeCodeCollection(this);

		void CustomsOffices_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!customsOfficesForDepartureNeedingReload && customsOfficesForDeparture != null)
			{
				var listChangedType = e.ListChangedType;
				if (listChangedType == ListChangedType.ItemAdded || listChangedType == ListChangedType.ItemDeleted || listChangedType == ListChangedType.Reset || listChangedType == ListChangedType.ItemChanged)
				{
					customsOfficesForDepartureNeedingReload = true;
					Factory.ClearCachedValue<CodeDescriptionPairList>(NctsDepartureMovementHeaderPhase5Lookups.OfficeCodeListCacheKey);
				}
			}
		}

		bool customsOfficesForDepartureNeedingReload;

		[ChildEditable(true)]
		public NctsEuOfficeCodeCollectionForDepartureGrid CustomsOfficesForDeparture
		{
			get
			{
				if (customsOfficesForDeparture == null)
				{
					customsOfficesForDeparture = GetNewCustomsOfficesForDeparture();
					customsOfficesForDeparture.Load();
					((IBindingList)customsOfficesForDeparture).ListChanged += CustomsOfficesForDeparture_ListChanged;
					RegisterEditableChildObject(customsOfficesForDeparture);
				}
				else if (customsOfficesForDepartureNeedingReload)
				{
					customsOfficesForDeparture.Reload(false);
					customsOfficesForDepartureNeedingReload = false;
				}
				return customsOfficesForDeparture;
			}
		}
		NctsEuOfficeCodeCollectionForDepartureGrid customsOfficesForDeparture;
		protected virtual NctsEuOfficeCodeCollectionForDepartureGrid GetNewCustomsOfficesForDeparture() => new NctsEuOfficeCodeCollectionForDepartureGrid(this);
		protected virtual void CustomsOfficesForDeparture_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!customsOfficesNeedingReload && customsOffices != null)
			{
				var listChangedType = e.ListChangedType;
				if (listChangedType == ListChangedType.ItemAdded || listChangedType == ListChangedType.ItemDeleted || listChangedType == ListChangedType.Reset || listChangedType == ListChangedType.ItemChanged)
				{
					customsOfficesNeedingReload = true;
				}
			}
		}

		bool customsOfficesNeedingReload;

		public CustomsOfficeRequirementHelper CustomsOfficeRequirementHelper => customsOfficeRequirementHelper ?? (customsOfficeRequirementHelper = GetCustomsOfficeRequirementHelper());
		CustomsOfficeRequirementHelper customsOfficeRequirementHelper;

		protected virtual CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new NctsHeaderCustomsOfficeRequirementHelper(this);

		#endregion

		#region ICanBeImportOrExport Members

		string ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		ZBool ICanBeImportOrExport.IsExport => false;

		ZBool ICanBeImportOrExport.IsImport => false;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string ICanBeImportOrExport.TrueCountryCode => CountryCode;
		string ICanBeImportOrExport.DataGroupingCode => CountryCode;

		#endregion

		[ResourceStringData("2CA2D5BF-46AF-4DE3-BCAA-0D82A539F1D5", Caption = "Country of Dispatch", MediumCaption = "Ctry. of Dispatch", ShortCaption = "Dispatch")]
		public ZString CountryOfDispatch => IsPhase5
			? MovementHeader?.BM_RN_NKCountryOfDispatch ?? ZString.Empty
			: BH_RL_NKImportLoadPort;

		public ZPropertyInfo CountryOfDispatchInfo => GetZPropertyInfo(Schema.CountryOfDispatch);

		public ZString ReasonForCannotRequestCancellation
		{
			get
			{
				var reason = ZString.Empty;
				if (MovementReferenceNumber.IsEmpty)
				{
					reason = Res.GetString("a239aec5-84f0-4858-b837-ef9ddc89c584", "Cannot cancel as no MRN is allocated");
				}
				// etc .... more rules here
				return reason;
			}
		}

		public ZString ExplanationToCustomsForWhyCancelling { get; set; }

		public string MakeDepartureAmendment()
		{
			var result = Res.GetString("be38cb68-a1cf-41b1-a19e-60459b2b7945", "Amendment is not allowed currently.");
			if (IsDepartureAmendmentAllowed)
			{
				var lrn = LocalReferenceNumber;
				UpdateLocalReferenceNumberForAmendment();
				var lrnHasChanged = lrn != LocalReferenceNumber;
				UpdateCustomsStatusForAmendment();
				UpdateMessageStatusForAmendment();

				if (lrnHasChanged)
				{
					result = Res.GetString("4BE52F08-24D7-496B-9BAE-AFBCC411676E", "LRN updated and status reset. Please save, close and reopen this declaration to amend and retransmit.");
				}
				else
				{
					result = Res.GetString("C45B2EFE-3766-4581-A2CA-10A5412F0A6E", "Status reset. Please save, close and reopen this declaration to amend and retransmit.");
				}
			}
			return result;
		}

		protected virtual void UpdateMessageStatusForAmendment()
		{
			if (!IsPhase5)
			{
				EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			}
		}

		protected virtual void UpdateCustomsStatusForAmendment()
		{
			MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
		}

		protected virtual void UpdateLocalReferenceNumberForAmendment()
		{
			LocalReferenceNumber = LocalReferenceNumberAmendment;
		}

		public bool IsDepartureAmendmentAllowed => IsDepartureMovement && IsDepartureAmendmentAllowedCore();

		protected virtual bool IsDepartureAmendmentAllowedCore()
		{
			var departureStatus = MovementHeader.BM_CustomsStatus;
			return departureStatus == NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid
			|| departureStatus == NctsTransitStatusList.Codes.GoodsNotReleasedForTransit
			|| departureStatus == NctsTransitStatusList.Codes.DeclarationCancelled
			|| AdditionalClausesForWhenDepartureAmendmentIsAllowed;
		}

		public bool IsDepartureCancellationAllowed => IsDepartureCancellationAllowedCore();

		protected virtual bool IsDepartureCancellationAllowedCore()
		{
			if (MovementHeader == null || !IsDepartureMovement)
			{
				return false;
			}
			var departureStatus = MovementHeader.BM_CustomsStatus;
			return departureStatus == NctsTransitStatusList.Codes.DeclarationMrnAllocated
				|| departureStatus == NctsTransitStatusList.Codes.GoodsUnderCustomsControl
				|| departureStatus == NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid
				|| departureStatus == NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
		}

		protected virtual bool AdditionalClausesForWhenDepartureAmendmentIsAllowed => false;

		[ResourceStringData("5502D86C-879F-4429-8A71-316FE25F55BF", Caption = "Language")]
		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.CommunicationLanguageList))]
		[ReadOnlyMember(nameof(BH_CommunicationLanguageReadOnly))]
		public override ZString BH_CommunicationLanguage
		{
			get => base.BH_CommunicationLanguage;
			set => base.BH_CommunicationLanguage = value;
		}

		public bool BH_CommunicationLanguageReadOnly => IsArrivalDetailsReadOnly || IsArrivalNotificationDisabled;

		protected override ZString HumanReadableNameCore => Res.GetString("7203abef-fe9c-4019-afa6-de46a5bda52e", "NCTS Transit Movement {0}", BH_JobReference);

		public ZString HumanReadableNameConcrete => HumanReadableNameCore;

		public ZBool IsCurrentCountryNotGermany => CountryCode != Core.Constants.CountryCodes.Germany;

		public ZBool IsBrokerNeeded => IsBrokerNeededCore;

		protected virtual ZBool IsBrokerNeededCore => true;

		public ZBool HasSecurityAtHeaderLevel
		{
			get { return BH_FTZMove && ((SecurityConsignor != null && !SecurityConsignor.IsEmpty) || (SecurityConsignee != null && !SecurityConsignee.IsEmpty)); }
		}

		public ZBool HasSecurityAtGoodsItemLevel => BH_FTZMove && MovementHeader != null && MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>()
			.Any(item => item.SecurityConsignee?.Address != null || item.SecurityConsignor?.Address != null);

		public ZBool IsSecurityDeclaration => (!IsPhase5 && (HasSecurityAtHeaderLevel || HasSecurityAtGoodsItemLevel)) || (IsPhase5Departure && MovementHeader.IsSecurityTypeENTOrBTHOrEXI);

		public ZString FallbackInformation => GetFallbackInformationCore();

		protected virtual ZString GetFallbackInformationCore() => ZString.Empty;

		public virtual ZBool FallBackIsActive => false;

		DocManagerInfo docManagerInfo;
		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new NctsHeaderDocManagerInfo(this));

		BusinessObject INctsMovement.BusinessObject => this;

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				if (BH_ParentTableCode == CargoWise.Schema.Schema.GetPrefixFromColumnName(JobConsolSchema.PK.Name))
				{
					var consol = Consol;
					if (consol != null)
					{
						list.Add(consol);
					}
				}
				else if (BH_ParentTableCode == CargoWise.Schema.Schema.GetPrefixFromColumnName(JobShipmentSchema.PK.Name))
				{
					var shipment = Shipment;
					if (shipment != null)
					{
						list.Add(shipment);
					}
				}
				return list;
			}
		}

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return workflowInformationProvider ?? (workflowInformationProvider = new NctsHeaderWorkflowInformationProvider(this));
		}
		IWorkflowInformationProvider workflowInformationProvider;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode; }
		}

		[ChildEditable]
		public ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader> WorkflowItems
		{
			get { return (ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader>)((IWorkflowProvider)this).WorkflowItems; }
		}

		protected override ProcessTaskCollection GetNewCusInBondHeaderProcessTaskCollection()
		{
			return new ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader>(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = this.GetJobRelatedTemplateSelectionCriteria();
			var job = new JobHeader.Loader(this).Load(true, false);

			if (!Principal.OrganisationPK.IsEmpty)
			{
				result = AdjustClientPriority(result);
			}
			result.Add(ProcessTaskTemplateSchema.P0_GB, job != null && job.JH_GB.IsValid ? job.JH_GB : GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, job != null && job.JH_GE.IsValid ? job.JH_GE : GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);

			return result;
		}

		ColumnValueRanker AdjustClientPriority(ColumnValueRanker result)
		{
			var columnValues = ((IColumnValueRankerInternals)result).ColumnValues;

			result = new ColumnValueRanker();
			object[] values;

			foreach (var columnValuesPair in columnValues)
			{
				values = columnValuesPair.Values;

				if (columnValuesPair.Column != null
					&& columnValuesPair.Column == ProcessTaskTemplateSchema.P0_OH_Client)
				{
					object[] newValues = [Principal.OrganisationPK];

					values = values != null
						? newValues.Union(values).ToArray()
						: newValues;
				}

				result.Add(columnValuesPair.Column, values);
			}
			return result;
		}

		#region IWorkflowAffectedPropertyProvider Members

		ZPropertyInfo[] IWorkflowAffectedPropertyProvider.PropertyThatAffectWorkflowChanged
		{
			get
			{
				if (!IsDeleted)
				{
					return
					[
						Principal.OrganisationPKInfo
					];
				}
				return null;
			}
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		[ChildEditable]
		protected CustomBusinessObject CustomBusinessObject
		{
			get
			{
				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
					customBusinessObject = new CustomBusinessObject(Factory, this, properties);
					RegisterEditableChildObject(customBusinessObject);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;
		#endregion

		public bool IsConditionR0520_UserShouldNotSaveAmendments => IsConditionR0520_UserShouldNotSaveAmendmentsCore();

		protected virtual bool IsConditionR0520_UserShouldNotSaveAmendmentsCore()
		{
			return (Configuration.ValidationRuleConfiguration.IsRuleR0520Active
				&& IsPhase5Departure
				&& !MovementHeader.BM_CustomsStatus.IsEmpty
				&& (Principal.HasChanges
					|| MovementHeader.CustomsOfficesForDeparture.Cast<NctsEuOfficeCode>().Any(co => (ZString)co.CY_CodeInfo.OriginalValue == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture && (co.CY_CodeInfo.HasChanges || co.CY_DataInfo.HasChanges))
					|| MovementHeader.BM_InBondEntryTypeInfo.HasChanges
					|| MovementHeader.BM_AdditionalDeclarationTypeInfo.HasChanges
					|| MovementHeader.Representative.HasChanges
				));
		}

		public bool IsConditionR0520_UserShouldNotSaveAmendmentsToGuarantees => IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesCore();

		protected virtual bool IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesCore() => false;

		#region Cloning and copying

		public IBusiness TemplateCopy()
		{
			var result = (NctsHeader)(new NctsDeepCloneStrategy(this, ZGuid.Empty).Clone());
			result.HasChanges = false;
			return result;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newHeader = (NctsHeader)base.CloneInternal(args);
			newHeader.BH_ParentID = ZGuid.Empty;
			newHeader.BH_ParentTableCode = null;
			var newHeaderIsDepartureMovement = newHeader.IsDepartureMovement;
			if (newHeaderIsDepartureMovement)
			{
				newHeader.BH_HeaderType = NctsMovementType.Codes.Departure; // need to set this early for Type Decider
			}
			newHeader.MovementHeaders.DeleteAll();

			if (newHeaderIsDepartureMovement)
			{
				CloneDepartureMovement(newHeader);
				CloneCusAuthorizationUsages(newHeader);
				ClonePreviousDocuments(newHeader);
				CloneAdditionalDocuments(newHeader);
				CloneCountriesOfRouting(newHeader);
				CloneSupplyChainActors(newHeader);
				CloneBills(newHeader);
				CloneTransportMeans(newHeader);

				if (newHeader.IsPhase5)
				{
					newHeader.MovementHeader.BM_MessageStatus = ZString.Empty;
				}
				else
				{
					newHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
				}
			}
			else
			{
				if (newHeader.IsArrivalMovement)
				{
					CloneArrivalMovement(newHeader);
				}
				newHeader.EffectiveMessageStatus = newHeader.Configuration.GetDefaultMessageStatusForArrival(newHeader);
			}

			if (!newHeader.IsPhase5)
			{
				CloneCustomsOffices(newHeader, newHeaderIsDepartureMovement);
				CloneGuarantees(newHeader);
			}
			return newHeader;
		}

		protected virtual void CloneCustomsOffices(NctsHeader newHeader, bool newHeaderIsDepartureMovement)
		{
			var customsOfficesToBeCloned = newHeaderIsDepartureMovement ? CustomsOfficesForDeparture.ToArray() : CustomsOffices.ToArray();
			foreach (NctsEuOfficeCode office in customsOfficesToBeCloned)
			{
				var newOffice = (NctsEuOfficeCode)new NctsDeepCloneStrategy(office, newHeader.PK).Clone();
				newHeader.CustomsOffices.Add(newOffice);
			}
		}

		void CloneGuarantees(NctsHeader newHeader)
		{
			foreach (NctsGuarantee guarantee in Guarantees.ToArray())
			{
				var newGuarantee = (NctsGuarantee)new NctsDeepCloneStrategy(guarantee, newHeader.PK).Clone();
				newHeader.Guarantees.Add(newGuarantee);
			}
		}

		void CloneCusAuthorizationUsages(NctsHeader newHeader)
		{
			foreach (var cusAuthorizationUsage in CusAuthorizationUsages)
			{
				var newCusAuthorizationUsage = (CusAuthorizationUsage)new NctsDeepCloneStrategy(cusAuthorizationUsage, newHeader.PK).Clone();
				newHeader.CusAuthorizationUsages.Add(newCusAuthorizationUsage);
			}
		}

		void ClonePreviousDocuments(NctsHeader newHeader)
		{
			foreach (var previousDocument in PreviousDocuments)
			{
				var newPreviousDocument = (PreviousDocument)new NctsDeepCloneStrategy(previousDocument, newHeader.PK).Clone();
				newHeader.PreviousDocuments.Add(newPreviousDocument);
			}
		}

		protected virtual void CloneAdditionalDocuments(NctsHeader newHeader)
		{
			foreach (var additionalDocument in AdditionalDocuments)
			{
				var newAdditionalDocument = (NctsAdditionalInfo)new NctsDeepCloneStrategy(additionalDocument, newHeader.PK).Clone();
				newHeader.AdditionalDocuments.Add(newAdditionalDocument);
			}
		}

		void CloneCountriesOfRouting(NctsHeader newHeader)
		{
			foreach (var countryOfRouting in CountriesOfRouting)
			{
				var newCountryOfRouting = (CountryOfRouting)new NctsDeepCloneStrategy(countryOfRouting, newHeader.PK).Clone();
				newHeader.CountriesOfRouting.Add(newCountryOfRouting);
			}
		}

		void CloneSupplyChainActors(NctsHeader newHeader)
		{
			foreach (var supplyChainActor in CusSupplyChainActors)
			{
				var newSupplyChainActor = (CusSupplyChainActorReference)new NctsDeepCloneStrategy(supplyChainActor, newHeader.PK).Clone();
				newHeader.CusSupplyChainActors.Add(newSupplyChainActor);
			}
		}

		void CloneBills(NctsHeader newHeader)
		{
			foreach (var bill in Bills)
			{
				var newBill = (NctsBill)new NctsDeepCloneStrategy(bill, newHeader.PK).Clone();
				newHeader.Bills.Add(newBill);

				foreach (var supportingDocument in bill.SupportingDocuments)
				{
					var newSupportingDocument = (NctsSupportingDocument)new NctsDeepCloneStrategy(supportingDocument, newBill.PK).Clone();
					newBill.SupportingDocuments.Add(newSupportingDocument);
				}

				foreach (var previousDocument in bill.PreviousDocuments)
				{
					var newPreviousDocument = (CommonPreviousDocument)new NctsDeepCloneStrategy(previousDocument, newBill.PK).Clone();
					newBill.PreviousDocuments.Add(newPreviousDocument);
				}

				foreach (var additionalDocument in bill.AdditionalDocuments)
				{
					var newAdditionalDocument = (NctsBillAdditionalDocument)new NctsDeepCloneStrategy(additionalDocument, newBill.PK).Clone();
					newBill.AdditionalDocuments.Add(newAdditionalDocument);
				}

				foreach (var supplyChainActorReference in bill.CusSupplyChainActorReferences)
				{
					var newSupplyChainActorReference = (CusSupplyChainActorReference)new NctsDeepCloneStrategy(supplyChainActorReference, newBill.PK).Clone();
					newBill.CusSupplyChainActorReferences.Add(newSupplyChainActorReference);
				}

				foreach (NctsDepartureCargoDesc goodsItem in bill.GoodsItems)
				{
					var newGoodsItem = (NctsDepartureCargoDesc)new NctsDeepCloneStrategy(goodsItem, newBill.PK).Clone();
					if (IsPhase5 && !IsInPhase5TransitionPeriod)
					{
						newGoodsItem.Consignee.Delete();
					}
					newBill.GoodsItems.Add(newGoodsItem);
				}
			}
		}

		void CloneTransportMeans(NctsHeader newHeader)
		{
			if (MovementHeader is NctsDepartureMovementHeader movementHeader && movementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport)
			{
				var newMovementHeader = newHeader.MovementHeader;
				foreach (var transportMeans in movementHeader.AdditionalTransportAtBorderList)
				{
					var newtransportMeans = (DepartureCusTransportMeans)new NctsDeepCloneStrategy(transportMeans, newMovementHeader.PK).Clone();
					newMovementHeader.AdditionalTransportAtBorderList.Add(newtransportMeans);
				}
			}
		}

		protected override bool SupportsCloneCore() => true;

		void CloneDepartureMovement(NctsHeader newHeader)
		{
			var movementHeaderToClone = MovementHeader;
			var newMovementHeader = (NctsDepartureMovementHeader)new NctsDeepCloneStrategy(movementHeaderToClone, newHeader.PK).Clone();

			if (IsPhase5)
			{
				CloneGoodsLocation(movementHeaderToClone, CusGoodsLocationUseList.Codes.Departure, newMovementHeader);

				newMovementHeader.BM_EntryDate = ZDate.Empty;

				var customsOfficesToBeCloned = movementHeaderToClone.CustomsOfficesForDeparture.ToArray();
				foreach (NctsEuOfficeCode office in customsOfficesToBeCloned)
				{
					var newOffice = (NctsEuOfficeCode)new NctsDeepCloneStrategy(office, newMovementHeader.PK).Clone();
					newMovementHeader.CustomsOffices.Add(newOffice);
				}
				foreach (var supportingDocument in movementHeaderToClone.SupportingDocuments)
				{
					var newSupportingDocument = (NctsSupportingDocument)new NctsDeepCloneStrategy(supportingDocument, newMovementHeader.PK).Clone();
					newMovementHeader.SupportingDocuments.Add(newSupportingDocument);
				}
			}

			newMovementHeader.BM_ExportDate = ZDate.Empty;
		}

		void CloneArrivalMovement(NctsHeader newHeader)
		{
			var arrivalMovementHeader = ArrivalMovementHeader;
			var newArrivalMovementHeader = (NctsArrivalMovementHeader)new NctsDeepCloneStrategy(arrivalMovementHeader, newHeader.PK).Clone();
			newArrivalMovementHeader.BM_TOLDate = ZDateTime.Today;
			newArrivalMovementHeader.BM_ExportDate = ZDate.Empty;
			newHeader.BH_ExportFlag = EventFlagList.Codes.No;
			CloneArrivalAuthorization(arrivalMovementHeader, newHeader);
			CloneGoodsLocation(arrivalMovementHeader, CusGoodsLocationUseList.Codes.Arrival, newArrivalMovementHeader);
		}

		void CloneArrivalAuthorization(NctsArrivalMovementHeader arrivalMovementHeader, NctsHeader newHeader)
		{
			if (arrivalMovementHeader.LoadAuthorization() is CusAuthorizationUsage cusAuthorizationUsage)
			{
				new NctsDeepCloneStrategy(cusAuthorizationUsage, newHeader.PK).Clone();
			}
		}

		void CloneGoodsLocation(NctsCommonMovementHeader movementHeader, ZString locationUse, NctsCommonMovementHeader newMovementHeader)
		{
			if (Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(movementHeader, locationUse) is CusGoodsLocation cusGoodsLocation)
			{
				Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(newMovementHeader, locationUse)?.Delete();

				var newGoodsLocation = (CusGoodsLocation)new NctsDeepCloneStrategy(cusGoodsLocation, newMovementHeader.PK).Clone();
				newGoodsLocation.Address.CopyPersistentValuesFrom(cusGoodsLocation.Address, new BusinessObjectCloneArgs());
			}
		}

		#endregion

		#region Movement Type Setting

		[ResourceStringData("NctsHeader.IsDepartureMovement", Caption = "Departure?", ShortCaption = "Dep.?", FullDescription = "Is Departure Movement?")]
		public bool IsDepartureMovement
		{
			get
			{
				var headerType = BH_HeaderType;
				return headerType == NctsMovementType.Codes.Departure || headerType == NctsMovementType.Codes.DepartureAndArrival;
			}
		}

		[ResourceStringData("NctsHeader.IsArrivalMovement", Caption = "Arrival?", ShortCaption = "Arr?", FullDescription = "Is Arrival Movement?")]
		public bool IsArrivalMovement
		{
			get
			{
				var headerType = BH_HeaderType;
				return headerType == NctsMovementType.Codes.Arrival || headerType == NctsMovementType.Codes.DepartureAndArrival;
			}
		}

		public bool IsDepartureAndArrivalMovement => BH_HeaderType == NctsMovementType.Codes.DepartureAndArrival;

		public void SetMovementType(ZString headerType)
		{
			if (BH_HeaderType.IsEmpty)
			{
				if (headerType.IsEmpty)
				{
					BH_HeaderType = NctsMovementType.Codes.Departure;
				}
				else
				{
					BH_HeaderType = headerType;
				}
				SetInitialStatus();
				SetInitialCustomsOffice();
				SetInitialDestinationTraderCore();
				SetInitialPrincipal();
			}
		}

		public void SetInitialCustomsOffice()
		{
			if (MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				AddCusomsOfficeIfRequired(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				AddCusomsOfficeIfRequired(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			}
		}

		void AddCusomsOfficeIfRequired(NctsDepartureMovementHeader movementHeader, ZString officeCode)
		{
			var isPhase5 = IsPhase5;
			var requirementHelper = isPhase5 ? movementHeader.CustomsOfficeRequirementHelper : CustomsOfficeRequirementHelper;
			var isCusomsOfficeRecommended = requirementHelper.GetOtherRequirementByRole(officeCode)?.IsRecommended ?? false;
			if (isCusomsOfficeRecommended)
			{
				var customsOffices = isPhase5 ? movementHeader.CustomsOffices : CustomsOffices;
				if (!customsOffices.GetElementsHaving(officeCode).Any())
				{
					customsOffices.AddNew(officeCode);
				}
			}
		}

		void SetInitialStatus()
		{
			if (EffectiveMessageStatus == NctsMessageStatusList.Codes.Unknown)
			{
				if (!IsPhase5 && IsDepartureMovement)
				{
					EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
				}
				else if (IsArrivalMovement)
				{
					EffectiveMessageStatus = Configuration.GetDefaultMessageStatusForArrival(this);
				}
			}
		}

		protected virtual void SetInitialDestinationTraderCore()
		{
			if (IsArrivalMovement)
			{
				DefaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
			}
		}

		void SetInitialPrincipal()
		{
			if (IsDepartureMovement)
			{
				DefaultPrincipalRegistryManager.ApplyDefaultingIfEnabled();
			}
		}

		#endregion

		#region Message sending message error validation

		public ZString GetNonMandatoryMessageErrors()
		{
			var notifications = new CustomsNotificationCollector(this, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName);
			var errorCollection = new MessageSendingNotificationCollection();

			foreach (var notification in notifications)
			{
				if (notification.Type != CargoWise.ComponentModel.NotificationType.Warning)
				{
					AddMesageErrorToCollection(errorCollection, notification.Message);
				}
			}
			return errorCollection.NotificationsAsString();
		}

		public MessageSendingNotificationCollection GetMandatoryMessageErrors(Dictionary<ZPropertyInfo, Action> propertiesToValidate)
		{
			var notificationCollection = new MessageSendingNotificationCollection();

			foreach (var propertyInfo in propertiesToValidate.Keys)
			{
				foreach (var notification in propertyInfo.Notifications)
				{
					if (notification.Type != CargoWise.ComponentModel.NotificationType.Warning)
					{
						AddMesageErrorToCollection(notificationCollection, notification.Message);
					}
				}
			}
			return notificationCollection;
		}

		public void AddMesageErrorToCollection(MessageSendingNotificationCollection notifications, ZString error)
		{
			if (notifications != null && !IsErrorInCollection(notifications, error))
			{
				notifications.AddError(error);
			}
		}

		bool IsErrorInCollection(MessageSendingNotificationCollection notifications, ZString errorText)
		{
			var elements = new List<MessageSendingNotification>();
			elements.AddRange(notifications);

			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsError && notification.Message.ToString() == errorText)
				{
					return true;
				}
			}
			return false;
		}

		public void GetMandatoryUnloadingPropertiesToValidate(Dictionary<ZPropertyInfo, Action> propertiesToValidate)
		{
			propertiesToValidate.Add(UnloadingRemark.G9_UnloadingDateInfo, UnloadingRemark.Validation.ValidateG9_UnloadingDate);
			propertiesToValidate.Add(UnloadingRemark.G9_ConformInfo, UnloadingRemark.Validation.ValidateG9_Conform);
			propertiesToValidate.Add(UnloadingRemark.G9_StateOfSealsOkInfo, UnloadingRemark.Validation.ValidateG9_StateOfSealsOk);
			propertiesToValidate.Add(UnloadingRemark.G9_UnloadingCompletionInfo, UnloadingRemark.Validation.ValidateG9_UnloadingCompletion);
		}

		public void GetMandatoryArrivalPropertiesToValidate(Dictionary<ZPropertyInfo, Action> propertiesToValidate)
		{
			if (!IsArrivalDetailsReadOnly)
			{
				propertiesToValidate.Add(DestinationTrader.OrganisationPKInfo, DestinationTrader.Validation.ValidateOrganisationPK);
			}
		}

		public void GetMandatoryDeclarationPropertiesToValidate(Dictionary<ZPropertyInfo, Action> propertiesToValidate)
		{
			propertiesToValidate.Add(MovementHeader.BM_InBondEntryTypeInfo, MovementHeader.Validation.ValidateBM_InBondEntryType);
			propertiesToValidate.Add(Principal.OrganisationPKInfo, Principal.Validation.ValidateOrganisationPK);
			propertiesToValidate.Add(Consignee.OrganisationPKInfo, Principal.Validation.ValidateOrganisationPK);
			propertiesToValidate.Add(BH_RL_NKImportLoadPortInfo, Validation.ValidateBH_RL_NKImportLoadPort);
			propertiesToValidate.Add(MovementHeader.BM_RL_NKDestinationPortInfo, MovementHeader.Validation.ValidateBM_RL_NKDestinationPort);
		}

		#endregion

		#region Menu and Tab visibility and Readonly processing based on status

		public bool IsDepartureTabVisible
		{
			get { return IsDepartureMovement || IsDepartureAndArrivalMovement; }
		}

		public void MakeArrivalNotificationFromDeparture()
		{
			BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			if (IsArrivalMovement)
			{
				ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			}
			EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;

			DestinationTrader.OrganisationPK = Consignee.OrganisationPK;
		}

		public bool IsDepartureTabReadOnly => IsDepartureTabReadOnlyCore;
		protected virtual bool IsDepartureTabReadOnlyCore
		{
			get
			{
				if (IsDepartureMovement)
				{
					var messageStatus = EffectiveMessageStatus;
					var departureStatus = MovementHeader.BM_CustomsStatus;
					return (messageStatus == NctsMessageStatusList.Codes.DepartureDeclarationSent
						|| messageStatus == NctsMessageStatusList.Codes.CancellationRequestSent
						|| departureStatus == NctsTransitStatusList.Codes.DeclarationAccepted
						|| departureStatus == NctsTransitStatusList.Codes.DeclarationMrnAllocated
						|| departureStatus == NctsTransitStatusList.Codes.GoodsUnderCustomsControl
						|| departureStatus == NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid
						|| departureStatus == NctsTransitStatusList.Codes.GoodsNotReleasedForTransit
						|| departureStatus == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture
						|| departureStatus == NctsTransitStatusList.Codes.DeclarationCancelled
						|| IsDepartureTabReadOnlyCountrySpecificRules(messageStatus, departureStatus)
						|| messageStatus == NctsMessageStatusList.Codes.MessageQueued)
						&& !IsDepartureTabEditableCountrySpecificRules(messageStatus, departureStatus);
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual bool IsDepartureTabEditableCountrySpecificRules(ZString messageStatus, ZString departureStatus) => false;
		protected virtual bool IsDepartureTabReadOnlyCountrySpecificRules(ZString messageStatus, ZString departureStatus) => false;

		public bool IsArrivalTabVisible
		{
			get { return IsArrivalMovementAllowed; }
		}

		public bool IsArrivalTabReadOnly => IsArrivalTabReadOnlyCore;
		protected virtual bool IsArrivalTabReadOnlyCore
		{
			get
			{
				if (IsArrivalMovement)
				{
					var messageStatus = EffectiveMessageStatus;
					var arrivalStatus = ArrivalMovementHeader.BM_CustomsStatus;
					return arrivalStatus == NctsTransitStatusList.Codes.UnloadingPermissionGranted
						|| arrivalStatus == NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival
						|| messageStatus == NctsMessageStatusList.Codes.ArrivalNotificationSent
						|| messageStatus == NctsMessageStatusList.Codes.UnloadingRemarksSent
						|| messageStatus == NctsMessageStatusList.Codes.UnloadingRemarksRejected;
				}
				else
				{
					return false;
				}
			}
		}

		public bool IsArrivalDetailsReadOnly => IsArrivalDetailsReadOnlyCore;

		protected virtual bool IsArrivalDetailsReadOnlyCore => IsArrivalMovement && EffectiveMessageStatus.In<ZString>(LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Acknowledged);

		public bool IsUnloadingRemarksTabVisible => UnloadingRemarksAllowedOverride || (IsArrivalMovementAllowed && IsUnloadingAllowedOrComplete);

		public bool IsUnloadingRemarksTabReadOnly => IsUnloadingRemarksTabReadOnlyCore;
		protected virtual bool IsUnloadingRemarksTabReadOnlyCore
		{
			get
			{
				if (IsPhase5 || !IsArrivalMovement)
				{
					return false;
				}

				var arrivalStatus = ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty;
				return arrivalStatus == NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival
					|| arrivalStatus == NctsTransitStatusList.Codes.GoodsWrittenOff
					|| EffectiveMessageStatus == NctsMessageStatusList.Codes.UnloadingRemarksSent;
			}
		}

		public bool IsArrivalMovementAllowed
		{
			get { return IsArrivalMovement || IsDepartureAndArrivalMovement; }
		}

		public virtual bool UnloadingRemarksAllowedOverride => false;

		public bool IsUnloadingAllowedOrComplete => IsUnloadingAllowedOrCompleteCore;

		protected virtual bool IsUnloadingAllowedOrCompleteCore
		{
			get
			{
				var arrivalStatus = ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty;
				return arrivalStatus.In(UnloadingAllowedOrCompleteStatusList);
			}
		}

		protected IReadOnlyList<ZString> UnloadingAllowedOrCompleteStatusList => IsPhase5 ? UnloadingAllowedOrCompleteStatusListPhase5 : new ZString[] { NctsTransitStatusList.Codes.UnloadingPermissionGranted, NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival };

		ZString[] UnloadingAllowedOrCompleteStatusListPhase5 => new ZString[] { NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease, NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease, NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease };

		public virtual bool SupportsCombinedArrivalAndDepartureMessage => false;

		public virtual bool StopFromSendingMessageWithMandatoryError => true;

		#endregion Menu and Tab visibility and Readonly processing based on status

		#region Synching from Forwarding

		protected override void SynchroniseWithParentIfNeededCore()
		{
			if (Synchroniser != null && ShouldSynchronise)
			{
				try
				{
					using (SuspendSettingHasChanges())
					using (GetValidationSuspender())
					{
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
						Messages.CountChanged += StopSynchronisationOnMessages_CountChanged;
					}
				}
				finally
				{
					ClearSynchronisationHasChanges();
				}
			}
		}

		void ClearSynchronisationHasChanges()
		{
			if (IsInDatabase)
			{
				((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
			}
		}

		void StopSynchronisationOnMessages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (Synchroniser != null)
			{
				Messages.CountChanged -= StopSynchronisationOnMessages_CountChanged;
				Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));
			}
		}

		public ICusInBondParent Parent
		{
			get { return (ICusInBondParent)ParentBusinessObject; }
		}

		protected sealed override BusinessObjectSynchroniser GetNewSynchroniserCore()
		{
			if (nctsSynchroniser == null && Parent != null)
			{
				if (Parent is ForwardingShipment)
				{
					nctsSynchroniser = GetNewNctsShipmentSynchroniser(Parent);
				}
				else if (Parent is ForwardingConsol)
				{
					nctsSynchroniser = GetNewNctsConsolSynchroniser(Parent);
				}
#pragma warning disable
				((IBusinessObjectState)ParentBusinessObject).UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
				((IBusinessObjectState)ParentBusinessObject).UpdatedByDataRefreshIncludingChildren += OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
			}
			return nctsSynchroniser;
		}
		BusinessObjectSynchroniser nctsSynchroniser;

		protected BusinessObjectSynchroniser GetNewNctsShipmentSynchroniser(ICusInBondParent parent) => this switch
		{
			NctsHeader { IsPhase5Arrival: true } => GetNewNctsPhase5ArrivalShipmentSynchroniser(parent),
			NctsHeader { IsPhase5Departure: true } => GetNewNctsPhase5DepartureShipmentSynchroniser(parent),
			_ => GetNewNctsPhase4ShipmentSynchroniser(parent)
		};

		protected virtual BusinessObjectSynchroniser GetNewNctsPhase5ArrivalShipmentSynchroniser(ICusInBondParent parent) => new NctsPhase5ArrivalShipmentSynchroniser(this, parent);

		protected virtual BusinessObjectSynchroniser GetNewNctsPhase5DepartureShipmentSynchroniser(ICusInBondParent parent) => new NctsPhase5DepartureShipmentSynchroniser(this, parent);

		protected virtual BusinessObjectSynchroniser GetNewNctsPhase4ShipmentSynchroniser(ICusInBondParent parent) => new NctsShipmentSynchroniser(this, parent);

		protected BusinessObjectSynchroniser GetNewNctsConsolSynchroniser(ICusInBondParent parent) => this switch
		{
			NctsHeader { IsPhase5Arrival: true } => GetNewNctsPhase5ArrivalConsolSynchroniser(parent),
			NctsHeader { IsPhase5Departure: true } => GetNewNctsPhase5DepartureConsolSynchroniser(parent),
			_ => GetNewNctsPhase4ConsolSynchroniser(parent)
		};

		protected virtual BusinessObjectSynchroniser GetNewNctsPhase5ArrivalConsolSynchroniser(ICusInBondParent parent) => new NctsPhase5ArrivalConsolSynchroniser(this, parent);

		protected virtual BusinessObjectSynchroniser GetNewNctsPhase5DepartureConsolSynchroniser(ICusInBondParent parent) => new NctsPhase5DepartureConsolSynchroniser(this, parent);

		protected virtual BusinessObjectSynchroniser GetNewNctsPhase4ConsolSynchroniser(ICusInBondParent parent) => new NctsConsolSynchroniser(this, parent);

		void OnConsolWasUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			nctsSynchroniser?.Synchronise();
		}

		public virtual bool ShouldTransportDetailsSyncDependsOnTransportMode => true;

		public sealed override ZBool BH_OverrideFreightDefaults
		{
			get { return base.BH_OverrideFreightDefaults; }
			set
			{
				var oldValue = BH_OverrideFreightDefaults;
				SetOverrideFreightDefaults(value);
				BH_OverrideFreightDefaultsInfo.RefreshBinding(oldValue);
			}
		}

		public bool IsPluggedIn
		{
			get { return IsPluggedIntoShipment || IsPluggedIntoConsol; }
		}

		public bool IsPluggedIntoConsol
		{
			get { return Consol != null; }
		}

		public bool IsPluggedIntoShipment
		{
			get { return Shipment != null; }
		}

		void SetOverrideFreightDefaults(ZBool value)
		{
			var oldValue = BH_OverrideFreightDefaults;
			if (oldValue != value)
			{
				if (!value && Parent != null)
				{
					Synchroniser?.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				}
				else
				{
					if (nctsSynchroniser != null)
					{
						nctsSynchroniser.SetEnabled(false, nctsSynchroniser.DetectEnabled);
						nctsSynchroniser.Dispose();
						nctsSynchroniser = null;
					}
				}
				base.BH_OverrideFreightDefaults = value;
				RefreshBindingIncludingChildren();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// dispose managed resources
				nctsSynchroniser?.Dispose();
			}
			// free native resources
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override bool ShouldSynchronise
		{
			get { return !BH_OverrideFreightDefaults && IsPluggedIn && !IsMessagingActive; }
		}

		public bool IsMessagingActive
		{
			get { return Messages.Count > 0; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return BH_JobReference; }
		}

		#endregion

		[ResourceStringData("3576A770-5137-43BE-B217-64DAB691A63D", Caption = "Job Number")]
		public ZString JobReferenceNumber => Shipment?.JS_UniqueConsignRef ?? BH_JobReference;

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(NctsEuOfficeCode) },
				{ CusCodeDataTypeList.Codes.Seal, typeof(Seal) },
				{ CusCodeDataTypeList.Codes.CountryOfRouting, typeof(CountryOfRouting) },
			};
		}

		#endregion

		#region ICusEntryNumFilterProvider Members

		ZQuery ICusEntryNumFilterProvider.ValidCusEntryNumFilter
		{
			get
			{
				var result = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
				result.DefaultJoinCondition = JoinCondition.And;
				result.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				return result;
			}
		}

		#endregion

		public ZString BrokerageCountryCode => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);

		public virtual ZString DefaultDataGroupingCode => CountryCode;

		public ZString LocalCurrency => Company.GC_RX_NKLocalCurrency;

		public ZInt DecimalCurrency => Company.LocalCurrency.Decimals;

		public ZInt MaximumGuaranteeCount => MaximumGuaranteeCountCore;

		protected virtual ZInt MaximumGuaranteeCountCore => 0;

		public void ApportionedAmountToGuaranteesLiabilityAmount()
		{
			if (IsPhase5Arrival)
			{
				ApportionedAmountToGuaranteesLiabilityAmountCore_Phase5Arrival();
			}
			else if (IsDepartureMovement)
			{
				ApportionedAmountToGuaranteesLiabilityAmountCore();
			}
		}

		protected internal ZDecimal ApportionedAmount
		{
			get
			{
				var goodItems = Array.Empty<NctsDepartureCargoDesc>();
				var result = ZDecimal.Zero;
				if (IsPhase4)
				{
					goodItems = MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().ToArray();
					result = goodItems.Sum(x => RoundToCurrency(x.VatAmount) + RoundToCurrency(x.DutyAmount) + RoundToCurrency(x.AntiDumpingDutyAmount) + RoundToCurrency(x.CountervailingDutyAmount) + RoundToCurrency(x.ExciseAmount));
				}
				else
				{
					goodItems = Bills.SelectMany(x => x.GoodsItems).Cast<NctsDepartureCargoDesc>().ToArray();
					result = goodItems.Sum(x => RoundToCurrency(x.LiabilityAmount));
				}
				return result;
			}
		}

		protected virtual void ApportionedAmountToGuaranteesLiabilityAmountCore_Phase5Arrival()
		{
			arrivalMovementHeader?.GuaranteesForArrival.ForEach(x => x.SetLiabilityAmount(x.PW_BondAmount));
		}

		protected virtual void ApportionedAmountToGuaranteesLiabilityAmountCore()
		{
			var guarantees = GetEffectiveGuarantees();
			var apportionmentTypes = guarantees.Cast<NctsGuarantee>().Select(x => x.ApportionmentType).Distinct();
			if (apportionmentTypes.Count() == 1)
			{
				var overrideGuarantees = guarantees.Cast<NctsGuarantee>().Where(x => x.PW_Override).ToArray();
				ZDecimal liabilityAmountWithOverride = overrideGuarantees.Sum(x => x.PW_BondAmount);
				var apportionmentType = apportionmentTypes.First();
				var apportionedAmount = ApportionedAmount - liabilityAmountWithOverride;
				var shouldSetZeroGuaranteeAmount = false;
				if (IsPhase5 && Bills.FirstOrDefault()?.GoodsItems.InitialApportionedAmount > 0 && apportionedAmount == 0)
				{
					shouldSetZeroGuaranteeAmount = true;
				}
				switch (apportionmentType)
				{
					case GuaranteeApportionmentType.EqualShare:
						{
							var originalGuaranteesWithOverride = overrideGuarantees.Select(x => (x.PK, x.PW_BondAmount)).ToArray();
							var guaranteesWithOverrideCount = originalGuaranteesWithOverride.Length;
							var guaranteeCount = guarantees.Count;
							var guaranteesWithoutOverrideCount = guaranteeCount - guaranteesWithOverrideCount;
							var sharedAmount = guaranteesWithoutOverrideCount > 0 ? RoundToCurrency(apportionedAmount / (guaranteeCount - guaranteesWithOverrideCount)) : liabilityAmountWithOverride;
							if (guaranteeCount > 1 && sharedAmount != 0)
							{
								SetGuaranteesAmountBeforeRound(guarantees, sharedAmount);
								SetRoundGuaranteesLiabilityAmount(guarantees, apportionedAmount);
								SetChangeLiabilityAmountOfGuranteesWithOverride(guarantees, originalGuaranteesWithOverride);
								SetGuaranteesLiabilityAmountWithValueInBondAmount(guarantees, shouldSetZeroGuaranteeAmount);
							}
							else
							{
								SetGuaranteesLiabilityAmount(guarantees, sharedAmount, shouldSetZeroGuaranteeAmount);
							}
							break;
						}
					case GuaranteeApportionmentType.None:
						SetGuaranteesLiabilityAmount(guarantees, RoundToCurrency(apportionedAmount), shouldSetZeroGuaranteeAmount);
						break;
					case GuaranteeApportionmentType.ConsumeAll:
						SetGuaranteesLiabilityAmountWithConsumeAll(guarantees, RoundToCurrency(apportionedAmount));
						break;
					case GuaranteeApportionmentType.Voucher:
						SetGuaranteesLiabilityAmount(guarantees, RoundToCurrency(VoucherAmount), shouldSetZeroGuaranteeAmount);
						break;
				}
			}
		}

		ZDecimal RoundToCurrency(ZDecimal amount) => Utilities.Round(amount, DecimalCurrency);

		void SetGuaranteesAmountBeforeRound(INctsGuaranteeCollection<NctsGuarantee> guarantees, ZDecimal amount)
		{
			guarantees.Cast<NctsGuarantee>().ForEach(x =>
			{
				if (amount > 0)
				{
					x.PW_BondAmount = amount.Round(2);
				}
			});
		}

		void SetGuaranteesLiabilityAmountWithValueInBondAmount(INctsGuaranteeCollection<NctsGuarantee> guarantees, ZBool shouldSetZeroGuaranteeAmount)
		{
			guarantees.Cast<NctsGuarantee>().ForEach(x =>
			{
				if (x.PW_BondAmount > 0)
				{
					x.DefaultSuretyCodeIfHasMatchLapPermitRule();
				}
				if (x.PW_BondAmount > 0 || shouldSetZeroGuaranteeAmount)
				{
					x.SetLiabilityAmount(x.PW_BondAmount);
				}
			});
		}

		void SetChangeLiabilityAmountOfGuranteesWithOverride(INctsGuaranteeCollection<NctsGuarantee> guarantees, (ZGuid PK, ZDecimal PW_BondAmount)[] originalGuaranteesWithOverride)
		{
			foreach (var (pK, bondAmount) in originalGuaranteesWithOverride)
			{
				var saveGuarantee = guarantees.Cast<NctsGuarantee>().FirstOrDefault(y => y.PK == pK);
				if (saveGuarantee != null)
				{
					saveGuarantee.PW_BondAmount = bondAmount;
				}
			}
		}

		void SetGuaranteesLiabilityAmount(INctsGuaranteeCollection<NctsGuarantee> guarantees, ZDecimal amount, ZBool shouldSetZeroGuaranteeAmount)
		{
			guarantees.Cast<NctsGuarantee>().ForEach(x =>
			{
				if (amount > 0)
				{
					x.DefaultSuretyCodeIfHasMatchLapPermitRule();
				}
				if (amount > 0 || shouldSetZeroGuaranteeAmount)
				{
					x.SetLiabilityAmount(amount);
				}
			});
		}

		void SetRoundGuaranteesLiabilityAmount(INctsGuaranteeCollection<NctsGuarantee> guarantees, ZDecimal apportionedAmount)
		{
			var liabilityAmountWithOutOverride = guarantees.Cast<NctsGuarantee>().Where(x => !x.PW_Override).Sum(x => x.PW_BondAmount);
			var diff_sumPW_BondAmountAndAmount = liabilityAmountWithOutOverride - apportionedAmount;
			var correctionAmount = (decimal)0.01;
			if (diff_sumPW_BondAmountAndAmount != 0)
			{
				guarantees.Cast<NctsGuarantee>().ForEach(x =>
				{
					if (diff_sumPW_BondAmountAndAmount != 0 && !x.PW_Override)
					{
						if (diff_sumPW_BondAmountAndAmount > 0)
						{
							x.PW_BondAmount = x.PW_BondAmount - correctionAmount;
							diff_sumPW_BondAmountAndAmount = diff_sumPW_BondAmountAndAmount - correctionAmount;
						}
						else
						{
							x.PW_BondAmount = x.PW_BondAmount + correctionAmount;
							diff_sumPW_BondAmountAndAmount = diff_sumPW_BondAmountAndAmount + correctionAmount;
						}
					}
				});
			}
		}

		void SetGuaranteesLiabilityAmountWithConsumeAll(INctsGuaranteeCollection<NctsGuarantee> guarantees, ZDecimal amount) => guarantees.Cast<NctsGuarantee>().ForEach(x => x.SetLiabilityAmountWithConsumeAllIfPositive(amount));

		#region IAllowPermitProcessing

		public ZString GetPermitReference() => GetPermitReferenceCore();

		protected virtual ZString GetPermitReferenceCore() => BH_JobReference;

		public ZString GetPermitComment(PermitRecord permitRecord) => new ZString(Res.GetString("872b9da7-2be1-486d-a621-4e848e60d3df", "NCTS departure {0}", LocalReferenceNumber));

		public ZInt GetPermitReferenceNumberLine() => 0;

		public IList<PermitRecord> GetPermitRecords() => GetPermitRecordsCore();

		protected virtual IList<PermitRecord> GetPermitRecordsCore()
		{
			return GetEffectiveGuarantees().Cast<NctsGuarantee>()
				.Select(x => (x.CusGuarantee, x.PW_BondAmount))
				.Where(x => x.CusGuarantee != null && x.PW_BondAmount > 0)
				.Select(x => new PermitRecord
				{
					PermitHeader = x.CusGuarantee,
					Quantity = 0,
					Value = x.PW_BondAmount
				}).ToList();
		}

		public ZInt PermitValueDecimalPlaceCount => RefCurrency.LoadFromCurrencyCode(Factory, LocalCurrency)?.Decimals ?? 2;

		public ZInt PermitQuantityDecimalPlaceCount => 5;

		ZInt IAllowPermitProcessing.PackageCount => TotalNumberOfPackages.ToZInt();
		#endregion

		public ZString[] GetDataGroupsForDestinationOfficeLookup() => GetDataGroupsForDestinationOfficeLookupCore();

		protected virtual ZString[] GetDataGroupsForDestinationOfficeLookupCore() => new ZString[] { DefaultDataGroupingCode };

		public ZString[] GetRolesForDestinationOfficeLookup() => GetRolesForDestinationOfficeLookupCore();

		protected virtual ZString[] GetRolesForDestinationOfficeLookupCore() => IsPhase5 ? new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination } : Array.Empty<ZString>();

		public NctsConfiguration Configuration => NctsConfiguration.GetConfiguration(Factory, BrokerageCountryCode);

		public ModeOfRepresentationCalculator ModeOfRepresentationCalculator => GetModeOfRepresentationCalculator();
		protected virtual ModeOfRepresentationCalculator GetModeOfRepresentationCalculator() => new ModeOfRepresentationCalculator(this);

		#region Document

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = GetNewDocumentSupporter());
		NctsHeaderDocumentSupporter documentSupporter;

		protected virtual NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		public ZString SourceType => nameof(DataContextType.NctsHeader);

		public ZString SourceID => JobNumber;

		public object GetDocDataObject(string dataContext, IDocDataObjectParameters parameters)
		{
			return null;
		}

		#endregion

		internal EU.Business.TransportModeTranslator TransportModeTranslator => transportModeTranslator ?? (transportModeTranslator = new EU.Business.TransportModeTranslator());
		EU.Business.TransportModeTranslator transportModeTranslator;

		#region ISupportMultipleResourceStringData

		public IReadOnlyList<string> MultipleKeysToUse => IsPhase5 ? ResolvedPhase5CaptionKey : new[] { Phase4CaptionKey };

		string[] ResolvedPhase5CaptionKey
		{
			get
			{
				if (IsPhase5Departure)
				{
					return new[] { Phase5DepartureCaptionKey, Phase5CaptionKey };
				}

				return new[] { Phase5CaptionKey };
			}
		}

		public const string Phase5DepartureCaptionKey = "837713EA-C4A5-4335-912A-45B7E438DFEC";

		public const string Phase5CaptionKey = "9B3CD485-8618-4ADD-B010-D0A8E09A8BA2";

		public const string Phase4CaptionKey = "817C85B2-1124-4559-B840-64DCDA890FD2";

		#endregion

		const decimal VoucherAmount = 10000m;

		public INctsIE29CusdecParser GetIE29CusdecParser(NctsEdiMessage message)
		{
			var result = GetNctsIE29CusdecParser();
			if (result != null)
			{
				result.EdiMessage = message;
				result.Factory = message.Factory;
			}
			return result;
		}

		protected virtual NctsIE29CusdecParser GetNctsIE29CusdecParser()
		{
			return null;
		}

		public Type CusInBondPersonType => CusInBondPersonTypeCore;

		protected virtual Type CusInBondPersonTypeCore => typeof(CusInBondPerson);

		#region INCTSAutoSendingMessageSupporter
		ZString INCTSAutoSendingMessageSupporter.GetReasonForNotSupportNCTSMessage => Res.GetString("C3B12B38-CA61-44D8-8B4F-42082E1C2EE0", "The Send NCTS Message trigger is not supported to be sent for {0}.", CountryCode);

		IProcessor INCTSAutoSendingMessageSupporter.CreateNCTSMessageProcessor()
		{
			return CreateNCTSMessageProcessorCore();
		}

		protected virtual IProcessor CreateNCTSMessageProcessorCore()
		{
			if (IsPhase5Departure && CountryCode != Core.Constants.CountryCodes.Norway && AutoMessageSendingHelper.SupportsNctsAutomaticMessageSending(CountryCode))
			{
				return new AutoSendNCTSP5MessageProcessor(this);
			}

			return new LogAction(((INCTSAutoSendingMessageSupporter)this).GetReasonForNotSupportNCTSMessage);
		}

		IProcessor INCTSAutoSendingMessageSupporter.CreateNCTSArrivalNotificationMessageProcessor()
		{
			return CreateNCTSArrivalNotificationMessageProcessorCore();
		}

		protected virtual IProcessor CreateNCTSArrivalNotificationMessageProcessorCore()
		{
			if (IsPhase5Arrival && AutoMessageSendingHelper.SupportsNctsAutomaticMessageSending(CountryCode))
			{
				return new AutoSendNCTSP5MessageProcessor(this);
			}

			return new LogAction(((INCTSAutoSendingMessageSupporter)this).GetReasonForNotSupportNCTSMessage);
		}

		IProcessor IBaseAutoSendingMessageSupporter.CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode)
		{
			return null;
		}
		#endregion

		#region DefaultTraderAtDestinationManager

		public INctsDefaultTraderAtDestinationManager DefaultTraderAtDestinationManager => defaultTraderAtDestinationManager ?? (defaultTraderAtDestinationManager = GetNewDefaultTraderAtDestinationManager());

		INctsDefaultTraderAtDestinationManager defaultTraderAtDestinationManager;

		protected virtual INctsDefaultTraderAtDestinationManager GetNewDefaultTraderAtDestinationManager() => new NctsDefaultTraderAtDestinationManager(this);

		#endregion

		#region DefaultPrincipalRegistryManager

		public INctsDefaultPrincipalRegistryManager DefaultPrincipalRegistryManager => defaultPrincipalRegistryManager ?? (defaultPrincipalRegistryManager = GetNewDefaultPrincipalRegistryManager());

		INctsDefaultPrincipalRegistryManager defaultPrincipalRegistryManager;

		protected virtual INctsDefaultPrincipalRegistryManager GetNewDefaultPrincipalRegistryManager() => new NctsDefaultPrincipalRegistryManager(this);

		protected void ResetDefaultPrincipalRegistryManager() => defaultPrincipalRegistryManager = null;

		#endregion

		#region DefaultConsignorConsigneeRegistryManager

		public INctsDefaultConsignorAndConsigneeRegistryManager DefaultConsignorConsigneeRegistryManager => defaultConsignorConsigneeRegistryManager ?? (defaultConsignorConsigneeRegistryManager = GetNewDefaultConsignorConsigneeRegistryManager());

		INctsDefaultConsignorAndConsigneeRegistryManager defaultConsignorConsigneeRegistryManager;

		protected virtual INctsDefaultConsignorAndConsigneeRegistryManager GetNewDefaultConsignorConsigneeRegistryManager() => new NctsDefaultConsignorConsigneeRegistryManager();

		protected void ResetDefaultConsignorConsigneeRegistryManager() => defaultConsignorConsigneeRegistryManager = null;

		#endregion

		#region ICusSupportingInfoTypeSupporter Implementation

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.PreviousDocument, PreviousDocumentType },
			{ CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoType },
			{ CusSupportingInfoTypeList.Codes.InstructionRequestedDocument, typeof(RequestedDocument) },
		};

		protected virtual Type AdditionalInfoType => NctsTypeDecider.GetNctsAdditionalInfoType(Factory, CountryCode);
		protected virtual Type PreviousDocumentType => typeof(CommonPreviousDocument);

		#endregion

		#region ICusReferenceTypeSupporter Members

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType },
		};

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

		#endregion

		#region ICusInBondHeader Members

		public void Synchronise(ZBool force)
		{
			Synchroniser?.Synchronise(force);
		}

		#endregion

		[ChildEditable(true)]
		public ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = GetPreviousDocuments();
					previousDocuments.Load();
					previousDocuments.ListChanged += PreviousDocuments_ListChanged;
					previousDocuments.SetReadOnlyIncludingChildren(IsPhase5 && IsArrivalMovement);
					RegisterEditableChildObject(previousDocuments);
				}
				return previousDocuments;
			}
		}
		ICommonPreviousDocumentCollection<CommonPreviousDocument> previousDocuments;

		protected virtual ICommonPreviousDocumentCollection<CommonPreviousDocument> GetPreviousDocuments() => new CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

		protected virtual void PreviousDocuments_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		public ZInt NCTSPreviousDocumentsCount => Factory.GetCached(ref fNCTSPreviousDocumentsCount, () => PreviousDocuments.Count(x => x.IsNCTSPreviousDocument));
		CachedProperty<ZInt> fNCTSPreviousDocumentsCount;

		public ZInt MaxNCTSPreviousDocumentsCount => Factory.GetCached(ref fMaxNCTSPreviousDocumentsCount, () => NCTSPreviousDocumentsCount + Bills.MaxOrDefault(x => x.MaxCountNCTSPreviousDocuments));
		CachedProperty<ZInt> fMaxNCTSPreviousDocumentsCount;

		[ChildEditable(true)]
		public ICusCodeDataCollection<CountryOfRouting> CountriesOfRouting
		{
			get
			{
				if (countriesOfRouting == null)
				{
					countriesOfRouting = GetCountryOfRoutingCollection();
					countriesOfRouting.Load();
					countriesOfRouting.ListChanged += CountriesOfRouting_ListChanged;
					RegisterEditableChildObject(countriesOfRouting);
				}
				return countriesOfRouting;
			}
		}
		ICusCodeDataCollection<CountryOfRouting> countriesOfRouting;

		protected virtual ICusCodeDataCollection<CountryOfRouting> GetCountryOfRoutingCollection() => new CountryOfRoutingCollection<CountryOfRouting>(this);

		protected virtual void CountriesOfRouting_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		[ChildEditable(true)]
		public INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments
		{
			get
			{
				if (additionalDocuments == null)
				{
					additionalDocuments = GetAdditionalDocuments();
					additionalDocuments.Load();
					additionalDocuments.ListChanged += AdditionalDocuments_ListChanged;
					RegisterEditableChildObject(additionalDocuments);
				}
				return additionalDocuments;
			}
		}
		INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalDocuments;

		protected virtual INctsAdditionalInfoCollection<NctsAdditionalInfo> GetAdditionalDocuments() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		protected virtual void AdditionalDocuments_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		[ChildEditable(true)]
		public RequestedDocumentCollection RequestedDocuments
		{
			get
			{
				if (requestedDocuments == null)
				{
					requestedDocuments = new RequestedDocumentCollection(this);
					requestedDocuments.Load();
					requestedDocuments.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(requestedDocuments);
				}
				return requestedDocuments;
			}
		}
		RequestedDocumentCollection requestedDocuments;

		public IEnumerable<IShortSequenceNumberLine> CountryOfRoutingLines => new TypedEnumerable<IShortSequenceNumberLine>(CountriesOfRouting);

		public ShortSequenceNumberGenerator CountryOfRoutingLineNumberGenerator => countryOfRoutingLineNumberGenerator ?? (countryOfRoutingLineNumberGenerator = new ShortSequenceNumberGenerator(() => CountryOfRoutingLines));
		ShortSequenceNumberGenerator countryOfRoutingLineNumberGenerator;

		public ICommonGoodsItemsIntegrator CommonGoodsItemsIntegrator => CommonGoodsItemsIntegratorCore;

		protected virtual ICommonGoodsItemsIntegrator CommonGoodsItemsIntegratorCore
			=> new NctsCommonGoodsItemsIntegrator(this);

		internal CustomsOfficesNumberGenerator OfficeSequenceNumberGenerator => officeLineNumberGenerator ?? (officeLineNumberGenerator = new CustomsOfficesNumberGenerator(this));
		CustomsOfficesNumberGenerator officeLineNumberGenerator;

		#region IHaveServices Members

		ZString IHaveServices.TableCode => TablePrefix;

		ZString IHaveServices.TransportMode => ZString.Empty;

		ZString IHaveServices.ContainerMode => ZString.Empty;

		BusinessObject IHaveServices.ServiceParent => this;

		bool IHaveServices.NeedsServiceEvents => false;

		ZGuid IHaveServices.PK => PK;

		JobServiceDependentCollection IHaveServices.Services => Services;

		IHaveServices[] IHaveServices.DependentServiceParents => Array.Empty<IHaveServices>();

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Branch;

		#endregion

		#region Authorization

		[ChildEditable]
		public ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader> CusAuthorizationUsages
		{
			get
			{
				if (cusAuthorizationUsages == null)
				{
					cusAuthorizationUsages = GetCusAuthorizationUsages();
					cusAuthorizationUsages.Load();
					cusAuthorizationUsages.ListChanged += CusAuthorizationUsages_ListChanged;
					cusAuthorizationUsages.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(cusAuthorizationUsages);
				}
				return cusAuthorizationUsages;
			}
		}
		ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader> cusAuthorizationUsages;

		protected virtual ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>(this);

		protected virtual void CusAuthorizationUsages_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		SchemaGuidColumn ICusAuthorizationUsageMaster.FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => BH_JobReference;

		ZString IRelatedJob.JobDescription => HumanReadableName;

		ZString IRelatedJob.JobStatus => JobStatus;

		protected virtual ZString JobStatus => ZString.Empty;
		#endregion

		#region ISequenceNumberHeader
		public IEnumerable<ISequenceNumberLine> Lines => AdditionalDocuments;

		public ShortSequenceNumberGenerator RefSequenceNumberGenerator
		{
			get
			{
				return refLineNumberGenerator ?? (refLineNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference));
			}
		}
		ShortSequenceNumberGenerator refLineNumberGenerator;

		public ShortSequenceNumberGenerator InfSequenceNumberGenerator
		{
			get
			{
				return infLineNumberGenerator ?? (infLineNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation));
			}
		}
		ShortSequenceNumberGenerator infLineNumberGenerator;

		public ShortSequenceNumberGenerator TraSequenceNumberGenerator
		{
			get
			{
				return traLineNumberGenerator ?? (traLineNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument));
			}
		}
		ShortSequenceNumberGenerator traLineNumberGenerator;

		#endregion

		#region IsArrivalNotificationDisabled

		public ZBool IsArrivalNotificationDisabled => CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsArrivalNotification) ?
													  (bool)IsLocked : IsArrivalNotificationDisabledNoConfig;

		bool IsArrivalNotificationDisabledNoConfig => Factory.GetValue(ref isArrivalNotificationDisabledNoConfig,
			() => Logs.HasLogWith(IsLogDisabledNoConfig) || (IsPhase5 && CommonMovementHeader.Logs.HasLogWith(IsLogDisabledNoConfig)));
		CachedProperty<bool> isArrivalNotificationDisabledNoConfig;

		Func<StmALog, bool> IsLogDisabledNoConfig => (l) => l.SL_SE_NKEvent == AutoEvents.CustomsEntryStatusCode &&
															l.SL_Reference.In<ZString>(
									NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted,
									NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks,
									NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
									NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease,
									NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease,
									NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease);

		public bool CanLockUnlockDeclaration(ZString declarationType) => CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Find(declarationType, Logs) != null || (IsPhase5 && CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Find(declarationType, CommonMovementHeader.Logs) != null);

		#endregion

		public ZBool IsLocked => Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit) != null;

		public bool IsAnyArrivalContainerSealDiscrepancy => Factory.GetCached(ref isAnyArrivalContainerSealDiscrepancy,
			() => ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().SelectMany(x => x.Seals).Cast<CusSeal>().Any(x => NctsHelper.IsUnloadedStateDiscrepancy(x.BK_UnloadingState)));
		CachedProperty<bool> isAnyArrivalContainerSealDiscrepancy;

		public bool IsAnyArrivalGoodsItemDiscrepancy => Factory.GetCached(ref isAnyArrivalGoodsItemDiscrepancy,
			() => Bills.Cast<NctsBill>().SelectMany(x => x.ArrivalGoodsItems).Cast<NctsArrivalCargoDesc>().Any(x => NctsHelper.IsUnloadedStateDiscrepancy(x.BY_UnloadedState)));
		CachedProperty<bool> isAnyArrivalGoodsItemDiscrepancy;

		public bool IsAnyArrivalHouseConsignmentDiscrepancy => Factory.GetCached(ref isAnyArrivalHouseConsignmentDiscrepancy,
			() => Bills.Cast<NctsBill>().Any(x => NctsHelper.IsUnloadedStateDiscrepancy(x.MovementDetail.B9_UnloadedState)));
		CachedProperty<bool> isAnyArrivalHouseConsignmentDiscrepancy;

		public ZGuid BranchPk => BH_GB;

		public ZString DeclarationType => IsArrivalMovement
			? EUJobMessageTypeList.Codes.NctsArrivalNotification
			: IsDepartureMovement
				? EUJobMessageTypeList.Codes.NctsDeparture
				: string.Empty;

		public ZPropertyInfo DeclarationTypeInfo => BH_HeaderTypeInfo;

		public void LockFile(ZString reference)
		{
			this.AddLockEvent(reference);
			this.RefreshBindingIncludingChildren();
		}

		public void UnlockFile(ZString reference)
		{
			this.AddUnlockEvent(reference);
			this.RefreshBindingIncludingChildren();
		}

		#region IHaveRequiredDocuments Members

		public ZString UniqueConsignRef => BH_JobReference;

		public ZString HouseBill => ZString.Empty;

		public ZString MasterBill => ZString.Empty;

		public OrgHeader ExportBroker => null;

		public ZString TableCode => TablePrefix;

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					fRequiredDocuments.Load();
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fRequiredDocuments;

		public BusinessObject UltimateDocumentParent => this;

		public IReadOnlyList<ZString> AdditionalRefTypes => Array.Empty<ZString>();

		public void PreLogAllDocumentsReceivedEvents()
		{
		}

		#endregion

		public void LockFileIfEnabledByConfiguration(ZString reference, ZString declarationType)
		{
			if (CanLockUnlockDeclaration(declarationType))
			{
				LockFile(reference);
			}
		}

		public void UnlockFileIfEnabledByConfiguration(ZString reference, ZString declarationType)
		{
			if (CanLockUnlockDeclaration(declarationType))
			{
				UnlockFile(reference);
			}
		}

		#region ICusInBondContainerTypeSupporter

		public Type ContainerType => GetContainerType();

		protected Type GetContainerType() => IsPhase5Arrival ? ArrivalContainerTypeCore : DepartureContainerTypeCore;
		protected virtual Type ArrivalContainerTypeCore => typeof(NctsArrivalHeaderContainer);
		protected virtual Type DepartureContainerTypeCore => typeof(NctsDepartureHeaderContainer);

		#endregion

		#region IWarehouseIntegrationSupporter

		void IWarehouseIntegrationSupporter.DoActionOnOutwardAccepted(BusinessObject job)
		{
		}

		void IWarehouseIntegrationSupporter.DoActionOnOutwardCanceled(BusinessObject job)
		{
		}

		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseInwardAction()
		{
		}

		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseOutwardAction()
		{
		}

		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseInwardAction()
		{
		}

		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseOutwardAction()
		{
		}

		ZString IWarehouseIntegrationSupporter.GetMessageErrorOfRequiredFieldsForBondedWarehousing(bool checkProduct, bool checkQuantity,
			bool checkEntryDetails)
		{
			return ZString.Empty;
		}

		ZString IWarehouseIntegrationSupporter.EntryNumber => MovementReferenceNumber;

		ZGuid IWarehouseIntegrationSupporter.ClientPK => Importer?.OA_OH ?? ZGuid.Empty;

		OrgAddress IWarehouseIntegrationSupporter.WarehouseAddress => CommonMovementHeader.WarehouseAddress;

		ZString IWarehouseIntegrationSupporter.WarehouseTransactionStatus
		{
			get { return CommonMovementHeader.BM_WarehouseTransactionStatus; }
			set { CommonMovementHeader.BM_WarehouseTransactionStatus = value; }
		}

		bool IWarehouseIntegrationSupporter.IsActive => false;

		bool IWarehouseIntegrationSupporter.SupportsBondedWarehousing => Configuration.IsBondedWarehouseSupported;

		bool IWarehouseIntegrationSupporter.SupportModificationState => false;

		bool IWarehouseIntegrationSupporter.IsBondedWarehousingDisabled => false;

		bool IWarehouseIntegrationSupporter.IsOutwardBondedWarehousingEnabled => Configuration.IsBondedWarehouseSupported;

		bool IWarehouseIntegrationSupporter.IsInwardBondedWarehousingEnabled => false;

		bool IWarehouseIntegrationSupporter.IsChangeOfOwnershipBondedWarehousingEnabled => false;

		bool IWarehouseIntegrationSupporter.IsChangeOfRegimeWarehousingEnabled => false;

		bool IWarehouseIntegrationSupporter.HasManualWhsUpdate
		{
			get => false;
			set { }
		}
		bool IWarehouseIntegrationSupporter.IsIntoTemporaryImportEnabled => false;

		bool IWarehouseIntegrationSupporter.IsOutOfTemporaryImportEnabled => false;

		bool IWarehouseIntegrationSupporter.IsIntoTemporaryExportEnabled => false;

		bool IWarehouseIntegrationSupporter.IsOutOfTemporaryExportEnabled => false;

		bool IWarehouseIntegrationSupporter.IsIntoInwardProcessingEnabled => false;

		bool IWarehouseIntegrationSupporter.IsOutOfInwardProcessingEnabled => false;

		bool IWarehouseIntegrationSupporter.IsIntoOutwardProcessingEnabled => false;

		bool IWarehouseIntegrationSupporter.IsOutOfOutwardProcessingEnabled => false;

		void IWarehouseIntegrationSupporter.UpdateHoldData(Shipment shipment, RecipientRoleType recipientRoleType) { }

		#endregion

		#region Message Initiator

		public ISendsMessagesToCustoms MessageInitiator
		{
			get
			{
				if (fMessageInitiator == null)
				{
					throw new ApplicationException("You can't perform this action that results in a message being sent because you have not hooked up a ISendsMessagesToCustoms to the NctsHeader");
				}

				return fMessageInitiator;
			}
			set { fMessageInitiator = value; }
		}

		public bool HasMessageInitiator
		{
			get { return fMessageInitiator != null; }
		}

		protected ISendsMessagesToCustoms fMessageInitiator;

		#endregion

		public IEnumerable<NctsCommonCargoDesc> GetGoodsItems()
		{
			if (IsDepartureMovement)
			{
				return DepartureGoodsItems;
			}

			if (!IsArrivalMovement)
			{
				return Array.Empty<NctsCommonCargoDesc>();
			}

			return IsPhase5
				? Bills.SelectMany(b => b.ArrivalGoodsItems).ToArray()
				: ArrivalMovementHeader.GoodsItems.Cast<NctsCommonCargoDesc>().ToArray();
		}

		public void SetAllUnloadedStateToDEC() => SetAllUnloadedStateToDECCore();

		protected virtual void SetAllUnloadedStateToDECCore()
		{
			var bills = Bills.Cast<IUnloadedStatusSupporter>();
			bills.Where(y => NctsHelper.IsUnloadedStateAccepted(y.UnloadedStatus)).ForEach(x => x.SetAllUnloadedStateToDEC());
			var billsToDelete = new List<IUnloadedStatusSupporter>();
			billsToDelete.AddRange(bills.Where(y => y.UnloadedStatus == NctsUnloadedStateList.Codes.NEW));
			billsToDelete.ForEach(x => x.Delete());
		}

		public bool ExistNonDECEntry => ExistNonDECEntryCore;

		protected virtual bool ExistNonDECEntryCore => Bills.Cast<IUnloadedStatusSupporter>().Any(x => x.HasNonDECUnloadedItem());

		public NctsDepartureCargoDesc[] GetDepartureGoodsItems()
		{
			if (IsDepartureMovement)
			{
				return IsPhase5
					? Bills.SelectMany(b => b.GoodsItems).ToArray()
					: MovementHeader.GoodsItems.ToArray();
			}

			return Array.Empty<NctsDepartureCargoDesc>();
		}

		#region EffectiveMessageStatus

		/// <summary>
		/// Contains Message Status depending on NCTS Phase
		/// </summary>
		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.NctsMessageStatusList))]
		[ReadOnly(true)]
		[ResourceStringData("F503B3DB-9E24-477D-AAA6-B475D4EFD4CF", Caption = "Messaging Status", ShortCaption = "Msg. St.", MultipleKey = Phase4CaptionKey)]
		[ResourceStringData("C00B4BAC-EF18-4BE6-BA92-3CB556E0679F", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Msg. Stat.", MultipleKey = Phase5CaptionKey)]
		public virtual ZString EffectiveMessageStatus
		{
			get
			{
				if (IsPhase5)
				{
					return CommonMovementHeader.BM_MessageStatus;
				}
				else
				{
					return BH_MessageStatus;
				}
			}
			set
			{
				if (IsPhase5)
				{
					CommonMovementHeader.BM_MessageStatus = value;
				}
				else
				{
					BH_MessageStatus = value;
				}
			}
		}

		public ZPropertyInfo EffectiveMessageStatusInfo => GetWrappedZPropertyInfo(nameof(EffectiveMessageStatus), x => IsPhase5 ? CommonMovementHeader.BM_MessageStatusInfo : BH_MessageStatusInfo);

		public static ZDBOnlySubQuery GetEffectiveMessageStatusSubQuery(object messageStatus)
		{
			var cusInBondHeaderMessageStatusQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			AddEffectiveMessageStatusFilter(cusInBondHeaderMessageStatusQuery, messageStatus);
			return cusInBondHeaderMessageStatusQuery;
		}

		public static ZDBOnlyQuery GetEffectiveMessageStatusQuery(object messageStatus)
		{
			var cusInBondHeaderMessageStatusQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			AddEffectiveMessageStatusFilter(cusInBondHeaderMessageStatusQuery, messageStatus);
			return cusInBondHeaderMessageStatusQuery;
		}

		static void AddEffectiveMessageStatusFilter(ZDBOnlyQuery query, object messageStatus)
		{
			var cusInBondHeaderNonPhase5Query = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			cusInBondHeaderNonPhase5Query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, SQLComparisonOperator.NotEqual, CusInBondApplicationCodeList.Codes.NCTS5);
			cusInBondHeaderNonPhase5Query.AddToFilter(CusInBondHeaderSchema.BH_MessageStatus, messageStatus);

			var cusInBoundHeaderPhase5Query = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			cusInBoundHeaderPhase5Query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS5);
			var cusInBoundMoveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			cusInBoundMoveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_MessageStatus, messageStatus);
			cusInBoundHeaderPhase5Query.AddSubQuery(cusInBoundMoveHeaderQuery, JoinCondition.And);

			query.AddSubQuery(cusInBondHeaderNonPhase5Query, JoinCondition.Or);
			query.AddSubQuery(cusInBoundHeaderPhase5Query, JoinCondition.Or);
		}

		#endregion

		public void CancelWarehouseIfNeeded(Action<PublishToUniversalResult> publishResult = default)
		{
			IWarehouseIntegrationSupporter warehouseIntegrationSupporter = this;
			if (warehouseIntegrationSupporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardCreated ||
				warehouseIntegrationSupporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardCreatedPending ||
				warehouseIntegrationSupporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardUpdated ||
				warehouseIntegrationSupporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardUpdatedPending)
			{
				Factory.Saved -= CancelWarehouse;
				Factory.Saved += CancelWarehouse;
			}

			void CancelWarehouse(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					factory.Saved -= CancelWarehouse;
				}

				var result = this.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);
				if (result.ResultType == UniversalResult.HadErrors && publishResult != null)
				{
					publishResult(result);
				}
			}
		}

		public void ResetGoodsItemNumbersWhenPhase5()
		{
			if (IsPhase5 && ShouldResetGoodsItemNumbers)
			{
				Bills.ForEach(b => b.GoodsItems.ForEach(i => i.BY_DeclarationGoodsItemNumber = 0));
			}
		}

		public bool ShouldResetGoodsItemNumbers { get; set; } = true;

		public INctsGuaranteeCollection<NctsGuarantee> GetEffectiveGuarantees() => IsPhase5Departure ? MovementHeader.Guarantees : Guarantees;

		string ITypeDeciderContext.Country => CountryCode;

		public ZBool IsInPhase5TransitionPeriod => FuncsHelper.IsFunctionalityValid(Constants.FunctionalityTypes.NCTSTransitionPeriod, NCTSPhase5TransitionPeriodEffectiveDate, options: FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN);

		protected virtual ZDateTime NCTSPhase5TransitionPeriodEffectiveDate => ZDateTime.Today;

		public CusGoodsLocation CusGoodsLocation
		{
			get
			{
				if (IsArrivalMovement)
				{
					return ArrivalMovementHeader.GoodsLocation;
				}

				return IsDepartureMovement ? MovementHeader.GoodsLocation : null;
			}
		}

		public BusinessObject GetEntityToValidate(string triggerAction)
		{
			return this;
		}

		public ZBool SupportValidateCustomsMessaging => AutoMessageSendingHelper.SupportsNctsAutomaticMessageSending(CountryCode);

		public void SynchronizeMonetaryValue()
		{
			if (IsPhase5Departure && MovementHeader.BM_ValuationDate.IsEmpty)
			{
				var originalMonetaryValue = 0m;
				var actualMonetaryValue = 0m;
				Bills.SelectMany(b => b.GoodsItems).ForEach(gi =>
				{
					originalMonetaryValue += gi.BY_MonetaryValue;
					gi.UpdateMonetaryValue();
					actualMonetaryValue += gi.BY_MonetaryValue;
				});

				if (originalMonetaryValue != actualMonetaryValue)
				{
					MovementHeader.Guarantees.Where(g => g.PW_Override).ForEach(g =>
					{
						g.PW_Status = NctsGuarantee.DirtyStatus;
					});
				}
			}
		}
	}
}
