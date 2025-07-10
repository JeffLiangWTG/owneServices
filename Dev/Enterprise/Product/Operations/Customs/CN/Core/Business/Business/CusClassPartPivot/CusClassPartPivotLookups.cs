using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		public OrgHeaderCollection Manufacturers => new OrgHeaderCollection(Factory);

		public ICodeDescriptionPairList OriginStateList => Factory.GetStateList(Parent.CI_RN_NKCountryOfOrigin, false);

		public ICodeDescriptionPairList TradeUnitQtyList => Factory.GetCachedValue<RefCusPackListProvider>().GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China);

		public UNDGSubstanceCollection UNDGSubs => new UNDGSubstanceCollection(Factory, new ZQuery(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));

		public RefCurrencyCollection TradeUnitPriceCurrencies => new RefCurrencyCollection(Factory);
	}
}
