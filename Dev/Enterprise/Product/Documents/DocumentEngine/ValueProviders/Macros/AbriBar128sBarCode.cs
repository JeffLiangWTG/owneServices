using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	/// <summary>
	/// Represents the text of a barcode. 
	/// Use this in conjunction with the AbriBar128s font.
	/// There are two steps - get the original text you want to encode -> Convert it to 128s 
	/// indexes for each letter you want to encode -> convert it to appropriate ASCII values
	/// for this specific font.
	/// </summary>
	class AbriBar128sBarCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<AbriBar128sBarCode('{StringValue}'[, UseOptimisedEncoding][, IsGS1128Barcode])>",
				ResString.GetMultilingualString("0ac59363-2a63-4d47-ad1e-69e459312c8a", @"Is used to convert a string value into a barcode text format. The extra parameter is used in the optimization mechanism.
If use option {0} with multiple AIs, please add a comma before AI which is variable length to separate these AIs.
Use this in conjunction with the {1} font.",
"IsGS1128Barcode", "AbriBar128s"),
				new List<(string example, object expectedResult)>
				{ ((NoResString)"<AbriBar128sBarCode('S00001234')>", (NoResString)"ÈS00001234,Ê"),
					((NoResString)"<AbriBar128sBarCode('<ShipmentNumber>', UseOptimisedEncoding)>", (NoResString)"ÈSÃ¯¯,BÆÊ"),
					((NoResString)"<AbriBar128sBarCode('S00001234, S00005678', IsGS1128Barcode)>", (NoResString)"ÈÆS00001234Æ¯S00005678ÆÊ") });
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var matchedGroups = Regex.Match(macro).Groups;
			ZString stringValue = matchedGroups[1].Value;

			bool useOptimisedEncoding = false;
			bool isGS1128Barcode = false;

			var useOptimisedEncodingGroup = matchedGroups[UseOptimisedEncoding];
			if (useOptimisedEncodingGroup != null)
			{
				useOptimisedEncoding = string.Equals(useOptimisedEncodingGroup.Value, UseOptimisedEncoding, StringComparison.OrdinalIgnoreCase);
			}
			var isGS1128BarcodeGroup = matchedGroups[IsGS1128Barcode];
			if (isGS1128BarcodeGroup != null)
			{
				isGS1128Barcode = string.Equals(isGS1128BarcodeGroup.Value, IsGS1128Barcode, StringComparison.OrdinalIgnoreCase);
			}

			TextBarcode textBarcode = null;

			if (isGS1128Barcode)
			{
				var textToEncode = stringValue.Split(',');
				textBarcode = new TextBarcode(textToEncode, useOptimisedEncoding, isGS1128Barcode);
			}
			else
			{
				textBarcode = new TextBarcode(stringValue, useOptimisedEncoding);
			}

			return textBarcode.TextAs128sFontString;
		}

		const string UseOptimisedEncoding = "UseOptimisedEncoding";
		const string IsGS1128Barcode = "IsGS1128Barcode";

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<(?:\s*)abribar128sbarcode(?:\s*)\((?:\s*)(?:')(.*)(?:')(?:\s*)(?:(?:,)(?:\s*)(?<UseOptimisedEncoding>(UseOptimisedEncoding))(?:\s*))?(?:(?:,)(?:\s*)(?<IsGS1128Barcode>(IsGS1128Barcode))(?:\s*))?\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
