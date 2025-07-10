using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	sealed class ComplexTypeElementParser : ElementWithChildren, IValidatedElementParser
	{
		internal ComplexTypeElementParser(XmlReader reader, PropertyInfo propertyInfo, Type calculatedPropertyType, IElementParser parentParser, IDataObjectFactory factory)
			: base(propertyInfo, calculatedPropertyType, reader)
		{
			this.reader = reader;
			this.parentParser = parentParser;
			this.factory = factory;
			this.elementType = ElementType.Property;
		}

		internal ComplexTypeElementParser(XmlReader reader, string name, Type typeForFields, IElementParser parentParser, IDataObjectFactory factory, bool isRootElement = false)
			: base(name, typeForFields, reader)
		{
			this.reader = reader;
			this.parentParser = parentParser;
			this.factory = factory;
			this.elementType = isRootElement ? ElementType.Root : ElementType.List;
		}

		readonly XmlReader reader;
		readonly ElementType elementType;
		readonly IElementNavigator parentParser;
		readonly IDataObjectFactory factory;

		enum ElementType { Root, Property, List }

		#region IElementParser Implementation

		IElementNavigator IElementNavigator.ParentElement
		{
			get { return parentParser; }
		}

		string IElementNavigator.CurrentElementName
		{
			get { return ElementName; }
		}

		public bool AllMandatoryElementsProvided
		{
			get;
			private set;
		}

		bool IElementParser.ShouldWarnAboutUnrecognizedAttribute => false;

		#endregion

		public void ParseOutContent(ICurrentElement surroundingElement, IDataObject parentDataObject)
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

			var startLine = surroundingElement.OpenLineNumber;
			var mandatoryElementNames = new List<string>(reader.GetMandatoryElementNamesCached(TypeForFields));

			var selfEndingElement = XmlRegexes.SelfEndingElement.Match(surroundingElement.Name);
			this.ReadAttributeValues<DataObjectAttributesAttribute>(targetDataObject, surroundingElement, selfEndingElement, reader);

			if (selfEndingElement.Success)
			{
				reader.CurrentElementParser = this;
				CheckMandatoryElements(startLine, surroundingElement, mandatoryElementNames);
				return;
			}

			while (!reader.EndOfStream)
			{
				using (var nextTag = reader.GetNextTag())
				{
					if (nextTag is ICurrentElement nextElement)
					{
						reader.CurrentElementParser = this;

						var closingTagMatch = XmlRegexes.ValidClosingTag.Match(nextElement.Name);
						if (closingTagMatch.Success)
						{
							var closingTagName = closingTagMatch.Groups["elementName"].Value;
							if (closingTagName != ElementName)
							{
								reader.AbortWithUnexpectedClosingTagFoundException(nextElement.OpenLineNumber, startLine, ElementName, closingTagName);
							}
							else
							{
								CheckMandatoryElements(startLine, nextElement, mandatoryElementNames);

								if (AllMandatoryElementsProvided && elementType == ElementType.Property)
								{
									PropertyInfo.SetValue(parentDataObject, targetDataObject, null);
								}

								return;
							}
						}
						else
						{
							closingTagMatch = XmlRegexes.FullClosingTag.Match(nextElement.Name);
							if (closingTagMatch.Success)
							{
								reader.AbortWithUnexpectedClosingTagFoundException(nextElement.OpenLineNumber, startLine, ElementName, closingTagMatch.Groups["tag"].Value);
							}
						}

						var openingTagMatch = XmlRegexes.ValidOpeningTag.Match(nextElement.Name);
						var selfEndingMatch = XmlRegexes.SelfEndingElement.Match(nextElement.Name);
						if (openingTagMatch.Success || selfEndingMatch.Success)
						{
							var childElementName = openingTagMatch.Success ? openingTagMatch.Groups["elementName"].Value : selfEndingMatch.Groups["elementName"].Value;
							var propertyInfo = TypeForFields.GetProperty(childElementName);
							IElementParser elementParser;

							if (propertyInfo == null || (elementParser = GetChildElementHandler(propertyInfo) as IElementParser) == null)
							{
								var parser = new UnknownElementParser(reader, childElementName, null, this);
								parser.ParseOutContent(nextElement);
							}
							else
							{
								if (targetDataObject is IDataObjectParseSupporter parseSupporter && !parseSupporter.IsElementSupported(childElementName))
								{
									reader.AbortWithException(nextElement.OpenLineNumber, ElementPath.None, parseSupporter.GetErrorTextWhenNonSupportedElementsFound());
								}

								elementParser.ParseOutContent(nextElement, targetDataObject);

								if (mandatoryElementNames.Contains(childElementName) && propertyInfo.GetValue(targetDataObject, null) != null)
								{
									mandatoryElementNames.Remove(childElementName);
								}
							}
						}
						else
						{
							var openingTagWithInvalidContentMatch = XmlRegexes.FullOpeningTag.Match(nextElement.Name);
							if (openingTagWithInvalidContentMatch.Success)
							{
								reader.AbortWithException(nextElement.OpenLineNumber, ElementPath.FullName, " - Invalid opening tag <" + openingTagWithInvalidContentMatch.Groups["tag"].Value + "> found.");
							}
						}
					}
				}
			}

			reader.CurrentElementParser = this;
			reader.AbortWithNoClosingTagFoundException(startLine);
		}

		void CheckMandatoryElements(int startLine, ICurrentElement currentElement, List<string> missingMandatoryElementNames)
		{
			if (missingMandatoryElementNames.Count > 0)
			{
				var currentLine = currentElement.OpenLineNumber;
				var missingMandatoryElements = "Missing: " + string.Join(", ", missingMandatoryElementNames.ToArray()) + ".";

				if (elementType == ElementType.Root)
				{
					reader.AbortWithException(currentLine, ElementPath.None, "Top Level Element <" + ElementName + "> opened at line " + startLine + " cannot be imported as it is missing mandatory elements. " + missingMandatoryElements);
				}
				else
				{
					var parentPath = reader.CurrentElementParser.GetFullName();

					reader.AddWarning(currentLine, ElementPath.None, "Element " + parentPath + " opened at line " + startLine + " was excluded as it was missing mandatory elements. " + missingMandatoryElements);
				}
			}
			else
			{
				AllMandatoryElementsProvided = true;
			}
		}

		protected override Element GetNewFieldElement(PropertyInfo propertyInfo)
		{
			return new SimpleTypeElementParser(reader, propertyInfo, this);
		}

		protected override Element GetNewFlattenedElement(PropertyInfo propertyInfo, Type calculatedPropertyType, string baseElementPropertyName)
		{
			return new SimpleTypeWithAttributesParser(reader, propertyInfo, this, baseElementPropertyName, factory);
		}

		protected override Element GetNewComplexElement(PropertyInfo propertyInfo, Type calculatedPropertyType)
		{
			return new ComplexTypeElementParser(reader, propertyInfo, calculatedPropertyType, this, factory);
		}

		protected override Element GetNewListElement(PropertyInfo propertyInfo, Type typeOfContainedObjects)
		{
			return new SequenceElementParser(reader, propertyInfo, typeOfContainedObjects, this, factory);
		}

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}
	}
}
