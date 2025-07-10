using System;
using System.IO;
using System.Text;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public static class ErrorLogHelper
	{
		public static string GetNormalizedContent(string filePath)
		{
			return FixBrokenXml(ReadAllText(filePath, 4096));
		}

		internal static StringBuilder ReadAllText(string filePath, int bufferSize)
		{
			using (var streamReader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize))
			{
				var sb = new StringBuilder(bufferSize);
				var buffer = new char[bufferSize];

				while (true)
				{
					var num = streamReader.ReadBlock(buffer, 0, buffer.Length);
					if (num <= 0)
					{
						break;
					}

					sb.Append(buffer, 0, num);
				}

				return sb;
			}
		}

		static string FixBrokenXml(StringBuilder data)
		{
			data.Replace("\0", "ASCIINULL"); //char 0x00 / ascii null, rest are strings
			data.Replace(@"&#x0;", "NULLENTITY");
			data.Replace(@"&#0;", "NULLENTITYDEC");
			data.Replace(@"\u0000", "UNICODENULLESCAPE");
			data.Replace("\\0<", "NULLESCAPE<");
			TrimEnd(data);

			var result = data.ToString();
			if (result.Contains("<EDI_Exception_Report>")
				&& !result.EndsWith("</EDI_Exception_Report>", StringComparison.Ordinal))
			{
				var indexOfExceptionDetailsTail = result.LastIndexOf("</ExceptionDetails>", StringComparison.Ordinal);
				if (indexOfExceptionDetailsTail >= 0)
				{
					result = result.Substring(0, indexOfExceptionDetailsTail);
				}
				result += "</ExceptionDetails></EDI_Exception_Report>";
			}

			return result;
		}

		internal static void TrimEnd(StringBuilder sb)
		{
			var index = sb.Length - 1;
			while (index >= 0 && char.IsWhiteSpace(sb[index]))
			{
				index--;
			}
			if (index < sb.Length - 1)
			{
				sb.Length = index + 1;
			}
		}
	}
}
