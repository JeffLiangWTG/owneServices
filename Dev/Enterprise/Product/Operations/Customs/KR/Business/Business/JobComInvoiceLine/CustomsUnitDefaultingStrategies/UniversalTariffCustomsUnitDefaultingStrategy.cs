namespace Enterprise.Customs.KR.Business
{
	public class UniversalTariffCustomsUnitDefaultingStrategy<TJobComInvoiceLine> : Customs.Business.UniversalTariffCustomsUnitDefaultingStrategy<TJobComInvoiceLine> where TJobComInvoiceLine : JobComInvoiceLine
	{
		public override void DefaultUOMs(TJobComInvoiceLine invoiceLine) { }
	}
}
