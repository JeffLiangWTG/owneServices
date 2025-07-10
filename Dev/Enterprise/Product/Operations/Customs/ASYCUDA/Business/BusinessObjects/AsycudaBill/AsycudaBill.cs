using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.AsycudaBill)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(AsycudaManifestHeader), "Bills")]
	public partial class AsycudaBill : ManifestBase.AsycudaBill  // NB - singular
		, ISynchroniserReadOnlyMembersProvider
		, IJobNumber
		, IManifestBillForSynchroniser
		, Integration.Customs.ASYCUDA.IAsycudaBill
		, IUniversalXMLNoteParent
		, ISelectionItem
		, ISailingSynchronisationTarget<BillOfLading>
		, ISetterSuspenderSupporter
		, ICusCodeDataTypeSupporter
		, IShortSequenceNumberLine
		, IAsycudaTaxTypeSupporter
		, IAsycudaBillScreeningTypeSupporter
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly AsycudaBillTypeDecider TypeDecider = new AsycudaBillTypeDecider();

		public new partial class Schema : ManifestBase.AsycudaBill.Schema
		{
			public const string ShipperOrgPK = "ShipperOrgPK";
			public const string ConsigneeOrgPK = "ConsigneeOrgPK";
			public const string NotifyPartyOrgPK = "NotifyPartyOrgPK";
			public const string ForwarderOrgPK = "ForwarderOrgPK";
			public const string BuyerOrgPK = "BuyerOrgPK";
			public const string SellerOrgPK = "SellerOrgPK";
			public const string DiscountValue = "DiscountValue";
			public const string DiscountValueCurrency = "DiscountValueCurrency";
			public const string MatchingReference = "MatchingReference";
			public const string OtherChargesValue = "OtherChargesValue";
			public const string OtherChargesValueCurrency = "OtherChargesValueCurrency";
			public const int DiscountValueCurrencyMaxLength = 3;
			public const int OtherChargesValueCurrencyMaxLength = 3;
			public const int MatchingReferenceMaxLength = GenAddOnColumn.Schema.XA_DataMaxLength;
		}

		public CargoStatusIndicator CargoStatusIndicator
		{
			get
			{
				switch (ABL_CargoStatus)
				{
					case CargoStatusList.Codes.LastPartShipment:
						return CargoStatusIndicator.LastPartShipment;
					case CargoStatusList.Codes.PartShipment:
						return CargoStatusIndicator.PartShipment;
					default:
						return CargoStatusIndicator.Fullshipment;
				}
			}
		}

		public bool IsAir => Header?.IsAir ?? false;

		public bool IsSea => Header?.IsSea ?? false;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected sealed override ManifestBase.AsycudaBillValidation GetNewValidation()
		{
			if (IsChildMasterBill)
			{
				return GetNewValidationForMasterChild();
			}
			else
			{
				return GetNewValidationForRegularBill();
			}
		}

		protected virtual ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
		protected virtual ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		public new AsycudaBillValidation Validation => (AsycudaBillValidation)base.Validation;
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		public bool HasManifestBeenSubmittedToCustomsIncludingChildren => Factory.GetValue(
			ref hasManifestBeenSubmittedToCustomsIncludingChildrenCached,
			() => HasManifestBeenSubmittedToCustoms || Packs.Cast<AsycudaPack>().Any(b => b.HasManifestBeenSubmittedToCustomsIncludingChildren));

		CachedProperty<bool> hasManifestBeenSubmittedToCustomsIncludingChildrenCached;

		public new IAsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (IAsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		[ChildEditable]
		public IAsycudaTaxCollection<AsycudaTax, AsycudaBill> AsycudaTaxes
		{
			get
			{
				if (asycudaTaxes == null)
				{
					asycudaTaxes = CreateNewAsycudaTaxCollection();
					asycudaTaxes.Load();
					RegisterEditableChildObject(asycudaTaxes);
				}
				return asycudaTaxes;
			}
		}
		IAsycudaTaxCollection<AsycudaTax, AsycudaBill> asycudaTaxes;

		protected virtual IAsycudaTaxCollection<AsycudaTax, AsycudaBill> CreateNewAsycudaTaxCollection() => new AsycudaTaxCollection<AsycudaTax, AsycudaBill>(this);

		public Type GetAsycudaTaxType() => GetAsycudaTaxTypeCore();
		protected virtual Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);

		[ChildEditable]
		public IAsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill> BillScreenings
		{
			get
			{
				if (billScreenings == null)
				{
					billScreenings = CreateNewAsycudaBillScreeningCollection();
					billScreenings.Load();
					RegisterEditableChildObject(billScreenings);
				}
				return billScreenings;
			}
		}
		IAsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill> billScreenings;

		protected virtual IAsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill> CreateNewAsycudaBillScreeningCollection() => new AsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill>(this);

		public Type GetAsycudaBillScreeningType() => GetAsycudaBillScreeningTypeCore();
		protected virtual Type GetAsycudaBillScreeningTypeCore() => typeof(AsycudaBillScreening);

		protected override void OnFactorySaving()
		{
			if (ApportionmentDirty)
			{
				using (new MarkApportionmentDirtySuspender(this))
				{
					CalculateBillApportionmentCalculator();
				}
			}

			if (!IsChildMasterBill)
			{
				DeleteCustomsNumbersNoLongerApplicable();
			}

			if (!Header?.FeatureProvider?.SupportsAsycudaPacks ?? false)
			{
				Packs.RemoveAndDeleteAll();
			}

			base.OnFactorySaving();
		}

		protected virtual void CalculateBillApportionmentCalculator()
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public ZString ABL_Calc_AMA_TransportMode => Header?.AMA_TransportMode ?? ZString.Empty;

		public const string ChildBolCode = "BOL";
		public bool IsChildMasterBill => ABL_BolType == ChildBolCode;

		#region Properties

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BolTypes))]
		public override ZString ABL_BolType
		{
			get => base.ABL_BolType;
			set
			{
				var oldValue = ABL_BolType;
				base.ABL_BolType = value;
				if (!IsCopying && oldValue != ABL_BolType)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(ABL_BillNumber_ReadOnly))]
		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set
			{
				var oldValue = ABL_BillNumber;
				base.ABL_BillNumber = value;
				if (!IsCopying && oldValue != ABL_BillNumber)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		public virtual bool ABL_BillNumber_ReadOnly => false;

		public ZString BillNumberWithHyphen => AsycudaManifestHeaderHelper.GetNumberWithHyphen(ABL_BillNumber);

		[ResourceStringData("AsycudaBill.ABL_SpecialCargoCode", ShortCaption = "SPC", Caption = "Special Cargo Code")]
		public override ZString ABL_SpecialCargoCode
		{
			get => base.ABL_SpecialCargoCode;
			set => base.ABL_SpecialCargoCode = value;
		}

		public override ZDateTime ABL_E_ARV
		{
			get => base.ABL_E_ARV;
			set
			{
				var oldValue = ABL_E_ARV;
				base.ABL_E_ARV = value;
				if (!IsCopying && oldValue != ABL_E_ARV)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime ABL_E_DEP
		{
			get => base.ABL_E_DEP;
			set
			{
				var oldValue = ABL_E_DEP;
				base.ABL_E_DEP = value;
				if (!IsCopying && oldValue != ABL_E_DEP)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString ABL_RL_NKPortOfDischarge
		{
			get => base.ABL_RL_NKPortOfDischarge;
			set
			{
				var oldValue = ABL_RL_NKPortOfDischarge;
				base.ABL_RL_NKPortOfDischarge = value;
				if (!IsCopying && oldValue != ABL_RL_NKPortOfDischarge)
				{
					if (IsChildMasterBill)
					{
						MarkAsNeedingValidation();
					}
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString ABL_RL_NKPortOfLoading
		{
			get => base.ABL_RL_NKPortOfLoading;
			set
			{
				var oldValue = ABL_RL_NKPortOfLoading;
				base.ABL_RL_NKPortOfLoading = value;
				if (!IsCopying && oldValue != ABL_RL_NKPortOfLoading)
				{
					if (IsChildMasterBill)
					{
						MarkAsNeedingValidation();
					}
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsDischargePortList))]
		public override ZString ABL_CustomsDischargePort
		{
			get => base.ABL_CustomsDischargePort;
			set
			{
				{
					var oldValue = ABL_CustomsDischargePort;
					base.ABL_CustomsDischargePort = value;
					if (!IsCopying && oldValue != ABL_CustomsDischargePort)
					{
						Header?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsLoadingPortList))]
		public override ZString ABL_CustomsLoadPort
		{
			get => base.ABL_CustomsLoadPort; set
			{
				{
					var oldValue = ABL_CustomsLoadPort;
					base.ABL_CustomsLoadPort = value;
					if (!IsCopying && oldValue != ABL_CustomsLoadPort)
					{
						Header?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("26CDE1E8-AB15-42BD-9406-F61AA9B58222", Caption = "Cargo Status", MediumCaption = "Status", ShortCaption = "")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CargoStatusList))]
		public override ZString ABL_CargoStatus
		{
			get => base.ABL_CargoStatus;
			set => base.ABL_CargoStatus = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PackageTypeList))]
		public override ZString ABL_ManifestUQ
		{
			get => base.ABL_ManifestUQ;
			set => base.ABL_ManifestUQ = value;
		}

		public bool IsManifestUQNeedToConvert => IsManifestUQNeedToConvertCore;

		protected virtual bool IsManifestUQNeedToConvertCore => true;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.WeightUQList))]
		public override ZString ABL_GrossWeightUQ
		{
			get => base.ABL_GrossWeightUQ;
			set => base.ABL_GrossWeightUQ = value;
		}

		[MeasureUnit(Schema.ABL_GrossWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal ABL_GrossWeight
		{
			get => base.ABL_GrossWeight;
			set => base.ABL_GrossWeight = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.WeightUQList))]
		public override ZString ABL_NetWeightUQ
		{
			get => base.ABL_NetWeightUQ;
			set => base.ABL_NetWeightUQ = value;
		}

		[MeasureUnit(Schema.ABL_NetWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal ABL_NetWeight
		{
			get => base.ABL_NetWeight;
			set => base.ABL_NetWeight = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.VolumeUQList))]
		public override ZString ABL_VolumeUQ
		{
			get => base.ABL_VolumeUQ;
			set => base.ABL_VolumeUQ = value;
		}

		[MeasureUnit(Schema.ABL_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal ABL_Volume
		{
			get => base.ABL_Volume;
			set => base.ABL_Volume = value;
		}

		#region Discount Value
		[ResourceStringData("AsycudaBill.DiscountValue", Caption = "Discount Value", ShortCaption = "Disc. Val.")]
		public virtual ZDecimal DiscountValue
		{
			get { return this.GetSystemDefinedValue<ZDecimal>(Schema.DiscountValue); }
			set
			{
				var oldValue = DiscountValue;
				this.SetSystemDefinedValue(Schema.DiscountValue, value);
				if (oldValue != value)
				{
					MarkApportionmentDirty();
				}

				if (!IsValidationSuspended)
				{
					(Validation as AsycudaBillValidationForRegularBill)?.ValidateDiscountValue();
				}

				DiscountValueInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DiscountValueInfo => GetZPropertyInfo(Schema.DiscountValue);

		[ResourceStringData("AsycudaBill.DiscountValueCurrency", Caption = "Discount Currency", MediumCaption = "Currency", ShortCaption = "Curr.")]
		[MaxLength(Schema.DiscountValueCurrencyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.DiscountValueCurrencies))]
		public virtual ZString DiscountValueCurrency
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.DiscountValueCurrency); }
			set
			{
				var oldValue = DiscountValueCurrency;
				CheckMaximumLength(DiscountValueCurrencyInfo, value);
				this.SetSystemDefinedValue(Schema.DiscountValueCurrency, value);

				if (oldValue != value)
				{
					MarkApportionmentDirty();
				}

				if (!IsValidationSuspended)
				{
					(Validation as AsycudaBillValidationForRegularBill)?.ValidateDiscountValueCurrency();
				}

				DiscountValueCurrencyInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DiscountValueCurrencyInfo => GetZPropertyInfo(Schema.DiscountValueCurrency);

		public RefCurrency RefDiscountValueCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, DiscountValueCurrency); }
		}
		#endregion

		#region OtherCharges Value
		[ResourceStringData("AsycudaBill.OtherChargesValue", Caption = "Other Charges Value", ShortCaption = "Other Charges")]
		public virtual ZDecimal OtherChargesValue
		{
			get { return this.GetSystemDefinedValue<ZDecimal>(Schema.OtherChargesValue); }
			set
			{
				var oldValue = OtherChargesValue;
				this.SetSystemDefinedValue(Schema.OtherChargesValue, value);

				if (oldValue != value)
				{
					MarkApportionmentDirty();
				}

				if (!IsValidationSuspended)
				{
					(Validation as AsycudaBillValidationForRegularBill)?.ValidateOtherChargesValue();
				}

				OtherChargesValueInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo OtherChargesValueInfo
		{
			get { return GetZPropertyInfo(Schema.OtherChargesValue); }
		}

		[MaxLength(Schema.OtherChargesValueCurrencyMaxLength)]
		[ResourceStringData("AsycudaBill.OtherChargesValueCurrency", Caption = "Other Charges Currency", MediumCaption = "Currency", ShortCaption = "Curr.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.OtherChargesValueCurrencies))]
		public virtual ZString OtherChargesValueCurrency
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.OtherChargesValueCurrency); }
			set
			{
				var oldValue = OtherChargesValueCurrency;
				CheckMaximumLength(OtherChargesValueCurrencyInfo, value);
				this.SetSystemDefinedValue(Schema.OtherChargesValueCurrency, value);

				if (oldValue != value)
				{
					MarkApportionmentDirty();
				}

				if (!IsValidationSuspended)
				{
					(Validation as AsycudaBillValidationForRegularBill)?.ValidateOtherChargesValueCurrency();
				}

				OtherChargesValueCurrencyInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo OtherChargesValueCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.OtherChargesValueCurrency); }
		}

		public RefCurrency RefOtherChargesValueCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, OtherChargesValueCurrency); }
		}
		#endregion

		public override ZString ABL_RL_NKFinalDestination
		{
			get => base.ABL_RL_NKFinalDestination;
			set
			{
				var hasChanged = ABL_RL_NKFinalDestination != value;
				if (hasChanged)
				{
					base.ABL_RL_NKFinalDestination = value;
					ReCalculateShipmentType();
				}
			}
		}

		void ReCalculateShipmentType()
		{
			CalculateShipmentType(this);
		}

		public override ZString ABL_RL_NKOrigin
		{
			get => base.ABL_RL_NKOrigin;
			set
			{
				var hasChanged = ABL_RL_NKOrigin != value;
				if (hasChanged)
				{
					base.ABL_RL_NKOrigin = value;
					ReCalculateShipmentType();
				}
			}
		}

		[ResourceStringData("eeb784c6-cb1c-4049-83a2-2964c9001a0e", Caption = "Cust. Val", MediumCaption = "Customs Value")]
		[ReadOnlyMember(nameof(ABL_CustomsValue_ReadOnly))]
		public override ZDecimal ABL_CustomsValue
		{
			get => base.ABL_CustomsValue;
			set => base.ABL_CustomsValue = value;
		}

		protected virtual bool ABL_CustomsValue_ReadOnly => false;

		[ResourceStringData("AsycudaBill.ABL_RX_NKCustomsValueCurrency", Caption = "Customs Value Currency", MediumCaption = "Currency", ShortCaption = "Curr.")]
		[ReadOnlyMember(nameof(ABL_RX_NKCustomsValueCurrency_ReadOnly))]
		public override ZString ABL_RX_NKCustomsValueCurrency
		{
			get => base.ABL_RX_NKCustomsValueCurrency;
			set => base.ABL_RX_NKCustomsValueCurrency = value;
		}

		protected virtual bool ABL_RX_NKCustomsValueCurrency_ReadOnly => false;

		public override ZDecimal ABL_TransportValue
		{
			get => base.ABL_TransportValue;
			set
			{
				var oldValue = ABL_TransportValue;
				base.ABL_TransportValue = value;
				if (!IsCopying && oldValue != ABL_TransportValue)
				{
					MarkApportionmentDirty();
				}
			}
		}

		public override ZString ABL_RX_NKTransportValueCurrency
		{
			get => base.ABL_RX_NKTransportValueCurrency;
			set
			{
				var oldValue = ABL_RX_NKTransportValueCurrency;
				base.ABL_RX_NKTransportValueCurrency = value;
				if (!IsCopying && oldValue != ABL_RX_NKTransportValueCurrency)
				{
					MarkApportionmentDirty();
				}
			}
		}

		public override ZDecimal ABL_InsuranceValue
		{
			get => base.ABL_InsuranceValue;
			set
			{
				var oldValue = base.ABL_InsuranceValue;

				if (oldValue != value)
				{
					base.ABL_InsuranceValue = value;
					MarkApportionmentDirty();
				}
			}
		}

		public override ZString ABL_RX_NKInsuranceValueCurrency
		{
			get => base.ABL_RX_NKInsuranceValueCurrency;
			set
			{
				var oldValue = base.ABL_RX_NKInsuranceValueCurrency;

				if (oldValue != value)
				{
					base.ABL_RX_NKInsuranceValueCurrency = value;
					MarkApportionmentDirty();
				}
			}
		}

		#region ApportionmentDirty

		public ZBool ApportionmentDirty
		{
			get => apportionmentDirty;
			set
			{
				var oldValue = ApportionmentDirty;
				apportionmentDirty = value && !IsCopying;

				if (ApportionmentDirty != oldValue)
				{
					ApportionmentDirtyChanged?.Invoke(this, EventArgs.Empty);
					HasChanges = true;
				}
			}
		}
		ZBool apportionmentDirty;

		public void MarkApportionmentDirty()
		{
			if (!IsDeleted && !IsMarkApportionmentDirtySuspended)
			{
				ApportionmentDirty = true;
			}
		}

		public event EventHandler ApportionmentDirtyChanged;

		bool IsMarkApportionmentDirtySuspended => markApportionmentDirtySuspenderIndex > 0 || (Header?.IsMarkApportionmentDirtySuspended ?? false);

		int markApportionmentDirtySuspenderIndex;

		class MarkApportionmentDirtySuspender : IDisposable
		{
			public MarkApportionmentDirtySuspender(AsycudaBill bill)
			{
				this.bill = bill;
				bill.markApportionmentDirtySuspenderIndex++;
			}

			readonly AsycudaBill bill;

			public void Dispose()
			{
				bill.markApportionmentDirtySuspenderIndex--;
			}
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PrepaidCollectList))]
		public override ZString ABL_PrepaidCollect
		{
			get => base.ABL_PrepaidCollect;
			set => base.ABL_PrepaidCollect = value;
		}

		public ZDecimal VolumeInM3 => Core.Constants.Volume.ContainsCode(ABL_VolumeUQ.ToUpper()) ? Core.Constants.Volume.Convert(ABL_Volume, ABL_VolumeUQ.ToUpper(), Core.Constants.Volume.CubicMetres) : 0m;

		public ZDecimal MassInKilos => Core.Constants.Weight.ContainsCode(ABL_GrossWeightUQ.ToUpper()) ? Core.Constants.Weight.Convert(ABL_GrossWeight, ABL_GrossWeightUQ.ToUpper(), Core.Constants.Weight.Kilograms) : 0m;

		[ResourceStringData("AsycudaBill.ABL_UCRNumber", Caption = "UCR Number")]
		public override ZString ABL_UCRNumber
		{
			get => base.ABL_UCRNumber;
			set => base.ABL_UCRNumber = value;
		}

		[MaxLength(Schema.MatchingReferenceMaxLength)]
		public ZString MatchingReference
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.MatchingReference); }
			set
			{
				var oldValue = MatchingReference;
				CheckMaximumLength(MatchingReferenceInfo, value);
				this.SetSystemDefinedValue(Schema.MatchingReference, value);
				MatchingReferenceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo MatchingReferenceInfo => GetZPropertyInfo(Schema.MatchingReference);

		[ResourceStringData("AsycudaBill.ABL_Incoterm", Caption = "Incoterm")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.IncotermList))]
		[MaxLength(Schema.ABL_IncotermMaxLength)]
		public override ZString ABL_Incoterm
		{
			get => base.ABL_Incoterm;
			set => base.ABL_Incoterm = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Procedures))]
		public override ZString ABL_Procedure
		{
			get => base.ABL_Procedure;
			set => base.ABL_Procedure = value;
		}

		[ResourceStringData("AsycudaBill.ABL_OA_ContainerAgent", Caption = "Agent")]
		public override ZGuid ABL_OA_ContainerAgent
		{
			get => base.ABL_OA_ContainerAgent;
			set => base.ABL_OA_ContainerAgent = value;
		}

		#endregion

		#region Shipper

		public ZBool CanConvertShipperToOrganization
		{
			get => ABL_OA_Shipper.IsEmpty && !ShipperABLAddress.AreEmpty();
		}

		#region ShipperOrgPK

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Consignors))]
		[ResourceStringData("AsycudaBill.ShipperOrgPK", Caption = "Shipper")]
		public virtual ZGuid ShipperOrgPK { get => ABL_OA_Shipper_ZAddress.OrgPK; set => ABL_OA_Shipper_ZAddress.OrgPK = value; }

		public ZPropertyInfo ShipperOrgPKInfo
		{
			get => GetWrappedZPropertyInfo(Schema.ShipperOrgPK, x => ABL_OA_Shipper_ZAddress.OrgPKInfo);
		}

		#endregion

		protected override ZAddress GetNewABL_OA_Shipper_ZAddress()
		{
			var result = base.GetNewABL_OA_Shipper_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.PIC;
			return result;
		}

		[ResourceStringData("AsycudaBill.ABL_OA_Shipper", Caption = "Shipper")]
		[List(nameof(ABL_OA_Shipper_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid ABL_OA_Shipper
		{
			get => base.ABL_OA_Shipper;
			set
			{
				var oldValue = ABL_OA_Shipper;
				base.ABL_OA_Shipper = value;
				if (!IsCopying && oldValue != ABL_OA_Shipper)
				{
					ClearValueIfNeeded(!ABL_OA_Shipper.IsEmpty || ClearShipperValueWhenEmpty, ABL_ShipperNameInfo, ABL_ShipperStreet1Info, ABL_ShipperStreet2Info, ABL_ShipperCityInfo, ABL_ShipperStateInfo, ABL_ShipperPostcodeInfo, ABL_RN_NKShipperCountryInfo, ABL_ShipperRegNoInfo, ABL_ShipperRegNoTypeInfo, ABL_ShipperPhoneInfo);
					DefaultPartyOrgAddressDetails(Shipper, AsycudaBillAddress.AddressType.Shipper);
				}
			}
		}
		public bool ShipperUseRealOrg => !ABL_OA_Shipper.IsEmpty;

		[ResourceStringData("AsycudaBill.ABL_ShipperName", Caption = "Shipper Name")]
		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperName { get => base.ABL_ShipperName; set => base.ABL_ShipperName = value; }

		[ResourceStringData("AsycudaBill.ABL_ShipperStreet1", Caption = "Shipper Street 1")]
		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperStreet1 { get => base.ABL_ShipperStreet1; set => base.ABL_ShipperStreet1 = value; }

		[ResourceStringData("AsycudaBill.ABL_ShipperStreet2", Caption = "Shipper Street 2")]
		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperStreet2 { get => base.ABL_ShipperStreet2; set => base.ABL_ShipperStreet2 = value; }

		[ResourceStringData("AsycudaBill.ABL_ShipperCity", Caption = "Shipper City")]
		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperCity { get => base.ABL_ShipperCity; set => base.ABL_ShipperCity = value; }

		[ResourceStringData("AsycudaBill.ABL_ShipperState", Caption = "Shipper State")]
		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ShipperState_List))]
		public override ZString ABL_ShipperState { get => base.ABL_ShipperState; set => base.ABL_ShipperState = value; }

		[ResourceStringData("AsycudaBill.ABL_ShipperPostcode", Caption = "Shipper Postcode")]
		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperPostcode { get => base.ABL_ShipperPostcode; set => base.ABL_ShipperPostcode = value; }

		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperPhone { get => base.ABL_ShipperPhone; set => base.ABL_ShipperPhone = value; }

		[ResourceStringData("AsycudaBill.ABL_RN_NKShipperCountry", Caption = "Shipper Country")]
		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_RN_NKShipperCountry { get => base.ABL_RN_NKShipperCountry; set => base.ABL_RN_NKShipperCountry = value; }

		[ReadOnlyMember(nameof(ShipperRegNoReadOnly))]
		public override ZString ABL_ShipperRegNo { get => base.ABL_ShipperRegNo; set => base.ABL_ShipperRegNo = value; }

		[ReadOnlyMember(nameof(ShipperRegNoReadOnly))]
		[MaxLength(Schema.ABL_ShipperRegNoTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ShipperRegistrationNoTypeList))]
		public override ZString ABL_ShipperRegNoType { get => base.ABL_ShipperRegNoType; set => base.ABL_ShipperRegNoType = value; }

		public virtual ZBool ShipperRegNoReadOnly => ShipperUseRealOrg;

		public virtual ZString[] ShipperRegNoTypes()
		{
			return Array.Empty<ZString>();
		}

		protected virtual ZBool ClearShipperValueWhenEmpty => false;

		#endregion

		#region Consignee

		public ZBool CanConvertConsigneeToOrganization
		{
			get { return ABL_OA_Consignee.IsEmpty && !ConsigneeABLAddress.AreEmpty(); }
		}

		#region ConsigneeOrgPK

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Consignees))]
		[ResourceStringData("AsycudaBill.ConsigneeOrgPK", Caption = "Consignee")]
		public virtual ZGuid ConsigneeOrgPK { get => ABL_OA_Consignee_ZAddress.OrgPK; set => ABL_OA_Consignee_ZAddress.OrgPK = value; }

		public ZPropertyInfo ConsigneeOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeOrgPK, x => ABL_OA_Consignee_ZAddress.OrgPKInfo); }
		}

		#endregion

		protected override ZAddress GetNewABL_OA_Consignee_ZAddress()
		{
			var result = base.GetNewABL_OA_Consignee_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.DLV;
			return result;
		}

		[List(nameof(ABL_OA_Consignee_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid ABL_OA_Consignee
		{
			get => base.ABL_OA_Consignee;
			set
			{
				var oldValue = ABL_OA_Consignee;
				base.ABL_OA_Consignee = value;
				if (!IsCopying && oldValue != ABL_OA_Consignee)
				{
					ClearValueIfNeeded(!ABL_OA_Consignee.IsEmpty || ClearConsigneeValueWhenEmpty, ABL_ConsigneeNameInfo, ABL_ConsigneeStreet1Info, ABL_ConsigneeStreet2Info, ABL_ConsigneeCityInfo, ABL_ConsigneeStateInfo, ABL_ConsigneePostcodeInfo, ABL_RN_NKConsigneeCountryInfo, ABL_ConsigneeRegNoInfo, ABL_ConsigneeRegNoTypeInfo, ABL_ConsigneePhoneInfo);
					DefaultPartyOrgAddressDetails(Consignee, AsycudaBillAddress.AddressType.Consignee);
				}
			}
		}

		public bool ConsigneeUseRealOrg => !ABL_OA_Consignee.IsEmpty;

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeName { get => base.ABL_ConsigneeName; set => base.ABL_ConsigneeName = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeStreet1 { get => base.ABL_ConsigneeStreet1; set => base.ABL_ConsigneeStreet1 = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeStreet2 { get => base.ABL_ConsigneeStreet2; set => base.ABL_ConsigneeStreet2 = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeCity { get => base.ABL_ConsigneeCity; set => base.ABL_ConsigneeCity = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ConsigneeState_List))]
		public override ZString ABL_ConsigneeState { get => base.ABL_ConsigneeState; set => base.ABL_ConsigneeState = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneePostcode { get => base.ABL_ConsigneePostcode; set => base.ABL_ConsigneePostcode = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneePhone { get => base.ABL_ConsigneePhone; set => base.ABL_ConsigneePhone = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_RN_NKConsigneeCountry { get => base.ABL_RN_NKConsigneeCountry; set => base.ABL_RN_NKConsigneeCountry = value; }

		[ReadOnlyMember(nameof(ConsigneeRegNoReadOnly))]
		public override ZString ABL_ConsigneeRegNo { get => base.ABL_ConsigneeRegNo; set => base.ABL_ConsigneeRegNo = value; }

		[ReadOnlyMember(nameof(ConsigneeRegNoReadOnly))]
		[MaxLength(Schema.ABL_ConsigneeRegNoTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ConsigneeRegistrationNoTypeList))]
		public override ZString ABL_ConsigneeRegNoType { get => base.ABL_ConsigneeRegNoType; set => base.ABL_ConsigneeRegNoType = value; }

		public virtual ZBool ConsigneeRegNoReadOnly => ConsigneeUseRealOrg;

		public virtual ZString[] ConsigneeRegNoTypes()
		{
			return Array.Empty<ZString>();
		}

		protected virtual ZBool ClearConsigneeValueWhenEmpty => false;

		#endregion

		#region Notify Party

		public ZBool CanConvertNotifyPartyToOrganization
		{
			get { return ABL_OA_NotifyParty.IsEmpty && !NotifyPartyABLAddress.AreEmpty(); }
		}

		#region NotifyPartyOrgPK

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Organisations))]
		[ResourceStringData("AsycudaBill.NotifyPartyOrgPK", Caption = "Notify Party")]
		public virtual ZGuid NotifyPartyOrgPK { get => ABL_OA_NotifyParty_ZAddress.OrgPK; set => ABL_OA_NotifyParty_ZAddress.OrgPK = value; }

		public ZPropertyInfo NotifyPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NotifyPartyOrgPK, x => ABL_OA_NotifyParty_ZAddress.OrgPKInfo); }
		}

		#endregion

		protected override ZAddress GetNewABL_OA_NotifyParty_ZAddress()
		{
			var result = base.GetNewABL_OA_NotifyParty_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.OFC;
			return result;
		}

		[List(nameof(ABL_OA_NotifyParty_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid ABL_OA_NotifyParty
		{
			get => base.ABL_OA_NotifyParty;
			set
			{
				var oldValue = ABL_OA_NotifyParty;
				base.ABL_OA_NotifyParty = value;
				if (!IsCopying && oldValue != ABL_OA_NotifyParty)
				{
					ClearValueIfNeeded(!ABL_OA_NotifyParty.IsEmpty || ClearNotifyPartyValueWhenEmpty, ABL_NotifyPartyNameInfo, ABL_NotifyPartyStreet1Info, ABL_NotifyPartyStreet2Info, ABL_NotifyPartyCityInfo, ABL_NotifyPartyStateInfo, ABL_NotifyPartyPostcodeInfo, ABL_RN_NKNotifyPartyCountryInfo, ABL_NotifyPartyRegNoInfo, ABL_NotifyPartyRegNoTypeInfo, ABL_NotifyPartyPhoneInfo);
					DefaultPartyOrgAddressDetails(NotifyParty, AsycudaBillAddress.AddressType.NotifyParty);
				}
			}
		}

		public bool NotifyPartyUseRealOrg => !ABL_OA_NotifyParty.IsEmpty;

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyName { get => base.ABL_NotifyPartyName; set => base.ABL_NotifyPartyName = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyStreet1 { get => base.ABL_NotifyPartyStreet1; set => base.ABL_NotifyPartyStreet1 = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyStreet2 { get => base.ABL_NotifyPartyStreet2; set => base.ABL_NotifyPartyStreet2 = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyCity { get => base.ABL_NotifyPartyCity; set => base.ABL_NotifyPartyCity = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.NotifyPartyState_List))]
		public override ZString ABL_NotifyPartyState { get => base.ABL_NotifyPartyState; set => base.ABL_NotifyPartyState = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyPostcode { get => base.ABL_NotifyPartyPostcode; set => base.ABL_NotifyPartyPostcode = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyPhone { get => base.ABL_NotifyPartyPhone; set => base.ABL_NotifyPartyPhone = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_RN_NKNotifyPartyCountry { get => base.ABL_RN_NKNotifyPartyCountry; set => base.ABL_RN_NKNotifyPartyCountry = value; }

		[ReadOnlyMember(nameof(NotifyPartyRegNoReadOnly))]
		public override ZString ABL_NotifyPartyRegNo { get => base.ABL_NotifyPartyRegNo; set => base.ABL_NotifyPartyRegNo = value; }

		[ReadOnlyMember(nameof(NotifyPartyRegNoReadOnly))]
		[MaxLength(Schema.ABL_NotifyPartyRegNoTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ConsigneeRegistrationNoTypeList))]
		public override ZString ABL_NotifyPartyRegNoType { get => base.ABL_NotifyPartyRegNoType; set => base.ABL_NotifyPartyRegNoType = value; }

		public virtual ZBool NotifyPartyRegNoReadOnly => NotifyPartyUseRealOrg;

		public virtual ZString[] NotifyPartyRegNoTypes()
		{
			return Array.Empty<ZString>();
		}

		protected virtual ZBool ClearNotifyPartyValueWhenEmpty => false;

		#endregion

		#region Buyer

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Buyers))]
		[ResourceStringData("AsycudaBill.BuyerOrgPK", Caption = "Buyer")]
		public ZGuid BuyerOrgPK { get => ABL_OA_Buyer_ZAddress.OrgPK; set => ABL_OA_Buyer_ZAddress.OrgPK = value; }

		public ZPropertyInfo BuyerOrgPKInfo
		{
			get => GetWrappedZPropertyInfo(Schema.BuyerOrgPK, x => ABL_OA_Buyer_ZAddress.OrgPKInfo);
		}

		public ZBool CanConvertBuyerToOrganization => ABL_OA_Buyer.IsEmpty && !BuyerABLAddress.AreEmpty();

		public AsycudaBillAddress BuyerABLAddress => buyer ??= GetBuyerAddress();
		AsycudaBillAddress buyer;

		AsycudaBillAddress GetBuyerAddress()
		{
			return new AsycudaBillAddress(
				AsycudaBillAddress.AddressType.Buyer,
				ABL_OA_BuyerInfo,
				ABL_BuyerNameInfo,
				ABL_BuyerStreet1Info,
				ABL_BuyerStreet2Info,
				ABL_BuyerCityInfo,
				ABL_BuyerStateInfo,
				ABL_BuyerPostcodeInfo,
				ABL_RN_NKBuyerCountryInfo,
				ABL_BuyerPhoneInfo);
		}

		protected override ZAddress GetNewABL_OA_Buyer_ZAddress()
		{
			var result = base.GetNewABL_OA_Buyer_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.DLV;
			return result;
		}

		[List(nameof(ABL_OA_Buyer_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid ABL_OA_Buyer
		{
			get => base.ABL_OA_Buyer;
			set
			{
				var oldValue = ABL_OA_Buyer;
				base.ABL_OA_Buyer = value;
				if (!IsCopying && oldValue != ABL_OA_Buyer)
				{
					ClearValueIfNeeded(!ABL_OA_Buyer.IsEmpty, ABL_BuyerNameInfo, ABL_BuyerStreet1Info, ABL_BuyerStreet2Info, ABL_BuyerCityInfo, ABL_BuyerStateInfo, ABL_BuyerPostcodeInfo, ABL_RN_NKBuyerCountryInfo, ABL_BuyerRegNoInfo, ABL_BuyerRegNoTypeInfo, ABL_BuyerPhoneInfo);
					DefaultPartyOrgAddressDetails(Buyer, AsycudaBillAddress.AddressType.Buyer);
				}
			}
		}

		public bool BuyerUseRealOrg => !ABL_OA_Buyer.IsEmpty;

		public bool BuyerFieldsReadOnly => BuyerFieldsDisabledCore || BuyerUseRealOrg;

		protected virtual bool BuyerFieldsDisabledCore => false;

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerName { get => base.ABL_BuyerName; set => base.ABL_BuyerName = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerStreet1 { get => base.ABL_BuyerStreet1; set => base.ABL_BuyerStreet1 = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerStreet2 { get => base.ABL_BuyerStreet2; set => base.ABL_BuyerStreet2 = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerCity { get => base.ABL_BuyerCity; set => base.ABL_BuyerCity = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BuyerState_List))]
		public override ZString ABL_BuyerState { get => base.ABL_BuyerState; set => base.ABL_BuyerState = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerPostcode { get => base.ABL_BuyerPostcode; set => base.ABL_BuyerPostcode = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerPhone { get => base.ABL_BuyerPhone; set => base.ABL_BuyerPhone = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_RN_NKBuyerCountry { get => base.ABL_RN_NKBuyerCountry; set => base.ABL_RN_NKBuyerCountry = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerRegNo { get => base.ABL_BuyerRegNo; set => base.ABL_BuyerRegNo = value; }

		[ReadOnlyMember(nameof(BuyerFieldsReadOnly))]
		public override ZString ABL_BuyerRegNoType { get => base.ABL_BuyerRegNoType; set => base.ABL_BuyerRegNoType = value; }

		public virtual ZString[] BuyerRegNoTypes() => Array.Empty<ZString>();

		#endregion

		#region Seller

		public virtual AsycudaBillAddress SellerABLAddress
		{
			get { return seller ?? (seller = new AsycudaBillAddress(AsycudaBillAddress.AddressType.Seller, ABL_OA_SellerInfo, ABL_SellerNameInfo, ABL_SellerStreet1Info, ABL_SellerStreet2Info, ABL_SellerCityInfo, ABL_SellerStateInfo, ABL_SellerPostcodeInfo, ABL_RN_NKSellerCountryInfo, ABL_SellerPhoneInfo)); }
		}

		AsycudaBillAddress seller;

		public virtual ZBool CanConvertSellerToOrganization
		{
			get { return ABL_OA_Seller.IsEmpty && !SellerABLAddress.AreEmpty(); }
		}

		#region SellerOrgPK

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SellersOrgList))]
		[ResourceStringData("AsycudaBill.SellerOrgPK", Caption = "Seller")]
		public virtual ZGuid SellerOrgPK { get => ABL_OA_Seller_ZAddress.OrgPK; set => ABL_OA_Seller_ZAddress.OrgPK = value; }

		public virtual ZPropertyInfo SellerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SellerOrgPK, x => ABL_OA_Seller_ZAddress.OrgPKInfo); }
		}

		#endregion

		protected override ZAddress GetNewABL_OA_Seller_ZAddress()
		{
			var result = base.GetNewABL_OA_Seller_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.DLV;
			return result;
		}

		[ResourceStringData("AsycudaBill.ABL_OA_Seller", Caption = "Seller")]
		public override ZGuid ABL_OA_Seller
		{
			get => base.ABL_OA_Seller;
			set
			{
				var oldValue = ABL_OA_Seller;
				base.ABL_OA_Seller = value;
				if (!IsCopying && oldValue != ABL_OA_Seller)
				{
					ClearValueIfNeeded(!ABL_OA_Seller.IsEmpty, ABL_SellerNameInfo, ABL_SellerStreet1Info, ABL_SellerStreet2Info, ABL_SellerCityInfo, ABL_SellerStateInfo, ABL_SellerPostcodeInfo, ABL_RN_NKSellerCountryInfo, ABL_SellerRegNoInfo, ABL_SellerRegNoTypeInfo, ABL_SellerPhoneInfo);
					DefaultPartyOrgAddressDetails(Seller, AsycudaBillAddress.AddressType.Seller);
				}
			}
		}

		public virtual bool SellerUseRealOrg => !ABL_OA_Seller.IsEmpty;

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_SellerName", Caption = "Seller Name")]
		public override ZString ABL_SellerName { get => base.ABL_SellerName; set => base.ABL_SellerName = value; }

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_SellerStreet1", Caption = "Seller Street 1")]
		public override ZString ABL_SellerStreet1 { get => base.ABL_SellerStreet1; set => base.ABL_SellerStreet1 = value; }

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_SellerStreet2", Caption = "Seller Street 2")]
		public override ZString ABL_SellerStreet2 { get => base.ABL_SellerStreet2; set => base.ABL_SellerStreet2 = value; }

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_SellerCity", Caption = "Seller City")]
		public override ZString ABL_SellerCity { get => base.ABL_SellerCity; set => base.ABL_SellerCity = value; }

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_SellerState", Caption = "Seller State")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SellerState_List))]
		public override ZString ABL_SellerState { get => base.ABL_SellerState; set => base.ABL_SellerState = value; }

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_SellerPostcode", Caption = "Seller Postcode")]
		public override ZString ABL_SellerPostcode { get => base.ABL_SellerPostcode; set => base.ABL_SellerPostcode = value; }

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_SellerPhone", Caption = "Seller Phone")]
		public override ZString ABL_SellerPhone { get => base.ABL_SellerPhone; set => base.ABL_SellerPhone = value; }

		[ReadOnlyMember(nameof(SellerUseRealOrg))]
		[ResourceStringData("AsycudaBill.ABL_RN_NKSellerCountry", Caption = "Seller Country")]
		public override ZString ABL_RN_NKSellerCountry { get => base.ABL_RN_NKSellerCountry; set => base.ABL_RN_NKSellerCountry = value; }

		[ReadOnlyMember(nameof(SellerRegNoReadOnly))]
		[ResourceStringData("AsycudaBill.ABL_SellerRegNo", Caption = "Seller Reg No")]
		public override ZString ABL_SellerRegNo { get => base.ABL_SellerRegNo; set => base.ABL_SellerRegNo = value; }

		[ReadOnlyMember(nameof(SellerRegNoReadOnly))]
		[MaxLength(Schema.ABL_SellerRegNoTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SellerRegistrationNoTypeList))]
		[ResourceStringData("AsycudaBill.ABL_SellerRegNoType", Caption = "Seller Reg No Type")]
		public override ZString ABL_SellerRegNoType { get => base.ABL_SellerRegNoType; set => base.ABL_SellerRegNoType = value; }

		public virtual ZBool SellerRegNoReadOnly => SellerUseRealOrg;

		public virtual ZString[] SellerRegNoTypes() => Array.Empty<ZString>();

		#endregion

		#region Freight Forwarder

		public AsycudaBillAddress ForwarderABLAddress
		{
			get { return forwarder ?? (forwarder = new AsycudaBillAddress(AsycudaBillAddress.AddressType.FreightForwarder, ABL_OA_ForwarderInfo)); }
		}
		AsycudaBillAddress forwarder;

		#region ForwarderOrgPK

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Consignees))]
		[ResourceStringData("AsycudaBill.ForwarderOrgPK", Caption = "Freight Forwarder")]
		public ZGuid ForwarderOrgPK { get => ABL_OA_Forwarder_ZAddress.OrgPK; set => ABL_OA_Forwarder_ZAddress.OrgPK = value; }

		public ZPropertyInfo ForwarderOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ForwarderOrgPK, x => ABL_OA_Forwarder_ZAddress.OrgPKInfo); }
		}

		#endregion
		#endregion

		#region ReadOnly

		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		public bool ShouldBeReadOnly => BillShouldBeLocked;

		bool BillShouldBeLocked => Factory.GetValue(ref billShouldBeLocked, GetBillShouldBeLocked);

		CachedProperty<bool> billShouldBeLocked;

		protected virtual bool GetBillShouldBeLocked() => false;

		#endregion

		public void AmendBillDetails()
		{
			if (IsOnePackedItemRelationship)
			{
				foreach (AsycudaPack pack in Packs)
				{
					var packedItem = pack.GetPackedItemFromCollection();
					if (packedItem != null && packedItem.HasManifestBeenSubmittedToCustoms && packedItem.API_MessageStatus != MessageStatusCodeList.Codes.Updated)
					{
						packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Updated;
					}
				}
			}
			Header?.RefreshBillLock(this);
		}

		[ResourceStringData("AsycudaBill.ABL_AMA", Caption = "Manifest")]
		public override ZGuid ABL_AMA
		{
			get => base.ABL_AMA;
			set
			{
				var oldValue = ABL_AMA;
				base.ABL_AMA = value;
				if (!IsCopying && oldValue != ABL_AMA)
				{
					if (!IsChildMasterBill)
					{
						CalculateShipmentType(this);
						CustomsEntryNumbers.MarkAsNeedingValidation();
						if (!ABL_AMA.IsValid)
						{
							DetachedLine(oldValue);
						}
						else
						{
							AttachedLine();
						}
					}
					Header?.MarkAsNeedingValidation();
				}
			}
		}
		string IJobNumber.JobNumber => ABL_BillNumber;

		void DetachedLine(ZGuid oldValue)
		{
			var header = Factory.Load<AsycudaManifestHeader>(oldValue);
			if (header != null)
			{
				header.Bills.SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedLine()
		{
			var header = Header;
			if (header != null)
			{
				header.Bills.SequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		public IEnumerable<AsycudaContainer> ContainersOnThisBill
		{
			get
			{
				foreach (AsycudaPack p in Packs)
				{
					if (p.Container != null)
					{
						yield return p.Container;
					}
				}
			}
		}

		public ForwardingShipment Shipment => Factory.Load<ForwardingShipment>(ABL_JS_Shipment);

		string ISelectionItem.SelectionDescription(bool showStatus)
		{
			var function = "";
			var additionalDesc = ZString.Empty;

			if (showStatus)
			{
				function = MessageStatusProvider?.AllowModificationMessage(this) ?? false ? " - Amendment" : " - Original";
				additionalDesc = GetAdditionalSelectionDescription(this);
			}

			return Invariant($"Bill Number - {ABL_BillNumber}{additionalDesc}{function}");
		}

		protected virtual ZString GetAdditionalSelectionDescription(AsycudaBill country)
		{
			return ZString.Empty;
		}

		[List(nameof(ABL_OA_GoodsLocation_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid ABL_OA_GoodsLocation { get => base.ABL_OA_GoodsLocation; set => base.ABL_OA_GoodsLocation = value; }

		#region Implementation

		protected void ClearValueIfNeeded(bool shouldClear, ZPropertyInfo partyNameInfo, ZPropertyInfo partyStreet1Info, ZPropertyInfo partyStreet2Info, ZPropertyInfo partyCityInfo, ZPropertyInfo partyStateInfo, ZPropertyInfo partyPostcodeInfo, ZPropertyInfo partyCountryInfo, ZPropertyInfo regNoInfo, ZPropertyInfo regNoTypeInfo, ZPropertyInfo partyPhoneInfo = null)
		{
			if (shouldClear)
			{
				ClearValueIfNeeded(partyNameInfo);
				ClearValueIfNeeded(partyStreet1Info);
				ClearValueIfNeeded(partyStreet2Info);
				ClearValueIfNeeded(partyCityInfo);
				ClearValueIfNeeded(partyStateInfo);
				ClearValueIfNeeded(partyPostcodeInfo);
				ClearValueIfNeeded(partyCountryInfo);
				ClearValueIfNeeded(regNoInfo);
				ClearValueIfNeeded(regNoTypeInfo);
				if (partyPhoneInfo != null)
				{
					ClearValueIfNeeded(partyPhoneInfo);
				}
			}
		}

		void ClearValueIfNeeded(ZPropertyInfo info)
		{
			if (!info.Value.IsDefault)
			{
				info.Value = info.DefaultValue;
			}
		}

		protected void DefaultPartyOrgAddressDetails(OrgAddress org, AsycudaBillAddress.AddressType addressType)
		{
			if (org != null)
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Shipper:
						PopulateOrgAddressDetails(org, addressType, ABL_ShipperNameInfo, ABL_ShipperStreet1Info, ABL_ShipperStreet2Info, ABL_ShipperCityInfo, ABL_ShipperStateInfo, ABL_ShipperPostcodeInfo, ABL_ShipperPhoneInfo, ABL_RN_NKShipperCountryInfo, ABL_ShipperRegNoInfo, ABL_ShipperRegNoTypeInfo, ShipperRegNoTypes());
						break;
					case AsycudaBillAddress.AddressType.Consignee:
						PopulateOrgAddressDetails(org, addressType, ABL_ConsigneeNameInfo, ABL_ConsigneeStreet1Info, ABL_ConsigneeStreet2Info, ABL_ConsigneeCityInfo, ABL_ConsigneeStateInfo, ABL_ConsigneePostcodeInfo, ABL_ConsigneePhoneInfo, ABL_RN_NKConsigneeCountryInfo, ABL_ConsigneeRegNoInfo, ABL_ConsigneeRegNoTypeInfo, ConsigneeRegNoTypes());
						break;
					case AsycudaBillAddress.AddressType.NotifyParty:
						PopulateOrgAddressDetails(org, addressType, ABL_NotifyPartyNameInfo, ABL_NotifyPartyStreet1Info, ABL_NotifyPartyStreet2Info, ABL_NotifyPartyCityInfo, ABL_NotifyPartyStateInfo, ABL_NotifyPartyPostcodeInfo, ABL_NotifyPartyPhoneInfo, ABL_RN_NKNotifyPartyCountryInfo, ABL_NotifyPartyRegNoInfo, ABL_NotifyPartyRegNoTypeInfo, NotifyPartyRegNoTypes());
						break;
					case AsycudaBillAddress.AddressType.Buyer:
						PopulateOrgAddressDetails(org, addressType, ABL_BuyerNameInfo, ABL_BuyerStreet1Info, ABL_BuyerStreet2Info, ABL_BuyerCityInfo, ABL_BuyerStateInfo, ABL_BuyerPostcodeInfo, ABL_BuyerPhoneInfo, ABL_RN_NKBuyerCountryInfo, ABL_BuyerRegNoInfo, ABL_BuyerRegNoTypeInfo, BuyerRegNoTypes());
						break;
					case AsycudaBillAddress.AddressType.Seller:
						PopulateOrgAddressDetails(org, addressType, ABL_SellerNameInfo, ABL_SellerStreet1Info, ABL_SellerStreet2Info, ABL_SellerCityInfo, ABL_SellerStateInfo, ABL_SellerPostcodeInfo, ABL_SellerPhoneInfo, ABL_RN_NKSellerCountryInfo, ABL_SellerRegNoInfo, ABL_SellerRegNoTypeInfo, SellerRegNoTypes());
						break;
				}
			}
		}

		void PopulateOrgAddressDetails(OrgAddress org, AsycudaBillAddress.AddressType addressType, ZPropertyInfo nameInfo, ZPropertyInfo street1Info, ZPropertyInfo street2Info,
			ZPropertyInfo cityInfo, ZPropertyInfo stateInfo, ZPropertyInfo postCodeInfo, ZPropertyInfo phoneInfo, ZPropertyInfo countryInfo, ZPropertyInfo regNoInfo, ZPropertyInfo regNoTypeInfo, ZString[] regNoTypes)
		{
			if (org != null)
			{
				if (nameInfo != null)
				{
					nameInfo.Value = org.EffectiveCompanyName;
				}
				if (street1Info != null)
				{
					street1Info.Value = org.OA_Address1;
				}
				if (street2Info != null)
				{
					street2Info.Value = org.Address2;
				}
				if (cityInfo != null)
				{
					cityInfo.Value = org.OA_City;
				}
				if (stateInfo != null)
				{
					stateInfo.Value = org.OA_State;
				}
				if (postCodeInfo != null)
				{
					postCodeInfo.Value = org.OA_PostCode;
				}
				if (phoneInfo != null)
				{
					phoneInfo.Value = org.PhoneNumber.FormattedForBinding.Left(phoneInfo.MaxLength);
				}
				if (countryInfo != null)
				{
					countryInfo.Value = org.OA_RN_NKCountryCode;
				}

				if (regNoInfo != null && regNoTypeInfo != null && Header != null)
				{
					var (regNumber, regNumberType) = GetPartyOrgAddressRegNoAndType(org, addressType, regNoTypes);

					regNoInfo.Value = regNumber.Left(regNoInfo.MaxLength);
					regNoTypeInfo.Value = regNumberType;
				}
			}
		}

		protected virtual (ZString RegNumber, ZString RegNumberType) GetPartyOrgAddressRegNoAndType(OrgAddress org, AsycudaBillAddress.AddressType addressType, ZString[] regNoTypes)
		{
			return GetRegNoAndTypeForParty(org, regNoTypes);
		}

		protected (ZString RegNo, ZString RegNoType) GetRegNoAndTypeForParty(OrgAddress address, params ZString[] regNoTypes)
		{
			foreach (var regNoType in regNoTypes)
			{
				var regNo = address?.Header?.CustomsCodes?.GetCustomsRegNo(regNoType, GetCountryCodeForCustomsRegNo(address)) ?? ZString.Empty;
				if (!regNo.IsEmpty)
				{
					return (regNo, regNoType);
				}
			}

			return (ZString.Empty, ZString.Empty);
		}

		protected (ZString RegNo, ZString RegNoType) GetRegNoAndTypeForAddress(OrgAddress address, params ZString[] regNoTypes)
		{
			foreach (var regNoType in regNoTypes)
			{
				var regNo = address?.Header?.CustomsCodes?.GetCustomsRegNoPremiseAddressOnly(regNoType, GetCountryCodeForCustomsRegNo(address), address.PK) ?? ZString.Empty;
				if (!regNo.IsEmpty)
				{
					return (regNo, regNoType);
				}
			}

			return (ZString.Empty, ZString.Empty);
		}

		protected virtual ZString GetCountryCodeForCustomsRegNo(OrgAddress address) => Header.AMA_RN_NKCountry;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = IsChildMasterBill ? "Manifest Master Bill" : "Manifest Bill";

				if (!ABL_BillNumber.IsEmpty)
				{
					result += " " + ABL_BillNumber;
				}

				return result;
			}
		}

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		#endregion

		#region ICanDelete Members

		public override bool CanDelete => base.CanDelete && !HasManifestBeenSubmittedToCustoms;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get => HasManifestBeenSubmittedToCustoms
				? ResString.GetMultilingualString("AD79DDD4-9FC2-425B-A8AD-D14C2D4B44C2", "This Bill is already registered with Customs.\r\nYou need to delete it from Customs file before deleting it here.")
				: base.ReasonForNotAbleToDelete;
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaBillFetchStrategy(this);

		#region ISailingSynchronisationTarget

		bool ISailingSynchronisationTarget<BillOfLading>.IsMatched(BillOfLading billOfLading)
		{
			return IsMatched(billOfLading);
		}

		bool IsMatched(BillOfLading billOfLading)
		{
			var sailing = Header?.Sailing;
			return sailing != null && ABL_BillNumber == GetBillNumber(billOfLading);
		}

		ZString GetBillNumber(BillOfLading billOfLading)
		{
			return billOfLading.JS_HouseBill.KeepAlphanumericCharacters().TrimEnd();
		}

		void ISailingSynchronisationTarget<BillOfLading>.Set(BillOfLading billOfLading)
		{
			using (GetValidationSuspender())
			{
				ABL_BillNumber = GetBillNumber(billOfLading);
			}
		}

		void ISailingSynchronisationTarget<BillOfLading>.Synchronise()
		{
			var source = SailingSynchronisationSource;

			if (source != null)
			{
				using (GetValidationSuspender())
				{
					Header.BillOfLadingForSync = source;

					ABL_RL_NKOrigin = source.JS_RL_NKOrigin;
					ABL_RL_NKFinalDestination = source.JS_RL_NKDestination;

					ABL_OA_Shipper = source.Consignor?.MainAddress?.PK ?? ZGuid.Empty;
					ABL_OA_Consignee = source.Consignee?.MainAddress?.PK ?? ZGuid.Empty;
					ABL_OA_NotifyParty = source.NotifyParty?.MainAddress?.PK ?? ZGuid.Empty;

					ABL_PrepaidCollect = source.JS_INCO;
					ABL_FreightValue = source.JS_GoodsValue;
					ABL_RX_NKFreightValueCurrency = source.JS_RX_NKGoodsValueCurr;

					ABL_MarksAndNumbers = source.JS_MarksAndNumbers.SubstringSafe(0, ABL_MarksAndNumbersInfo.MaxLength);
					ABL_GoodsDescription = source.JS_GoodsDescription.SubstringSafe(0, ABL_GoodsDescriptionInfo.MaxLength);

					ImportQuantities(source);
					ImportManifestSpecificValues(source);
				}
			}

			Header.BillOfLadingForSync = null;
		}

		void ImportQuantities(BillOfLading source)
		{
			if (source.IsContainerised)
			{
				var containerTargetCollection = new AsycudaContainerSynchronisationTargetCollection(Header.Containers);
				containerTargetCollection.Synchronise(source.RealContainers.Cast<BillOfLadingContainer>(), deleteOrphanTarget: false);

				var packLines = source.RealContainers
					.Cast<BillOfLadingContainer>()
					.SelectMany(x => x.PackLines)
					.Cast<BillOfLadingPackLine>()
					.ToArray();

				ABL_GrossWeightUQ = packLines.Select(x => x.JL_ActualWeightUQ).FirstOrDefault(x => !x.IsEmpty);
				ABL_VolumeUQ = packLines.Select(x => x.JL_ActualVolumeUQ).FirstOrDefault(x => !x.IsEmpty);
				ABL_ManifestUQ = packLines.Select(x => x.JL_F3_NKPackType).FirstOrDefault(x => !x.IsEmpty);

				ABL_GrossWeight = ABL_GrossWeightUQ.IsEmpty ? ABL_GrossWeight : (ZDecimal)packLines.Sum(x => new ZWeight(x.JL_ActualWeight, x.JL_ActualWeightUQ).ConvertTo(ABL_GrossWeightUQ));
				ABL_Volume = ABL_VolumeUQ.IsEmpty ? ABL_Volume : (ZDecimal)packLines.Sum(x => new ZVolume(x.JL_ActualVolume, x.JL_ActualVolumeUQ).ConvertTo(ABL_VolumeUQ));
				ABL_ManifestQty = packLines.Sum(x => x.JL_PackageCount);

				var packsTargetCollection = new AsycudaPackSynchronisationTargetCollection(Packs);
				packsTargetCollection.Synchronise(packLines, deleteOrphanTarget: true);

				AddPackLinesForEmptyContainers(source);
			}
			else
			{
				ABL_GrossWeight = source.JS_ActualWeight;
				ABL_GrossWeightUQ = source.JS_UnitOfWeight;
				ABL_Volume = source.JS_ActualVolume;
				ABL_VolumeUQ = source.JS_UnitOfVolume;
				ABL_ManifestQty = source.JS_OuterPacks;
				ABL_ManifestUQ = source.JS_F3_NKPackType;
			}
		}

		void AddPackLinesForEmptyContainers(BillOfLading source)
		{
			var containersWithoutPackLines = source.RealContainers
				.Cast<BillOfLadingContainer>()
				.Where(x => x.PackLines.Count == 0);

			foreach (var container in containersWithoutPackLines)
			{
				var emptyPackLine = Packs.AddNew();
				emptyPackLine.SetContainer(container.JC_ContainerNum, Header);
			}
		}

		protected virtual void ImportManifestSpecificValues(BillOfLading source)
		{
		}

		BillOfLading ISailingSynchronisationTarget<BillOfLading>.Source => SailingSynchronisationSource;

		BillOfLading SailingSynchronisationSource
		{
			get
			{
				return sailingSynchronisationSource != null && IsMatched(sailingSynchronisationSource) && !sailingSynchronisationSource.IsCancelled
					? sailingSynchronisationSource
					: sailingSynchronisationSource = Header.BillsOfLadings.FirstOrDefault(x => IsMatched(x) && !x.IsCancelled);
			}
		}
		BillOfLading sailingSynchronisationSource;

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

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					result.AddRange(Packs.Cast<AsycudaPack>());
				}
				return result.ToArray();
			}
		}

		public ZGlobalMutex SendToManifestMutex
		{
			get
			{
				return sendToManifestMutex ?? (sendToManifestMutex = new ZGlobalMutex(MutexIDs.SendCustomsMessage, PK.ToString()));
			}
		}
		ZGlobalMutex sendToManifestMutex;

		#region ABL_SequenceNumber
		[ResourceStringData("AsycudaBill.ABL_SequenceNumber", Caption = "Sequence Number", ShortCaption = "Seq. No")]
		public override ZShort ABL_SequenceNumber
		{
			get => base.ABL_SequenceNumber;
			set
			{
				var oldValue = ABL_SequenceNumber;
				base.ABL_SequenceNumber = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					Header?.Bills.SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		ZGuid ISequenceNumberLine.FKToHeader => ABL_AMA;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return ABL_SequenceNumber; }
			set { ABL_SequenceNumber = value; }
		}

		#endregion

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());

		SetterSuspender setterSuspender;

		IEnumerable<string> ISetterSuspenderSupporter.SupportedFields => GetSupportedFields();

		protected virtual IEnumerable<string> GetSupportedFields() => Enumerable.Empty<string>();

		#endregion

		#region IManifestBillForSynchroniser

		IManifestBillAddress IManifestBillForSynchroniser.Consignee => ConsigneeABLAddress;

		IManifestBillAddress IManifestBillForSynchroniser.ForeignShipper => ShipperABLAddress;

		IManifestHeaderForSynchroniser IManifestBillForSynchroniser.Header => Header;

		bool IManifestBillForSynchroniser.IsBillAlreadyOnFile => HasManifestBeenSubmittedToCustoms;

		ZPropertyInfo IManifestBillForSynchroniser.LastForeignPortInfo => ABL_RL_NKOriginInfo;

		ZPropertyInfo IManifestBillForSynchroniser.ManifestQtyInfo => ABL_ManifestQtyInfo;

		ZPropertyInfo IManifestBillForSynchroniser.ManifestUQInfo => ABL_ManifestUQInfo;

		ZPropertyInfo IManifestBillForSynchroniser.MasterBillNumberInfo => ABL_BillNumberInfo;

		IManifestBillAddress IManifestBillForSynchroniser.NotifyParty1 => NotifyPartyABLAddress;

		ZPropertyInfo IManifestBillForSynchroniser.PlaceOfReceiptInfo => ABL_RL_NKFinalDestinationInfo;

		ZPropertyInfo IManifestBillForSynchroniser.PortOfLadingInfo => ABL_RL_NKOriginInfo;

		ZPropertyInfo IManifestBillForSynchroniser.VolumeInfo => ABL_VolumeInfo;

		ZPropertyInfo IManifestBillForSynchroniser.VolumeUQInfo => ABL_VolumeUQInfo;

		ZPropertyInfo IManifestBillForSynchroniser.WeightInfo => ABL_GrossWeightInfo;

		ZPropertyInfo IManifestBillForSynchroniser.WeightUQInfo => ABL_GrossWeightUQInfo;

		public ZBool SynchronisePaymentType => ShouldSynchronisePaymentType();
		protected virtual ZBool ShouldSynchronisePaymentType() => ZBool.False;

		#endregion

		public ZDecimal TotalPacksGrossWeight => Factory.GetValue(ref totalPacksGrossWeightInCached, () =>
		{
			var result = ZDecimal.Zero;

			if (!ABL_GrossWeightUQ.IsEmpty)
			{
				foreach (AsycudaPack item in Packs)
				{
					if (!item.APA_WeightUQ.IsEmpty)
					{
						if (item.APA_WeightUQ != ABL_GrossWeightUQ)
						{
							result += Core.Constants.Weight.ConvertSafe(item.APA_Weight, item.APA_WeightUQ, ABL_GrossWeightUQ);
						}
						else
						{
							result += item.APA_Weight;
						}
					}
				}
			}

			return result;
		});
		CachedProperty<ZDecimal> totalPacksGrossWeightInCached;

		public ZDecimal TotalPacksVolume => Factory.GetValue(ref totalPacksVolumeInCached, () =>
		{
			var result = ZDecimal.Zero;

			if (!ABL_VolumeUQ.IsEmpty)
			{
				foreach (AsycudaPack item in Packs)
				{
					if (!item.APA_VolumeUQ.IsEmpty)
					{
						if (item.APA_VolumeUQ != ABL_VolumeUQ)
						{
							result += Core.Constants.Volume.ConvertSafe(item.APA_Volume, item.APA_VolumeUQ, ABL_VolumeUQ);
						}
						else
						{
							result += item.APA_Volume;
						}
					}
				}
			}

			return result;
		});
		CachedProperty<ZDecimal> totalPacksVolumeInCached;
	}
}
