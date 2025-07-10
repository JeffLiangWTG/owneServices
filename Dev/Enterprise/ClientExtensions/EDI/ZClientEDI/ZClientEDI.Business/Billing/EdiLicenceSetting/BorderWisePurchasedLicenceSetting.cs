using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BorderWisePurchasedLicenceSetting : EdiLicenceSetting
	{
		public BorderWisePurchasedLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.BorderWisePurchasedLicences;
		}

		[List("Lookups.BorderWisePriceCodes")]
		[MaxLength(4)]
		public override ZString PriceCode
		{
			get { return LS9_Name; }
			set
			{
				if (LS9_Name != value)
				{
					CheckMaximumLength(PriceCodeInfo, value);
					LS9_Name = value;
				}
			}
		}

		public override ZString Summary
		{
			get
			{
				var text = LicenceCount.ToString();
				if (!LS9_Name.IsEmpty)
				{
					text += " x " + PriceCode + " - " + BillingConstants.BorderWise.GetCachedBorderWiseModuleAndGroupList(Factory).GetDescriptionFromCode(PriceCode);
				}
				return text;
			}
		}

		public ZInt LicenceCount
		{
			get { return (int)LS9_Price; }
			set { LS9_Price = (int)value;  }
		}

		public ZPropertyInfo LicenceCountInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LicenceCount), x => LS9_PriceInfo); }
		}

		protected override EdiLicenceSettingValidation GetNewValidation()
		{
			return new BorderWisePurchasedLicenceSettingValidation(this);
		}
	}

	public class BorderWisePurchasedLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public BorderWisePurchasedLicenceSettingValidation(BorderWisePurchasedLicenceSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_Price()
		{
			base.CheckLS9_Price();
			MandatoryValidation.CheckEntered(Parent.LS9_PriceInfo, "Purchased Licences");
			MandatoryValidation.CheckNotZero(Parent.LS9_PriceInfo, "Purchased Licences");
			MandatoryValidation.CheckNotNegative(Parent.LS9_PriceInfo, "Purchased Licences");
		}

		protected override void CheckLS9_Name()
		{
			base.CheckLS9_Name();
			if (!Parent.IsInDatabase || Parent.PriceCodeInfo.HasChanges)
			{
				MandatoryValidation.CheckEntered(Parent.PriceCodeInfo, "Price Code");
				ListValidation.ErrorIfInvalidCode(Parent.PriceCodeInfo);
			}
		}
	}
}

