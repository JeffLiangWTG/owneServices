using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class LoginPhoneNumber : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginPhoneNumber>",
				ResString.GetMultilingualString("20f52ecb-b694-4098-9f32-4d22f98d8a9a", @"Gets the phone number of the current user logged into {0}.
If the current logged in user does not have a work phone number entered or it is not set as published, then fall back to the branch/company phone number.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginPhoneNumber>", "+61 2 9025 1100") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var number = GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_WorkPhone : ZString.Empty;

			if (string.IsNullOrEmpty(number) || !GlbStaff.CurrentUser.GS_PublishWorkPhone)
			{
				number = GlbBranch.CurrentBranch != null ? GlbBranch.CurrentBranch.GB_Phone : ZString.Empty;

				if (string.IsNullOrEmpty(number))
				{
					number = GlbCompany.CurrentCompany != null ? GlbCompany.CurrentCompany.GC_Phone : ZString.Empty;
				}
			}

			return number;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)Login(?:[\s]*)Phone(?:[\s]*)Number(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
