using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class MinSpendLicenceSetting : EdiLicenceSetting
	{
		public MinSpendLicenceSetting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.MinSpend;
		}

		public override ZString Summary
			=> LS9_Name + " " + LS9_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

		protected override EdiLicenceSettingValidation GetNewValidation()
			=> new MinSpendLicenceSettingValidation(this);
	}

	public class MinSpendLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public MinSpendLicenceSettingValidation(MinSpendLicenceSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_Name()
		{
			base.CheckLS9_Name();
			MandatoryValidation.CheckEntered(Parent.PriceCategoryInfo, "Price Category");
			MandatoryValidation.CheckEntered(Parent.PriceCodeInfo, "Price Code");
			ListValidation.ErrorIfInvalidCode((NoResString)"Enter a valid Price Category", Parent.PriceCategoryInfo);
			ListValidation.ErrorIfInvalidCode((NoResString)"Enter a valid Price Code", Parent.PriceCodeInfo);
		}

		protected override void CheckLS9_Price()
		{
			MandatoryValidation.CheckNotNegative(Parent.LS9_PriceInfo);
			MandatoryValidation.CheckNotZero(Parent.LS9_PriceInfo);
		}
	}
}
