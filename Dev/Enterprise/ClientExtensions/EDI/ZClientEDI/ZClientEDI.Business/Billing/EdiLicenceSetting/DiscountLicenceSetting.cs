using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DiscountLicenceSetting : EdiLicenceSetting
	{
		public DiscountLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.Discount;
		}

		[List("Lookups.DiscountNames")]
		public override ZString LS9_Name
		{
			get { return base.LS9_Name; }
			set
			{
				base.LS9_Name = value;
				if (!IsManualOverrideApplicable)
				{
					LS9_IsManualOverride = false;
				}

				if (!IsLS9_PercentApplicable)
				{
					LS9_Percent = 0;
				}

				SummaryInfo.RefreshBinding();
			}
		}

		public override ZString NameDesc
		{
			get
			{
				return Lookups.DiscountNames.GetDescriptionFromCode(LS9_Name);
			}
		}

		public ZBool IsManualOverrideApplicable
		{
			get
			{
				var prepayDiscount = Database?.PriceHeaderLinkForDate(ZDateTime.Today)?.PriceHeader?.StlDiscounts.FirstOrDefault(d => d.PHD_Type == BillingConstants.DiscountCalculator.Prepayment);
				var prepayDiscountName = prepayDiscount?.PHD_Name ?? ZString.Empty;
				return !prepayDiscountName.IsEmpty && LS9_Name.EqualsIgnoringCase(prepayDiscountName);
			}
		}

		protected override EdiLicenceSettingValidation GetNewValidation()
		{
			return new DiscountLicenceSettingValidation(this);
		}

		public bool LS9_Percent_ReadOnly => !IsLS9_PercentApplicable;

		bool IsLS9_PercentApplicable
		{
			get
			{
				var result = true;

				var discounts = Database?.PriceHeaderLinkForDate(ZDateTime.Today)?.PriceHeader?.StlDiscounts?.Where(x => x.PHD_Name == LS9_Name);

				if (discounts != null && discounts.Any())
				{
					result = BillingConstants.DiscountCalculator.IsCustomerPercentageSettingApplicable(discounts.First().PHD_Type);
				}

				return result;
			}
		}
	}

	public class DiscountLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public DiscountLicenceSettingValidation(DiscountLicenceSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_Name()
		{
			MandatoryValidation.CheckEntered(Parent.LS9_NameInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LS9_NameInfo);
		}
	}
}

