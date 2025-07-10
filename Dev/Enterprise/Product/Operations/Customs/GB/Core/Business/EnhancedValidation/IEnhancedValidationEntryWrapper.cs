namespace Enterprise.Customs.GB.Business
{
	public interface IEnhancedValidationEntryWrapper
	{
		string CommodityLine1 { get; }
		string CommodityLine2 { get; }
		string CommodityLine3 { get; }
		string CommodityLine4 { get; }
		string CommodityLine5 { get; }
		int CommodityLineCount { get; }
		string TaxLine1 { get; }
		string TaxLine2 { get; }
		string TaxLine3 { get; }
		string TaxLine4 { get; }
		string TaxLine5 { get; }
		int TaxLineCount { get; }
	}
}