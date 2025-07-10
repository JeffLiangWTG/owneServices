using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class IdentificationOfGoodsProvider : IIdentificationOfGoods
	{
		internal IdentificationOfGoodsProvider(CusEntryInstruction instruction)
		{
			this.instruction = instruction;
		}

		public string RateOfYield => instruction.ZG_RateOfYield;

		public IProcessedProducts ProcessedProducts
			=> CachedValueHelper.GetValue(ref processedProducts, () => new ProcessedProductsProvider(instruction));
		CachedValue<IProcessedProducts> processedProducts;

		public IAdditionalInformation IdentificationOfGoods
			=> CachedValueHelper.GetValue(ref identificationOfGoods, () => new CodeDetailsProvider(instruction));
		CachedValue<IAdditionalInformation> identificationOfGoods;

		readonly CusEntryInstruction instruction;
	}
}
