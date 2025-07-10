using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceLicenceSetting : EdiLicenceSetting
	{
		public PriceLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.Price;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			enableUnits = LS9_Units >= 0;
		}

		public override ZString Summary
		{
			get
			{
				return LS9_Name + " " + LS9_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}
		}

		public ZBool EnableUnits
		{
			get
			{
				return enableUnits;
			}
			set
			{
				SetNonPersistentPropertyValue(EnableUnitsInfo, ref enableUnits, value);
				if (!enableUnits)
				{
					LS9_Units = -1;
				}
				else if (LS9_Units < 0)
				{
					LS9_Units = 0;
				}
			}
		}
		ZBool enableUnits = true;

		public virtual ZPropertyInfo EnableUnitsInfo => GetZPropertyInfo(nameof(EnableUnits));

		public bool LS9_Units_ReadOnly => !EnableUnits;
		public bool UnitsForBinding_ReadOnly => !EnableUnits;

		public ZDecimal UnitsForBinding
		{
			get
			{
				if (LS9_Units < 0)
				{
					return 0;
				}
				else
				{
					return LS9_Units;
				}
			}
			set
			{
				LS9_Units = value;
			}
		}

		public ZPropertyInfo UnitsForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(UnitsForBinding), x => LS9_UnitsInfo); }
		}

		protected override EdiLicenceSettingValidation GetNewValidation()
		{
			return new PriceLicenceSettingValidation(this);
		}

		public virtual bool SupportUnitBreak => false;

		public virtual (ZDecimal? Price, ZDecimal? Units) GetPriceAndUnits()
			=> (LS9_Price, LS9_Units);

		public virtual (ZDecimal? Price, ZDecimal? Units) GetPriceAndUnits(ZInt unitBreak)
			=> throw new NotImplementedException();
	}

	public class PriceLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public PriceLicenceSettingValidation(PriceLicenceSetting parent)
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

			if (Parent.LS9_Type == BillingConstants.LicenceSetting.Price && Parent.Lookups.PriceTierCodes.Contains(Parent.PriceCode.ToString()))
			{
				Parent.PriceCodeInfo.AddWarning("This feature code has multiple tiers. Price setting will apply to all tiers. Use 'Price (Tier)' to setup each tier.");
			}
		}
	}
}

