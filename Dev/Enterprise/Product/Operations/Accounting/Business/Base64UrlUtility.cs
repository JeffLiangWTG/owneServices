using System.Collections.Immutable;
using System.Linq;

namespace Enterprise.Accounting.Business
{
	public static class Base64UrlUtility
	{
		#region RFC 4648, URL Safe base64 encoding. It's basically regular base64 with a couple of characters swapped and no padding
		// Taken from $/Dev/Enterprise/Product/Operations/Recruitment/Recruitment.Common/EmailAddressGenerator.cs
		// TODO: move this to CW1Shared

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054: URI parameters should not be strings", Justification = "Parameter is not a URI")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055: URI return values should not be strings", Justification = "Return value is not a URI")]
		public static string ToUrlSafeBase64(string regularBase64)
			=> replaceChars.Aggregate(regularBase64.TrimEnd(paddingChar), (e, p) => e.Replace(p.original, p.urlsafe));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054: URI parameters should not be strings", Justification = "Parameter is not a URI")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055: URI return values should not be strings", Justification = "Return value is not a URI")]
		public static string FromUrlSafeBase64(string urlSafeBase64)
		{
			var base64 = replaceChars.Aggregate(urlSafeBase64, (e, p) => e.Replace(p.urlsafe, p.original));
			switch (base64.Length % 4)
			{
				case 2: base64 += new string(paddingChar, 2); break;
				case 3: base64 += new string(paddingChar, 1); break;
			}

			return base64;
		}

		readonly static ImmutableArray<(string original, string urlsafe)> replaceChars = ImmutableArray.Create(("+", "-"), ("/", "_"));
		const char paddingChar = '=';

		#endregion
	}
}