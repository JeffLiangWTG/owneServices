using System;

namespace NUnit.Framework
{
	public interface IXmlAssertion : IHasNameAssertions<IXmlAssertion>, IHasValueAssertions<IXmlAssertion>
	{
		IXmlAssertion WithAttribute(string name, string value, string failedMessage = null);

		IXmlAssertion WithAttribute(Action<IXmlAttributeAssertion> predicate, string failedMessage = null);

		IXmlAssertion HavingAtLeastOneChildNode(Action<IXmlAssertion> predicate = null, string failedMessage = null);

		IXmlAssertion HavingAtLeastOneChildNode(string path, Action<IXmlAssertion> predicate = null, string failedMessage = null);

		IXmlAssertion HavingExactlyOneChildNode(Action<IXmlAssertion> predicate = null, string failedMessage = null);

		IXmlAssertion HavingExactlyOneChildNode(string path, Action<IXmlAssertion> predicate = null, string failedMessage = null);

		IXmlAssertion HavingAllChildNodes(Action<IXmlAssertion> predicate, string failedMessage = null);

		IXmlAssertion HavingAtLeastOneDescendantNode(Action<IXmlAssertion> predicate = null, string failedMessage = null);

		IXmlAssertion HavingExactlyOneDescendantNode(Action<IXmlAssertion> predicate = null, string failedMessage = null);
	}
}
