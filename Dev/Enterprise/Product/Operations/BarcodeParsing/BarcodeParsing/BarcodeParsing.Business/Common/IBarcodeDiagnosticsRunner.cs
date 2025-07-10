namespace Enterprise.BarcodeParsing.Business
{
	public interface IBarcodeDiagnosticsRunner
	{
		BarcodeDiagnosticsResult Run(IBarcodeParsingConsumer consumer, LoadMatchingRulesParameters loadMatchingRulesParameters, string barcode, string targetField);
	}
}
