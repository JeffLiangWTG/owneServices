using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AsycudaBillLookups : EU.H7.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ImporterIdentificationTypeList => Factory.GetCachedValue<ESH7ImporterIdentificationTypes>();

		public CodeDescriptionPairList DocumentationRequired => Factory.GetCachedValue<ESH7DocumentationRequiredList>();

		public override CodeDescriptionPairList AdditionalProcedureList => Factory.GetCachedValue<ESH7AdditionalProcedureCodeList>();

		public override CodeDescriptionPairList ConsigneeState_List => Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Spain
			? CustomsFiscalTerritoriesList.GetSpainFullList(Parent.Factory) : GetState_List(Parent.ABL_RN_NKConsigneeCountryInfo);

		public override CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<ESH7AISEntryStatusList>();

		public override CodeDescriptionPairList ShipperState_List => Parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Spain
			? CustomsFiscalTerritoriesList.GetSpainFullList(Parent.Factory) : GetState_List(Parent.ABL_RN_NKShipperCountryInfo);

		public override CodeDescriptionPairList SellerState_List => Parent.ABL_RN_NKSellerCountry == Core.Constants.CountryCodes.Spain
			? CustomsFiscalTerritoriesList.GetSpainFullList(Parent.Factory) : GetState_List(Parent.ABL_RN_NKSellerCountryInfo);

		protected override ICollection CountryList => CusRefTradeGroupCountryView.Loader.GetCachedListTradeGroupCountries(Factory, ZString.Empty, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<LogicalStatusList>();
	}
}
