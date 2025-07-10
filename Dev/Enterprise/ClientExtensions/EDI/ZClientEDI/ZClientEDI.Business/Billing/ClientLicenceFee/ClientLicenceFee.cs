using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(LicenceCompany), "Fees")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientLicenceFee : AutoClientLicenceFee
	{
		public ClientLicenceFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(L8_LD); }
		}

		public LicenceCompany Company
		{
			get { return Factory.Load<LicenceCompany>(L8_LC); }
		}

		[RelatedBusinessObject("Company")]
		public override ZGuid L8_LC
		{
			get { return base.L8_LC; }
			set { base.L8_LC = value; }
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicenceFeeLookups.Databases))]
		[RelatedBusinessObject("Database")]
		public override ZGuid L8_LD
		{
			get { return base.L8_LD; }
			set { base.L8_LD = value; }
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicenceFeeLookups.FeeTypes))]
		public override ZString L8_Type
		{
			get { return base.L8_Type; }
			set
			{
				base.L8_Type = value;

				if (!L8_Type.IsEmpty)
				{
					var feeType = (CodeDescriptionBool)EDIDataRegistry.Instance.LicenceFeeTypes.Value.FindByCode(L8_Type);
					if (feeType != null)
					{
						L8_IsDiscountable = !feeType.Bool;
					}
				}
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal L8_Amount
		{
			get { return base.L8_Amount; }
			set
			{
				base.L8_Amount = value;
			}
		}

		public ZInt AmountDecimals
			=> Currency?.Decimals ?? 2;

		[RelatedBusinessObject("Currency")]
		[List(nameof(Lookups) + "." + nameof(ClientLicenceFeeLookups.Currencies))]
		public override ZString L8_RX_NKCurrency
		{
			get => base.L8_RX_NKCurrency;
			set
			{
				if (base.L8_RX_NKCurrency != value)
				{
					var amount = L8_Amount;
					var oldDecimals = amount == 0 ? 0 : (int)AmountDecimals;
					base.L8_RX_NKCurrency = value;
					var newDecimals = amount == 0 ? 0 : (int)AmountDecimals;
					if (newDecimals < oldDecimals)
					{
						L8_Amount = Utilities.Round(L8_Amount, newDecimals);
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicenceFeeLookups.SystemCodes))]
		public override ZString L8_SystemCode
		{
			get { return base.L8_SystemCode; }
			set { base.L8_SystemCode = value; }
		}

		public ZString DiscountChargeCode
		{
			get
			{
				return EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes.Value.GetDescriptionFromCode(L8_ChargeCode);
			}
		}

		public ZPropertyInfo DiscountChargeCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DiscountChargeCode), x => L8_ChargeCodeInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ClientLicenceFeeLookups.TaxDateCodes))]
		public override ZString L8_TaxDateCode { get => base.L8_TaxDateCode; set => base.L8_TaxDateCode = value; }

		/// <summary>
		/// Calculate Tax Date based on L8_TaxDateCode.
		/// For an fee with renewal greater than one month, assumes the bill period coincides with the start of a fee period.
		/// </summary>
		/// <param name="billPeriod">monthly period being billed</param>
		/// <returns>Empty when the invoice date should be used, and a date based on the fee and bill period otherwise</returns>
		public ZDateTime CalculateTaxDate(ZDateTime billPeriod)
		{
			if (billPeriod.IsEmpty)
			{
				return ZDateTime.Empty;
			}
			var taxDateCode = L8_TaxDateCode;
			ZDateTime result;
			switch (taxDateCode)
			{
				case BillingConstants.Fee.TaxDateCode.FeeTaxAtStartDate:
					result = billPeriod;
					break;
				case BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate:
					result = billPeriod.AddMonths(L8_RenewalMonths).AddDays(-1);
					break;
				default:
					result = ZDateTime.Empty;
					break;
			}
			return result;
		}

		#endregion

		#region IsDateRangeMatched

		public bool IsDateRangeMatched(ZDateTime billingDate)
		{
			bool result = false;

			if (((L8_StartDate.IsEmpty && L8_RenewalMonths == 1) || billingDate >= L8_StartDate)
				&& (L8_EndDate.IsEmpty || billingDate <= L8_EndDate))
			{
				if (L8_RenewalMonths > 1)
				{
					int monthDiff = (billingDate.Year - L8_StartDate.Year) * 12 + billingDate.Month - L8_StartDate.Month;
					result = (monthDiff % L8_RenewalMonths) == 0;
				}
				else
				{
					result = true;
				}
			}

			return result;
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
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
				(L8_TypeInfo.HasChanges || L8_DescriptionInfo.HasChanges || L8_AmountInfo.HasChanges || L8_ChargeCodeInfo.HasChanges || L8_StartDateInfo.HasChanges || L8_EndDateInfo.HasChanges);

			if (criticalFieldsHasChanges && Company != null && Company.Header != null)
			{
				ZStringBuilder builder = new ZStringBuilder("Fee");
				AppendFieldInformation(builder, "Typ", (ZString)L8_TypeInfo.OriginalValue, L8_Type);
				AppendFieldInformation(builder, "Amt", ((ZDecimal)L8_AmountInfo.OriginalValue).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), L8_Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
				AppendFieldInformation(builder, "Chrg", (ZString)L8_ChargeCodeInfo.OriginalValue, L8_ChargeCode);
				AppendFieldInformation(builder, "Sta", ((ZDateTime)L8_StartDateInfo.OriginalValue).ToShortDateString(), L8_StartDate.ToShortDateString());
				AppendFieldInformation(builder, "End", ((ZDateTime)L8_EndDateInfo.OriginalValue).ToShortDateString(), L8_EndDate.ToShortDateString());
				AppendFieldInformation(builder, "Desc", (ZString)L8_DescriptionInfo.OriginalValue, L8_Description);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Company.Header.Logs.AddNew(Events.EditedARecord, new ZString(builder.ToString()).SubstringSafe(0, StmALog.Schema.SL_ReferenceMaxLength));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void AppendFieldInformation(ZStringBuilder builder, ZString fieldName, ZString originalValue, ZString newValue)
		{
			bool fieldHasChanges = newValue != originalValue;
			if (!originalValue.IsEmpty || fieldHasChanges)
			{
				builder.Append(string.Format(CultureInfo.CurrentCulture, "|{0}:{1}", fieldName, originalValue));
				if (fieldHasChanges)
				{
					builder.Append(string.Format(CultureInfo.CurrentCulture, "=>{0}", newValue));
				}
			}
		}
		#endregion

		#region TranslatableDataField

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.L8_Description, Schema.L8_DescriptionMaxLength, Schema.L8_Description, Type = typeof(ClientLicenceFee), Asmid = ResString.AssemblyId)]
		public override ZString L8_Description { get => base.L8_Description; set => base.L8_Description = value; }

		public MultilingualString L8_DescriptionMultilingual => GetMultilingual(L8_DescriptionInfo);

		#endregion
	}
}

