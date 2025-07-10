using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class IsProductionSystem : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<IsProductionSystem>",
				ResString.GetMultilingualString("1F54C159-BA70-4FB1-86C3-D4643D2F762A", "Returns a value that indicates whether the current system is a production system as opposed to a test/training system."),
				new List<(string example, object expectedResult)> { ("<IsProductionSystem>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return new ZBool(EnvProxy.Instance.IsProductionSystem).ToYN();
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<(?:[\s]*)IsProductionSystem(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
