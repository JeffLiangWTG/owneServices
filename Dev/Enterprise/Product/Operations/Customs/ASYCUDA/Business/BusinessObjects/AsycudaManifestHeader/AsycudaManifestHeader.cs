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
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using ImportAction = Enterprise.Customs.Business.ImportAction;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[SystemDefinedValues]
	[CodeProperty(AsycudaManifestHeader.Schema.AMA_JobReference)]
	[DescriptionProperty(nameof(AsycudaManifestHeader.HumanReadableName))]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[UniversalDataContext(DataContextType.AsycudaManifest)]
	public abstract partial class AsycudaManifestHeader : ManifestBase.AsycudaManifestHeader
		, ISynchroniserReadOnlyMembersProvider
		, IJobNumber
		, IManifestHeaderForSynchroniser
		, Integration.Customs.ASYCUDA.IAsycudaManifestHeader
		, IDocAddresses
		, IWorkflowProvider
		, IDocumentSupportable
		, IDocManagerSupport
		, IUniversalXMLNoteParent
		, ISailingParentFindBox
		, ICusCodeDataTypeSupporter
		, IEDIMessageCollectionOwner
		, IValidateForCustomsMessagingSupporter
		, Integration.Customs.ASYCUDA.ISendGlobalManifestMessageSupporter
		, ITransportParent
		, IRoutingSupport
		, IWorkflowTriggerEventSource
	{
		public sealed class ManifestContext
		{
			public IEnumerable<AsycudaBill> ManifestSendBills { get; set; }
			public IEnumerable<AsycudaPack> ManifestSendPacks { get; set; }
		}

		protected AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Aligns Schema with updated base class functionality.\"")]
		public new partial class Schema : ManifestBase.AsycudaManifestHeader.Schema
		{
			public const string AMA_GoodsDescription = "AMA_GoodsDescription";
			public const string AMA_MasterBill = "AMA_MasterBill";
			public const string AMA_MasterBillIssueDate = "AMA_MasterBillIssueDate";
			public const string AMA_CustomsDischargePort = "AMA_CustomsDischargePort";
			public const string AMA_CarrierReference = "AMA_CarrierReference";
			public const string AMA_CustomsOriginPort = "AMA_CustomsOriginPort";
			public const string AMA_CustomsLoadPort = "AMA_CustomsLoadPort";
			public const string AMA_NoOfBills = "AMA_NoOfBills";
			public const string ConsignmentReferenceTracker = "ConsignmentReferenceTracker";
			public const string MasterBol = "MasterBOL";
			public const string AMA_E_ARV = "AMA_E_ARV";
			public const string AMA_E_DEP = "AMA_E_DEP";
			public const string AMA_A_DEP = "AMA_A_DEP";
			public const string AMA_A_ARV = "AMA_A_ARV";
			public const string AMA_RL_NKPortOfDischarge = "AMA_RL_NKPortOfDischarge";
			public const string AMA_RL_NKPortOfLoading = "AMA_RL_NKPortOfLoading";
			public const string AMA_RL_NKOrigin = "AMA_RL_NKOrigin";
			public const string AMA_RL_NKFinalDestination = "AMA_RL_NKFinalDestination";
			public const string ShippingAgentOrgPK = "ShippingAgentOrgPK";
			public const string ShippingAgentName = "ShippingAgentName";
			public const string ShippingAgentAddress = "ShippingAgentAddress";
			public const string ValuationDate = "ValuationDate";
			public const int AMA_CustomsStatusMaxLength = 3;
			public const int MasterBolMaxLength = 35;
			public new const int AMA_VehicleRegistrationMaxLength = 10;
		}

		public new static readonly AsycudaManifestHeaderTypeDecider TypeDecider = new AsycudaManifestHeaderTypeDecider();

		public const string ApplicationCode_ZAOutturnGateInOut = ApplicationCodeTypeList.Codes.ZAOutturnAndGateInOrOut;

		public new BaseAsycudaManifestHeaderValidation Validation => (BaseAsycudaManifestHeaderValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation()
		{
			if (AMA_RN_NKCountry.IsEmpty)
			{
				return new AsycudaManifestHeaderValidation(this);
			}
			else if (IsTrueAsycudaCountry)
			{
				return new AsycudaManifestHeaderValidation(this);
			}
			else
			{
				return new ManifestHeaderValidation(this);
			}
		}

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public static TriLockMutex CreateMutex(ZGuid boPK)
		{
			return new TriLockMutex(MutexIDs.AsycudaManJobBeingCreated, boPK.ToString());
		}

		public bool HasManifestBeenSubmittedToCustomsIncludingChildren
		=> Factory.GetValue(ref hasManifestBeenSubmittedToCustomsIncludingChildrenCached, () => HasManifestBeenSubmittedToCustoms || Bills.Cast<AsycudaBill>().Any(b => b.HasManifestBeenSubmittedToCustomsIncludingChildren));
		CachedProperty<bool> hasManifestBeenSubmittedToCustomsIncludingChildrenCached;

		#region Related Objects

		public new IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		public new IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		public IEnumerable<BillOfLading> BillsOfLadings
		{
			get
			{
				var voyage = Sailing?.Voyage;

				if (billsOfLadings == null && voyage != null)
				{
					billsOfLadings = new List<BillOfLading>(new BillsOfLadingAtPortStrategy(voyage).GetBillsOnVesselAt(Sailing.JX_JB_E_ARV, false));
				}

				return billsOfLadings ?? Enumerable.Empty<BillOfLading>();
			}
		}
		List<BillOfLading> billsOfLadings;

		public BillOfLading BillOfLadingForSync { get; set; }

		#endregion

		internal ZInt ConsignmentReferenceTracker
		{
			get { return this.GetSystemDefinedValue<ZInt>(Schema.ConsignmentReferenceTracker); }
			set
			{
				if (value > 0 && value > ConsignmentReferenceTracker)
				{
					this.SetSystemDefinedValue(Schema.ConsignmentReferenceTracker, value);
				}
			}
		}

		internal ICusInBondContainerCollection ContainersAsICusInBondContainerCollectionForSynching => new AsycudaActiveContainerCollection(this);

		CusPersonCollection persons;
		[ChildEditable]
		public CusPersonCollection Persons
		{
			get
			{
				if (persons == null)
				{
					persons = CreateNewCusPersonCollection();
					persons.Load();
					RegisterEditableChildObject(persons);
				}
				return persons;
			}
		}

		protected virtual CusPersonCollection CreateNewCusPersonCollection() => new CusPersonCollection(this);

		public override void OnSaving()
		{
			base.OnSaving();

			var consol = Consol;
			if (consol == null)
			{
				PopulateNumberPropertyIfRequired(AMA_JobReferenceInfo, x => GetNewJobReference(x));
			}
			else
			{
				consol.PopulateJK_UniqueConsignRefIfNeeded();
				if (AMA_JobReference.IsEmpty || !AMA_JobReference.StartsWith(consol.JK_UniqueConsignRef + "_"))
				{
					AMA_JobReference = GetNewReferenceFromConsol(consol);
				}
			}
		}

		public bool IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml()
		{
			if (Bills.Count > 100)
			{
				return true;
			}
			var packCount = 0;
			foreach (AsycudaBill b in Bills)
			{
				packCount += b.Packs.Count;
			}
			return packCount > 100;
		}

		public virtual ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			var target = new ManifestJobNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.ManifestJobReference,
				FountainGetter = Env.NumberFountains.GetManifestJobReferenceGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		ZString GetNewReferenceFromConsol(ForwardingConsol consol)
		{
			var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
			query.AddToFilter(AsycudaManifestHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_JobReference, SQLComparisonOperator.Contains, consol.JK_UniqueConsignRef);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, ApplicationCode_ZAOutturnGateInOut);
			query.FetchOnlyFromLocalCache = !consol.IsInDatabase;

			var manifests = Factory.Load<AsycudaManifestHeader>(query).Where(x => !x.AMA_JobReference.IsEmpty);
			var manifestReferences = new List<ZInt>();
			foreach (var manifest in manifests)
			{
				var jobReference = manifest.AMA_JobReference;
				if (ZInt.TryParse(jobReference.SubstringSafe(jobReference.LastIndexOf('_') + 1), out var intReference))
				{
					manifestReferences.Add(intReference);
				}
			}

			return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", consol.JK_UniqueConsignRef, manifestReferences.Any() ? manifestReferences.Max() + 1 : 1);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				AMA_JobReference = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		[ChildEditable]
		public IAsycudaArrivalHeaderCollection<AsycudaArrivalHeader> ArrivalHeaders
		{
			get
			{
				if (arrivalHeaders == null)
				{
					arrivalHeaders = CreateNewAsycudaArrivalHeaderCollection();
					RegisterEditableChildObject(arrivalHeaders);
				}
				return arrivalHeaders;
			}
		}
		IAsycudaArrivalHeaderCollection<AsycudaArrivalHeader> arrivalHeaders;

		protected virtual IAsycudaArrivalHeaderCollection<AsycudaArrivalHeader> CreateNewAsycudaArrivalHeaderCollection() => new AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>(this);

		public ZBool NeedPersonsTab => NeedPersonsTabCore;
		protected virtual bool NeedPersonsTabCore => IsRoad;
		public ZPropertyInfo NeedPersonsTabInfo => GetZPropertyInfo(nameof(NeedPersonsTab));

		public bool IsMarkApportionmentDirtySuspended { get; set; }

		public bool IsConsolidator => AMA_ApplicationCode == ApplicationCodeTypeList.Codes.Consolidator;
		public bool IsShippingLine => AMA_ApplicationCode == ApplicationCodeTypeList.Codes.ShippingLine;

		public bool ShowVINNumbers => ShowVINNumbersCore;

		protected virtual bool ShowVINNumbersCore => false;

		public ZString AMA_CustomsStatus => RegistrationEntryNumber?.CE_EntryStatus ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Natures))]
		[ReadOnlyMember(nameof(IsAMA_NatureReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.AMA_Nature", Caption = "Manifest Nature", ShortCaption = "Nature")]
		public override ZString AMA_Nature
		{
			get => base.AMA_Nature;
			set
			{
				var oldValue = AMA_Nature;
				base.AMA_Nature = value;
				if (!IsCopying && oldValue != AMA_Nature)
				{
					OnNatureOrCountryChanged?.Invoke(this, null);
					foreach (AsycudaBill bill in Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected virtual bool IsAMA_NatureReadOnly => false;

		public virtual ZBool IsTSS => AMA_Nature == ShipmentTypeList.Codes.Transhipment28;

		public ZString AMA_NatureDescription => Lookups.Natures.GetDescriptionFromCode(AMA_Nature);

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOffices))]
		public override ZString AMA_CustomsOffice
		{
			get => base.AMA_CustomsOffice;
			set => base.AMA_CustomsOffice = value;
		}

		public ZString AMA_CustomsOfficeDescription => Lookups.CustomsOffices switch
		{
			ICodeDescriptionPairList customsOffices => customsOffices.GetDescriptionFromCode(AMA_CustomsOffice),
			IFindBoxListProvider customsOffices => customsOffices.DescriptionFromCode(AMA_CustomsOffice),
			_ => ZString.Empty,
		};

		[ReadOnlyMember(nameof(AMA_MessageStatus_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MessageStatusList))]
		public override ZString AMA_MessageStatus
		{
			get => base.AMA_MessageStatus;
			set
			{
				if (AMA_MessageStatus != value)
				{
					base.AMA_MessageStatus = value;
					((IStatusSupporter)this).LogEventsOnParent(Events.MessageStatusChange, AMA_MessageStatus);

					if (HasManifestBeenSubmittedToCustoms)
					{
						RemoveSynchroniser();
					}
				}
			}
		}

		protected virtual bool AMA_MessageStatus_ReadOnly => true;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.ManifestTypes))]
		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				var oldValue = AMA_ManifestType;
				if (oldValue != value)
				{
					ResetManifestType();
					base.AMA_ManifestType = value;
					OnManifestTypeChanged?.Invoke(this, null);
					if (!IsCopying)
					{
						ClearDeconcolidateAddressAndTerminalAddress();
						DefaultNatureFromManifestType();
						Bills.MarkAsNeedingValidation();
						Containers.MarkAsNeedingValidation();
						MasterBOLInfo.RefreshBinding();
					}
					foreach (AsycudaBill bill in Bills)
					{
						UpdateAsycudaBillOnManifestTypeChanged(bill, value);
					}
				}
			}
		}

		protected virtual void UpdateAsycudaBillOnManifestTypeChanged(AsycudaBill bill, ZString manifestType)
		{
			if (!bill.IsIssuerCodeMandatory)
			{
				bill.ABL_BillIssuer = ZString.Empty;
			}
		}

		void DefaultNatureFromManifestType()
		{
			var manifestNatures = ManifestType?.ManifestNatures;
			if (manifestNatures != null)
			{
				AMA_Nature = manifestNatures.Count == 1 ? manifestNatures[0].Code : string.Empty;
			}
		}

		void DefaultNatureFromPorts()
		{
			if (!AMA_RN_NKCountry.IsEmpty)
			{
				if (AMA_RL_NKPortOfLoading.StartsWith(AMA_RN_NKCountry, StringComparison.OrdinalIgnoreCase))
				{
					AMA_Nature = ShipmentTypeList.Codes.Export22;
				}
				else if (AMA_RL_NKPortOfDischarge.StartsWith(AMA_RN_NKCountry, StringComparison.OrdinalIgnoreCase))
				{
					AMA_Nature = ShipmentTypeList.Codes.Import23;
				}
			}
		}

		void DefaultNatureFromConsol()
		{
			var consol = Consol;
			if (consol != null)
			{
				var manifestCountry = AMA_RN_NKCountry;
				var transports = consol.Transports;
				var isAnyDischargeInCountry = transports.IsAnyDischargeInCountry(manifestCountry);
				var isAnyLoadInCountry = transports.IsAnyLoadInCountry(manifestCountry);

				if (isAnyLoadInCountry && !isAnyDischargeInCountry)
				{
					AMA_Nature = ShipmentTypeList.Codes.Export22;
				}
				else if (!isAnyLoadInCountry && isAnyDischargeInCountry)
				{
					AMA_Nature = ShipmentTypeList.Codes.Import23;
				}
				else
				{
					AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
				}
			}
		}

		void ClearDeconcolidateAddressAndTerminalAddress()
		{
			if (!AMA_OA_DeconsolidateAddress.IsEmpty && !IsDeconsolidatorEnabled)
			{
				AMA_OA_DeconsolidateAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
				AMA_OA_DeconsolidateAddress = ZGuid.Empty;
			}

			if (!AMA_OA_DischargeTerminalAddress.IsEmpty && !IsDischargeTerminalEnabled)
			{
				AMA_OA_DischargeTerminalAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
				AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
			}
		}

		public ZString AMA_ManifestTypeDescription => Lookups.ManifestTypes.GetDescriptionFromCode(AMA_ManifestType);

		[ResourceStringData("AsycudaManifestHeader.AMA_DateAtCustomsOffice", Caption = "Date at Customs Office", ShortCaption = "Date at Cus. Office")]
		public override ZDateTime AMA_DateAtCustomsOffice
		{
			get => base.AMA_DateAtCustomsOffice;
			set => base.AMA_DateAtCustomsOffice = value;
		}

		#region ShippingAgentOrgPK
		[ReadOnlyMember(nameof(ShippingAgentOrgPKReadOnly))]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Organisations))]
		public ZGuid ShippingAgentOrgPK
		{
			get { return AMA_OA_ShippingAgent_ZAddress.OrgPK; }
			set { AMA_OA_ShippingAgent_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShippingAgentOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShippingAgentOrgPK, x => AMA_OA_ShippingAgent_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ShippingAgentOrg
		{
			get { return Factory.Load<OrgHeader>(ShippingAgentOrgPK); }
		}

		public bool ShippingAgentOrgPKReadOnly => AMA_OA_ShippingAgentInfo.ReadOnly;

		protected override ZAddress GetNewAMA_OA_ShippingAgent_ZAddress()
		{
			var address = base.GetNewAMA_OA_ShippingAgent_ZAddress();
			address.DefaultAddressType = ZArchitecture.Business.AddressType.OFC;
			return address;
		}

		#endregion

		[ResourceStringData("AsycudaManifestHeader.ShippingAgentName", Caption = "Shipping Agent")]
		public ZString ShippingAgentName => ShippingAgent?.EffectiveCompanyNameTruncated ?? ZString.Empty;

		public ZPropertyInfo ShippingAgentNameInfo => GetZPropertyInfo(Schema.ShippingAgentName);

		[ResourceStringData("AsycudaManifestHeader.ShippingAgentAddress", Caption = "Shipping Agent Address")]
		public ZString ShippingAgentAddress => ShippingAgent?.AddressAsASingleLineWithoutCompanyName ?? ZString.Empty;

		public ZPropertyInfo ShippingAgentAddressInfo => GetZPropertyInfo(Schema.ShippingAgentAddress);

		public override ZString AMA_ApplicationCode
		{
			get { return base.AMA_ApplicationCode; }
			set
			{
				var oldValue = AMA_ApplicationCode;
				base.AMA_ApplicationCode = value;
				if (!IsCopying && oldValue != AMA_ApplicationCode)
				{
					MasterBill?.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(AMA_OA_DeconsolidateAddressReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.AMA_OA_DeconsolidateAddress", Caption = "De-consolidator")]
		public override ZGuid AMA_OA_DeconsolidateAddress
		{
			get => base.AMA_OA_DeconsolidateAddress;
			set => base.AMA_OA_DeconsolidateAddress = value;
		}

		protected virtual ZBool AMA_OA_DeconsolidateAddressReadOnly => !IsDeconsolidatorEnabled;
		public ZBool IsDeconsolidatorEnabled => IsDeconsolidatorEnabledCore;
		protected virtual ZBool IsDeconsolidatorEnabledCore => false;

		protected override ZAddress GetNewAMA_OA_DeconsolidateAddress_ZAddress()
		{
			var address = base.GetNewAMA_OA_DeconsolidateAddress_ZAddress();
			address.DefaultAddressType = ZArchitecture.Business.AddressType.OFC;
			return address;
		}

		[ReadOnlyMember(nameof(AMA_OA_DischargeTerminalAddressReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.AMA_OA_DischargeTerminalAddress", Caption = "Discharge Terminal", ShortCaption = "Terminal")]
		public override ZGuid AMA_OA_DischargeTerminalAddress
		{
			get => base.AMA_OA_DischargeTerminalAddress;
			set => base.AMA_OA_DischargeTerminalAddress = value;
		}

		protected virtual ZBool AMA_OA_DischargeTerminalAddressReadOnly => !IsDischargeTerminalEnabled;
		public ZBool IsDischargeTerminalEnabled => IsDischargeTerminalEnabledCore;
		protected virtual ZBool IsDischargeTerminalEnabledCore => false;

		protected override ZAddress GetNewAMA_OA_DischargeTerminalAddress_ZAddress()
		{
			var address = base.GetNewAMA_OA_DischargeTerminalAddress_ZAddress();
			address.DefaultAddressType = ZArchitecture.Business.AddressType.OFC;
			return address;
		}

		[ResourceStringData("AsycudaManifestHeader.AMA_MasterInformation", Caption = "Master\'s Name", ShortCaption = "Master")]
		public override ZString AMA_MasterInformation { get => base.AMA_MasterInformation; set => base.AMA_MasterInformation = value; }

		[ResourceStringData("AsycudaManifestHeader.AMA_RN_NKConveyanceNationality", Caption = "Conveyance Country/Region", ShortCaption = "Convey. Ctry/Rgn.")]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithSailing))]
		public override ZString AMA_RN_NKConveyanceNationality { get => base.AMA_RN_NKConveyanceNationality; set => base.AMA_RN_NKConveyanceNationality = value; }

		[ResourceStringData("AsycudaManifestHeader.AMA_Trailer1RegNo", Caption = "Trailer 1")]
		public override ZString AMA_Trailer1RegNo { get => base.AMA_Trailer1RegNo; set => base.AMA_Trailer1RegNo = value; }

		[ResourceStringData("AsycudaManifestHeader.AMA_RN_NKTrailer1RegCountry", Caption = "Trailer 1 Country")]
		public override ZString AMA_RN_NKTrailer1RegCountry { get => base.AMA_RN_NKTrailer1RegCountry; set => base.AMA_RN_NKTrailer1RegCountry = value; }

		[ResourceStringData("AsycudaManifestHeader.AMA_Trailer2RegNo", Caption = "Trailer 2")]
		public override ZString AMA_Trailer2RegNo { get => base.AMA_Trailer2RegNo; set => base.AMA_Trailer2RegNo = value; }

		[ResourceStringData("AsycudaManifestHeader.AMA_RN_NKTrailer2RegCountry", Caption = "Trailer 2 Country")]
		public override ZString AMA_RN_NKTrailer2RegCountry { get => base.AMA_RN_NKTrailer2RegCountry; set => base.AMA_RN_NKTrailer2RegCountry = value; }

		[ResourceStringData("AsycudaManifestHeader.AMA_IsBuyersConsolidation", Caption = "Buyers Consolidation", ShortCaption = "Buyers Cons.")]
		public override ZBool AMA_IsBuyersConsolidation
		{
			get => base.AMA_IsBuyersConsolidation;
			set
			{
				var oldValue = AMA_IsBuyersConsolidation;
				base.AMA_IsBuyersConsolidation = value;
				if (!IsCopying && oldValue != AMA_IsBuyersConsolidation)
				{
					Containers.MarkAsNeedingValidation();
				}
			}
		}

		public AsycudaBill MasterBill
		{
			get
			{
				if (!IsDeleting && !IsDeleted && (fMasterBill == null || fMasterBill.IsDeleted))
				{
					fMasterBill = Factory.Load<AsycudaBill>(GetMasterBillQuery()).OrderBy(x => x.ABL_SystemCreateTimeUtc).FirstOrDefault();
					if (fMasterBill == null)
					{
						fMasterBill = (AsycudaBill)Factory.New(GetBillType());
						fMasterBill.ABL_BolType = AsycudaBill.ChildBolCode;
						fMasterBill.ABL_AMA = PK;
						fMasterBill.ABL_ClusterKey = AMA_ClusterKey;
						if (!IsInDatabase)
						{
							fMasterBill.SetHeader(this);
						}
					}
					RegisterEditableChildObject(fMasterBill);
				}
				return fMasterBill;
			}
		}
		AsycudaBill fMasterBill;

		internal ZQuery GetMasterBillQuery()
		{
			var query = DataHelper.GenerateClusterKeyQuery(AMA_ClusterKey, PK, AsycudaBillSchema.ABL_ClusterKey, AsycudaBillSchema.ABL_AMA, !IsInDatabase);
			query.AddToFilter(new ZQuery(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode));
			return query;
		}

		[MaxLength(AsycudaBill.Schema.ABL_BillNumberMaxLength)]
		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsAMA_MasterBillReadOnly))]
		public virtual ZString AMA_MasterBill
		{
			get { return MasterBill?.ABL_BillNumber ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_MasterBill;
				CheckMaximumLength(AMA_MasterBillInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_BillNumber = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAMA_MasterBill();
				}
				AMA_MasterBillInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_MasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.AMA_MasterBill); }
		}

		protected virtual bool IsAMA_MasterBillReadOnly => false;

		[BusinessObjectTestExclude]
		[ResourceStringData("AsycudaManifestHeader.AMA_MasterBillIssueDate", Caption = "Issue Date")]
		public virtual ZDate AMA_MasterBillIssueDate
		{
			get { return MasterBill?.ABL_BillIssueDate ?? ZDate.Empty; }
			set
			{
				var oldValue = AMA_MasterBillIssueDate;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_BillIssueDate = value;
				}
				AMA_MasterBillIssueDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_MasterBillIssueDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_MasterBillIssueDate, x => MasterBill?.ABL_BillIssueDateInfo); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public virtual ZDateTime AMA_E_ARV
		{
			get { return MasterBill?.ABL_E_ARV ?? ZDateTime.Empty; }
			set
			{
				var oldValue = AMA_E_ARV;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_E_ARV = value;
				}
				AMA_E_ARVInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_E_ARVInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_E_ARV, x => MasterBill?.ABL_E_ARVInfo); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public virtual ZDateTime AMA_E_DEP
		{
			get { return MasterBill?.ABL_E_DEP ?? ZDateTime.Empty; }
			set
			{
				var oldValue = AMA_E_DEP;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_E_DEP = value;
				}
				AMA_E_DEPInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_E_DEPInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_E_DEP, x => MasterBill?.ABL_E_DEPInfo); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		[ResourceStringData("67C7FA76-B869-4C9F-98D2-92C4B7AE9F0E", Caption = "Actual Departure Time", MediumCaption = "Act. Departure", ShortCaption = "ATD")]
		public virtual ZDateTimeOffset AMA_A_DEP
		{
			get { return MasterBill?.ABL_A_DEP ?? ZDateTimeOffset.Empty; }
			set
			{
				var oldValue = AMA_A_DEP;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_A_DEP = value;
				}
				AMA_A_DEPInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_A_DEPInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_A_DEP, x => MasterBill?.ABL_A_DEPInfo); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public virtual ZDateTime AMA_A_ARV
		{
			get { return MasterBill?.ABL_A_ARV ?? ZDateTime.Empty; }
			set
			{
				var oldValue = AMA_A_ARV;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_A_ARV = value;
				}
				AMA_A_ARVInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_A_ARVInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_A_ARV, x => MasterBill?.ABL_A_ARVInfo); }
		}

		[MaxLength(AsycudaManifestHeader.Schema.AMA_VehicleRegistrationMaxLength)]
		public override ZString AMA_VehicleRegistration
		{
			get { return base.AMA_VehicleRegistration; }
			set { base.AMA_VehicleRegistration = value; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				RemoveSynchroniser();
				FetchStrategy.FetchForDelete();
				MasterBill?.Delete();
				fMasterBill = null;
				Persons.RemoveAndDeleteAll();
				Messages.RemoveAndDeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				if (!IsDeleted)
				{
					result.AddRange(Bills);
					result.AddRange(Bills.SelectMany(bill => bill.BusinessObjectsWithRelatedEvents));
				}

				return result.ToArray();
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		protected override ZAddress GetNewAMA_OA_Carrier_ZAddress()
		{
			var zAddress = base.GetNewAMA_OA_Carrier_ZAddress();
			zAddress.DefaultAddressType = ZArchitecture.Business.AddressType.OFC;
			return zAddress;
		}

		public override ZBool AMA_OverrideFreightDefaults
		{
			get => base.AMA_OverrideFreightDefaults;
			set => SetOverrideDefaults(value);
		}

		void SetOverrideDefaults(ZBool value)
		{
			var oldValue = AMA_OverrideFreightDefaults;

			if (oldValue != value)
			{
				var source = (IBusinessObjectState)Consol ?? Sailing;

				if (!value && source != null)
				{
					base.AMA_OverrideFreightDefaults = value;
					Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				}
				else
				{
					base.AMA_OverrideFreightDefaults = value;
					AMA_OverrideFreightDefaultsInfo.RefreshBinding(oldValue);

					RemoveSynchroniser();
				}

				RefreshBindingIncludingChildren();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsStandAlone || IsManifestConsolDecouplingEnabled)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		public static bool IsManifestConsolDecouplingEnabled => ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.Value;

		internal void RemoveSynchroniser()
		{
			if (fSynchroniser != null)
			{
				var source = (IBusinessObjectState)Consol ?? Sailing;

				if (source != null)
				{
#pragma warning disable
					source.UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
				}

				fSynchroniser.SetEnabled(false, fSynchroniser.DetectEnabled);
				fSynchroniser.Dispose();
				fSynchroniser = null;
			}
		}

		public ZBool IsOverrideFreightDefaultsVisible
		{
			get { return Consol != null || IsSea && Sailing != null; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			AMA_RN_NKCountry = GetDefaultCountryCodeForTesting();
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_RL_NKPortOfDischarge = "ERXXX";
			AMA_Nature = "IMP";
			AMA_JobReference = "C123456";
		}

		protected virtual ZString GetDefaultCountryCodeForTesting()
		{
			var result = GetDefaultCountryCode();
			return result.IsEmpty ? (ZString)Core.Constants.CountryCodes.Eritrea : result;
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new AsycudaManifestHeaderTestDataHelper();
		}

		class AsycudaManifestHeaderTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateString(ZPropertyInfo property)
			{
				if (property.Name != "AMA_RN_NKCountry")
				{
					base.PopulateString(property);
				}
			}
		}
#endif

		public ZString ValidateEverythingAndSummariseProblems()
		{
			RunPreSaveValidationWithFetchHints();
			var notifications = MessageSendingValidation.New(this, null).CheckBusinessObjectLevelValidation();
			return notifications.NotificationsAsString();
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.AgentTypeList))]
		public override ZString AMA_AgentType
		{
			get { return base.AMA_AgentType; }
			set
			{
				base.AMA_AgentType = value;
				if (value != Core.Constants.AgentType.CoLoad)
				{
					MasterBOL = ZString.Empty;
				}
			}
		}

		[ResourceStringData("AsycudaManifestHeader.MasterBOL", Caption = "Master BOL")]
		[MaxLength(Schema.MasterBolMaxLength)]
		public virtual ZString MasterBOL
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.MasterBol);
			set
			{
				var oldValue = MasterBOL;
				CheckMaximumLength(MasterBOLInfo, value);
				this.SetSystemDefinedValue(Schema.MasterBol, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMasterBOL();
				}
				MasterBOLInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo MasterBOLInfo => GetZPropertyInfo(Schema.MasterBol);

		public ZBool MasterBOLVisible
		{
			get
			{
				var (isMandatory, _, _, _) = IsMasterBOLMandatory();
				return isMandatory;
			}
		}

		internal (ZBool IsMandatory, ZString agentType, ZString manifestType, RefCountry country) IsMasterBOLMandatory()
		{
			var agentType = AMA_AgentType;
			var countryCode = AMA_RN_NKCountry;
			var type = AMA_ManifestType;
			ZBool isMandatory = ZZDatabaseValidationHelper.IsMandatoryForOneCountryWhenAttributeMatches(
									Factory, countryCode,
									Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MasterBOL,
									Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MANDATORYFORAGENTTYPE,
									agentType)
								&& ZZDatabaseValidationHelper.IsMandatoryForOneCountryWhenAttributeMatches(
									Factory, countryCode,
									Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MasterBOL,
									Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE,
									type);
			RefCountry country = null;
			if (isMandatory)
			{
				country = Country;
			}
			return (isMandatory, agentType, manifestType: type, country);
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TransportModeList))]
		public override ZString AMA_TransportMode
		{
			get { return base.AMA_TransportMode; }
			set
			{
				var oldValue = AMA_TransportMode;
				base.AMA_TransportMode = value;
				if (oldValue != AMA_TransportMode)
				{
					OnTransportModeChanged?.Invoke(this, null);
					NeedPersonsTabInfo.RefreshBinding();
					DefaultOfficeCodeIfNeed();
					DefaultNature();
					UpdateContainerMode();
					DefaultManifestTypeOnceAttachToManifestAndCountryIsSet();
					MasterBill?.MarkAsNeedingValidation();
					Bills?.MarkAsNeedingValidation();
					ClearSailing();
					ClearVesselVoyage(oldValue == Core.Constants.TransportModes.Sea);
					ClearPersons();
				}
			}
		}

		void UpdateContainerMode()
		{
			if (IsAir)
			{
				AMA_ContainerMode = Core.Constants.ContainerModes.Other;
			}
		}

		void ClearSailing()
		{
			if (!CanBeLinkedWithSailing && Sailing != null)
			{
				ClearSailing(false);
			}
		}

		void ClearVesselVoyage(bool wasSea)
		{
			if (wasSea)
			{
				AMA_Voyage = ZString.Empty;
				ClearVesselFields();
			}
		}

		void ClearPersons()
		{
			if (!NeedPersonsTab && Persons.Any())
			{
				Persons.RemoveAndDeleteAll();
			}
		}

		public event EventHandler<EventArgs> OnTransportModeChanged;
		public event EventHandler<EventArgs> OnNatureOrCountryChanged;
		public event EventHandler<EventArgs> OnManifestTypeChanged;
		public event EventHandler<EventArgs> OnSpecificCircumstanceIndicatorChanged;

		public ZString AMA_TransportModeDescription => Lookups.TransportModeList.GetDescriptionFromCode(AMA_TransportMode);

		public ZBool IsAir => AMA_TransportMode == Core.Constants.TransportModes.Air;

		public virtual ZBool IsSea => AMA_TransportMode == Core.Constants.TransportModes.Sea;

		public ZBool IsInlandWaterway => AMA_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport;

		public virtual ZBool IsRoad => AMA_TransportMode == Core.Constants.TransportModes.Road;

		public ZBool IsRail => AMA_TransportMode == Core.Constants.TransportModes.Rail;

		public ZBool IsMail => AMA_TransportMode == Core.Constants.TransportModes.Mail;

		public ZBool IsStandAlone => Factory.GetValue(ref isStandAlone, () => Consol == null);

		CachedProperty<ZBool> isStandAlone;

		public ForwardingConsol Consol
		{
			get { return AMA_ParentTableCode == JobConsolSchema.Constants.Prefix ? Factory.Load<ForwardingConsol>(AMA_ParentId) : null; }
		}

		public override ZGuid AMA_ParentId
		{
			get => base.AMA_ParentId;
			set
			{
				base.AMA_ParentId = value;
				foreach (var bill in Bills)
				{
					bill.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString AMA_ParentTableCode
		{
			get => base.AMA_ParentTableCode;
			set
			{
				base.AMA_ParentTableCode = value;

				foreach (var bill in Bills)
				{
					bill.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public override ZString AMA_Voyage
		{
			get => base.AMA_Voyage;
			set => base.AMA_Voyage = value;
		}

		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set => base.AMA_OA_Carrier = value;
		}

		public ZString CarrierCCCCode
		{
			get
			{
				var addressCCC = Carrier?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, AMA_RN_NKCountry) ?? ZString.Empty;
				if (!addressCCC.IsEmpty)
				{
					return addressCCC;
				}

				var orgCCC = Carrier?.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, AMA_RN_NKCountry) ?? ZString.Empty;
				if (!orgCCC.IsEmpty)
				{
					return orgCCC;
				}

				return ZString.Empty;
			}
		}
		protected internal RefVesselZZ GetZZVesselFromGlobalVessel(RefVessel globalVessel)
		{
			return globalVessel != null ? RefVesselZZ.LookupVesselByCode(globalVessel.RV_Code, AMA_RN_NKCountry, globalVessel.Factory) : null;
		}

		protected internal ZZRefCarrierCombined GetZZCarrier(ZString carrierType, ZString carrierCode)
		{
			var query = ZZRefCarrierCombinedCollection.GetLoadingQuery(AMA_RN_NKCountry
				, carrierType
				, AMA_TransportMode);
			query.AddToFilter(ZZRefCarrierCombinedSchema.ZZ4_Code, SQLComparisonOperator.Equal, carrierCode);

			return Factory.LoadTop1<ZZRefCarrierCombined>(query);
		}

		public void SetParent(BusinessObject parent)
		{
			AMA_ParentId = parent?.PK ?? ZGuid.Empty;
			AMA_ParentTableCode = parent?.TablePrefix ?? ZString.Empty;
		}

		public ZZDatabaseValidationHelper ZZValidationHelper => validationHeaderHelper ?? GetNewZZValidationHelper();
		ZZDatabaseValidationHelper validationHeaderHelper;

		protected virtual ZZDatabaseValidationHelper GetNewZZValidationHelper() => new ZZDatabaseValidationHelper(this);

		#region Enable Bills Lock

		public void EnableBillsLock(bool enable)
		{
			if (SupportBillLock() && IsBillLockEnabled != enable)
			{
				IsBillLockEnabled = enable;
				HookCountryCodeEvents();
				FetchStrategy.FetchForEnableBillsLock();
				SetupBillLock();
			}
		}

		public bool IsBillLockEnabled { get; private set; }

		public void RefreshBillLock()
		{
			foreach (AsycudaBill bill in Bills)
			{
				RefreshBillLock(bill);
			}
		}

		protected virtual bool SupportBillLock() => false;

		internal void RefreshBillLock(AsycudaBill bill)
		{
			if (bill != null)
			{
				if (supportBillLock && bill.ShouldBeReadOnly)
				{
					bill.SetReadOnlyIncludingChildren(true);
					bill.ReadOnly = false;
					SetPropertyInfoReadOnly(bill, propInfo => propInfo.Name != bill.ABL_RemarksInfo.Name);
					UnLockPacksIfNeed(bill);
				}
				else
				{
					bill.SetReadOnlyIncludingChildren(false);
					SetPropertyInfoReadOnly(bill, info => false);
				}
				HookBillEvents(bill);
			}
		}

		void SetPropertyInfoReadOnly(AsycudaBill bill, Func<ZPropertyInfo, ZBool> propertyInfoReadOnlySetter)
		{
			foreach (ZPropertyInfo propInfo in bill.ZPropertyInfoHash)
			{
				if (propInfo.HasSetter)
				{
					((IZPropertyInfoObsolete)propInfo).ReadOnly = propertyInfoReadOnlySetter?.Invoke(propInfo) ?? ZBool.False;
				}
			}
		}

		bool supportBillLock;

		void HookBillEvents(AsycudaBill bill)
		{
			if (ShowPackedItems)
			{
				foreach (AsycudaPack pack in bill.Packs)
				{
					var packedItem = pack.GetPackedItemFromCollection();
					if (packedItem != null)
					{
						HookPackedItemEvents(packedItem);
					}
				}
			}
		}

		void HookPackedItemEvents(AsycudaPackedItem packedItem)
		{
			packedItem.API_MessageStatusInfo.ValueChanged -= API_MessageStatusInfo_ValueChanged;
			if (supportBillLock)
			{
				packedItem.API_MessageStatusInfo.ValueChanged += API_MessageStatusInfo_ValueChanged;
			}
		}

		void API_MessageStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			var packedItem = (e as ValueChangedEventArgs)?.Info?.BizObj as AsycudaPackedItem;
			UpdateBillLockIfNeeded(packedItem);
		}

		void UpdateBillLockIfNeeded(AsycudaPackedItem packedItem)
		{
			var pack = packedItem?.Pack;
			var bill = pack?.Bill;
			if (bill != null)
			{
				var currentReadOnly = bill.ReadOnly;
				var newReadOnly = supportBillLock && bill.ShouldBeReadOnly;
				bill.SetReadOnlyIncludingChildren(newReadOnly);
				if (newReadOnly)
				{
					SetPropertyInfoReadOnly(bill, propInfo => propInfo.Name != bill.ABL_RemarksInfo.Name);
					UnLockPacksIfNeed(bill);
				}
				else
				{
					SetPropertyInfoReadOnly(bill, info => false);
				}
				SetupAdditionalLockingIfNeeded(packedItem, pack, bill, currentReadOnly, newReadOnly);
			}
		}

		protected virtual void SetupAdditionalLockingIfNeeded(AsycudaPackedItem packedItem, AsycudaPack pack, AsycudaBill bill, bool currentReadOnly, bool newReadOnly)
		{
		}

		protected virtual void UnLockPacksIfNeed(AsycudaBill bill)
		{
		}

		void Bills_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var bill = e.BizObject as AsycudaBill;
			if (bill != null)
			{
				HookBillEvents(bill);
			}
		}

		void AMA_RN_NKCountryInfo_ValueChanged(object sender, EventArgs e) => SetupBillLock();

		void SetupBillLock()
		{
			var oldSupportBillLock = supportBillLock;
			supportBillLock = IsBillLockEnabled && SupportBillLock();
			if (oldSupportBillLock != supportBillLock)
			{
				Bills.CountChanged -= Bills_CountChanged;
				if (supportBillLock)
				{
					Bills.CountChanged += Bills_CountChanged;
				}
				RefreshBillLock();
			}
		}

		void HookCountryCodeEvents()
		{
			AMA_RN_NKCountryInfo.ValueChanged -= AMA_RN_NKCountryInfo_ValueChanged;
			if (IsBillLockEnabled)
			{
				AMA_RN_NKCountryInfo.ValueChanged += AMA_RN_NKCountryInfo_ValueChanged;
			}
		}

		#endregion

		public ZBool ShouldSynchroniseWithConsol => !AMA_OverrideFreightDefaults && Consol != null;

		#region
		IForwardingConsol IManifestHeaderForSynchroniser.Consol => Consol;

		IManifestBillForSynchroniser IManifestHeaderForSynchroniser.AddNewBill() => Bills.AddNew();

		IEnumerable<IManifestBillForSynchroniser> IManifestHeaderForSynchroniser.Bills => Bills.Cast<AsycudaBill>();

		bool IManifestHeaderForSynchroniser.OverrideFreightDefaults => AMA_OverrideFreightDefaults;

		#endregion

		public ZBool ShouldSynchroniseWithSailing => !AMA_OverrideFreightDefaults && IsSea && Sailing != null;

		public ZBool ShouldSynchroniseWithConsolOrSailing => ShouldSynchroniseWithConsol || ShouldSynchroniseWithSailing;

		public void SynchroniseWithSourceIfNeeded()
		{
			if (ShouldSynchroniseWithConsolOrSailing && !HasSentMessages)
			{
				try
				{
					using (SuspendSettingHasChanges())
					using (GetValidationSuspender())
					{
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
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

		public BusinessObjectSynchroniser Synchroniser
		{
			get
			{
				if (fSynchroniser == null)
				{
					var source = (IBusinessObjectState)Consol ?? Sailing;

					if (source != null)
					{
						fSynchroniser = AMA_ParentTableCode == JobConsolSchema.Constants.Prefix
							? GetConsolSynchronizerCore((ForwardingConsol)source)
							: new AsycudaManifestHeaderSailingSynchroniser(this, (JobSailing)source);
#pragma warning disable
						source.UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
						source.UpdatedByDataRefreshIncludingChildren += OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
					}
					else
					{
						throw new NotSupportedException("You can't synchronise with when you don't have a consol or sailing.");
					}
				}

				return fSynchroniser;
			}
		}
		BusinessObjectSynchroniser fSynchroniser;
		protected virtual BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source)
			=> new AsycudaManifestHeaderSynchroniser(this, source);

		bool HasSentMessages => HasManifestBeenSubmittedToCustoms;

		internal void OnConsolWasUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			if (fSynchroniser != null && !IsDeleted && AMA_ParentTableCode == JobConsolSchema.Constants.Prefix)
			{
				SynchroniseWithSourceIfNeeded();
			}
		}

		void ClearSynchronisationHasChanges()
		{
			((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
		}

		public void CopyAdditionalConsolShipmentToBills()
		{
			var shipments = Consol?.Shipments;
			AsycudaBill bill;
			if (shipments != null)
			{
				foreach (var shipment in shipments.ToArray<ForwardingShipment>())
				{
					var billNumber = shipment.JS_HouseBill;
					if (!Bills.Any(x => !x.IsDeleted && x.ABL_BillNumber == billNumber))
					{
						bill = Bills.AddNew();
						var synchroniser = new AsycudaBillSynchroniser(bill, shipment);
						synchroniser.Synchronise(true);
						synchroniser.SetEnabled(false, false);
						synchroniser.Dispose();
						synchroniser = null;
					}
				}
			}
		}

		[MaxLength(AsycudaBill.Schema.ABL_RL_NKPortOfDischargeMaxLength)]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.DischargePortList))]
		public virtual ZString AMA_RL_NKPortOfDischarge
		{
			get { return MasterBill?.ABL_RL_NKPortOfDischarge ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_RL_NKPortOfDischarge;
				CheckMaximumLength(AMA_RL_NKPortOfDischargeInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_RL_NKPortOfDischarge = value;
				}
				if (!IsCopying && oldValue != AMA_RL_NKPortOfDischarge)
				{
					DefaultOfficeCodeIfNeed();
					DefaultNature();
					DefaultCustomsDischargePort(value);
					Bills.MarkAsNeedingValidation();
				}
				AMA_RL_NKPortOfDischargeInfo.RefreshBinding(oldValue);
			}
		}

		public void DefaultCustomsLoadPort(ZString loadPortCode)
		{
			if ((FeatureProvider?.SupportsCustomsPorts(this) ?? false) && !loadPortCode.IsEmpty && MasterBill != null)
			{
				MasterBill.ABL_CustomsLoadPort = GetCustomsLocalCode(loadPortCode, PortOfLoading);
			}
		}

		public void DefaultCustomsDischargePort(ZString dischargePortCode)
		{
			if ((FeatureProvider?.SupportsCustomsPorts(this) ?? false) && !dischargePortCode.IsEmpty && MasterBill != null)
			{
				MasterBill.ABL_CustomsDischargePort = GetCustomsLocalCode(dischargePortCode, PortOfDischarge);
			}
		}

		public ZPropertyInfo AMA_RL_NKPortOfDischargeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_RL_NKPortOfDischarge, x => MasterBill?.ABL_RL_NKPortOfDischargeInfo); }
		}

		public virtual RefUNLOCO PortOfDischarge
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, AMA_RL_NKPortOfDischarge); }
		}

		[MaxLength(AsycudaBill.Schema.ABL_CustomsOriginPortMaxLength)]
		public virtual ZString AMA_CustomsOriginPort
		{
			get { return MasterBill?.ABL_CustomsOriginPort ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_CustomsOriginPort;
				CheckMaximumLength(AMA_CustomsOriginPortInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_CustomsOriginPort = value;
				}
				AMA_CustomsOriginPortInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_CustomsOriginPortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_CustomsOriginPort, x => MasterBill?.ABL_CustomsOriginPortInfo); }
		}

		[MaxLength(AsycudaBill.Schema.ABL_CustomsLoadPort)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsLoadingPortList))]
		[ResourceStringData("AsycudaManifestHeader.AMA_CustomsLoadPort", Caption = "Customs Load Port")]
		public virtual ZString AMA_CustomsLoadPort
		{
			get { return MasterBill?.ABL_CustomsLoadPort ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_CustomsLoadPort;
				CheckMaximumLength(AMA_CustomsLoadPortInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_CustomsLoadPort = value;
				}
				if (!IsCopying && oldValue != AMA_CustomsLoadPort)
				{
					Bills.MarkAsNeedingValidation();
				}
				AMA_CustomsLoadPortInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo AMA_CustomsLoadPortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_CustomsLoadPort, x => MasterBill?.ABL_CustomsLoadPortInfo); }
		}

		[MaxLength(AsycudaBill.Schema.ABL_CustomsDischargePort)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsDischargePortList))]
		[ResourceStringData("AsycudaManifestHeader.AMA_CustomsDischargePort", Caption = "Customs Discharge Port", ShortCaption = "Customs Discharge")]
		public virtual ZString AMA_CustomsDischargePort
		{
			get { return MasterBill?.ABL_CustomsDischargePort ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_CustomsDischargePort;
				CheckMaximumLength(AMA_CustomsDischargePortInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_CustomsDischargePort = value;
				}
				if (!IsCopying && oldValue != AMA_CustomsDischargePort)
				{
					Bills.MarkAsNeedingValidation();
				}
				AMA_CustomsDischargePortInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo AMA_CustomsDischargePortInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_CustomsDischargePort, x => MasterBill?.ABL_CustomsDischargePortInfo); }
		}

		public virtual ZString AMA_CarrierReference
		{
			get
			{
				return MasterBill?.ABL_CarrierReference ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(AMA_CarrierReferenceInfo, value);
				var oldValue = AMA_CarrierReference;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					MasterBill.ABL_CarrierReference = value;
				}
				AMA_CarrierReferenceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_CarrierReferenceInfo => GetWrappedZPropertyInfo(Schema.AMA_CarrierReference, x => MasterBill?.ABL_CarrierReferenceInfo);

		[MaxLength(AsycudaBill.Schema.ABL_RL_NKPortOfLoadingMaxLength)]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.LoadingPortList))]
		public virtual ZString AMA_RL_NKPortOfLoading
		{
			get { return MasterBill?.ABL_RL_NKPortOfLoading ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_RL_NKPortOfLoading;
				CheckMaximumLength(AMA_RL_NKPortOfLoadingInfo, value);

				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_RL_NKPortOfLoading = value;
				}

				if (!IsCopying && oldValue != AMA_RL_NKPortOfLoading)
				{
					DefaultOfficeCodeIfNeed();
					DefaultNature();
					DefaultCustomsLoadPort(value);
					Bills.MarkAsNeedingValidation();
				}

				AMA_RL_NKPortOfLoadingInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_RL_NKPortOfLoadingInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AMA_RL_NKPortOfLoading, x => MasterBill?.ABL_RL_NKPortOfLoadingInfo); }
		}

		public RefUNLOCO PortOfLoading
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, AMA_RL_NKPortOfLoading); }
		}

		[MaxLength(AsycudaBill.Schema.ABL_RL_NKOriginMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.OriginPortList))]
		[ResourceStringData("AsycudaManifestHeader.AMA_RL_NKOrigin", Caption = "Origin Port", ShortCaption = "Origin")]
		public virtual ZString AMA_RL_NKOrigin
		{
			get { return MasterBill?.ABL_RL_NKOrigin ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_RL_NKOrigin;
				CheckMaximumLength(AMA_RL_NKOriginInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_RL_NKOrigin = value;
				}

				if (!IsCopying && oldValue != AMA_RL_NKOrigin)
				{
					Bills.MarkAsNeedingValidation();
				}

				AMA_RL_NKOriginInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_RL_NKOriginInfo => GetWrappedZPropertyInfo(Schema.AMA_RL_NKOrigin, x => MasterBill?.ABL_RL_NKOriginInfo);

		[MaxLength(AsycudaBill.Schema.ABL_RL_NKFinalDestination)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.DestinationPortList))]
		[ResourceStringData("AsycudaManifestHeader.AMA_RL_NKFinalDestination", Caption = "Destination Port", MediumCaption = "Destination", ShortCaption = "Destination")]
		public virtual ZString AMA_RL_NKFinalDestination
		{
			get { return MasterBill?.ABL_RL_NKFinalDestination ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_RL_NKFinalDestination;
				CheckMaximumLength(AMA_RL_NKFinalDestinationInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_RL_NKFinalDestination = value;
				}

				if (!IsCopying && oldValue != AMA_RL_NKFinalDestination)
				{
					Bills.MarkAsNeedingValidation();
				}

				AMA_RL_NKFinalDestinationInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo AMA_RL_NKFinalDestinationInfo => GetWrappedZPropertyInfo(Schema.AMA_RL_NKFinalDestination, x => MasterBill?.ABL_RL_NKFinalDestinationInfo);

		[MaxLength(AsycudaBill.Schema.ABL_GoodsDescriptionMaxLength)]
		public virtual ZString AMA_GoodsDescription
		{
			get { return MasterBill?.ABL_GoodsDescription ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_GoodsDescription;
				CheckMaximumLength(AMA_GoodsDescriptionInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_GoodsDescription = value;
				}

				AMA_GoodsDescriptionInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_GoodsDescriptionInfo => GetWrappedZPropertyInfo(Schema.AMA_GoodsDescription, x => MasterBill?.ABL_GoodsDescriptionInfo);

		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public override ZString AMA_VesselName
		{
			get => base.AMA_VesselName;
			set => base.AMA_VesselName = value;
		}

		internal CountryHelper CountryHelper
		{
			get { return countryHelper ?? (countryHelper = new CountryHelper(this)); }
		}
		CountryHelper countryHelper;

		public ManifestContext GetCurrentManifestContext()
		{
			return manifestContext ?? (manifestContext = new ManifestContext());
		}
		ManifestContext manifestContext;

		[ChildEditable]
		public EDIMessageCollectionNonDependent Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = CreateNewEDIMessageCollection();
					ediMessages.Load();

					AddUXMLMessagesToCollectionIfRequired();

					ediMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessages);
				}
				return ediMessages;
			}
		}
		EDIMessageCollectionNonDependent ediMessages;

		protected virtual EDIMessageCollectionNonDependent CreateNewEDIMessageCollection() => new EDIMessageCollectionNonDependent(Factory, this);

		void AddUXMLMessagesToCollectionIfRequired()
		{
			if (ShouldAddUXMLMessagesToCollection)
			{
				var genPivotPKs = Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.MessageReceivedCode, AutoEvents.MessageRejectedCode })).Select(x => x.PK).ToArray();
				if (genPivotPKs.Length > 0)
				{
					var genPivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.XmlEdiMessage);
					genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, genPivotPKs);
					genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, StmALogSchema.Constants.Prefix);
					var messagePKs = Factory.Load<GenPivot>(genPivotQuery).Select(x => x.XX_Relation2ID).ToArray();
					if (messagePKs.Length > 0)
					{
						var messagingProvider = MessagingProvider;
						if (messagingProvider != null)
						{
							ediMessages.AddRange(Factory.Load(messagingProvider.GetAsycudaEDIMessageType(), new ZQuery(EDIMessageSchema.PK, messagePKs)));
						}
					}
				}
			}
		}

		protected virtual bool ShouldAddUXMLMessagesToCollection => false;

		string IJobNumber.JobNumber
		{
			get { return AMA_MasterBill; }
		}

		#region JobDocAddress auxilliary stuff
		JobDocAddressHelper<AsycudaManifestHeader> jobDocAddressHelper;
		JobDocAddressHelper<AsycudaManifestHeader> JobDocAddressHelper
		{
			get { return jobDocAddressHelper ?? (jobDocAddressHelper = new JobDocAddressHelper<AsycudaManifestHeader>(this)); }
		}

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
			get { return JobDocAddressHelper.DocAddresses; }
		}

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return JobDocAddressHelper.GetDocAddressRequirement(addressType);
		}

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

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new DocAddressType[] { DocAddressType.ReceivingForwarderAddress }; }
		}
		#endregion

		internal bool ShouldDeleteContainersDuringSynch
		{
			get { return false; }
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.ContainerModes))]
		public override ZString AMA_ContainerMode
		{
			get { return base.AMA_ContainerMode; }
			set
			{
				var oldValue = AMA_ContainerMode;
				base.AMA_ContainerMode = value;
				if (!IsCopying && oldValue != AMA_ContainerMode)
				{
					Containers.MarkAsNeedingValidation();
				}
			}
		}

		public ZBool BuyersConsolidationVisible => IsContainerized;

		public ZBool IsContainerized => AMA_ContainerMode == (ZString)Core.Constants.ContainerModes.Containerised;

		public ZString ManifestNumber
		{
			get { return IsAir ? MawbNumberWithHyphen : AMA_MasterBill; }
		}

		public ZString MawbNumberWithHyphen => AsycudaManifestHeaderHelper.GetNumberWithHyphen(AMA_MasterBill);

		public virtual ResourceStringData VoyageFlightNoLabel =>
			IsAir ? Res.GetData("10ad7cd5-22e9-47c5-9195-2790ab1577e9", "Flight") :
			IsSea ? Res.GetData("59170e0a-853f-420b-bdf4-f5f9c9b402a8", "Voyage") :
			Res.GetData("19d47706-95eb-4a82-892d-f99d8513051c", "Flight/Voyage");

		public ResourceStringData MasterBillLabel =>
			IsAir ? Res.GetData("a4b3f2bf-d5ce-464c-9cd0-a58fd88885be", "MAWB") :
			IsSea ? Res.GetData("661cd2c3-c962-4da4-b28f-48367fc2856b", "BOL") :
			Res.GetData("e35148c4-8343-4867-b2ad-72dabae056df", "Manifest No.");

		public void MakeGlbPersonsFromStaffOrContacts(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Length > 0)
			{
				var maybeStaff = selectedBusinessObjects[0] as GlbStaff;
				if (maybeStaff != null)
				{
					MakeGlbPersonsFromStaff(selectedBusinessObjects.OfType<GlbStaff>());
					return;
				}
				var maybeContact = selectedBusinessObjects[0] as OrgContact;
				if (maybeContact != null)
				{
					MakeGlbPersonsFromContact(selectedBusinessObjects.OfType<OrgContact>());
				}
			}
		}

		void MakeGlbPersonsFromStaff(IEnumerable<GlbStaff> staffs)
		{
			foreach (var staff in staffs)
			{
				var wrapper = new PersonalIdentityWrapper(staff);
				var glbPerson = wrapper.TryReallyHardToGetExistingGlbPerson() ?? GlbPerson.CreateFromStaff(Factory, staff);
				var cusPerson = Persons.AddNew();
				cusPerson.CPN_PER_Person = glbPerson.PK;
			}
		}

		void MakeGlbPersonsFromContact(IEnumerable<OrgContact> contacts)
		{
			foreach (var contact in contacts)
			{
				var wrapper = new PersonalIdentityWrapper(contact);
				var glbPerson = wrapper.TryReallyHardToGetExistingGlbPerson() ?? GlbPerson.CreateFromContact(Factory, contact);

				var cusPerson = Persons.AddNew();
				cusPerson.CPN_PER_Person = glbPerson.PK;
			}
		}

		protected virtual ZString GetCustomsLocalCode(ZString portCode, RefUNLOCO port)
		{
			var result = ZString.Empty;
			var manifestCountryPK = Country?.PK ?? ZGuid.Empty;
			if (manifestCountryPK.IsValid && port != null)
			{
				result = GetCustomsLocalCodeList(port).FirstOrDefault()?.RY_LocalPortCode ?? ZString.Empty;
			}
			return result;
		}

		public virtual IEnumerable<RefLocoMap> GetCustomsLocalCodeList(RefUNLOCO port)
		{
			var manifestCountryPK = Country?.PK ?? ZGuid.Empty;
			var result = port.RefLocoMaps
					.Where(x => x.RY_RN == manifestCountryPK && x.RY_SystemUsage == LocoMapSystemUsageList.Codes.CustomsPortCodeList)
					.OrderBy(x => x.RY_LocalPortCode);
			return result;
		}

		public FeatureProvider FeatureProvider
		{
			get
			{
				var (countryOrGrouping, manifestTypeCode, applicationCode) = GetApplicationProviderKey();
				var cachedKey = ZString.Format("{0}_{1}_{2}", countryOrGrouping, manifestTypeCode, applicationCode);
				return Factory.GetCachedValue(cachedKey, () => ApplicationBusinessProvider?.FeatureProvider);
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = "Manifest";

				if (!HumanReadableNamePrefix.IsNullOrEmpty())
				{
					result = string.Format("{0} {1}", HumanReadableNamePrefix, result);
				}

				if (!AMA_JobReference.IsEmpty)
				{
					result += " " + AMA_JobReference;
				}

				return result;
			}
		}

		public string HumanReadableNamePrefix => HumanReadableNamePrefixCore;

		protected virtual string HumanReadableNamePrefixCore => AMA_RN_NKCountry;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (GetCheckBusinessObjectTypeSuspender())
			{
				AMA_RN_NKCountry = GetDefaultCountryCode();
			}
		}

		protected override Type GetArrivalHeaderTypeCore() => typeof(AsycudaArrivalHeader);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		public Type GetPersonType() => GetPersonTypeCore();
		protected virtual Type GetPersonTypeCore() => typeof(CusPerson);

		#region FetchStrategy

		public void AddFetchHints(IDictionary<Type, FetchHintData> fetchDataToAdd)
		{
			FetchHintCreator.Create(fetchDataToAdd);
		}

		FetchHintCreator FetchHintCreator => fetchHintCreator ?? (fetchHintCreator = FetchHintCreator.New(this));
		FetchHintCreator fetchHintCreator;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaManifestHeaderFetchStrategy(this);

		public new AsycudaManifestHeaderFetchStrategy FetchStrategy => (AsycudaManifestHeaderFetchStrategy)base.FetchStrategy;

		#endregion

		public AsycudaManifestHeaderDocWrapper GetDocWrapper() => GetDocWrapperCore();
		protected virtual AsycudaManifestHeaderDocWrapper GetDocWrapperCore() => new AsycudaManifestHeaderDocWrapper(this);

		Integration.Customs.ASYCUDA.IAsycudaBill Integration.Customs.ASYCUDA.IAsycudaManifestHeader.MasterBill => MasterBill;

		public MessageChooser GetNewMessageChooser(IEnumerable<ISelectionItem> items, string messageType, bool showStatus) => GetNewMessageChooserCore(items, messageType, showStatus);

		protected virtual MessageChooser GetNewMessageChooserCore(IEnumerable<ISelectionItem> items, string messageType, bool showStatus) => new MessageChooser(this, items, showStatus);

		#region Sailing Statistics

		#region AMA_NoOfBills

		public ZInt AMA_NoOfBills => Bills.Count;

		public ZPropertyInfo AMA_NoOfBillsInfo => GetZPropertyInfo(Schema.AMA_NoOfBills);

		#endregion

		#region AMA_NoOfSailingBills

		public ZInt AMA_NoOfSailingBills => BillsOfLadings.Count();

		public ZPropertyInfo AMA_NoOfSailingBillsInfo => GetZPropertyInfo(nameof(AMA_NoOfSailingBills));

		#endregion

		#region AMA_NoOfContainers

		public ZInt AMA_NoOfContainers => Containers.Count;

		public ZPropertyInfo AMA_NoOfAMSContainersInfo => GetZPropertyInfo(nameof(AMA_NoOfContainers));

		#endregion

		#region AMA_NoOfSailingContainers

		public ZInt AMA_NoOfSailingContainers
		{
			get
			{
				if (!fAMA_NoOfSailingContainers.HasValue)
				{
					fAMA_NoOfSailingContainers = BillsOfLadings.Where(x => x.IsContainerised).Sum(x => x.RealContainers.Count);
				}

				return fAMA_NoOfSailingContainers.Value;
			}
		}
		ZInt? fAMA_NoOfSailingContainers;

		public ZPropertyInfo AMA_NoOfSailingContainersInfo => GetZPropertyInfo(nameof(AMA_NoOfSailingContainers));

		#endregion

		#endregion

		#region Sailing

		public void ImportBillsOfLadingLinkedToTheSameSailing(BillImportActionCollection importActions)
		{
			var sourceBills = new List<BillOfLading>(BillsOfLadings);

			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				foreach (var importAction in importActions.Cast<BillImportAction>())
				{
					if (importAction.Action == ImportAction.Replace && !importAction.IsSelected)
					{
						sourceBills.Remove(((ISailingSynchronisationTarget<BillOfLading>)importAction.Bill).Source);
					}
					else if (importAction.Action == ImportAction.Delete && importAction.IsSelected)
					{
						Bills.RemoveAndDelete(importAction.Bill);
					}
				}
			}

			var synchronisationTargetCollection = new AsycudaBillSynchronisationTargetCollection(Bills);
			synchronisationTargetCollection.Synchronise(sourceBills, false);

			RemoveOrphanContainers();
			RefreshSailingStatistics();
		}

		void RemoveOrphanContainers()
		{
			var containers = Containers.Cast<AsycudaContainer>().ToArray();
			if (containers.Any())
			{
				var containersInUse = Bills.Cast<AsycudaBill>()
					.SelectMany(b => b.Packs.Cast<AsycudaPack>())
					.Select(p => p.ContainerPK).Distinct().ToList();

				foreach (var container in containers)
				{
					if (!containersInUse.Contains(container.PK))
					{
						Containers.Remove(container);
						container.Delete();
					}
				}
			}
		}

		public bool CanChangeSailing => !Messages.Any();

		public void ChangeSailing(ZGuid sailingPk)
		{
			Sailings.Remove(AMA_ParentId);

			AMA_ParentId = sailingPk;

			if (!sailingPk.IsEmpty)
			{
				AMA_ParentTableCode = JobSailingSchema.Constants.Prefix;

				var sailing = Factory.Load<JobSailing>(sailingPk);
				if (sailing != null)
				{
					Sailings.Add(sailing);

					if (!AMA_OverrideFreightDefaults)
					{
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
					}
				}

				RefreshSailingStatistics();
			}
		}

		public void ClearSailing(bool clearSynchroniserValues)
		{
			Sailings.RemoveAll();

			AMA_ParentTableCode = ZString.Empty;
			AMA_ParentId = ZGuid.Empty;

			if (clearSynchroniserValues)
			{
				AMA_RL_NKPortOfLoading = ZString.Empty;
				AMA_RL_NKPortOfDischarge = ZString.Empty;
				AMA_E_DEP = ZDateTime.Empty;
				AMA_E_ARV = ZDateTime.Empty;

				AMA_OA_Carrier = ZGuid.Empty;
				AMA_OA_Carrier_ZAddress?.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);

				ClearVesselVoyage(true);
			}

			RemoveSynchroniser();
			RefreshSailingStatistics();
		}

		public void RefreshSailingStatistics()
		{
			billsOfLadings = null;
			fAMA_NoOfSailingContainers = null;

			AMA_NoOfSailingBillsInfo.RefreshBinding();
			AMA_NoOfSailingContainersInfo.RefreshBinding();
		}

		public ZBool CanBeLinkedWithSailing => IsSea && IsShippingLine && IsStandAlone;

		public JobSailing Sailing
		{
			get
			{
				JobSailing result = null;
				if (Sailings.Count == 1)
				{
					result = Sailings[0];
				}
				else if (AMA_ParentTableCode == JobSailingSchema.Constants.Prefix)
				{
					result = Factory.Load<JobSailing>(AMA_ParentId);
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

					if (AMA_ParentTableCode == JobSailingSchema.Constants.Prefix && !AMA_ParentId.IsEmpty)
					{
						sailings.AddFromDatabase(AMA_ParentId);
					}
				}
				return sailings;
			}
		}
		JobSailingCollection sailings;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return AsycudaManifestWorkflowDescriptor.Constants.Code; }
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

		[ChildEditable]
		public virtual ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(CreateNewProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;
		protected virtual ProcessTaskCollection CreateNewProcessTaskCollection() => new ProcessTaskCollection<AsycudaManifestHeaderProcessTask, AsycudaManifestHeader>(this);

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, Carrier?.Header?.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, AMA_TransportMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, AMA_ContainerMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, AMA_ManifestType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType4, AMA_RN_NKCountry, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, AMA_RL_NKPortOfLoading, AMA_RL_NKPortOfLoading.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, AMA_RL_NKPortOfDischarge, AMA_RL_NKPortOfDischarge.Substring(0, 2), ZString.Empty);

			return result;
		}
		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AsycudaManifestHeaderDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = CreateNewDocumentSupporter());
		DocumentSupporter documentSupporter;

		protected virtual DocumentSupporter CreateNewDocumentSupporter()
		{
			return new AsycudaManifestHeaderDocumentSupporter(this);
		}

		#endregion

		#region ISailingParentFindBox

		ZString ISailingParentFindBox.LoadPort => AMA_RL_NKPortOfLoading;

		ZString ISailingParentFindBox.DischargePort => AMA_RL_NKPortOfDischarge;

		ZString ISailingParentFindBox.Origin => ZString.Empty;

		ZString ISailingParentFindBox.Destination => ZString.Empty;

		ZGuid ISailingParentFindBox.SailingPK
		{
			get => AMA_ParentTableCode == JobSailingSchema.Constants.Prefix ? AMA_ParentId : ZGuid.Empty;
			set => ChangeSailing(value);
		}

		ZString ISailingParentFindBox.TransportMode => AMA_TransportMode;

		#endregion

		#region ICusCodeDataTypeSupporter

		public IDictionary<ZString, Type> GetCusCodeDataTypes() => SupportedCusCodeDataTypes;

		protected virtual IDictionary<ZString, Type> SupportedCusCodeDataTypes => new Dictionary<ZString, Type>();

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			if (SupportedCusCodeDataTypes.Any())
			{
				yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
			}
		}
		#endregion

		#region IValidateForCustomsMessagingSupporter
		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction) => this;

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging => ApplicationBusinessProvider.SupportsAutoSendGlobalManifest(AMA_ManifestType);
		#endregion

		#region ISendGlobalManifestMessageSupporter
		IProcessor Integration.Customs.ASYCUDA.ISendGlobalManifestMessageSupporter.CreateSendGlobalManifestMessageProcessor()
		{
			return ApplicationBusinessProvider.GetSendGlobalManifestProcessor(this);
		}
		#endregion

		public ZString ManifestApplicationType
		{
			get
			{
				var result = ZString.Empty;
				if (IsShippingLine)
				{
					result = Res.GetString("EE1505F3-F31C-423A-9378-7F33017D30B9", "Carrier");
				}
				else if (IsConsolidator)
				{
					result = Res.GetString("55671CE2-DF48-44F5-8D66-B7BE50CCD644", "Forwarder");
				}
				return result;
			}
		}

		public bool IsTrueAsycudaCountry => RefDataGrouping.GetParentDataGroupingCode(Factory, AMA_RN_NKCountry) == Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;

		[ChildEditable]
		public TransportMeanCollection TransportMeans
		{
			get
			{
				if (transportMeans == null)
				{
					transportMeans = GetNewTransportCollection();
					transportMeans.Load();
					RegisterEditableChildObject(transportMeans);
				}

				return transportMeans;
			}
		}
		TransportMeanCollection transportMeans;

		protected virtual TransportMeanCollection GetNewTransportCollection()
		{
			return new TransportMeanCollection(this);
		}

		public ZBool ShowTransportMeansTab => ShowTransportMeansTabCore;

		protected virtual ZBool ShowTransportMeansTabCore => false;

		#region ITransportParent

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue) { }

		TransportSupporter ITransportParent.TransportSupporter => GetNewTransportSupporter();

		[ChildEditable]
		TransportCollection ITransportParent.Transports => TransportMeans;

		public Directions JobDirection => Directions.Unknown;

		ZString ITransportParentCommon.TypeCode => Constants.ASYCUDAManifestTypesCodes.ASY;

		public RoutingCollection TransportsIncludingRelated => transportsIncludingRelated ??= GetNewTransportsIncludingRelated();
		RoutingCollection transportsIncludingRelated;

		protected virtual RoutingCollection GetNewTransportsIncludingRelated()
		{
			return new RoutingCollection(this);
		}

		protected virtual TransportSupporter GetNewTransportSupporter() => new AsycudaManifestHeaderTransportSupporter(this);

		#endregion

		#region IRoutingSupport

		TransportCollection IRoutingSupport.Transports => TransportMeans;

		RoutingCollection IRoutingSupport.TransportsIncludingRelated => TransportsIncludingRelated;

		ZString IRoutingSupport.TransportMode => AMA_TransportMode;

		string IRoutingSupport.AdditionalETAUpdateMsg => string.Empty;

		string IRoutingSupport.AdditionalETDUpdateMsg => string.Empty;

		#endregion

		#region IEDIMessageCollectionOwner
		BusinessObject IEDIMessageCollectionOwner.MessageOwner => this;

		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => this.Messages;

		#endregion

		public virtual ZBool IsImport => AMA_Nature == ShipmentTypeList.Codes.Import23;

		public virtual ZBool IsExport => AMA_Nature == ShipmentTypeList.Codes.Export22;

		public ZDecimal TotalHouseBillsGrossWeight => Factory.GetValue(ref totalHouseBillsGrossWeightInCached, () =>
		{
			var result = ZDecimal.Zero;
			var masterGrossWightUQ = MasterBill?.ABL_GrossWeightUQ ?? ZString.Empty;

			if (!masterGrossWightUQ.IsEmpty)
			{
				foreach (var bill in Bills)
				{
					var grossWeight = bill.ABL_GrossWeight;
					var grossWeightUQ = bill.ABL_GrossWeightUQ;

					if (!bill.IsChildMasterBill && !grossWeightUQ.IsEmpty && !grossWeight.IsEmpty)
					{
						result += Core.Constants.Weight.ConvertSafe(grossWeight, grossWeightUQ, masterGrossWightUQ);
					}
				}
			}

			return result;
		});
		CachedProperty<ZDecimal> totalHouseBillsGrossWeightInCached;

		public ZInt TotalHouseBillsPackages => Factory.GetValue(ref totalHouseBillsPackagesInCached, () =>
		{
			var result = ZInt.Zero;
			var masterManifestUQ = MasterBill?.ABL_ManifestUQ ?? ZString.Empty;

			if (!masterManifestUQ.IsEmpty)
			{
				foreach (var bill in Bills)
				{
					if (!bill.IsChildMasterBill && !bill.ABL_ManifestQty.IsEmpty && !bill.ABL_ManifestUQ.IsEmpty)
					{
						result += bill.ABL_ManifestQty;
					}
				}
			}

			return result;
		});

		CachedProperty<ZInt> totalHouseBillsPackagesInCached;

		#region IWorkflowTriggerEventSource

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var consol = Consol;
				if (consol != null)
				{
					list.Add(consol);
				}

				return list;
			}
		}

		public IGlbCompany JobHeaderCompany => Branch.Company;

		#endregion
	}
}
