using System.Text.RegularExpressions;
using Enterprise.Barcode.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class EAN13 : Barcode
	{
		protected override string CodeType { get; } = "EAN-13";
		internal override string ExampleContentForDocumentation => "1234567890128";
		protected internal override IBarcodeProcessor BarcodeProcessor { get; } = new EAN13CodeProcessor();
		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(
			@"^<\s*ean-13\s*\(\s*""(?<InputText>.*)""\s*,\s*(?<WidthInColumns>\d+)\s*,\s*(?<HeightInRows>\d+)\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
