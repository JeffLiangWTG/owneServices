using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(LicenceDatabase), "LicenceSettings")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class EdiLicenceSetting : AutoEdiLicenceSetting
	{
		public EdiLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Database

		public LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(LS9_LD); }
		}

		#endregion

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new EdiLicenceSettingTypeDecider();

		#endregion

		#region Type

		[List(nameof(Lookups) + "." + nameof(EdiLicenceSettingLookups.Types))]
		public override ZString LS9_Type
		{
			get { return base.LS9_Type; }
			set
			{
				base.LS9_Type = value;
				TypeDescInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(EdiLicenceSettingLookups.TypeDescriptions))]
		public ZString TypeDesc
		{
			get
			{
				return typeDesc ?? (typeDesc = Lookups.Types.GetDescriptionFromCode(LS9_Type));
			}
			set
			{
				if (typeDesc != value)
				{
					typeDesc = value;

					var code = Lookups.Types.GetCodeFromDescription(value);
					// calling base to avoid double call to TypeDescInfo.RefreshBinding();
					base.LS9_Type = code;

					TypeDescInfo.RefreshBinding();
				}
			}
		}

		string typeDesc;

		public ZPropertyInfo TypeDescInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDesc)); }
		}

		#endregion

		#region Summary

		public virtual ZString Summary
		{
			get
			{
				return NameDesc;
			}
		}

		public ZPropertyInfo SummaryInfo
		{
			get { return GetZPropertyInfo(nameof(Summary)); }
		}

		#endregion

		#region Price

		public override ZDecimal LS9_Price
		{
			get
			{
				return base.LS9_Price;
			}
			set
			{
				base.LS9_Price = value;
				SummaryInfo.RefreshBinding();
			}
		}

		#endregion

		#region Name

		public override ZString LS9_Name
		{
			get { return base.LS9_Name; }
			set
			{
				base.LS9_Name = value;
				SummaryInfo.RefreshBinding();
			}
		}

		public virtual ZString NameDesc => LS9_Name;

		#endregion

		#region Price Category and Code (for derived classes PriceLicenceSetting, HighVolumeFeatureSetting)

		[List(nameof(Lookups) + "." + nameof(EdiLicenceSettingLookups.PriceCategories))]
		[MaxLength(ClientLicencePriceItem.Schema.L7_CategoryMaxLength)]
		public ZString PriceCategory
		{
			get => PriceKey.Category;
			set
			{
				if (PriceCategory != value)
				{
					CheckMaximumLength(PriceCategoryInfo, value);
					PriceKey = new UsageCodeKey(value, PriceKey.Code);
					if (!IsValidationSuspended)
					{
						Validation.ValidateLS9_Name();
					}
				}
			}
		}

		public ZPropertyInfo PriceCategoryInfo
			=> GetWrappedZPropertyInfo(nameof(PriceCategory), x => LS9_NameInfo);

		[List(nameof(Lookups) + "." + nameof(EdiLicenceSettingLookups.PriceCodesForCategory))]
		[MaxLength(ClientLicencePriceItem.Schema.L7_CodeMaxLength)]
		public virtual ZString PriceCode
		{
			get { return PriceKey.Code; }
			set
			{
				if (PriceCode != value)
				{
					CheckMaximumLength(PriceCodeInfo, value);
					PriceKey = new UsageCodeKey(PriceCategory, value);
				}
			}
		}

		public ZPropertyInfo PriceCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PriceCode), x => LS9_NameInfo); }
		}

		public UsageCodeKey PriceKey
		{
			get => UsageCodeKey.FromDatabaseText(LS9_Name);
			set => LS9_Name = value.ToDatabaseText();
		}

		#endregion

		#region Create Time (Local)

		public ZDateTime SystemCreateTimeLocal
		{
			get { return LS9_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}

	public sealed class EdiLicenceSettingTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(EdiLicenceSetting);
		}

		public static Type GetTypeByCode(string typeCode)
		{
			switch (typeCode)
			{
				case BillingConstants.LicenceSetting.BuyingGroup:
					return typeof(BuyingGroupLicenceSetting);
				case BillingConstants.LicenceSetting.Commitment:
					return typeof(CommitmentLicenceSetting);
				case BillingConstants.LicenceSetting.ConversionCredit:
					return typeof(ConversionCreditLicenceSetting);
				case BillingConstants.LicenceSetting.Discount:
					return typeof(DiscountLicenceSetting);
				case BillingConstants.LicenceSetting.Price:
					return typeof(PriceLicenceSetting);
				case BillingConstants.LicenceSetting.PriceTier:
					return typeof(PriceTierLicenceSetting);
				case BillingConstants.LicenceSetting.BorderWisePurchasedLicences:
					return typeof(BorderWisePurchasedLicenceSetting);
				case BillingConstants.LicenceSetting.HighVolumeFeature:
					return typeof(HighVolumeFeatureSetting);
				case BillingConstants.LicenceSetting.VersionSurcharge:
					return typeof(VersionSurchargeLicenceSetting);
				case BillingConstants.LicenceSetting.MinSpend:
					return typeof(MinSpendLicenceSetting);
				case BillingConstants.LicenceSetting.DiscountSuspensionPolicy:
					return typeof(DiscountSuspensionPolicyLicenceSetting);
				case BillingConstants.LicenceSetting.BillingSummaryCurrency:
					return typeof(BillingSummaryCurrencyLicenceSetting);
				default: return typeof(EdiLicenceSetting);
			}
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string typeCode = row[EdiLicenceSettingSchema.Constants.LS9_Type].ToString();
			return GetTypeByCode(typeCode);
		}

		public override Type GetTypeForNew()
		{
			return typeof(EdiLicenceSetting);
		}
	}
}

