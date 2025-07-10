using System;
using System.Xml.Linq;

namespace NUnit.Framework
{
	public interface IHasNameAssertions<out TOwner>
	{
		TOwner WithName(string name, string failedMessage = null);

		TOwner WithName(Func<string, bool> predicate, string failedMessage = null);

		TOwner WithXName(Func<XName, bool> predicate, string failedMessage = null);

		TOwner WithNamespace(string @namespace, string failedMessage = null);

		TOwner WithNamespace(Func<string, bool> predicate, string failedMessage = null);

		TOwner WithNamespaceAndName(string @namespace, string name, string failedMessage = null);
	}
}
