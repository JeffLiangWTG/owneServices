using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class CommodityTypeWithSupplementaryUnitsProvider : CommodityTypeWithGrossNetMassAndTaxesProvider, ICommodityTypeWithSupplementaryUnits
	{
		public CommodityTypeWithSupplementaryUnitsProvider(EntryLineWrapper entryLineWrapper) : base(entryLineWrapper)
		{
		}

		public decimal SupplementaryUnits => entryLine.SupplementaryQuantity;
	}
}
