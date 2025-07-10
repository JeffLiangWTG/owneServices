using System;
using System.Xml.Linq;

namespace NUnit.Framework
{
	class XmlAttributeValidationError : XmlValidationError<XAttribute>
	{
		protected XmlAttributeValidationError(string error, string errorInformation, XAttribute actualObject = null)
			: base(error, errorInformation, actualObject)
		{
		}

		public static XmlAttributeValidationError NameExactMatchFailed(XAttribute attribute, string expectedName, string error = null)
		{
			return new XmlAttributeValidationError(error, $"Name '{attribute.Name.LocalName}' of attribute is not equal to '{expectedName}'.", attribute);
		}

		public static XmlAttributeValidationError NameLambdaMatchFailed(XAttribute attribute, string error = null)
		{
			return new XmlAttributeValidationError(error, $"Name '{attribute.Name.LocalName}' of attribute does not match the given criteria.", attribute);
		}

		public static XmlAttributeValidationError NamespaceExactMatchFailed(XAttribute attribute, string expectedNamespace, string error = null)
		{
			return new XmlAttributeValidationError(error, $"Namespace '{attribute.Name.NamespaceName}' of attribute is not equal to '{expectedNamespace}'.", attribute);
		}

		public static XmlAttributeValidationError NamespaceLambdaMatchFailed(XAttribute attribute, string error = null)
		{
			return new XmlAttributeValidationError(error, $"Namespace '{attribute.Name.NamespaceName}' of attribute does not match the given criteria.", attribute);
		}

		public static XmlAttributeValidationError NamespaceAndNameExactMatchFailed(XAttribute attribute, string expectedNamespace, string expectedName, string error = null)
		{
			return new XmlAttributeValidationError(error, $"Attribute does not have namespace '{expectedNamespace}' and name '{expectedName}'.", attribute);
		}

		public static XmlAttributeValidationError XNameLambdaMatchFailed(XAttribute attribute, string error = null)
		{
			return new XmlAttributeValidationError(error, $"XName '{attribute.Name}' of attribute does not match the given criteria.", attribute);
		}

		public static XmlAttributeValidationError ValueExactMatchFailed(XAttribute attribute, string expectedValue, string error = null)
		{
			return new XmlAttributeValidationError(error, $"Value '{attribute.Value}' of attribute is not equal to '{expectedValue}'.", attribute);
		}

		public static XmlAttributeValidationError ValueLambdaMatchFailed(XAttribute attribute, string error = null)
		{
			return new XmlAttributeValidationError(error, $"Value '{attribute.Value}' of attribute does not match the given criteria.", attribute);
		}

		protected override string GetActualValueString()
		{
			return $"{Environment.NewLine}Actual Value: {actualObject}";
		}
	}
}
