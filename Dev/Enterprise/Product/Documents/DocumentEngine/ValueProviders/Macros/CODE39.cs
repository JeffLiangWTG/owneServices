using System.Text.RegularExpressions;
using Enterprise.Barcode.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CODE39 : Barcode
	{
		protected override string CodeType { get; } = "CODE-39";
		internal override string ExampleContentForDocumentation => "10404UZ176908720122";
		protected internal override IBarcodeProcessor BarcodeProcessor { get; } = new CODE39CodeProcessor();
		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(
			@"^<\s*code-39\s*\(\s*""(?<InputText>.*)""\s*,\s*(?<WidthInColumns>\d+)\s*,\s*(?<HeightInRows>\d+)\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
