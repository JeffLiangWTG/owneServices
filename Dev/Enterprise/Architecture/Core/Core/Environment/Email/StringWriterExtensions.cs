using System;
using System.IO;
using System.Web;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// These code copied from HTMLTextWriter.
	/// https://referencesource.microsoft.com/#System.Web/UI/HTMLTextWriter.cs,3152b2f9f40f2ee7
	/// </summary>
	public static class StringWriterExtensions
	{
		public const char TagLeftChar = '<';
		public const char TagRightChar = '>';
		public const char DoubleQuoteChar = '"';
		public const char SpaceChar = ' ';
		public const char SlashChar = '/';
		public const string EqualsDoubleQuoteString = "=\"";

		public static void WriteBeginTag(this StringWriter writer, string tagName)
		{
			writer.Write(TagLeftChar);
			writer.Write(tagName);
		}

		public static void WriteEndTag(this StringWriter writer, string tagName)
		{
			writer.Write(TagLeftChar);
			writer.Write(SlashChar);
			writer.Write(tagName);
			writer.Write(TagRightChar);
		}

		//
		// Summary:
		//     Writes the specified markup attribute and value to the output stream, and, if
		//     specified, writes the value encoded.
		//
		// Parameters:
		//   name:
		//     The markup attribute to write to the output stream.
		//
		//   value:
		//     The value assigned to the attribute.
		//
		//   fEncode:
		//     true to encode the attribute and its assigned value; otherwise, false.
		public static void WriteAttribute(this StringWriter writer, string name, string value)
		{
			writer.Write(SpaceChar);
			writer.Write(name);
			if (value != null)
			{
				writer.Write(EqualsDoubleQuoteString);
				writer.Write(value);
				writer.Write(DoubleQuoteChar);
			}
		}

		public static void WriteBreak(this StringWriter writer)
		{
			writer.Write("<br />");
		}

		public static void WriteEncodedText(this StringWriter writer, string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			const char NBSP = '\u00A0';

			// When inner text is retrieved for a text control, &nbsp; is
			// decoded to 0x00A0 (code point for nbsp in Unicode).
			// HtmlEncode doesn't encode 0x00A0  to &nbsp;, we need to do it
			// manually here.
			int length = text.Length;
			int pos = 0;
			while (pos < length)
			{
				int nbsp = text.IndexOf(NBSP, pos);
				if (nbsp < 0)
				{
					HttpUtility.HtmlEncode(pos == 0 ? text : text.Substring(pos, length - pos), writer);
					pos = length;
				}
				else
				{
					if (nbsp > pos)
					{
						HttpUtility.HtmlEncode(text.Substring(pos, nbsp - pos), writer);
					}
					writer.Write("&nbsp;");
					pos = nbsp + 1;
				}
			}
		}
	}
}
