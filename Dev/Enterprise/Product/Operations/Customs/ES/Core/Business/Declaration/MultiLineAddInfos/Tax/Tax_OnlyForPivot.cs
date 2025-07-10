using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.ES.Business.MasterFiles;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
	public class Tax_OnlyForPivot : Tax_CusAddInfoOnlyForPIVOT
	{
		public Tax_OnlyForPivot(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

		protected override EUAddInfoTaxLookups GetNewLookups() => new ESAddInfoTaxLookups(this);

		public new ESAddInfoTaxLookups Lookups => (ESAddInfoTaxLookups)base.Lookups;
	}
}
