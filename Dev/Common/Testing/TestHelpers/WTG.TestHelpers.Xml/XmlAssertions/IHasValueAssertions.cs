using System;

namespace NUnit.Framework
{
	public interface IHasValueAssertions<out TOwner>
	{
		TOwner WithValue(string value, string failedMessage = null);

		TOwner WithValue(Func<string, bool> predicate, string failedMessage = null);
	}
}
