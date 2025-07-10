using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class LoginFaxNumber : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginFaxNumber>",
				ResString.GetMultilingualString("82000e16-0cc8-4b03-aa50-1a4bc5764ebc", @"Gets the Fax number of the current user logged into {0}.
If the current logged in user does not have a work Fax number entered or it is not set as published, then fall back to the branch/company Fax number.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginFaxNumber>", "07 3868 1274") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var number = GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_FaxNum : ZString.Empty;

			if (string.IsNullOrEmpty(number) || !GlbStaff.CurrentUser.GS_PublishFaxNum)
			{
				number = GlbBranch.CurrentBranch != null ? GlbBranch.CurrentBranch.GB_Fax : ZString.Empty;

				if (string.IsNullOrEmpty(number))
				{
					number = GlbCompany.CurrentCompany != null ? GlbCompany.CurrentCompany.GC_Fax : ZString.Empty;
				}
			}

			return number;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)Login(?:[\s]*)Fax(?:[\s]*)Number(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
