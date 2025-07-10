using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading.ValueParsers;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading
{
	public class XmlReader : ElementProcessor, IXmlReader
	{
		public XmlReader()
		{
			this.ValueParsers = new ValueParser[]
			{
				new ZStringValueParser(this),
				new ParserWrapper<ZIntParser>(this),
				new ParserWrapper<ZLongParser>(this),
				new ParserWrapper<ZDecimalParser>(this),
				new ParserWrapper<ZDateParser>(this),
				new ParserWrapper<ZDateTimeParser>(this),
				new ParserWrapper<ZDateTimeOffsetParser>(this),
				new UXmlDateTimeValueParser(this),
				new ParserWrapper<ZBoolParser>(this),
				new EnumValueParser(this),
				new ParserWrapper<ZShortParser>(this),
				new ParserWrapper<ZByteParser>(this),
				new ParserWrapper<ZBlobParser>(this),
				new ParserWrapper<TimeSpanParser>(this),
				new SubStreamableStreamParser(this),
			};

			processingState = ProcessingState.NormalCharacter;
			factory = new DefaultDataObjectFactory();
		}

		public XmlReader(ICodeMappingManager codeMapper, bool throwOnParsingError = false, IDataObjectFactory factory = null)
			: this()
		{
			this.codeMapper = codeMapper;
			this.factory = factory ?? new DefaultDataObjectFactory();
			this.throwOnParsingError = throwOnParsingError;
		}

		readonly ICodeMappingManager codeMapper;
		readonly IDataObjectFactory factory;
		readonly bool throwOnParsingError;
		internal readonly IEnumerable<ValueParser> ValueParsers;

		internal IElementNavigator CurrentElementParser;
		int currentReaderLineNumber;
		ProcessingState processingState;
		IXmlDomObject lastElement;
		PositionRecordingStreamReader reader;
		SubStreamableStream inputStream;
		IXmlImportLogger logger;
		char[] tagReaderBuffer = new char[200];

		string IXmlReader.Namespace
		{
			get { return Namespace; }
		}

		enum ProcessingState
		{
			NormalCharacter,
			SurrogatePair
		}

		public void ReadXML(IDataObject targetDataObject, SubStreamableStream theInputStream, IXmlImportLogger theLogger)
		{
			ReadXML(targetDataObject, theInputStream, theLogger, null, null);
		}

		public void ReadXML(IDataObject targetDataObject, SubStreamableStream theInputStream, IXmlImportLogger theLogger, string nameSpaceOverride = null, string versionOverride = null)
		{
			Argument.NotNull(targetDataObject, "IDataObject targetDataObject");
			this.inputStream = Argument.NotNull(theInputStream, "Stream inputStream");
			this.logger = Argument.NotNull(theLogger, "IXmlImportLogger logger");

			try
			{
				using (reader = new PositionRecordingStreamReader(theInputStream))
				{
					FlushUntilTagFound();

					IXmlDomObject firstTag = null;
					IXmlDomObject secondTag = null;
					IXmlDomObject thirdTag = null;

					try
					{
						firstTag = GetNextTag();

						if (firstTag is ICurrentElement validTag && XmlRegexes.EncodingTag.Match(validTag.Name).Success)
						{
							firstTag.Dispose();
							firstTag = GetNextTag(); // We don't care about Xml Encoding headers because of Postel's law.
						}

						if (firstTag is ICurrentElement rootElement)
						{
							var nextElement = rootElement;
							var targetDataObjectType = targetDataObject.GetType();
							var rootElementAttribute = targetDataObjectType.GetCustomAttribute<RootElementAttribute>(false);
							var rootElementInfo = XmlRegexes.RootElementTag.Match(rootElement.Name).Groups;
							var rootStartLine = rootElement.OpenLineNumber;
							var rootElementName = rootElementInfo["elementName"].Value;

							SetupNamespaceAndVersion(nameSpaceOverride, versionOverride, rootElementInfo);

							if (rootElementAttribute != null)
							{
								if (rootElementName != rootElementAttribute.RootElementName)
								{
									if (string.IsNullOrEmpty(rootElementName))
									{
										rootElementName = XmlRegexes.FullOpeningTag.Match(rootElement.Name).Groups["tag"].Value;
									}

									AbortWithException(rootElement.OpenLineNumber, ElementPath.FullName, (new StringBuilder()).Append("Invalid Root element found. Expected <").Append(rootElementAttribute.RootElementName).Append("> but found <").Append(rootElementName).Append("> instead.").ToString());
								}

								secondTag = GetNextTag();
								nextElement = secondTag as ICurrentElement;
							}

							var expectedTopLevelElementName = targetDataObjectType.Name;
							var topLevelElementNameFound = nextElement is null ? null : XmlRegexes.ValidOpeningTag.Match(nextElement.Name).Groups["elementName"].Value;
							if (topLevelElementNameFound != expectedTopLevelElementName)
							{
								if (string.IsNullOrEmpty(topLevelElementNameFound) && nextElement != null)
								{
									topLevelElementNameFound = XmlRegexes.FullOpeningTag.Match(nextElement.Name).Groups["tag"].Value;
								}

								var lineNumber = (nextElement ?? secondTag)?.OpenLineNumber;
								AbortWithException(lineNumber, ElementPath.FullName, (new StringBuilder()).Append("Root element <").Append(rootElementName).Append("> must contain one <").Append(expectedTopLevelElementName).Append("> element. <").Append(topLevelElementNameFound).Append("> is not valid in this scope.").ToString());
							}
							else
							{
								var elementReader = new ComplexTypeElementParser(this, expectedTopLevelElementName, targetDataObjectType, null, factory, true);
								elementReader.ParseOutContent(nextElement, targetDataObject);
								CurrentElementParser = null;

								if (rootElementAttribute != null)
								{
									thirdTag = GetNextTag();
									if (!(thirdTag is ICurrentElement next) || XmlRegexes.ValidClosingTag.Match(next.Name).Groups["elementName"].Value != rootElementName)
									{
										AbortWithNoClosingTagFoundException(rootStartLine, rootElementName);
									}
								}
							}

							while (!EndOfStream)
							{
								GetNextTag().Dispose(); // Flush any remaining XML. If anything is returned here it is probably a bug...
							}
						}
						else
						{
							throw new XmlProcessingException("No root element found.");
						}
					}
					finally
					{
						firstTag?.Dispose();
						secondTag?.Dispose();
						thirdTag?.Dispose();
					}
				}
			}
			catch (XmlProcessingException exception)
			{
				theLogger.Log(LogType.Error, exception.Message);

				if (this.throwOnParsingError)
				{
					throw;
				}
			}
		}

		void FlushUntilTagFound()
		{
			while (IsNotTheEndOfNextChunkFromStream('<', reader.Peek()))
			{
				var hasChar = TryRead(out char c);
				if (hasChar && c == '\r')
				{
					currentReaderLineNumber++;
				}
			}
		}

		void SetupNamespaceAndVersion(string nameSpaceOverride, string versionOverride, GroupCollection rootElementInfo)
		{
			if (nameSpaceOverride != null && versionOverride != null)
			{
				SetNamespaceAndVersion(nameSpaceOverride, versionOverride);
			}
			else
			{
				var nameSpace = rootElementInfo["xmlns"].Value;
				if (string.IsNullOrEmpty(nameSpace) || nameSpace != UniversalXmlInfo.Namespace_2012_11)
				{
					SetNamespaceAndVersion(UniversalXmlInfo.Namespace_2011_11, UniversalXmlInfo.Version_2011_11);
				}
				else
				{
					var version = rootElementInfo["version"].Value;
					SetNamespaceAndVersion(nameSpace, version);
				}
			}
		}

		#region XmlReading

		class XmlComment : IXmlComment
		{
			public XmlComment()
			{
				DisposableLeakListener.Instance.RegisterDisposable(this, true);
			}

			public SubStreamableStream CommentText { get; set; }
			public int OpenLineNumber { get; set; }

			public void Dispose()
			{
				CommentText?.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		class CurrentElement : ICurrentElement
		{
			public CurrentElement()
			{
				DisposableLeakListener.Instance.RegisterDisposable(this, true);
			}

			public string Name { get; set; }
			public int OpenLineNumber { get; set; }
			public SubStreamableStream Body { get; set; }

			public void Dispose()
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				Body?.Dispose();
			}
		}

		class MalformedXml : IMalformedXmlElement
		{
			public MalformedXml()
			{
				DisposableLeakListener.Instance.RegisterDisposable(this, true);
			}
			public SubStreamableStream Body { get; set; }
			public int OpenLineNumber { get; set; }

			public void Dispose()
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				Body?.Dispose();
			}
		}

		internal IXmlDomObject GetNextTag()
		{
			var commentPrefix = "<!--";
			var parseStartLineNumber = CurrentLineNumber;
			var startPos = reader.StreamPosition;

			while (true)
			{
				if (TryRead(out char c))
				{
					if (c == '\r')
					{
						currentReaderLineNumber++;
					}
					else if (c == '<')
					{
						break;
					}
				}
				else
				{
					return new MalformedXml
					{
						OpenLineNumber = parseStartLineNumber,
						Body = this.inputStream.GetSubStream(startPos, reader.StreamPosition - startPos)
					};
				}
			}

			var len = 1;
			while (this.reader.Peek() == commentPrefix[len])
			{
				TryRead(out char _);
				len++;

				if (len == commentPrefix.Length)
				{
					var result = ReadComment(reader.StreamPosition, CurrentLineNumber);
					if (result is IXmlComment comment) // Only null if we couldn't find the --> at the end.
					{
						AddComment(comment.OpenLineNumber, comment.CommentText);
						result.Dispose();
						return GetNextTag(); // Recur until we find some XML that can actually be serialized into something.
					}
					else
					{
						return result;
					}
				}
			}
			var element = lastElement = ReadCurrentElement();
#if DEBUG
			OnNextTag_TestHook.Value?.Invoke();
#endif
			return element;
		}

#if DEBUG
		public static readonly Overridable<Action> OnNextTag_TestHook = new Overridable<Action>();
#endif

		bool TryRead(out char c)
		{
			var i = reader.Read();

			unchecked
			{
				c = (char)i;
			}

			if (i == -1)
			{
				return false;
			}

			if (processingState == ProcessingState.SurrogatePair)
			{
				processingState = ProcessingState.NormalCharacter;
			}
			else if (!System.Xml.XmlConvert.IsXmlChar(c))
			{
				char nextChar;

				unchecked
				{
					nextChar = (char)reader.Peek();
				}

				if (!System.Xml.XmlConvert.IsXmlSurrogatePair(nextChar, c))
				{
					AbortWithInvalidXmlCharacterFoundException(c);
				}
				else
				{
					//the next char to read is a low surrogate character which we know is valid at this point
					processingState = ProcessingState.SurrogatePair;
				}
			}

			return true;
		}

		bool TryRead(PositionRecordingStreamReader streamReader, out char c)
		{
			var i = streamReader.Read();
			unchecked
			{
				c = (char)i;
			}
			if (i == -1)
			{
				return false;
			}
			if (processingState == ProcessingState.SurrogatePair)
			{
				processingState = ProcessingState.NormalCharacter;
			}
			else if (!System.Xml.XmlConvert.IsXmlChar(c))
			{
				char nextChar;
				unchecked
				{
					nextChar = (char)streamReader.Peek();
				}
				if (!System.Xml.XmlConvert.IsXmlSurrogatePair(nextChar, c))
				{
					AbortWithInvalidXmlCharacterFoundException(c);
				}
				else
				{
					//the next char to read is a low surrogate character which we know is valid at this point
					processingState = ProcessingState.SurrogatePair;
				}
			}
			return true;
		}

		SubStreamableStream ReadCharacterData(SubStreamableStream subStream)
		{
			var buffer = new char[3]; // The default char value of 0 suits our purpose
			var bufferIndex = 0;
			var charDataPrefix = "<![CDATA[";
			var len = 0;
			using (var subStreamReader = new PositionRecordingStreamReader(subStream))
			{
				while (true)
				{
					if (subStreamReader.Peek() == charDataPrefix[len])
					{
						TryRead(subStreamReader, out char _);
						len++;
						if (len == charDataPrefix.Length)
						{
							break;
						}
					}
					else
					{
						return null;
					}
				}
				while (true)
				{
					if (!TryRead(subStreamReader, out char next))
					{
						return null;
					}
					char nextChar;
					unchecked
					{
						nextChar = next;
					}
					buffer[bufferIndex] = nextChar;
					if (nextChar == '\r')
					{
						currentReaderLineNumber++;
					}
					else if (buffer[bufferIndex] != '>' && buffer[(bufferIndex + 2) % 3] == ']' && buffer[(bufferIndex + 1) % 3] == ']')
					{
						AbortWithUnexpectedClosingCharacterDataFoundException(CurrentLineNumber);
					}
					else if (buffer[bufferIndex] == '>' && buffer[(bufferIndex + 2) % 3] == ']' && buffer[(bufferIndex + 1) % 3] == ']')
					{
						return subStream.GetSubStream(charDataPrefix.Length, subStreamReader.StreamPosition - ("]]>".Length + charDataPrefix.Length));
					}
					bufferIndex = (bufferIndex + 1) % 3;
				}
			}
		}

		IXmlDomObject ReadComment(long startPos, int startLineNumber)
		{
			var buffer = new char[3]; // The default char value of 0 suits our purpose
			int bufferIndex = 0;
			while (true)
			{
				if (!TryRead(out char next))
				{
					return new MalformedXml
					{
						OpenLineNumber = startLineNumber,
						Body = this.inputStream.GetSubStream(startPos, reader.StreamPosition - startPos)
					};
				}

				char nextChar;
				unchecked
				{
					nextChar = next;
				}

				buffer[bufferIndex] = nextChar;

				if (nextChar == '\r')
				{
					currentReaderLineNumber++;
				}
				else if (buffer[bufferIndex] == '>' && buffer[(bufferIndex + 2) % 3] == '-' && buffer[(bufferIndex + 1) % 3] == '-')
				{
					var comment = new XmlComment();
					comment.OpenLineNumber = startLineNumber;
					comment.CommentText = this.inputStream.GetSubStream(startPos, reader.StreamPosition - (startPos + "-->".Length));
					return comment;
				}

				bufferIndex = (bufferIndex + 1) % 3;
			}
		}

		ICurrentElement ReadCurrentElement()
		{
			var name = GetNextTagString(addTagStart: true);
			var openLineNumber = CurrentLineNumber; //order matters here for line number
			var body = ReadStuffUntilNextTagStart();
			var result = new CurrentElement();
			result.Name = name;
			result.OpenLineNumber = openLineNumber;
			result.Body = body;
			return result;
		}

		int AppendToTagReaderBuffer(char c, int index)
		{
			var buffer = tagReaderBuffer;
			if (index == buffer.Length)
			{
				buffer = new char[buffer.Length * 2];
				tagReaderBuffer.CopyTo(buffer, 0);
				tagReaderBuffer = buffer;
			}

			buffer[index++] = c;
			return index;
		}

		internal string GetNextTagString(bool addTagStart = false)
		{
			var index = 0;
			if (addTagStart)
			{
				index = AppendToTagReaderBuffer('<', index);
			}

			while (true)
			{
				if (!TryRead(out char last))
				{
					break; // End of stream
				}
				else
				{
					index = AppendToTagReaderBuffer(last, index);

					if (last == '\r')
					{
						currentReaderLineNumber++;
					}
					else if (last == '>')
					{
						break;
					}
				}
			}

			return new string(tagReaderBuffer, 0, index);
		}

		SubStreamableStream ReadStuffUntilNextTagStart()
		{
			var peek = reader.Peek();
			const int tagStart = '<';
			if (peek == -1 || peek == tagStart)
			{
				//Try to read character data first
				var result = ReadCharacterData(this.inputStream.GetSubStream(reader.StreamPosition, this.inputStream.Length - reader.StreamPosition));
				if (result != null)
				{
					for (var i = 0; i < result.Length + "<![CDATA[]]>".Length; i++)
					{
						this.reader.Read();
					}
					return result;
				}
				return this.inputStream.GetSubStream(0, 0);
			}
			else
			{
				var startPos = reader.StreamPosition;

				while (IsNotTheEndOfNextChunkFromStream('<', reader.Peek()))
				{
					var hasChar = TryRead(out char c);
					if (hasChar && c == '\r')
					{
						currentReaderLineNumber++;
					}
				}

				return this.inputStream.GetSubStream(startPos, reader.StreamPosition - startPos);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool IsNotTheEndOfNextChunkFromStream(char tag, int next)
		{
			return next != tag && next != -1;
		}

		internal bool EndOfStream
		{
			get { return reader.EndOfStream; }
		}

		internal int CurrentLineNumber
		{
			get { return currentReaderLineNumber + 1; }
		}

		internal IXmlDomObject LastElement => lastElement;

		#endregion

		internal bool QueueForCodeMappingWhereApplicable(object targetDataObject, PropertyInfo propertyInfo, string input, DataFieldValidatorAndSetter setter, bool isAttribute)
		{
			return codeMapper?.QueueForCodeMappingWhereApplicable(this, targetDataObject, propertyInfo, GetName(ElementPath.FullName), input, setter, CurrentLineNumber, isAttribute) ?? false;
		}

		#region Error Logging

		internal void AbortWithNoClosingTagFoundException(int startingLineNumber, string elementName = "")
		{
			var parentPath = string.IsNullOrEmpty(elementName) ? CurrentElementParser.GetFullName() : "<" + elementName + ">";
			throw new XmlProcessingException("Line " + CurrentLineNumber + ": Reached end of file without finding closing tag for element " + parentPath + " opened on line " + startingLineNumber + ".");
		}

		internal void AbortWithUnexpectedClosingTagFoundException(int currentLineNumber, int startingLineNumber, string elementName, string closingTagName)
		{
			throw new XmlProcessingException("Line " + currentLineNumber + ": " + CurrentElementParser.GetFullName() + " - Unexpected closing tag </" + closingTagName + "> found. Expected closing tag </" + elementName + "> for opening tag on line " + startingLineNumber + ".");
		}

		internal void AbortWithUnexpectedClosingCharacterDataFoundException(int currentLineNumber)
		{
			throw new XmlProcessingException("Line " + currentLineNumber + ": " + CurrentElementParser.GetFullName() + " - Unexpected closing CDATA tag ]] found within CDATA.");
		}

		internal void AddWarning(int? lineNumber, ElementPath nameTypeToPrepend, string errorMessage)
		{
			AddWarning(lineNumber, GetName(nameTypeToPrepend), errorMessage);
		}

		internal void AddWarning(int? lineNumber, string elementFullName, string errorMessage)
		{
			logger.Log(LogType.Warning, Res.GetString("f47d4bc3-2d20-40af-93e5-f1ba4a2ae5d3", "Line {0}: {1}{2}", lineNumber ?? currentReaderLineNumber, elementFullName, errorMessage));
		}

		internal void AbortWithException(int? lineNumber, ElementPath nameTypeToPrepend, string errorMessage)
		{
			throw new XmlProcessingException("Line " + (lineNumber ?? CurrentLineNumber) + ": " + GetName(nameTypeToPrepend) + errorMessage);
		}

		void AbortWithInvalidXmlCharacterFoundException(char invalidChar)
		{
			var hexValue = Convert.ToUInt32(invalidChar).ToString("X2");
			throw new XmlProcessingException("Line " + CurrentLineNumber + ": Hexadecimal value 0x" + hexValue + " is an invalid XML character.");
		}

		string GetName(ElementPath nameType)
		{
			switch (nameType)
			{
				case ElementPath.None:
					return "";
				case ElementPath.FullName:
					return CurrentElementParser.GetFullName();
				case ElementPath.FullNameFromParent:
					return CurrentElementParser.ParentElement.GetFullName();
			}

			throw new InvalidOperationException("Unknown ElementPath type: " + nameType.ToString());
		}

		internal void AddComment(int commentLineNumber, SubStreamableStream commentMessage)
		{
			string comment;
			using (var streamReader = new StreamReader(commentMessage))
			{
				comment = Res.GetString("5c2b4ba9-73a3-4cd1-8ed4-4f6fc0ef0829", "Comment found at Line {0} :- {1}", commentLineNumber, streamReader.ReadToEnd());
			}

			this.logger.Log(LogType.Information, comment);
		}

		internal ISimpleLogger Logger
		{
			get { return logger; }
		}

		#endregion

		#region Caching of Reflected Attributes

		readonly Dictionary<string, IEnumerable<string>> mandatoryElementNameCache = new Dictionary<string, IEnumerable<string>>();
		internal IEnumerable<string> GetMandatoryElementNamesCached(Type typeForFields)
		{
			IEnumerable<string> result;

			if (!mandatoryElementNameCache.TryGetValue(typeForFields.Name, out result))
			{
				mandatoryElementNameCache[typeForFields.Name] = result = GetMandatoryElementNames(typeForFields);
			}

			return result;
		}

		static List<string> GetMandatoryElementNames(Type typeForFields)
		{
			return typeForFields.GetProperties()
								.Where(property => Attribute.IsDefined(property, typeof(MandatoryAttribute), false))
								.Select(property => property.Name)
								.ToList();
		}

		readonly Dictionary<Type, IEnumerable<PropertyInfo>> candidateKeyPropertyInfosCache = new Dictionary<Type, IEnumerable<PropertyInfo>>();
		internal IEnumerable<PropertyInfo> GetCandidateKeyPropertyInfosCached(Type type)
		{
			IEnumerable<PropertyInfo> result;

			if (!candidateKeyPropertyInfosCache.TryGetValue(type, out result))
			{
				candidateKeyPropertyInfosCache[type] = result = GetCandidateKeyPropertyInfos(type);
			}

			return result;
		}

		static IEnumerable<PropertyInfo> GetCandidateKeyPropertyInfos(Type type)
		{
			return type.GetProperties().Where(property => property.GetCustomAttributes(typeof(CandidateKeyAttribute), false).Length > 0).ToList();
		}

		#endregion
	}

	internal enum ElementPath { None, FullName, FullNameFromParent }
}
