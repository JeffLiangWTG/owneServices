using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[DependentBusinessObject(typeof(NctsHeader), nameof(NctsHeader.MovementHeader))]
	[SystemDefinedValues]
	[UserDefinedValues]
	[CodeProperty(NctsDepartureMovementHeader.Schema.BM_PaperlessInbondNum)]
	public class NctsDepartureMovementHeader : NctsCommonMovementHeader
		, Integration.Customs.EU.NCTS.IDepartureMovementHeader
		, ICusCodeDataTypeSupporter
		, ICusGoodsLocationProviderWithValidationDecider
		, ICusGoodsLocationProviderWhichAllowsMixedCase
		, IDepartureTransportMeansProvider
		, IAdditionalTransportMeansProvider
		, INCTSCusAuthorizationUsageMaster
		, ICusReferenceTypeSupporter
		, IWorkflowProvider
		, ICustomFieldProvider
		, IWorkflowAffectedPropertyProvider
		, IDocManagerSupport
		, ICusSupportingInfoTypeSupporter
		, ICanBeImportOrExport
	{
		public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : NctsCommonMovementHeader.Schema
		{
			public const string CustomsStatusDescription = nameof(NctsDepartureMovementHeader.CustomsStatusDescription);
			public const string MessageStatusDescription = nameof(NctsDepartureMovementHeader.MessageStatusDescription);
			public const string FromWarehouseCode = nameof(NctsDepartureMovementHeader.FromWarehouseCode);
			public const string FromWarehouseOrgPK = nameof(NctsDepartureMovementHeader.FromWarehouseOrgPK);
			public const string PlaceOfLoading = nameof(NctsDepartureMovementHeader.PlaceOfLoading);
			public const string IsContainerised = nameof(NctsDepartureMovementHeader.IsContainerised);
			public const string DepartureCustomsOfficeCodeForModuleGrid = nameof(NctsDepartureMovementHeader.DepartureCustomsOfficeCodeForModuleGrid);
			public const string DestinationCustomsOfficeCodeForDepartureForModuleGrid = nameof(NctsDepartureMovementHeader.DestinationCustomsOfficeCodeForDepartureForModuleGrid);
		}

		public new static readonly NctsDepartureMovementHeaderTypeDecider TypeDecider = new NctsDepartureMovementHeaderTypeDecider();

		[ChildEditable]
		public InlandTransportCollection InlandTransportList
		{
			get
			{
				if (inlandTransportList == null)
				{
					inlandTransportList = GetNewInlandTransportCollection();
					inlandTransportList.Load();
					RegisterEditableChildObject(inlandTransportList);
				}

				return inlandTransportList;
			}
		}

		InlandTransportCollection inlandTransportList;

		protected virtual InlandTransportCollection GetNewInlandTransportCollection() => new InlandTransportCollection(this);

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = GetNewMessageCollection();
					ediMessages?.Load();
					ediMessages?.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessages);
				}
				return ediMessages;
			}
		}
		EDIMessageCollection ediMessages;

		public ActiveBusinessObjectCollection<EDIMessage> MessagesForDisplay
		{
			get
			{
				if (messagesForDisplay == null)
				{
					var query = new ZQuery(Messages.CompleteFilter);
					query.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_LinkUniqueID, Header.PK);
					messagesForDisplay = new ActiveBusinessObjectCollection<EDIMessage>(Factory, query);
					messagesForDisplay.SetReadOnlyIncludingChildren(true);
				}
				return messagesForDisplay;
			}
		}
		ActiveBusinessObjectCollection<EDIMessage> messagesForDisplay;

		protected virtual EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this, Factory);

		#region ICusSupportingInfoTypeSupporter Implementation

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

		public new INctsDepartureMovementHeaderLookups Lookups => (INctsDepartureMovementHeaderLookups)base.Lookups;

		public new NctsDepartureMovementHeaderValidation Validation => (NctsDepartureMovementHeaderValidation)base.Validation;

		internal ZBool IsGrossWeightValid => GrossWeight.IsValid && GrossWeight >= TotalWeightOfBills;

		public ZWeight TotalWeightOfBills => Factory.GetValue(ref totalWeightOfBillsCache, () => Header.Bills
			.Select(b => b.GrossWeight)
			.Where(w => w.IsValid)
			.Aggregate(new ZWeight(0, BM_GrossWeightUQ), (totalWeight, billWeight) => totalWeight + billWeight));

		CachedProperty<ZWeight> totalWeightOfBillsCache;

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		[ResourceStringData("416245F1-7792-40F1-B3C8-1E0D1C2BE10C", Caption = "Reference Number / UCR", ShortCaption = "Ref. No. / UCR", FullDescription = "Indicate the Reference Number / Unique Consignment Reference (UCR)", MultipleKey = NctsHeader.Phase5DepartureCaptionKey)]
		public override ZString BM_UniqueConsignmentReference { get => base.BM_UniqueConsignmentReference; set => base.BM_UniqueConsignmentReference = value; }

		protected override void ValidateRepresentative(JobDocAddressValidation validation)
		{
			base.ValidateRepresentative(validation);

			if (ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider phase5ValidationDecider)
			{
				var representative = Representative;
				if (!representative.IsEmpty)
				{
					if (phase5ValidationDecider.IsRuleR0850Active)
					{
						CheckMandatoryEoriOrTCUCustomCode(representative, ValidationRuleCodeConstants.R0850.GetRuleCodeMessagePrefix(), Res.GetString("C5976318-09F7-4D28-8842-1829A7797598", "Representative"));
					}

					if (phase5ValidationDecider.IsRuleNR0070Active)
					{
						CheckRepresentative_NR0070(representative);
					}
				}
			}
		}

		void CheckRepresentative_NR0070(JobDocAddress representative)
		{
			if (representative.Organisation.GetEORI().IsEmpty)
			{
				var hasAdditionalReferenceWithCodeY025AtHeaderLevel = Header.AdditionalDocuments.Any(a => a.IsAnAdditionalReference && a.CSI_Code == NctsConstants.AdditionalInfoCodes.RepresentativeAEOCertificateNumber);

				if (hasAdditionalReferenceWithCodeY025AtHeaderLevel)
				{
					representative.OrganisationPKInfo.AddMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.NR0070Message);
				}
			}
		}

		void CheckMandatoryEoriOrTCUCustomCode(JobDocAddress address, string ruleCode, string propertyCaption)
		{
			var hasEoriOrTcuCode = address.Organisation?.CustomsCodes.GetOrgCusCodesForMatchingCodesIgnoringCountry(
					OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
					OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU)
				.Length > 0;

			if (!hasEoriOrTcuCode)
			{
				address.OrganisationPKInfo.AddMessageError(Res.GetString("70A9CF1E-5C1B-4DCF-9D28-36722FA6D738",
					"{0} {1} has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers / Codes", ruleCode, propertyCaption));
			}
		}

		protected sealed override CusInBondMoveHeaderLookups GetNewLookups() => IsPhase5 ? GetNewPhase5Lookups() : GetNewPhase4Lookups();

		protected virtual CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderPhase5Lookups(this);

		protected virtual CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderPhase4Lookups(this);

		protected sealed override CusInBondMoveHeaderValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderPhase5Validation(this);

		protected virtual NctsDepartureMovementHeaderPhase4Validation GetNewPhase4Validation() => new NctsDepartureMovementHeaderPhase4Validation(this);

		[ChildEditable(false)]
		public NctsDeparturePayInfoCollection PayInfoCollection
		{
			get
			{
				if (payInfoCollection == null)
				{
					payInfoCollection = GetNewPayInfoCollection();
					RegisterEditableChildObject(payInfoCollection);
				}
				return payInfoCollection;
			}
		}

		NctsDeparturePayInfoCollection payInfoCollection;

		protected virtual NctsDeparturePayInfoCollection GetNewPayInfoCollection() => new NctsDeparturePayInfoCollection(this);

		public Type PayInfoType => PayInfoTypeCore;

		protected virtual Type PayInfoTypeCore => typeof(NctsDeparturePayInfo);

		[ChildEditable]
		public virtual IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> AdditionalTransportAtBorderList
		{
			get
			{
				if (additionalTransportAtBorderList == null)
				{
					additionalTransportAtBorderList = GetNewCusTransportMeansCollection();
					additionalTransportAtBorderList.Load();
					RegisterEditableChildObject(additionalTransportAtBorderList);
					additionalTransportAtBorderList.CountChanged += (o, e) =>
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateBM_ActiveBorderIdentificationType();
						}
						AdditionalTransportAtBorderListCountInfo.RefreshBinding();
					};
				}

				return additionalTransportAtBorderList;
			}
		}
		IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> additionalTransportAtBorderList;

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

		protected virtual IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> GetNewCusTransportMeansCollection() => new DepartureCusTransportMeansCollection<DepartureCusTransportMeans>(this);

		[ResourceStringData("FBF340F1-B934-4EB3-9881-30D2533E8EC0", Caption = "Additional Border Num.", ShortCaption = "Add. Border Num.", FullDescription = "Number of Additional Border MOT")]
		public ZInt AdditionalTransportAtBorderListCount => Factory.GetValue(ref additionalTransportAtBorderListCountCache, () => AdditionalTransportAtBorderList.Count);
		CachedProperty<ZInt> additionalTransportAtBorderListCountCache;

		public ZPropertyInfo AdditionalTransportAtBorderListCountInfo => GetZPropertyInfo(nameof(AdditionalTransportAtBorderListCount));

		#region IAdditionalTransportMeansProvider

		IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> IAdditionalTransportMeansProvider.AdditionalTransportAtBorderList => AdditionalTransportAtBorderList;

		void IAdditionalTransportMeansProvider.ValidateAdditionalTransportAtBorderListCount()
		{
			if (Validation is NctsDepartureMovementHeaderPhase5Validation phase5Validation)
			{
				phase5Validation.ValidateAdditionalTransportAtBorderListCount();
			}
		}
		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Departure;
		}

		[ChildEditable(true)]
		public INctsGuaranteeCollection<NctsGuarantee> Guarantees
		{
			get
			{
				if (guarantees == null)
				{
					guarantees = GetGuaranteesCore();
					guarantees.Load();
					guarantees.ListChanged += Guarantees_ListChanged;
					RegisterEditableChildObject(guarantees);
				}
				return guarantees;
			}
		}
		INctsGuaranteeCollection<NctsGuarantee> guarantees;

		protected virtual INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

		protected virtual void Guarantees_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		#region Properties

		[ResourceStringData("4F5DA6C2-CA8E-42FA-B22D-2DB2B8A392B0", Caption = "Customs Office at Border", ShortCaption = "Office at Border", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("E2D3184A-8336-491A-867B-1C225B681FDA", Caption = "Customs Office", ShortCaption = "Office", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.OfficeCodeList))]
		public override ZString BM_CustomsOfficeAtBorder
		{
			get => base.BM_CustomsOfficeAtBorder;
			set => base.BM_CustomsOfficeAtBorder = value;
		}

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.AdditionalDeclarationTypeList))]
		[ResourceStringData("BE07FC8C-F3AB-4B12-9E8C-1B570346A2C6", FullDescription = "11 02 001 000 Additional Declaration type", Caption = "Additional Declaration type", MediumCaption = "Add. Declaration Type", ShortCaption = "Add. Decl. Type", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_AdditionalDeclarationType
		{
			get => base.BM_AdditionalDeclarationType;
			set => base.BM_AdditionalDeclarationType = value;
		}

		[ResourceStringData("F3DACDD4-7AA7-4343-9187-BD04E8DE4EFA", FullDescription = "15 08 000 000 Presentation Date and Time", Caption = "Presentation Date and Time", MediumCaption = "Presentation Date&Time", ShortCaption = "Presentation", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZDateTimeOffset BM_PresentationDateTime
		{
			get => base.BM_PresentationDateTime;
			set => base.BM_PresentationDateTime = value;
		}

		[ResourceStringData("14364534-1CB8-4A65-A247-15712FD46BA2", Caption = "Nationality", ShortCaption = "Nat.")]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TOLCarrierNationalityList))]
		public override ZString BM_RN_NKTOLCarrierNationality
		{
			get => base.BM_RN_NKTOLCarrierNationality;
			set => base.BM_RN_NKTOLCarrierNationality = value;
		}

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportAtDepartureTypeOfIdList))]
		[ResourceStringData("4EF931D8-8F7D-455D-9BDE-43D3AC688143", Caption = "Type of Identification", ShortCaption = "Type of ID")]
		public override ZString BM_TransportAtDepartureType
		{
			get => base.BM_TransportAtDepartureType;
			set => base.BM_TransportAtDepartureType = value;
		}

		[MaxLength(nameof(BM_AircraftIDAtDeparture_MaxLength))]
		[ResourceStringData("89831BCB-BEA7-4979-8B8F-DE9A43C358A6", Caption = "Aircraft Identification", ShortCaption = "Aircraft ID")]
		public override ZString BM_AircraftIDAtDeparture
		{
			get => base.BM_AircraftIDAtDeparture;
			set
			{
				var oldValue = BM_AircraftIDAtDeparture;
				base.BM_AircraftIDAtDeparture = value;

				if (!IsValidationSuspended && oldValue != BM_AircraftIDAtDeparture)
				{
					Validation.ValidateBM_InlandTransportMode();
					Validation.ValidateBM_TransportAtDeparture();
				}
			}
		}

		int BM_AircraftIDAtDeparture_MaxLength => IsPhase5Departure && IsInPhase5TransitionPeriod && IsAirInlandTransport ? 27 : 35;

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.NctsMessageStatusList))]
		[ReadOnly(true)]
		[ResourceStringData("2bb7f671-64ad-4e1f-90bf-1a2a6023e679", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Msg. Stat.")]

		public override ZString BM_MessageStatus
		{
			get => base.BM_MessageStatus;
			set
			{
				var oldValue = BM_MessageStatus;
				base.BM_MessageStatus = value;
				if (!IsCopying && oldValue != BM_MessageStatus)
				{
					Header.Bills.SelectMany(x => x.GoodsItems).ForEach(x => x.MarkAsNeedingValidation());
				}
			}
		}

		[MaxLength(nameof(BM_TransportAtDeparture_MaxLength))]
		public override ZString BM_TransportAtDeparture
		{
			get => base.BM_TransportAtDeparture;
			set
			{
				var oldValue = BM_TransportAtDeparture;
				base.BM_TransportAtDeparture = value;

				if (!IsValidationSuspended && oldValue != BM_TransportAtDeparture)
				{
					Validation.ValidateBM_InlandTransportMode();
					Validation.ValidateBM_AircraftIDAtDeparture();
				}
			}
		}

		int BM_TransportAtDeparture_MaxLength => IsPhase5 ? (IsPhase5Departure && IsInPhase5TransitionPeriod && (IsRailInlandTransport || IsRoadInlandTransport || IsAirInlandTransport || IsFixedTransportInstallationsInlandTransport || IsOwnPropulsionInlandTransport) ? 27 : 35) : 27;

		[MaxLength(nameof(VesselNameAtDeparture_MaxLength))]
		[ResourceStringData("40BA398F-8A13-4578-B44F-317AA513AA5A", Caption = "Vessel Name", ShortCaption = "Vessel")]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.Vessels))]
		public virtual ZString VesselNameAtDeparture
		{
			get => BM_TransportAtDeparture;
			set
			{
				var oldValue = VesselNameAtDeparture;
				BM_TransportAtDeparture = value;
				if (!IsCopying && oldValue != VesselNameAtDeparture)
				{
					MakeTransportAtDepartureUpperCaseIfNecessary();
				}
				VesselNameAtDepartureInfo.RefreshBinding();
			}
		}

		int VesselNameAtDeparture_MaxLength => IsPhase5Departure && IsInPhase5TransitionPeriod && IsSeaInlandTransport ? 27 : 35;

		public ZPropertyInfo VesselNameAtDepartureInfo => GetWrappedZPropertyInfo(nameof(VesselNameAtDeparture), x => BM_TransportAtDepartureInfo);

		[ResourceStringData("F06C7D17-8549-40CF-9858-8C820A06C22B", Caption = "Vessel Nationality", ShortCaption = "Nationality")]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportNationalityList))]
		public virtual ZString VesselCountryAtDeparture
		{
			get => BM_RN_NKTransportAtDepartureCountry;
			set => BM_RN_NKTransportAtDepartureCountry = value;
		}

		public ZPropertyInfo VesselCountryAtDepartureInfo => GetWrappedZPropertyInfo(nameof(VesselCountryAtDeparture), x => BM_RN_NKTransportAtDepartureCountryInfo);

		[MaxLength(nameof(BM_TransportAtDepartureTrailer1RegNo_MaxLength))]
		[ResourceStringData("57E9FC62-43FA-4294-B5BC-A7F7B342EB4F", Caption = "Trailer 1 ID", MediumCaption = "Trailer 1", ShortCaption = "TRLR 1")]
		public override ZString BM_TransportAtDepartureTrailer1RegNo
		{
			get => base.BM_TransportAtDepartureTrailer1RegNo;
			set
			{
				var oldValue = BM_TransportAtDepartureTrailer1RegNo;
				base.BM_TransportAtDepartureTrailer1RegNo = value;

				if (!IsValidationSuspended && oldValue != BM_TransportAtDepartureTrailer1RegNo)
				{
					Validation.ValidateBM_InlandTransportMode();
					Validation.ValidateBM_TransportAtArrivalTrailer1RegNo();
				}
			}
		}

		int BM_TransportAtDepartureTrailer1RegNo_MaxLength => IsPhase5Departure && IsInPhase5TransitionPeriod && (IsRailInlandTransport || IsRoadInlandTransport) ? 27 : 35;

		[ResourceStringData("E38C5BBA-F037-48B9-8EB6-8659BC2932E4", Caption = "Trailer 1 Nationality", MediumCaption = "TRLR 1 Nationality", ShortCaption = "Nationality")]
		public override ZString BM_RN_NKTransportAtDepartureTrailer1Nationality
		{
			get => base.BM_RN_NKTransportAtDepartureTrailer1Nationality;
			set => base.BM_RN_NKTransportAtDepartureTrailer1Nationality = value;
		}

		[MaxLength(nameof(BM_TransportAtDepartureTrailer2RegNo_MaxLength))]
		[ResourceStringData("D14CC0F5-3CDD-472E-9E01-B55A972DDB23", Caption = "Trailer 2 ID", MediumCaption = "Trailer 2", ShortCaption = "TRLR 2")]
		public override ZString BM_TransportAtDepartureTrailer2RegNo
		{
			get => base.BM_TransportAtDepartureTrailer2RegNo;
			set
			{
				var oldValue = BM_TransportAtDepartureTrailer2RegNo;
				base.BM_TransportAtDepartureTrailer2RegNo = value;

				if (!IsValidationSuspended && oldValue != BM_TransportAtDepartureTrailer2RegNo)
				{
					Validation.ValidateBM_InlandTransportMode();
				}
			}
		}

		int BM_TransportAtDepartureTrailer2RegNo_MaxLength => IsPhase5Departure && IsInPhase5TransitionPeriod && IsRoadInlandTransport ? 27 : 35;

		[ResourceStringData("4B4555A9-0E28-4E99-91AA-145431533681", Caption = "Trailer 2 Nationality", MediumCaption = "TRLR 2 Nationality", ShortCaption = "Nationality")]
		public override ZString BM_RN_NKTransportAtDepartureTrailer2Nationality
		{
			get => base.BM_RN_NKTransportAtDepartureTrailer2Nationality;
			set => base.BM_RN_NKTransportAtDepartureTrailer2Nationality = value;
		}

		public override ZString BM_InBondEntryType
		{
			get => base.BM_InBondEntryType;
			set
			{
				var oldValue = BM_InBondEntryType;
				base.BM_InBondEntryType = value;
				if (oldValue != BM_InBondEntryType)
				{
					var isPhase5 = IsPhase5;
					if (isPhase5)
					{
						if (IsTIRDeclaration)
						{
							IsSimplifiedNctsProcedure = false;
							BM_ReducedDatasetIndicator = false;
						}
						else
						{
							TirCarnetNumber = ZString.Empty;
						}
					}
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						var header = Header;
						if (isPhase5)
						{
							CustomsOffices?.MarkAsNeedingValidation();
							header?.Bills?.MarkAsNeedingValidationIncludingChildren();
						}
						else
						{
							header?.CustomsOffices?.MarkAsNeedingValidation();
						}
						header?.MarkAsNeedingValidation();
						GoodsItems.MarkAsNeedingValidationIncludingChildren();
					}
				}
			}
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.NctsTransitStatusList))]
		[ReadOnly(true)]
		[ResourceStringData("286D42B0-BCF9-404D-A13B-CACA9C370CC9", Caption = "Departure Status", ShortCaption = "Dep. St.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("D34850B8-CF72-44C0-BC34-23FA92E9A72A", Caption = "Departure Status", MediumCaption = "Dep. Status", ShortCaption = "Dep. Stat.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_CustomsStatus
		{
			get => base.BM_CustomsStatus;
			set
			{
				var oldValue = BM_CustomsStatus;
				base.BM_CustomsStatus = value;
				if (!IsCopying && oldValue != BM_CustomsStatus)
				{
					Header.Bills.SelectMany(x => x.GoodsItems).ForEach(x => x.MarkAsNeedingValidation());
				}

				if (ShouldConfirmTemporaryStorageGoodsConsumption)
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty,
						CountryCode,
						BM_CustomsStatus,
						TemporaryStorageTransactionInternalReferenceNumber,
						TemporaryStorageTransactionInternalReferenceType,
						BM_PaperlessInbondNum,
						PreviousDocumentCodeForDataToReserveTemporaryStorageGoods,
						GetGoodsItemDataDeclaredForDepartureToReserveTSGoods,
						Header.MovementReferenceNumber,
						TemporaryStorageTransactionCommentPrefix,
						Header.BH_JobReference,
						Header.MovementReferenceIssueDate,
						ReleaseDateForTemporaryStorage,
						TemporaryStorageWriteOffTransactionCommentReferenceNumber,
						Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						formatDocRef: ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation ? GetDocumentNumberFormat : null);
				}
			}
		}

		#region Confirm/Reserve Temporary Storage Goods

		public ZString TemporaryStorageTransactionInternalReferenceNumber => TemporaryStorageTransactionInternalReferenceNumberCore;
		protected virtual ZString TemporaryStorageTransactionInternalReferenceNumberCore => ZString.Empty;

		public ZString TemporaryStorageTransactionInternalReferenceType => TemporaryStorageTransactionInternalReferenceTypeCore;
		protected virtual ZString TemporaryStorageTransactionInternalReferenceTypeCore => ZString.Empty;

		public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetGoodsItemDataDeclaredForDepartureToReserveTSGoods() => GetGoodsItemDataDeclaredForDepartureToReserveTSGoodsCore();
		protected virtual (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetGoodsItemDataDeclaredForDepartureToReserveTSGoodsCore() => (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), ZString.Empty);

		protected virtual ZBool ShouldConfirmTemporaryStorageGoodsConsumption => false;

		protected virtual IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => Array.Empty<ZString>();

		protected virtual ZString TemporaryStorageTransactionCommentPrefix => ZString.Empty;

		protected virtual ZDateTime ReleaseDateForTemporaryStorage => ZDateTime.Empty;

		protected virtual ZString PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => ZString.Empty;

		protected virtual ZString TemporaryStorageWriteOffTransactionCommentReferenceNumber => ZString.Empty;

		protected virtual ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => false;

		protected virtual ZString GetDocumentNumberFormat(ZString dsdtMRN) => dsdtMRN;

		#endregion

		[ResourceStringData("D77F7D48-9DD4-4CA9-87BA-E90E506ADBB1", Caption = "Departure Status Description", ShortCaption = "Dep. St. Desc.")]
		public ZString CustomsStatusDescription
		{
			get
			{
				var customsStatus = BM_CustomsStatus;
				var statusDescription = ZString.Empty;
				if (!customsStatus.IsEmpty && customsStatus != NctsTransitStatusList.Codes.Unknown)
				{
					statusDescription = Lookups.NctsTransitStatusList.GetDescriptionFromCode(BM_CustomsStatus);
				}
				return statusDescription;
			}
		}

		[ResourceStringData("8cc53f51-2461-4a23-b103-8f20e4556c87", Caption = "Departure Message Status Description", ShortCaption = "Dep. Msg. St. Desc.")]
		public ZString MessageStatusDescription
		{
			get
			{
				var messageStatus = BM_MessageStatus;
				var statusDescription = ZString.Empty;
				if (!messageStatus.IsEmpty && messageStatus != NctsMessageStatusList.Codes.Unknown)
				{
					statusDescription = Lookups.NctsMessageStatusList.GetDescriptionFromCode(BM_MessageStatus);
				}
				return statusDescription;
			}
		}

		public ZPropertyInfo DepartureStatusDescriptionInfo => GetZPropertyInfo(Schema.CustomsStatusDescription);

		[MaxLength(17)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.LocationOfGoodsCodeList))]
		[ResourceStringData("12992ACB-3536-4D35-B5C8-07ABB12F1526", Caption = "Location Code", ShortCaption = "Loc. Code", FullDescription = "Location of goods (code) (agreed or authorized)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("086AB1AF-3CD9-435D-BC38-C235E67ACD22", Caption = "Location Code", ShortCaption = "Loc.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_LocationOfGoodsCode
		{
			get => base.BM_LocationOfGoodsCode;
			set => base.BM_LocationOfGoodsCode = value;
		}

		[ResourceStringData("715C25FD-0EC3-4737-9295-2BE8E4C3FD9B", Caption = "Valuation Date")]
		public override ZDateTime BM_ValuationDate
		{
			get => base.BM_ValuationDate;
			set
			{
				var oldValue = BM_ValuationDate;
				base.BM_ValuationDate = value;
				if (BM_ValuationDate != oldValue)
				{
					foreach (var goodsItem in GoodsItems.OfType<NctsDepartureCargoDesc>())
					{
						goodsItem.RefreshBindingsWhenValuationDateChanged();
						goodsItem.MarkAsNeedingValidation();
					}

					Header?.Bills.ForEach(x => x.GoodsItems.ForEach(y => y.MarkAsNeedingValidation()));
				}
			}
		}

		public ZBool PreLodgedForAgreedLocationOfGoodsCode
		{
			get { return GetPreLodgedForAgreedLocationOfGoodsCode(); }
			set
			{
				SetPreLodgedForAgreedLocationOfGoodsCode(value);
			}
		}

		string oldAgreedLocationOfGoodsCodeForPreLodgeTick;

		public ZPropertyInfo PreLodgedForAgreedLocationOfGoodsCodeInfo => GetZPropertyInfo(nameof(PreLodgedForAgreedLocationOfGoodsCode));

		protected virtual ZBool GetPreLodgedForAgreedLocationOfGoodsCode() => BM_LocationOfGoodsCode == PreLodgedString;

		protected virtual void SetPreLodgedForAgreedLocationOfGoodsCode(bool value)
		{
			var oldValue = PreLodgedForAgreedLocationOfGoodsCode;
			if (value)
			{
				oldAgreedLocationOfGoodsCodeForPreLodgeTick = BM_LocationOfGoodsCode;
				BM_LocationOfGoodsCode = PreLodgedString;
			}
			else
			{
				BM_LocationOfGoodsCode = oldAgreedLocationOfGoodsCodeForPreLodgeTick;
			}
			PreLodgedForAgreedLocationOfGoodsCodeInfo.RefreshBinding(oldValue);
			BM_LocationOfGoodsCodeInfo.RefreshBinding();
		}

		[MaxLength(35)]
		[ResourceStringData("E2F398B4-691B-452C-BE81-A4C3C85E464D", Caption = "Location", ShortCaption = "Loc.", FullDescription = "Location of goods")]
		public override ZString BM_LocationOfGoods
		{
			get => base.BM_LocationOfGoods;
			set => base.BM_LocationOfGoods = value;
		}

		[MaxLength(17)]
		[ResourceStringData("D20B9988-94FB-4B8B-800A-90A290B6A3EE", Caption = "Sub Place", FullDescription = "Customs Sub Place")]
		public override ZString BM_CustomsSubPlace
		{
			get => base.BM_CustomsSubPlace;
			set => base.BM_CustomsSubPlace = value;
		}

		[ResourceStringData("1922918F-D320-460A-8423-63F28254E189", Caption = "[27] Place of Loading Code", ShortCaption = "Loading", FullDescription = "Place of Loading (code)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("5D429120-550A-4442-9BB6-C92427D5CE35", Caption = "Place of Loading", ShortCaption = "Loading", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_RL_NKForeignDestPort
		{
			get => base.BM_RL_NKForeignDestPort;
			set => base.BM_RL_NKForeignDestPort = value;
		}

		[ResourceStringData("F6413596-5C31-482E-B494-5830B225730F", Caption = "Loading", FullDescription = "Place of Loading")]
		public ZString PlaceOfLoading => PlaceOfLoadingCore();

		protected virtual ZString PlaceOfLoadingCore() => ForeignDestPort?.Description.Left(17) ?? ZString.Empty;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.ModeOfTransportList))]
		[ResourceStringData("92D195F0-8A3D-4037-BEDC-2DB14DA7321D", Caption = "[26] Transport Mode (Inland)", ShortCaption = "Inland M.O.T.", FullDescription = "Inland Mode of Transport (Code)")]
		public override ZString BM_InlandTransportMode
		{
			get => base.BM_InlandTransportMode;
			set
			{
				var oldValue = BM_InlandTransportMode;
				base.BM_InlandTransportMode = value;
				if (IsPhase5 && !IsCopying && oldValue != BM_InlandTransportMode)
				{
					ClearInlandTransportModeRelatedProperties();
					SetDefaultTransportTypeAtDeparture();
					foreach (var bill in Header.Bills)
					{
						bill.MarkAsNeedingValidation();
						bill.InlandTransportModeAtDepartureInfo.RefreshBinding(oldValue);
						bill.FirstDepartureTransportMeansID = ZString.Empty;
						bill.SecondDepartureTransportMeansID = ZString.Empty;
						bill.ThirdDepartureTransportMeansID = ZString.Empty;
						bill.TransportTypeAtDeparture = ZString.Empty;
						bill.FirstDepartureTransportMeansNationality = ZString.Empty;
						bill.SecondDepartureTransportMeansNationality = ZString.Empty;
						bill.ThirdDepartureTransportMeansNationality = ZString.Empty;
						bill.TransportDepartureAdditionalWagonNumbers.RemoveAndDeleteAll();
						bill.DepartureTransportInfos.RemoveAndDeleteAll();
					}
				}
			}
		}

		void SetDefaultTransportTypeAtDeparture()
		{
			TransportTypeAtDeparture = Lookups.TransportAtDepartureTypeOfIdList.DefaultCode;
		}

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.ForeignDestPortCodes))]
		[ResourceStringData("01696F19-7062-4028-AE69-D37A455D0EFF", Caption = "Place of Unloading Code", ShortCaption = "Unloading", FullDescription = "Place of Unloading (code)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("72868EF2-9A7F-40B8-864B-97DD4C17E200", Caption = "Place of Unloading", ShortCaption = "Unloading", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[MaxLength(5)]
		public override ZString BM_ForeignDestPortKCode
		{
			get => base.BM_ForeignDestPortKCode;
			set
			{
				var oldValue = BM_ForeignDestPortKCode;
				base.BM_ForeignDestPortKCode = value;
				if (!IsCopying && oldValue != BM_ForeignDestPortKCode && (BM_PlaceOfUnloading.IsEmpty || GetUnlocoDescriptionFromCode(oldValue).Left(BM_PlaceOfUnloadingInfo.MaxLength) == BM_PlaceOfUnloading))
				{
					if (value.Length == 5)
					{
						var description = GetUnlocoDescriptionFromCode(value);
						if (!description.IsEmpty)
						{
							BM_PlaceOfUnloading = description.Left(BM_PlaceOfUnloadingInfo.MaxLength);
						}
					}
					else if (value.IsEmpty)
					{
						BM_PlaceOfUnloading = ZString.Empty;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.PortOfPresentationCodes))]
		[MaxLength(5)]
		public override ZString BM_PortOfPresentationCode
		{
			get => base.BM_PortOfPresentationCode;
			set
			{
				var oldValue = BM_PortOfPresentationCode;
				base.BM_PortOfPresentationCode = value;
				if (!IsCopying && oldValue != BM_PortOfPresentationCode && (BM_PlaceOfLoading.IsEmpty || GetUnlocoDescriptionFromCode(oldValue).Left(BM_PlaceOfUnloadingInfo.MaxLength) == BM_PlaceOfLoading))
				{
					if (value.Length == 5)
					{
						var description = GetUnlocoDescriptionFromCode(value);
						if (!description.IsEmpty)
						{
							BM_PlaceOfLoading = description.Left(BM_PlaceOfLoadingInfo.MaxLength);
						}
					}
					else if (value.IsEmpty)
					{
						BM_PlaceOfLoading = ZString.Empty;
					}
				}
			}
		}

		ZString GetUnlocoDescriptionFromCode(ZString code) => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code)?.RL_NameWithDiacriticals ?? ZString.Empty;

		protected override int BM_PlaceOfUnloadingMaxLength => IsInPhase5TransitionPeriod ? 17 : AutoCusInBondMoveHeader.Schema.BM_PlaceOfUnloadingMaxLength;

		public ZBool IsSeaInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._1_SeaTransport;

		public ZBool IsRailInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport;

		public ZBool IsRoadInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport;

		public ZBool IsAirInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._4_AirTransport;

		public ZBool IsPostalConsignmentInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._5_PostalConsignment;

		public ZBool IsFixedTransportInstallationsInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._7_FixedTransportInstallations;

		public ZBool IsInlandWaterwayInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._8_InlandWaterwayTransport;

		public ZBool IsOwnPropulsionInlandTransport => BM_InlandTransportMode == ModeOfTransportList.Codes._9_OwnPropulsion;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.BorderModeOfTransportList))]
		[ResourceStringData("36670E6F-68A6-48AC-A3E5-4D9BF161DFA5", Caption = "[21] Transport Mode (Frontier)", ShortCaption = "Frontier Transp. Mode", FullDescription = "Frontier Mode of Transport (Code)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("7A4A0E06-50C0-4B04-9ACA-F664F35A0A9C", Caption = "Border Method of Transport", MediumCaption = "Border M.O.T.", ShortCaption = "M.O.T.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_ExportTransportMode
		{
			get => base.BM_ExportTransportMode;
			set
			{
				var oldValue = BM_ExportTransportMode;
				base.BM_ExportTransportMode = value;
				if (IsPhase5 && !IsCopying && oldValue != BM_ExportTransportMode)
				{
					ClearExportTransportModeRelatedProperties();
					CustomsOfficesForDeparture.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportAtBorderTypeOfIdList))]
		[ResourceStringData("DC5B0DB5-FEAC-40EE-9555-D3E4F7EBC1A5", Caption = "Type of Identification", MediumCaption = "Type of ID", ShortCaption = "Type")]
		public override ZString BM_ActiveBorderIdentificationType
		{
			get => base.BM_ActiveBorderIdentificationType;
			set
			{
				var oldValue = BM_ActiveBorderIdentificationType;
				base.BM_ActiveBorderIdentificationType = value;
				if (IsPhase5 && !IsCopying && oldValue != BM_ActiveBorderIdentificationType)
				{
					if (!IsValidationSuspended)
					{
						AdditionalTransportAtBorderList.Cast<DepartureCusTransportMeans>().ForEach(x => x.Validation.ValidateTPM_ParentTableCode());
					}
					MakeTOLCarrierIDUpperCaseIfNecessary();
				}
			}
		}

		[MaxLength(nameof(BM_TOLCarrierID_MaxLength))]
		[ResourceStringData("F70D26BC-8413-414B-B064-108EC40B4C33", Caption = "[21] Transport ID (Frontier)", ShortCaption = "Frontier ID", FullDescription = "Frontier Transport ID", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("9E3CFDCA-5A9D-4760-9F59-AF85C153ED6B", Caption = "Transport Identification", MediumCaption = "Transport ID", ShortCaption = "Transp. ID", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ResourceStringData("40BA398F-8A13-4578-B44F-317AA513AA5A", Caption = "Vessel Name", ShortCaption = "Vessel", MultipleKey = NctsDepartureMovementHeader.SeaVesselPhase5SeaExportTransportModeCaptionKey)]
		[ResourceStringData("ADB516E9-74EC-4464-8062-B3D2D5D9E4D0", Caption = "Lloyds Number", ShortCaption = "Lloyds No.", MultipleKey = NctsDepartureMovementHeader.LloydsPhase5SeaExportTransportModeCaptionKey)]
		[ResourceStringData("3038A937-232B-4A6F-A444-92A7C1B9BDCE", Caption = "Wagon Number", ShortCaption = "Wagon No.", MultipleKey = NctsDepartureMovementHeader.WagonPhase5SeaExportTransportModeCaptionKey)]
		[ResourceStringData("6CAAD134-4185-4699-93A0-1D46DD2FCC09", Caption = "Train Number", ShortCaption = "Train No.", MultipleKey = NctsDepartureMovementHeader.TrainPhase5SeaExportTransportModeCaptionKey)]
		[ResourceStringData("D3578099-E363-41D7-AB6D-AC4557FE8B2D", Caption = "Flight Number", ShortCaption = "Flight No.", MultipleKey = NctsDepartureMovementHeader.FlightPhase5SeaExportTransportModeCaptionKey)]
		[ResourceStringData("2093C600-46CF-4C5F-80FC-040AE7E7DCAE", Caption = "Registration Number", ShortCaption = "Registration No.", MultipleKey = NctsDepartureMovementHeader.RegistrationPhase5SeaExportTransportModeCaptionKey)]
		[ResourceStringData("0FADA506-42CF-4E72-9D3B-709C2ADB518D", Caption = "ENI Code", MultipleKey = NctsDepartureMovementHeader.ENIPhase5SeaExportTransportModeCaptionKey)]
		[ResourceStringData("10CDDA8A-2270-4E80-8787-9468F03874FE", Caption = "Vessel Name", ShortCaption = "Vessel", MultipleKey = NctsDepartureMovementHeader.InlandWaterwayVesselPhase5SeaExportTransportModeCaptionKey)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TOLCarrierIDList))]
		public override ZString BM_TOLCarrierID
		{
			get => base.BM_TOLCarrierID;
			set
			{
				var oldValue = BM_TOLCarrierID;
				base.BM_TOLCarrierID = value;
				if (!IsCopying && oldValue != BM_TOLCarrierID)
				{
					MakeTOLCarrierIDUpperCaseIfNecessary();
				}
			}
		}

		int BM_TOLCarrierID_MaxLength => IsPhase5 ? ((IsPhase5Departure && IsInPhase5TransitionPeriod) ? 27 : 35) : 27;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportNationalityList))]
		[ResourceStringData("{1119BB3C-C4A9-4F05-8A94-33A6FDB8300A", Caption = "[21] Transport Nationality (Frontier)", ShortCaption = "Frontier Flag", FullDescription = "Frontier Transport Nationality")]
		public override ZString BM_TOLCarrierCode
		{
			get => base.BM_TOLCarrierCode;
			set => base.BM_TOLCarrierCode = value;
		}

		[ResourceStringData("292C6808-42B0-4B6D-BA44-F73D7F41D45A", Caption = "Declaration Date", ShortCaption = "Dec. Date", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("74BCC57A-9455-4C47-A29B-0A54802BE57F", Caption = "Acceptance Date", ShortCaption = "Accept. Date", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZDateTime BM_EntryDate
		{
			get => base.BM_EntryDate;
			set => base.BM_EntryDate = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.SpecificCircumstanceIndicatorList))]
		[ResourceStringData("4B6D97A7-36CE-42FC-92CB-3173EB5E9BF2", Caption = "Specific Circumstance Indicator", ShortCaption = "Spec. Circ.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("1559637D-B8C6-4E92-9515-B2332AB3FF7F", Caption = "Specific Circumstance Indicator", MediumCaption = "Specific Circumstance", ShortCaption = "Circumstance", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_BTAIndicator
		{
			get => base.BM_BTAIndicator;
			set => base.BM_BTAIndicator = value;
		}

		[ReadOnlyMember(nameof(BM_MethodOfPaymentReadOnly))]
		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportChargesModeOfPaymentList))]
		[ResourceStringData("C7AFFE28-927E-4EF4-ACD0-110303084851", Caption = "Transport Charges / Method of Payment", ShortCaption = "MoP.", FullDescription = "Method of Payment of Transport Charges", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("FB1532F8-D804-40C4-9BC7-FE8C7827CA58", Caption = "Transport Method of Payment", MediumCaption = "Transport MoP", ShortCaption = "Transp. MoP", FullDescription = "Method of Payment of Transport Charges", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_MethodOfPayment
		{
			get => base.BM_MethodOfPayment;
			set => base.BM_MethodOfPayment = value;
		}

		protected bool BM_MethodOfPaymentReadOnly => IsRuleC0186Applied;

		[MaxLength(70)]
		[ResourceStringData("F99C8AB8-99C9-4038-AD37-3B73B43F08D5", Caption = "[7] Commercial Reference No.", ShortCaption = "Comm. Ref. No.", FullDescription = "Commercial Reference Number", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("E20F183C-3D21-4333-B130-BA5916B0A21D", Caption = "Unique Consignment Reference", MediumCaption = "Reference", ShortCaption = "UCR", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_AdditionalText
		{
			get => base.BM_AdditionalText;
			set => base.BM_AdditionalText = value;
		}

		[MaxLength(nameof(BM_ConveyanceNumber_MaxLength))]
		[ResourceStringData("C48FFE52-4B15-4752-B279-E191FA1C8581", Caption = "Conveyance Reference No.", ShortCaption = "Conv. Ref. No.", FullDescription = "Conveyance Reference Number", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("D66BD36C-7436-44F3-A7D8-D82AB64F51CD", Caption = "Conveyance Number", MediumCaption = "Conveyance No.", ShortCaption = "Conv. No.", FullDescription = "Conveyance Reference Number", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_ConveyanceNumber
		{
			get => base.BM_ConveyanceNumber;
			set => base.BM_ConveyanceNumber = value;
		}

		public int BM_ConveyanceNumber_MaxLength => IsPhase5 ? 17 : 35;

		[ResourceStringData("B1E0E27C-87BD-4A6D-BFC8-44777531B04F", Caption = "Simplified Procedure (Authorized Consignor)", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("A23D089F-1143-483F-87C1-B5BC0966BD7A", Caption = "Simplified Procedure", MediumCaption = "Simplified Proc.", ShortCaption = "Procedure", FullDescription = "Simplified Procedure for the Authorized Consignor", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZBool IsSimplifiedNctsProcedure
		{
			get => base.IsSimplifiedNctsProcedure;
			set
			{
				var oldValue = IsSimplifiedNctsProcedure;
				base.IsSimplifiedNctsProcedure = value;
				if (!IsCopying && oldValue != IsSimplifiedNctsProcedure && IsPhase5)
				{
					if (!IsSimplifiedNctsProcedure)
					{
						if (ShouldBM_ExportDateBeEmpty)
						{
							BM_ExportDate = ZDateTime.Empty;
						}
						ClearGoodsLocationWhenIsNotSimplifiedNctsProcedure();
					}
					UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, IsSimplifiedNctsProcedure);
					UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.SpecialSeals, IsSimplifiedNctsProcedure);
				}
			}
		}

		public override ZBool BM_ReducedDatasetIndicator
		{
			get => base.BM_ReducedDatasetIndicator;
			set
			{
				var oldValue = BM_ReducedDatasetIndicator;
				base.BM_ReducedDatasetIndicator = value;
				if (!IsCopying && oldValue != BM_ReducedDatasetIndicator && IsPhase5)
				{
					UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, BM_ReducedDatasetIndicator);
				}
			}
		}

		protected virtual void ClearGoodsLocationWhenIsNotSimplifiedNctsProcedure()
		{
		}

		protected virtual bool ShouldBM_ExportDateBeEmpty => true;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.NctsControlResultList))]
		[ResourceStringData("99CF035B-AE38-463B-BBEE-8E6FE6B151BA", Caption = "Control Result", FullDescription = "Control Result Code")]
		public override ZString BM_GONumber
		{
			get => base.BM_GONumber;
			set
			{
				base.BM_GONumber = value;
				Header?.MarkAsNeedingValidation();
			}
		}

		[ResourceStringData("961722F8-2400-4923-9CE2-ACD2FA44B61B", Caption = "Control Result Date", FullDescription = "Date Limit of Control Result", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("9D59C777-A616-4153-949F-CF3459481DF1", Caption = "Date Limit", ShortCaption = "Limit", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZDateTime BM_ExportDate
		{
			get => base.BM_ExportDate;
			set => base.BM_ExportDate = value;
		}

		[MaxLength(3)]
		[ResourceStringData("E24B7BF4-2D7C-47E2-801B-1C2EC1A008AC", Caption = "Representative", FullDescription = "Representative Code")]
		public override ZString BM_GS_NKCusAgent
		{
			get => base.BM_GS_NKCusAgent;
			set => base.BM_GS_NKCusAgent = value;
		}

		[ResourceStringData("65FB5DC4-AFFA-4B4B-9067-B92631B68F68", Caption = "Representative", FullDescription = "Representative Name")]
		public ZString RepresentativeName => CusAgent?.GS_FullName.Left(35) ?? GlbStaff.CurrentUser.GS_FullName.Left(35);

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.SealTypeList))]
		[ResourceStringData("E2F51E41-8399-40BA-9AD6-9D6A3C80625E", Caption = "Seal Type", MediumCaption = "Type")]
		public override ZString BM_SealType
		{
			get => base.BM_SealType;
			set
			{
				var oldValue = BM_SealType;
				if (oldValue != value)
				{
					var args = new CancelEventArgs(false);
					if (!oldValue.IsEmpty)
					{
						BM_SealTypeAboutToChange(this, args);
					}

					if (!args.Cancel)
					{
						base.BM_SealType = value;

						if (value != SealTypeList.Codes.ContainerSeal)
						{
							Header.DepartureHeaderContainers.RemoveAndDeleteAll();
						}

						if (value != SealTypeList.Codes.PackageSeal)
						{
							Header.Seals.RemoveAndDeleteAll();
							Header.CusSeals.RemoveAndDeleteAll();
						}
					}
				}
			}
		}

		public event CancelEventHandler OnBM_SealTypeAboutToChange;

		public void BM_SealTypeAboutToChange(object sender, CancelEventArgs args) => OnBM_SealTypeAboutToChange?.Invoke(sender, args);

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TypeOfSecurityList))]
		[ResourceStringData("2D47EDC3-EA6F-4594-8872-7F5C71432D47", Caption = "Security")]
		public override ZString BM_TypeOfSecurity
		{
			get => base.BM_TypeOfSecurity;
			set
			{
				var oldValue = base.BM_TypeOfSecurity;
				base.BM_TypeOfSecurity = value;

				if (!IsCopying && oldValue != value)
				{
					if (IsRuleC0186Applied)
					{
						BM_MethodOfPayment = ZString.Empty;
						Header.Bills.ForEach(bill => bill.B0_TransportPaymentMethod = ZString.Empty);
					}

					if (Header.Configuration.ValidationRuleConfiguration.IsRuleB1896Active)
					{
						Header.Bills.MarkAsNeedingValidation();
					}

					if (IsPhase5)
					{
						Header.BH_FTZMove = IsSecurityTypeENTOrBTHOrEXI;
						Header.Consignee.Validation.ValidateOrganisationPK();
						Header.Bills.ForEach(x => x.Consignee.Validation.ValidateOrganisationPK());
					}
				}
			}
		}

		public ZBool IsSecurityTypeENTOrNON => BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.ENT) || BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.NON);

		public ZBool IsSecurityTypeENTOrBTH => BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.ENT) || BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.BTH);

		public ZBool IsSecurityTypeENTOrBTHOrEXI => BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.EXI) || IsSecurityTypeENTOrBTH;

		public ZBool IsSecurityTypeBTHOrEXI => BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.BTH) || BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.EXI);

		public ZBool IsSecurityTypeNONOrENT => BM_TypeOfSecurity.In(new ZString[] { NctsTypeOfSecurityList.Codes.NON, NctsTypeOfSecurityList.Codes.ENT });

		[ResourceStringData("0E077FE5-0733-4A22-981D-E16B22FE1AB6", Caption = "Seal Quantity", MediumCaption = "Quantity")]
		public override ZShort BM_SealQty
		{
			get => base.BM_SealQty;
			set => base.BM_SealQty = value;
		}

		[ResourceStringData("12D395A9-758C-4BBB-A60A-81BB0522EC18", Caption = "Gross Weight", MediumCaption = "Gross Weight")]
		public override ZDecimal BM_GrossWeight
		{
			get => base.BM_GrossWeight;
			set => base.BM_GrossWeight = value;
		}

		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.WeightUnitList))]
		[ResourceStringData("C7B475A8-0325-40CF-ABDF-BE01C3955C68", Caption = "Gross Weight Unit", MediumCaption = "Gross Weight Unit")]
		[ReadOnlyMember(nameof(IsGrossWeightUQReadOnly))]
		public override ZString BM_GrossWeightUQ
		{
			get => base.BM_GrossWeightUQ;
			set => base.BM_GrossWeightUQ = value;
		}

		public bool IsGrossWeightUQReadOnly => !IsPhase5 || IsGrossWeightUQReadOnlyCore;

		protected virtual bool IsGrossWeightUQReadOnlyCore => true;

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.NctsSpecificCircumstanceIndicatorList))]
		[ResourceStringData("1559637D-B8C6-4E92-9515-B2332AB3FF7F", Caption = "Specific Circumstance Indicator", MediumCaption = "Specific Circumstance", ShortCaption = "Circumstance")]
		public override ZString BM_SpecificCircumstance
		{
			get => base.BM_SpecificCircumstance;
			set => base.BM_SpecificCircumstance = value;
		}

		public ZString CustomsValueCaption
		{
			get
			{
				var localCurrency = HeaderBranch.Company.GC_RX_NKLocalCurrency;
				return Factory.GetCachedValue<ZString>("CustomsValueCaption" + localCurrency, () => Res.GetString("8E740202-BB5B-48DF-9C68-631B708B3688", "Value in {0}", localCurrency));
			}
		}

		[ResourceStringData("CD35513A-9E3A-488E-B17B-01F8A3925FE8", Caption = "Containerized", FullDescription = "Containerized Indicator")]
		public ZBool IsContainerised => IsPhase5 ? IsContainerised_Phase5 : IsContainerised_Phase4;
		protected virtual ZBool IsContainerised_Phase5 => Header?.DepartureHeaderContainers.Any(c => c.BC_Mode.EqualsIgnoringCase(Core.Constants.ContainerModes.Containerised)) ?? false;
		protected virtual ZBool IsContainerised_Phase4 => GoodsItems
			.Any(x => x.ContainersPivots
				.Cast<NonPersistentDepartureContainerPivot>()
				.Any(p => p.ContainerSelected && !p.ContainerNumber.IsEmpty));

		[ResourceStringData("C539F396-6DE9-417D-996F-B08382BCE17A", Caption = "[5] No. of Items", ShortCaption = "Items", FullDescription = "Total Number of Items")]
		public override ZInt TotalNumberOfItems => base.TotalNumberOfItems;

		[ResourceStringData("3085ADBD-9BF5-47ED-A8DB-47982CEA0B90", Caption = "[6] Total Packages", ShortCaption = "Packs", FullDescription = "Total Number of Packages")]
		public override ZLong TotalNumberOfPackages => base.TotalNumberOfPackages;

		[ResourceStringData("73E1FD1E-C892-4D65-A960-6251198715A9", Caption = "[35] Total Gross Weight (kg)", ShortCaption = "Total Gross")]
		public override ZDecimal TotalGrossMassInKilograms => IsPhase5 ? Header.Bills.SelectMany(x => x.GoodsItems).Sum(x => x.GrossMassInKilograms) : base.TotalGrossMassInKilograms;

		[ResourceStringData("897A1D53-381E-4DD7-89D4-E57F1E16963B", Caption = "Total Net Weight (kg)", ShortCaption = "Total Net")]
		public ZDecimal TotalNettMassInKilograms => GoodsItems.Sum(gi => gi.NetMassInKilograms);

		public bool IsTIRDeclaration => BM_InBondEntryType.EqualsIgnoringCase(NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration);

		public bool IsMixedConsignment => BM_InBondEntryType.EqualsIgnoringCase(DeclarationTypeMixedContent);

		public void SetMixedConsignment() => BM_InBondEntryType = DeclarationTypeMixedContent;

		public string DeclarationTypeMixedContent => IsPhase5
					? NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5
					: NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4;

		public bool IsInternalTransitProcedure => BM_InBondEntryType.EqualsIgnoringCase(NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure);

		[MaxLength(nameof(TirCarnetNumberMaxLength))]
		[ResourceStringData("3D70FD7B-C4A9-4758-AAAE-BEB8E6F3F6D2", Caption = "TIR Carnet Num.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("BC75F76D-AF53-4762-ACD5-5EE91A027802", Caption = "TIR Carnet Num.", MediumCaption = "Carnet Num.", ShortCaption = "Carnet", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public virtual ZString TirCarnetNumber
		{
			get => TirCarnetEntryNumber.CE_EntryNum;
			set
			{
				var oldValue = TirCarnetNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(TirCarnetNumberInfo, value);
					TirCarnetEntryNumber.CE_EntryNum = value;
					TirCarnetNumberInfo.RefreshBinding(oldValue);

					if (!IsCopying)
					{
						AddTirSupportingDocIfNeeded();
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateTirCarnetNumber();
					}
				}
			}
		}

		protected int TirCarnetNumberMaxLength => IsPhase5 ? AutoCusEntryNum.Schema.CE_EntryNumMaxLength : 10;

		protected virtual void AddTirSupportingDocIfNeeded()
		{
			var tirCarnetNumber = TirCarnetNumber;
			if (!tirCarnetNumber.IsEmpty && !IsPhase5)
			{
				var goodsItems = GoodsItems;
				var goodsItem = goodsItems.OrderBy(x => x.BY_LineNo).FirstOrDefault() ?? goodsItems.AddNew();
				var supportingDocuments = goodsItem.SupportingDocuments;

				var tirCarnetSupportingDocumentCode = TIRCarnetSupportingDocumentCode;
				supportingDocuments.DeleteAllDocumentsHavingCode(tirCarnetSupportingDocumentCode);
				supportingDocuments.AddNew(tirCarnetSupportingDocumentCode, tirCarnetNumber);
			}
		}

		protected virtual ZString TIRCarnetSupportingDocumentCode => NctsHeaderValidationHelper.TirCarnetDocumentCode;

		public ZPropertyInfo TirCarnetNumberInfo => GetZPropertyInfo(nameof(TirCarnetNumber));

		public ZDateTime TirCarnetExpiryDate
		{
			get => TirCarnetEntryNumber.CE_ExpiryDate;
			set
			{
				var oldValue = TirCarnetExpiryDate;
				if (oldValue != value)
				{
					TirCarnetEntryNumber.CE_ExpiryDate = value;
					TirCarnetExpiryDateInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo TirCarnetExpiryDateInfo => GetZPropertyInfo(nameof(TirCarnetExpiryDate));

		CusEntryNumber TirCarnetEntryNumber
		{
			get
			{
				if (tirCarnetEntryNumber == null || tirCarnetEntryNumber.IsDeleted)
				{
					tirCarnetEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.EU.TIRCarnetNumber, Header.CountryCode);
					RegisterEditableChildObject(tirCarnetEntryNumber);
				}
				return tirCarnetEntryNumber;
			}
		}

		CusEntryNumber tirCarnetEntryNumber;

		[ReadOnlyMember(nameof(BM_RL_NKDestinationPortReadOnly))]
		public override ZString BM_RL_NKDestinationPort
		{
			get => base.BM_RL_NKDestinationPort;
			set
			{
				var oldValue = BM_RL_NKDestinationPort;
				base.BM_RL_NKDestinationPort = value;
				if (!IsCopying && oldValue != BM_RL_NKDestinationPort && Header is NctsHeader header)
				{
					header.AddValueToTransitCustomsOfficeDependingOnDestinationPortCountry(this, Header.BH_RL_NKImportLoadPort);

					GoodsItems.ForEach(x => x.BY_RN_NKCountryOfDestinationInfo.RefreshBinding());

					if (!IsMarkingAsNeedingValidationSuspended)
					{
						GoodsItems.MarkAsNeedingValidation();
						if (IsPhase5)
						{
							header.Bills.MarkAsNeedingValidation();
							header.Bills.ForEach(b =>
							{
								b.GoodsItems.MarkAsNeedingValidation();
								b.Consignee.Validation.ValidateOrganisationPK();
							});
							header.Consignee.Validation.ValidateOrganisationPK();
						}
					}
				}
			}
		}

		public bool BM_RL_NKDestinationPortReadOnly => BM_RL_NKDestinationPort.IsEmpty && HasGoodsItemsWithCountryOfDestination;

		[List(nameof(Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.CountryOfDispatchList))]
		[ResourceStringData("2FEE6BA0-81F4-42DA-8BB8-C38439E1C399", Caption = "Country/Region of Dispatch", MediumCaption = "Dispatch Ctry./Rgn.", ShortCaption = "Disp. Ctry./Rgn.")]
		[ResourceStringData("2641F7FE-85F7-42BB-AEC2-BD0BA49245F4", Caption = "Country/Region of Dispatch", MediumCaption = "Dispatch Ctry./Rgn.", ShortCaption = "Disp. Ctry./Rgn.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_RN_NKCountryOfDispatch
		{
			get => base.BM_RN_NKCountryOfDispatch;
			set
			{
				var oldValue = BM_RN_NKCountryOfDispatch;
				base.BM_RN_NKCountryOfDispatch = value;
				if (oldValue != BM_RN_NKCountryOfDispatch && !IsMarkingAsNeedingValidationSuspended)
				{
					GoodsItems.MarkAsNeedingValidation();
					if (IsPhase5)
					{
						Header?.Bills?.ForEach(x => x.MarkAsNeedingValidation());
						Header?.Bills?.ForEach(b => b.GoodsItems.MarkAsNeedingValidation());
					}
				}
			}
		}

		public void WipeAdditionalText()
		{
			BM_AdditionalText = string.Empty;
		}

		[ResourceStringData("878fc4b0-aa32-4df4-8ba7-37f2d07d24a8", Caption = "From Warehouse", ShortCaption = "From Whs.")]
		[List($"{nameof(BM_OA_WarehouseAddress_ZAddress)}.{nameof(ZAddress.OrgAddress_List)}")]
		public override ZGuid BM_OA_WarehouseAddress
		{
			get => base.BM_OA_WarehouseAddress;
			set => base.BM_OA_WarehouseAddress = value;
		}

		[ResourceStringData("8a8626b5-9e77-4361-83de-a6ea7471621d", Caption = "From Warehouse Code", ShortCaption = "From Whs. Code")]
		public ZString FromWarehouseCode => Factory.GetValue(ref fromWarehouseCodeCached, GetFromWarehouseCode);
		CachedProperty<ZString> fromWarehouseCodeCached;

		protected ZString GetFromWarehouseCode()
			=> WarehouseAddress?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Header?.CountryCode ?? Env.CurrentCompany.Country.Code) ?? ZString.Empty;

		[ResourceStringData("f97d278e-0655-40ce-a67e-9e04be3f4c89", Caption = "From Warehouse Org.", ShortCaption = "From Whs. Org.")]
		[List($"{nameof(Lookups)}.{nameof(INctsDepartureMovementHeaderLookups.BondedWarehouseCollection)}")]
		public ZGuid FromWarehouseOrgPK
		{
			get { return BM_OA_WarehouseAddress_ZAddress.OrgPK; }
			set
			{
				BM_OA_WarehouseAddress_ZAddress.OrgPK = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateFromWarehouseOrgPK();
				}
				FromWarehouseOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FromWarehouseOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FromWarehouseOrgPK, x => BM_OA_WarehouseAddress_ZAddress.OrgPKInfo); }
		}

		#region Carrier

		public JobDocAddress Carrier
		{
			get
			{
				if (carrierJobDocAddress == null || carrierJobDocAddress.IsDeleted)
				{
					if (carrierJobDocAddress != null)
					{
						carrierJobDocAddress.DocAddressChanged -= new EventHandler(CarrierJobDocAddressChanged);
					}
					carrierJobDocAddress = DocAddresses.FindOrCreateWithRequirement(CarrierJobDocAddressRequirement);
					carrierJobDocAddress.DocAddressChanged += new EventHandler(CarrierJobDocAddressChanged);
					carrierJobDocAddress.AdditionalValidation = GetCarrierJobDocAddressAdditionalValidation(carrierJobDocAddress);
				}
				return carrierJobDocAddress;
			}
		}

		JobDocAddress carrierJobDocAddress;

		protected virtual ZValidation GetCarrierJobDocAddressAdditionalValidation(JobDocAddress carrierJobDocAddress) => null;

		public JobDocAddressRequirement CarrierJobDocAddressRequirement
		{
			get
			{
				if (carrierJobDocAddressRequirement == null)
				{
					carrierJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Carrier);
					carrierJobDocAddressRequirement.CanOverride = CarrierJobDocAddressCanOverride;
					carrierJobDocAddressRequirement.ValidateOrganisationPK = ValidateCarrier;
					JobDocAddressManager.AddRequirement(carrierJobDocAddressRequirement);
				}
				return carrierJobDocAddressRequirement;
			}
		}

		JobDocAddressRequirement carrierJobDocAddressRequirement;

		void CarrierJobDocAddressChanged(object sender, EventArgs e)
		{
			OnChangedCarrierJobDocAddressRequirement();
		}

		protected virtual void OnChangedCarrierJobDocAddressRequirement()
		{
		}

		protected virtual bool CarrierJobDocAddressCanOverride => true;

		void ValidateCarrier(JobDocAddressValidation validation)
		{
			new NctsDepartureMovementHeaderG0090Validation(this).Validate();

			var isRuleTR0064Applied = IsPhase5
					&& Header.Configuration.ValidationRuleConfiguration.IsRuleTR0064Active
					&& !Carrier.IsEmpty;

			if (isRuleTR0064Applied)
			{
				CheckMandatoryEoriOrTCUCustomCode(Carrier, ValidationRuleCodeConstants.TR0064.GetRuleCodeMessagePrefix(), Res.GetString("C579DADB-A4FD-4E57-A8FE-20CF3D3BB96A", "Carrier"));
			}
			CheckCarrier_NR0072(Carrier);
		}

		void CheckCarrier_NR0072(JobDocAddress carrier)
		{
			if (!carrier.IsEmpty && ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider validationDecider && validationDecider.IsRuleNR0072Active && carrier.Organisation.GetEORI().IsEmpty)
			{
				var hasAdditionalReferenceWithCodeY028AtHeaderLevel = Header.AdditionalDocuments.Any(a => a.IsAnAdditionalReference && a.CSI_Code == NctsConstants.AdditionalInfoCodes.CarrierAEOCertificateNumber);

				if (hasAdditionalReferenceWithCodeY028AtHeaderLevel)
				{
					carrier.OrganisationPKInfo.AddMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.NR0072Message);
				}
			}
		}

		#endregion

		#region IsAirExportTransportMode

		public bool IsAirExportTransportMode => IsAirExportTransportModeCore;
		protected virtual bool IsAirExportTransportModeCore => BM_ExportTransportMode == ModeOfTransportList.Codes._4_AirTransport;

		#endregion

		public bool IsSeaExportTransportMode => BM_ExportTransportMode == ModeOfTransportList.Codes._1_SeaTransport;

		#endregion

		#region IDocAddresses Members

		protected override JobDocAddressRequirement IDocAddressesGetDocAddressRequirementCore(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Carrier:
					return CarrierJobDocAddressRequirement;

				default:
					return base.IDocAddressesGetDocAddressRequirementCore(addressType);
			}
		}

		protected override IReadOnlyList<DocAddressType> IDocAddressesSupportedAddressTypesCore
		{
			get
			{
				var docAddressTypeBase = base.IDocAddressesSupportedAddressTypesCore;
				docAddressTypeBase = docAddressTypeBase.Concat(new DocAddressType[] { DocAddressType.Carrier }).ToArray();
				return docAddressTypeBase;
			}
		}

		#endregion

		#region ICusGoodsLocationProvider

		EU.Business.CusGoodsLocation ICusGoodsLocationProvider.GoodsLocation => GoodsLocation;

		public CusGoodsLocation GoodsLocation
		{
			get
			{
				if (goodsLocation == null)
				{
					goodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Departure);
					RegisterEditableChildObject(goodsLocation);
				}
				return goodsLocation;
			}
		}

		CusGoodsLocation goodsLocation;

		[ResourceStringData("24940D76-D153-4617-97A6-27005B8CA650", Caption = "Location of Goods", ShortCaption = "Location")]
		public ZString GoodsLocationDescription
		{
			get
			{
				if (goodsLocation == null || goodsLocation.IsDeleted)
				{
					goodsLocation = Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Departure);
					if (goodsLocation != null)
					{
						RegisterEditableChildObject(goodsLocation);
					}
				}
				return goodsLocation?.DisplayText ?? ZString.Empty;
			}
		}

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		public void ValidateGoodsLocationDescription()
		{
			Validation.ValidateGoodsLocationDescription();
		}

		ZString ICusGoodsLocationProvider.ProviderKey => DefaultDataGroupingCode + GoodsLocationProviderApplications.Codes.NCTSMovement;

		ICusGoodsLocationValidationDecider ICusGoodsLocationProviderWithValidationDecider.GoodsLocationValidationDecider => Header?.Configuration.MovementHeaderConfiguration.GetGoodsLocationValidationDecider(this);

		public ZBool AllowMixedCaseAuthorisationNumbers => AllowMixedCaseAuthorisationNumbersCore;

		protected virtual ZBool AllowMixedCaseAuthorisationNumbersCore => Header?.Configuration.AllowMixedCaseAuthorisationNumbers ?? ZBool.False;

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			UpdateBM_GrossWeightIfInvalid();
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		protected override ICusInBondMoveDetailCollection CreateMovementDetails() => new CusInBondMoveDetailCollection(this);

		protected override Type MovementDetailTypeCore => Header.IsPhase5 ? typeof(CusInBondMoveDetail) : null;

		public event EventHandler OnFactorySavingAndGrossWeightInvalid;
		public void FactorySavingAndGrossWeightInvalid(object sender, EventArgs args) => OnFactorySavingAndGrossWeightInvalid?.Invoke(sender, args);

		void UpdateBM_GrossWeightIfInvalid()
		{
			if (ShouldTotalWeightOfBillsBeRecalculated
				&& !IsGrossWeightValid)
			{
				FactorySavingAndGrossWeightInvalid(this, EventArgs.Empty);
			}
		}

		public void UpdateBM_GrossWeightFromBills()
		{
			BM_GrossWeight = TotalWeightOfBills.Amount;
			BM_GrossWeightUQ = TotalWeightOfBills.Unit;
		}

		protected override bool BM_PaperlessInbondNumReadOnly => IsPhase5 && !Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.Value;

		protected override bool ShouldGenerateLocalReferenceNumberOnSavingCore => true;

		protected virtual ZBool ShouldTotalWeightOfBillsBeRecalculated => true;

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

		protected override void SetDefaultValuesAfterNctsHeaderIsSet(NctsHeader header)
		{
			if (header.IsPhase5)
			{
				BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			}
		}

		const string PreLodgedString = "PRE-LODGED";

		void ClearInlandTransportModeRelatedProperties()
		{
			AdditionalWagons.RemoveAndDeleteAll();
			BM_TransportAtDeparture = ZString.Empty;
			BM_RN_NKTransportAtDepartureCountry = ZString.Empty;
			BM_TransportAtDepartureTrailer1RegNo = ZString.Empty;
			BM_RN_NKTransportAtDepartureTrailer1Nationality = ZString.Empty;
			BM_TransportAtDepartureTrailer2RegNo = ZString.Empty;
			BM_RN_NKTransportAtDepartureTrailer2Nationality = ZString.Empty;
			BM_TransportAtDepartureType = ZString.Empty;
			BM_AircraftIDAtDeparture = ZString.Empty;
			AdditionalWagons.Clear();
		}

		void ClearExportTransportModeRelatedProperties()
		{
			AdditionalTransportAtBorderList.RemoveAndDeleteAll();
			BM_ActiveBorderIdentificationType = Lookups.TransportAtBorderTypeOfIdList.DefaultCode;
			BM_TOLCarrierID = ZString.Empty;
			BM_RN_NKTOLCarrierNationality = ZString.Empty;
			BM_ConveyanceNumber = ZString.Empty;
			BM_CustomsOfficeAtBorder = ZString.Empty;
		}

		public IDictionary<ZString, Type> GetCusCodeDataTypes() => GetCusCodeDataTypesCore();

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore() => new Dictionary<ZString, Type>
		{
			{ NctsConstants.CusCodeDataTypes.TransportInland, typeof(InlandTransport) },
			{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(NctsEuOfficeCode) },
		};

		internal ZBool IsRuleC0186Applied => IsPhase5 && BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON && Header.Configuration.ValidationRuleConfiguration.IsRuleC0186Active;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				WorkflowItems.RemoveAndDeleteAll();
			}

			CusAuthorizationUsages.RemoveAndDeleteAll();
			base.Delete();
		}

		#region IDepartureTransportMeans

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.ModeOfTransportList))]
		[ResourceStringData("F23F177F-4399-4A1B-99B5-9EF14FAEE308", Caption = "[26] Transport Mode (Inland)", ShortCaption = "Inland M.O.T.", FullDescription = "Inland Mode of Transport (Code)")]
		[ResourceStringData("88887602-AF6A-4DE2-9325-2339CC829D46", Caption = "Departure Transport Mode (Inland)", ShortCaption = "Inland M.O.T.", FullDescription = "[19 04 001 000] Inland Mode of Transport (Code)", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public ZString InlandTransportModeAtDeparture
		{
			get => BM_InlandTransportMode;
			set
			{
				BM_InlandTransportMode = value;
				InlandTransportModeAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo InlandTransportModeAtDepartureInfo => GetWrappedZPropertyInfo(nameof(InlandTransportModeAtDeparture), x => BM_InlandTransportModeInfo);

		[ChildEditable]
		public IBusinessObjectCollection<IAdditionalWagonProvider> AdditionalWagons => InlandTransportList;

		void MakeTransportAtDepartureUpperCaseIfNecessary()
		{
			if (RequireTransportAtDepartureUpperCase)
			{
				VesselNameAtDeparture = VesselNameAtDeparture.ToUpperInvariant();
				TransportAtDeparture = TransportAtDeparture.ToUpperInvariant();
			}
		}

		public bool RequireTransportAtDepartureUpperCase => Factory.GetValue(ref requireTransportAtDepartureUpperCase, () => GetRequireTransportAtDepartureUpperCase());
		CachedProperty<bool> requireTransportAtDepartureUpperCase;

		bool GetRequireTransportAtDepartureUpperCase()
		{
			var header = Header;
			var validationRuleConfiguration = header.Configuration.ValidationRuleConfiguration;

			var requireTransportUpperCaseID = header.IsPhase5Departure
				&& validationRuleConfiguration.IsRuleR0076Active
				&& TransportAtDepartureTypesRequiringUpperCaseIDs().Contains(TransportTypeAtDeparture);

			return requireTransportUpperCaseID
				&& (!validationRuleConfiguration.IsRuleB1811Active || !IsInPhase5TransitionPeriod);
		}

		protected virtual ZString[] TransportAtDepartureTypesRequiringUpperCaseIDs() => Array.Empty<ZString>();

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportAtDepartureTypeOfIdList))]
		[ResourceStringData("BB570688-8575-4C3F-9023-4EFDC03E52F7", Caption = "Type of Identification", ShortCaption = "Type of ID")]
		public ZString TransportTypeAtDeparture
		{
			get => BM_TransportAtDepartureType;
			set
			{
				var oldValue = TransportTypeAtDeparture;
				BM_TransportAtDepartureType = value;
				if (!IsCopying && oldValue != TransportTypeAtDeparture)
				{
					MakeTransportAtDepartureUpperCaseIfNecessary();
				}
				TransportTypeAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransportTypeAtDepartureInfo => GetWrappedZPropertyInfo(nameof(TransportTypeAtDeparture), x => BM_TransportAtDepartureTypeInfo);

		public ZString TransportAtDeparture
		{
			get => BM_TransportAtDeparture;
			set
			{
				var oldValue = TransportAtDeparture;
				BM_TransportAtDeparture = value;
				if (!IsCopying && oldValue != TransportAtDeparture)
				{
					MakeTransportAtDepartureUpperCaseIfNecessary();
				}
				TransportAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransportAtDepartureInfo => GetWrappedZPropertyInfo(nameof(TransportAtDeparture), x => BM_TransportAtDepartureInfo);

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportNationalityList))]
		[ResourceStringData("EF167A5F-D863-4EA8-9AEF-CB1A8424CC6B", Caption = "Nationality")]
		public ZString TransportCountryAtDeparture
		{
			get => BM_RN_NKTransportAtDepartureCountry;
			set
			{
				BM_RN_NKTransportAtDepartureCountry = value;
				TransportCountryAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransportCountryAtDepartureInfo => GetWrappedZPropertyInfo(nameof(TransportCountryAtDeparture), x => BM_RN_NKTransportAtDepartureCountryInfo);

		[ResourceStringData("84CF3760-0281-42C3-85A8-74E68B39693D", Caption = "Trailer 1 ID", MediumCaption = "Trailer 1", ShortCaption = "TRLR 1")]
		public ZString Trailer1IDAtDeparture
		{
			get => BM_TransportAtDepartureTrailer1RegNo;
			set
			{
				BM_TransportAtDepartureTrailer1RegNo = value;
				Trailer1IDAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo Trailer1IDAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer1IDAtDeparture), x => BM_TransportAtDepartureTrailer1RegNoInfo);

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportNationalityList))]
		[ResourceStringData("4CB25698-31E6-4C92-911E-A1EC31E50189", Caption = "Nationality")]
		public ZString Trailer1NationalityAtDeparture
		{
			get => BM_RN_NKTransportAtDepartureTrailer1Nationality;
			set
			{
				BM_RN_NKTransportAtDepartureTrailer1Nationality = value;
				Trailer1NationalityAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo Trailer1NationalityAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer1NationalityAtDeparture), x => BM_RN_NKTransportAtDepartureTrailer1NationalityInfo);

		[ResourceStringData("D1D2957F-8A34-4D64-B915-2B72C1BDA2F0", Caption = "Trailer 2 ID", MediumCaption = "Trailer 2", ShortCaption = "TRLR 2")]
		public ZString Trailer2IDAtDeparture
		{
			get => BM_TransportAtDepartureTrailer2RegNo;
			set
			{
				BM_TransportAtDepartureTrailer2RegNo = value;
				Trailer2IDAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo Trailer2IDAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer2IDAtDeparture), x => BM_TransportAtDepartureTrailer2RegNoInfo);

		[List(nameof(Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportNationalityList))]
		[ResourceStringData("2AFB6B16-E392-4707-AED6-F4A806EA507E", Caption = "Nationality")]
		public ZString Trailer2NationalityAtDeparture
		{
			get => BM_RN_NKTransportAtDepartureTrailer2Nationality;
			set
			{
				BM_RN_NKTransportAtDepartureTrailer2Nationality = value;
				Trailer2NationalityAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo Trailer2NationalityAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer2NationalityAtDeparture), x => BM_RN_NKTransportAtDepartureTrailer2NationalityInfo);

		[ResourceStringData("6937EA3B-C197-4C67-94C1-3418A1FA17EE", Caption = "Aircraft Identification", ShortCaption = "Aircraft ID")]
		public ZString AircraftIDAtDeparture
		{
			get => BM_AircraftIDAtDeparture;
			set
			{
				BM_AircraftIDAtDeparture = value;
				AircraftIDAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AircraftIDAtDepartureInfo => GetWrappedZPropertyInfo(nameof(AircraftIDAtDeparture), x => BM_AircraftIDAtDepartureInfo);

		#endregion IDepartureTransportMeans

		public bool IsCombinedWithExit => BM_TypeOfSecurity.In(new ZString[] { NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH });

		public bool Has30600AdditionalInformation => Factory.GetValue(ref has30600AdditionalInformation, () =>
		{
			return Header.AdditionalDocuments.Has30600AdditionalInformation()
					|| Header.Bills.SelectMany<NctsBill, AdditionalInfo>(b => b.AdditionalDocuments).Has30600AdditionalInformation()
					|| Header.Bills.SelectMany(b => b.GoodsItems.Cast<NctsDepartureCargoDesc>()).SelectMany(b => b.AdditionalInfos).Has30600AdditionalInformation();
		});

		CachedProperty<bool> has30600AdditionalInformation;

		public bool RequireTransportUpperCaseID => Factory.GetValue(ref requireTransportUpperCaseIDCached, () => GetRequireTransportUpperCaseID());
		CachedProperty<bool> requireTransportUpperCaseIDCached;

		bool GetRequireTransportUpperCaseID()
		{
			var header = Header;
			var validationRuleConfiguration = header.Configuration.ValidationRuleConfiguration;

			var requireTransportUpperCaseID = header.IsPhase5Departure
				&& validationRuleConfiguration.IsRuleR0076Active
				&& TransportMeansValidationHelper.TransportTypesRequiringUpperCaseIDs.Contains(BM_ActiveBorderIdentificationType);

			return requireTransportUpperCaseID
				&& (!validationRuleConfiguration.IsRuleB1811Active || !IsInPhase5TransitionPeriod);
		}

		void MakeTOLCarrierIDUpperCaseIfNecessary()
		{
			if (RequireTransportUpperCaseID)
			{
				BM_TOLCarrierID = BM_TOLCarrierID.ToUpperInvariant();
			}
		}

		public const string Phase5SeaExportTransportModeCaptionKey = "738C2764-1194-418C-83C9-1B73F6752F8D";
		public const string DelimiterForCaptionKey = "-";
		public const string SeaVesselPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._11 + DelimiterForCaptionKey + ModeOfTransportList.Codes._1_SeaTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;
		public const string LloydsPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._10 + DelimiterForCaptionKey + ModeOfTransportList.Codes._1_SeaTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;
		public const string WagonPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._20 + DelimiterForCaptionKey + ModeOfTransportList.Codes._2_RailTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;
		public const string TrainPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._21 + DelimiterForCaptionKey + ModeOfTransportList.Codes._2_RailTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;
		public const string FlightPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._40 + DelimiterForCaptionKey + ModeOfTransportList.Codes._4_AirTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;
		public const string RegistrationPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._41 + DelimiterForCaptionKey + ModeOfTransportList.Codes._4_AirTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;
		public const string ENIPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._80 + DelimiterForCaptionKey + ModeOfTransportList.Codes._8_InlandWaterwayTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;
		public const string InlandWaterwayVesselPhase5SeaExportTransportModeCaptionKey = NctsTransportTypeOfIdList.Codes._81 + DelimiterForCaptionKey + ModeOfTransportList.Codes._8_InlandWaterwayTransport + DelimiterForCaptionKey + Phase5SeaExportTransportModeCaptionKey;

		protected override IReadOnlyList<string> MultipleKeysToUseCore
		{
			get
			{
				if (IsPhase5Departure)
				{
					var result = new List<string>(base.MultipleKeysToUseCore);
					if (!BM_ExportTransportMode.IsEmpty)
					{
						var seaExportTransportModeCaptionKey = new ZStringBuilder(BM_ExportTransportMode);
						seaExportTransportModeCaptionKey.Append(Phase5SeaExportTransportModeCaptionKey);
						if (!BM_ActiveBorderIdentificationType.IsEmpty)
						{
							seaExportTransportModeCaptionKey.Prepend(BM_ActiveBorderIdentificationType);
						}
						result.Insert(0, seaExportTransportModeCaptionKey.ToStringWithDelimiterBetweenAppends(DelimiterForCaptionKey));
					}
					return result.ToArray();
				}
				else
				{
					return base.MultipleKeysToUseCore;
				}
			}
		}

		[ChildEditable]
		public ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader> CusAuthorizationUsages
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
		ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader> cusAuthorizationUsages;

		protected virtual ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader>(this);

		SchemaGuidColumn ICusAuthorizationUsageMaster.FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;

		protected virtual void CusAuthorizationUsages_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		public NctsConfiguration Configuration => Header?.Configuration;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newDepartureMovementHeader = (NctsDepartureMovementHeader)base.CloneInternal(args);
			CloneCusAuthorizationUsages(newDepartureMovementHeader);
			CloneGuarantees(newDepartureMovementHeader);
			CloneCusSupplyChainActors(newDepartureMovementHeader);
			return newDepartureMovementHeader;
		}

		void CloneCusAuthorizationUsages(NctsDepartureMovementHeader newDepartureMovementHeader)
		{
			foreach (var cusAuthorizationUsage in CusAuthorizationUsages)
			{
				var newCusAuthorizationUsage = (CusAuthorizationUsage)new NctsDeepCloneStrategy(cusAuthorizationUsage, newDepartureMovementHeader.PK).Clone();
				newDepartureMovementHeader.CusAuthorizationUsages.Add(newCusAuthorizationUsage);
			}
		}

		void CloneGuarantees(NctsDepartureMovementHeader newDepartureMovementHeader)
		{
			if (IsPhase5Departure)
			{
				foreach (NctsGuarantee guarantee in Guarantees.ToArray())
				{
					var newGuarantee = (NctsGuarantee)new NctsDeepCloneStrategy(guarantee, newDepartureMovementHeader.PK).Clone();
					newDepartureMovementHeader.Guarantees.Add(newGuarantee);
				}
			}
		}

		void CloneCusSupplyChainActors(NctsDepartureMovementHeader newHeader)
		{
			foreach (var supplyChainActor in CusSupplyChainActors)
			{
				var newCusSupplyChainActor = (CusSupplyChainActorReference)new NctsDeepCloneStrategy(supplyChainActor, newHeader.PK).Clone();
				newHeader.CusSupplyChainActors.Add(newCusSupplyChainActor);
			}
		}

		public void UpdateAuthorizationUsageFromPrincipal(string authorizationCode, bool authorizationEnabled)
		{
			var header = Header;
			if (header is null)
			{
				return;
			}

			var principalOrg = header.Principal.Organisation;
			if (!authorizationEnabled || principalOrg is null)
			{
				ClearAuthorizations();
			}
			else
			{
				var authorizations = GetAuthorizations();
				switch (authorizations.Length)
				{
					case 0:
						ClearAuthorizations();
						break;
					case 1:
						UpdateAuthorization(authorizations[0]);
						break;
					default:
						if (Header.IsPhase5)
						{
							ProcessAuthorizations(authorizations);
						}
						break;
				}
			}

			CusAuthorisationHeader[] GetAuthorizations()
			{
				var authorizations = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, CountryCode, new ZString[] { authorizationCode }, ZDate.Today, principalOrg.PK);
				if (authorizations.Length == 0)
				{
					if (Configuration.UseCompanyOrgProxyFallBack && GlbBranch.CurrentBranch.GB_OH_OrgProxy == principalOrg.PK && !GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
					{
						authorizations = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, CountryCode, new ZString[] { authorizationCode }, ZDate.Today, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					}
				}
				return authorizations;
			}

			void ClearAuthorizations()
			{
				var cusAuthorizationUsagesToDelete = CusAuthorizationUsages.Where(cau => cau.AGC_Code == authorizationCode).ToArray();
				cusAuthorizationUsagesToDelete.ForEach(cau => CusAuthorizationUsages.RemoveAndDelete(cau));
			}

			void UpdateAuthorization(CusAuthorisationHeader authorizationToUse)
			{
				var authorizationsToUpdate = CusAuthorizationUsages.Where(cau => cau.AGC_Code == authorizationCode).ToList();
				if (authorizationsToUpdate.Count == 0)
				{
					var newAuthorization = CusAuthorizationUsages.AddNew();
					newAuthorization.AGC_Code = authorizationCode;
					authorizationsToUpdate.Add(newAuthorization);
				}

				DefaultDepartureLocationCodeFromCusAuthorisationIfBlank(authorizationToUse);

				foreach (var authorizationToUpdate in authorizationsToUpdate)
				{
					authorizationToUpdate.AGC_Number = authorizationToUse.CPH_Number;
					authorizationToUpdate.AGC_OH_Owner = authorizationToUse.CPH_OH_PermitHolder;
				}
			}

			void ProcessAuthorizations(CusAuthorisationHeader[] authorisations)
			{
				var validAuthorisations = authorisations.Where(a => a.CPH_OA_AppliesTo == header.Principal.E2_OA_Address);
				if (validAuthorisations?.Count() == 1)
				{
					var authorization = validAuthorisations.Single();
					var authorizationsToUpdate = CusAuthorizationUsages.Where(cau => cau.AGC_Code == authorizationCode).ToList();
					if (authorizationsToUpdate.Count == 0)
					{
						var newAuthorization = CusAuthorizationUsages.AddNew();
						newAuthorization.AGC_Code = authorizationCode;
						newAuthorization.AGC_Number = authorization.CPH_Number;
						newAuthorization.AGC_OH_Owner = authorization.CPH_OH_PermitHolder;
					}
					DefaultDepartureLocationCodeFromCusAuthorisationIfBlank(authorization);
				}
			}
		}

		protected virtual void DefaultDepartureLocationCodeFromCusAuthorisationIfBlank(CusAuthorisationHeader authorizationToUse)
		{
			var configuration = Header.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
			if (Header.IsPhase5 && configuration.IsDefaultingEnabled)
			{
				if ((GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty).IsEmpty)
				{
					var locationCode = GetLocationCodeFromCusAuthorisation(authorizationToUse);
					if (locationCode != null)
					{
						GoodsLocation.CGL_Qualifier = configuration.QualifierCode;
						GoodsLocation.CGL_Type = configuration.TypeCode;
						GoodsLocation.Address.IdentificationHolderPK = authorizationToUse.CPH_OH_PermitHolder;
						GoodsLocation.Address.AuthorisationNumber = locationCode.CPR_ValueFrom.SubstringSafe(0, JobDocAddressSchema.E2_GovRegNum.MaxLength);
					}
				}
			}
		}

		protected virtual CusAuthorisationRule GetLocationCodeFromCusAuthorisation(CusAuthorisationHeader authorizationToUse)
		{
			CusAuthorisationRule locationCode = null;
			var locationCodes = authorizationToUse.CusAuthorisationRules?.Where(r => r.CPR_RuleCode == EU.Business.CusAuthorisationRuleTypeList.Codes.Location && !r.CPR_ValueFrom.IsEmpty);
			if (locationCodes.Count() == 1)
			{
				locationCode = locationCodes.Single();
			}
			else if (locationCodes.Count() > 1)
			{
				var locationRules = locationCodes.Where(lc => lc.LinkedCusAuthorisationRules.Any(r => r.CPR_RuleCode == Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice && r.CPR_ValueFrom == DepartureCustomsOfficeCode));
				if (locationRules.Count() == 1)
				{
					locationCode = locationRules.Single();
				}
			}
			return locationCode;
		}

		public override ZString DestinationCustomsOfficeCode => DestinationCustomsOfficeCodeForDeparture;

		protected override ICustomsOffice GetDestinationCustomsOffice() => DestinationCustomsOfficeForDeparture;

		[ResourceStringData("NctsDepartureMovementHeader.DepartureCustomsOfficeCodeForModuleGrid", Caption = "Departure Office", FullDescription = "Customs Office of Departure")]
		public ZString DepartureCustomsOfficeCodeForModuleGrid => IsPhase5 ? DepartureCustomsOfficeCode : Header?.DepartureCustomsOfficeCode ?? ZString.Empty;

		[ResourceStringData("NctsDepartureMovementHeader.DestinationCustomsOfficeCodeForDepartureForModuleGrid", Caption = "Actual Office of Destination", MediumCaption = "Destination Office", ShortCaption = "Dest. Office", FullDescription = "Customs Office of Destination")]
		public ZString DestinationCustomsOfficeCodeForDepartureForModuleGrid => IsPhase5 ? DestinationCustomsOfficeCodeForDeparture : Header?.DestinationCustomsOfficeCodeForDeparture ?? ZString.Empty;

		public NctsMovementHeaderRetransmissionResult ResetMovementForRetransmission()
		{
			if (!IsDepartureRetransmissionAllowed)
			{
				return new NctsMovementHeaderRetransmissionResult(false, Res.GetString("FBAB2E22-B754-4DEE-8130-7ADADA80F170", "Retransmission is not allowed currently because of the customs status."));
			}

			NeedsRegenerateLocalReferenceNumber = true;

			UpdateLocalReferenceNumberForRetransmission();
			UpdateCustomsStatusForRetransmission();
			UpdateMessageStatusForRetransmission();

			return new NctsMovementHeaderRetransmissionResult(true, Res.GetString("D97E7C48-EFE7-400D-B96D-6BE7E187A167", "LRN and status reset. Please save, close and reopen this declaration to edit and retransmit."));
		}

		protected virtual void UpdateLocalReferenceNumberForRetransmission()
		{
			Header.LocalReferenceNumber = ZString.Empty;
		}

		protected virtual void UpdateCustomsStatusForRetransmission()
		{
			Header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
		}

		protected virtual void UpdateMessageStatusForRetransmission()
		{
			Header.MovementHeader.BM_MessageStatus = ZString.Empty;
		}

		public bool IsDepartureRetransmissionAllowed => IsDepartureRetransmissionAllowedCore() && Configuration.IsDepartureRetransmissionSupported;

		protected virtual bool IsDepartureRetransmissionAllowedCore() => Header.MovementHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

		[ResourceStringData("4F3CF840-1258-4DB2-A13A-BFE5794D2487", Caption = "Warehouse Transaction Status Description", ShortCaption = "WHS Trans. Status Des.")]
		public ZString WarehouseTransactionStatusDescription => WarehouseTransactionStatusDescriptionCore;

		protected virtual ZString WarehouseTransactionStatusDescriptionCore => (warehouseTransactionStatusList ??= new WarehouseTransactionStatusList()).GetDescriptionFromCode(BM_WarehouseTransactionStatus) ?? ZString.Empty;

		WarehouseTransactionStatusList warehouseTransactionStatusList;

		GuaranteeTransactionCoordinator guaranteeTransactionCoordinator;
		public GuaranteeTransactionCoordinator GuaranteeTransactionCoordinator => guaranteeTransactionCoordinator ??= GetNewGuaranteeTransactionCoordinator();

		protected virtual GuaranteeTransactionCoordinator GetNewGuaranteeTransactionCoordinator() => new GuaranteeTransactionCoordinator(this);

		#region ICusReferenceTypeSupporter Implementation

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType }
			};
		}

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

		#endregion

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo => docManagerInfo ??= new DepartureMovementHeaderDocManagerInfo(this);
		DocManagerInfo docManagerInfo;

		#endregion

		#region ICusSupportingInfoTypeSupporter Implementation

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.SupportingDocument, SupportingDocumentType },
		};

		protected virtual Type SupportingDocumentType => typeof(NctsSupportingDocument);

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

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var header = Header;
			if (header != null)
			{
				if (!Header.Principal.OrganisationPK.IsEmpty)
				{
					result = AdjustClientPriority(header.GetJobRelatedTemplateSelectionCriteria());
				}
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, BM_InBondEntryType, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType2, BM_AdditionalDeclarationType, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType3, BM_InlandTransportMode, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType4, (ZString)IsSimplifiedNctsProcedure.ToString(), ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType5, BM_RL_NKDestinationPort, ZString.Empty);
			}
			result.Add(ProcessTaskTemplateSchema.P0_GB, header != null && header.BH_GB.IsValid ? header.BH_GB : GlbBranch.CurrentBranch.PK, ZGuid.Empty);

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
					object[] newValues = [Header.Principal.OrganisationPK];

					values = values != null
						? newValues.Union(values).ToArray()
						: newValues;
				}

				result.Add(columnValuesPair.Column, values);
			}
			return result;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader> workflowItems;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor; }
		}

		#endregion

		protected override ProcessTaskCollection GetNewCusInBondMoveHeaderProcessTaskCollection()
		{
			return new ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader>(this);
		}

		#region IWorkflowAffectedPropertyProvider Members

		ZPropertyInfo[] IWorkflowAffectedPropertyProvider.PropertyThatAffectWorkflowChanged
		{
			get
			{
				if (!IsDeleted)
				{
					if (Header is NctsHeader header)
					{
						return
						[
							header.Principal.OrganisationPKInfo,
							BM_InBondEntryTypeInfo,
							BM_AdditionalDeclarationTypeInfo,
							BM_InlandTransportModeInfo,
							IsSimplifiedNctsProcedureInfo,
							BM_RL_NKDestinationPortInfo,
						];
					}
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

		#region CanDelete related implementation

		public override bool CanDelete => base.CanDelete && canDelete;

		public override MultilingualString ReasonForNotAbleToDelete => !canDelete
			? ResString.GetMultilingualString("B50A8B57-44B5-4EC3-A356-088A56B9B83E", "The main departure movement cannot be deleted.")
			: base.ReasonForNotAbleToDelete;

		bool canDelete = true;

		internal void ForbidDeletion()
		{
			canDelete = false;
		}

		#endregion

		ZBool IDepartureTransportMeansProvider.IsInPhase5TransitionPeriod => IsInPhase5TransitionPeriod;

		[ChildEditable(true)]
		public INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = GetSupportingDocuments();
					supportingDocuments.Load();
					supportingDocuments.ListChanged += SupportingDocuments_ListChanged;
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}
		INctsSupportingDocumentCollection<NctsSupportingDocument> supportingDocuments;

		protected virtual INctsSupportingDocumentCollection<NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected virtual void SupportingDocuments_ListChanged(object sender, ListChangedEventArgs e)
		{
		}

		public bool IsLocationManagedInPremises => Factory.GetValue(ref isLocationManagedInPremises, () => GetManagedPremises(Factory, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, GoodsLocation.CGL_AdditionalIdentifier) != null);
		CachedProperty<bool> isLocationManagedInPremises;

		public ZString MovementReferenceNumber => GetMovementReferenceEntryNumberCore()?.CE_EntryNum ?? ZString.Empty;

		internal CusEntryNumber GetMovementReferenceEntryNumberCore()
		{
			if (mrnEntryNumber == null || mrnEntryNumber.IsDeleted)
			{
				mrnEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
				if (mrnEntryNumber != null)
				{
					RegisterEditableChildObject(mrnEntryNumber);
				}
			}
			return mrnEntryNumber;
		}

		CusEntryNumber mrnEntryNumber;
	}
}
