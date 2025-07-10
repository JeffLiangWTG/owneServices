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
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[SystemDefinedValues]
	public class AsycudaPackedItem : ManifestBase.AsycudaPackedItem
		, IStatusSupporter
		, Integration.Customs.ASYCUDA.IAsycudaPackedItem
		, IUnitConverterDataProvider
		, IMessageParent
		, ITariffFormatProvider
		, IUNDGDataItemProvider
		, ISynchroniserReadOnlyMembersProvider
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Aligns Schema with updated base class functionality.")]
		public new class Schema : ManifestBase.AsycudaPackedItem.Schema
		{
			public const string API_FormattedTariff = "API_FormattedTariff";
			public const string RegistrationNumber = "RegistrationNumber";
		}

		public new static readonly AsycudaPackedItemTypeDecider TypeDecider = new AsycudaPackedItemTypeDecider();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaPackedItemFetchStrategy(this);

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;

		public OrgHeader Consignee => Factory.Load<OrgHeader>(Pack?.Bill?.ConsigneeOrgPK ?? ZGuid.Empty);

		public string SetNewCountryMessagingStatus(ZString newStatus)
		{
			var oldStatusForRollback = ZString.Empty;
			oldStatusForRollback = API_MessageStatus;
			API_MessageStatus = newStatus;
			SetNewCountryMessagingStatusAdditionalAction(newStatus, oldStatusForRollback);
			return oldStatusForRollback;
		}

		protected virtual void SetNewCountryMessagingStatusAdditionalAction(ZString newStatus, ZString oldStatusForRollback)
		{
		}

		public string SetNewCountryCustomsStatus(ZString newStatus)
		{
			var oldStatusForRollback = "";
			oldStatusForRollback = API_PackStatus;
			API_PackStatus = newStatus;
			return oldStatusForRollback;
		}

		[ChildEditable(true)]
		public AsycudaPackedItemEntryNumCollection CustomsEntryNumbers
		{
			get
			{
				if (customsEntryNumbers == null)
				{
					customsEntryNumbers = CreateNewAsycudaPackedItemEntryNumCollection();
					customsEntryNumbers.Load();
					RegisterEditableChildObject(customsEntryNumbers);
				}

				return customsEntryNumbers;
			}
		}
		AsycudaPackedItemEntryNumCollection customsEntryNumbers;

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		protected virtual AsycudaPackedItemEntryNumCollection CreateNewAsycudaPackedItemEntryNumCollection() => new AsycudaPackedItemEntryNumCollection(this);

		protected virtual bool RegistrationDetails_ReadOnly => true;

		[MaxLength(Common.CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		public ZString RegistrationNumber => RegistrationEntryNumber?.CE_EntryNum ?? ZString.Empty;

		public ZPropertyInfo RegistrationNumberInfo
		{
			get { return GetZPropertyInfo(Schema.RegistrationNumber); }
		}

		public AsycudaPackedItemEntryNum RegistrationEntryNumber
		{
			get
			{
				if (registrationEntryNumber == null || registrationEntryNumber.IsDeleted || registrationEntryNumber.CE_EntryType != CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
				{
					var countryCode = CountryCode;
					registrationEntryNumber = CustomsEntryNumbers.OfType<AsycudaPackedItemEntryNum>().FirstOrDefault(x => x.CE_RN_NKCountryCode == countryCode && x.CE_EntryType == CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
					if (registrationEntryNumber != null)
					{
						RegisterEditableChildObject(registrationEntryNumber);
					}
				}

				return registrationEntryNumber;
			}
		}
		AsycudaPackedItemEntryNum registrationEntryNumber;

		[BusinessObjectTestExclude]
		[ResourceStringData("AsycudaManifestHeader.API_FormattedTariff", Caption = "Tariff", ShortCaption = "Tariff")]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.TariffList))]
		public virtual ZString API_FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(API_Tariff); }
			set { API_Tariff = value; }
		}

		public ZPropertyInfo API_FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.API_FormattedTariff, x => API_TariffInfo); }
		}

		public TariffView UniversalTariff
		{
			get
			{
				var tariff = API_Tariff;
				if (tariff.IsEmpty)
				{
					return null;
				}

				var applicationBusinessProvider = Header?.ApplicationBusinessProvider;
				var dataGrouping = applicationBusinessProvider?.PackedItemTariffDataGrouping ?? string.Empty;
				var tariffType = applicationBusinessProvider?.PackedItemTariffType ?? string.Empty;
				return new TariffView.Loader(Factory).LoadMostRecentCachedTariff(dataGrouping, tariffType, tariff, EffectiveDateForDutyRate);
			}
		}

		ITariffFormatter TariffFormatter => TariffFormatterDecider.GetByCountryCode(CountryCode);

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#region override properties
		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchStrategy.FetchForDelete();
				this.DeleteChildren<AsycudaPackedItemEntryNum>(CusEntryNumSchema.CE_ParentID);
			}
			base.Delete();
		}

		[DecimalPlaces("API_CustomsValueDecimalPlaces")]
		[ResourceStringData("AsycudaManifestHeader.API_CustomsValue", Caption = "Customs Value", ShortCaption = "Customs Value")]
		public override ZDecimal API_CustomsValue
		{
			get => base.API_CustomsValue;
			set => base.API_CustomsValue = value;
		}

		public int API_CustomsValueDecimalPlaces => LocalCurrencyDecimals;

		[DecimalPlaces("API_DutyAmountDecimalPlaces")]
		[ResourceStringData("AsycudaManifestHeader.API_DutyAmount", Caption = "Duty Amount", ShortCaption = "Duty Amount")]
		public override ZDecimal API_DutyAmount
		{
			get => base.API_DutyAmount;
			set => base.API_DutyAmount = value;
		}

		public int API_DutyAmountDecimalPlaces => LocalCurrencyDecimals;

		[DecimalPlaces("API_TaxAmountDecimalPlaces")]
		[ResourceStringData("AsycudaManifestHeader.API_TaxAmount", Caption = "Tax Amount", ShortCaption = "Tax Amount")]
		public override ZDecimal API_TaxAmount
		{
			get => base.API_TaxAmount;
			set
			{
				var oldValue = base.API_TaxAmount;

				if (oldValue != value)
				{
					base.API_TaxAmount = value;
					MakeBillApportionmentDirtyIfNeed();
				}
			}
		}
		public int API_TaxAmountDecimalPlaces => LocalCurrencyDecimals;

		#region Tariff
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.TariffList))]
		public override ZString API_Tariff
		{
			get { return base.API_Tariff; }
			set { base.API_Tariff = TariffFormatter.Format(value).Left(API_TariffInfo.MaxLength); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.CustomsStatusList))]
		[ReadOnlyMember(nameof(API_PackStatus_ReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.API_PackStatus", Caption = "Customs Status", ShortCaption = "Customs Status")]
		public override ZString API_PackStatus
		{
			get => base.API_PackStatus;
			set
			{
				if (API_PackStatus != value)
				{
					base.API_PackStatus = value;
					((IStatusSupporter)this).LogEventsOnParent(Events.CustomsManifestStatus, API_PackStatus);
				}
			}
		}

		protected virtual bool API_PackStatus_ReadOnly => true;

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.MessageStatusList))]
		[ReadOnlyMember(nameof(API_MessageStatus_ReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.API_MessageStatus", Caption = "Message Status", ShortCaption = "Message Status")]
		public override ZString API_MessageStatus
		{
			get => base.API_MessageStatus;
			set
			{
				if (API_MessageStatus != value)
				{
					base.API_MessageStatus = value;
					((IStatusSupporter)this).LogEventsOnParent(Events.MessageStatusChange, API_MessageStatus);
				}
			}
		}

		protected virtual bool API_MessageStatus_ReadOnly => true;

		[MaxLength(Schema.API_CustomsUQMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.CustomsUQList))]
		[ResourceStringData("AsycudaManifestHeader.API_CustomsUQ", Caption = "Customs UQ", ShortCaption = "UQ")]
		public override ZString API_CustomsUQ { get => base.API_CustomsUQ; set => base.API_CustomsUQ = value; }

		[MaxLength(Schema.API_CustomsUQ2MaxLength)]
		public override ZString API_CustomsUQ2 { get => base.API_CustomsUQ2; set => base.API_CustomsUQ2 = value; }

		[MaxLength(Schema.API_CustomsUQ3MaxLength)]
		public override ZString API_CustomsUQ3 { get => base.API_CustomsUQ3; set => base.API_CustomsUQ3 = value; }

		[ResourceStringData("AsycudaManifestHeader.API_CustomsQty", Caption = "Customs Qty", ShortCaption = "Customs Qty")]
		public override ZDecimal API_CustomsQty { get => base.API_CustomsQty; set => base.API_CustomsQty = value; }

		[ResourceStringData("AsycudaManifestHeader.API_GoodsValue", Caption = "Goods Value", ShortCaption = "Goods Value")]
		public override ZDecimal API_GoodsValue { get => base.API_GoodsValue; set => base.API_GoodsValue = value; }

		[ResourceStringData("AsycudaManifestHeader.API_RX_NKGoodsValueCurrency", Caption = "Goods Value Currency", ShortCaption = "Currency")]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.Currencies))]
		public override ZString API_RX_NKGoodsValueCurrency { get => base.API_RX_NKGoodsValueCurrency; set => base.API_RX_NKGoodsValueCurrency = value; }

		[ResourceStringData("AsycudaManifestHeader.API_RN_NKGoodsOrigin", Caption = "Goods Origin Country/Region", MediumCaption = "Goods Origin Ctry/Rgn.", ShortCaption = "Origin", FullDescription = "ISO 3166-1 alpha-2 country code of the origin of the goods.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.Countries))]
		public override ZString API_RN_NKGoodsOrigin { get => base.API_RN_NKGoodsOrigin; set => base.API_RN_NKGoodsOrigin = value; }

		[ResourceStringData("AsycudaManifestHeader.API_NetWeight", Caption = "Net Weight", ShortCaption = "Net Weight")]

		public override ZDecimal API_NetWeight { get => base.API_NetWeight; set => base.API_NetWeight = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.NetWeightUQList))]
		[ResourceStringData("AsycudaManifestHeader.API_NetWeightUQ", Caption = "Net Weight UQ", ShortCaption = "UQ")]
		public override ZString API_NetWeightUQ { get => base.API_NetWeightUQ; set => base.API_NetWeightUQ = value; }

		[ResourceStringData("AsycudaManifestHeader.API_GrossWeight", Caption = "Gross Weight", ShortCaption = "Gross Weight")]
		public override ZDecimal API_GrossWeight { get => base.API_GrossWeight; set => base.API_GrossWeight = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.GrossWeightUQList))]
		[ResourceStringData("AsycudaManifestHeader.API_GrossWeightUQ", Caption = "Gross Weight UQ", ShortCaption = "UQ")]
		public override ZString API_GrossWeightUQ { get => base.API_GrossWeightUQ; set => base.API_GrossWeightUQ = value; }

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = "Pack Country";

				var countryCode = CountryCode;
				if (!countryCode.IsEmpty)
				{
					result += " " + countryCode;
				}

				return result;
			}
		}

		#endregion

		public ZDateTime EffectiveDateForDutyRate
		{
			get { return Header?.ApplicationBusinessProvider.GetEffectiveDateForDutyRate(Header) ?? ZDateTime.Today; }
		}

		#region Header
		public AsycudaManifestHeader Header
		{
			get
			{
				if (!IsDeleted && (header == null || header.IsDeleted))
				{
					header = (AsycudaManifestHeader)Bill?.Header;
				}
				return header;
			}
		}
		AsycudaManifestHeader header;

		public ZString CountryCode => Header?.AMA_RN_NKCountry ?? ZString.Empty;
		#endregion

		public bool HasManifestBeenSubmittedToCustoms => MessageStatusProvider?.HasManifestBeenSubmittedToCustoms(this) ?? false;

		public MessageStatusProvider MessageStatusProvider => Header?.MessageStatusProvider;

		#region IMessageParent

		ZString IMessageParent.ManifestType => Header?.AMA_ManifestType ?? ZString.Empty;

		bool IMessageParent.HasCustomsNumbers => !((IMessageParent)this).RegistrationNumber.IsEmpty;

		bool IMessageParent.IsCustomsCleared => AsycudaUniversalReference.CustomsStatusAttributeHelper.IsCustomsCleared(Factory, CountryCode, API_PackStatus);

		ISelectionItem IMessageParent.SelectionItem => Pack;

		ZString IMessageParent.MessageStatus => API_MessageStatus;

		ZString IMessageParent.CustomsStatus => API_PackStatus;

		void IMessageParent.ResetMessageStatus()
		{
			API_MessageStatus = ZString.Empty;
		}

		#endregion

		#region IStatusSupporter
		ZString IStatusSupporter.ManifestPermitNumber { set => AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<AsycudaPackedItemEntryNum>(this, value, CountryCode); }
		ZString IStatusSupporter.CustomsStatus { get => API_PackStatus; set => API_PackStatus = value; }
		ZString IStatusSupporter.MessageStatus { set => API_MessageStatus = value; }
		ZString IStatusSupporter.CustomsEntryNumber { set => AsycudaEntryNumberCreator.CreateOrUpdateCustomsEntryNumber<AsycudaPackedItemEntryNum>(this, value, CountryCode); }
		ZBool IStatusSupporter.SupportsPackLevelMessages => Header?.IsPackedItemLevelManifestType ?? false;
		void IStatusSupporter.LogEventsOnParent(Event eventType, ZString reference)
		{
			var pack = Pack;
			if (pack != null)
			{
				pack.Logs.AddNew(eventType, reference, ZDateTimeOffset.Now);
			}
		}

		#endregion

		#region UnitConverters

		public UnitConverter UnitConverter => fUnitConverter ?? (fUnitConverter = new UnitConverter(this));
		UnitConverter fUnitConverter;

		#endregion

		#region IUnitConverterDataProvider

		ZString IUnitConverterDataProvider.CountryCode
		{
			get { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode); }
		}

		BusinessObjectFactory IUnitConverterDataProvider.Factory
		{
			get { return Factory; }
		}

		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits()
		{
			return Array.Empty<IUnitConverter>();
		}

		MasterFiles.Business.OrgSupplierPart IUnitConverterDataProvider.Product
		{
			get { return null; }
		}

		ZGuid IUnitConverterDataProvider.SupplierFK
		{
			get { return Pack?.Bill?.Shipper?.OA_OH ?? ZGuid.Empty; }
		}

		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions
		{
			get { return false; }
		}

		ZString IUnitConverterDataProvider.Type
		{
			get { return RPTypeList.Codes.GlobalManifestLine; }
		}

		#endregion

		#region ReadOnly

		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		#endregion

		#region Implementation

		protected virtual void MakeBillApportionmentDirtyIfNeed()
		{
		}

		protected virtual int LocalCurrencyDecimals => Factory.GetValue(ref localCurrencyDecimalsCached, () => Header?.Country?.LocalCurrency?.Decimals ?? 4);

		CachedProperty<int> localCurrencyDecimalsCached;

		#endregion

		protected override ManifestBase.IAsycudaPackedItemTaxCollection<ManifestBase.AsycudaTax, ManifestBase.AsycudaPackedItem> CreateNewAsycudaPackedItemTaxCollection() => new AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem>(this);

		public new IAsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem> AsycudaTaxes => (IAsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem>)base.AsycudaTaxes;

		protected override Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);
	}
}
