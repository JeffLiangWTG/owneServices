using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIDECLineProductProvider : ISCIDECLineProduct
	{
		public SCIDECLineProductProvider(InwardProcessingProduct product)
		{
			Argument.NotNull(product, nameof(product));

			GoodsDescription = product.CSI_Description;
			YieldType = product.CSI_SubType;
			YieldRate = product.CSI_AdditionalDescription;
			CombinedNomenclatureCode = product.CSI_Tariff;
		}

		public string GoodsDescription { get; }

		public string YieldType { get; }

		public string YieldRate { get; }

		public string CombinedNomenclatureCode { get; }
	}
}
