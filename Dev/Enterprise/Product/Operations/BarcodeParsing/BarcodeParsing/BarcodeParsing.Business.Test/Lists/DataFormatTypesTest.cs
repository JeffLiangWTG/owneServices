using System;
using System.Linq;
using Enterprise.BarcodeParsingEngine;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class DataFormatTypesTest : TestCase
	{
		#region TestCodeDescriptionPairListMatchesEnumInBarcodeParser

		public void TestCodeDescriptionPairListMatchesEnumInBarcodeParser()
		{
			AssertContainsExactElementsInAnyOrder("Data Format Types list should match the Enum that the Barcode Parser uses.",
				new GS1DataFormatTypes()
					.Cast<CodeDescriptionPair>()
					.Concat(new OtherDataFormatTypes()
					.Cast<CodeDescriptionPair>())
					.Select(c => c.Code),
				Enum.GetValues(typeof(FormatType))
					.Cast<FormatType>()
					.Where(f => f != FormatType.Undefined)
					.Select(e => e.ToString()));
		}

		#endregion

		#region TestEnglishDescriptionsAreCorrect

		public void TestEnglishDescriptionsAreCorrect()
		{
			var codeAndDescriptions = new GS1DataFormatTypes()
				.Cast<CodeDescriptionPair>()
				.Concat(new OtherDataFormatTypes()
				.Cast<CodeDescriptionPair>())
				.Select(c => c.CodeAndDescription);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"ANY - Alpha Numeric (*)",
				"ANS - Alpha Numeric with Space (a-z | A-Z | 0-9 | '  ' )",
				"AN - Alpha Numeric (a-z | A-Z | 0-9)",
				"AS - Alpha with Space (a-z | A-Z | '  ' )",
				"A - Alpha (a-z | A-Z)",
				"N - Numeric (0-9 |  .  |  , )",
				"D - Digits (0-9)",
				"D1 - Date (DDMMYY)",
				"D2 - Date (DDMMYYYY)",
				"D3 - Date (MMDDYY)",
				"D4 - Date (MMDDYYYY)",
				"D5 - Date (YYMMDD) - GS1"
			}, codeAndDescriptions);
		}

		#endregion
	}
}
