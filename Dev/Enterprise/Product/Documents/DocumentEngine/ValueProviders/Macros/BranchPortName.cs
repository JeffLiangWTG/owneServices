using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class BranchPortName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<BranchPortName>",
				ResString.GetMultilingualString("50860426-683c-4f9d-af5b-031bfa9e54f0", "Returns the name of the location of the current branch."),
				new List<(string example, object expectedResult)> { ("<BranchPortName>", (NoResString)"Brisbane") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			GlbBranch branch;
			RefUNLOCO port;

			if ((branch = GlbBranch.CurrentBranch) != null && (port = branch.HomePort) != null)
			{
				return port.RL_PortName;
			}
			else
			{
				return "";
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*BranchPortName\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
