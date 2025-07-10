using System;
using System.Xml.Linq;

namespace NUnit.Framework
{
	class XmlElementValidationError : XmlValidationError<XElement>
	{
		protected XmlElementValidationError(string error, string errorDetailedInformation, XElement actualObject = null)
			: base(error, errorDetailedInformation, actualObject)
		{
		}

		public static XmlElementValidationError ParseError(string actualValue)
		{
			return new XmlElementValidationError(null, $"Invalid XML string.{Environment.NewLine}Actual Value:{Environment.NewLine}{actualValue}");
		}

		public static XmlElementValidationError NameExactMatchFailed(XElement xml, string expectedName, string error = null)
		{
			return new XmlElementValidationError(error, $"Name '{xml.Name.LocalName}' of element <{xml.Name.LocalName}> is not equal to '{expectedName}'.", xml);
		}

		public static XmlElementValidationError NameLambdaMatchFailed(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Name '{xml.Name.LocalName}' of element <{xml.Name}> does not match the given criteria.", xml);
		}

		public static XmlElementValidationError NamespaceExactMatchFailed(XElement xml, string expectedNamespace, string error = null)
		{
			return new XmlElementValidationError(error, $"Namespace '{xml.Name.NamespaceName}' of element <{xml.Name}> is not equal to '{expectedNamespace}'.", xml);
		}

		public static XmlElementValidationError NamespaceLambdaMatchFailed(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Namespace '{xml.Name.NamespaceName}' of element <{xml.Name}> does not match the given criteria.", xml);
		}

		public static XmlElementValidationError NamespaceAndNameExactMatchFailed(XElement xml, string expectedNamespace, string expectedName, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> does not have namespace '{expectedNamespace}' and name '{expectedName}'.", xml);
		}

		public static XmlElementValidationError XNameLambdaMatchFailed(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"XName '{xml.Name}' of element <{xml.Name}> does not match the given criteria.", xml);
		}

		public static XmlElementValidationError ValueExactMatchFailed(XElement xml, string expectedValue, string error = null)
		{
			return new XmlElementValidationError(error, $"Value '{xml.Value}' of element <{xml.Name}> is not equal to '{expectedValue}'.", xml);
		}

		public static XmlElementValidationError ValueLambdaMatchFailed(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Value '{xml.Value}' of element <{xml.Name}> does not match the given criteria.", xml);
		}

		public static XmlElementValidationError ElementDoesNotHaveValue(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> is expected to have inner text but has nested elements instead.", xml);
		}

		public static XmlElementValidationError AttributeExactMatchFailed(XElement xml, string expectedAttributeName, string expectedAttributeValue, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> does not have an attribute '{expectedAttributeName}' with value '{expectedAttributeValue}'.", xml);
		}

		public static XmlElementValidationError AttributeCriteriaMatchFailed(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> does not have an attribute matching the given criteria.", xml);
		}

		public static XmlElementValidationError NoChildNodeFound(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> does not have any child element matching the given criteria.", xml);
		}

		public static XmlElementValidationError MoreThanOneChildNodeFound(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> has more than one child elements matching the given criteria.", xml);
		}

		public static XmlElementValidationError HavingAllChildNodesFailed(XElement xml, XElement failedChild, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> has child element <{failedChild.Name}> not matching the given criteria.{Environment.NewLine}Actual child Element that failed validation:{Environment.NewLine}{failedChild}", xml);
		}

		public static XmlElementValidationError NoDescendantNodeFound(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> does not have any descendant element matching the given criteria.", xml);
		}

		public static XmlElementValidationError MoreThanOneDescendantNodesFound(XElement xml, string error = null)
		{
			return new XmlElementValidationError(error, $"Element <{xml.Name}> has more than one descendant elements matching the given criteria.", xml);
		}
	}
}
