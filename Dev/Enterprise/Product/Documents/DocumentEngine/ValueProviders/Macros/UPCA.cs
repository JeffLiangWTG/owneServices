using System.Text.RegularExpressions;
using Enterprise.Barcode.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class UPCA : Barcode
	{
		protected override string CodeType { get; } = "UPC-A";
		internal override string ExampleContentForDocumentation => "725272730706";
		protected internal override IBarcodeProcessor BarcodeProcessor { get; } = new UPCACodeProcessor();
		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(
			@"^<\s*upc-a\s*\(\s*""(?<InputText>.*)""\s*,\s*(?<WidthInColumns>\d+)\s*,\s*(?<HeightInRows>\d+)\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
