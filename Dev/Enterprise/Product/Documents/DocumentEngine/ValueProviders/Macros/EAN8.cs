using System.Text.RegularExpressions;
using Enterprise.Barcode.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class EAN8 : Barcode
	{
		protected override string CodeType { get; } = "EAN-8";
		internal override string ExampleContentForDocumentation => "12345670";
		protected internal override IBarcodeProcessor BarcodeProcessor { get; } = new EAN8CodeProcessor();
		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(
			@"^<\s*ean-8\s*\(\s*""(?<InputText>.*)""\s*,\s*(?<WidthInColumns>\d+)\s*,\s*(?<HeightInRows>\d+)\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
