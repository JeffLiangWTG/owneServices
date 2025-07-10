using System;
using System.IO;
using System.Security;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ZExceptionExtensionsTest : TestCase
	{
		public void TestGetFullMessage()
		{
			var outerAggrEx = new AggregateException(
				"Outer aggr ex.",
				new AggregateException("Inner aggr ex.", new FormatException("Error format.")),
				new IOException("Unauthorized file access.", new SecurityException("Not administrator.")));
			AssertEquals("Outer aggr ex. --> Inner aggr ex. --> Error format. --> Unauthorized file access. --> Not administrator.", outerAggrEx.GetFullMessage());
		}
	}
}
