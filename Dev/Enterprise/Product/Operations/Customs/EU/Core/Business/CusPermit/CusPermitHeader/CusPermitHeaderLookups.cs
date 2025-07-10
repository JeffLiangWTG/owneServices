using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusPermitHeaderLookups : Customs.Business.BaseCusPermitHeaderLookups
	{
		public CusPermitHeaderLookups(CusPermitHeader parent)
			: base(parent)
		{
		}

		public new CusPermitHeader Parent => (CusPermitHeader)base.Parent;

		public virtual ZZRefCusCodeListCombinedCollection PermitFullTypes => Parent.CountrySpecificInstruction.GetFullTypeList(Parent.GetDefaultDataGroupingCode());

		public virtual CodeDescriptionPairList UnitOfQuantityList => TaxLookupsCommon.GetUnitOfQuantityList(Parent.Factory, Parent.GetDefaultDataGroupingCode());
	}
}

