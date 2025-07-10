using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent) : base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		public override IBaseCusGoodsCatalogCollection<BaseCusGoodsCatalog> GoodsCatalogList
		{
			get
			{
				var result = new CusGoodsCatalogCollection(Parent);

				if (!Parent.IsHTB)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Type", "Property", new ZString(Parent.IsImportClassification ? GoodsCatalogTypeList.Codes.Import : GoodsCatalogTypeList.Codes.Export), false));
				}
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff", "Property", new ZString(Parent.CI_TariffNum), false));
				return result;
			}
		}
	}
}
