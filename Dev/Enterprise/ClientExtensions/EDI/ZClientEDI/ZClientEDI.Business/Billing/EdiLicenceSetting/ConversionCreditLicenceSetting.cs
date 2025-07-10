using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ConversionCreditLicenceSetting : EdiLicenceSetting
	{
		public ConversionCreditLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.ConversionCredit;
		}

		#region CreditChargeCode (Alias for LS9_Name)

		[List("Lookups.DepositChargeCodes")]
		[MaxLength(AccChargeCode.Schema.AC_CodeMaxLength)]
		public ZString CreditChargeCode
		{
			get { return LS9_Name; }
			set
			{
				if (LS9_Name != value)
				{
					CheckMaximumLength(CreditChargeCodeInfo, value);
					LS9_Name = value;
				}
			}
		}

		public ZPropertyInfo CreditChargeCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CreditChargeCode), x => LS9_NameInfo); }
		}

		#endregion

		public override ZString Summary
		{
			get
			{
				return LS9_Name + " " + LS9_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}
		}

		protected override EdiLicenceSettingValidation GetNewValidation()
		{
			return new ConversionCreditLicenceSettingValidation(this);
		}
	}

	public class ConversionCreditLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public ConversionCreditLicenceSettingValidation(ConversionCreditLicenceSetting parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ConversionCreditLicenceSetting parent;

		protected override void CheckLS9_Name()
		{
			base.CheckLS9_Name();
			MandatoryValidation.CheckEntered(parent.CreditChargeCodeInfo, "Charge Code");
			ListValidation.ErrorIfInvalidCode(parent.CreditChargeCodeInfo);
		}

		protected override void CheckLS9_Price()
		{
			MandatoryValidation.CheckNotNegative(parent.LS9_PriceInfo);
			MandatoryValidation.CheckNotZero(parent.LS9_PriceInfo);
		}

		protected override void CheckLS9_RX_NKPriceCurrency()
		{
			MandatoryValidation.CheckEntered(parent.LS9_RX_NKPriceCurrencyInfo);
		}

		protected override void CheckLS9_GE_Department1()
		{
			MandatoryValidation.CheckEntered(parent.LS9_GE_Department1Info);
		}

		protected override void CheckLS9_GE_Department2()
		{
			MandatoryValidation.CheckEntered(parent.LS9_GE_Department2Info);
		}
	}
}

