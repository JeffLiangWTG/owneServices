using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.IE.Business.MasterFiles;

namespace Enterprise.Customs.IE.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
	public class Tax_OnlyForPivot : Tax_CusAddInfoOnlyForPIVOT
	{
		public Tax_OnlyForPivot(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

		protected override EUAddInfoTaxLookups GetNewLookups() => new AddInfoTaxLookups(this);

		public new AddInfoTaxLookups Lookups => (AddInfoTaxLookups)base.Lookups;
	}
}
