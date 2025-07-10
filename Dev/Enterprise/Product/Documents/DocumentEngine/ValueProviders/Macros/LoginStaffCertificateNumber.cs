using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LoginStaffCertificateNumber : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginStaffCertificateNumber({typecode})>",
				ResString.GetMultilingualString("1f53a287-828d-4728-9d58-df67b6a85a11", @"Returns a specific certificate number for the current user."),
				new List<(string example, object expectedResult)> { ("<LoginStaffCertificateNumber(PAS)>", "123456") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (GlbStaff.CurrentUser == null)
			{
				return string.Empty;
			}

			var match = Regex.Match(macro);
			var typeCode = match.Groups["CertificateTypeCode"].Value.Trim();

			return new GenRegCertAccredMaintList.Loader(Factory).GetByCertificateType(GlbStaff.CurrentUser, typeCode);
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)LoginStaffCertificateNumber(?:[\s]*)\((?:[\s]*)(?<CertificateTypeCode>[^\s]+)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
