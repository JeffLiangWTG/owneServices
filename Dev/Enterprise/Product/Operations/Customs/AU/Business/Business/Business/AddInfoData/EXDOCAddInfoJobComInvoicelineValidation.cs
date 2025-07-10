using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class EXDOCAddInfoJobComInvoicelineValidation : AUAddInfoLineValidation
	{
		public EXDOCAddInfoJobComInvoicelineValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_TemporaryImportNumbers_Hidden()
		{
			base.CheckZA_TemporaryImportNumbers_Hidden();
			var invoiceLine = InvoiceLine;
			if (invoiceLine != null && invoiceLine.InvoiceHeader != null)
			{
				if (invoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Dairy)
				{
					if (Parent.ZA_TemporaryImportNumbers_Hidden.Length > 25)
					{
						Parent.ZA_TemporaryImportNumbers_HiddenInfo.AddMessageError(TemporaryImportNumberMaxLengthForDairy);
					}
				}
				else if (Parent.ZA_TemporaryImportNumbers_Hidden.Length > 20 &&
					invoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.GrainsAndPlants &&
					invoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Horticulture)
				{
					Parent.ZA_TemporaryImportNumbers_HiddenInfo.AddMessageError(TemporaryImportNumberMaximumLength);
				}
			}

			ValidateZA_TemporaryImportDate_Hidden();
		}
		internal const string TemporaryImportNumberMaximumLength = "Temporary import number must have a maximum length of 20 characters when produce type is not dairy, horticulture or grains and plants.";
		internal const string TemporaryImportNumberMaxLengthForDairy = "Temporary import number can only have a maximum length of 25 characters when produce type is dairy.";

		protected override void CheckZA_TemporaryImportDate_Hidden()
		{
			base.CheckZA_TemporaryImportDate_Hidden();
			if (!Parent.ZA_TemporaryImportDate_Hidden.IsEmpty)
			{
				if (Parent.ZA_TemporaryImportNumbers_Hidden.IsEmpty)
				{
					Parent.ZA_TemporaryImportDate_HiddenInfo.AddMessageError("Temporary import date requires a temporary import number to exist.");
				}

				if (Parent.ZA_TemporaryImportDate_Hidden > ZDateTime.Now)
				{
					Parent.ZA_TemporaryImportDate_HiddenInfo.AddMessageError("Temporary import date must be less than or equal to todays date.");
				}
			}
		}

		protected override void CheckZA_RelatedExportPermitAuthority_Hidden()
		{
			base.CheckZA_RelatedExportPermitAuthority_Hidden();
			if (!Parent.ZA_RelatedExportPermitDate_Hidden.IsEmpty || !Parent.ZA_RelatedExportPermitNumber_Hidden.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZA_RelatedExportPermitAuthority_HiddenInfo, InvoiceLine.Lookups.EXDOCPermitAuthorityList);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_RelatedExportPermitAuthority_HiddenInfo, InvoiceLine.Lookups.EXDOCPermitAuthorityList);
			}
		}

		protected override void CheckZA_RelatedExportPermitNumber_Hidden()
		{
			base.CheckZA_RelatedExportPermitNumber_Hidden();
			if (Parent.ZA_RelatedExportPermitNumber_Hidden.IsEmpty && (!Parent.ZA_RelatedExportPermitDate_Hidden.IsEmpty || !Parent.ZA_RelatedExportPermitAuthority_Hidden.IsEmpty))
			{
				Parent.ZA_RelatedExportPermitNumber_HiddenInfo.AddMessageError("Related export permit number may not be blank when related export permit date/authority are entered.");
			}
		}

		protected override void CheckZA_RelatedExportPermitDate_Hidden()
		{
			base.CheckZA_RelatedExportPermitDate_Hidden();
			if (Parent.ZA_RelatedExportPermitDate_Hidden.IsEmpty && (!Parent.ZA_RelatedExportPermitAuthority_Hidden.IsEmpty || !Parent.ZA_RelatedExportPermitNumber_Hidden.IsEmpty))
			{
				Parent.ZA_RelatedExportPermitDate_HiddenInfo.AddMessageError("Related export date may not be blank when related export permit number/authority are entered.");
			}
		}

		protected override void CheckZA_AQISDominantProduct_Hidden()
		{
			base.CheckZA_AQISDominantProduct_Hidden();
			if (InvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AQISDominantProduct_HiddenInfo, InvoiceLine.QuarantineExDocLine.Lookups.DominantProducts);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.ZA_AQISDominantProduct_HiddenInfo, "Dominant Product");
			}

			ValidateZA_AQISAdditionalProducts_Hidden();
		}

		protected override void CheckZA_AQISAdditionalProducts_Hidden()
		{
			base.CheckZA_AQISAdditionalProducts_Hidden();

			if (InvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.ZA_AQISAdditionalProducts_HiddenInfo, "Additional Products");
			}
			else if (!InvoiceLine.QuarantineExDocLine.QL_AdditionalProducts.IsEmpty)
			{
				if (InvoiceLine.QuarantineExDocLine.QL_DominantProduct.IsEmpty)
				{
					Parent.ZA_AQISAdditionalProducts_HiddenInfo.AddMessageError(FirstEnterDominentProduct);
				}
				else if (InvoiceLine.QuarantineExDocLine.QL_AdditionalProducts.Contains(" ", StringComparison.Ordinal))
				{
					Parent.ZA_AQISAdditionalProducts_HiddenInfo.AddMessageError(AdditionalProductsHasSpaces);
				}
				else
				{
					ZString[] additionalProducts = InvoiceLine.QuarantineExDocLine.QL_AdditionalProducts.Split(',');
					var dominantProducts = InvoiceLine.QuarantineExDocLine.Lookups.DominantProducts;

					if (additionalProducts.Any(p => !p.IsEmpty && dominantProducts.FindByCode(p) == null))
					{
						Parent.ZA_AQISAdditionalProducts_HiddenInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
					}
				}
			}

			ValidateZA_AQISDominantProduct_Hidden();
		}

		internal const string FirstEnterDominentProduct = "If you enter Additional Products you must first enter the Dominant Product.";
		internal const string AdditionalProductsHasSpaces = "Additional Products list cannot include spaces.";

		protected override void CheckZA_AQISHalal_Hidden()
		{
			base.CheckZA_AQISHalal_Hidden();
			if (InvoiceLine.QuarantineExDocLine.QL_HalalProductIndicator && InvoiceLine.InvoiceHeader.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
			{
				Parent.ZA_AQISHalal_HiddenInfo.AddMessageError("Halal Product Indicator may only be set when produce type is meat.");
			}
		}
	}
}
