using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeValidationDiagnosticsRunner : BarcodeDiagnosticsRunner<BarcodeValidationRule>
	{
		protected override IReadOnlyCollection<BarcodeValidationRule> GetRules(BusinessObjectFactory factory, LoadMatchingRulesParameters parameters)
			=> BarcodeValidationRule.LoadMatchingRules(factory, parameters.ModuleCode, parameters.BuyerPK, parameters.SupplierPK, parameters.RelatedEntityPK).ToList();

		protected override BarcodeDiagnosticsResult GetResult<TEnum>(IBarcodeParsingConsumer consumer, IEnumerable<BarcodeValidationRule> rules, string barcode, string targetField)
		{
			if (Enum.TryParse(targetField, out TEnum parsedTargetField))
			{
				var result = BarcodeValidator.Validate(barcode, parsedTargetField, rules);
				return new BarcodeDiagnosticsResult((IBusiness)result.MatchedRule, BarcodeResultsFormatter.FormatValidationResult(result.MatchedRule != null, result.IsValid));
			}
			else
			{
				throw new ArgumentException("BarcodeValidationDiagnosticsRunner must have a valid Enum for the Target Field.");
			}
		}
	}
}
