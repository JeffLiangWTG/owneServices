using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportLicenseJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaLists => new List<IZZRateSelectionCriteria>() { };

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();

			if (Parent.UsedMaterialRegimeIsNationalization)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_BrandNameInfo);
			}
		}

		protected override void CheckJI_Model()
		{
			base.CheckJI_Model();

			if (Parent.UsedMaterialRegimeIsNationalization)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ModelInfo);
			}
		}

		protected override void CheckJI_SecondaryPreference()
		{
			base.CheckJI_SecondaryPreference();

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_SecondaryPreferenceInfo);
		}

		protected override void CheckFullGoodsDescription()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.FullGoodsDescriptionInfo);
		}

		protected override void CheckJI_ManufacturerIndicator()
		{
			base.CheckJI_ManufacturerIndicator();
			if (!Parent.JI_ManufacturerIndicatorReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ManufacturerIndicatorInfo);
			}
		}

		protected override void CheckJI_UsedMaterialRegime()
		{
			base.CheckJI_UsedMaterialRegime();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_UsedMaterialRegimeInfo);
		}

		protected override void CheckJI_UsedMaterialOperationType()
		{
			base.CheckJI_UsedMaterialOperationType();

			if (Parent.UsedMaterialRegimeIsNationalization)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_UsedMaterialOperationTypeInfo);
			}
		}

		protected override void CheckJI_UsedMaterialSerialNumber()
		{
			base.CheckJI_UsedMaterialSerialNumber();

			if (Parent.UsedMaterialRegimeIsNationalization)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_UsedMaterialSerialNumberInfo);
			}
		}

		protected override void CheckJI_UsedMaterialManufactureYear()
		{
			base.CheckJI_UsedMaterialManufactureYear();

			if (Parent.UsedMaterialRegimeIsNationalization)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_UsedMaterialManufactureYearInfo);

				if (!Parent.JI_UsedMaterialManufactureYear.IsEmpty && !Regex.IsMatch(Parent.JI_UsedMaterialManufactureYear, "^[1-9][0-9]{3}$"))
				{
					Parent.JI_UsedMaterialManufactureYearInfo.AddError(Res.GetString("9BBAC0E5-8189-4137-AC0A-2A9FC0FB9BE3", "Incorrect format (YYYY)."));
				}
			}
		}
	}
}
