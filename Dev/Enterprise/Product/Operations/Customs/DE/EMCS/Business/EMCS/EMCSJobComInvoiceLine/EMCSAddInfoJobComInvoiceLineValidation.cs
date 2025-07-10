using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using EMCSAddInfoJobComInvoiceLine = Enterprise.Customs.EU.EMCS.Business.EMCSAddInfoJobComInvoiceLine;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSAddInfoJobComInvoiceLineValidation : EU.EMCS.Business.EMCSAddInfoJobComInvoiceLineValidation
	{
		public EMCSAddInfoJobComInvoiceLineValidation(EMCSAddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckZG_ExciseProductCode()
		{
			base.CheckZG_ExciseProductCode();

			var declaration = (EMCSJobDeclaration)ParentInvoiceLine.Declaration;
			var exciseProductCodeInfo = Parent.ZG_ExciseProductCodeInfo;
			var isWine = ParentInvoiceLine.IsWine;
			if (isWine && IsOrgCountryDE(declaration.Supplier) && IsOrgCountryDE(declaration.Importer))
			{
				exciseProductCodeInfo.AddMessageError(Res.GetString("f2c49b0f-3d16-4adf-be2f-9eb220ebd3e3", "An Excise Product Code of 'W200' is not allowed for domestic EMCS movements."));
			}
			bool IsOrgCountryDE(OrgHeader org) => (org?.CountryCode ?? ZString.Empty) == Core.Constants.CountryCodes.Germany;

			if (declaration.IsConsolidatedDocument() && (isWine || ParentInvoiceLine.ZG_ExciseProductCode.StartsWith("E", StringComparison.Ordinal)))
			{
				exciseProductCodeInfo.AddMessageError(Res.GetString("FAC3564B-E692-4D12-A187-6C401526F8B0", "The Excise Code 'W200' and all 'E'-Codes are invalid for Consolidated Documents."));
			}
		}

		protected override void CheckExciseProductCodeIsMappedForCorrectTariff()
		{
		}
	}
}

