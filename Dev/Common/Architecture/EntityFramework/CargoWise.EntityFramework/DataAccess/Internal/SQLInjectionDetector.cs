using System;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class SQLInjectionException : Exception
	{
		public SQLInjectionException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected SQLInjectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public class SQLInjectionDetector
	{
		static int injectionDisableCount;
		public SQLInjectionDetector()
		{
		}

		static readonly Regex nonLiteralQuotedString = new Regex(@"(/\*\s*StringLiteral\s*\*/\s*)?'(.*?)'(?!')", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
		static readonly Regex safeString = new Regex(@"^(/\*\s*StringLiteral\s*\*/\s*)|('(|~|[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})'$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static IDisposable Disable()
		{
			injectionDisableCount++;
			return new DisposableAction(delegate
			{ injectionDisableCount--; });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "simple string comparison")]
		public void CheckForSQLInjection(string sQLText)
		{
			if (injectionDisableCount == 0)
			{
				foreach (Match stringContents in nonLiteralQuotedString.Matches(sQLText))
				{
					if (!safeString.IsMatch(stringContents.Groups[0].Value) && stringContents.Groups[0].Value != "'System.Guid'")
					{
						throw new SQLInjectionException("The following SQL has the potential to be exploited for SQL injection. Use Parameters for all string literals.\r\n\r\n" + sQLText);
					}
				}
			}
		}
	}
}
