using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(ClientLicencePriceHeader), "Items")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientLicencePriceItem : AutoClientLicencePriceItem
	{
		public ClientLicencePriceItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		[ActionFieldFollow(false)]
		public ClientLicencePriceHeader Parent
		{
			get { return Factory.Load<ClientLicencePriceHeader>(L7_L6); }
		}

		#endregion

		#region Properties

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.L7_Description, Schema.L7_DescriptionMaxLength, Schema.L7_Description, Type = typeof(ClientLicencePriceItem), Asmid = ResString.AssemblyId)]
		public override ZString L7_Description
		{
			get { return base.L7_Description; }
			set
			{
				descriptionIndentLevel = null;
				base.L7_Description = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_CountryTierCode();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public ZString L7_DescriptionLocalized
		{
			get
			{
				ZString result;

				if (L7_DescriptionMultilingualOverride != null)
				{
					result = L7_DescriptionMultilingualOverride;
				}
				else
				{
					var description = L7_Description;
					if (description.StartsWith(" ", StringComparison.OrdinalIgnoreCase))
					{
						result = string.Concat(new string(' ', description.ToString().TakeWhile(x => char.IsWhiteSpace(x)).Count()), GetMultilingual(L7_DescriptionInfo).Trim());
					}
					else
					{
						result = GetMultilingual(L7_DescriptionInfo).Trim();
					}
				}

				return result;
			}
		}

		public void SetPriceItemDescription(MultilingualString value)
		{
			L7_DescriptionMultilingualOverride = value;
		}

		MultilingualString L7_DescriptionMultilingualOverride;

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.L7_ChargeBasis, Schema.L7_ChargeBasisMaxLength, Schema.L7_ChargeBasis, Type = typeof(ClientLicencePriceItem), Asmid = ResString.AssemblyId)]
		public override ZString L7_ChargeBasis { get => base.L7_ChargeBasis; set => base.L7_ChargeBasis = value; }

		public MultilingualString L7_ChargeBasisMultilingual => GetMultilingual(L7_ChargeBasisInfo);

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.PriceCategoryList))]
		[ActionField(ReadOnly = false)]
		public override ZString L7_Category
		{
			get => base.L7_Category;
			set
			{
				base.L7_Category = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_Price();
					Validation.ValidateL7_DisbursementDirection();
					Validation.ValidateL7_RN_NKDisbursementCountry();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.PriceCodeList))]
		public override ZString L7_Code
		{
			get { return base.L7_Code; }
			set
			{
				base.L7_Code = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_Price();
					Validation.ValidateL7_DisbursementDirection();
					Validation.ValidateL7_RN_NKDisbursementCountry();
				}
			}
		}

		public UsageCodeKey CodeKey
		{
			get => new UsageCodeKey(L7_Category, L7_Code);
			set
			{
				L7_Category = value.Category;
				L7_Code = value.Code;
			}
		}

		public ZString CodeAndSubCodeForDisplay
		{
			get { return L7_Ref4.IsEmpty ? L7_Code : new ZString(L7_Code + " [" + L7_Ref4 + ']'); }
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.PriceCategoryList))]
		[ActionField(ReadOnly = false)]
		public override ZString L7_ParentCategory { get => base.L7_ParentCategory; set => base.L7_ParentCategory = value; }

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.ParentCategoryCodeList))]
		[ActionField(ReadOnly = false)]
		public override ZString L7_ParentCode
		{
			get { return base.L7_ParentCode; }
			set
			{
				base.L7_ParentCode = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.CategoryCodeList))]
		[ActionField(ReadOnly = false)]
		public override ZString L7_WebParentCode
		{
			get { return base.L7_WebParentCode; }
			set
			{
				base.L7_WebParentCode = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.CategoryCodeList))]
		[ActionField(ReadOnly = false)]
		public override ZString L7_UnitBreakParentCode
		{
			get { return base.L7_UnitBreakParentCode; }
			set
			{
				base.L7_UnitBreakParentCode = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.FeeTypes))]
		public override ZString L7_FeeType
		{
			get { return base.L7_FeeType; }
			set
			{
				base.L7_FeeType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_CountryTierCode();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.SubCodes))]
		[ActionField(ReadOnly = true)]
		public override ZString L7_Ref4
		{
			get { return base.L7_Ref4; }
			set { base.L7_Ref4 = value; }
		}

		[ActionField(ReadOnly = true)]
		public override ZShort L7_Order
		{
			get { return base.L7_Order; }
			set { base.L7_Order = value; }
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.Languages))]
		public override ZString L7_Language
		{
			get { return base.L7_Language; }
			set { base.L7_Language = value; }
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.ExchangeRateGroupCodes))]
		public override ZString L7_ExchangeRateGroupCode { get => base.L7_ExchangeRateGroupCode; set => base.L7_ExchangeRateGroupCode = value; }

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.ProductAvailabilityPairList))]
		public override ZString L7_ProductAvailability { get => base.L7_ProductAvailability; set => base.L7_ProductAvailability = value; }

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.ProductDisplayCategories))]
		public override ZString L7_ProductDisplayCategory { get => base.L7_ProductDisplayCategory; set => base.L7_ProductDisplayCategory = value; }

		[MaxLength(256)]
		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.ProductDisplayCategories))]
		public ZString ProductDisplayCategoryDescription
		{
			get => Lookups.ProductDisplayCategories.GetDescriptionFromCode(L7_ProductDisplayCategory) ?? "";
			set
			{
				L7_ProductDisplayCategory = Lookups.ProductDisplayCategories.GetCodeFromDescription(value) ?? "";
				RefreshBinding();
			}
		}

		public ZPropertyInfo ProductDisplayCategoryDescriptionInfo => GetWrappedZPropertyInfo(nameof(ProductDisplayCategoryDescription), x => L7_ProductDisplayCategoryInfo);

		protected override ZString HumanReadableNameCore => Res.GetString("C47FF940-23FC-46B4-AA93-A6B14ABEB280", "Price Item");

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.CountryTierCodeList))]
		public override ZString L7_CountryTierCode
		{
			get => base.L7_CountryTierCode;
			set
			{
				base.L7_CountryTierCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_Code();
					Validation.ValidateL7_UnitBreak();
				}
			}
		}

		public override ZDecimal L7_Price
		{
			get { return base.L7_Price; }
			set
			{
				base.L7_Price = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_CountryTierCode();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.DisbursementDirectionList))]
		public override ZString L7_DisbursementDirection
		{
			get => base.L7_DisbursementDirection;
			set
			{
				base.L7_DisbursementDirection = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_RN_NKDisbursementCountry();
				}
			}
		}

		public override ZString L7_RN_NKDisbursementCountry
		{
			get => base.L7_RN_NKDisbursementCountry;
			set
			{
				base.L7_RN_NKDisbursementCountry = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateL7_DisbursementDirection();
				}
			}
		}

		#endregion

		#region Rates

		[ChildEditable]
		public EdiPriceItemRateCollection CurrencyRates
		{
			get
			{
				if (currencyRates == null)
				{
					currencyRates = new EdiPriceItemRateCollection(this);
					RegisterEditableChildObject(currencyRates);
					RefreshReadOnlyForChildItems();
				}

				return currencyRates;
			}
		}
		EdiPriceItemRateCollection currencyRates;

		#endregion

		#region Discount

		[List(nameof(Lookups) + "." + nameof(ClientLicencePriceItemLookups.DiscountGroupCodes))]
		[ActionField(FieldType = ActionFieldType.Text)]
		public override ZString L7_PGM_DiscountGroupCode
		{
			get { return base.L7_PGM_DiscountGroupCode; }
			set { base.L7_PGM_DiscountGroupCode = value; }
		}

		#endregion

		#region Calculated Properties

		public ZString CodeDescription
		{
			get { return Lookups.PriceCodeList.GetDescriptionFromCode(L7_Code); }
		}

		public ZString ParentCodeDescription
		{
			get { return Lookups.ParentCategoryCodeList.GetDescriptionFromCode(L7_ParentCode); }
		}

		public ZString OrganisationCode
		{
			get { return Parent.LicCompany.Header.OH_Code; }
		}

		public ZString L7_FeeTypeDesc
		{
			get { return Lookups.FeeTypes.GetDescriptionFromCode(L7_FeeType); }
		}

		public MultilingualString L7_FeeTypeDescMultilingual
		{
			get { return Lookups.FeeTypes.GetMultilingualDescriptionFromCode(L7_FeeType); }
		}

		public bool IsTransactional
		{
			get { return L7_FeeType == BillingConstants.FeeType.Transactional; }
		}

		public bool IsTransactionalOneVolumeBreak
		{
			get { return L7_FeeType == BillingConstants.FeeType.TransactionalOneVolumeBreak; }
		}

		public bool IsPerPage
		{
			get { return L7_FeeType == BillingConstants.FeeType.PerPage; }
		}

		public int DescriptionIndentLevel
		{
			get { return descriptionIndentLevel ?? (descriptionIndentLevel = CalculatedDescriptionIndentLevel).Value; }
		}
		int? descriptionIndentLevel;

		int CalculatedDescriptionIndentLevel
		{
			get
			{
				var description = L7_DescriptionLocalized.ToString();
				for (int i = 0; i < description.Length; ++i)
				{
					if (description[i] != ' ')
					{
						return i;
					}
				}

				return description.Length;
			}
		}

		public bool HasLicenceUnits
		{
			get { return L7_LicenceUnits > 0; }
		}

		public string PriceDescriptionForUniquePriceCodeLookup
		{
			get
			{
				var descriptionItem = this;

				//get the parent item's description if the price is one of the prices which have different break unit.
				if (!L7_Code.IsEmpty && Parent.Items.Any(x => x.PK != PK && x.L7_Code == L7_Code && x.L7_Category == L7_Category && x.L7_UnitBreak != L7_UnitBreak ))
				{
					descriptionItem = Parent.Items.Where(x => x.L7_Code.IsEmpty && x.L7_UnitBreak.IsEmpty && x.L7_Order < L7_Order)
								.OrderByDescending(x => x.L7_Order).FirstOrDefault() ?? this;
				}
				return descriptionItem.L7_DescriptionLocalized.Trim();
			}
		}

		#endregion

		#region IsOnDemandFeeType

		public bool IsOnDemandFeeType
		{
			get { return BillingConstants.FeeType.IsOnDemand(L7_FeeType); }
		}

		#endregion

		#region Maintenance

		public bool IsMaintenanceFeeType
		{
			get { return BillingConstants.FeeType.IsMaintenance(L7_FeeType); }
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		protected bool L7_ExchangeRateGroupCode_ReadOnly => !(Parent?.L6_HasExchangeRates ?? false);

		protected bool L7_IsVolumeAdjustmentEligible_ReadOnly => !(Parent?.L6_HasExchangeRates ?? false);

		public void RefreshReadOnlyForChildItems()
		{
			var parent = Parent;

			var currencyRatesReadOnly = ReadOnly
				|| !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed
				|| (parent != null && !BillingConstants.PriceHeaderType.CanHaveMultipleCurrencies(parent.L6_SystemCode))
				|| (parent != null && parent.L6_HasExchangeRates);

			CurrencyRates.SetReadOnlyIncludingChildren(currencyRatesReadOnly);
			CurrencyRates.RefreshBinding();
		}

		#endregion

		#region Log Changes

		public override void OnSaving()
		{
			base.OnSaving();
			LogChanges();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void LogChanges()
		{
			bool criticalFieldsHasChanges = IsInDatabase &&
				(L7_CodeInfo.HasChanges || L7_ParentCodeInfo.HasChanges || L7_FeeTypeInfo.HasChanges || L7_PriceInfo.HasChanges);

			if (criticalFieldsHasChanges && Parent != null && Parent.LicCompany != null && Parent.LicCompany.Header != null)
			{
				ZStringBuilder builder = new ZStringBuilder(string.Format(CultureInfo.InvariantCulture, "PriceItem[{0}]", L7_Description.Trim().Substring(0, Math.Min(L7_Description.Length, 20))));
				if (L7_CodeInfo.HasChanges)
				{
					builder.Append(string.Concat(" | Code:", (ZString)L7_CodeInfo.OriginalValue, "=>", L7_Code));
				}

				if (L7_ParentCodeInfo.HasChanges)
				{
					builder.Append(string.Concat(" | Parent:", (ZString)L7_ParentCodeInfo.OriginalValue, "=>", L7_ParentCode));
				}

				if (L7_FeeTypeInfo.HasChanges)
				{
					builder.Append(string.Concat(" | FeeType:", (ZString)L7_FeeTypeInfo.OriginalValue, "=>", L7_FeeType));
				}

				if (L7_PriceInfo.HasChanges)
				{
					builder.Append(string.Concat(" | Price:", (ZDecimal)L7_PriceInfo.OriginalValue, "=>", L7_Price));
				}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Parent.LicCompany.Header.Logs.AddNew(Events.EditedARecord, builder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			CurrencyRates.DeleteAll();
			base.Delete();
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			L7_DisbursementDirection = "";
			L7_RN_NKDisbursementCountry = "";
		}

#endif

		public ClientLicencePriceItem CreateNewCopy(ZString newPriceCode, ZString newDescription)
		{
			var newObj = Factory.New<ClientLicencePriceItem>();
			newObj.CopyPersistentValuesFrom(this, new BusinessObjectCloneArgs(Enumerable.Empty<string>(), true));
			newObj.L7_Code = newPriceCode;
			newObj.L7_Description = newDescription;
			newObj.L7_Order++;

			foreach (var rate in this.CurrencyRates)
			{
				var newRate = newObj.CurrencyRates.AddNew();
				newRate.CopyPersistentValuesFrom(rate, new BusinessObjectCloneArgs(new[] { EdiPriceItemRateSchema.Constants.PIR_L7 }, true));
			}

			return newObj;
		}
	}
}

