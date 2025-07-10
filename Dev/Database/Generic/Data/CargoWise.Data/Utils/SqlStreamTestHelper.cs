using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Common;

#region Test
#if DEBUG

using NUnit.Framework;

namespace CargoWise.Data
{
	class SqlStreamTestHelper
	{
		public static void CopyXmlStream(XmlReader reader, XmlWriter writer)
		{
			while (reader.Read())
			{
				WriteShallowNode(reader, writer);
			}
		}

		public static void WriteShallowNode(XmlReader reader, XmlWriter writer)
		{
			Argument.NotNull(reader, "reader");
			Argument.NotNull(writer, "writer");

			switch (reader.NodeType)
			{
				case XmlNodeType.Element:
					writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
					writer.WriteAttributes(reader, true);
					if (reader.IsEmptyElement)
					{
						writer.WriteEndElement();
					}
					break;
				case XmlNodeType.Text:
					writer.WriteString(reader.Value);
					break;
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					writer.WriteWhitespace(reader.Value);
					break;
				case XmlNodeType.CDATA:
					writer.WriteCData(reader.Value);
					break;
				case XmlNodeType.EntityReference:
					writer.WriteEntityRef(reader.Name);
					break;
				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.ProcessingInstruction:
					writer.WriteProcessingInstruction(reader.Name, reader.Value);
					break;
				case XmlNodeType.DocumentType:
					writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
					break;
				case XmlNodeType.Comment:
					writer.WriteComment(reader.Value);
					break;
				case XmlNodeType.EndElement:
					writer.WriteFullEndElement();
					break;
			}
		}

		public static bool XmlFileAreEqual(string filePath1, string filePath2)
		{
			return GetXmlFileAsString(filePath1) == GetXmlFileAsString(filePath2);
		}

		static string GetXmlFileAsString(string filePath)
		{
			StringBuilder sb = new StringBuilder();

			using (XmlReader reader = XmlReader.Create(filePath))
			{
				if (reader != null)
				{
					while (reader.Read())
					{
						sb.AppendLine(reader.ReadOuterXml());
					}
				}
			}

			return sb.ToString();
		}

		public static bool TextFileAreEqual(string filePath1, string filePath2)
		{
			using (FileStream fileStream1 = new FileStream(filePath1, FileMode.Open))
			{
				using (StreamReader reader1 = new StreamReader(fileStream1))
				{
					using (FileStream fileStream2 = new FileStream(filePath2, FileMode.Open))
					{
						using (StreamReader reader2 = new StreamReader(fileStream2))
						{
							return StreamAreEqual(reader1.BaseStream, reader2.BaseStream);
						}
					}
				}
			}
		}

		public static bool BinaryFileAreEqual(string filePath1, string filePath2)
		{
			using (FileStream fileStream1 = new FileStream(filePath1, FileMode.Open))
			{
				using (BinaryReader reader1 = new BinaryReader(fileStream1))
				{
					using (FileStream fileStream2 = new FileStream(filePath2, FileMode.Open))
					{
						using (BinaryReader reader2 = new BinaryReader(fileStream2))
						{
							return StreamAreEqual(reader1.BaseStream, reader2.BaseStream);
						}
					}
				}
			}
		}

		public static bool StreamAreEqual(Stream stream1, Stream stream2)
		{
			if (stream1.Length != stream2.Length)
			{
				return false;
			}

			while (true)
			{
				int byte1 = stream1.ReadByte();
				int byte2 = stream2.ReadByte();

				if (byte1 != byte2)
				{
					return false;
				}

				if (byte1 == -1)
				{
					return true;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1071:DoNotUseGCWaitForPendingFinalizersOrGetTotalMemory", Justification = "Baseline")]
		public static long GetCurrentApplicationMemorySizeByte()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			return GC.GetTotalMemory(true);
		}

		public enum FileType
		{
			Binary,
			Text,
			Xml
		}

		public static string CreateTestFile(FileType type, long sizeInBytes)
		{
			string patternString = "";
			string textPatternString = @"Probably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug. As you wrote, maybe you considerProbably my description was unclear. This is definitely not a bug.";
			string xmlPatternString = "<book id=\"bk101\"><author>Gambardella, Matthew</author><title>XML Developer's Guide</title><genre>Computer</genre><price>44.95</price><publish_date>2000-10-01</publish_date><description>An in-depth look at creating applications with XML.</description></book>";

			string filePath = TempForTest.GetTempFileName();
			StreamWriter writer = new StreamWriter(filePath);

			if (type == FileType.Xml)
			{
				writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				writer.Write("<catalog>");
				patternString = xmlPatternString;
			}
			else
			{
				patternString = textPatternString;
			}

			for (int i = 0; i < sizeInBytes / patternString.Length + 1; i++)
			{
				writer.Write(patternString);
			}

			if (type == FileType.Xml)
			{
				writer.Write("</catalog>");
			}

			writer.Flush();
			writer.Close();
			return filePath;
		}

		public static void WriteTextStream(string fileName, int bufferSize, DbConnection connection, string tableName, string pkColumnName, Guid pk, string dataColumnName, SqlDbType sqlDbType)
		{
			char[] buffer = new char[bufferSize];
			Int32 read = 0;

			using (SqlTextFieldWriter writer = new SqlTextFieldWriter(connection, tableName, pkColumnName, pk, dataColumnName, sqlDbType))
			{
				using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(fileStream))
					{
						do
						{
							read = reader.Read(buffer, 0, buffer.Length);
							writer.Write(buffer, 0, read);
						}
						while (read > 0);
					}
				}
			}
		}

		public static void WriteTextStreamUsingXmlWriter(string fileName, DbConnection connection, string tableName, string pkColumnName, Guid pk, string columnName, SqlDbType sqlDbType)
		{
			using (SqlTextFieldWriter writerStream = new SqlTextFieldWriter(connection, tableName, pkColumnName, pk, columnName, sqlDbType))
			{
				using (XmlWriter writer = XmlWriter.Create(writerStream))
				{
					using (FileStream readerStream = new FileStream(fileName, FileMode.Open))
					{
						using (XmlReader reader = XmlReader.Create(readerStream))
						{
							CopyXmlStream(reader, writer);
						}
					}
				}
			}
		}
	}
}

#endif
#endregion
