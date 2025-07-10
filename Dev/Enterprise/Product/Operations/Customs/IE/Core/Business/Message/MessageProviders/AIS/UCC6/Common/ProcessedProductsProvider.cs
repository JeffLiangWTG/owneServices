using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class ProcessedProductsProvider : IProcessedProducts
	{
		internal ProcessedProductsProvider(CusEntryInstruction instruction)
		{
			this.instruction = instruction;
		}

		public string CommodityCode => instruction.ZG_ProcessedProductsCommodityCode;

		public string DescriptionOfGoods => instruction.ProcessedProductDescription;

		readonly CusEntryInstruction instruction;
	}
}
