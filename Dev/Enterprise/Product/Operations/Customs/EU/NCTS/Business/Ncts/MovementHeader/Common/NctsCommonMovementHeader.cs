using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsCommonMovementHeader : CusInBondMoveHeader
		, IWorkflowTriggerFieldChangeSource
		, IWorkflowTriggerEventSource
		, IDocAddresses
		, ISequenceNumberHeader
		, ICusInBondCargoDescTypeProvider
		, ISupportMultipleResourceStringData
		, INctsCusInBondCargoDescMaster
		, ICanSupportPhase5
		, ILRNGenerator
		, IEuOfficeCodeProvider
		, ICusCodeDataTypeSupporter
	{
		protected NctsCommonMovementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondMoveHeader.Schema
		{
			public const string IsSimplifiedNctsProcedure = nameof(NctsCommonMovementHeader.IsSimplifiedNctsProcedure);
			public const string TotalNumberOfPackages = nameof(NctsCommonMovementHeader.TotalNumberOfPackages);
			public const string TotalNumberOfItems = nameof(NctsCommonMovementHeader.TotalNumberOfItems);
			public const string TotalGrossMassInKilograms = nameof(NctsCommonMovementHeader.TotalGrossMassInKilograms);
			public const string PhaseStatusDescription = nameof(NctsCommonMovementHeader.PhaseStatusDescription);
			public const string DestinationCustomsOfficeCode = nameof(NctsHeader.DestinationCustomsOfficeCode);
			public const string DestinationCustomsOfficeCodeForArrival = nameof(NctsArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival);
			public const string DestinationCustomsOfficeCodeForDeparture = nameof(NctsDepartureMovementHeader.DestinationCustomsOfficeCodeForDeparture);
			public const string DepartureCustomsOfficeCode = nameof(NctsDepartureMovementHeader.DepartureCustomsOfficeCode);
			public const string EnquiryCustomsOfficeCode = nameof(NctsHeader.EnquiryCustomsOfficeCode);
		}

		#region Construction / Loading

		public static T LoadOrCreate<T>(NctsHeader parent, ZString movementType) where T : NctsCommonMovementHeader
		{
			return Load<T>(parent, movementType) ?? New<T>(parent);
		}

		static T Load<T>(NctsHeader parent, ZString movementType) where T : NctsCommonMovementHeader
		{
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, parent.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, movementType);
			query.OrderBy = CusInBondMoveHeaderSchema.BM_SystemCreateTimeUtc.Name;
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			return parent.Factory.LoadTop1<T>(query);
		}

		static T New<T>(NctsHeader parent) where T : NctsCommonMovementHeader
		{
			var result = parent.Factory.New<T>(parent);
			using (result.SuspendSettingHasChanges())
			{
				result.BM_BH = parent.PK;
				result.SetDefaultValuesAfterNctsHeaderIsSet(parent);
			}
			return result;
		}

		protected virtual void SetDefaultValuesAfterNctsHeaderIsSet(NctsHeader parent)
		{
		}

		#endregion

		public new static readonly NctsCommonMovementHeaderTypeDecider TypeDecider = new NctsCommonMovementHeaderTypeDecider();

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new NctsCommonMovementHeaderLookups Lookups => (NctsCommonMovementHeaderLookups)base.Lookups;

		public INctsMovementHeaderValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsMovementHeaderValidationDecider> validationDeciderCached;

		INctsMovementHeaderValidationDecider GetValidationDecider() => Header?.Configuration.MovementHeaderConfiguration.GetValidationDecider(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (BM_GrossWeightUQ.IsEmpty)
			{
				BM_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			}
		}

		[BusinessObjectTestExclude]
		[LightValidationTestExempt]
		public override ZGuid BM_BH
		{
			get => base.BM_BH;
			set => base.BM_BH = value;
		}

		[BusinessObjectTestExclude]
		[LightValidationTestExempt]
		public override ZString BM_SubApplicationCode
		{
			get => base.BM_SubApplicationCode;
			set => base.BM_SubApplicationCode = value;
		}

		[ResourceStringData("956FBA84-8219-4CD8-8167-B965B3E629A8", Caption = "Reduced Dataset Indicator", MediumCaption = "Reduced Dataset Ind.")]
		public override ZBool BM_ReducedDatasetIndicator
		{
			get => base.BM_ReducedDatasetIndicator;
			set => base.BM_ReducedDatasetIndicator = value;
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.NctsMovementHeaderTransactionStatusList))]
		[ResourceStringData("36162248-F9EE-4594-850F-580B2817D6B2", Caption = "Phase Status", ShortCaption = "Phase")]
		public override ZString BM_Phase
		{
			get => base.BM_Phase;
			set => base.BM_Phase = value;
		}

		[ResourceStringData("65878E40-8B68-47B6-85F8-B31682706A98", Caption = "Phase Status Description", ShortCaption = "Ph. Desc.")]
		public ZString PhaseStatusDescription
		{
			get
			{
				var phase = BM_Phase;
				var description = ZString.Empty;
				if (!phase.IsEmpty)
				{
					description = Lookups.NctsMovementHeaderTransactionStatusList.GetDescriptionFromCode(phase);
				}
				return description;
			}
		}

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.DeclarationTypeList))]
		[ResourceStringData("D08B8C94-C4BB-4358-9844-16F4E54D5609", Caption = "[1] Declaration Type", ShortCaption = "Dec. Ty.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("BDC27A04-60F0-4147-ACA6-301395073CAD", Caption = "Declaration Type", MediumCaption = "Dec. Type", ShortCaption = "Type", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_InBondEntryType
		{
			get => base.BM_InBondEntryType;
			set => base.BM_InBondEntryType = value;
		}

		[MaxLength(nameof(BM_PlaceOfUnloadingMaxLength))]
		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.PortsOfUnloading))]
		[ResourceStringData("03F9B547-C71E-4714-BAF4-1E61A7B8136E", Caption = "Place of Unloading Code", ShortCaption = "Unloading", FullDescription = "Place of Unloading (code)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("4D0ED5E9-AF38-4FF9-81D5-D31E597F0EFE", Caption = "Place of Unloading", ShortCaption = "Unloading", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_PlaceOfUnloading
		{
			get => base.BM_PlaceOfUnloading;
			set => base.BM_PlaceOfUnloading = value;
		}

		protected virtual int BM_PlaceOfUnloadingMaxLength => IsPhase5 ? AutoCusInBondMoveHeader.Schema.BM_PlaceOfUnloadingMaxLength : 5;

		[ResourceStringData("988F7716-9695-45BE-8C6A-77005981BADF", Caption = "Place of Loading", ShortCaption = "Loading")]
		public override ZString BM_PlaceOfLoading
		{
			get => base.BM_PlaceOfLoading;
			set => base.BM_PlaceOfLoading = value;
		}

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.CountryOfDestinationList))]
		[ResourceStringData("54035035-726B-44F3-8E84-FC33D2085DFC", Caption = "[17A] Country/Region of Destination", ShortCaption = "Dest.", FullDescription = "Destination Country/Region", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("C696E86E-262F-4E98-B727-CC044A0A1E3A", Caption = "Country/Region of Destination", MediumCaption = "Destination Ctry./Rgn.", ShortCaption = "Destin. Ctry./Rgn.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_RL_NKDestinationPort
		{
			get => base.BM_RL_NKDestinationPort;
			set
			{
				var oldValue = BM_RL_NKDestinationPort;
				base.BM_RL_NKDestinationPort = value;
				if (!IsCopying && oldValue != BM_RL_NKDestinationPort && !IsMarkingAsNeedingValidationSuspended)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("FDFAD897-00C4-4BCC-A9BC-2905804FD4ED", Caption = "Reference Number / UCR", MediumCaption = "Reference No. / UCR", ShortCaption = "Ref. No. / UCR")]
		public override ZString BM_UniqueConsignmentReference
		{
			get => base.BM_UniqueConsignmentReference;
			set
			{
				var oldValue = BM_UniqueConsignmentReference;
				base.BM_UniqueConsignmentReference = value;

				if (!IsCopying && oldValue != BM_UniqueConsignmentReference)
				{
					if (!IsValidationSuspended && IsPhase5)
					{
						Header?.Bills.Where(bill => bill.B0_ReferenceID.IsEmpty).ForEach(bill => bill.ValidateGoodsItemsMandatoryCommercialReferenceNumber());
						GoodsItems.MarkAsNeedingValidation();
					}
				}
			}
		}

		[MaxLength(70)]
		public override ZString BM_AdditionalText
		{
			get => base.BM_AdditionalText;
			set => base.BM_AdditionalText = value;
		}

		[MaxLength(2)]
		public override ZString BM_GONumber
		{
			get => base.BM_GONumber;
			set
			{
				if (BM_GONumber != value)
				{
					base.BM_GONumber = value;
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						if (IsPhase5)
						{
							CustomsOffices.MarkAsNeedingValidation();
						}
						else
						{
							Header?.CustomsOffices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		[MaxLength(27)]
		[ResourceStringData("129FD015-FB2F-412E-8A7D-2E5F82C232F6", Caption = "[18] Transport ID (Departure)", ShortCaption = "Dept. Transp. ID", FullDescription = "Departure Transport Identification", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("1688A72C-15C3-4378-B5B8-092B8C4823BE", Caption = "Transport Identification", MediumCaption = "Transport ID", ShortCaption = "Trans. ID", MultipleKey = NctsHeader.Phase5CaptionKey)]

		public override ZString BM_TransportAtDeparture
		{
			get => base.BM_TransportAtDeparture;
			set => base.BM_TransportAtDeparture = value;
		}

		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.TransportNationalityList))]
		[ResourceStringData("0D1D72FC-C6DF-476B-A444-B9F6A3D6CA36", Caption = "[18] Transport Nationality (Departure)", ShortCaption = "Dept. Nat.", FullDescription = "Departure Transport Nationality", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("CBD4F94D-B562-413D-A7AC-8FC04AE4A488", Caption = "Transport Nationality", MediumCaption = "Trans. Nationality", ShortCaption = "Nationality", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_RN_NKTransportAtDepartureCountry
		{
			get => base.BM_RN_NKTransportAtDepartureCountry;
			set
			{
				base.BM_RN_NKTransportAtDepartureCountry = value;
				Header?.MarkAsNeedingValidation();
			}
		}

		[ReadOnlyMember(nameof(BM_PaperlessInbondNumReadOnly))]
		[ResourceStringData("243E529B-606C-4595-B6B4-82A27F52DA47", Caption = "Customer Reference", MediumCaption = "Customer Ref.", ShortCaption = "LRN")]
		public override ZString BM_PaperlessInbondNum
		{
			get => base.BM_PaperlessInbondNum;
			set
			{
				base.BM_PaperlessInbondNum = value;
				Header?.MarkAsNeedingValidation();
			}
		}

		protected virtual bool BM_PaperlessInbondNumReadOnly => false;

		[ResourceStringData("C8928E18-BC71-4702-8DC2-A319E8EDFFBA", Caption = "Time Limit For Transit", ShortCaption = "Time Limit")]
		public override ZShort BM_ExportTimeLimit
		{
			get => base.BM_ExportTimeLimit;
			set => base.BM_ExportTimeLimit = value;
		}

		public override ZString BM_BTAIndicator
		{
			get => base.BM_BTAIndicator;
			set
			{
				if (BM_BTAIndicator != value)
				{
					base.BM_BTAIndicator = value;
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						if (IsPhase5)
						{
							CustomsOffices.MarkAsNeedingValidation();
						}
						else
						{
							Header?.CustomsOffices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public virtual ZBool IsSimplifiedNctsProcedure
		{
			get => BM_GONumber == NctsControlResult.Codes.AuthorizedTrader;
			set
			{
				var oldValue = IsSimplifiedNctsProcedure;
				BM_GONumber = value ? NctsControlResult.Codes.AuthorizedTrader : string.Empty;
				IsSimplifiedNctsProcedureInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsSimplifiedNctsProcedureInfo => GetWrappedZPropertyInfo(nameof(IsSimplifiedNctsProcedure), x => BM_GONumberInfo);

		public virtual ZLong TotalNumberOfPackages => GoodsItems.SelectMany(item => item.Packages.Cast<NctsPackage>()).Aggregate<NctsPackage, ZLong>(0, (current, package) => current + (package.IsBulk ? (package.B5_UnitCount == 0 ? 1 : package.B5_UnitCount) : package.B5_UnitCount));

		public ZPropertyInfo TotalNumberOfPackagesInfo => GetZPropertyInfo(nameof(TotalNumberOfPackages));

		public virtual ZInt TotalNumberOfItems => GoodsItems.Count;

		public ZPropertyInfo TotalNumberOfItemsInfo => GetZPropertyInfo(nameof(TotalNumberOfItems));

		public virtual ZDecimal TotalGrossMassInKilograms => GoodsItems.Sum(gi => gi.GrossMassInKilograms);

		public ZPropertyInfo TotalGrossMassInKilogramsInfo => GetZPropertyInfo(nameof(TotalGrossMassInKilograms));

		public bool IsArrivalMovementHeader => BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Arrival;

		public bool IsDepartureMovementHeader => BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Departure;

		public bool IsUnloadingMovementHeader => BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Unloading;

		public ZString DefaultDataGroupingCode => Header?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public IReadOnlyList<IWorkflowProvider> ParentWorkflowProviders => new IWorkflowProvider[] { Header };

		public override void Delete()
		{
			GoodsItems.DeleteAll();
			base.Delete();
		}

		public bool HasGoodsItemsWithCountryOfDispatch => Factory.GetValue(ref hasGoodsItemsWithCountryOfDispatch, () => GoodsItems.Any(x => !x.BY_RN_NKCountryOfDispatch.IsEmpty));
		CachedProperty<bool> hasGoodsItemsWithCountryOfDispatch;

		public bool HasGoodsItemsWithCountryOfDestination => Factory.GetValue(ref hasGoodsItemsWithCountryOfDestination, () => GoodsItems.Any(x => !x.BY_RN_NKCountryOfDestination.IsEmpty));
		CachedProperty<bool> hasGoodsItemsWithCountryOfDestination;

		[ChildEditable]
		public INctsCommonCargoDescCollection<NctsCommonCargoDesc> GoodsItems
		{
			get
			{
				if (goodsItems == null)
				{
					goodsItems = CreateGoodsItems();
					RegisterEditableChildObject(goodsItems);
				}
				return goodsItems;
			}
		}
		INctsCommonCargoDescCollection<NctsCommonCargoDesc> goodsItems;

		protected virtual INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsCommonCargoDescCollection<NctsCommonCargoDesc>(this);

		[BusinessObjectTestExclude]
		public new ICusInBondMoveDetailCollection MovementDetails => base.MovementDetails;

		public override ZString BM_MessageStatus
		{
			get => base.BM_MessageStatus;
			set
			{
				base.BM_MessageStatus = value;
				if (IsPhase5)
				{
					Header?.EffectiveMessageStatusInfo.RefreshBinding();
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			LogCustomsStatusIfRequired();
			LogMessageStatusIfRequired();
		}

		void LogCustomsStatusIfRequired()
		{
			if (!PK.IsEmpty && ((ZString)BM_CustomsStatusInfo.OriginalValue != BM_CustomsStatus || (!BM_CustomsStatus.IsEmpty && !IsInDatabase)))
			{
				LogCustomsStatus(BM_CustomsStatus);

				CustomsEntryStatusLogAdded?.Invoke(this, null);
			}
		}

		public void LogCustomsStatus(ZString customsStatus)
		{
			var logs = Header.IsPhase5 ? Logs : Header.Logs;
			logs.AddNew(AutoEvents.CustomsEntryStatus, customsStatus, ZDateTimeOffset.Now);
		}

		public EventHandler CustomsEntryStatusLogAdded;

		protected override ICusInBondMoveDetailCollection CreateMovementDetails() => null;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new string[] { Schema.BM_BM_DepartureMovement });
			var newMovementHeader = (NctsCommonMovementHeader)base.CloneInternal(args);
			foreach (var goodsItem in GoodsItems.ToArray())
			{
				var newItem = (NctsCommonCargoDesc)new NctsDeepCloneStrategy(goodsItem, newMovementHeader.PK).Clone();
				newMovementHeader.GoodsItems.Add(newItem);
			}
			return newMovementHeader;
		}

		protected override bool SupportsCloneCore() => true;

		bool ShouldAutoGenerateLocalReferenceNumber => (BM_PaperlessInbondNumReadOnly || NeedsRegenerateLocalReferenceNumber) && (BM_PaperlessInbondNum.IsEmpty || ForceRegenerateLocalReferenceNumber);

		public bool ShouldGenerateLocalReferenceNumberOnSaving => ShouldGenerateLocalReferenceNumberOnSavingCore && ShouldAutoGenerateLocalReferenceNumber;
		protected virtual bool ShouldGenerateLocalReferenceNumberOnSavingCore => false;

		public override void OnSaving()
		{
			if (ShouldGenerateLocalReferenceNumberOnSaving && !ShouldGenerateLocalReferenceNumberOnFactorySaving)
			{
				GenerateLocalReferenceNumber();
			}
			base.OnSaving();
		}

		bool localReferenceNumberAutoGenerated;

		void GenerateLocalReferenceNumber()
		{
			PopulateNumberPropertyIfRequired(BM_PaperlessInbondNumInfo, factory => GenerateLocalReferenceNumberCore(), ignoreInDatabaseCheck: GenerateLocalReferenceNumberIgnoreInDatabaseCheck, forceRegenerate: ForceRegenerateLocalReferenceNumber || NeedsRegenerateLocalReferenceNumber);
			localReferenceNumberAutoGenerated = true;
		}

		protected virtual ZString GenerateLocalReferenceNumberCore() => LRNGeneratorHelper.GenerateLocalReferenceNumber(this);

		internal bool GenerateLocalReferenceNumberIgnoreInDatabaseCheck => Header.Configuration.UseLocalReferenceNumberIgnoreInDatabaseCheck;

		protected virtual bool ForceRegenerateLocalReferenceNumber => false;

		protected bool NeedsRegenerateLocalReferenceNumber { get; set; }

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				RecoverFromUnsuccessfulSave();
			}

			localReferenceNumberAutoGenerated = false;
			NeedsRegenerateLocalReferenceNumber = false;
			base.OnSaved(saveSucceeded);
		}

		void RecoverFromUnsuccessfulSave()
		{
			if (localReferenceNumberAutoGenerated)
			{
				if (IsInDatabase)
				{
					BM_PaperlessInbondNum = (ZString)BM_PaperlessInbondNumInfo.OriginalValue;
				}
				else
				{
					BM_PaperlessInbondNum = ZString.Empty;
				}
			}
		}

		public bool ShouldGenerateLocalReferenceNumberOnFactorySaving => ShouldGenerateLocalReferenceNumberOnFactorySavingCore && ShouldAutoGenerateLocalReferenceNumber;
		protected virtual bool ShouldGenerateLocalReferenceNumberOnFactorySavingCore => false;

		protected override void OnFactorySaving()
		{
			if (ShouldGenerateLocalReferenceNumberOnFactorySaving)
			{
				GenerateLocalReferenceNumber();
			}
			base.OnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				RecoverFromUnsuccessfulSave();
			}
			base.OnFactorySaved(saveSucceeded);
		}

		protected override Type MovementDetailTypeCore => null;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override Type GoodsLocationTypeCore => CusGoodsLocation.TypeDecider.GetTypeForCountryCode(DefaultDataGroupingCode);

		public bool IsPhase5 => Header?.IsPhase5 ?? false;

		public bool IsPhase4 => Header?.IsPhase4 ?? false;

		public bool IsPhase5Arrival => Header?.IsPhase5Arrival ?? false;

		public bool IsPhase5Departure => Header?.IsPhase5Departure ?? false;

		public bool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;

		public JobDocAddressManager JobDocAddressManager
		{
			get { return jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager()); }
		}
		JobDocAddressManager jobDocAddressManager;

		void LogMessageStatusIfRequired()
		{
			if (!PK.IsEmpty && ((ZString)BM_MessageStatusInfo.OriginalValue != BM_MessageStatus || (!BM_MessageStatus.IsEmpty && !IsInDatabase)))
			{
				var log = Logs.AddNew(Events.MessageStatusChange, BM_MessageStatus, ZDateTimeOffset.Now);
				MessageStatusLogAdded?.Invoke(this, new CustomsStatusLogAddedEventArgs(this, log));
			}
		}

		public event EventHandler<CustomsStatusLogAddedEventArgs> MessageStatusLogAdded;

		#region Representative

		[ResourceStringData("050964BE-D305-4D51-BC0A-8ADE8089C4CB", Caption = "[50] Representative", ShortCaption = "[50] Rep.")]
		public JobDocAddress Representative
		{
			get
			{
				if (representativeJobDocAddress == null || representativeJobDocAddress.IsDeleted)
				{
					if (representativeJobDocAddress != null)
					{
						representativeJobDocAddress.DocAddressChanged -= new EventHandler(RepresentativeJobDocAddressChanged);
					}
					representativeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(RepresentativeJobDocAddressRequirement);
					representativeJobDocAddress.DefaultContactAllocationType = OrgConstants.ContactAllocationType.CUS;
					representativeJobDocAddress.DocAddressChanged += new EventHandler(RepresentativeJobDocAddressChanged);
					representativeJobDocAddress.AdditionalValidation = GetRepresentativeJobDocAddressAdditionalValidation(representativeJobDocAddress);
				}
				representativeJobDocAddress.SetReadOnlyIncludingChildren(IsRepresentativeReadOnly);
				return representativeJobDocAddress;
			}
		}
		JobDocAddress representativeJobDocAddress;

		protected virtual ZValidation GetRepresentativeJobDocAddressAdditionalValidation(JobDocAddress representativeJobDocAddress) => null;

		public JobDocAddressRequirement RepresentativeJobDocAddressRequirement
		{
			get
			{
				if (representativeJobDocAddressRequirement == null)
				{
					representativeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Representative);
					JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(representativeJobDocAddressRequirement);
					representativeJobDocAddressRequirement.ValidateContact += ValidateRepresentativeContact;
					representativeJobDocAddressRequirement.CanOverride = RepresentativeJobDocAddressCanOverride;
					representativeJobDocAddressRequirement.ValidateOrganisationPK = ValidateRepresentative;
					JobDocAddressManager.AddRequirement(representativeJobDocAddressRequirement);
					representativeJobDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateRepresentativeAddress;
				}
				return representativeJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement representativeJobDocAddressRequirement;

		protected virtual void ValidateRepresentative(JobDocAddressValidation validation)
		{
			Header.CheckConditionR0520(Representative.E2_OA_AddressInfo, Representative.OrganisationPKInfo);
			Header.CheckRepresentative_R0850_1();
		}

		protected virtual void ValidateRepresentativeAddress(JobDocAddressValidation validation)
		{
			validation.ValidateOrganisationPK();
			Header.CheckConditionR0520(Representative.E2_OA_AddressInfo);
		}

		void RepresentativeJobDocAddressChanged(object sender, EventArgs e)
		{
			OnChangedRepresentativeJobDocAddressRequirement();
		}

		protected virtual void OnChangedRepresentativeJobDocAddressRequirement() { }

		protected virtual bool RepresentativeJobDocAddressCanOverride => true;

		protected virtual void ValidateRepresentativeContact(JobDocAddressValidation validation)
		{
		}

		protected virtual bool IsRepresentativeReadOnly => false;

		#endregion

		#region IDocAddresses Members

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (jobDocAddressDependentCollection == null)
				{
					jobDocAddressDependentCollection = new JobDocAddressDependentCollection(this);
					jobDocAddressDependentCollection.Load();
					RegisterEditableChildObject(jobDocAddressDependentCollection);
				}
				return jobDocAddressDependentCollection;
			}
		}
		JobDocAddressDependentCollection jobDocAddressDependentCollection;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => GetJobDocAddressValidation(addressToValidate);

		protected virtual ZValidation GetJobDocAddressValidation(JobDocAddress addressToValidate) => new NctsJobDocAddressValidation(addressToValidate, Header);

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Environment.Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => IDocAddressesGetDocAddressRequirementCore(addressType);

		protected virtual JobDocAddressRequirement IDocAddressesGetDocAddressRequirementCore(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Representative:
					return RepresentativeJobDocAddressRequirement;
				default:
					return null;
			}
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => true;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => new OrgHeaderCollection(Factory);

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => IDocAddressesSupportedAddressTypesCore;

		protected virtual IReadOnlyList<DocAddressType> IDocAddressesSupportedAddressTypesCore => new DocAddressType[] { DocAddressType.Representative };

		#endregion

		#region ICusInBondCargoDescTypeProvider Implementation

		public Type CusInBondCargoDescType => CusInBondCargoDescTypeCore;

		protected abstract Type CusInBondCargoDescTypeCore { get; }

		#endregion

		#region ISequenceNumberHeader Implementation

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(GoodsItems);

		#endregion

		#region  INctsCusInBondCargoDescMaster Implementation

		public ShortSequenceNumberGenerator LineNumberGenerator => lineNumberGenerator ?? (lineNumberGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator lineNumberGenerator;

		#endregion

		#region ISupportMultipleResourceStringData Implementation

		public IReadOnlyList<string> MultipleKeysToUse => MultipleKeysToUseCore;

		protected virtual IReadOnlyList<string> MultipleKeysToUseCore => Header?.MultipleKeysToUse ?? Array.Empty<string>();

		#endregion

		#region ILRNGenerator Implementation

		protected virtual INumberFountainProxy LrnNumberFountain => Env.NumberFountains.EULocalReferenceNumber(Header.Company.PK.ToGuid());

		INumberFountainProxy ILRNGenerator.LrnNumberFountain => LrnNumberFountain;

		GlbBranch ILRNGenerator.Branch => Header.Branch;

		#endregion

		#region IEuOfficeCodeProvider

		public ZString CountryCode => Header?.CountryCode ?? ZString.Empty;

		public ZBool IsImport => false;

		public ZBool IsExport => false;

		public bool IsNCTS => true;

		public bool IsEMCS => false;

		public CustomsOfficeRequirementHelper CustomsOfficeRequirementHelper => customsOfficeRequirementHelper ?? (customsOfficeRequirementHelper = GetCustomsOfficeRequirementHelper());
		CustomsOfficeRequirementHelper customsOfficeRequirementHelper;

		protected virtual CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new NctsMovementHeaderCustomsOfficeRequirementHelper(this);

		#endregion IEuOfficeCodeProvider

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(NctsEuOfficeCode) },
			};
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}
		#endregion

		IEnumerable<EuOfficeCode> IEuOfficeCodeProvider.CustomsOffices => CustomsOffices.Cast<NctsEuOfficeCode>();

		[ChildEditable(true)]
		public NctsEuOfficeCodeCollection CustomsOffices
		{
			get
			{
				if (customsOffices == null)
				{
					customsOffices = GetNewCustomsOffices();
					customsOffices.Load();
					((IBindingList)customsOffices).ListChanged += CustomsOffices_ListChanged;
					RegisterEditableChildObject(customsOffices);
				}
				else if (CustomsOfficesNeedingReload)
				{
					customsOffices.Reload(false);
					CustomsOfficesNeedingReload = false;
				}
				return customsOffices;
			}
		}
		NctsEuOfficeCodeCollection customsOffices;

		protected bool CustomsOfficesNeedingReload { get; set; }

		protected virtual NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsEuOfficeCodeCollection(this);

		protected void SetOfficeCode(ICustomsOffice destinationCustomsOffice, ZString value)
		{
			if (destinationCustomsOffice is CusCodeData destinationOfficeCusCodeData)
			{
				destinationOfficeCusCodeData.CY_Data = value;
			}
		}

		public ICustomsOffice DestinationCustomsOffice => GetDestinationCustomsOffice();

		protected virtual ICustomsOffice GetDestinationCustomsOffice() => null;

		protected ICustomsOffice AddNewDestinationCustomsOfficeCode(string officeCode)
		{
			var destinationOffice = CustomsOffices.AddNew();
			destinationOffice.CY_Code = officeCode;
			return destinationOffice;
		}

		public ZString[] GetDataGroupsForDestinationOfficeLookup() => GetDataGroupsForDestinationOfficeLookupCore();

		protected virtual ZString[] GetDataGroupsForDestinationOfficeLookupCore() => new ZString[] { DefaultDataGroupingCode };

		public ZString[] GetRolesForDestinationOfficeLookup() => GetRolesForDestinationOfficeLookupCore();

		protected virtual ZString[] GetRolesForDestinationOfficeLookupCore() => IsPhase5 ? new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination } : Array.Empty<ZString>();

		public bool HasExitForTransitOffice(ZString exitForTransitOfficeCountry) => exitForTransitOfficeCountry.IsEmpty
			? ExitForTransitCustomsOfficeCodeList.Count > 0
			: ExitForTransitCustomsOfficeCodeList.Any(x => x.OfficeCode.SubstringSafe(0, 2).EqualsIgnoringCase(exitForTransitOfficeCountry));

		public List<ICustomsOffice> ExitForTransitCustomsOfficeCodeList => GetExitForTransitCustomsOfficeCollection().ToList();

		protected virtual IEnumerable<ICustomsOffice> GetExitForTransitCustomsOfficeCollection()
		{
			return CustomsOffices
				.Cast<NctsEuOfficeCode>()
				.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
		}

		public bool HasTransitOffice(string transitOfficeCountry = null) => string.IsNullOrEmpty(transitOfficeCountry)
			? TransitCustomsOfficeCodeList.Count > 0
			: TransitCustomsOfficeCodeList.Any(x => x.OfficeCode.SubstringSafe(0, 2).EqualsIgnoringCase(transitOfficeCountry));

		[MaxLength(8)]
		public List<ICustomsOffice> TransitCustomsOfficeCodeList => GetTransitCustomsOfficeCollection().ToList();

		protected virtual IEnumerable<ICustomsOffice> GetTransitCustomsOfficeCollection()
		{
			return CustomsOffices
				.Cast<NctsEuOfficeCode>()
				.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		}

		internal CustomsOfficesNumberGenerator OfficeSequenceNumberGenerator => officeLineNumberGenerator ?? (officeLineNumberGenerator = new CustomsOfficesNumberGenerator(this));
		CustomsOfficesNumberGenerator officeLineNumberGenerator;

		public virtual ZString DestinationCustomsOfficeCode => ZString.Empty;

		public ZPropertyInfo DestinationCustomsOfficeCodeInfo => GetZPropertyInfo(Schema.DestinationCustomsOfficeCode);

		[ResourceStringData("437999A9-67B7-4FE3-B67A-3FE6AF81876A", Caption = "Actual Office of Destination", MediumCaption = "Destination Office", ShortCaption = "Dest. Office", FullDescription = "Customs Office of Destination")]
		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.DestinationCustomsOfficeCodeList))]
		[ReadOnlyMember("DestinationCustomsOfficeCodeReadOnly")]
		public ZString DestinationCustomsOfficeCodeForDeparture
		{
			get
			{
				return DestinationCustomsOfficeForDeparture?.OfficeCode ?? ZString.Empty;
			}
			set
			{
				ZString destinationCustomsOfficeCodeForDeparture = DestinationCustomsOfficeCodeForDeparture;
				SetOfficeCode(DestinationCustomsOfficeForDeparture ?? AddNewDestinationCustomsOfficeCode("DES"), value);
				DestinationCustomsOfficeCodeForDepartureInfo.RefreshBinding(destinationCustomsOfficeCodeForDeparture);
			}
		}
		public ZPropertyInfo DestinationCustomsOfficeCodeForDepartureInfo => GetZPropertyInfo(Schema.DestinationCustomsOfficeCodeForDeparture);

		public ICustomsOffice DestinationCustomsOfficeForDeparture => GetDestinationCustomsOfficeForDeparture();

		protected virtual ICustomsOffice GetDestinationCustomsOfficeForDeparture() => NctsEuOfficeCode.Load<NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

		[ResourceStringData("E6CD3A7F-B154-46FF-9A40-D456A5333C60", Caption = "Actual Office of Destination", MediumCaption = "Destination Office", ShortCaption = "Dest. Office", FullDescription = "Country of Customs Office of Destination")]
		public ZString DestinationCustomsOfficeCodeCountryForDeparture => DestinationCustomsOfficeCodeForDeparture.Left(2);

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

		protected virtual void CustomsOffices_ListChanged(object sender, ListChangedEventArgs e)
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

		protected virtual NctsEuOfficeCodeCollectionForDepartureGrid GetNewCustomsOfficesForDeparture() => new NctsEuOfficeCodeCollectionForDepartureGrid(this);

		protected virtual void CustomsOfficesForDeparture_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!CustomsOfficesNeedingReload && CustomsOffices != null)
			{
				var listChangedType = e.ListChangedType;
				if (listChangedType == ListChangedType.ItemAdded || listChangedType == ListChangedType.ItemDeleted || listChangedType == ListChangedType.Reset || listChangedType == ListChangedType.ItemChanged)
				{
					CustomsOfficesNeedingReload = true;
				}
			}
		}

		public ICustomsOffice DepartureCustomsOffice => GetDepartureCustomsOffice();

		protected virtual ICustomsOffice GetDepartureCustomsOffice() => NctsEuOfficeCode.Load<NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		[ResourceStringData("465957FA-AE32-48C1-888D-674524BECFA5", Caption = "Departure Office", FullDescription = "Customs Office of Departure")]
		public ZString DepartureCustomsOfficeCode => DepartureCustomsOffice?.OfficeCode ?? ZString.Empty;

		public ZPropertyInfo DepartureCustomsOfficeCodeInfo => GetZPropertyInfo(Schema.DepartureCustomsOfficeCode);

		[ResourceStringData("9A1F6CB1-89B6-426E-9843-85E14EA98E65", Caption = "Departure Office Country", FullDescription = "Country of Customs Office of Departure")]
		public ZString DepartureCustomsOfficeCodeCountry => DepartureCustomsOfficeCode.Left(2);

		[ResourceStringData("NctsArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival", Caption = "Actual Office of Destination for Arrival", MediumCaption = "Destination Office for Arrival", ShortCaption = "Dest. Office (Arrival)", FullDescription = "Customs Office of Destination for Arrival")]
		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.DestinationCustomsOfficeCodeList))]
		[MaxLength(10)]
		[ReadOnlyMember(nameof(DestinationCustomsOfficeCodeForArrivalReadOnly))]
		public virtual ZString DestinationCustomsOfficeCodeForArrival
		{
			get => DestinationCustomsOfficeForArrival?.OfficeCode ?? ZString.Empty;
			set
			{
				var oldValue = DestinationCustomsOfficeCodeForArrival;
				SetOfficeCode(DestinationCustomsOfficeForArrival ?? AddNewDestinationCustomsOfficeCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival), value);
				DestinationCustomsOfficeCodeForArrivalInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo DestinationCustomsOfficeCodeForArrivalInfo => GetZPropertyInfo(Schema.DestinationCustomsOfficeCodeForArrival);

		public bool DestinationCustomsOfficeCodeForArrivalReadOnly => DestinationCustomsOfficeCodeForArrivalReadOnlyCore;

		protected virtual bool DestinationCustomsOfficeCodeForArrivalReadOnlyCore => Header.IsArrivalNotificationDisabled || Header.IsArrivalDetailsReadOnly;

		public ICustomsOffice DestinationCustomsOfficeForArrival => GetDestinationCustomsOfficeForArrival();

		protected virtual ICustomsOffice GetDestinationCustomsOfficeForArrival()
		{
			return CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
		}

		public ZString DestinationCustomsOfficeCodeCountryForArrival => DestinationCustomsOfficeCodeForArrival.Left(2);

		public NctsEuOfficeCode EnquiryCustomsOffice => NctsEuOfficeCode.Load<NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

		[ResourceStringData("9F7801AE-69B9-4D4D-AEC2-F302697EEF43", Caption = "Enquiry Office", FullDescription = "Customs Office of Enquiry")]
		public ZString EnquiryCustomsOfficeCode => EnquiryCustomsOffice?.CY_Data ?? ZString.Empty;

		public ZPropertyInfo EnquiryCustomsOfficeCodeInfo => GetZPropertyInfo(Schema.EnquiryCustomsOfficeCode);

		public ZWeight GrossWeight => new(BM_GrossWeight, BM_GrossWeightUQ);

		public ZDecimal GrossWeightInKilograms => GrossWeight.InKilogramsSafe;

		#region FetchHints

		public virtual void AddCusGoodsLocationAdditionalIdentifierDescriptionFetchHintsForViewCore()
		{
		}

		#endregion

		#region IWorkflowTriggerEventSource

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders => Factory.GetCached(ref parentWorkflowProviders, GetParentWorkflowProviders);
		CachedProperty<IWorkflowProviderCore[]> parentWorkflowProviders;

		IWorkflowProviderCore[] GetParentWorkflowProviders() => Header is NctsHeader header ? new[] { header } : Array.Empty<IWorkflowProviderCore>();

		public IGlbCompany JobHeaderCompany => ((IWorkflowTriggerEventSource)Header).JobHeaderCompany;

		#endregion

		#region Event Args

		public class CustomsStatusLogAddedEventArgs : EventArgs
		{
			public CustomsStatusLogAddedEventArgs(NctsCommonMovementHeader movementHeader, StmALog log)
			{
				MovementHeader = movementHeader;
				Log = log;
			}

			public NctsCommonMovementHeader MovementHeader { get; }
			public StmALog Log { get; }
		}

		#endregion
	}
}
