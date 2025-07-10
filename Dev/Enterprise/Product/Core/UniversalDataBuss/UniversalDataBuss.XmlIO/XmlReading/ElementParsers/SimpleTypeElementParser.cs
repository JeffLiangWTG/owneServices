using System;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	sealed class SimpleTypeElementParser : Element, IElementParser
	{
		internal SimpleTypeElementParser(XmlReader reader, PropertyInfo propertyInfo, IElementParser parentParser)
			: base(propertyInfo, reader)
		{
			this.reader = reader;
			this.parentParser = parentParser;
		}

		readonly XmlReader reader;
		readonly IElementNavigator parentParser;

		#region IElementParser Implementation

		IElementNavigator IElementNavigator.ParentElement
		{
			get { return parentParser; }
		}

		string IElementNavigator.CurrentElementName
		{
			get { return ElementName; }
		}

		bool IElementParser.ShouldWarnAboutUnrecognizedAttribute => false;

		#endregion

		public void ParseOutContent(ICurrentElement currentLineText, IDataObject targetDataObject)
		{
			Argument.NotNull(targetDataObject, "IDataObject targetDataObject");

			reader.CurrentElementParser = this;
			var startLine = currentLineText.OpenLineNumber;
			object content;
			Type targetContentType = PropertyInfo.PropertyType.IsGenericType ? PropertyInfo.PropertyType.GetGenericArguments()[0] : PropertyInfo.PropertyType;

			if (typeof(SubStreamableStream) == targetContentType)
			{
				content = null;
			}
			else
			{
				content = string.Empty;
			}

			if (!XmlRegexes.SelfEndingElement.IsMatch(currentLineText.Name))
			{
				var closeTag = reader.GetNextTagString();
				if (reader.EndOfStream)
				{
					reader.AbortWithNoClosingTagFoundException(startLine);
				}

				var openTagMatch = XmlRegexes.ValidOpeningTag.Match(currentLineText.Name);
				var closeTagMatch = XmlRegexes.ValidClosingTag.Match(closeTag);
				if (!openTagMatch.Success)
				{
					reader.AbortWithException(null, ElementPath.FullName, "Should not call parser with an invalid open tag!");
				}
				else if (!closeTagMatch.Success || closeTagMatch.Groups["elementName"].Value != openTagMatch.Groups["elementName"].Value)
				{
					ReportError(currentLineText.Name + currentLineText.Body + closeTag, startLine, reader.CurrentLineNumber);
				}

				if (typeof(SubStreamableStream) == targetContentType)
				{
					content = currentLineText.Body;
				}
				else
				{
					using (var streamReader = new StreamReader(currentLineText.Body))
					{
						content = streamReader.ReadToEnd();
					}
				}
			}

			ParseToTargetType(targetContentType, PropertyInfo, content, targetDataObject, startLine);
		}

		void ReportError(string currentLineText, int openTagLine, int closeTagLine)
		{
			var unexpectedClosingTagMatch = XmlRegexes.FullClosingTag.Match(currentLineText);
			if (unexpectedClosingTagMatch.Success)
			{
				reader.AbortWithUnexpectedClosingTagFoundException(reader.CurrentLineNumber, openTagLine, ElementName, unexpectedClosingTagMatch.Groups["tag"].Value);
			}
			else
			{
				var unexpectedOpeningTagMatch = XmlRegexes.FullOpeningTag.Match(currentLineText).NextMatch();
				if (unexpectedOpeningTagMatch.Success)
				{
					reader.AbortWithException(closeTagLine, ElementPath.FullName, " - Unexpected opening tag <" + unexpectedOpeningTagMatch.Groups["tag"].Value + "> found. Expected closing tag </" + ElementName + "> for opening tag on line " + openTagLine + ".");
				}
			}

			reader.AbortWithException(null, ElementPath.FullName, "Malformed Xml found when parsing simpleType element");
		}

		object ParseToTargetType(Type targetType, PropertyInfo propertyInfo, object rawValue, object targetDataObject, int lineNumber)
		{
			foreach (var parser in reader.ValueParsers)
			{
				if (parser.HandlesType(targetType))
				{
					return parser.Process(propertyInfo, targetDataObject, targetType, rawValue, lineNumber);
				}
			}

			throw new InvalidOperationException("Unhandled property type: " + targetType.FullName);
		}

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}
	}
}
