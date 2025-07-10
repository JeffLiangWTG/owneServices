using System;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	sealed class SimpleTypeWithAttributesParser : Element, IValidatedElementParser
	{
		internal SimpleTypeWithAttributesParser(XmlReader reader, PropertyInfo propertyInfo, IElementParser parentParser, string baseElementPropertyName, IDataObjectFactory factory)
			: base(propertyInfo, reader)
		{
			this.reader = reader;
			this.parentParser = parentParser;
			this.baseElementPropertyName = baseElementPropertyName;
			this.factory = factory;
			this.elementType = PropertyInfo.PropertyType.IsGenericAndATypeUsedForCollections() ? ElementType.List : ElementType.Property;
		}

		readonly XmlReader reader;
		readonly IElementNavigator parentParser;
		readonly string baseElementPropertyName;
		readonly IDataObjectFactory factory;
		readonly ElementType elementType;

		enum ElementType { List, Property }

		#region IValidatedElementParser Implementation

		IElementNavigator IElementNavigator.ParentElement
		{
			get { return parentParser; }
		}

		string IElementNavigator.CurrentElementName
		{
			get { return ElementName; }
		}

		bool IValidatedElementParser.AllMandatoryElementsProvided
		{
			get { return true; }
		}

		bool IElementParser.ShouldWarnAboutUnrecognizedAttribute => true;

		#endregion

		/// <summary>
		/// At some point this should be pushed up to ComplexElement from ElementWithChildren and then this made 
		/// to subclass ComplexElement. This will then need to be tested to make sure we can use namespace 
		/// versioning to introduce new versions of DataObjects flattened into attributes.
		/// </summary>
		Type TypeForFields
		{
			get { return PropertyInfo.PropertyType; }
		}

		public void ParseOutContent(ICurrentElement currentLineText, IDataObject parentDataObject)
		{
			Argument.NotNull(parentDataObject, "IDataObject parentDataObject");

			IDataObject targetDataObject;
			if (elementType == ElementType.Property)
			{
				targetDataObject = factory.Create(TypeForFields);
			}
			else
			{
				targetDataObject = parentDataObject;
				parentDataObject = null;
			}

			reader.CurrentElementParser = this;
			var startLine = currentLineText.OpenLineNumber;
			var content = string.Empty;
			var attributes = string.Empty;

			if (!XmlRegexes.SelfEndingElement.IsMatch(currentLineText.Name))
			{
				var closeTag = reader.GetNextTagString();

				if (reader.EndOfStream)
				{
					reader.AbortWithNoClosingTagFoundException(startLine);
				}

				var match = XmlRegexes.ValidOpeningTag.Match(currentLineText.Name);
				if (!match.Success || !XmlRegexes.ValidClosingTag.IsMatch(closeTag))
				{
					ReportError(currentLineText.Name + currentLineText.Body + closeTag, startLine);
				}

				using (var streamReader = new StreamReader(currentLineText.Body))
				{
					content = streamReader.ReadToEnd();
				}

				attributes = match.Groups["attributes"].Value.Trim();
			}

			var targetDataObjectType = targetDataObject.GetType();

			var baseElementPropertyInfo = targetDataObjectType.GetProperty(baseElementPropertyName);

			var valueSetter = new PropertyValueSetter(reader);

			var convertedValue = valueSetter.SetValueOnTarget(content, targetDataObject, baseElementPropertyInfo, false, startLine);
			if (convertedValue != null)
			{
				if (!string.IsNullOrEmpty(attributes))
				{
					this.ReadAttributeValues(targetDataObject, currentLineText, targetDataObjectType, attributes, null, reader);
				}

				if (elementType == ElementType.Property)
				{
					PropertyInfo.SetValue(parentDataObject, targetDataObject, null);
				}
			}
		}

		void ReportError(string currentLineText, int startLine)
		{
			var unexpectedClosingTagMatch = XmlRegexes.FullClosingTag.Match(currentLineText);
			if (unexpectedClosingTagMatch.Success)
			{
				reader.AbortWithUnexpectedClosingTagFoundException(reader.CurrentLineNumber, startLine, ElementName, unexpectedClosingTagMatch.Groups["tag"].Value);
			}
			else
			{
				var unexpectedOpeningTagMatch = XmlRegexes.FullOpeningTag.Match(currentLineText).NextMatch();
				if (unexpectedOpeningTagMatch.Success)
				{
					reader.AbortWithException(startLine, ElementPath.FullName, " - Unexpected opening tag <" + unexpectedOpeningTagMatch.Groups["tag"].Value + "> found. Expected closing tag </" + ElementName + "> for opening tag on line " + startLine + ".");
				}
			}

			reader.AbortWithException(startLine, ElementPath.FullName, "Malformed Xml found when parsing simpleType element");
		}

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}
	}
}
