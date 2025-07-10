using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.IO;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading
{
	public class SingleElementExtractor
	{
		public SingleElementExtractor(TextReader textReader)
		{
			this.reader = new ChunkingReader(textReader);
			this.RootElementInfo = new RootElement(reader);
		}
		readonly ChunkingReader reader;
		public readonly RootElement RootElementInfo;

		public class RootElement
		{
			internal RootElement(ChunkingReader reader)
			{
				string rootElementText = GetRootElementText(reader);

				if (rootElementText != null)
				{
					var rootMatch = XmlRegexes.RootElementTag.Match(rootElementText);
					if (rootMatch.Success)
					{
						this.ElementName = rootMatch.Groups["elementName"].Value;
						this.Namespace = rootMatch.Groups["xmlns"].Value;
						this.Version = rootMatch.Groups["version"].Value;
					}
				}
			}

			public readonly string ElementName;
			public readonly string Namespace;
			public readonly string Version;

			static string GetRootElementText(ChunkingReader reader)
			{
				string rootElementText = null;
				bool inElement = false;
				List<char> result = new List<char>();
				while (reader.ReadNextChar())
				{
					if (reader.NextChar == '<')
					{
						inElement = true;
					}

					if (inElement)
					{
						result.Add(reader.NextChar);
					}

					if (reader.NextChar == '>')
					{
						inElement = false;
						rootElementText = new string(result.ToArray());
						if (rootElementText.StartsWith("<?xml"))
						{
							rootElementText = null;
							result.Clear();
						}
						else
						{
							break;
						}
					}
				}
				return rootElementText;
			}
		}

		internal class ChunkingReader
		{
			internal ChunkingReader(TextReader xmlReader)
			{
				this.xmlReader = xmlReader;
				this.buffer = new char[32384];
				this.currentIndex = -1;
				this.currentLength = 0;
			}

			readonly TextReader xmlReader;
			readonly char[] buffer;
			int currentIndex;
			int currentLength;

			internal bool ReadNextChar()
			{
				currentIndex++;
				if (currentIndex >= currentLength)
				{
					currentLength = xmlReader.Read(buffer, 0, buffer.Length);
					currentIndex = 0;
				}

				return currentIndex < currentLength;
			}

			internal char NextChar
			{
				get { return buffer[currentIndex]; }
			}
		}

		public SubStreamableStream GetNestedElementContentIncludingElementIdentifiers(string elementName)
		{
			var result = new CargoWise.IO.Shim.SubStreamableStream();

			try
			{
				using (StreamWriter writer = new StreamWriter(result, Encoding.UTF8, SubStreamableStream.DEFAULTMEMORYSTREAMCHUNKSIZE, true))
				{
					var elementNameChars = (elementName + ">").ToCharArray();

					while (reader.ReadNextChar())
					{
						if (reader.NextChar == '<')
						{
							writer.Write(reader.NextChar);

							var foundOpeningTag = ParseElementTag(writer, elementNameChars);
							if (foundOpeningTag)
							{
								elementNameChars = ("/" + elementName + ">").ToCharArray();

								while (reader.ReadNextChar())
								{
									writer.Write(reader.NextChar);

									if (reader.NextChar == '<')
									{
										var foundClosingTag = ParseElementTag(writer, elementNameChars);
										if (foundClosingTag)
										{
											writer.Flush();
											result.Position = 0;
											return result;
										}
									}
								}
							}
							else
							{
								writer.Flush();
								result.SetLength(0);
							}
						}
					}
				}
			}
			catch
			{
				result.Dispose();
				throw;
			}

			result.Dispose();
			return null;
		}

		bool ParseElementTag(StreamWriter outWriter, char[] elementNameChars)
		{
			foreach (var elementNameChar in elementNameChars)
			{
				if (!reader.ReadNextChar())
				{
					return false;
				}

				outWriter.Write(reader.NextChar);

				if (elementNameChar != reader.NextChar)
				{
					return false;
				}
			}

			return true;
		}
	}
}
