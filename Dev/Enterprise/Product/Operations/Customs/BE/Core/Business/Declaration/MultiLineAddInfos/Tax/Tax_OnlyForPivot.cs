using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.MasterFiles;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.BE.Business.Declaration;

[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
public class Tax_OnlyForPivot : Tax_CusAddInfoOnlyForPIVOT
{
	public Tax_OnlyForPivot(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
	{
	}

	public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

	protected override EUAddInfoTaxLookups GetNewLookups() => new BEAddInfoTaxLookups(this);

	public new BEAddInfoTaxLookups Lookups => (BEAddInfoTaxLookups)base.Lookups;
}
