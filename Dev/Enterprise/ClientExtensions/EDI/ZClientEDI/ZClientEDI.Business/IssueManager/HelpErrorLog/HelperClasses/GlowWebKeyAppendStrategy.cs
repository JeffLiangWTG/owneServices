using System;
using System.Text;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	sealed class GlowWebKeyAppendStrategy : IAppendStrategy
	{
		public GlowWebKeyAppendStrategy(IAppendStrategy inner, string version, Uri requestUrl)
		{
			if (inner == null)
			{
				throw new ArgumentNullException(nameof(inner));
			}

			if (string.IsNullOrEmpty(version))
			{
				throw new ArgumentException("Version must not be null or empty", nameof(version));
			}

			if (requestUrl == null)
			{
				throw new ArgumentNullException(nameof(requestUrl), "Request URL must not be null");
			}

			this.inner = inner;
			this.version = version;
			this.requestUrl = requestUrl;
		}

		readonly IAppendStrategy inner;
		readonly string version;
		readonly Uri requestUrl;

		void IAppendStrategy.AppendMatch(string text, StringBuilder builder)
		{
			text = text.Replace(version, "<VERSION>");

			// Remove the common prefix of the script URL (http://blah/Portals/JS/aaaa) and the request (http://blah/Portals/CRM/Desktop/#currentTaskTypeThing).
			var schemeWithColon = requestUrl.Scheme + ":";
			var indexOfUriScheme = text.IndexOf(schemeWithColon, StringComparison.Ordinal);
			if (indexOfUriScheme >= 0)
			{
				var scriptUrlPlusLineAndColumnInfo = text.Substring(indexOfUriScheme);
				var lengthOfCommonPrefix = GetLengthOfCommonPrefix(scriptUrlPlusLineAndColumnInfo, requestUrl.AbsoluteUri);
				if (lengthOfCommonPrefix > (schemeWithColon.Length + 1)) // Don't remove stray 'http's etc., only if it's likely to be a url.
				{
					text = InverseSplice(text, indexOfUriScheme, lengthOfCommonPrefix);
				}
			}

			inner.AppendMatch(text, builder);
		}

		static string InverseSplice(string str, int startIndex, int length)
		{
			var left = str.Substring(0, startIndex);
			var right = str.Substring(startIndex + length);
			return left + right;
		}

		static int GetLengthOfCommonPrefix(string string1, string string2)
		{
			int lengthOfShorterString = Math.Min(string1.Length, string2.Length);

			int i = 0;
			while (i < lengthOfShorterString)
			{
				if (char.ToUpperInvariant(string1[i]) != char.ToUpperInvariant(string2[i]))
				{
					break;
				}

				i++;
			}

			return i;
		}
	}
}
