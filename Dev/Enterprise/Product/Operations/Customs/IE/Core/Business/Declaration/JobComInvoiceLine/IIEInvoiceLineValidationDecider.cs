namespace Enterprise.Customs.IE.Business
{
	public interface IIEInvoiceLineValidationDecider
	{
		public bool IsRuleCD5151ActiveForJI_CountryOfOrigin { get; }
		public bool IsRuleCD5161ActiveForZG_CountryOfSupply { get; }
	}
}
