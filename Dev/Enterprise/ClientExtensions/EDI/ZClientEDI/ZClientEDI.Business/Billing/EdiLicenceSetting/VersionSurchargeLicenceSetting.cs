using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class VersionSurchargeLicenceSetting : EdiLicenceSetting
	{
		public VersionSurchargeLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.VersionSurcharge;
			var reg = EDIDataRegistry.Instance;
			LS9_Percent = reg.VersionSurchargePercent.Value;
			LS9_Price = reg.VersionSurchargeAdditionalPercent.Value;
		}

		/// <summary>
		/// The additional surcharge for every version released after the Non-Current Version.
		/// Uses LS9_Price field to store the value.
		/// </summary>
		[DecimalPlaces(2)]
		public ZDecimal AdditionalPercent
		{
			get => LS9_Price;
			set => LS9_Price = value;
		}

		public ZPropertyInfo AdditionalPercentInfo
			=> GetWrappedZPropertyInfo(nameof(AdditionalPercent), x => LS9_PriceInfo);

		public override ZString Summary => LS9_Percent.ToStringTrimZeros() + "% + " + AdditionalPercent.ToStringTrimZeros() + "% per Version";

		protected override EdiLicenceSettingValidation GetNewValidation() => new VersionSurchargeLicenceSettingValidation(this);
	}

	public class VersionSurchargeLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public VersionSurchargeLicenceSettingValidation(VersionSurchargeLicenceSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_Percent()
		{
			MandatoryValidation.CheckNotNegative(Parent.LS9_PercentInfo);
		}

		protected override void CheckLS9_Price()
		{
			MandatoryValidation.CheckNotNegative(Parent.LS9_PriceInfo);
		}
	}
}


