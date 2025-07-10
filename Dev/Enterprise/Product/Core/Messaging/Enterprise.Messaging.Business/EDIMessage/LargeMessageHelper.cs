using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business
{
	public static class LargeMessageHelper
	{
		public const int ShortTextSizeLimit = 1024;
		public const int DetailTextSizeLimit = 256 * 1024;
		public const int BufferSize = 32 * 1024;
		public const int VirtualStreamSizeLimit = 128 * 1024;

		public static int SizeInKb(int sizeInBytes)
		{
			return sizeInBytes / 1024;
		}

		public static void AddStream(this StreamWriter writer, TextReader reader)
		{
			while (true)
			{
				char[] buffer = new char[LargeMessageHelper.BufferSize];
				int readCount = reader.Read(buffer, 0, buffer.Length);
				if (readCount == 0)
				{
					break;
				}

				writer.Write(buffer, 0, readCount);
			}

			writer.Flush();
		}

		public static void AddStream(this Stream writer, Stream reader)
		{
			while (true)
			{
				byte[] buffer = new byte[LargeMessageHelper.BufferSize];
				int readCount = reader.Read(buffer, 0, buffer.Length);
				if (readCount == 0)
				{
					break;
				}

				writer.Write(buffer, 0, readCount);
			}

			writer.Flush();
		}

		public static Stream CopyAndDispose(this TextReader reader)
		{
			var stream = new VirtualMemoryStream();
			var writer = new StreamWriter(stream, MessageEncoding.UTF8WithoutBOM);
			using (reader)
			{
				writer.AddStream(reader);
			}
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static Stream CopyWithoutDispose(this TextReader reader)
		{
			var stream = new VirtualMemoryStream();
			try
			{
				var writer = new StreamWriter(stream, MessageEncoding.UTF8WithoutBOM);
				writer.AddStream(reader);
				writer.Flush();
				stream.Position = 0;
				return stream;
			}
#pragma warning disable ENT0001
			catch
#pragma warning restore ENT0001
			{
				stream.Dispose();
				throw;
			}
		}

		public static void ReadToNextElement(XmlReader reader)
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					break;
				}
			}
		}

		public static Stream GetStreamFromNode(XmlReader reader)
		{
			var stream = new VirtualMemoryStream();
			var settings = new XmlWriterSettings()
			{
				OmitXmlDeclaration = true,
				NewLineHandling = NewLineHandling.None,
				Encoding = MessageEncoding.UTF8WithoutBOM
			};
			var writer = XmlWriter.Create(stream, settings);
			writer.WriteNode(reader.ReadSubtree(), false);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static ZString GetString(this TextReader reader, int characterNumber)
		{
			var buffer = new char[characterNumber];
			int readCount = reader.Read(buffer, 0, buffer.Length);
			if (readCount != buffer.Length)
			{
				Array.Resize(ref buffer, readCount);
			}

			return new string(buffer);
		}

		public delegate ZString TextPadder(ZString text, TextPadderDataType dataType);

		public enum TextPadderDataType { Header, Body, Footer }

		public static ZString GetTruncatedInterchangeText(ZString header, TextReader bodyReader, ZString footer, int sizeToTruncate, TextPadder textPadder)
		{
			StringBuilder text = new StringBuilder();
			header = textPadder != null ? textPadder(header, TextPadderDataType.Header) : header;
			if (header.Length > sizeToTruncate)
			{
				text.Append(header.Left(sizeToTruncate));
			}
			else
			{
				text.Append(header);

				ZString body = bodyReader.GetString(sizeToTruncate - text.Length);
				body = textPadder != null ? textPadder(body, TextPadderDataType.Body) : body;
				if (body.Length > sizeToTruncate - text.Length)
				{
					text.Append(body.Left(sizeToTruncate - text.Length));
				}
				else
				{
					text.Append(body);

					footer = textPadder != null ? textPadder(footer, TextPadderDataType.Footer) : footer;

					if (footer.Length > sizeToTruncate - text.Length)
					{
						text.Append(footer.Left(sizeToTruncate - text.Length));
					}
					else
					{
						text.Append(footer);
					}
				}
			}

			return text.ToString();
		}
	}

	public class LargeFileHolder : ITextReaderSource
	{
		readonly string fileName;

		public LargeFileHolder(string fileName)
		{
			this.fileName = fileName;
		}

		public TextReader GetReader(bool closeUnderlyingStream = true)
		{
			return string.IsNullOrEmpty(fileName) ? null : new StreamReader(fileName);
		}
	}

	public class TextReaderSource : ITextReaderSource
	{
		readonly Stream stream;
		readonly Encoding encoding;

		public TextReaderSource(Stream stream)
		{
			this.stream = stream;
		}

		public TextReaderSource(Stream stream, Encoding encoding)
			: this(stream)
		{
			this.encoding = encoding;
		}

		public TextReader GetReader(bool closeUnderlyingStream = true)
		{
			TextReader result = null;
			if (stream != null)
			{
				stream.Position = 0;
				if (encoding == null)
				{
					result = new StreamReader(closeUnderlyingStream ? stream : new UnclosableStreamWrapper(stream));
				}
				else
				{
					result = new StreamReader(closeUnderlyingStream ? stream : new UnclosableStreamWrapper(stream), encoding);
				}
			}
			return result;
		}
	}
}
