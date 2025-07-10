using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class IsPointInShape : ValueProvider
	{
		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(@"^<\s*IsPointInShape\s*\(\s*([^\s,]+)\s*,\s*(.+)\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<IsPointInShape({GeographyPoint}, {GeographyPolygon})>",
				ResString.GetMultilingualString("ee30ae94-2b4f-47ec-ae3e-9b7bcfebd409", @"This macro takes geography values and will return true if the {0} is within the {1}.
This can be used inside the IF Macro to present different values depending on the location.",
"{GeographyPoint}", "{GeographyPolygon}"),
				new List<(string example, object expectedResult)> {
					("<IsPointInShape(<GetConsigneeDocAddress.E2_GeoLocation>, <GetGeography(\"Scotland\", \"UKN\")>)>", "Y"),
					("<IsPointInShape(<GetConsigneeDocAddress.E2_GeoLocation>, <GetGeography(\"England\", \"UKN\")>)>", "N"),
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);

			var pointMacro = match.Groups[1].Value;
			ZGeography? point = GetGeographyValueByMacro(
				macro: pointMacro,
				report: report,
				validationCondition: g => g.IsPoint,
				errorMessage: Res.GetString("7f69df2a-9b6a-43f1-80f7-b9d9621c12c9", "'{0}' does not represent a valid geography point.", pointMacro));

			var shapeMacro = match.Groups[2].Value;
			ZGeography? shape = GetGeographyValueByMacro(
				macro: shapeMacro,
				report: report,
				validationCondition: g => g.IsEmpty || g.IsPolygon || g.IsMultiPolygon,
				errorMessage: Res.GetString("bed7449f-8081-4f5a-a5c1-02d17a340ee3", "'{0}' does not represent a valid geography shape.", shapeMacro));

			return point != null && shape != null ? ((ZBool)shape.Value.STContains(point.Value)).ToYN() : ZBool.False.ToYN();
		}

		ZGeography? GetGeographyValueByMacro(string macro, Report report, Func<ZGeography, bool> validationCondition, string errorMessage)
		{
			ZGeography? result = null;
			var geoObj = report.MacroTranslator.GetValue(macro, report.Renderer.CurrentPass);
			if (geoObj is ZGeography)
			{
				var geoValue = (ZGeography)geoObj;
				if (validationCondition == null || validationCondition(geoValue))
				{
					result = geoValue;
				}
			}

			if (result == null)
			{
				ReportMacroError(report, errorMessage);
			}

			return result;
		}

		protected override bool ShouldEvaluateInnerMacrosCore(string macro) => false;

		public override bool EvaluateAllInnerMacrosWhenGettingReplacement => true;
	}
}
