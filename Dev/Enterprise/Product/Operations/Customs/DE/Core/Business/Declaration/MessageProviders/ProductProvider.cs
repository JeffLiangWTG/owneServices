using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business
{
	public class ProductProvider : IProduct
	{
		public static ProductProvider NewOrNull(ProductSupportingInfo product) => product == null ? null : new ProductProvider(product);

		ProductProvider(ProductSupportingInfo product)
		{
			this.product = product;
		}
		readonly ProductSupportingInfo product;

		public string CommodityCode => product.CSI_Code;

		public string GoodsDescription => product.CSI_Description;

		public string HarmonizedSystemSubHeadingCode => product.CSI_Tariff.SubstringSafe(0, 6);

		public string CombinedNomenclatureCode => product.CSI_Tariff.SubstringSafe(6, 2);
	}
}
