namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message815LineProviderHelper : LineProviderHelper
	{
		public Message815LineProviderHelper(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
		}

		public string Tariff => emcsInvoiceLine.JI_Tariff;

		public decimal Quantity => emcsInvoiceLine.JI_CustomsQuantity.Normalize();

		public bool FiscalMarkUsedFlag => emcsInvoiceLine.ZG_FiscalMarkUsed;

		public decimal GrossWeight => emcsInvoiceLine.JI_Weight.Normalize();

		public decimal NetWeight => emcsInvoiceLine.NetWeightInKG.Normalize();

		public decimal AlcoholicStrength => emcsInvoiceLine.ZG_AlcoholicStrength.Normalize();

		public decimal DegreePlato => emcsInvoiceLine.ZG_DegreePlato.Normalize();

		public decimal SizeOfProducer => emcsInvoiceLine.ZG_SizeOfProducer;

		public decimal Density => emcsInvoiceLine.ZG_Density.Normalize();
	}
}
