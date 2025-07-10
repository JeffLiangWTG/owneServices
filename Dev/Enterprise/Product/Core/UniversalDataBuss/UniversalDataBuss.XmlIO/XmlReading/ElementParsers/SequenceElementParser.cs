using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XsdGeneration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	sealed class SequenceElementParser : ComplexElement, IElementParser
	{
		internal SequenceElementParser(XmlReader reader, PropertyInfo propertyInfo, Type typeContainedWithinCollection, IElementParser parentParser, IDataObjectFactory factory )
			: base(propertyInfo, reader)
		{
			this.reader = Argument.NotNull(reader, "XmlReader reader");
			this.typeContainedWithinCollection = Argument.NotNull(typeContainedWithinCollection, "Type typeContainedWithinCollection");
			this.containedElementName = ChildCollectionElementConverter.GetContainedElementName(propertyInfo, ElementName);
			this.parentParser = parentParser;
			this.factory = factory;
		}

		readonly XmlReader reader;
		readonly Type typeContainedWithinCollection;
		readonly string containedElementName;
		readonly IElementParser parentParser;
		readonly IDataObjectFactory factory;

		#region IElementParser Implementation

		IElementNavigator IElementNavigator.ParentElement
		{
			get { return parentParser; }
		}

		string IElementNavigator.CurrentElementName
		{
			get { return ElementName; }
		}

		bool IElementParser.ShouldWarnAboutUnrecognizedAttribute => true;

		#endregion

		public void ParseOutContent(ICurrentElement surroundingElement, IDataObject parentDataObject)
		{
			Argument.NotNull(parentDataObject, "IDataObject parentDataObject");
			var targetCollectionType = PropertyInfo.PropertyType;
			var targetCollection = (IList)Activator.CreateInstance(targetCollectionType);

			var duplicateChecker = new DuplicateChecker(this);

			var selfEndingMatch = XmlRegexes.SelfEndingElement.Match(surroundingElement.Name);

			reader.CurrentElementParser = this;

			this.ReadAttributeValues<CollectionAttributesAttribute>(targetCollection, surroundingElement, selfEndingMatch, reader);

			if (selfEndingMatch.Success)
			{
				PropertyInfo.SetValue(parentDataObject, targetCollection, null);
				return;
			}

			while (!reader.EndOfStream)
			{
				using (var nextTag = reader.GetNextTag())
				{
					if (nextTag is ICurrentElement currentElement)
					{
						reader.CurrentElementParser = this;

						selfEndingMatch = XmlRegexes.SelfEndingElement.Match(currentElement.Name);
						var openingTagMatch = XmlRegexes.ValidOpeningTag.Match(currentElement.Name);

						if (openingTagMatch.Success || selfEndingMatch.Success)
						{
							var elementNameFromOpeningTag = openingTagMatch.Success ? openingTagMatch.Groups["elementName"].Value : selfEndingMatch.Groups["elementName"].Value;
							if (elementNameFromOpeningTag != containedElementName)
							{
								var parentPath = reader.CurrentElementParser.GetFullName();
								reader.CurrentElementParser = null;
								var parser = new UnknownElementParser(reader, elementNameFromOpeningTag, Res.GetString("b50727b3-01b5-4046-8816-b94007baa961", "Element {0} should only contain <{1}> elements.", parentPath, containedElementName), this);
								parser.ParseOutContent(currentElement);
							}
							else
							{
								var newElementStartLine = currentElement.OpenLineNumber;
								var newElement = factory.Create(typeContainedWithinCollection);

								var parser = (IValidatedElementParser)GetNewObjectElement(PropertyInfo, typeContainedWithinCollection);
								parser.ParseOutContent(currentElement, newElement);

								if (parser.AllMandatoryElementsProvided && !duplicateChecker.IsDuplicate(newElement, reader.LastElement.OpenLineNumber, newElementStartLine))
								{
									targetCollection.Add(newElement);
								}
							}
						}
						else
						{
							var closingTagMatch = XmlRegexes.ValidClosingTag.Match(currentElement.Name);
							var closingTagName = closingTagMatch.Groups["elementName"].Value;
							if (closingTagMatch.Success)
							{
								if (closingTagName == ElementName)
								{
									if (PropertyInfo.GetCustomAttributes(typeof(MandatoryAttribute), false).Length > 0 && targetCollection.Count == 0)
									{
										var parentPath = reader.CurrentElementParser.GetFullName();
										reader.AddWarning(currentElement.OpenLineNumber, ElementPath.None, Res.GetString("d99edf6e-75c0-424f-a3b9-d70e7c3752ce", "Mandatory Collection {0} opened at line {1} was excluded as it does not contain at least one valid element.", parentPath, surroundingElement.OpenLineNumber));
									}
									else
									{
										PropertyInfo.SetValue(parentDataObject, targetCollection, null);
									}

									return;
								}
								else
								{
									closingTagName = XmlRegexes.FullClosingTag.Match(currentElement.Name).Groups["tag"].Value;
									reader.AbortWithUnexpectedClosingTagFoundException(currentElement.OpenLineNumber, surroundingElement.OpenLineNumber, ElementName, closingTagName);
								}
							}
						}
					}
				}
			}

			reader.CurrentElementParser = this;
			reader.AbortWithNoClosingTagFoundException(surroundingElement.OpenLineNumber);
		}

		class DuplicateChecker
		{
			internal DuplicateChecker(SequenceElementParser parser)
			{
				this.parser = parser;
			}

			readonly SequenceElementParser parser;
			readonly HashSet<string> candidateKeys = new HashSet<string>();

			internal bool IsDuplicate(object newElement, int wrappingElementStartLine, int newElementStartLine)
			{
				var reader = parser.reader;
				var candidateKeyBuilder = new CandidateKeyBuilder(newElement, reader.GetCandidateKeyPropertyInfosCached(parser.typeContainedWithinCollection));
				var candidateKey = candidateKeyBuilder.Key;

				if (!string.IsNullOrEmpty(candidateKey) && candidateKeys.Contains(candidateKey))
				{
					reader.AddWarning(wrappingElementStartLine, ElementPath.FullNameFromParent, " - " + Res.GetString("32b5f108-9ab4-45a0-9d25-1c2700fc07f0", "Duplicate Candidate Key found, element <{0}> opened at line {1} was not imported. Candidate Key: [{2}]", parser.containedElementName, newElementStartLine, candidateKeyBuilder.HumanReadableKey));
					return true;
				}

				candidateKeys.Add(candidateKey);
				return false;
			}
		}

		class CandidateKeyBuilder
		{
			internal CandidateKeyBuilder(object element, IEnumerable<PropertyInfo> candidateKeyPropertyInfos)
			{
				this.values = new List<KeyValuePair<string, string>>();

				foreach (var propertyInfo in candidateKeyPropertyInfos)
				{
					var valueFromProperty = propertyInfo.GetValue(element, null);
					var propertyValueType = propertyInfo.PropertyType;
					if (propertyValueType.IsGenericType)
					{
						propertyValueType = propertyValueType.GetGenericArguments()[0];
						values.Add(new KeyValuePair<string, string>(propertyInfo.Name, GetFormattedValueAsString(valueFromProperty, propertyValueType)));
					}
					else
					{
						if (valueFromProperty != null)
						{
							var relatedCodeDataObject = valueFromProperty as ICodeDataObject;
							if (relatedCodeDataObject != null)
							{
								values.Add(new KeyValuePair<string, string>(propertyInfo.Name, relatedCodeDataObject.Code));
							}
							else
							{
								var locationDataObject = valueFromProperty as UNLOCO;
								if (locationDataObject != null)
								{
									values.Add(new KeyValuePair<string, string>(propertyInfo.Name, locationDataObject.Code));
								}
								else
								{
									throw new InvalidOperationException("Unhandled Candidate Key Type: " + propertyValueType.FullName);
								}
							}
						}
					}
				}
			}

			readonly List<KeyValuePair<string, string>> values;

			internal string HumanReadableKey
			{
				get { return string.Join(", ", values.ConvertAll(element => string.Format("{0}='{1}'", element.Key, element.Value)).ToArray()); }
			}

			internal string Key
			{
				get { return string.Join("|", values.ConvertAll(element => element.Value.Replace("^", "^^").Replace("|", "^|")).ToArray()); }
			}

			static string GetFormattedValueAsString(object valueFromProperty, Type propertyValueType)
			{
				if (valueFromProperty == null)
				{
					return Activator.CreateInstance(propertyValueType).ToString();
				}
				else if (propertyValueType == typeof(ZDecimal))
				{
					string valueAsString = ((ZDecimal)valueFromProperty).ToStringTrimZeros();

					return valueAsString.Contains(".") ? valueAsString : valueAsString + ".0";
				}
				else
				{
					return valueFromProperty.ToString();
				}
			}
		}

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.Collections; }
		}

		protected override Element GetNewComplexElement(PropertyInfo propertyInfo, Type calculatedPropertyType)
		{
			return new ComplexTypeElementParser(reader, containedElementName, calculatedPropertyType, this, factory);
		}

		protected override Element GetNewFlattenedElement(PropertyInfo propertyInfo, Type calculatedPropertyType, string baseElementPropertyName)
		{
			return new SimpleTypeWithAttributesParser(reader, propertyInfo, this, baseElementPropertyName, factory);
		}
	}
}
