using System;
using System.Xml.Linq;

namespace NUnit.Framework
{
	public class XmlAttributeAssertion : XmlAssertion<XAttribute>, IXmlAttributeAssertion
	{
		public XmlAttributeAssertion(XAttribute attribute, bool withDelayedChecks = false)
			: base(attribute, withDelayedChecks, null)
		{
		}

		public IXmlAttributeAssertion WithName(string name, string failedMessage = null)
		{
			return WithXNameCore(xname => xname.LocalName == name, () => XmlAttributeValidationError.NameExactMatchFailed(Object, name, failedMessage));
		}
		public IXmlAttributeAssertion WithName(Func<string, bool> predicate, string failedMessage = null)
		{
			return WithXNameCore(xname => predicate(xname.LocalName), () => XmlAttributeValidationError.NameLambdaMatchFailed(Object, failedMessage));
		}

		public IXmlAttributeAssertion WithXName(Func<XName, bool> predicate, string failedMessage = null)
		{
			return WithXNameCore(predicate, failedMessage: failedMessage);
		}

		IXmlAttributeAssertion WithXNameCore(Func<XName, bool> predicate, Func<XmlAttributeValidationError> error = null, string failedMessage = null)
		{
			QueueOrDo(() =>
			{
				if (predicate(Object.Name))
				{
					return (true, null);
				}

				return (false, error?.Invoke() ?? XmlAttributeValidationError.XNameLambdaMatchFailed(Object, failedMessage));
			});

			return this;
		}

		public IXmlAttributeAssertion WithNamespace(string @namespace, string failedMessage = null)
		{
			return WithXNameCore(xname => xname.NamespaceName == @namespace, () => XmlAttributeValidationError.NamespaceExactMatchFailed(Object, @namespace, failedMessage));
		}

		public IXmlAttributeAssertion WithNamespace(Func<string, bool> predicate, string failedMessage = null)
		{
			return WithXNameCore(xname => predicate(xname.NamespaceName), () => XmlAttributeValidationError.NamespaceLambdaMatchFailed(Object, failedMessage));
		}

		public IXmlAttributeAssertion WithNamespaceAndName(string @namespace, string name, string failedMessage = null)
		{
			return WithXNameCore(xname => xname.NamespaceName == @namespace && xname.LocalName == name, () => XmlAttributeValidationError.NamespaceAndNameExactMatchFailed(Object, @namespace, name, failedMessage));
		}

		public IXmlAttributeAssertion WithValue(string value, string failedMessage = null)
		{
			return WithValueCore(v => value == v, () => XmlAttributeValidationError.ValueExactMatchFailed(Object, value, failedMessage));
		}

		public IXmlAttributeAssertion WithValue(Func<string, bool> predicate, string failedMessage = null)
		{
			return WithValueCore(predicate, failedMessage: failedMessage);
		}

		IXmlAttributeAssertion WithValueCore(Func<string, bool> predicate, Func<XmlAttributeValidationError> error = null, string failedMessage = null)
		{
			QueueOrDo(() => {
				if (predicate(Object.Value))
				{
					return (true, null);
				}

				return (false, error?.Invoke() ?? XmlAttributeValidationError.ValueLambdaMatchFailed(Object, failedMessage));
			});

			return this;
		}
	}
}
