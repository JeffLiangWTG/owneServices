using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CNOrgSupplierBuyerLinkAddInfoLookups : ZLookups
	{
		public CNOrgSupplierBuyerLinkAddInfoLookups(CNOrgSupplierBuyerLinkAddInfo parent) : base(parent)
		{
		}

		protected new CNOrgSupplierBuyerLinkAddInfo Parent => base.Parent as CNOrgSupplierBuyerLinkAddInfo;

		public CodeDescriptionPairList ProcedureCodeList
		{
			get
			{
				var countryCode = GlbCompany.CurrentCompany.Country.Code;
				var shipmentType = Parent.ImporterCountry == Core.Constants.CountryCodes.China ? Common.Shared.SharedJobMessageTypeList.Codes.Import
					: Common.Shared.SharedJobMessageTypeList.Codes.Export;
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_RefCusProcedures_ProcedureCode", countryCode, shipmentType, ZDateTime.Today);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();
					var procedures = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, countryCode, shipmentType, ZDateTime.Today);
					foreach (var procedure in procedures)
					{
						result.AddPair(procedure.ZZ6_ProcedureCode, procedure.ZZ6_Description);
					}
					result.Sort();
					return result;
				});
			}
		}

		public CodeDescriptionPairList LevyTypeList => Factory.GetCachedValue<LevyTypeList>();
	}
}
