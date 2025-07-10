using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class CommodityTypeWithGrossNetMassAndTaxesProvider : CommodityTypeWithGrossMassProvider, ICommodityTypeWithGrossNetMassAndTaxes
	{
		public CommodityTypeWithGrossNetMassAndTaxesProvider(EntryLineWrapper entryLineWrapper) : base(entryLineWrapper)
		{
		}

		public decimal NetMass
		{
			get
			{
				if (!netMass.HasValue)
				{
					netMass = entryLineWrapper.EntryLine.EffectiveCustomsWeight.InKilogramsSafe;
				}
				return netMass.Value;
			}
		}
		decimal? netMass;

		public ICalculationOfTaxes CalculationOfTaxes => null;
	}
}
