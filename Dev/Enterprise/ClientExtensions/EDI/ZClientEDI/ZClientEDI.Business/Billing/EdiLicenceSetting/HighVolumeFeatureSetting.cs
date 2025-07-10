using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class HighVolumeFeatureSetting : EdiLicenceSetting
	{
		public HighVolumeFeatureSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.HighVolumeFeature;
		}

		public override ZString Summary => LS9_Name;

		protected override EdiLicenceSettingValidation GetNewValidation() => new HighVolumeFeatureSettingValidation(this);
	}

	public class HighVolumeFeatureSettingValidation : EdiLicenceSettingValidation
	{
		public HighVolumeFeatureSettingValidation(HighVolumeFeatureSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_Name()
		{
			base.CheckLS9_Name();
			MandatoryValidation.CheckEntered(Parent.PriceCategoryInfo, "Price Category");
			MandatoryValidation.CheckEntered(Parent.PriceCodeInfo, "Price Code");
			ListValidation.ErrorIfInvalidCode((NoResString)"Enter a valid Price Category.", Parent.PriceCategoryInfo);
			ListValidation.ErrorIfInvalidCode((NoResString)"Enter a valid Price Code.", Parent.PriceCodeInfo);
		}

		protected override void CheckLS9_Type()
		{
			base.CheckLS9_Type();
			var parent = Parent;

			if (!parent.LS9_TypeInfo.HasErrors() && parent.Database != null)
			{
				if (parent.Database.LicenceSettings.Any(x => x.PK != parent.PK && x.LS9_Type == BillingConstants.LicenceSetting.Price && x.LS9_Name.EqualsIgnoringCase(parent.LS9_Name)
						&& HaveOverlap(parent.LS9_ValidFrom, parent.LS9_ValidTo, x.LS9_ValidFrom, x.LS9_ValidTo)))
				{
					parent.LS9_TypeInfo.AddError("Can't have price setting and high volume setting for the same feature in the same period.");
				}

				if (!parent.Database.PriceHeaderLinks.Any(x => x.PHL_VolumeCode == EdiPriceHeaderLinkVolumeCodeList.Codes.HV
						&& HaveOverlap(parent.LS9_ValidFrom, parent.LS9_ValidTo, x.PHL_ValidFrom, x.PHL_ValidTo)))
				{
					parent.LS9_TypeInfo.AddError("Can't have a High Volume feature without a price list link of type \"HV\" - \"High Volume\" in the same period.");
				}
			}
		}
	}
}


