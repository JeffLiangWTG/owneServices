using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CurrentDepartment : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrentDepartment>",
				ResString.GetMultilingualString("a6855b82-ba20-4965-863b-d67d4fc7522d", "Returns the PK for the Current Department ({0}) the current user is logged into.", "GlbDepartment"),
				new List<(string example, object expectedResult)> { ("<CurrentDepartment>", GlbDepartment.CurrentDepartment.PK) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var department = GlbDepartment.CurrentDepartment;
			return department != null ? department.PK : Guid.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Current\s*Department\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
