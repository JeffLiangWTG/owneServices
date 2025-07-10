using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingSummaryCurrencyLicenceSetting : EdiLicenceSetting
	{
		public BillingSummaryCurrencyLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.BillingSummaryCurrency;
		}

		public override ZString Summary => $"{LS9_RX_NKPriceCurrency}";

		protected override EdiLicenceSettingValidation GetNewValidation() => new BillingSummaryCurrencyLicenceSettingValidation(this);
	}

	public class BillingSummaryCurrencyLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public BillingSummaryCurrencyLicenceSettingValidation(BillingSummaryCurrencyLicenceSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_RX_NKPriceCurrency()
		{
			MandatoryValidation.CheckEntered(Parent.LS9_RX_NKPriceCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LS9_RX_NKPriceCurrencyInfo);
		}
	}
}


