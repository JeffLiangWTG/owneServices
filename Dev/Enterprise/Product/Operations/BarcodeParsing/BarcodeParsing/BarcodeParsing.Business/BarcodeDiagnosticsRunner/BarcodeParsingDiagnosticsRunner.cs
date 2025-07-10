using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeParsingDiagnosticsRunner : BarcodeDiagnosticsRunner<BarcodeRule>
	{
		protected override IReadOnlyCollection<BarcodeRule> GetRules(BusinessObjectFactory factory, LoadMatchingRulesParameters parameters)
				=> BarcodeRule.LoadMatchingRules(factory, parameters.ModuleCode, parameters.BuyerPK, parameters.SupplierPK, parameters.RelatedEntityPK, parameters.IsGS1).ToList();

		protected override BarcodeDiagnosticsResult GetResult<TEnum>(IBarcodeParsingConsumer consumer, IEnumerable<BarcodeRule> rules, string barcode, string targetField)
		{
			var resultSet = BarcodeParser<TEnum>.ParseBarcode(barcode, rules);
			var barcodeRuleProcessingMessage = BarcodeResultsFormatter.FormatResults(consumer.TargetFields.GetDescriptionFromCode, resultSet);

			return new BarcodeDiagnosticsResult((IBusiness)resultSet.GetMatchedRules().FirstOrDefault(), barcodeRuleProcessingMessage);
		}
	}
}
