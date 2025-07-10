using System;
using System.Text.RegularExpressions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	public static class IElementParserExtensions
	{
		public static string GetFullName(this IElementNavigator parser)
		{
			if (parser != null)
			{
				var parent = parser.ParentElement;
				if (parent != null)
				{
					return string.Format("{0}.<{1}>", parent.GetFullName(), parser.CurrentElementName);
				}

				return string.Format("<{0}>", parser.CurrentElementName);
			}

			return string.Empty;
		}

		public static void ReadAttributeValues<T>(this IElementParser parser, object targetDataObject, ICurrentElement currentElement, Match selfEndingMatch, XmlReader reader)
			where T : Attribute, IAttributesAttribute
		{
			if (targetDataObject != null)
			{
				var targetDataObjectType = targetDataObject.GetType();
				var attributeInfo = targetDataObjectType.GetAttribute<T>();
				if (attributeInfo != null)
				{
					var openingTagMatch = XmlRegexes.ValidOpeningTag.Match(currentElement.Name);
					if (!openingTagMatch.Success)
					{
						openingTagMatch = selfEndingMatch;
						if (!openingTagMatch.Success)
						{
							reader.AbortWithException(null, ElementPath.FullName, " - " + Res.GetString("2fa1fb5b-c801-4c36-a980-93776c462e0e", "Opening tag contains unrecognized sequence [{0}].", currentElement.Name));
						}
					}

					var attributes = openingTagMatch.Groups["attributes"].Value.Trim();
					parser.ReadAttributeValues(targetDataObject, currentElement, targetDataObjectType, attributes, attributeInfo, reader);
				}
			}
		}

		public static void ReadAttributeValues(this IElementParser parser, object targetDataObject, ICurrentElement currentElement, Type targetDataObjectType, string attributes, IAttributesAttribute attributeInfo, XmlReader reader)
		{
			var attributeMatch = XmlRegexes.SingleAttribute.Match(attributes);
			if (attributeMatch.Success)
			{
				var valueSetter = new PropertyValueSetter(reader);
				do
				{
					var attributeName = attributeMatch.Groups["attributeName"].Value;
					var attributeValue = attributeMatch.Groups["attributeValue"].Value;

					var targetPropertyInfo = (attributeInfo?.HasAttributeDefined(attributeName) ?? true)
						? targetDataObjectType.GetProperty(attributeName)
						: null;

					if (targetPropertyInfo != null)
					{
						valueSetter.SetValueOnTarget(attributeValue, targetDataObject, targetPropertyInfo, true, currentElement.OpenLineNumber);
					}
					else if (parser.ShouldWarnAboutUnrecognizedAttribute)
					{
						reader.AddWarning(currentElement.OpenLineNumber, ElementPath.FullName, " - " + Res.GetString("e4fcaea2 -80f3-4067-aa3a-dc1d69e4d874", "Unrecognized attribute [{0}] found. Attribute value skipped.", attributeName));
					}
				}
				while ((attributeMatch = attributeMatch.NextMatch()) != null && attributeMatch.Success);
			}
		}
	}
}
