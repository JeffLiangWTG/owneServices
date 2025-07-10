using System;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.Common;

#if DEBUG

namespace Enterprise.DbUpgrader.Shared
{
	public static class DbTextFormatTestHelper
	{
		const string SqlDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

		public static string BooleanToBitString(bool b) => b ? "1" : "0";

		public static string GetEqualityPredicateOrIsNull(object o)
		{
			return o == null ? "is null" : "= " + EncloseInQuotesOrNullString(o);
		}

		public static string EncloseInQuotesOrNullString(object o)
		{
			return o == null ? "null" : "'" + (o is DateTime dt ? dt.ToString(SqlDateTimeFormat, CultureInfo.InvariantCulture) : o.ToString()) + "'";
		}

		public static string EncloseInQuotesOrAlternativeOrNullString(object o, object alt)
		{
			return o == null ? EncloseInQuotesOrNullString(alt) : "'" + (o is DateTime dt ? dt.ToString(SqlDateTimeFormat, CultureInfo.InvariantCulture) : o.ToString()) + "'";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Calm down CA, it's for unit tests.")]
		public static string FormatXmlDocSafely(string xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				return doc.ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return xml;
			}
		}
	}
}

#endif
