using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NUnit.Framework
{
	public class XmlElementAssertion : XmlAssertion<XElement>, IXmlAssertion
	{
		public XmlElementAssertion(string xml, bool withDelayedChecks = false, string message = null)
			: base(null, withDelayedChecks, message)
		{
			QueueOrDo(() => {
				try
				{
					this.Object = XElement.Parse(xml);
					return (true, null);
				}
				catch (Exception)
				{
					return (false, XmlElementValidationError.ParseError(xml));
				}
			});
		}

		public XmlElementAssertion(XElement xml, bool withDelayedChecks = false, string message = null)
			: base(xml, withDelayedChecks, message)
		{
		}

		public IXmlAssertion HavingAllChildNodes(Action<IXmlAssertion> predicate, string failedMessage = null)
		{
			QueueOrDo(() => {
				foreach (var element in Object.Elements())
				{
					var assertion = new XmlElementAssertion(element, withDelayedChecks: true);
					predicate(assertion);
					var result = assertion.Check();
					if (!result.IsSuccessful)
					{
						return (false, XmlElementValidationError.HavingAllChildNodesFailed(Object, element, failedMessage));
					}
				}

				return (true, null);
			});

			return this;
		}

		public IXmlAssertion HavingAtLeastOneChildNode(Action<IXmlAssertion> predicate = null, string failedMessage = null)
		{
			QueueOrDo(() => {
				foreach (var element in Object.Elements())
				{
					var assertion = new XmlElementAssertion(element, withDelayedChecks: true);
					predicate?.Invoke(assertion);
					var result = assertion.Check();
					if (result.IsSuccessful)
					{
						return (true, null);
					}
				}

				return (false, XmlElementValidationError.NoChildNodeFound(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion HavingAtLeastOneChildNode(string path, Action<IXmlAssertion> predicate = null, string failedMessage = null)
		{
			var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			QueueOrDo(() => {
				IEnumerable<XElement> matchedElements = new[] { Object };
				foreach (var part in parts)
				{
					matchedElements = matchedElements.Elements().Where(e => e.Name.LocalName == part);
				}
				foreach (var element in matchedElements)
				{
					var assertion = new XmlElementAssertion(element, withDelayedChecks: true);
					predicate?.Invoke(assertion);
					var result = assertion.Check();
					if (result.IsSuccessful)
					{
						return (true, null);
					}
				}

				return (false, XmlElementValidationError.NoChildNodeFound(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion HavingExactlyOneChildNode(Action<IXmlAssertion> predicate = null, string failedMessage = null)
		{
			QueueOrDo(() => {
				var alreadyFound = false;
				foreach (var element in Object.Elements())
				{
					var assertion = new XmlElementAssertion(element, withDelayedChecks: true);
					predicate?.Invoke(assertion);
					var result = assertion.Check();
					if (result.IsSuccessful)
					{
						if (alreadyFound)
						{
							return (false, XmlElementValidationError.MoreThanOneChildNodeFound(Object, failedMessage));
						}

						alreadyFound = true;
					}
				}

				if (alreadyFound)
				{
					return (true, null);
				}

				return (false, XmlElementValidationError.NoChildNodeFound(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion HavingExactlyOneChildNode(string path, Action<IXmlAssertion> predicate = null, string failedMessage = null)
		{
			var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			QueueOrDo(() => {
				IEnumerable<XElement> matchedElements = new[] { Object };
				foreach (var part in parts)
				{
					matchedElements = matchedElements.Elements().Where(e => e.Name.LocalName == part);
				}
				var alreadyFound = false;
				foreach (var element in matchedElements)
				{
					var assertion = new XmlElementAssertion(element, withDelayedChecks: true);
					predicate?.Invoke(assertion);
					var result = assertion.Check();
					if (result.IsSuccessful)
					{
						if (alreadyFound)
						{
							return ((bool IsSuccessful, IXmlValidationError ValidationError))(false, XmlElementValidationError.MoreThanOneChildNodeFound(Object, failedMessage));
						}

						alreadyFound = true;
					}
				}

				if (alreadyFound)
				{
					return ((bool IsSuccessful, IXmlValidationError ValidationError))(true, null);
				}

				return ((bool IsSuccessful, IXmlValidationError ValidationError))(false, XmlElementValidationError.NoChildNodeFound(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion HavingAtLeastOneDescendantNode(Action<IXmlAssertion> predicate = null, string failedMessage = null)
		{
			QueueOrDo(() => {
				foreach (var element in Object.DescendantNodes().OfType<XElement>())
				{
					var assertion = new XmlElementAssertion(element, withDelayedChecks: true);
					predicate?.Invoke(assertion);
					var result = assertion.Check();
					if (result.IsSuccessful)
					{
						return (true, null);
					}
				}

				return (false, XmlElementValidationError.NoDescendantNodeFound(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion HavingExactlyOneDescendantNode(Action<IXmlAssertion> predicate = null, string failedMessage = null)
		{
			QueueOrDo(() => {
				var alreadyFound = false;
				foreach (var element in Object.DescendantNodes().OfType<XElement>())
				{
					var assertion = new XmlElementAssertion(element, withDelayedChecks: true);
					predicate?.Invoke(assertion);
					var result = assertion.Check();
					if (result.IsSuccessful)
					{
						if (alreadyFound)
						{
							return (false, XmlElementValidationError.MoreThanOneDescendantNodesFound(Object, failedMessage));
						}

						alreadyFound = true;
					}
				}

				if (alreadyFound)
				{
					return (true, null);
				}

				return (false, XmlElementValidationError.NoDescendantNodeFound(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion WithAttribute(string name, string value, string failedMessage = null)
		{
			QueueOrDo(() => {
				if (!Object.Attributes().Any(a => a.Name.LocalName == name && a.Value == value))
				{
					return (false, XmlElementValidationError.AttributeExactMatchFailed(Object, name, value, failedMessage));
				}

				return (true, null);
			});

			return this;
		}

		public IXmlAssertion WithAttribute(Action<IXmlAttributeAssertion> predicate, string failedMessage = null)
		{
			QueueOrDo(() => {
				foreach (var attribute in Object.Attributes())
				{
					var assertion = new XmlAttributeAssertion(attribute, withDelayedChecks: true);
					predicate(assertion);
					var result = assertion.Check();
					if (result.IsSuccessful)
					{
						return (true, null);
					}
				}

				return (false, XmlElementValidationError.AttributeCriteriaMatchFailed(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion WithName(string name, string failedMessage = null)
		{
			return WithXNameCore(xname => xname.LocalName == name, () => XmlElementValidationError.NameExactMatchFailed(Object, name, failedMessage));
		}

		public IXmlAssertion WithName(Func<string, bool> predicate, string failedMessage = null)
		{
			return WithXNameCore(xname => predicate(xname.LocalName), () => XmlElementValidationError.NameLambdaMatchFailed(Object, failedMessage));
		}

		public IXmlAssertion WithXName(Func<XName, bool> predicate, string failedMessage = null)
		{
			return WithXNameCore(predicate, failedMessage: failedMessage);
		}

		IXmlAssertion WithXNameCore(Func<XName, bool> predicate, Func<XmlElementValidationError> error = null, string failedMessage = null)
		{
			QueueOrDo(() =>
			{
				if (predicate(Object.Name))
				{
					return (true, null);
				}

				return (false, error?.Invoke() ?? XmlElementValidationError.XNameLambdaMatchFailed(Object, failedMessage));
			});

			return this;
		}

		public IXmlAssertion WithNamespace(string @namespace, string failedMessage = null)
		{
			return WithXNameCore(xname => xname.NamespaceName == @namespace, () => XmlElementValidationError.NamespaceExactMatchFailed(Object, @namespace, failedMessage));
		}

		public IXmlAssertion WithNamespace(Func<string, bool> predicate, string failedMessage = null)
		{
			return WithXNameCore(xname => predicate(xname.NamespaceName), () => XmlElementValidationError.NamespaceLambdaMatchFailed(Object, failedMessage));
		}

		public IXmlAssertion WithNamespaceAndName(string @namespace, string name, string failedMessage = null)
		{
			return WithXNameCore(xname => xname.NamespaceName == @namespace && xname.LocalName == name, () => XmlElementValidationError.NamespaceAndNameExactMatchFailed(Object, @namespace, name, failedMessage));
		}

		public IXmlAssertion WithValue(string value, string failedMessage = null)
		{
			return WithValueCore(v => v == value, () => XmlElementValidationError.ValueExactMatchFailed(Object, value, failedMessage));
		}

		public IXmlAssertion WithValue(Func<string, bool> predicate, string failedMessage = null)
		{
			return WithValueCore(predicate, failedMessage: failedMessage);
		}

		IXmlAssertion WithValueCore(Func<string, bool> predicate, Func<XmlElementValidationError> error = null, string failedMessage = null)
		{
			QueueOrDo(() =>
			{
				if (Object.HasElements)
				{
					return (false, XmlElementValidationError.ElementDoesNotHaveValue(Object, failedMessage));
				}

				if (!predicate(Object.Value))
				{
					return (false, error?.Invoke() ?? XmlElementValidationError.ValueLambdaMatchFailed(Object, failedMessage));
				}

				return (true, null);
			});

			return this;
		}
	}
}
