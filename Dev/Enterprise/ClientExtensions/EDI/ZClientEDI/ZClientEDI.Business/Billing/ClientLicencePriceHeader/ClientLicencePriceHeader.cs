using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(LicenceCompany), "PriceHeaders")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientLicencePriceHeader : AutoClientLicencePriceHeader
	{
		public ClientLicencePriceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetChildCollectionsToReadOnlyIfCargoWiseNextAndPastValidFrom();
		}

		public LicenceCompany LicCompany
		{
			get { return Factory.Load<LicenceCompany>(L6_LC); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			// Original code was all for OnDemand billing
			L6_SystemCode = BillingConstants.BillingSystem.ODM;

			L6_TestDbPriceCode = "#NP";
			L6_LiveMonthsUntilTestDbBilling = 3;
			L6_Rounding = ZString.Empty;
		}

		#region PriceItems

		[ChildEditable]
		public ClientLicencePriceItemCollection Items
		{
			get
			{
				if (items == null)
				{
					items = new ClientLicencePriceItemCollection(this);
					RegisterEditableChildObject(items);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || L6_IsStandard)
					{
						items.SetReadOnlyIncludingChildren(true);
					}
				}

				return items;
			}
		}
		ClientLicencePriceItemCollection items;
		public bool AreItemsLoaded => items != null;

		[ChildEditable]
		public ClientLicencePriceItemCollection LocalOrStandardItems
		{
			get
			{
				ClientLicencePriceItemCollection result = null;
				if (L6_IsStandard)
				{
					ClientLicencePriceHeader licPriceHeader = StandardPrices;
					if (licPriceHeader != null)
					{
						result = licPriceHeader.Items;
						result.SetReadOnlyIncludingChildren(true);
					}
				}
				AreLocalOrStandardItemsLoaded = true;
				return result ?? Items;
			}
		}
		public bool AreLocalOrStandardItemsLoaded { get; private set; }

		#endregion

		#region Usage Mapping

		[ChildEditable]
		public EdiPriceUsageMappingCollection UsageMaps
		{
			get
			{
				if (usageMaps == null)
				{
					usageMaps = new EdiPriceUsageMappingCollection(this);
					RegisterEditableChildObject(usageMaps);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || !BillingConstants.PriceHeaderType.IsUsedInBillingStl(L6_SystemCode))
					{
						usageMaps.SetReadOnlyIncludingChildren(true);
					}
				}

				return usageMaps;
			}
		}
		EdiPriceUsageMappingCollection usageMaps;

		#endregion

		#region Exchange Rates

		[ChildEditable]
		public EdiPriceHeaderExchangeRateCollection ExchangeRates
		{
			get
			{
				if (exchangeRates == null)
				{
					exchangeRates = new EdiPriceHeaderExchangeRateCollection(this);
					RegisterEditableChildObject(exchangeRates);
					RefreshReadOnlyForChildItems();
				}

				return exchangeRates;
			}
		}
		EdiPriceHeaderExchangeRateCollection exchangeRates;

		void RefreshReadOnlyForChildItems()
		{
			var exchangeRatesReadOnly = ReadOnly
				|| !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed
				|| L6_SystemCode == BillingConstants.PriceHeaderType.ODM
				|| L6_SystemCode == BillingConstants.PriceHeaderType.Maintenance
				|| !L6_HasExchangeRates;

			ExchangeRates.SetReadOnlyIncludingChildren(exchangeRatesReadOnly);

			foreach (var item in Items)
			{
				item.RefreshReadOnlyForChildItems();
			}

			ExchangeRates.RefreshBinding();
		}

		#endregion

		public bool HasSettings(ZString systemCode, ZString countryCode, ZString licenceEdition, ZString currencyCode)
		{
			return 0 == string.Compare(L6_SystemCode, systemCode, StringComparison.OrdinalIgnoreCase)
				&& 0 == string.Compare(L6_RN_NKCountry, countryCode, StringComparison.OrdinalIgnoreCase)
				&& 0 == string.Compare(L6_LicenceEdition, licenceEdition, StringComparison.OrdinalIgnoreCase)
				&& 0 == string.Compare(L6_RX_NKCurrency, currencyCode, StringComparison.OrdinalIgnoreCase);
		}

		public bool HasSettings(ZString systemCode, ZString countryCode, ZString licenceEdition, ZString currencyCode, ZString version)
		{
			return HasSettings(systemCode, countryCode, licenceEdition, currencyCode)
				&& string.Compare(L6_PricelistVersion, version, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public bool HasSettings(ClientLicencePriceHeader other)
		{
			return HasSettings(other.L6_SystemCode, other.L6_RN_NKCountry, other.L6_LicenceEdition, other.L6_RX_NKCurrency, other.L6_PricelistVersion);
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceHeaderLookups.LicenceEditions))]
		public override ZString L6_LicenceEdition
		{
			get { return base.L6_LicenceEdition; }
			set
			{
				base.L6_LicenceEdition = value;
				InvalidateStandardPrices();
			}
		}

		public bool L6_LicenceEdition_ReadOnly => BillingConstants.PriceHeaderType.IsUsedInBillingStl(L6_SystemCode);

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceHeaderLookups.PricelistVersions))]
		public override ZString L6_PricelistVersion
		{
			get { return base.L6_PricelistVersion; }
			set
			{
				base.L6_PricelistVersion = value;
				InvalidateStandardPrices();
			}
		}

		public bool L6_PricelistVersion_ReadOnly
		{
			get { return false; }
		}

		public override ZDecimal L6_LicenceUnitRate
		{
			get
			{
				var stdPrices = StandardPrices;
				return stdPrices != null ? stdPrices.L6_LicenceUnitRate : base.L6_LicenceUnitRate;
			}
			set
			{
				base.L6_LicenceUnitRate = value;
			}
		}

		public bool L6_LicenceUnitRate_ReadOnly
		{
			get { return L6_IsStandard; }
		}

		// Should be considered for deleting/re-factoring a.s.a.p.
		public ZBool IsQuickTransactionalPricelist
		{
			get { return !LocalOrStandardItems.Any(x => x.L7_Code == BillingConstants.CoreModuleCode); }
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceHeaderLookups.SystemCodes))]
		public override ZString L6_SystemCode
		{
			get { return base.L6_SystemCode; }
			set
			{
				base.L6_SystemCode = value;
				if (L6_RN_NKCountry_ReadOnly)
				{
					L6_RN_NKCountry = "";
				}
				if (L6_LicenceEdition_ReadOnly)
				{
					L6_LicenceEdition = "";
				}
				if (L6_HasExchangeRates_ReadOnly)
				{
					L6_HasExchangeRates = false;
				}

				if (isCargoWiseNextAndPastValidFromCached && value != BillingConstants.PriceHeaderType.CargoWiseNext)
				{
					isCargoWiseNextAndPastValidFromCached = false;
				}
				else
				{
					SetChildCollectionsToReadOnlyIfCargoWiseNextAndPastValidFrom();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateL6_ValidFrom();
				}

				RemoveRowError(ClientLicencePriceHeader.CargoWiseNextPastValidFromEditErrorMessage);
				InvalidateStandardPrices();
			}
		}

		public bool IsCargoWiseNext => L6_SystemCode == BillingConstants.PriceHeaderType.CargoWiseNext;

		void SetChildCollectionsToReadOnlyIfCargoWiseNextAndPastValidFrom()
		{
			if (!IsInDatabase || !IsCargoWiseNextAndPastValidFrom)
			{
				return;
			}

			Items.SetReadOnlyIncludingChildren(true);
			StlDiscounts.SetReadOnlyIncludingChildren(true);
			StlItemDiscounts.SetReadOnlyIncludingChildren(true);
			UsageMaps.SetReadOnlyIncludingChildren(true);
			ExchangeRates.SetReadOnlyIncludingChildren(true);
		}

		public override ZDateTime L6_ValidFrom
		{
			get => base.L6_ValidFrom;
			set
			{
				if (isCargoWiseNextAndPastValidFromCached)
				{
					isCargoWiseNextAndPastValidFromCached = false;
				}

				base.L6_ValidFrom = value;

				RemoveRowError(ClientLicencePriceHeader.CargoWiseNextPastValidFromEditErrorMessage);
				SetChildCollectionsToReadOnlyIfCargoWiseNextAndPastValidFrom();
			}
		}

		public override ZString L6_RN_NKCountry
		{
			get { return base.L6_RN_NKCountry; }
			set
			{
				base.L6_RN_NKCountry = value;
				InvalidateStandardPrices();
			}
		}

		public bool L6_RN_NKCountry_ReadOnly => BillingConstants.PriceHeaderType.IsUsedInBillingStl(L6_SystemCode);

		public override ZString L6_RX_NKCurrency
		{
			get { return base.L6_RX_NKCurrency; }
			set
			{
				base.L6_RX_NKCurrency = value;
				InvalidateStandardPrices();
			}
		}

		[ReadOnly(true)]
		public override ZDateTime L6_SystemCreateTimeUtc
		{
			get { return base.L6_SystemCreateTimeUtc; }
			set { base.L6_SystemCreateTimeUtc = value; }
		}

		[ReadOnly(true)]
		public override ZString L6_SystemCreateUser
		{
			get { return base.L6_SystemCreateUser; }
			set { base.L6_SystemCreateUser = value; }
		}

		public override ZBool L6_HasExchangeRates
		{
			get => base.L6_HasExchangeRates;
			set
			{
				base.L6_HasExchangeRates = value;
				RefreshReadOnlyForChildItems();
			}
		}

		public ClientLicencePriceHeader StandardPrices
		{
			get
			{
				if (!standardPricesLoaded)
				{
					standardPricesLoaded = true;
					if (L6_IsStandard && LicenceCompany.StandardPricesCompany != null && LicenceCompany.StandardPricesCompany.PK != L6_LC)
					{
						standardPrices = LicenceCompany.StandardPricesCompany.PriceHeaders.FindSettings(this);
					}
					else
					{
						standardPrices = null;
					}
				}

				return standardPrices;
			}
		}

		void InvalidateStandardPrices()
		{
			standardPricesLoaded = false;
			L6_DiscountCodeInfo.RefreshBinding();
			L6_LicenceUnitRateInfo.RefreshBinding();

			if (L6_IsStandard && StandardPrices != null)
			{
				L6_TestDbPriceCode = StandardPrices.L6_TestDbPriceCode;
				L6_LiveMonthsUntilTestDbBilling = StandardPrices.L6_LiveMonthsUntilTestDbBilling;
			}
		}

		bool standardPricesLoaded;
		ClientLicencePriceHeader standardPrices;

		public IEnumerable<string> SecondaryPriceListTypesWithLocallyUniqueCodes
		{
			get
			{
				yield return BillingConstants.PriceHeaderType.BorderWise;
			}
		}

		public bool IsSecondaryPriceListTypeWithLocallyUniqueCodes => SecondaryPriceListTypesWithLocallyUniqueCodes.Any(x => x == L6_SystemCode);

		internal bool IsMainPriceListType
		{
			get { return L6_SystemCode == BillingConstants.PriceHeaderType.ODM || L6_SystemCode == BillingConstants.PriceHeaderType.STL || L6_SystemCode == BillingConstants.PriceHeaderType.Maintenance; }
		}

		#region Discounts

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceHeaderLookups.DiscountCodes))]
		public override ZString L6_DiscountCode
		{
			get
			{
				if (L6_DiscountCode_ReadOnly)
				{
					ClientLicencePriceHeader stdPrices = StandardPrices;
					return stdPrices != null ? stdPrices.L6_DiscountCode : ZString.Empty;
				}
				else
				{
					return base.L6_DiscountCode;
				}
			}
			set
			{
				base.L6_DiscountCode = value;
			}
		}

		public bool L6_DiscountCode_ReadOnly
		{
			get { return L6_UseStandardDiscount; }
		}

		public ClientLicenceBillingDiscountCollection Discounts
		{
			get { return LoadDiscounts(); }
		}

		ClientLicenceBillingDiscountCollection LoadDiscounts()
		{
			ClientLicenceBillingDiscountCollection result = null;

			if (!L6_DiscountCode.IsEmpty && LicenceCompany.StandardPricesCompany != null)
			{
				var billing = LicenceCompany.StandardPricesCompany.ReadonlySelfBilling;
				if (billing != null)
				{
					result = new ClientLicenceBillingDiscountCollection(Factory, billing, new ZQuery(ClientLicenceBillingDiscountSchema.L5_DiscountCode, L6_DiscountCode));
					result.SetReadOnlyIncludingChildren(true);
				}
			}

			return result ?? new ClientLicenceBillingDiscountCollection(Factory, null, ZQuery.NoResultQuery);
		}

		public bool L6_UseStandardDiscount_ReadOnly
		{
			get { return !L6_IsStandard && (LicenceCompany.StandardPricesCompany == null || LicenceCompany.StandardPricesCompany.PK != L6_LC); }
		}

		#endregion

		#region STL discounts

		[ChildEditable]
		public EdiPriceHeaderDiscountCollection StlDiscounts
		{
			get { return stlDiscounts ?? (stlDiscounts = LoadStlDiscounts()); }
		}
		EdiPriceHeaderDiscountCollection stlDiscounts;

		EdiPriceHeaderDiscountCollection LoadStlDiscounts()
		{
			var result = new EdiPriceHeaderDiscountCollection(this);
			RegisterEditableChildObject(result);
			return result;
		}

		[ChildEditable]
		public EdiPriceDiscountGroupMemberCollection StlItemDiscounts
		{
			get { return stlItemDiscounts ?? (stlItemDiscounts = LoadStlItemDiscounts()); }
		}
		EdiPriceDiscountGroupMemberCollection stlItemDiscounts;

		EdiPriceDiscountGroupMemberCollection LoadStlItemDiscounts()
		{
			var result = new EdiPriceDiscountGroupMemberCollection(this);
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region TestDbPriceCode

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceHeaderLookups.PriceCodesForServiceCategory))]
		public override ZString L6_TestDbPriceCode
		{
			get { return base.L6_TestDbPriceCode; }
			set { base.L6_TestDbPriceCode = value; }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceHeaderLookups.RoundingList))]
		public override ZString L6_Rounding { get => base.L6_Rounding; set => base.L6_Rounding = value; }

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceHeaderLookups.RoundingList))]
		public ZString L6_RoundingForBinding
		{
			get => !L6_Rounding_ReadOnly ? L6_Rounding : ZString.Empty;
			set => L6_Rounding = value;
		}

		public virtual ZPropertyInfo L6_RoundingForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(L6_RoundingForBinding), x => L6_RoundingInfo); }
		}

		public bool L6_Rounding_ReadOnly => !L6_HasExchangeRates || L6_HasExchangeRates_ReadOnly;
		public bool L6_RoundingForBinding_ReadOnly => L6_Rounding_ReadOnly;

		public IEnumerable<PriceRounding> PriceRoundingParams
		{
			get => Factory.GetCachedValue("PriceRoundingParams" + L6_Rounding, () => CreatePriceRoundingParams(L6_Rounding));
		}

		IEnumerable<PriceRounding> CreatePriceRoundingParams(string code)
		{
			string text = EDIDataRegistry.Instance.BillingPriceRoundingParams.Value.GetDescriptionFromCode(code);
			if (!string.IsNullOrEmpty(text))
			{
				return PriceRounding.DecodePriceRoundingParams(text);
			}

			return Enumerable.Empty<PriceRounding>();
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		protected bool L6_HasExchangeRates_ReadOnly
		{
			get
			{
				switch (L6_SystemCode.ToString())
				{
					case BillingConstants.PriceHeaderType.ABMCustoms:
					case BillingConstants.PriceHeaderType.BorderWise:
					case BillingConstants.PriceHeaderType.GlobalContainerTracking:
					case BillingConstants.PriceHeaderType.EHub:
					case BillingConstants.PriceHeaderType.FlightStats:
					case BillingConstants.PriceHeaderType.LDaaS:
					case BillingConstants.PriceHeaderType.Maintenance:
					case BillingConstants.PriceHeaderType.ODM:
					case BillingConstants.PriceHeaderType.Other:
						return true;
					default:
						return false;
				}
			}
		}

		public override bool ReadOnly
		{
			get
			{
				if (IsInDatabase && !L6_SystemCodeInfo.HasChanges && !L6_ValidFromInfo.HasChanges && IsCargoWiseNextAndPastValidFrom)
				{
					return true;
				}

				return base.ReadOnly;
			}
			set => base.ReadOnly = value;
		}

		//14 hours added to UTC time as this is the most advanced timezone in the world
		public bool IsCargoWiseNextAndPastValidFrom
		{
			get
			{
				if (isCargoWiseNextAndPastValidFromCached)
				{
					return true;
				}

				if (IsCargoWiseNext && ZDateTime.UtcNow.AddHours(14) >= L6_ValidFrom)
				{
					isCargoWiseNextAndPastValidFromCached = true;
					return true;
				}

				return false;
			}
		}

		bool isCargoWiseNextAndPastValidFromCached;

		#endregion

		#region Delete

		public override void Delete()
		{
			Items.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Log Changes

		public override void OnSaving()
		{
			base.OnSaving();
			LogChanges();
		}

		void LogChanges()
		{
			bool criticalFieldsHasChanges = IsInDatabase &&
				(L6_RN_NKCountryInfo.HasChanges || L6_RX_NKCurrencyInfo.HasChanges || L6_ValidFromInfo.HasChanges || L6_ValidToInfo.HasChanges || L6_LicenceEditionInfo.HasChanges);

			if (criticalFieldsHasChanges && LicCompany != null && LicCompany.Header != null)
			{
				ZStringBuilder builder = new ZStringBuilder("PriceList");
				if (L6_RN_NKCountryInfo.HasChanges)
				{
					builder.Append(string.Concat(" | Country:", (ZString)L6_RN_NKCountryInfo.OriginalValue, "=>", L6_RN_NKCountry));
				}

				if (L6_RX_NKCurrencyInfo.HasChanges)
				{
					builder.Append(string.Concat(" | Currency:", (ZString)L6_RX_NKCurrencyInfo.OriginalValue, "=>", L6_RX_NKCurrency));
				}

				if (L6_ValidFromInfo.HasChanges)
				{
					builder.Append(string.Concat(" | ValidFrom:", ((ZDateTime)L6_ValidFromInfo.OriginalValue).ToShortDateString(), "=>", L6_ValidFrom.ToShortDateString()));
				}

				if (L6_ValidToInfo.HasChanges)
				{
					builder.Append(string.Concat(" | ValidTo:", ((ZDateTime)L6_ValidToInfo.OriginalValue).ToShortDateString(), "=>", L6_ValidTo.ToShortDateString()));
				}

				if (L6_LicenceEditionInfo.HasChanges)
				{
					builder.Append(string.Concat(" | Edition:", (ZString)L6_LicenceEditionInfo.OriginalValue, "=>", L6_LicenceEdition));
				}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				LicCompany.Header.Logs.AddNew(Events.EditedARecord, builder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (HasChanges && IsCargoWiseNext)
			{
				if (IsCargoWiseNextAndPastValidFrom)
				{
					AddRowError(CargoWiseNextPastValidFromEditErrorMessage);
				}
			}
		}

		public bool IsNextCargoWiseNextPriceList
		{
			get
			{
				var pastValidFromDate = ZDateTime.UtcNow.AddHours(14);
				var nextCWNextPriceLists = LicCompany.PriceHeaders.Where(x => x.IsCargoWiseNext && !IsCargoWiseNextAndPastValidFrom).OrderBy(x => x.L6_ValidFrom).FirstOrDefault();
				return PK == nextCWNextPriceLists.PK;
			}
		}

		public const string CargoWiseNextPastValidFromEditErrorMessage = "Price list cannot be edited past its Valid From date.";
	}
}

