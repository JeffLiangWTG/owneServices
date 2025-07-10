using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CusGoodsCatalogCollection : BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>
	{
		public CusGoodsCatalogCollection(CusClassPartPivot pivot) : base(pivot.Factory)
		{
			this.pivot = Argument.NotNull(pivot, nameof(pivot));
		}

		readonly CusClassPartPivot pivot;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var tariffNumber = pivot.TariffNumber;
			var cusGoodsCatalog = child as CusGoodsCatalog;
			cusGoodsCatalog.CGC_Tariff = tariffNumber.IsEmpty ? pivot.Classification?.CC_TariffNum ?? ZString.Empty : tariffNumber;
			cusGoodsCatalog.CGC_Type = GetCatalogType(pivot.CI_ChildType);
			cusGoodsCatalog.CGC_OH_Owner = pivot.CI_OH;

			var partNum = pivot.SupplierPart?.OP_PartNum ?? ZString.Empty;
			if (!partNum.IsEmpty)
			{
				cusGoodsCatalog.LocalPartNumbers.AddLocalPartNumberIfNotExists(partNum);
			}
		}

		string GetCatalogType(string type)
		{
			return type switch
			{
				ClassificationTypeList.Codes.HTE => GoodsCatalogTypeList.Codes.Export,
				ClassificationTypeList.Codes.HTI => GoodsCatalogTypeList.Codes.Import,
				_ => string.Empty,
			};
		}
	}
}
