using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.PdfBuilder
{
	public static class PdfBuilderXmlSchemeLoader
	{
		public static PdfElement[] Load(Stream schemeStream)
		{
			var list = new List<PdfElement>();

			var reader = new XmlTextReader(schemeStream);
			ReadNext(reader);
			if (reader.Name != (NoResString)"scheme")
			{
				throw new NotSupportedException($"element {reader.Name} is not supported");
			}

			while (ReadNext(reader) && reader.IsStartElement())
			{
				if (reader.Name == (NoResString)"TextSection")
				{
					list.Add(LoadTextSection(reader));
				}
				else if (reader.Name == (NoResString)"TableSection")
				{
					list.Add(LoadTableSection(reader));
				}
				else
				{
					throw new NotSupportedException($"element {reader.Name} is not supported");
				}
			}

			return list.ToArray();
		}

		static bool ReadNext(XmlTextReader reader)
		{
			do
			{
				if (!reader.Read())
				{
					return false;
				}
			} while (reader.NodeType == XmlNodeType.Whitespace);

			return true;
		}

		static int ReadNumber(XmlTextReader reader)
		{
			if (reader.NodeType != XmlNodeType.Text)
			{
				throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
			}

			if (Int32.TryParse(reader.Value.Trim(), out int value))
			{
				return value;
			}

			throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
		}

		static ColumnAlignment ReadAlignment(XmlTextReader reader)
		{
			if (reader.NodeType != XmlNodeType.Text)
			{
				throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
			}

			if (ColumnAlignment.TryParse(reader.Value.Trim(), out ColumnAlignment value))
			{
				return value;
			}

			throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
		}

		static bool ReadBoolean(XmlTextReader reader)
		{
			if (reader.NodeType != XmlNodeType.Text)
			{
				throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
			}

			if (bool.TryParse(reader.Value, out bool value))
			{
				return value;
			}

			throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
		}

		static bool LoadCommonAttributes(XmlTextReader reader, PdfElement element)
		{
			if (reader.Name == (NoResString)"MarginLeft")
			{
				ReadNext(reader);
				element.MarginLeft = ReadNumber(reader);
			}
			else if (reader.Name == (NoResString)"MarginTop")
			{
				ReadNext(reader);
				element.MarginTop = ReadNumber(reader);
			}
			else if (reader.Name == (NoResString)"FontHeight")
			{
				ReadNext(reader);
				element.FontHeight = ReadNumber(reader);
			}
			else if (reader.Name == (NoResString)"LineHeight")
			{
				ReadNext(reader);
				element.LineHeight = ReadNumber(reader);
			}
			else if (reader.Name == (NoResString)"FontName")
			{
				ReadNext(reader);
				if (reader.NodeType != XmlNodeType.Text)
				{
					throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
				}
				element.FontName = reader.Value.Trim();
			}
			else if (reader.Name == (NoResString)"Padding")
			{
				ReadNext(reader);
				element.MarginLeft = ReadNumber(reader);
			}
			else if (reader.Name == (NoResString)"IsCentered")
			{
				ReadNext(reader);
				element.IsCentered = ReadBoolean(reader);
			}
			else
			{
				return false;
			}

			ReadNext(reader);
			if (reader.IsStartElement())
			{
				throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
			}

			return true;
		}

		static string[] LoadLines(XmlTextReader reader)
		{
			var list = new List<string>();

			while (ReadNext(reader) && reader.IsStartElement())
			{
				if (reader.Name == (NoResString)"Line")
				{
					ReadNext(reader);
					if (reader.NodeType == XmlNodeType.Text)
					{
						list.Add(reader.Value);
					}
					else if (reader.NodeType == XmlNodeType.EndElement)
					{
						list.Add(reader.Value);

						continue;
					}
					else
					{
						throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
					}

					ReadNext(reader);
					if (reader.IsStartElement())
					{
						throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
					}
				}
				else
				{
					throw new NotSupportedException($"element {reader.Name} is not supported");
				}
			}

			return list.ToArray();
		}

		static string[][] LoadRows(XmlTextReader reader)
		{
			var list = new List<string[]>();

			while (ReadNext(reader) && reader.IsStartElement())
			{
				if (reader.Name == (NoResString)"Row")
				{
					list.Add(LoadLines(reader));
				}
				else
				{
					throw new NotSupportedException($"element {reader.Name} is not supported");
				}
			}

			return list.ToArray();
		}

		static int[] LoadValues(XmlTextReader reader)
		{
			var list = new List<int>();

			while (ReadNext(reader) && reader.IsStartElement())
			{
				if (reader.Name == (NoResString)"Value")
				{
					ReadNext(reader);
					if (reader.NodeType == XmlNodeType.Text)
					{
						list.Add(ReadNumber(reader));
					}
					else
					{
						throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
					}

					ReadNext(reader);
					if (reader.IsStartElement())
					{
						throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
					}
				}
				else
				{
					throw new NotSupportedException($"element {reader.Name} is not supported");
				}
			}

			return list.ToArray();
		}

		static ColumnAlignment[] LoadAlignments(XmlTextReader reader)
		{
			var list = new List<ColumnAlignment>();

			while (ReadNext(reader) && reader.IsStartElement())
			{
				if (reader.Name == (NoResString)"Alignment")
				{
					ReadNext(reader);
					if (reader.NodeType == XmlNodeType.Text)
					{
						list.Add(ReadAlignment(reader));
					}
					else
					{
						throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
					}

					ReadNext(reader);
					if (reader.IsStartElement())
					{
						throw new NotSupportedException($"Invalid scheme {reader.LineNumber}:{reader.LinePosition}");
					}
				}
				else
				{
					throw new NotSupportedException($"The element {reader.Name} is not supported");
				}
			}

			return list.ToArray();
		}

		static PdfElement LoadTextSection(XmlTextReader reader)
		{
			var textSection = new PdfTextSection();
			while (ReadNext(reader) && reader.IsStartElement())
			{
				if (!LoadCommonAttributes(reader, textSection))
				{
					if (reader.Name == (NoResString)"Texts")
					{
						textSection.Texts.AddRange(LoadLines(reader));
					}
					else
					{
						throw new NotSupportedException($"The element {reader.Name} is not supported");
					}
				}
			}

			return textSection;
		}

		static PdfElement LoadTableSection(XmlTextReader reader)
		{
			var tableSection = new PdfTableSection();
			while (ReadNext(reader) && reader.IsStartElement())
			{
				if (!LoadCommonAttributes(reader, tableSection))
				{
					if (reader.Name == (NoResString)"Headers")
					{
						tableSection.Headers.AddRange(LoadLines(reader));
					}
					else if (reader.Name == (NoResString)"Widths")
					{
						tableSection.Widths.AddRange(LoadValues(reader));
					}
					else if (reader.Name == (NoResString)"Alignments")
					{
						tableSection.Alignments.AddRange(LoadAlignments(reader));
					}
					else if (reader.Name == (NoResString)"Rows")
					{
						tableSection.Rows.AddRange(LoadRows(reader));
					}
					else
					{
						throw new NotSupportedException($"The element {reader.Name} is not supported");
					}
				}
			}

			return tableSection;
		}
	}
}
