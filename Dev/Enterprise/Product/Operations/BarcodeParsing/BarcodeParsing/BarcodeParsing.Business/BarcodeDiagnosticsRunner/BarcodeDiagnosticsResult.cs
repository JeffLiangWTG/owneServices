using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeDiagnosticsResult
	{
		public BarcodeDiagnosticsResult(IBusiness matchedRule, string barcodeRuleProcessingMessage)
		{
			MatchedRule = matchedRule;
			BarcodeRuleProcessingMessage = Argument.NotNullOrEmpty(barcodeRuleProcessingMessage, nameof(barcodeRuleProcessingMessage));
		}

		public readonly IBusiness MatchedRule;
		public readonly string BarcodeRuleProcessingMessage;
	}
}
